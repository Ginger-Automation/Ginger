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
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
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
        static RunSetConfig runSetConfig = new RunSetConfig();
        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            mGingerRunner = new GingerRunner();
            mGingerRunner.Executor = new GingerExecutionEngine(mGingerRunner);

            ((GingerExecutionEngine)mGingerRunner.Executor).RunListeners.Clear(); // temp as long as GR auto start with some listener, remove when fixed
            mExecutionDumperListener = new ExecutionDumperListener(mDumpFolder);
            ((GingerExecutionEngine)mGingerRunner.Executor).RunListeners.Add(mExecutionDumperListener);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {

        }


        private void RunFlow(BusinessFlow businessFlow)
        {
            // We lock Ginger runner so we will not run 2 flows at the same time on same GR
            lock (mGingerRunner)
            {
                mGingerRunner.Executor.BusinessFlows.Clear();
                mGingerRunner.Executor.BusinessFlows.Add(businessFlow);
                mGingerRunner.Executor.CurrentBusinessFlow = businessFlow;
                mGingerRunner.Executor.RunRunner();
            }
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
            //Arrange
            BusinessFlow businessFlow = new BusinessFlow() { Name = "BF TEST Execution Dumper Listener", Active = true };

            Activity activitiy1 = new Activity() { ActivityName = "a1", Active = true };
            activitiy1.Acts.Add(new ActDummy() { Description = "Dummy action 1", Active = true });
            activitiy1.Acts.Add(new ActDummy() { Description = "Dummy action 2", Active = true });
            activitiy1.Acts.Add(new ActDummy() { Description = "Dummy action 3", Active = true });
            businessFlow.Activities.Add(activitiy1);

            Activity activitiy2 = new Activity() { ActivityName = "a2", Active = true };
            activitiy2.Acts.Add(new ActDummy() { Description = "A2 action 1", Active = true });
            businessFlow.Activities.Add(activitiy2);


            //Act            
            RunFlow(businessFlow);

            //check folder structure and files contents

            // string folder = mExecutionDumperListener.

            string BFDir = Path.Combine(mDumpFolder, "1 " + businessFlow.Name);

            //Assert
            Assert.IsTrue(Directory.Exists(BFDir), "BF directory exist");
            //TODO: all the rest             
        }



        [TestMethod]
        public void DumperListenerBigFlow()
        {
            // TODO: add more data and check speed, like variables and more          
            //Arrange
            BusinessFlow businessFlow = new BusinessFlow() { Name = "Big Flow", Active = true };

            for (int i = 0; i < 10; i++)
            {
                Activity activitiy = new Activity() { ActivityName = "activity " + i, Active = true };
                for (int j = 0; j < 10; j++)
                {
                    activitiy.Acts.Add(new ActDummy() { Description = "Dummy action " + j, Active = true });
                }
                businessFlow.Activities.Add(activitiy);
            }


            //Act
            RunFlow(businessFlow);

            //check folder structure and files contents


            string BFDir = Path.Combine(mDumpFolder, "1 " + businessFlow.Name);

            //Assert
            Assert.IsTrue(Directory.Exists(BFDir), "BF directory exist");
            //TODO: all the rest 

        }

        [Ignore] // it fails !!!!!! needs code fix to check BF is null
        [TestMethod]
        public void DumperListenerEmptyFlow()
        {
            lock (mGingerRunner)
            {
                //Arrange
                BusinessFlow businessFlow = new BusinessFlow() { Name = "Empty Flow", Active = true };

                //Act
                RunFlow(businessFlow);

                string BFDir = Path.Combine(mDumpFolder, "1 " + businessFlow.Name);

                //Assert
                Assert.IsTrue(Directory.Exists(BFDir), "BF directory exist");
            }
        }

        [TestMethod]
        public void MultiFlows()
        {
            // TODO: add several BFs to GR and run all + verify
            //lock (mGingerRunner)
            //{
            //    //Arrange
            //    BusinessFlow mBF = new BusinessFlow();
            //    mBF.Activities = new ObservableList<Activity>();
            //    mBF.Name = "Empty Flow";
            //    mBF.Active = true;

            //    //Act
            //    RunListenerBase.Start();
            //    mGingerRunner.RunBusinessFlow(mBF);

            //    string BFDir = Path.Combine(mDumpFolder, "1 " + mBF.Name);

            //    //Assert
            //    Assert.IsTrue(Directory.Exists(BFDir), "BF directory exist");
            //}
        }



    }

}

