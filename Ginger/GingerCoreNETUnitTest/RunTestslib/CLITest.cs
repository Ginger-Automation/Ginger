using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.RosLynLib;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using static Amdocs.Ginger.CoreNET.RunLib.CLILib.CLIArgs;

namespace GingerCoreNETUnitTest.RunTestslib
{
    [Level3]
    [TestClass]
    public class CLITest
    {
        // TODO: run one by one as it used same run exc

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {            
        
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            
        }

        [TestInitialize]
        public void TestInitialize()
        {


        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }



        [TestMethod]
        public void ScriptFile()
        {
            // Arrange
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;

            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

            // Create script file

            // Generate a script which contains something like below and exeucte it
            // int i = 1;
            // i++;
            // OpenSolution(@"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TestResources\Solutions\CLI");
            // OpenRunSet("Default Run Set", "Default");
            // CreateExecutionSummaryJSON(@"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TempFolder\runset.json");
            // i

            string CLISolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\CLI");
            string scriptFile = TestResources.GetTempFile("runset1.ginger.script");
            string jsonFileName = TestResources.GetTempFile("runset.json");
            string txt = "int i=1;" + Environment.NewLine;
            txt += "i++;" + Environment.NewLine;
            txt += nameof(GingerScriptGlobals.OpenSolution) +  "(@\"" + CLISolutionFolder + "\");" + Environment.NewLine;
            txt += nameof(GingerScriptGlobals.OpenRunSet) + "(\"Default Run Set\", \"Default\");" + Environment.NewLine;    // Runset, env
            txt += nameof(GingerScriptGlobals.CreateExecutionSummaryJSON) + "(@\"" + jsonFileName + "\");" + Environment.NewLine;    // summary json


            txt += "i" + Environment.NewLine;  // script rc
            System.IO.File.WriteAllText(scriptFile, txt);
            
            // Act
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { "scriptfile", scriptFile });

            // Assert
            // Assert.AreEqual("1")
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");

        }

        [TestMethod]
        public void ConfigFile()
        {
            // Arrange
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;
            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

            // Create config file
            string CLISolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\CLI");
            string scriptFile = TestResources.GetTempFile("runset1.ginger.config");                        
            string txt = string.Format("Solution={0}", CLISolutionFolder) + Environment.NewLine;
            txt += string.Format("Env={0}", "Default") + Environment.NewLine;
            txt += string.Format("RunSet={0}", "Default Run Set") + Environment.NewLine;
            System.IO.File.WriteAllText(scriptFile, txt);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { "ConfigFile=" , scriptFile });

