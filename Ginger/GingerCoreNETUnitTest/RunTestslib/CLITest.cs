using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.RosLynLib;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Amdocs.Ginger.Repository;
using CommandLine;
using Ginger.Run;
using GingerCore.Environments;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amdocs.Ginger.Common;

namespace WorkspaceHold
{
    [Level3]
    [TestClass]
    public class CLITest
    {
        // TODO: run one by one as it used same run exc
        static string mTempFolder;
        static string mSolutionFolder;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            mTempFolder = TestResources.GetTempFolder(nameof(CLITest));
            mSolutionFolder = Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "CLI");

            // Hook console message
            Reporter.logToConsoleEvent += ConsoleMessageEvent;
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {

        }


        [TestInitialize]
        public void TestInitialize()
        {
            WorkSpace.LockWS();
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            WorkSpace.RelWS();            
        }


        [TestMethod]
        public void OLDCLIConfigTest()
        {
            
            // Arrange
            PrepareForCLICreationAndExecution();
            // Create config file
            CLIHelper cLIHelper = new CLIHelper();
            cLIHelper.RunAnalyzer = true;
            cLIHelper.ShowAutoRunWindow = false;
            cLIHelper.DownloadUpgradeSolutionFromSourceControl = false;

            RunSetAutoRunConfiguration runSetAutoRunConfiguration = new RunSetAutoRunConfiguration(WorkSpace.Instance.Solution, WorkSpace.Instance.RunsetExecutor, cLIHelper);
            runSetAutoRunConfiguration.ConfigFileFolderPath = mTempFolder;
            runSetAutoRunConfiguration.SelectedCLI = new CLIConfigFile();
            runSetAutoRunConfiguration.CreateContentFile();

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { "ConfigFile=" + runSetAutoRunConfiguration.ConfigFileFullPath });

