﻿using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.WebServices;
using GingerCore.Activities;
using GingerCore.Drivers.WebServicesDriverLib;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GingerCoreNETUnitTest.GingerRunnerTests
{
    [TestClass]
    [Level1]
    public class ErrorHandlerActivityTest
    {
        static GingerRunner mGR;
        static BusinessFlow mBF;
        static WebServicesDriver mDriver;
        static Agent wsAgent = new Agent();

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {

            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();

            // Init SR
            SolutionRepository mSolutionRepository = WorkSpace.Instance.SolutionRepository;

            string TempRepositoryFolder = TestResources.GetTestTempFolder(Path.Combine("Solutions", "temp"));
            mSolutionRepository.Open(TempRepositoryFolder);
            Ginger.SolutionGeneral.Solution sol = new Ginger.SolutionGeneral.Solution();
            sol.ContainingFolderFullPath = TempRepositoryFolder;
            WorkSpace.Instance.Solution = sol;

            WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder = Path.Combine(TempRepositoryFolder, "ExecutionResults");

            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "Error Hanlder Testing";
            mBF.Active = true;


            Platform p = new Platform();
            p.PlatformType = ePlatformType.WebServices;


            mDriver = new WebServicesDriver(mBF);
            mDriver.SaveRequestXML = true;
            mDriver.SavedXMLDirectoryPath = "~\\Documents";
            mDriver.SecurityType = @"None";

            AgentOperations agentOperations = new AgentOperations(wsAgent);
            wsAgent.AgentOperations = agentOperations;

            wsAgent.DriverType = Agent.eDriverType.WebServices;
            ((AgentOperations)wsAgent.AgentOperations).Driver = mDriver;
            ApplicationAgent mAG = new ApplicationAgent();
            mAG.Agent = wsAgent;

            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = new ObservableList<Agent>();
            ((GingerExecutionEngine)mGR.Executor).SolutionAgents.Add(wsAgent);

            mGR.Executor.BusinessFlows.Add(mBF);

        }

        [TestCleanup]
        public void TestMethodCleanUP()
        {
            mBF.Activities.ClearAll();
        }

        [TestMethod]
        [Timeout(60000)]
        public void ErrorHandlerActivityShouldNotExecuteWhenAllActivitiesPass()
        {

            CreateActivityListForBusinessFlow();
            //Act
            mGR.Executor.RunBusinessFlow(mBF);

            Assert.AreEqual(eRunStatus.Passed, mBF.RunStatus, "Business Flow Status");
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[0].Status, "Activity 1 Status");
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[1].Status, "Activity 2 Status");
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[2].Status, "Activity 3 Status");
            Assert.AreEqual(eRunStatus.Skipped, mBF.Activities[3].Status);
        }

        [TestMethod]
        [Timeout(60000)]
        public void ErrorHandlerActivityShouldExecuteWhenAnyActivitiesFail()
        {
            CreateActivityListForBusinessFlow();

            Activity Activity5 = new Activity();
            Activity5.ActivityName = "Activity5";
            Activity5.Active = true;

            ActWebAPIRest restAct = new ActWebAPIRest();

            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "https://jsonplaceholder.typicode.com/posts/100");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.POST.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eContentType.JSon.ToString());

            restAct.Active = true;
            restAct.EnableRetryMechanism = false;
            restAct.ItemName = "Web API REST";
            Activity5.Acts.Add(restAct);

            Activity5.ErrorHandlerMappingType = eHandlerMappingType.AllAvailableHandlers;
            mBF.AddActivity(Activity5, null, 3);
            
            Context context1 = new Context();
            context1.BusinessFlow = mBF;
            context1.Activity = mBF.Activities[0];

            mGR.Executor.CurrentBusinessFlow = mBF;
            mGR.Executor.CurrentBusinessFlow.CurrentActivity = mBF.Activities[0];
            mGR.Executor.Context = context1;

            //Act
            mGR.Executor.RunBusinessFlow(mBF);

            Assert.AreEqual(eRunStatus.Failed, mBF.RunStatus, "Business Flow Failed");
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[0].Status, "Activity 1 Status");
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[1].Status, "Activity 2 Status");
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[2].Status, "Activity 3 Status");
            Assert.AreEqual(eRunStatus.Failed, mBF.Activities[3].Status, "Activity5 Status");
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[4].Status);
        }


        [TestMethod]
        [Timeout(60000)]
        public void ErrorHandlerActivityWithReRunActivityPostAction()
        {
            CreateActivityListForBusinessFlow();
            var Activity5 = GetActivityWithFailedActionScenario();
            ErrorHandler errorHandlerActivity1 = GetErrorHandlerActivity();
            errorHandlerActivity1.ActivityName = "Error Handler Activity With RerrunActivity PostAction";


            mBF.AddActivity(Activity5, null, 3);
            mBF.AddActivity(errorHandlerActivity1, null, 5);

            Context context1 = new Context();
            context1.BusinessFlow = mBF;
            context1.Activity = mBF.Activities[0];

            mGR.Executor.CurrentBusinessFlow = mBF;
            mGR.Executor.CurrentBusinessFlow.CurrentActivity = mBF.Activities[0];
            mGR.Executor.Context = context1;

            //Act
            mGR.Executor.RunBusinessFlow(mBF);

            Assert.AreEqual(eRunStatus.Failed, mBF.RunStatus, "Business Flow Failed");
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[0].Status, "Activity 1 Status");
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[1].Status, "Activity 2 Status");
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[2].Status, "Activity 3 Status");
            Assert.AreEqual(eRunStatus.Failed, mBF.Activities[3].Status, "Activity5 Status");
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[4].Status);
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[5].Status);

        }
        
        [TestMethod]
        [Timeout(60000)]
        public void ErrorHandlerActivityWithReRunBusinesFlowPostAction()
        {
            CreateActivityListForBusinessFlow();
            var Activity5 = GetActivityWithFailedActionScenario();
            ErrorHandler errorHandlerActivity1 = GetErrorHandlerActivity();
            errorHandlerActivity1.ActivityName = "Error Handler Activity With RerunBusiness Flow PostAction";


            mBF.AddActivity(Activity5, null, 3);
            mBF.AddActivity(errorHandlerActivity1, null, 5);

            Context context1 = new Context();
            context1.BusinessFlow = mBF;
            context1.Activity = mBF.Activities[0];

            mGR.Executor.CurrentBusinessFlow = mBF;
            mGR.Executor.CurrentBusinessFlow.CurrentActivity = mBF.Activities[0];
            mGR.Executor.Context = context1;

            //Act
            mGR.Executor.RunBusinessFlow(mBF);

            Assert.AreEqual(eRunStatus.Failed, mBF.RunStatus, "Business Flow Failed");
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[0].Status, "Activity 1 Status");
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[1].Status, "Activity 2 Status");
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[2].Status, "Activity 3 Status");
            Assert.AreEqual(eRunStatus.Failed, mBF.Activities[3].Status, "Activity5 Status");
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[4].Status);
            Assert.AreEqual(eRunStatus.Passed, mBF.Activities[5].Status);

        }

        private static ErrorHandler GetErrorHandlerActivity()
        {
            ErrorHandler errorHandlerActivity1 = new ErrorHandler();
            errorHandlerActivity1.Active = true;
            errorHandlerActivity1.ErrorHandlerMappingType = eHandlerMappingType.AllAvailableHandlers;
            errorHandlerActivity1.ErrorHandlerPostExecutionAction = eErrorHandlerPostExecutionAction.ReRunOriginAction;

            ActDummy actDummy7 = new ActDummy();
            actDummy7.Active = true;

            errorHandlerActivity1.Acts.Add(actDummy7);
            return errorHandlerActivity1;
        }

        private Activity GetActivityWithFailedActionScenario()
        {
            Activity Activity5 = new Activity();
            Activity5.ActivityName = "Activity5";
            Activity5.Active = true;

            ActWebAPIRest restAct = new ActWebAPIRest();

            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "https://jsonplaceholder.typicode.com/posts/100");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.POST.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eContentType.JSon.ToString());

            restAct.Active = true;
            restAct.EnableRetryMechanism = false;
            restAct.ItemName = "Web API REST";
            Activity5.Acts.Add(restAct);

            Activity5.ErrorHandlerMappingType = eHandlerMappingType.AllAvailableHandlers;

            return Activity5;
        }

        private void CreateActivityListForBusinessFlow()
        {

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



            ErrorHandler errorHandlerActivity = new ErrorHandler();
            errorHandlerActivity.Active = true;
            errorHandlerActivity.ActivityName = "Error Handler Activity";
            errorHandlerActivity.ErrorHandlerMappingType = eHandlerMappingType.AllAvailableHandlers;
            errorHandlerActivity.ErrorHandlerPostExecutionAction = eErrorHandlerPostExecutionAction.ReRunOriginAction;

            ActDummy actDummy7 = new ActDummy();
            actDummy7.Active = true;

            errorHandlerActivity.Acts.Add(actDummy7);

            activityGroup.AddActivityToGroup(activity1);
            activityGroup.AddActivityToGroup(activity2);
            activityGroup.AddActivityToGroup(activity3);
            activityGroup.AddActivityToGroup(errorHandlerActivity);


            mBF.Activities.Add(activity1);
            mBF.Activities.Add(activity2);
            mBF.Activities.Add(activity3);
            mBF.Activities.Add(errorHandlerActivity);

            mBF.ActivitiesGroups.Add(activityGroup);

        }
    }
}
