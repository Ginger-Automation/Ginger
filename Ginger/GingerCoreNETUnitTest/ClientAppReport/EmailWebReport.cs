using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Logger;
using Amdocs.Ginger.CoreNET.Reports.ReportHelper;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore.Environments;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace GingerCoreNETUnitTest.ClientAppReport
{
    [TestClass]

    // Change class name to EmailWebReportTest
    public class EmailWebReport
    {
        #region Data Members
        static TestHelper mTestHelper = new TestHelper();
        public TestContext TestContext { get; set; }

        static string mTempFolder;
        static string mSolutionFolder;
        #endregion


        #region Events
        [ClassInitialize()]
        public static void ClassInit(TestContext TestContext)
        {
            mTestHelper.ClassInitialize(TestContext);

            mTempFolder = TestResources.GetTempFolder("CLI Tests");
            mSolutionFolder = Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "EmailWebReport");
            Reporter.WorkSpaceReporter = new UnitTestWorkspaceReporter();

            CreateWorkspace();
        }

        static void CreateWorkspace()
        {
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;
            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new DotnetCoreHelper());
            WorkSpace.Instance.OpenSolution(mSolutionFolder);
            WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod = Ginger.Reports.ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB;
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;
            RunsetExecutor runsetExecutor = new RunsetExecutor();
            runsetExecutor.RunsetExecutionEnvironment = (from x in SR.GetAllRepositoryItems<ProjEnvironment>() where x.Name == "Default" select x).SingleOrDefault();
            runsetExecutor.RunSetConfig = (from x in SR.GetAllRepositoryItems<RunSetConfig>() where x.Name == "Default Run Set" select x).SingleOrDefault();
            WorkSpace.Instance.RunsetExecutor = runsetExecutor;
            WorkSpace.Instance.RunsetExecutor.InitRunners();
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

        [Ignore]   // fail on Azure - TODO: remove ignore and check why it fails
        // Error:
        // Test method GingerCoreNETUnitTest.ClientAppReport.EmailWebReport.CLIRunSetWithSendEmailReport threw exception: 
        // System.IO.FileNotFoundException: Could not find file 'C:\Users\VssAdministrator\AppData\Roaming\amdocs\ginger\Reports\Ginger-Web-Client\assets\Execution_Data\executiondata.js'.

        [TestMethod]
        public void CLIRunSetWithSendEmailReport()
        {
            //Arrange
            // Create config file
            CLIHelper cLIHelper = new CLIHelper();
            cLIHelper.RunAnalyzer = true;
            cLIHelper.ShowAutoRunWindow = false;
            cLIHelper.DownloadUpgradeSolutionFromSourceControl = false;
            RunSetAutoRunConfiguration runSetAutoRunConfiguration = new RunSetAutoRunConfiguration(WorkSpace.Instance.Solution, WorkSpace.Instance.RunsetExecutor, cLIHelper);
            runSetAutoRunConfiguration.ConfigFileFolderPath = mTempFolder;
            runSetAutoRunConfiguration.SelectedCLI = new CLIArgs();
            CLIProcessor CLI = new CLIProcessor();

            // Act
            CLI.ExecuteArgs(new string[] { runSetAutoRunConfiguration.SelectedCLI.Verb + "=" + runSetAutoRunConfiguration.CLIContent });
            string clientAppFilePath = Path.Combine(WorkSpace.Instance.LocalUserApplicationDataFolderPath, "Reports", "Ginger-Web-Client", "assets", "Execution_Data", "executiondata.js");
            bool isFileExists = File.Exists(clientAppFilePath);
            string jsDataStr = File.ReadAllText(clientAppFilePath);

            //Artifacts
            // TODO: zip and upload the report - create it in temp folder
            // mTestHelper.AddTestArtifact("...\Report.zip" - zipFileName);

            //Assert
            Assert.IsTrue(jsDataStr.StartsWith("window.runsetData={\"GingerVersion\""), "jsDataStr.StartsWith(window.runsetData ={\"GingerVersion\"");
            Assert.AreEqual(true, isFileExists, "clientAppFilePath exist - " + clientAppFilePath);

            // TODO: verify the content of the jsDataStr
            //Assert.AreEqual(....
        }

        
        [TestMethod]
        public void CopyWebRep()
        {
            //Arrange
            WebReportGenerator webReportGenerator = new WebReportGenerator();
            // webReportGenerator.  - check method of WebReportGenerator - no need to run run set, just use LiteDB samples or json data file

            //Act


            //Assert
        }

        [TestMethod]
        public void VerifyReportHTML()
        {
            //Arrange
            // Start Chrome driver using selenium

            //Act
            // Got report url

            // Verify html elements of UI
            //Assert

        }




    }



}
