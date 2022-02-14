using Amdocs.Ginger.Common;
using Amdocs.Ginger.Run;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerTestHelper;
using GingerUtils.TimeLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;

namespace GingerCoreNETUnitTest.RunTestslib
{
    
    [Level2]
    [TestClass]
    public class RunnerListenerTest
    {
        static GingerRunner mGingerRunner;
        static GingerRunnerTimeLine mGingerRunnerTimeLine;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            mGingerRunner = new GingerRunner();
            mGingerRunner.Executor = new GingerExecutionEngine(mGingerRunner);

            mGingerRunnerTimeLine = new GingerRunnerTimeLine();
            ((GingerExecutionEngine)mGingerRunner.Executor).RunListeners.Add(mGingerRunnerTimeLine);
            RunListenerBase.Start();
            Thread.Sleep(10);
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
        public void TimeLineListener()
        {
            //Arrange
            BusinessFlow mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF TEst time-line events listener";
            mBF.Active = true;
            Activity activitiy1 = new Activity() { Active = true};
            activitiy1.Active = true;
            mBF.Activities.Add(activitiy1);

            ActDummy action1 = new ActDummy() { Description = "Dummy action 1", Active = true };
            activitiy1.Acts.Add(action1);
            mGingerRunner.Executor.BusinessFlows.Add(mBF);


            //Act            
            mGingerRunner.Executor.RunBusinessFlow(mBF);

            List<TimeLineEvent> events = mGingerRunnerTimeLine.timeLineEvents.EventList;


            //Assert
            Assert.AreEqual(1, events.Count, "Events count");

            TimeLineEvent businessFlowTimeLineEvent = events[0];
            Assert.IsTrue(businessFlowTimeLineEvent.Start != 0, "Business FlowTimeLine Event.Start !=0");
            Assert.IsTrue(businessFlowTimeLineEvent.End != 0, "Business FlowTimeLine Event.End !=0");
            Assert.AreEqual("BusinessFlow", businessFlowTimeLineEvent.ItemType, "ItemType");

            TimeLineEvent activityTimeLineEvent = businessFlowTimeLineEvent.ChildrenList[0];
            Assert.IsTrue(activityTimeLineEvent.Start != 0, "Activity TimeLine Event.Start !=0");
            Assert.IsTrue(activityTimeLineEvent.End != 0, "Activity TimeLine Event.End !=0");
            Assert.AreEqual("Activity", activityTimeLineEvent.ItemType, "ItemType");

            TimeLineEvent prepActionLineEvent = activityTimeLineEvent.ChildrenList[0];
            Assert.IsTrue(prepActionLineEvent.Start != 0, "PrepAction TimeLine Event.Start !=0");
            Assert.IsTrue(prepActionLineEvent.End != 0, "PrepAction TimeLine Event.End !=0");
            Assert.AreEqual("Prep Action", prepActionLineEvent.ItemType, "ItemType");

            TimeLineEvent actionLineEvent = activityTimeLineEvent.ChildrenList[1];
            Assert.IsTrue(actionLineEvent.Start != 0, "Action TimeLine Event.Start !=0");
            Assert.IsTrue(actionLineEvent.End != 0, "Action TimeLine Event.End !=0");
            Assert.AreEqual("Action", actionLineEvent.ItemType, "ItemType");


            //TODO: add more test that timeline is in boundries of parent
        }

        

    }



}
