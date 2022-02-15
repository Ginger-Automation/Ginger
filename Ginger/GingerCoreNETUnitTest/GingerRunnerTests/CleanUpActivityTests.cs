using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository.BusinessFlowLib;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

            Platform p = new Platform();
            p.PlatformType = ePlatformType.Web;
    

            if (WorkSpace.Instance != null && WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations != null)
            {
                string TempRepositoryFolder = TestResources.GetTestTempFolder(Path.Combine("Solutions", "temp"));
                WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder = Path.Combine(TempRepositoryFolder, "ExecutionResults");
            }


            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);

            mGR.Name = "Test Runner";
            mGR.Executor.CurrentSolution = new Ginger.SolutionGeneral.Solution();

            environment = new ProjEnvironment();
            environment.Name = "Default";
            //BF1.Environment = environment.Name;

            Agent a = new Agent();
            a.DriverType = Agent.eDriverType.SeleniumChrome;

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = new ObservableList<Agent>();
            ((GingerExecutionEngine)mGR.Executor).SolutionAgents.Add(a);

            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });
            mGR.Executor.SolutionApplications = new ObservableList<ApplicationPlatform>();

            mGR.Executor.SolutionApplications.Add(new ApplicationPlatform() { AppName = "SCM", Platform = ePlatformType.Web, Description = "New application" });
            //mGR.BusinessFlows.Add(BF1);

      
        }

        [TestMethod]
        [Timeout(60000)]
        public void CleanUpActivityShouldExecuteWhenAllActivitiesPass()
        {
            BusinessFlow businessFlow = CreateBusinessFlow();
            mGR.Executor.BusinessFlows.Add(businessFlow);

            Context context1 = new Context();
            context1.BusinessFlow = businessFlow;
            context1.Activity = businessFlow.Activities[0];

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

            ActReturnValue returnValue = new ActReturnValue();
            returnValue.Active = true;
            returnValue.mExpected = "2";
            returnValue.Operator = Amdocs.Ginger.Common.Expressions.eOperator.Equals;

            businessFlow.Activities[0].Mandatory = true;
            businessFlow.Activities[0].Acts[0].ActReturnValues.Add(returnValue);

            mGR.Executor.BusinessFlows.Add(businessFlow);

            Context context1 = new Context();
            context1.BusinessFlow = businessFlow;
            context1.Activity = businessFlow.Activities[0];

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

            ActReturnValue returnValue = new ActReturnValue();
            returnValue.Active = true;
            returnValue.mExpected = "2";
            returnValue.Operator = Amdocs.Ginger.Common.Expressions.eOperator.Equals;


            businessFlow.Activities[1].Acts[0].ActReturnValues.Add(returnValue);

            mGR.Executor.BusinessFlows.Add(businessFlow);

            Context context1 = new Context();
            context1.BusinessFlow = businessFlow;
            context1.Activity = businessFlow.Activities[0];

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

            FlowControl flowControl = new FlowControl();
            flowControl.Active = true;
            flowControl.FlowControlAction = eFlowControlAction.StopRun;
            flowControl.Operator = eFCOperator.ActionPassed;

            businessFlow.Activities[2].Acts[0].FlowControls.Add(flowControl);

            mGR.Executor.BusinessFlows.Add(businessFlow);
          

            Context context1 = new Context();
            context1.BusinessFlow = businessFlow;
            context1.Activity = businessFlow.Activities[0];

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

            FlowControl flowControl = new FlowControl();
            flowControl.Active = true;
            flowControl.FlowControlAction = eFlowControlAction.StopBusinessFlow;
            flowControl.Operator = eFCOperator.ActionPassed;

            businessFlow.Activities[2].Acts[0].FlowControls.Add(flowControl);

            mGR.Executor.BusinessFlows.Add(businessFlow);

            Context context1 = new Context();
            context1.BusinessFlow = businessFlow;
            context1.Activity = businessFlow.Activities[0];

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
            BusinessFlow BF1 = new BusinessFlow();
            BF1.Name = "Clean Up Activity Test";
            BF1.Active = true;

            ActivitiesGroup activityGroup = new ActivitiesGroup();

            Activity activity1 = new Activity();
            activity1.Active = true;

            ActDummy actDummy1 = new ActDummy();
            actDummy1.Active = true;
            ActDummy actDummy2 = new ActDummy();
            actDummy2.Active = true;

            activity1.Acts.Add(actDummy1);
            activity1.Acts.Add(actDummy2);


            Activity activity2 = new Activity();
            activity2.Active = true;

            ActDummy actDummy3 = new ActDummy();
            actDummy3.Active = true;
            ActDummy actDummy4 = new ActDummy();
            actDummy4.Active = true;


            activity2.Acts.Add(actDummy3);
            activity2.Acts.Add(actDummy4);


            Activity activity3 = new Activity();
            activity3.Active = true;

            ActDummy actDummy5 = new ActDummy();
            actDummy5.Active = true;
            ActDummy actDummy6 = new ActDummy();
            actDummy6.Active = true;

            activity3.Acts.Add(actDummy5);
            activity3.Acts.Add(actDummy6);



            CleanUpActivity cleanUpActivity = new CleanUpActivity();
            cleanUpActivity.Active = true;
            ActDummy actDummy7 = new ActDummy();
            actDummy7.Active = true;

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
