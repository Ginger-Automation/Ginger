using Amdocs.Ginger.Common;
using Amdocs.Ginger.Run;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GingerCoreNETUnitTest.RunListeners
{
    [TestClass]
    [Level2]
    public class ExecutionListenerTest
    {
        static GingerRunner mGingerRunner;
        static ExecutionLogger mExecutionLogger;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            mGingerRunner = new GingerRunner();

            // Add listener
            //ProjEnvironment projEnvironment = new ProjEnvironment();   // !!!!!!!!!!!!!!!!!!!!!!!remove the need for proj env
            //mExecutionLogger = new ExecutionLogger(projEnvironment, eExecutedFrom.Automation);
            //mExecutionLogger.ExecutionLogfolder = @"c:\temp\koko1";
            //mExecutionLogger.Configuration.ExecutionLoggerConfigurationIsEnabled = true; // !!!!!!!!!!!!!!!!!!!!! remove this flag            
            //mGingerRunner.RunListeners.Add(mExecutionLogger);
            mExecutionLogger = (ExecutionLogger)mGingerRunner.RunListeners.Where(x=>x.GetType()==typeof(ExecutionLogger)).FirstOrDefault();   // !!!!!!!!!!!!!!!!
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


        [TestMethod]
        public void ExecutionListenerSimple()
        {
            //Arrange
            BusinessFlow mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF TEst timeline events listener";
            mBF.Active = true;
            Activity activitiy1 = new Activity() { Active = true };
            activitiy1.Active = true;
            mBF.Activities.Add(activitiy1);

            ActDummy action1 = new ActDummy() { Description = "Dummay action 1", Active = true };
            activitiy1.Acts.Add(action1);
            mGingerRunner.BusinessFlows.Add(mBF);


            //Act
            RunListenerBase.Start();
            mGingerRunner.RunBusinessFlow(mBF);

            mExecutionLogger.ExecutionLogBusinessFlowsCounter = 1;


            //Assert
            

           

            //TimeLineEvent actionLineEvent = activityTimeLineEvent.ChildrenList[1];
            //Assert.IsTrue(actionLineEvent.Start != 0, "Action TimeLine Event.Start !=0");
            //Assert.IsTrue(actionLineEvent.End != 0, "Action TimeLine Event.End !=0");
            //Assert.AreEqual("Action", actionLineEvent.ItemType, "ItemType");
        }

    }
}