            // Assert            
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");
            
        }

        [TestMethod]
        public void OLDCLIConfigRegressionTest()
        {
            
            //Arrange                
            //Create config file         
            PrepareForCLICreationAndExecution();
            string txt = string.Format("Solution={0}", mSolutionFolder) + Environment.NewLine;
            txt += string.Format("Env={0}", "Default") + Environment.NewLine;
            txt += string.Format("RunSet={0}", "Default Run Set") + Environment.NewLine;
            txt += string.Format("RunAnalyzer={0}", "True") + Environment.NewLine;
            txt += string.Format("ShowAutoRunWindow={0}", "False") + Environment.NewLine;
            string configFile = TestResources.GetTempFile("runset1.ginger.config");
            System.IO.File.WriteAllText(configFile, txt);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { "ConfigFile=" + configFile });

            // Assert            
            Assert.AreEqual(eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, "BF RunStatus=Passed");
            
        }


        [TestMethod]
        public void CLIVersion()
        {

            //Arrange                            
            PrepareForCLICreationAndExecution();

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { "-t" });

            // Assert            
            // ????

        }

        [TestMethod]
        public void CLIBadParams()
        {
            lock (mConsoleMessages)
            {
                //Arrange                            
                mConsoleMessages.Clear();
                CLIProcessor CLI = new CLIProcessor();

                // Act            
                CLI.ExecuteArgs(new string[] { "--blabla" });

                // Assert            
                Assert.AreEqual(1, mConsoleMessages.Count, "There is one console message");
                Assert.AreEqual(eLogLevel.ERROR, mConsoleMessages[0].LogLevel, "message loglevel is ERROR");
                Assert.AreEqual("Please fix the arguments and try again", mConsoleMessages[0].MessageToConsole, "console message");
            }

        }

        [TestMethod]
        public void CLIHelp()
        {
            lock (mConsoleMessages)
            {
                //Arrange                            
                mConsoleMessages.Clear();
                CLIProcessor CLI = new CLIProcessor();

                // Act            
                CLI.ExecuteArgs(new string[] { "help" });

                // Assert            
                Assert.AreEqual(1, mConsoleMessages.Count, "message count");
                Assert.IsTrue(mConsoleMessages[0].MessageToConsole.Contains("Ginger support"), "help message");
            }
        }

        [TestMethod]
        public void CLIGridWithoutParams()
        {
            lock (mConsoleMessages)
            {
                //Arrange                            
                mConsoleMessages.Clear();
                CLIProcessor CLI = new CLIProcessor();

                // Act            
                CLI.ExecuteArgs(new string[] { "grid" });

                // Assert            
                Assert.AreEqual(1, mConsoleMessages.Count, "There are 5 lines of help");
                Assert.AreEqual(eLogLevel.INFO, mConsoleMessages[0].LogLevel, "message loglevel is ERROR");
                Assert.AreEqual("Starting Ginger Grid at port: 15001", mConsoleMessages[0].MessageToConsole, "console message");
            }

        }

        class ConsoleMessage
        {
            public eLogLevel LogLevel;
            public string MessageToConsole;
        }

        static List<ConsoleMessage> mConsoleMessages = new List<ConsoleMessage>();
        public static void ConsoleMessageEvent(eLogLevel logLevel, string messageToConsole)
        {
            mConsoleMessages.Add(new ConsoleMessage(){LogLevel = logLevel, MessageToConsole = messageToConsole});
        }

        

        [TestMethod]
        public void CLIDynamicTest()
        {
            //lock (WorkSpace.Instance)
            //{
                // Arrange
                PrepareForCLICreationAndExecution();
                // Create config file
                CLIHelper cLIHelper = new CLIHelper();
                cLIHelper.RunAnalyzer = true;
                cLIHelper.ShowAutoRunWindow = false;
                cLIHelper.DownloadUpgradeSolutionFromSourceControl = false;

                RunSetAutoRunConfiguration runSetAutoRunConfiguration = new RunSetAutoRunConfiguration(WorkSpace.Instance.Solution, WorkSpace.Instance.RunsetExecutor, cLIHelper);
                runSetAutoRunConfiguration.ConfigFileFolderPath = mTempFolder;
                runSetAutoRunConfiguration.SelectedCLI = new CLIDynamicXML();
                runSetAutoRunConfiguration.CreateContentFile();

                // Act            
                CLIProcessor CLI = new CLIProcessor();
                CLI.ExecuteArgs(new string[] { "dynamic", "-f" ,runSetAutoRunConfiguration.ConfigFileFullPath });

                // Assert            
                Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, eRunStatus.Passed, "BF RunStatus=Passed");
            //}
        }

        [TestMethod]
        public void OLDCLIDynamicRegressionTest()
        {
            //lock (WorkSpace.Instance)
            //{
                //Arrange
                // PrepareForCLIExecution();
                //Create config file       
                string fileName = Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-Default Run Set.Ginger.AutoRunConfigs.xml");
                string dynamicXML = System.IO.File.ReadAllText(fileName);
                dynamicXML = dynamicXML.Replace("SOLUTION_PATH", mSolutionFolder);
                string configFile = TestResources.GetTempFile("CLI-Default Run Set.Ginger.AutoRunConfigs.xml");
                System.IO.File.WriteAllText(configFile, dynamicXML);

                // Act            
                CLIProcessor CLI = new CLIProcessor();
                CLI.ExecuteArgs(new string[] { "Dynamic=" + configFile });

                // Assert            
                Assert.AreEqual(eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, "BF RunStatus=Passed");
            //}
        }


        [TestMethod]
        public void OLDCLIConfigFileTest()
        {
            //lock (WorkSpace.Instance)
            //{
                // Arrange
                PrepareForCLICreationAndExecution();
                // Create config file
                CLIHelper cLIHelper = new CLIHelper();
                cLIHelper.RunAnalyzer = true;
                cLIHelper.ShowAutoRunWindow = false;
                cLIHelper.DownloadUpgradeSolutionFromSourceControl = false;

                RunSetAutoRunConfiguration runSetAutoRunConfiguration = new RunSetAutoRunConfiguration(WorkSpace.Instance.Solution, WorkSpace.Instance.RunsetExecutor, cLIHelper);
                runSetAutoRunConfiguration.ConfigFileFolderPath = mTempFolder;
                runSetAutoRunConfiguration.SelectedCLI = new CLIConfigFile();
                runSetAutoRunConfiguration.CreateContentFile();

                // Act            
                CLIProcessor CLI = new CLIProcessor();
                CLI.ExecuteArgs(new string[] { "ConfigFile=" + runSetAutoRunConfiguration.ConfigFileFullPath });

                // Assert            
                Assert.AreEqual(eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, "BF RunStatus=Passed");
            //}
        }

        [TestMethod]
        public void CLIScriptRegressionTest()
        {
            //lock (WorkSpace.Instance)
            //{

                //Arrange
                // PrepareForCLIExecution();
                // Create config file
                string scriptFile = TestResources.GetTempFile("runset1.ginger.script");
                string jsonFileName = TestResources.GetTempFile("runset.json");
                string txt = "int i=1;" + Environment.NewLine;
                txt += "i++;" + Environment.NewLine;
                txt += nameof(GingerScriptGlobals.OpenSolution) + "(@\"" + mSolutionFolder + "\");" + Environment.NewLine;
                txt += nameof(GingerScriptGlobals.OpenRunSet) + "(\"Default Run Set\", \"Default\");" + Environment.NewLine;    // Runset, env
                txt += nameof(GingerScriptGlobals.CreateExecutionSummaryJSON) + "(@\"" + jsonFileName + "\");" + Environment.NewLine;    // summary json
                txt += "i" + Environment.NewLine;  // script rc
                System.IO.File.WriteAllText(scriptFile, txt);

                // Act
                CLIProcessor CLI = new CLIProcessor();
                CLI.ExecuteArgs(new string[] { "script", "-f" , scriptFile });

                // Assert
                Assert.AreEqual(eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, "BF RunStatus=Passed");
            //}
        }

        [TestMethod]
        public void CLIArgsTest()
        {
            //lock (WorkSpace.Instance)
            //{
                // Arrange
                PrepareForCLICreationAndExecution();
                // Create config file
                CLIHelper cLIHelper = new CLIHelper();
                cLIHelper.RunAnalyzer = true;
                cLIHelper.ShowAutoRunWindow = false;
                cLIHelper.DownloadUpgradeSolutionFromSourceControl = false;

                RunSetAutoRunConfiguration runSetAutoRunConfiguration = new RunSetAutoRunConfiguration(WorkSpace.Instance.Solution, WorkSpace.Instance.RunsetExecutor, cLIHelper);
                runSetAutoRunConfiguration.ConfigFileFolderPath = mTempFolder;
                runSetAutoRunConfiguration.SelectedCLI = new CLIArgs();

                // Act            
                CLIProcessor CLI = new CLIProcessor();
                string[] args = CommandLineToStringArray(runSetAutoRunConfiguration.CLIContent).ToArray();
                CLI.ExecuteArgs(args);

                // Assert            
                Assert.AreEqual(eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, "BF RunStatus=Passed");
            //}
        }


        



        [TestMethod]
        public void CLIArgsWithAnalyzeTest()
        {
            //lock (WorkSpace.Instance)
            //{
                //Arrange
                PrepareForCLICreationAndExecution();
                // Create args
                string[] args = { "run", "--solution", mSolutionFolder, "--environment", "Default", "--runset", "Default Run Set", "--analyze"};
                
                // Act            
                CLIProcessor CLI = new CLIProcessor();
                CLI.ExecuteArgs(args);

                // Assert            
                Assert.AreEqual(eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, "BF RunStatus=Passed");

            // TODO: test analyze was run !!!!
            //}
        }


        private void PrepareForCLICreationAndExecution()
        {            
            WorkSpace.Instance.OpenSolution(mSolutionFolder);
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;
            RunsetExecutor runsetExecutor = new RunsetExecutor();
            runsetExecutor.RunsetExecutionEnvironment = (from x in SR.GetAllRepositoryItems<ProjEnvironment>() where x.Name == "Default" select x).SingleOrDefault();
            runsetExecutor.RunSetConfig = (from x in SR.GetAllRepositoryItems<RunSetConfig>() where x.Name == "Default Run Set" select x).SingleOrDefault();
            WorkSpace.Instance.RunsetExecutor = runsetExecutor;
            WorkSpace.Instance.RunsetExecutor.InitRunners();
        }




        //[Ignore]
        //[TestMethod]
        //public void RunFlow()
        //{
        //    // Arrange
        //    WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
        //    WorkSpace.Init(WSEH);
        //    WorkSpace.Instance.RunningFromUnitTest = true;

        //    WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

        //    // Create script file

        //    // Generate a script which contains something like below and exeucte it
        //    // int i = 1;
        //    // i++;
        //    // OpenSolution(@"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TestResources\Solutions\CLI");
        //    // OpenRunSet("Default Run Set", "Default");
        //    // CreateExecutionSummaryJSON(@"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TempFolder\runset.json");
        //    // i

        //    string CLISolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\CLI");
        //    string scriptFile = TestResources.GetTempFile("runset1.ginger.script");
        //    string jsonFileName = TestResources.GetTempFile("runset.json");
        //    string txt = "int i=1;" + Environment.NewLine;
        //    txt += "i++;" + Environment.NewLine;
        //    txt += nameof(GingerScriptGlobals.OpenSolution) + "(@\"" + CLISolutionFolder + "\");" + Environment.NewLine;
        //    txt += nameof(GingerScriptGlobals.RunBusinessFlow) + "(\"Flow 1\");" + Environment.NewLine;    // Runset, env


        //    txt += "i" + Environment.NewLine;  // script rc
        //    System.IO.File.WriteAllText(scriptFile, txt);


        //    // Act
        //    CLIProcessor CLI = new CLIProcessor();
        //    CLI.ExecuteArgs(new string[] { "--scriptfile=" , scriptFile });

        //    // Assert
        //    // Assert.AreEqual("1")
        //    // Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");
        //}

        //[Ignore]
        //[TestMethod]
        //public void TestRunSetHTMLReport()
        //{
        //    // Arrange
        //    WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
        //    WorkSpace.Init(WSEH);
        //    WorkSpace.Instance.RunningFromUnitTest = true;
        //    WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

        //    // Create script file

        //    // Generate a script which contains something like below and exeucte it
        //    // int i = 1;
        //    // i++;
        //    // OpenSolution(@"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TestResources\Solutions\CLI");
        //    // OpenRunSet("Default Run Set", "Default");
        //    // CreateExecutionSummaryJSON(@"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TempFolder\runset.json");
        //    // i

        //    string CLISolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\CLI");
        //    string scriptFile = TestResources.GetTempFile("runset1.ginger.script");
        //    string jsonFileName = TestResources.GetTempFile("runset.json");
        //    string txt = "int i=1;" + Environment.NewLine;
        //    txt += "i++;" + Environment.NewLine;
        //    txt += nameof(GingerScriptGlobals.OpenSolution) + "(@\"" + CLISolutionFolder + "\");" + Environment.NewLine;
        //    txt += nameof(GingerScriptGlobals.OpenRunSet) + "(\"Default Run Set\", \"Default\");" + Environment.NewLine;    // Runset, env
        //    txt += nameof(GingerScriptGlobals.CreateExecutionHTMLReport) + "(@\"" + jsonFileName + "\");" + Environment.NewLine;    // summary json


        //    txt += "i" + Environment.NewLine;  // script rc
        //    System.IO.File.WriteAllText(scriptFile, txt);

        //    // Act
        //    CLIProcessor CLI = new CLIProcessor();
        //    CLI.ExecuteArgs(new string[] { "--scriptfile=", scriptFile });

        //    // Assert
        //    // Assert.AreEqual("1")
        //    Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");


        [TestMethod]
        public void NewCLIArgsRegressionTest()
        {            
            //Arrange
            PrepareForCLICreationAndExecution();

            // Create args
            List<string> args = new List<string>();

            args.Add("run");

            args.Add("--solution");
            args.Add(mSolutionFolder);

            args.Add("--environment");
            args.Add("Default");

            args.Add("--runset");
            args.Add("Default Run Set");

            args.Add("--analyze");

            args.Add("--showAutoRunWindow");            

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(args.ToArray());

            // Assert            
            Assert.AreEqual(eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, "BF RunStatus=Passed");
            
        }


        [TestMethod]
        public void NewCreateCLIArgs()
        {
            //Arrange
            RunOptions options = new RunOptions() { Environment = "env1", RunAnalyzer = true, Runset = "rs1" };


            // Act            
            var arguments = CommandLine.Parser.Default.FormatCommandLine<RunOptions>(options);

            // Assert            
            Assert.IsTrue(arguments.StartsWith("run"), "arguments Starts With run");
            Assert.IsTrue(arguments.Contains("--environment env1"), "arguments Contains --environment env1");
            Assert.IsTrue(arguments.Contains("--runset rs1"), "arguments Contains --runset rs1");
            Assert.IsTrue(arguments.Contains("--analyze"), "arguments Contains --analyze");                   
        }

        [TestMethod]
        public void ParseStringToArgs()
        {
            //Arrange
            string s = @"run -s c:\123 -e env1";

            // Act            
            var arguments = CommandLineToStringArray(s);

            // Assert            
            Assert.IsTrue(arguments.First() == "run");
            Assert.IsTrue(arguments.Contains("-s"), "arguments Contains -s");
            Assert.IsTrue(arguments.Contains(@"c:\123"), @"arguments Contains c:\123");
            Assert.IsTrue(arguments.Contains("-e"), "arguments Contains -e");
            Assert.IsTrue(arguments.Contains("env1"), "arguments Contains env1");
        }

        // Parse a command line with multiple switches to string list - used for test only!
        public static IEnumerable<string> CommandLineToStringArray(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
                yield break;

            var sb = new StringBuilder();
            bool inQuote = false;
            foreach (char c in commandLine)
            {
                if (c == '"' && !inQuote)
                {
                    inQuote = true;
                    continue;
                }

                if (c != '"' && !(char.IsWhiteSpace(c) && !inQuote))
                {
                    sb.Append(c);
                    continue;
                }

                if (sb.Length > 0)
                {
                    var result = sb.ToString();
                    sb.Clear();
                    inQuote = false;
                    yield return result;
                }
            }

            if (sb.Length > 0)
                yield return sb.ToString();
        }


    }
}
