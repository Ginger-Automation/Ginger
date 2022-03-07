using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.CoreNET.Run.RunSetActions;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using Ginger.Run.RunSetActions;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GingerCoreNETUnitTest.LinuxTransformationTests
{
    [TestClass]
    public class RunSetActionGenerateTestNGReportTests
    {
        static GingerExecutionEngine mGingerRunner;
        static BusinessFlow mBF;
        static GingerExecutionEngine mGR;
        static SolutionRepository SR;
        static Solution solution;
        static ProjEnvironment environment;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            mBF = new BusinessFlow();
            mBF.Name = "BF Test";
            mBF.Active = true;

            Activity activity = new Activity();
            mBF.AddActivity(activity);

            ActDummy action1 = new ActDummy();
            ActDummy action2 = new ActDummy();

            mBF.Activities[0].Acts.Add(action1);
            mBF.Activities[0].Acts.Add(action2);

            Platform p = new Platform();
            p.PlatformType = ePlatformType.Web;
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "SCM" });

            VariableString v1 = new VariableString() { Name = "v1", InitialStringValue = "1" };
            mBF.AddVariable(v1);

            mGR = new GingerExecutionEngine(new GingerRunner());
            mGR.GingerRunner.Name = "Test Runner";
            mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();

            mGR.CurrentBusinessFlow = mBF;
            mGR.CurrentBusinessFlow.CurrentActivity = mBF.Activities[0];

            environment = new ProjEnvironment();
            environment.Name = "Default";
            mBF.Environment = environment.Name;
            mGR.GingerRunner.ProjEnvironment = environment;

            Agent a = new Agent();
            a.DriverType = Agent.eDriverType.SeleniumChrome;

            mGR.SolutionAgents = new ObservableList<Agent>();
            mGR.SolutionAgents.Add(a);
            mGR.GingerRunner.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });
            mGR.SolutionApplications = new ObservableList<ApplicationPlatform>();
            mGR.SolutionApplications.Add(new ApplicationPlatform() { AppName = "SCM", Platform = ePlatformType.Web, Description = "New application" });
            mGR.BusinessFlows.Add(mBF);
            mGR.GingerRunner.SpecificEnvironmentName = environment.Name;
            mGR.GingerRunner.UseSpecificEnvironment = false;

            string path = Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions" + Path.DirectorySeparatorChar + "BasicSimple"));
            string solutionFile = System.IO.Path.Combine(path, @"Ginger.Solution.xml");
            solution = SolutionOperations.LoadSolution(solutionFile);
            SR = GingerSolutionRepository.CreateGingerSolutionRepository();
            SR.Open(path);
            WorkSpace.Instance.Solution = solution;
            WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder = WorkSpace.Instance.Solution.LoggerConfigurations.ExecutionLoggerConfigurationExecResultsFolder;
            if (WorkSpace.Instance.Solution.SolutionOperations == null)
            {
                WorkSpace.Instance.Solution.SolutionOperations = new SolutionOperations(WorkSpace.Instance.Solution);
            }
        }
        [TestMethod]
        [Timeout(60000)]
        public void JsonOperationTest()
        {
            //Arrange
            ObservableList<BusinessFlow> bfList = SR.GetAllRepositoryItems<BusinessFlow>();
            BusinessFlow BF1 = bfList[0];
            ObservableList<Activity> activityList = BF1.Activities;
            BF1.Active = true;
            GingerExecutionEngine mGRForRunset = new GingerExecutionEngine(new GingerRunner());
            mGRForRunset.GingerRunner.Name = "Test Runner";
            Agent a = new Agent();
            a.DriverType = Agent.eDriverType.SeleniumChrome;
            mGRForRunset.SolutionAgents = new ObservableList<Agent>();
            mGRForRunset.SolutionAgents.Add(a);
            mGRForRunset.GingerRunner.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });
            mGRForRunset.SolutionApplications = new ObservableList<ApplicationPlatform>();
            mGRForRunset.SolutionApplications.Add(new ApplicationPlatform() { AppName = "SCM", Platform = ePlatformType.Web, Description = "New application" });
            mGRForRunset.BusinessFlows.Add(BF1);
            WorkSpace.Instance.SolutionRepository = SR;
            mGRForRunset.GingerRunner.SpecificEnvironmentName = environment.Name;
            mGRForRunset.GingerRunner.UseSpecificEnvironment = false;
            RunSetConfig runSetConfig1 = new RunSetConfig();
            mGRForRunset.IsUpdateBusinessFlowRunList = true;
            runSetConfig1.GingerRunners.Add(mGRForRunset.GingerRunner);
            runSetConfig1.UpdateRunnersBusinessFlowRunsList();
            runSetConfig1.mRunModeParallel = false;
            RunSetActionJSONSummary jsonAction = CreateJsonOperation();
            runSetConfig1.RunSetActions.Add(jsonAction);
            RunsetExecutor GMR1 = new RunsetExecutor();
            GMR1.RunsetExecutionEnvironment = environment;
            GMR1.RunSetConfig = runSetConfig1;
            WorkSpace.Instance.RunsetExecutor = GMR1;

            //Act
            GMR1.RunSetConfig.RunSetActions[0].runSetActionBaseOperations.ExecuteWithRunPageBFES();

            //Assert
            Assert.AreEqual(GMR1.RunSetConfig.RunSetActions[0].Status, RunSetActionBase.eRunSetActionStatus.Completed);
        }
        [TestMethod]
        [Timeout(60000)]
        public void TestNGOperationTest()
        {
            //Arrange
            ObservableList<BusinessFlow> bfList = SR.GetAllRepositoryItems<BusinessFlow>();
            BusinessFlow BF1 = bfList[0];
            ObservableList<Activity> activityList = BF1.Activities;
            BF1.Active = true;
            GingerExecutionEngine mGRForRunset = new GingerExecutionEngine(new GingerRunner());
            mGRForRunset.GingerRunner.Name = "Test Runner";
            Agent a = new Agent();
            a.DriverType = Agent.eDriverType.SeleniumChrome;
            mGRForRunset.SolutionAgents = new ObservableList<Agent>();
            mGRForRunset.SolutionAgents.Add(a);
            mGRForRunset.GingerRunner.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });
            mGRForRunset.SolutionApplications = new ObservableList<ApplicationPlatform>();
            mGRForRunset.SolutionApplications.Add(new ApplicationPlatform() { AppName = "SCM", Platform = ePlatformType.Web, Description = "New application" });
            mGRForRunset.BusinessFlows.Add(BF1);
            WorkSpace.Instance.SolutionRepository = SR;
            mGRForRunset.GingerRunner.SpecificEnvironmentName = environment.Name;
            mGRForRunset.GingerRunner.UseSpecificEnvironment = false;
            RunSetConfig runSetConfig1 = new RunSetConfig();
            mGRForRunset.IsUpdateBusinessFlowRunList = true;
            runSetConfig1.GingerRunners.Add(mGRForRunset.GingerRunner);
            runSetConfig1.UpdateRunnersBusinessFlowRunsList();
            runSetConfig1.mRunModeParallel = false;
            RunSetActionGenerateTestNGReport testNGAction = CreateTestNGOperation();
            runSetConfig1.RunSetActions.Add(testNGAction);
            RunsetExecutor GMR1 = new RunsetExecutor();
            GMR1.RunsetExecutionEnvironment = environment;
            GMR1.RunSetConfig = runSetConfig1;
            WorkSpace.Instance.RunsetExecutor = GMR1;

            //Act
            GMR1.RunSetConfig.RunSetActions[0].runSetActionBaseOperations.ExecuteWithRunPageBFES();

            //Assert
            Assert.AreEqual(GMR1.RunSetConfig.RunSetActions[0].Status, RunSetActionBase.eRunSetActionStatus.Completed);
        }
        public RunSetActionJSONSummary CreateJsonOperation()
        {
            //added JSON action
            RunSetActionJSONSummary jsonReportOperation = new RunSetActionJSONSummary();
            jsonReportOperation.Name = "Json Report";
            jsonReportOperation.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            jsonReportOperation.Condition = RunSetActionBase.eRunSetActionCondition.AlwaysRun;
            jsonReportOperation.Active = true;

            RunSetActionJSONSummaryOperations runSetActionJSONSummaryOperations = new RunSetActionJSONSummaryOperations(jsonReportOperation);
            jsonReportOperation.RunSetActionJSONSummaryOperations = runSetActionJSONSummaryOperations;

            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(jsonReportOperation);
            jsonReportOperation.runSetActionBaseOperations = runSetActionBaseOperations;
            return jsonReportOperation;
        }
        public RunSetActionGenerateTestNGReport CreateTestNGOperation()
        {
            //added JSON action
            RunSetActionGenerateTestNGReport testNGReportOperation = new RunSetActionGenerateTestNGReport();
            testNGReportOperation.Name = "TestNG Report";
            testNGReportOperation.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            testNGReportOperation.Condition = RunSetActionBase.eRunSetActionCondition.AlwaysRun;
            testNGReportOperation.Active = true;

            RunSetActionGenerateTestNGReportOperations runSetActionGenerateTestNGReportOperations = new RunSetActionGenerateTestNGReportOperations(testNGReportOperation);
            testNGReportOperation.RunSetActionGenerateTestNGReportOperations = runSetActionGenerateTestNGReportOperations;

            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(testNGReportOperation);
            testNGReportOperation.runSetActionBaseOperations = runSetActionBaseOperations;
            return testNGReportOperation;
        }
    }
}
