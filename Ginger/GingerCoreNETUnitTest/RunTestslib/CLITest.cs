using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.RosLynLib;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
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


        [Ignore]
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
            CLI.ExecuteArgs(new string[] { "--scriptfile", scriptFile });

            // Assert
            // Assert.AreEqual("1")
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");

        }

        [TestMethod]
        public void CLIConfigFile()
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
            txt += string.Format("ShowAutoRunWindow={0}", "False") + Environment.NewLine;
            System.IO.File.WriteAllText(scriptFile, txt);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { "ConfigFile=" + scriptFile });

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
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { "--args", args });

            // Assert            
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");
        }


        [TestMethod]
        public void ResultsJSON()
        {

        }

        [Ignore]
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


            // Act
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { "--scriptfile=" , scriptFile });

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
            
            // Act
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { "--scriptfile=", scriptFile });

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

        [TestMethod]
        public void TestRunsetAutoRunConfigSettings()
        {
            // Arrange
            Solution sol = new Solution();
            sol.Folder = Path.Combine(Path.GetTempPath(), "CliTest" + DateTime.Now.ToString("_dd-MMM-yy_HH-mm"));
            Directory.CreateDirectory(sol.Folder);
            sol.Name = "TestSolution";
            RunsetExecutor executer = new RunsetExecutor();
            executer.RunsetExecutionEnvironment = new GingerCore.Environments.ProjEnvironment() { Name = "TestEnv" };
            RunSetConfig runsetConfig = new RunSetConfig();
            runsetConfig.Name = "TestRunset";
            executer.RunSetConfig = runsetConfig;
            CLIHelper cliHelp = new CLIHelper();
            RunSetAutoRunConfiguration autoRunConfig = new RunSetAutoRunConfiguration(sol, executer, cliHelp);

            // Act
            autoRunConfig.SelectedCLI = new CLIConfigFile();


            Assert.AreEqual(autoRunConfig.ConfigFileFolderPath, Path.Combine(sol.Folder, @"Documents\RunSetShortCuts\"));
            Assert.AreEqual(autoRunConfig.ConfigFileName, "TestSolution-TestRunset.Ginger.AutoRunConfigs.Config");
            Assert.AreEqual(autoRunConfig.ConfigFileFullPath, Path.Combine(sol.Folder, @"Documents\RunSetShortCuts\", "TestSolution-TestRunset.Ginger.AutoRunConfigs.Config"));
        }

        [TestMethod]
        public void TestRunsetAutoRunConfigCreation()
        {
            // Arrange
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;
            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());
            Solution sol = new Solution();
            sol.Folder = Path.Combine(Path.GetTempPath(), "CliTest" + DateTime.Now.ToString("_dd-MMM-yy_HH-mm"));
            Directory.CreateDirectory(sol.Folder);
            sol.Name = "TestSolution";
            RunsetExecutor executer = new RunsetExecutor();
            executer.RunsetExecutionEnvironment = new GingerCore.Environments.ProjEnvironment() { Name = "TestEnv" };
            RunSetConfig runsetConfig = new RunSetConfig();
            runsetConfig.Name = "TestRunset";
            executer.RunSetConfig = runsetConfig;
            CLIHelper cliHelp = new CLIHelper();
            RunSetAutoRunConfiguration autoRunConfig = new RunSetAutoRunConfiguration(sol, executer, cliHelp);

            // Act
            autoRunConfig.SelectedCLI = new CLIConfigFile();
            autoRunConfig.CreateConfigFile();

            Assert.AreEqual(File.Exists(autoRunConfig.ConfigFileFullPath), true);
        }

        [TestMethod]
        public void TestRunsetAutoRunConfigCreationContent()
        {
            // Arrange
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;
            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());
            Solution sol = new Solution();
            sol.Folder = Path.Combine(Path.GetTempPath(), "CliTest" + DateTime.Now.ToString("_dd-MMM-yy_HH-mm"));
            Directory.CreateDirectory(sol.Folder);
            sol.Name = "TestSolution";            
            RunsetExecutor executer = new RunsetExecutor();
            executer.RunsetExecutionEnvironment = new GingerCore.Environments.ProjEnvironment() { Name = "TestEnv" };
            RunSetConfig runsetConfig = new RunSetConfig();
            runsetConfig.Name = "TestRunset";
            executer.RunSetConfig = runsetConfig;
            CLIHelper cliHelp = new CLIHelper();
            RunSetAutoRunConfiguration autoRunConfig = new RunSetAutoRunConfiguration(sol, executer, cliHelp);

            // Act
            cliHelp.RunAnalyzer = true;
            autoRunConfig.SelectedCLI = new CLIConfigFile();
            autoRunConfig.CreateConfigFile();
            string content = File.ReadAllText(autoRunConfig.ConfigFileFullPath);

            Assert.AreEqual(content.Contains("Solution=" + sol.Folder), true);
            Assert.AreEqual(content.Contains("RunSet=TestRunset"), true);
            Assert.AreEqual(content.Contains("Env=TestEnv"), true);
            Assert.AreEqual(content.Contains("RunAnalyzer=True"), true);
            Assert.AreEqual(content.Contains("ShowAutoRunWindow=False"), true);
        }
    }
}
