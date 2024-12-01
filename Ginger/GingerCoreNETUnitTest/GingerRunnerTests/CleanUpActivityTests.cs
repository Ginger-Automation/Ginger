#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Common.Repository.BusinessFlowLib;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Environments;
using GingerCore.FlowControlLib;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GingerCoreNETUnitTest.GingerRunnerTests
{
    [TestClass]
    [Level1]
    public class CleanUpActivityTests
    {
        //static BusinessFlow BF1;
        static GingerRunner mGR;
        static ProjEnvironment environment;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {

            Platform p = new Platform
            {
                PlatformType = ePlatformType.Web
            };


            if (WorkSpace.Instance != null && WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations != null)
            {
                string TempRepositoryFolder = TestResources.GetTestTempFolder(Path.Combine("Solutions", "temp"));
                WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder = Path.Combine(TempRepositoryFolder, "ExecutionResults");
            }


            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);

            mGR.Name = "Test Runner";
            mGR.Executor.CurrentSolution = new Ginger.SolutionGeneral.Solution();

            environment = new ProjEnvironment
            {
                Name = "Default"
            };
            //BF1.Environment = environment.Name;

            Agent a = new Agent
            {
                DriverType = Agent.eDriverType.Selenium
            };
            DriverConfigParam browserTypeParam = a.GetOrCreateParam(parameter: nameof(GingerWebDriver.BrowserType), defaultValue: nameof(WebBrowserType.Chrome));
            browserTypeParam.Value = nameof(WebBrowserType.Chrome);

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = [a];

            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });
            mGR.Executor.SolutionApplications =
            [
                new ApplicationPlatform() { AppName = "SCM", Platform = ePlatformType.Web, Description = "New application" },
            ];
            //mGR.BusinessFlows.Add(BF1);


        }

        [TestMethod]
        [Timeout(60000)]
        public void CleanUpActivityShouldExecuteWhenAllActivitiesPass()
        {
            BusinessFlow businessFlow = CreateBusinessFlow();
            mGR.Executor.BusinessFlows.Add(businessFlow);

            Context context1 = new Context
            {
                BusinessFlow = businessFlow,
                Activity = businessFlow.Activities[0]
            };

            mGR.Executor.CurrentBusinessFlow = businessFlow;
            mGR.Executor.CurrentBusinessFlow.CurrentActivity = businessFlow.Activities[0];
            mGR.Executor.Context = context1;

            //Act
            mGR.Executor.RunBusinessFlow(businessFlow);

            Assert.AreEqual(eRunStatus.Passed, businessFlow.RunStatus, "Business Flow Status");
            Assert.AreEqual(eRunStatus.Passed, businessFlow.Activities[0].Status, "Activity 1 Status");
            Assert.AreEqual(eRunStatus.Passed, businessFlow.Activities[1].Status, "Activity 2 Status");
            Assert.AreEqual(eRunStatus.Passed, businessFlow.Activities[2].Status, "Activity 3 Status");
            Assert.AreEqual(eRunStatus.Passed, businessFlow.Activities[3].Status, "Clean Up Activity is executed");
        }

        [TestMethod]
        [Timeout(60000)]
        public void CleanUpActivityShouldExecuteWhenMandatoryActivityFails()
        {
            BusinessFlow businessFlow = CreateBusinessFlow();

            ActReturnValue returnValue = new ActReturnValue
            {
                Active = true,
                mExpected = "2",
                Operator = Amdocs.Ginger.Common.Expressions.eOperator.Equals
            };

            businessFlow.Activities[0].Mandatory = true;
            businessFlow.Activities[0].Acts[0].ActReturnValues.Add(returnValue);

            mGR.Executor.BusinessFlows.Add(businessFlow);

            Context context1 = new Context
            {
                BusinessFlow = businessFlow,
                Activity = businessFlow.Activities[0]
            };

            mGR.Executor.CurrentBusinessFlow = businessFlow;
            mGR.Executor.CurrentBusinessFlow.CurrentActivity = businessFlow.Activities[0];
            mGR.Executor.Context = context1;

            //Act
            mGR.Executor.RunBusinessFlow(businessFlow);

            Assert.AreEqual(eRunStatus.Failed, businessFlow.RunStatus, "Business Flow Status");
            Assert.AreEqual(eRunStatus.Failed, businessFlow.Activities[0].Status, "Mandatory Activity 1 Status should be failed");
            Assert.AreEqual(eRunStatus.Blocked, businessFlow.Activities[1].Status, "Activity 2 Status");
            Assert.AreEqual(eRunStatus.Blocked, businessFlow.Activities[2].Status, "Activity 3 Status");
            Assert.AreEqual(eRunStatus.Passed, businessFlow.Activities[3].Status, "Clean Up Activity should execute");
        }

        [TestMethod]
        [Timeout(60000)]
        public void CleanUpActivityShouldExecuteWhenNonMandatoryActivityFails()
        {
            BusinessFlow businessFlow = CreateBusinessFlow();

            ActReturnValue returnValue = new ActReturnValue
            {
                Active = true,
                mExpected = "2",
                Operator = Amdocs.Ginger.Common.Expressions.eOperator.Equals
            };


            businessFlow.Activities[1].Acts[0].ActReturnValues.Add(returnValue);

            mGR.Executor.BusinessFlows.Add(businessFlow);

            Context context1 = new Context
            {
                BusinessFlow = businessFlow,
                Activity = businessFlow.Activities[0]
            };

            mGR.Executor.CurrentBusinessFlow = businessFlow;
            mGR.Executor.CurrentBusinessFlow.CurrentActivity = businessFlow.Activities[0];
            mGR.Executor.Context = context1;

            //Act
            mGR.Executor.RunBusinessFlow(businessFlow);

            Assert.AreEqual(eRunStatus.Failed, businessFlow.RunStatus, "Business Flow Status");
            Assert.AreEqual(eRunStatus.Passed, businessFlow.Activities[0].Status, "Activity 1 Status");
            Assert.AreEqual(eRunStatus.Failed, businessFlow.Activities[1].Status, "Not mandatory Activity 2 Status should be failed");
            Assert.AreEqual(eRunStatus.Passed, businessFlow.Activities[2].Status, "Activity 3 Status");
            Assert.AreEqual(eRunStatus.Passed, businessFlow.Activities[3].Status, "Clean Up Activity should execute");
        }




        [TestMethod]
        [Timeout(60000)]
        public void CleanUpActivityShouldNOTExecuteWhenFCStopsRun()
        {
            BusinessFlow businessFlow = CreateBusinessFlow();

            FlowControl flowControl = new FlowControl
            {
                Active = true,
                FlowControlAction = eFlowControlAction.StopRun,
                Operator = eFCOperator.ActionPassed
            };

            businessFlow.Activities[2].Acts[0].FlowControls.Add(flowControl);

            mGR.Executor.BusinessFlows.Add(businessFlow);


            Context context1 = new Context
            {
                BusinessFlow = businessFlow,
                Activity = businessFlow.Activities[0]
            };

            mGR.Executor.CurrentBusinessFlow = businessFlow;
            mGR.Executor.CurrentBusinessFlow.CurrentActivity = businessFlow.Activities[0];
            mGR.Executor.Context = context1;

            //Act
            mGR.Executor.RunBusinessFlow(businessFlow);

            Assert.AreEqual(eRunStatus.Stopped, businessFlow.RunStatus, "Business Flow Status");
            Assert.AreEqual(eRunStatus.Passed, businessFlow.Activities[0].Status, "Activity 1 Status");
            Assert.AreEqual(eRunStatus.Passed, businessFlow.Activities[1].Status, "Activity 2 Status");
            Assert.AreEqual(eRunStatus.Stopped, businessFlow.Activities[2].Status, "Activity 3 Status stopped by FC");
            Assert.AreEqual(eRunStatus.Skipped, businessFlow.Activities[3].Status, "Clean Up Activity Stshould not execute");
        }

        [TestMethod]
        [Timeout(60000)]
        public void CleanUpActivityShouldWhenFCStopsBusinessFlow()
        {
            BusinessFlow businessFlow = CreateBusinessFlow();

            FlowControl flowControl = new FlowControl
            {
                Active = true,
                FlowControlAction = eFlowControlAction.StopBusinessFlow,
                Operator = eFCOperator.ActionPassed
            };

            businessFlow.Activities[2].Acts[0].FlowControls.Add(flowControl);

            mGR.Executor.BusinessFlows.Add(businessFlow);

            Context context1 = new Context
            {
                BusinessFlow = businessFlow,
                Activity = businessFlow.Activities[0]
            };

            mGR.Executor.CurrentBusinessFlow = businessFlow;
            mGR.Executor.CurrentBusinessFlow.CurrentActivity = businessFlow.Activities[0];
            mGR.Executor.Context = context1;

            //Act
            mGR.Executor.RunBusinessFlow(businessFlow);

            Assert.AreEqual(eRunStatus.Passed, businessFlow.RunStatus, "Business Flow Status");
            Assert.AreEqual(eRunStatus.Passed, businessFlow.Activities[0].Status, "Activity 1 Status");
            Assert.AreEqual(eRunStatus.Passed, businessFlow.Activities[1].Status, "Activity 2 Status");
            Assert.AreEqual(eRunStatus.Passed, businessFlow.Activities[2].Status, "Activity 3 Status");
            Assert.AreEqual(eRunStatus.Passed, businessFlow.Activities[3].Status, "Clean Up Activity should execute");
        }


        private BusinessFlow CreateBusinessFlow()
        {
            BusinessFlow BF1 = new BusinessFlow
            {
                Name = "Clean Up Activity Test",
                Active = true
            };

            ActivitiesGroup activityGroup = new ActivitiesGroup();

            Activity activity1 = new Activity
            {
                Active = true
            };

            ActDummy actDummy1 = new ActDummy
            {
                Active = true
            };
            ActDummy actDummy2 = new ActDummy
            {
                Active = true
            };

            activity1.Acts.Add(actDummy1);
            activity1.Acts.Add(actDummy2);


            Activity activity2 = new Activity
            {
                Active = true
            };

            ActDummy actDummy3 = new ActDummy
            {
                Active = true
            };
            ActDummy actDummy4 = new ActDummy
            {
                Active = true
            };


            activity2.Acts.Add(actDummy3);
            activity2.Acts.Add(actDummy4);


            Activity activity3 = new Activity
            {
                Active = true
            };

            ActDummy actDummy5 = new ActDummy
            {
                Active = true
            };
            ActDummy actDummy6 = new ActDummy
            {
                Active = true
            };

            activity3.Acts.Add(actDummy5);
            activity3.Acts.Add(actDummy6);



            CleanUpActivity cleanUpActivity = new CleanUpActivity
            {
                Active = true
            };
            ActDummy actDummy7 = new ActDummy
            {
                Active = true
            };

            cleanUpActivity.Acts.Add(actDummy7);

            activityGroup.AddActivityToGroup(activity1);
            activityGroup.AddActivityToGroup(activity2);
            activityGroup.AddActivityToGroup(activity3);
            activityGroup.AddActivityToGroup(cleanUpActivity);


            BF1.Activities.Add(activity1);
            BF1.Activities.Add(activity2);
            BF1.Activities.Add(activity3);
            BF1.Activities.Add(cleanUpActivity);

            BF1.ActivitiesGroups.Add(activityGroup);

            BF1.TargetApplications.Add(new TargetApplication() { AppName = "SCM" });
            BF1.Environment = environment.Name;

            return BF1;
        }
    }
}
