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
using Amdocs.Ginger.Run;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GingerCoreNETUnitTest.RunListeners
{

    [TestClass]
    [Level2]
    public class ExecutionListenerTest
    {
        static GingerRunner mGingerRunner;
        static ExecutionLoggerManager mExecutionLogger;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            mGingerRunner = new GingerRunner();
            mGingerRunner.Executor = new GingerExecutionEngine(mGingerRunner);

            // Add listener
            //ProjEnvironment projEnvironment = new ProjEnvironment();   // !!!!!!!!!!!!!!!!!!!!!!!remove the need for proj env
            //mExecutionLogger = new ExecutionLogger(projEnvironment, eExecutedFrom.Automation);
            //mExecutionLogger.ExecutionLogfolder = @"c:\temp\koko1";
            //mExecutionLogger.Configuration.ExecutionLoggerConfigurationIsEnabled = true; // !!!!!!!!!!!!!!!!!!!!! remove this flag            
            //mGingerRunner.RunListeners.Add(mExecutionLogger);
            mExecutionLogger = (ExecutionLoggerManager)((GingerExecutionEngine)mGingerRunner.Executor).RunListeners.FirstOrDefault(x => x.GetType() == typeof(ExecutionLoggerManager));   // !!!!!!!!!!!!!!!!
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
            BusinessFlow mBF = new BusinessFlow
            {
                Activities = [],
                Name = "BF TEst timeline events listener",
                Active = true
            };
            Activity activitiy1 = new Activity() { Active = true };
            activitiy1.Active = true;
            mBF.Activities.Add(activitiy1);

            ActDummy action1 = new ActDummy() { Description = "Dummay action 1", Active = true };
            activitiy1.Acts.Add(action1);
            mGingerRunner.Executor.BusinessFlows.Add(mBF);


            //Act
            RunListenerBase.Start();
            mGingerRunner.Executor.RunBusinessFlow(mBF);

            mExecutionLogger.ExecutionLogBusinessFlowsCounter = 1;


            //Assert




            //TimeLineEvent actionLineEvent = activityTimeLineEvent.ChildrenList[1];
            //Assert.IsTrue(actionLineEvent.Start != 0, "Action TimeLine Event.Start !=0");
            //Assert.IsTrue(actionLineEvent.End != 0, "Action TimeLine Event.End !=0");
            //Assert.AreEqual("Action", actionLineEvent.ItemType, "ItemType");
        }

    }
}
