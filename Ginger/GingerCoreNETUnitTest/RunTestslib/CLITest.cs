using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.RosLynLib;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore.Environments;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Amdocs.Ginger.CoreNET.RunLib.CLILib.CLIArgs;

namespace WorkspaceHold
{    
    [Ignore] // temp
    [Level3]
    [TestClass]
    public class CLITest
    {
        static WorkspaceLocker mWorkspaceLocker = new WorkspaceLocker("CLITest");

        // TODO: run one by one as it used same run exc
        static string mTempFolder;
        static string mSolutionFolder;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {            
            mTempFolder = TestResources.GetTempFolder("CLI Tests");
            mSolutionFolder = Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "CLI");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            mWorkspaceLocker.ReleaseWorkspace();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            WorkspaceHelper.InitWS(mWorkspaceLocker);  // we get seperate workspace for each test
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            
        }


        [TestMethod]
        public void CLIConfigTest()
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
            runSetAutoRunConfiguration.CreateConfigFile();

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { runSetAutoRunConfiguration.SelectedCLI.Identifier + "=" + runSetAutoRunConfiguration.ConfigFileFullPath });

            // Assert            
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");
        }

        [TestMethod]
        public void CLIConfigRegressionTest()
        {
            //Arrange
            // PrepareForCLIExecution();
            //Create config file            
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
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");
        }

        [TestMethod]
        public void CLIDynamicTest()
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
            runSetAutoRunConfiguration.SelectedCLI = new CLIDynamicXML();
            runSetAutoRunConfiguration.CreateConfigFile();

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { runSetAutoRunConfiguration.SelectedCLI.Identifier + "=" + runSetAutoRunConfiguration.ConfigFileFullPath });

            // Assert            
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");
        }

        [TestMethod]
        public void CLIDynamicRegressionTest()
        {
            //Arrange
            // PrepareForCLIExecution();
            //Create config file       
            string fileName = Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-Default Run Set.Ginger.AutoRunConfigs.xml");
            string dynamicXML= System.IO.File.ReadAllText(fileName);
            dynamicXML = dynamicXML.Replace("SOLUTION_PATH", mSolutionFolder);
            string configFile = TestResources.GetTempFile("CLI-Default Run Set.Ginger.AutoRunConfigs.xml");
            System.IO.File.WriteAllText(configFile, dynamicXML);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { "Dynamic=" + configFile });

            // Assert            
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");
        }


        [TestMethod]
        public void CLIScriptTest()
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
            runSetAutoRunConfiguration.SelectedCLI = new CLIScriptFile();
            runSetAutoRunConfiguration.CreateConfigFile();

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { runSetAutoRunConfiguration.SelectedCLI.Identifier + "=" + runSetAutoRunConfiguration.ConfigFileFullPath });

            // Assert            
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");
        }

        [TestMethod]
        public void CLIScriptRegressionTest()
        {
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
            CLI.ExecuteArgs(new string[] { "Script=" + scriptFile });

            // Assert
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");
        }

        [TestMethod]
        public void CLIArgsTest()
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
            runSetAutoRunConfiguration.SelectedCLI = new CLIArgs();

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { runSetAutoRunConfiguration.SelectedCLI.Identifier + "=" + runSetAutoRunConfiguration.ConfigFileContent });

            // Assert            
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");
        }

        [TestMethod]
        public void CLIArgsRegressionTest()
        {
            //Arrange
            // PrepareForCLIExecution();
            // Create config file
            string args = string.Format("--solution {0}", mSolutionFolder);
            args += string.Format("--environment {0}", "Default");
            args += string.Format("--runset {0}", "Default Run Set");
            args += string.Format("--runAnalyzer {0}", "True");
            args += string.Format("--showAutoRunWindow {0}", "False");

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { "--args", args });

            // Assert            
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");
        }

        
        [TestMethod]
        public void ArgSplit1()
        {
            // Arrange
            CLIArgs CLIArgs = new CLIArgs();

            // Act
            List<Arg> args = CLIArgs.SplitArgs(@"--solution c:\abc\def\sol1");

            Assert.AreEqual(args[0].ArgName, "--solution");
            Assert.AreEqual(args[0].ArgValue, @"c:\abc\def\sol1");
        }

        [TestMethod]
        public void ArgSplit2()
        {
            // Arrange
            CLIArgs CLIArgs = new CLIArgs();

            // Act
            List<Arg> args = CLIArgs.SplitArgs(@"--solution c:\abc\def\sol1 --environment Env1");

            Assert.AreEqual(args[0].ArgName, "--solution");
            Assert.AreEqual(args[0].ArgValue, @"c:\abc\def\sol1");

            Assert.AreEqual(args[1].ArgName, "--environment");
            Assert.AreEqual(args[1].ArgValue, "Env1");
        }

        [TestMethod]
        public void ArgSplit2WithSpaces()
        {
            // Arrange
            CLIArgs CLIArgs = new CLIArgs();

            // Act
            List<Arg> args = CLIArgs.SplitArgs(@"  --solution  c:\abc\def\sol1    --environment  Env1");

            Assert.AreEqual(args[0].ArgName, "--solution");
            Assert.AreEqual(args[0].ArgValue, @"c:\abc\def\sol1");

            Assert.AreEqual(args[1].ArgName, "--environment");
            Assert.AreEqual(args[1].ArgValue, "Env1");
        }

        [TestMethod]
        public void ArgsMixStyle()
        {
            // Arrange
            CLIArgs CLIArgs = new CLIArgs();

            // Act
            List<Arg> args = CLIArgs.SplitArgs(@"-s c:\abc\def\sol1 --environment Env1");

            Assert.AreEqual(args[0].ArgName, "-s");
            Assert.AreEqual(args[0].ArgValue, @"c:\abc\def\sol1");

            Assert.AreEqual(args[1].ArgName, "--environment");
            Assert.AreEqual(args[1].ArgValue, "Env1");
        }


        private void PrepareForCLICreationAndExecution()
        {
            //WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            //WorkSpace.Init(WSEH);
            //WorkSpace.Instance.RunningFromUnitTest = true;
            //WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

            WorkSpace.Instance.OpenSolution(mSolutionFolder);
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;
            RunsetExecutor runsetExecutor = new RunsetExecutor();
            runsetExecutor.RunsetExecutionEnvironment = (from x in SR.GetAllRepositoryItems<ProjEnvironment>() where x.Name == "Default" select x).SingleOrDefault();
            runsetExecutor.RunSetConfig = (from x in SR.GetAllRepositoryItems<RunSetConfig>() where x.Name == "Default Run Set" select x).SingleOrDefault();
            WorkSpace.Instance.RunsetExecutor = runsetExecutor;
            WorkSpace.Instance.RunsetExecutor.InitRunners();
        }

        //private void PrepareForCLIExecution()
        //{
            
            //WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            //WorkSpace.Init(WSEH);
            //WorkSpace.Instance.RunningFromUnitTest = true;
            //WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());
        //}


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

        //}
    }
}
