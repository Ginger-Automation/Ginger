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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web;
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
            mBF = new BusinessFlow
            {
                Name = "BF Test",
                Active = true
            };

            Activity activity = new Activity();
            mBF.AddActivity(activity);

            ActDummy action1 = new ActDummy();
            ActDummy action2 = new ActDummy();

            mBF.Activities[0].Acts.Add(action1);
            mBF.Activities[0].Acts.Add(action2);

            Platform p = new Platform
            {
                PlatformType = ePlatformType.Web
            };
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "SCM" });

            VariableString v1 = new VariableString() { Name = "v1", InitialStringValue = "1" };
            mBF.AddVariable(v1);

            mGR = new GingerExecutionEngine(new GingerRunner());
            mGR.GingerRunner.Name = "Test Runner";
            mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();

            mGR.CurrentBusinessFlow = mBF;
            mGR.CurrentBusinessFlow.CurrentActivity = mBF.Activities[0];

            environment = new ProjEnvironment
            {
                Name = "Default"
            };
            mBF.Environment = environment.Name;
            mGR.GingerRunner.ProjEnvironment = environment;

            Agent a = new Agent
            {
                DriverType = Agent.eDriverType.Selenium
            };
            DriverConfigParam browserTypeParam = a.GetOrCreateParam(parameter: nameof(GingerWebDriver.BrowserType), defaultValue: nameof(WebBrowserType.Chrome));
            browserTypeParam.Value = nameof(WebBrowserType.Chrome);

            mGR.SolutionAgents = [a];
            mGR.GingerRunner.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });
            mGR.SolutionApplications =
            [
                new ApplicationPlatform() { AppName = "SCM", Platform = ePlatformType.Web, Description = "New application" },
            ];
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
            Agent a = new Agent
            {
                DriverType = Agent.eDriverType.Selenium
            };
            mGRForRunset.SolutionAgents = [a];
            mGRForRunset.GingerRunner.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });
            mGRForRunset.SolutionApplications =
            [
                new ApplicationPlatform() { AppName = "SCM", Platform = ePlatformType.Web, Description = "New application" },
            ];
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
            RunsetExecutor GMR1 = new RunsetExecutor
            {
                RunsetExecutionEnvironment = environment,
                RunSetConfig = runSetConfig1
            };
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
            Agent a = new Agent
            {
                DriverType = Agent.eDriverType.Selenium
            };
            mGRForRunset.SolutionAgents = [a];
            mGRForRunset.GingerRunner.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });
            mGRForRunset.SolutionApplications =
            [
                new ApplicationPlatform() { AppName = "SCM", Platform = ePlatformType.Web, Description = "New application" },
            ];
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
            RunsetExecutor GMR1 = new RunsetExecutor
            {
                RunsetExecutionEnvironment = environment,
                RunSetConfig = runSetConfig1
            };
            WorkSpace.Instance.RunsetExecutor = GMR1;

            //Act
            GMR1.RunSetConfig.RunSetActions[0].runSetActionBaseOperations.ExecuteWithRunPageBFES();

            //Assert
            Assert.AreEqual(GMR1.RunSetConfig.RunSetActions[0].Status, RunSetActionBase.eRunSetActionStatus.Completed);
        }
        public RunSetActionJSONSummary CreateJsonOperation()
        {
            //added JSON action
            RunSetActionJSONSummary jsonReportOperation = new RunSetActionJSONSummary
            {
                Name = "Json Report",
                RunAt = RunSetActionBase.eRunAt.ExecutionEnd,
                Condition = RunSetActionBase.eRunSetActionCondition.AlwaysRun,
                Active = true
            };

            RunSetActionJSONSummaryOperations runSetActionJSONSummaryOperations = new RunSetActionJSONSummaryOperations(jsonReportOperation);
            jsonReportOperation.RunSetActionJSONSummaryOperations = runSetActionJSONSummaryOperations;

            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(jsonReportOperation);
            jsonReportOperation.runSetActionBaseOperations = runSetActionBaseOperations;
            return jsonReportOperation;
        }
        public RunSetActionGenerateTestNGReport CreateTestNGOperation()
        {
            //added JSON action
            RunSetActionGenerateTestNGReport testNGReportOperation = new RunSetActionGenerateTestNGReport
            {
                Name = "TestNG Report",
                RunAt = RunSetActionBase.eRunAt.ExecutionEnd,
                Condition = RunSetActionBase.eRunSetActionCondition.AlwaysRun,
                Active = true
            };

            RunSetActionGenerateTestNGReportOperations runSetActionGenerateTestNGReportOperations = new RunSetActionGenerateTestNGReportOperations(testNGReportOperation);
            testNGReportOperation.RunSetActionGenerateTestNGReportOperations = runSetActionGenerateTestNGReportOperations;

            RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(testNGReportOperation);
            testNGReportOperation.runSetActionBaseOperations = runSetActionBaseOperations;
            return testNGReportOperation;
        }
    }
}
