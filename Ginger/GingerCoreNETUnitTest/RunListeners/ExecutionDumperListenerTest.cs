using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Amdocs.Ginger.Run;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GingerCoreNETUnitTest.RunTestslib
{
    [Level2]
    [TestClass]
    public class ExecutionDumperListenerTest
    {
        static GingerRunner mGingerRunner;
        static ExecutionDumperListener mExecutionDumperListener;
        static string mDumpFolder = TestResources.GetTempFolder("ExecutionDumperListener");

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            mGingerRunner = new GingerRunner();            
            mExecutionDumperListener = new ExecutionDumperListener(mDumpFolder);
            mGingerRunner.RunListeners.Add(mExecutionDumperListener);
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
        public void DumperListener()
        {
            
            // We lock Ginger runner so we will not run 2 flows at the same time on same GR
            lock (mGingerRunner)
            {
                //Arrange
                BusinessFlow mBF = new BusinessFlow();
                mBF.Activities = new ObservableList<Activity>();
                mBF.Name = "BF TEST Execution Dumper Listener";
                mBF.Active = true;

                Activity activitiy1 = new Activity() { ActivityName = "a1", Active = true };
                activitiy1.Acts.Add(new ActDummy() { Description = "Dummy action 1", Active = true });
                activitiy1.Acts.Add(new ActDummy() { Description = "Dummy action 2", Active = true });
                activitiy1.Acts.Add(new ActDummy() { Description = "Dummy action 3", Active = true });
                mBF.Activities.Add(activitiy1);

                Activity activitiy2 = new Activity() { ActivityName = "a2", Active = true };
                activitiy2.Acts.Add(new ActDummy() { Description = "A2 action 1", Active = true });
                mBF.Activities.Add(activitiy2);

                mGingerRunner.BusinessFlows.Add(mBF);

                //Act
                RunListenerBase.Start();
                mGingerRunner.RunBusinessFlow(mBF);


                //check folder structure and files contents

                // string folder = mExecutionDumperListener.

                string BFDir = Path.Combine(mDumpFolder, "1 " + mBF.Name);

                //Assert


                Assert.IsTrue(Directory.Exists(BFDir), "BF directory exist");
                //TODO: all the rest 
            }



        }


        [TestMethod]
        public void DumperListenerBigFlow()
        {
            // TODO: add more data and check speed
            // We lock Ginger runner so we will not run 2 flows at the same time on same GR
            lock (mGingerRunner)
            {
                //Arrange
                BusinessFlow mBF = new BusinessFlow();
                mBF.Activities = new ObservableList<Activity>();
                mBF.Name = "Big Flow";
                mBF.Active = true;

                for(int i=0;i<10;i++)
                {
                    Activity activitiy = new Activity() { ActivityName = "activity " + i, Active = true };
                    for (int j=0;j<10;j++)
                    {
                        activitiy.Acts.Add(new ActDummy() { Description = "Dummy action " + j, Active = true });
                    }
                    mBF.Activities.Add(activitiy);
                }
                
                mGingerRunner.BusinessFlows.Add(mBF);

                //Act
                RunListenerBase.Start();
                mGingerRunner.RunBusinessFlow(mBF);


                //check folder structure and files contents

                // string folder = mExecutionDumperListener.

                string BFDir = Path.Combine(mDumpFolder, "1 " + mBF.Name);

                //Assert


                Assert.IsTrue(Directory.Exists(BFDir), "BF directory exist");
                //TODO: all the rest 
            }



        }


    }

}

