#region License
/*
Copyright © 2014-2026 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Platforms;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests.UITests
{
    /// <summary>
    /// Attaching the Ginger Java agent to running Java windows with a real JDK 8, GingerAgentStarter and handle.exe.
    /// Agent state is checked from outside Ginger: listening sockets by netstat, the loaded agent jar by handle.exe.
    /// The attach tests run a plain Swing application compiled here: JavaTestApp.jar bundles the agent classes,
    /// so the JVM does not keep GingerAgent.jar open after an attach the way it does for a real application.
    /// </summary>
    [Level3]
    [TestClass]
    public class ActLaunchJavaWSApplicationAttachTest
    {
        private const string WindowTitle = "Java Swing Test App";
        private const string WindowNotFoundError = "Failed to find the Java application with title";
        private const string AgentConsoleTitle = "Ginger Agent Console";

        private const string PlainAppClass = "AttachTestApp";
        private const string PlainAppSource =
            "import javax.swing.*;\n" +
            "public class " + PlainAppClass + " {\n" +
            "    public static void main(String[] args) throws Exception {\n" +
            "        SwingUtilities.invokeAndWait(new Runnable() { public void run() {\n" +
            "            JFrame frame = new JFrame(\"" + WindowTitle + "\");\n" +
            "            frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);\n" +
            "            frame.add(new JButton(\"Attach\"));\n" +
            "            frame.setSize(300, 150);\n" +
            "            frame.setVisible(true);\n" +
            "        }});\n" +
            "    }\n" +
            "}\n";

        private const string StderrFloodAppClass = "StderrFloodApp";
        private const string StderrFloodAppSource =
            "import javax.swing.*;\n" +
            "public class " + StderrFloodAppClass + " {\n" +
            "    public static void main(String[] args) throws Exception {\n" +
            "        for (int i = 0; i < 3000; i++) {\n" +
            "            System.err.println(\"stderr line \" + i + \" of the application log, long enough to fill the pipe buffer\");\n" +
            "        }\n" +
            "        SwingUtilities.invokeAndWait(new Runnable() { public void run() {\n" +
            "            JFrame frame = new JFrame(\"" + WindowTitle + "\");\n" +
            "            frame.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);\n" +
            "            frame.setSize(300, 150);\n" +
            "            frame.setVisible(true);\n" +
            "        }});\n" +
            "    }\n" +
            "}\n";

        private static string mJavaExe;
        private static string mTestAppJar;
        private static string mPlainAppFolder;
        private static string mAgentJar;
        private static string mHandleExe;

        private readonly List<int> mStartedProcessIds = [];
        private readonly List<JavaDriver> mStartedDrivers = [];

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            string javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            mJavaExe = string.IsNullOrEmpty(javaHome) ? null : Path.Combine(javaHome, "bin", "java.exe");
            mTestAppJar = TestResources.GetTestResourcesFile(Path.Combine("JavaTestApp", "JavaTestApp.jar"));
            string staticDrivers = Path.Combine(Path.GetDirectoryName(typeof(ActLaunchJavaWSApplication).Assembly.Location), "StaticDrivers");
            mAgentJar = Path.Combine(staticDrivers, "GingerAgent.jar");
            mHandleExe = Path.Combine(staticDrivers, "handle.exe");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            KillTestAppWindows();
            if (mPlainAppFolder != null && Directory.Exists(mPlainAppFolder))
            {
                try
                {
                    Directory.Delete(mPlainAppFolder, true);
                }
                catch (IOException)
                {
                    // a test application may still hold its classes
                }
            }
            mPlainAppFolder = null;
        }

        [TestInitialize]
        public void TestInit()
        {
            if (mJavaExe == null || !File.Exists(mJavaExe))
            {
                Assert.Inconclusive("JAVA_HOME has to point to a JDK 8 for the Java agent attach tests");
            }
            if (!JavaVersionOutput().Contains("\"1.8"))
            {
                Assert.Inconclusive("The agent jar check covers GingerAgent.jar, which is used for Java 8 only. JAVA_HOME: " + mJavaExe);
            }
            CompilePlainApp();
            KillTestAppWindows();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            foreach (JavaDriver driver in mStartedDrivers)
            {
                driver.CloseDriver();
            }
            mStartedDrivers.Clear();
            foreach (int processId in mStartedProcessIds)
            {
                KillProcess(processId);
            }
            mStartedProcessIds.Clear();
            KillTestAppWindows();
        }

        [TestMethod]
        [Timeout(180000)]
        public void LaunchAndAttach_AppLaunchedByGinger_AgentListens()
        {
            int port = GetFreePort();
            ActLaunchJavaWSApplication act = CreateAction(port, 60);
            act.LaunchJavaApplication = true;
            act.URL = mTestAppJar;

            act.Execute();

            int processId = int.Parse(act.ReturnValues.First(rv => rv.Param == "Process ID").Actual);
            mStartedProcessIds.Add(processId);
            Assert.IsTrue(string.IsNullOrEmpty(act.Error), act.Error);
            AssertListensOn(processId, port);
        }

        [TestMethod]
        [Timeout(180000)]
        public void LaunchAndAttach_ShowAgentChecked_AgentConsoleIsShown()
        {
            int port = GetFreePort();
            ActLaunchJavaWSApplication act = CreateAction(port, 60);
            act.LaunchJavaApplication = true;
            act.URL = mTestAppJar;
            act.ShowAgent = true;

            act.Execute();

            int processId = int.Parse(act.ReturnValues.First(rv => rv.Param == "Process ID").Actual);
            mStartedProcessIds.Add(processId);
            Assert.IsTrue(string.IsNullOrEmpty(act.Error), act.Error);
            AssertListensOn(processId, port);
            AssertWindowShown(processId, AgentConsoleTitle);
        }

        [TestMethod]
        [Timeout(180000)]
        public void Launch_AppWritingMuchToStderr_WindowAppears()
        {
            ActLaunchJavaWSApplication act = CreateAction(0, 30);
            act.LaunchJavaApplication = true;
            act.LaunchWithAgent = false;
            act.WaitForWindowWhenDoingLaunch = true;
            act.URL = CreateStderrFloodJar();

            act.Execute();

            if (int.TryParse(act.ReturnValues.FirstOrDefault(rv => rv.Param == "Process ID")?.Actual, out int processId) && processId > 0)
            {
                mStartedProcessIds.Add(processId);
            }
            Assert.IsTrue(string.IsNullOrEmpty(act.Error), "The application must not block on a full stderr pipe: " + act.Error);
        }

        [TestMethod]
        [Timeout(180000)]
        public void Attach_RunningAppWithoutAgent_AgentListens()
        {
            int processId = StartTestApp(withAgentJarOnClassPath: false);
            int port = GetFreePort();

            ActLaunchJavaWSApplication act = RunAttach(port, 60, out TimeSpan elapsed);

            Assert.IsTrue(string.IsNullOrEmpty(act.Error), act.Error);
            Assert.AreEqual(processId.ToString(), act.ReturnValues.First(rv => rv.Param == "Process ID").Actual);
            AssertListensOn(processId, port);
            Assert.IsTrue(elapsed < TimeSpan.FromSeconds(30), "A window without agent must be picked without the inactive agent grace time, took " + elapsed);
        }

        [TestMethod]
        [Timeout(180000)]
        public void Attach_AutoDetectPort_AgentListensOnReturnedPort()
        {
            int processId = StartTestApp(withAgentJarOnClassPath: false);
            ActLaunchJavaWSApplication act = CreateAction(0, 60);
            act.AddOrUpdateInputParamValue(ActLaunchJavaWSApplication.Fields.PortConfigParam, ActLaunchJavaWSApplication.ePortConfigType.AutoDetect.ToString());

            act.Execute();

            Assert.IsTrue(string.IsNullOrEmpty(act.Error), act.Error);
            Assert.AreEqual(processId.ToString(), act.ReturnValues.First(rv => rv.Param == "Process ID").Actual);
            AssertListensOn(processId, int.Parse(act.ReturnValues.First(rv => rv.Param == "Port").Actual));
        }

        [TestMethod]
        [Timeout(180000)]
        public void Attach_SecondFlowWithTwoInstances_PicksInstanceWithoutAgent()
        {
            int firstProcessId = StartTestApp(withAgentJarOnClassPath: false);
            int firstPort = GetFreePort();
            ActLaunchJavaWSApplication first = RunAttach(firstPort, 60, out _);
            Assert.IsTrue(string.IsNullOrEmpty(first.Error), first.Error);
            AssertListensOn(firstProcessId, firstPort);

            int secondProcessId = StartTestApp(withAgentJarOnClassPath: false);
            int secondPort = GetFreePort(firstPort);
            ActLaunchJavaWSApplication second = RunAttach(secondPort, 60, out _);

            Assert.IsTrue(string.IsNullOrEmpty(second.Error), second.Error);
            AssertListensOn(secondProcessId, secondPort);
            AssertListensOn(firstProcessId, firstPort);
            CollectionAssert.DoesNotContain(GetListeningProcessIds(secondPort), firstProcessId);
        }

        [TestMethod]
        [Timeout(240000)]
        public void Attach_ParallelFlowsWithTwoInstances_EachFlowGetsOwnInstance()
        {
            int firstProcessId = StartTestApp(withAgentJarOnClassPath: false);
            int secondProcessId = StartTestApp(withAgentJarOnClassPath: false);
            int firstPort = GetFreePort();
            int secondPort = GetFreePort(firstPort);

            Task<ActLaunchJavaWSApplication> firstFlow = Task.Run(() => RunAttach(firstPort, 60, out _));
            Task<ActLaunchJavaWSApplication> secondFlow = Task.Run(() => RunAttach(secondPort, 60, out _));
            Task.WaitAll(firstFlow, secondFlow);

            Assert.IsTrue(string.IsNullOrEmpty(firstFlow.Result.Error), firstFlow.Result.Error);
            Assert.IsTrue(string.IsNullOrEmpty(secondFlow.Result.Error), secondFlow.Result.Error);
            List<int> firstOwners = GetListeningProcessIds(firstPort);
            List<int> secondOwners = GetListeningProcessIds(secondPort);
            Assert.AreEqual(1, firstOwners.Count, "first port owners: " + string.Join(",", firstOwners));
            Assert.AreEqual(1, secondOwners.Count, "second port owners: " + string.Join(",", secondOwners));
            Assert.AreNotEqual(firstOwners[0], secondOwners[0], "Both parallel flows attached to the same window");
            CollectionAssert.AreEquivalent(new[] { firstProcessId, secondProcessId }, new[] { firstOwners[0], secondOwners[0] });
        }

        [TestMethod]
        [Timeout(240000)]
        public void Attach_ParallelFlowsToSingleWindow_OnlyOneAgentIsAttached()
        {
            int processId = StartTestApp(withAgentJarOnClassPath: false);
            int firstPort = GetFreePort();
            int secondPort = GetFreePort(firstPort);

            Task<ActLaunchJavaWSApplication> firstFlow = Task.Run(() => RunAttach(firstPort, 12, out _));
            Task<ActLaunchJavaWSApplication> secondFlow = Task.Run(() => RunAttach(secondPort, 12, out _));
            Task.WaitAll(firstFlow, secondFlow);

            ActLaunchJavaWSApplication[] passed = new[] { firstFlow.Result, secondFlow.Result }.Where(a => string.IsNullOrEmpty(a.Error)).ToArray();
            ActLaunchJavaWSApplication[] failed = new[] { firstFlow.Result, secondFlow.Result }.Where(a => !string.IsNullOrEmpty(a.Error)).ToArray();
            Assert.AreEqual(1, passed.Length, "Exactly one flow may attach to the single window. Errors: " + string.Join(" | ", failed.Select(a => a.Error)));
            StringAssert.Contains(failed[0].Error, WindowNotFoundError);
            int passedPort = int.Parse(passed[0].Port);
            int failedPort = int.Parse(failed[0].Port);
            AssertListensOn(processId, passedPort);
            Assert.AreEqual(0, GetListeningProcessIds(failedPort).Count, "The window got a second agent");
        }

        [TestMethod]
        [Timeout(180000)]
        public void Attach_JavaToolOptionsSetOnMachine_Passes()
        {
            int processId = StartTestApp(withAgentJarOnClassPath: false);
            int port = GetFreePort();
            string previousToolOptions = Environment.GetEnvironmentVariable("JAVA_TOOL_OPTIONS");
            string previousJavaOptions = Environment.GetEnvironmentVariable("_JAVA_OPTIONS");
            Environment.SetEnvironmentVariable("JAVA_TOOL_OPTIONS", "-Dginger.attach.test=true");
            Environment.SetEnvironmentVariable("_JAVA_OPTIONS", "-Dginger.attach.test2=true");
            try
            {
                ActLaunchJavaWSApplication act = RunAttach(port, 60, out _);
                AssertListensOn(processId, port);
                // the attach command reports its stderr only after the starter process exits
                Thread.Sleep(3000);

                Assert.IsTrue(string.IsNullOrEmpty(act.Error), act.Error);
                StringAssert.Contains(act.ExInfo, "Picked up JAVA_TOOL_OPTIONS");
                StringAssert.Contains(act.ExInfo, "Picked up _JAVA_OPTIONS");
            }
            finally
            {
                Environment.SetEnvironmentVariable("JAVA_TOOL_OPTIONS", previousToolOptions);
                Environment.SetEnvironmentVariable("_JAVA_OPTIONS", previousJavaOptions);
            }
        }

        [TestMethod]
        [Timeout(180000)]
        public void Attach_RetryWithSamePortWhileAgentListens_ReusesAgent()
        {
            int processId = StartTestApp(withAgentJarOnClassPath: false);
            int port = GetFreePort();
            ActLaunchJavaWSApplication first = RunAttach(port, 60, out _);
            Assert.IsTrue(string.IsNullOrEmpty(first.Error), first.Error);
            AssertListensOn(processId, port);

            ActLaunchJavaWSApplication retry = RunAttach(port, 30, out TimeSpan elapsed);

            Assert.IsTrue(string.IsNullOrEmpty(retry.Error), retry.Error);
            Assert.AreEqual(processId.ToString(), retry.ReturnValues.First(rv => rv.Param == "Process ID").Actual);
            Assert.AreEqual(port.ToString(), retry.ReturnValues.First(rv => rv.Param == "Port").Actual);
            AssertListensOn(processId, port);
            Assert.IsTrue(elapsed < TimeSpan.FromSeconds(20), "The agent on the requested port must be used without waiting for the window time out, took " + elapsed);
            StringAssert.Contains(retry.ExInfo, "no new agent console was opened");
        }

        [TestMethod]
        [Timeout(180000)]
        public void Attach_AgentJarLoadedWithoutListeningAgent_MaxWaitBelowGraceTime_FailsAsBefore()
        {
            int processId = StartTestApp(withAgentJarOnClassPath: true);
            AssertAgentJarLoadedWithoutListener(processId);
            int port = GetFreePort();

            ActLaunchJavaWSApplication act = RunAttach(port, 5, out _);

            StringAssert.Contains(act.Error, WindowNotFoundError);
            Assert.AreEqual(0, GetListeningProcessIds(port).Count);
        }

        [TestMethod]
        [Timeout(180000)]
        public void Attach_InactiveAgentWindowAndWindowWithoutAgent_PicksWindowWithoutAgent()
        {
            int staleProcessId = StartTestApp(withAgentJarOnClassPath: true);
            AssertAgentJarLoadedWithoutListener(staleProcessId);
            int freeProcessId = StartTestApp(withAgentJarOnClassPath: false);
            int port = GetFreePort();

            ActLaunchJavaWSApplication act = RunAttach(port, 60, out TimeSpan elapsed);

            Assert.IsTrue(string.IsNullOrEmpty(act.Error), act.Error);
            AssertListensOn(freeProcessId, port);
            CollectionAssert.DoesNotContain(GetListeningProcessIds(port), staleProcessId);
            Assert.IsTrue(elapsed < TimeSpan.FromSeconds(30), "A window without agent must win over an inactive agent window, took " + elapsed);
        }

        [TestMethod]
        [Timeout(240000)]
        public void Attach_ParallelFlowsToInactiveAgentWindow_OnlyOneAgentIsAttached()
        {
            int processId = StartTestApp(withAgentJarOnClassPath: true);
            AssertAgentJarLoadedWithoutListener(processId);
            int firstPort = GetFreePort();
            int secondPort = GetFreePort(firstPort);

            TimeSpan firstElapsed = TimeSpan.Zero;
            TimeSpan secondElapsed = TimeSpan.Zero;
            Task<ActLaunchJavaWSApplication> firstFlow = Task.Run(() => RunAttach(firstPort, 20, out firstElapsed));
            Task<ActLaunchJavaWSApplication> secondFlow = Task.Run(() => RunAttach(secondPort, 20, out secondElapsed));
            Task.WaitAll(firstFlow, secondFlow);

            ActLaunchJavaWSApplication[] passed = new[] { firstFlow.Result, secondFlow.Result }.Where(a => string.IsNullOrEmpty(a.Error)).ToArray();
            ActLaunchJavaWSApplication[] failed = new[] { firstFlow.Result, secondFlow.Result }.Where(a => !string.IsNullOrEmpty(a.Error)).ToArray();
            Assert.AreEqual(1, passed.Length, "Exactly one flow may re-attach to the window. Errors: " + string.Join(" | ", failed.Select(a => a.Error)));
            StringAssert.Contains(failed[0].Error, WindowNotFoundError);
            Assert.AreEqual(processId.ToString(), passed[0].ReturnValues.First(rv => rv.Param == "Process ID").Actual);
            AssertListensOn(processId, int.Parse(passed[0].Port));
            Assert.AreEqual(0, GetListeningProcessIds(int.Parse(failed[0].Port)).Count, "The window got a second agent");
            TimeSpan passedElapsed = passed[0] == firstFlow.Result ? firstElapsed : secondElapsed;
            Assert.IsTrue(passedElapsed >= TimeSpan.FromSeconds(10), "The inactive agent has to be observed for the grace time before attaching, took " + passedElapsed);
        }

        [TestMethod]
        [Timeout(180000)]
        public void Attach_AgentConnectedByThisRunner_ReturnsSameAgentDetails()
        {
            int processId = StartTestApp(withAgentJarOnClassPath: false);
            int agentPort = GetFreePort();
            ActLaunchJavaWSApplication first = RunAttach(agentPort, 60, out _);
            Assert.IsTrue(string.IsNullOrEmpty(first.Error), first.Error);
            AssertListensOn(processId, agentPort);
            JavaDriver driver = StartConnectedDriver(agentPort);

            int requestedPort = GetFreePort(agentPort);
            ActLaunchJavaWSApplication second = RunAttach(requestedPort, 30, out TimeSpan elapsed, CreateRunnerContext(driver));

            Assert.IsTrue(string.IsNullOrEmpty(second.Error), second.Error);
            Assert.AreEqual(processId.ToString(), second.ReturnValues.First(rv => rv.Param == "Process ID").Actual);
            Assert.AreEqual(agentPort.ToString(), second.ReturnValues.First(rv => rv.Param == "Port").Actual);
            Assert.AreEqual(0, GetListeningProcessIds(requestedPort).Count, "No second agent may be attached");
            Assert.IsTrue(driver.IsRunning(), "The connected driver must stay connected");
            Assert.IsTrue(elapsed < TimeSpan.FromSeconds(20), "The connected agent must be used without waiting for the window time out, took " + elapsed);
        }

        [TestMethod]
        [Timeout(180000)]
        public void Attach_AgentConnectedByOtherRunner_LeavesItAlone()
        {
            int processId = StartTestApp(withAgentJarOnClassPath: false);
            int agentPort = GetFreePort();
            ActLaunchJavaWSApplication first = RunAttach(agentPort, 60, out _);
            Assert.IsTrue(string.IsNullOrEmpty(first.Error), first.Error);
            JavaDriver otherRunnerDriver = StartConnectedDriver(agentPort);

            int requestedPort = GetFreePort(agentPort);
            ActLaunchJavaWSApplication second = RunAttach(requestedPort, 5, out _, CreateRunnerContext());

            StringAssert.Contains(second.Error, WindowNotFoundError);
            Assert.AreEqual(0, GetListeningProcessIds(requestedPort).Count, "No agent may be attached for the other runner");
            AssertListensOn(processId, agentPort);
            Assert.IsTrue(otherRunnerDriver.IsRunning(), "The other runner's driver must stay connected");
        }

        [TestMethod]
        [Timeout(180000)]
        public void Attach_DriverOfThisRunnerDisconnected_FailsAsBefore()
        {
            int processId = StartTestApp(withAgentJarOnClassPath: false);
            int agentPort = GetFreePort();
            ActLaunchJavaWSApplication first = RunAttach(agentPort, 60, out _);
            Assert.IsTrue(string.IsNullOrEmpty(first.Error), first.Error);
            JavaDriver driver = StartConnectedDriver(agentPort);
            driver.CloseDriver();

            int requestedPort = GetFreePort(agentPort);
            ActLaunchJavaWSApplication second = RunAttach(requestedPort, 5, out _, CreateRunnerContext(driver));

            StringAssert.Contains(second.Error, WindowNotFoundError);
            Assert.AreEqual(0, GetListeningProcessIds(requestedPort).Count);
            AssertListensOn(processId, agentPort);
        }

        [TestMethod]
        [Timeout(180000)]
        public void Attach_DriverConnectedAndFreeWindowExists_AttachesFreeWindowAsBefore()
        {
            int connectedProcessId = StartTestApp(withAgentJarOnClassPath: false);
            int agentPort = GetFreePort();
            ActLaunchJavaWSApplication first = RunAttach(agentPort, 60, out _);
            Assert.IsTrue(string.IsNullOrEmpty(first.Error), first.Error);
            JavaDriver driver = StartConnectedDriver(agentPort);
            int freeProcessId = StartTestApp(withAgentJarOnClassPath: false);

            int requestedPort = GetFreePort(agentPort);
            ActLaunchJavaWSApplication second = RunAttach(requestedPort, 60, out _, CreateRunnerContext(driver));

            Assert.IsTrue(string.IsNullOrEmpty(second.Error), second.Error);
            Assert.AreEqual(freeProcessId.ToString(), second.ReturnValues.First(rv => rv.Param == "Process ID").Actual);
            Assert.AreEqual(requestedPort.ToString(), second.ReturnValues.First(rv => rv.Param == "Port").Actual);
            AssertListensOn(freeProcessId, requestedPort);
            AssertListensOn(connectedProcessId, agentPort);
        }

        private JavaDriver StartConnectedDriver(int port)
        {
            JavaDriver driver = new JavaDriver(null)
            {
                JavaAgentHost = "127.0.0.1",
                JavaAgentPort = port,
                CommandTimeout = 60,
                ImplicitWait = 10,
                DriverLoadWaitingTime = 30
            };
            mStartedDrivers.Add(driver);
            driver.StartDriver();
            Assert.IsTrue(driver.IsRunning(), "The Java driver did not connect to the agent on port " + port);
            return driver;
        }

        private static Context CreateRunnerContext(JavaDriver driver = null)
        {
            GingerRunner runner = new GingerRunner();
            runner.Executor = new GingerExecutionEngine(runner);
            if (driver != null)
            {
                Agent agent = new Agent { Active = true, DriverType = Agent.eDriverType.JavaDriver, Name = "Java Agent" };
                agent.AgentOperations = new AgentOperations(agent) { Driver = driver };
                runner.ApplicationAgents.Add(new ApplicationAgent { AppName = "JavaTestApp", Agent = agent });
            }
            return new Context { Runner = runner.Executor };
        }

        private ActLaunchJavaWSApplication RunAttach(int port, int maxWaitSeconds, out TimeSpan elapsed, Context context = null)
        {
            ActLaunchJavaWSApplication act = CreateAction(port, maxWaitSeconds);
            act.Context = context;
            Stopwatch clock = Stopwatch.StartNew();
            act.Execute();
            elapsed = clock.Elapsed;
            TestContext.WriteLine($"Port {port}: took {elapsed.TotalSeconds:0.0}s, Error='{act.Error}', Process ID={act.ReturnValues.FirstOrDefault(rv => rv.Param == "Process ID")?.Actual}");
            return act;
        }

        private static ActLaunchJavaWSApplication CreateAction(int port, int maxWaitSeconds)
        {
            ActLaunchJavaWSApplication act = new ActLaunchJavaWSApplication
            {
                LaunchJavaApplication = false,
                LaunchWithAgent = true,
                ShowAgent = false,
                WaitForWindowTitle = WindowTitle,
                WaitForWindowTitleMaxTime = maxWaitSeconds.ToString(),
                Port = port.ToString(),
                ValueExpression = new PassThroughValueExpression(),
                AddNewReturnParams = true
            };
            act.AddOrUpdateInputParamValue(ActLaunchJavaWSApplication.Fields.PortConfigParam, ActLaunchJavaWSApplication.ePortConfigType.Manual.ToString());
            return act;
        }

        private static void CompilePlainApp()
        {
            if (mPlainAppFolder != null)
            {
                return;
            }
            string folder = Path.Combine(Path.GetTempPath(), "GingerAttachTestApp_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, PlainAppClass + ".java"), PlainAppSource);
            string javac = Path.Combine(Path.GetDirectoryName(mJavaExe), "javac.exe");
            if (!File.Exists(javac))
            {
                Assert.Inconclusive("JAVA_HOME has to point to a JDK, javac.exe was not found: " + javac);
            }
            ProcessStartInfo startInfo = new ProcessStartInfo(javac, $"-d \"{folder}\" \"{Path.Combine(folder, PlainAppClass + ".java")}\"") { UseShellExecute = false, RedirectStandardError = true, CreateNoWindow = true };
            using Process process = Process.Start(startInfo);
            string errors = process.StandardError.ReadToEnd();
            process.WaitForExit();
            Assert.AreEqual(0, process.ExitCode, "Compiling the plain Swing application failed: " + errors);
            mPlainAppFolder = folder;
        }

        private static string CreateStderrFloodJar()
        {
            string classesFolder = Path.Combine(mPlainAppFolder, StderrFloodAppClass);
            string jarPath = Path.Combine(mPlainAppFolder, StderrFloodAppClass + ".jar");
            if (File.Exists(jarPath))
            {
                return jarPath;
            }
            Directory.CreateDirectory(classesFolder);
            string sourcePath = Path.Combine(classesFolder, StderrFloodAppClass + ".java");
            File.WriteAllText(sourcePath, StderrFloodAppSource);
            string jdkBin = Path.GetDirectoryName(mJavaExe);
            RunJdkTool(Path.Combine(jdkBin, "javac.exe"), $"-d \"{classesFolder}\" \"{sourcePath}\"");
            RunJdkTool(Path.Combine(jdkBin, "jar.exe"), $"cfe \"{jarPath}\" {StderrFloodAppClass} -C \"{classesFolder}\" .");
            return jarPath;
        }

        private static void RunJdkTool(string tool, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(tool, arguments) { UseShellExecute = false, RedirectStandardError = true, CreateNoWindow = true };
            using Process process = Process.Start(startInfo);
            string errors = process.StandardError.ReadToEnd();
            process.WaitForExit();
            Assert.AreEqual(0, process.ExitCode, $"'{Path.GetFileName(tool)} {arguments}' failed: {errors}");
        }

        /// <summary>
        /// With GingerAgent.jar first on the class path the JVM keeps the jar open, as after an attach,
        /// while no agent is serving: the state left when the application is reopened in the same JVM.
        /// </summary>
        private int StartTestApp(bool withAgentJarOnClassPath)
        {
            string arguments = withAgentJarOnClassPath
                ? $"-cp \"{mAgentJar};{mPlainAppFolder}\" {PlainAppClass}"
                : $"-cp \"{mPlainAppFolder}\" {PlainAppClass}";
            Process process = Process.Start(new ProcessStartInfo
            {
                FileName = mJavaExe,
                Arguments = arguments,
                WorkingDirectory = mPlainAppFolder,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            mStartedProcessIds.Add(process.Id);

            Stopwatch clock = Stopwatch.StartNew();
            while (clock.Elapsed < TimeSpan.FromSeconds(60))
            {
                Thread.Sleep(500);
                process.Refresh();
                Assert.IsFalse(process.HasExited, "The Java test application exited with code " + (process.HasExited ? process.ExitCode : 0));
                if (process.MainWindowTitle.Contains(WindowTitle))
                {
                    return process.Id;
                }
            }
            Assert.Fail($"The Java test application did not show '{WindowTitle}'");
            return -1;
        }

        private void AssertAgentJarLoadedWithoutListener(int processId)
        {
            string handleOutput = RunAndRead(mHandleExe, $"-accepteula -p {processId} GingerAgent.jar");
            if (!handleOutput.Contains("GingerAgent.jar"))
            {
                Assert.Inconclusive("handle.exe does not report GingerAgent.jar for the test process, the agent jar check cannot be exercised here: " + handleOutput);
            }
            Thread.Sleep(2000);
            Assert.IsFalse(GetListeningProcessIdsOfAnyPort().Contains(processId), "Precondition: the test application must not listen on any port");
        }

        private void AssertListensOn(int processId, int port)
        {
            Stopwatch clock = Stopwatch.StartNew();
            List<int> owners = [];
            while (clock.Elapsed < TimeSpan.FromSeconds(30))
            {
                owners = GetListeningProcessIds(port);
                if (owners.Contains(processId))
                {
                    return;
                }
                Thread.Sleep(500);
            }
            Assert.Fail($"Process {processId} does not listen on port {port}. Listening: {string.Join(",", owners)}");
        }

        private void AssertWindowShown(int processId, string titlePrefix)
        {
            Stopwatch clock = Stopwatch.StartNew();
            List<string> titles = [];
            while (clock.Elapsed < TimeSpan.FromSeconds(30))
            {
                titles = GetVisibleWindowTitles(processId);
                if (titles.Any(title => title.StartsWith(titlePrefix, StringComparison.Ordinal)))
                {
                    return;
                }
                Thread.Sleep(500);
            }
            Assert.Fail($"Process {processId} shows no '{titlePrefix}' window. Visible windows: {string.Join(" | ", titles)}");
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

        private static List<string> GetVisibleWindowTitles(int processId)
        {
            List<string> titles = [];
            EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out uint windowProcessId);
                if (windowProcessId == processId && IsWindowVisible(hWnd))
                {
                    StringBuilder title = new StringBuilder(256);
                    GetWindowText(hWnd, title, title.Capacity);
                    titles.Add(title.ToString());
                }
                return true;
            }, IntPtr.Zero);
            return titles;
        }

        private static List<int> GetListeningProcessIds(int port)
        {
            return GetListeners().Where(l => l.Port == port).Select(l => l.ProcessId).Distinct().ToList();
        }

        private static HashSet<int> GetListeningProcessIdsOfAnyPort()
        {
            return GetListeners().Select(l => l.ProcessId).ToHashSet();
        }

        private static List<(int Port, int ProcessId)> GetListeners()
        {
            List<(int Port, int ProcessId)> listeners = [];
            foreach (string line in RunAndRead("netstat.exe", "-ano -p TCP").Split('\n').Concat(RunAndRead("netstat.exe", "-ano -p TCPv6").Split('\n')))
            {
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 5 && parts[3] == "LISTENING"
                    && int.TryParse(parts[1][(parts[1].LastIndexOf(':') + 1)..], out int port)
                    && int.TryParse(parts[4], out int processId))
                {
                    listeners.Add((port, processId));
                }
            }
            return listeners;
        }

        private static int GetFreePort(params int[] exclude)
        {
            while (true)
            {
                TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
                listener.Start();
                int port = ((IPEndPoint)listener.LocalEndpoint).Port;
                listener.Stop();
                if (!exclude.Contains(port))
                {
                    return port;
                }
            }
        }

        private static string JavaVersionOutput()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(mJavaExe, "-version") { UseShellExecute = false, RedirectStandardError = true, CreateNoWindow = true };
            using Process process = Process.Start(startInfo);
            string output = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        private static string RunAndRead(string fileName, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(fileName, arguments) { UseShellExecute = false, RedirectStandardOutput = true, CreateNoWindow = true };
            using Process process = Process.Start(startInfo);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        private static void KillTestAppWindows()
        {
            foreach (Process process in Process.GetProcessesByName("java"))
            {
                try
                {
                    if (process.MainWindowTitle.Contains(WindowTitle))
                    {
                        KillProcess(process.Id);
                    }
                }
                catch (InvalidOperationException)
                {
                    // already gone
                }
            }
        }

        private static void KillProcess(int processId)
        {
            try
            {
                using Process process = Process.GetProcessById(processId);
                process.Kill();
                process.WaitForExit(10000);
            }
            catch (ArgumentException)
            {
                // already gone
            }
            catch (InvalidOperationException)
            {
                // already gone
            }
        }

        private sealed class PassThroughValueExpression : IValueExpression
        {
            public string Value { get; set; }
            public string ValueCalculated => Value;
            public bool DecryptFlag { get; set; }
            public string EncryptedValue => null;
            public string Calculate(string expression) => expression;
        }
    }
}
