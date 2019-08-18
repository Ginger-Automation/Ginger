using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Reports.ReportHelper;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore.Environments;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GingerCoreNETUnitTest.ClientAppReport
{
    [TestClass]
    public class EmailWebReport
    {
        #region Data Members
        static TestHelper mTestHelper = new TestHelper();
        public TestContext TestContext { get; set; }

        private string mTempFolder;
        private string mSolutionFolder;
        #endregion

        #region Ctor
        public EmailWebReport()
        {
            mTempFolder = TestResources.GetTempFolder("CLI Tests");
            mSolutionFolder = Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "EmailWebReport");
            Reporter.WorkSpaceReporter = new UnitTestWorkspaceReporter();
            RunFlow();
        }
        #endregion

        #region Events
        [ClassInitialize()]
        public static void ClassInit(TestContext TestContext)
        {
            mTestHelper.ClassInitialize(TestContext);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            mTestHelper.ClassCleanup();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            mTestHelper.TestInitialize(TestContext);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            mTestHelper.TestCleanup();
        }
        #endregion

        [TestMethod]
        public void TestNewReportExecData()
        {
            string clientAppFilePath = Path.Combine(WorkSpace.Instance.LocalUserApplicationDataFolderPath, "Reports", "Ginger-Web-Client", "assets", "Execution_Data", "executiondata.js");
            bool isFileExists = File.Exists(clientAppFilePath);
            string jsDataStr = string.Empty;
            if (isFileExists)
                jsDataStr = File.ReadAllText(clientAppFilePath);
            Assert.IsTrue(isFileExists && jsDataStr.StartsWith("window.runsetData={\"GingerVersion\""));
        }

        [TestMethod]
        public void TestNewReportFolderCreation()
        {
            string clientAppFolderPath = Path.Combine(WorkSpace.Instance.LocalUserApplicationDataFolderPath, "Reports", "Ginger-Web-Client");
            Assert.IsTrue(Directory.Exists(clientAppFolderPath));
        }

        private void CreateWorkspace()
        {
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;
            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new RepoCoreItem());
            WorkSpace.Instance.OpenSolution(mSolutionFolder);
            WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod = Ginger.Reports.ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB;
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;
            RunsetExecutor runsetExecutor = new RunsetExecutor();
            runsetExecutor.RunsetExecutionEnvironment = (from x in SR.GetAllRepositoryItems<ProjEnvironment>() where x.Name == "Default" select x).SingleOrDefault();
            runsetExecutor.RunSetConfig = (from x in SR.GetAllRepositoryItems<RunSetConfig>() where x.Name == "Default Run Set" select x).SingleOrDefault();
            WorkSpace.Instance.RunsetExecutor = runsetExecutor;
            WorkSpace.Instance.RunsetExecutor.InitRunners();
        }
        private void RunFlow()
        {
            CreateWorkspace();
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
        }

    }
}