            // Assert            
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");
        }


        [TestMethod]
        public void CLIArgs()
        {
            // Arrange
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;
            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

            // Create config file
            string CLISolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\CLI");
            

            // TODO: use also CLIArgs creator
            string args = string.Format("--solution {0}", CLISolutionFolder) ;
            args += string.Format("--environment {0}", "Default");
            args += string.Format("--runset {0}", "Default Run Set");            

            // Act
            string arg = "Args=" + args;
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { arg });

            // Assert            
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");
        }


        [TestMethod]
        public void ResultsJSON()
        {

        }

        [TestMethod]
        public void RunFlow()
        {
            // Arrange
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;

            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

            // Create script file

            // Generate a script which contains something like below and exeucte it
            // int i = 1;
            // i++;
            // OpenSolution(@"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TestResources\Solutions\CLI");
            // OpenRunSet("Default Run Set", "Default");
            // CreateExecutionSummaryJSON(@"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TempFolder\runset.json");
            // i

            string CLISolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\CLI");
            string scriptFile = TestResources.GetTempFile("runset1.ginger.script");
            string jsonFileName = TestResources.GetTempFile("runset.json");
            string txt = "int i=1;" + Environment.NewLine;
            txt += "i++;" + Environment.NewLine;
            txt += nameof(GingerScriptGlobals.OpenSolution) + "(@\"" + CLISolutionFolder + "\");" + Environment.NewLine;
            txt += nameof(GingerScriptGlobals.RunBusinessFlow) + "(\"Flow 1\");" + Environment.NewLine;    // Runset, env
            

            txt += "i" + Environment.NewLine;  // script rc
            System.IO.File.WriteAllText(scriptFile, txt);


            string arg = "ScriptFile=" + scriptFile;

            // Act
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { arg });

            // Assert
            // Assert.AreEqual("1")
            // Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");
        }

        [Ignore]
        [TestMethod]
        public void TestRunSetHTMLReport()
        {
            // Arrange
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;

            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

            // Create script file

            // Generate a script which contains something like below and exeucte it
            // int i = 1;
            // i++;
            // OpenSolution(@"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TestResources\Solutions\CLI");
            // OpenRunSet("Default Run Set", "Default");
            // CreateExecutionSummaryJSON(@"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TempFolder\runset.json");
            // i

            string CLISolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\CLI");
            string scriptFile = TestResources.GetTempFile("runset1.ginger.script");
            string jsonFileName = TestResources.GetTempFile("runset.json");
            string txt = "int i=1;" + Environment.NewLine;
            txt += "i++;" + Environment.NewLine;
            txt += nameof(GingerScriptGlobals.OpenSolution) + "(@\"" + CLISolutionFolder + "\");" + Environment.NewLine;
            txt += nameof(GingerScriptGlobals.OpenRunSet) + "(\"Default Run Set\", \"Default\");" + Environment.NewLine;    // Runset, env
            txt += nameof(GingerScriptGlobals.CreateExecutionHTMLReport) + "(@\"" + jsonFileName + "\");" + Environment.NewLine;    // summary json


            txt += "i" + Environment.NewLine;  // script rc
            System.IO.File.WriteAllText(scriptFile, txt);


            string arg = "ScriptFile=" + scriptFile;

            // Act
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { arg });

            // Assert
            // Assert.AreEqual("1")
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");

        }

        
        [TestMethod]
        public void ArgSplit1()
        {
            // Arrange
            CLIArgs CLIArgs = new CLIArgs();

            // Act
            List<Arg> args = CLIArgs.SplitArgs(@"--solution c:\abc\def\sol1");

            Assert.AreEqual(args[0].prefix, "--");
            Assert.AreEqual(args[0].param, "solution");
            Assert.AreEqual(args[0].value, @"c:\abc\def\sol1");
        }

        [TestMethod]
        public void ArgSplit2()
        {
            // Arrange
            CLIArgs CLIArgs = new CLIArgs();

            // Act
            List<Arg> args = CLIArgs.SplitArgs(@"--solution c:\abc\def\sol1 --environment Env1");

            Assert.AreEqual(args[0].prefix, "--");
            Assert.AreEqual(args[0].param, "solution");
            Assert.AreEqual(args[0].value, @"c:\abc\def\sol1");

            Assert.AreEqual(args[1].prefix, "--");
            Assert.AreEqual(args[1].param, "environment");
            Assert.AreEqual(args[1].value, "Env1");
        }

        [TestMethod]
        public void ArgSplit2WithSpaces()
        {
            // Arrange
            CLIArgs CLIArgs = new CLIArgs();

            // Act
            List<Arg> args = CLIArgs.SplitArgs(@"  --solution  c:\abc\def\sol1    --environment  Env1");

            Assert.AreEqual(args[0].prefix, "--");
            Assert.AreEqual(args[0].param, "solution");
            Assert.AreEqual(args[0].value, @"c:\abc\def\sol1");

            Assert.AreEqual(args[1].prefix, "--");
            Assert.AreEqual(args[1].param, "environment");
            Assert.AreEqual(args[1].value, "Env1");
        }

        [TestMethod]
        public void ArgsMixStyle()
        {
            // Arrange
            CLIArgs CLIArgs = new CLIArgs();

            // Act
            List<Arg> args = CLIArgs.SplitArgs(@"-s c:\abc\def\sol1 --environment Env1");

            Assert.AreEqual(args[0].prefix, "-");
            Assert.AreEqual(args[0].param, "s");
            Assert.AreEqual(args[0].value, @"c:\abc\def\sol1");

            Assert.AreEqual(args[1].prefix, "--");
            Assert.AreEqual(args[1].param, "environment");
            Assert.AreEqual(args[1].value, "Env1");
        }


    }
}
