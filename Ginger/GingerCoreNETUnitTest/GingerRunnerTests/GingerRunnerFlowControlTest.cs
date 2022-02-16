#region License
/*
Copyright © 2014-2022 European Support Limited

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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.FlowControlLib;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace amdocs.ginger.GingerCoreNETTest.GingerRunnerTests
{
    [TestClass]
    [Level1]
    public class GingerRunnerFlowControlTest
    {        

        static GingerRunner mGR;
        

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            // RepositoryItemHelper.RepositoryItemFactory = new RepositoryItemFactory();                        

            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);

            Agent a = new Agent();
            a.DriverType = Agent.eDriverType.WindowsAutomation; // just a dummy driver not really for use

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = new ObservableList<Agent>();
            ((GingerExecutionEngine)mGR.Executor).SolutionAgents.Add(a);

            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "App1", Agent = a });            
        }

        [TestInitialize]
        public void TestInitialize()
        {
            
        }

        [TestCleanup]
        public void TestCleanup()
        {
            
        }


        [TestMethod]
        [Timeout(60000)]
        public void SimpleAllActionsActive()
        {
            //Arrange
            BusinessFlow mBF = CreateBusinessFlow();

            Activity a1 = new Activity();
            a1.Active = true;
            a1.TargetApplication = "App1";
            mBF.Activities.Add(a1);

            ActDummy act1 = new ActDummy() { Description = "A1", Active = true };
            a1.Acts.Add(act1);

            ActDummy act2 = new ActDummy() { Description = "A2", Active = true };
            a1.Acts.Add(act2);

            ActDummy act3 = new ActDummy() { Description = "A3", Active = true };
            a1.Acts.Add(act3);


            //Act            
            Run();

            //Assert
            Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed);
            Assert.AreEqual(a1.Status, eRunStatus.Passed);
            Assert.AreEqual(act1.Status, eRunStatus.Passed);
            Assert.AreEqual(act2.Status, eRunStatus.Passed);
            Assert.AreEqual(act3.Status, eRunStatus.Passed);
        }

        [TestMethod]
        [Timeout(60000)]
        public void SimpleOneActionNotActive()
        {

            //Arrange
            BusinessFlow mBF = CreateBusinessFlow();

            Activity a1 = new Activity();
            a1.Active = true;
            a1.TargetApplication = "App1";
            mBF.Activities.Add(a1);

            ActDummy act1 = new ActDummy() { Description = "A1", Active = true };
            a1.Acts.Add(act1);

            ActDummy act2 = new ActDummy() { Description = "A2", Active = false };
            a1.Acts.Add(act2);

            ActDummy act3 = new ActDummy() { Description = "A3", Active = true };
            a1.Acts.Add(act3);

            //Act         
            Run();

            //Assert
            Assert.AreEqual(eRunStatus.Passed, mBF.RunStatus);
            Assert.AreEqual(eRunStatus.Passed, a1.Status);
            Assert.AreEqual(eRunStatus.Passed, act1.Status);
            Assert.AreEqual(eRunStatus.Skipped, act2.Status);
            Assert.AreEqual(eRunStatus.Passed, act3.Status);
        }

        [Ignore]  // FIME check the 1=1 cond - it throws tons of exceptions to log
        [TestMethod]
        [Timeout(60000)]
        public void FlowControlTestForIfFailedStopRunner()
        {

            //Arrange
            BusinessFlow mBF = CreateBusinessFlow();

            Activity a1 = new Activity();
            a1.Active = true;
            a1.TargetApplication = "App1";
            mBF.Activities.Add(a1);

            ActDummy act1 = new ActDummy() { Description = "A1", Active = true, };
            a1.Acts.Add(act1);

            ActDummy act2 = new ActDummy() { Description = "A2", Active = false };
            a1.Acts.Add(act2);

            ActDummy act3 = new ActDummy() { Description = "A3", Active = true };
            a1.Acts.Add(act3);
            act3.FlowControls.Add(new GingerCore.FlowControlLib.FlowControl() { Condition = "1=1", FlowControlAction = eFlowControlAction.StopRun, Active = true });


            ActDummy act4 = new ActDummy() { Description = "A2", Active = true };
            a1.Acts.Add(act4);

            //Act           
            Run();                        

            //Assert
            Assert.AreEqual(eRunStatus.Stopped, mBF.RunStatus);
            Assert.AreEqual(eRunStatus.Stopped, a1.Status);
            Assert.AreEqual(eRunStatus.Passed, act1.Status);
            Assert.AreEqual(eRunStatus.Skipped, act2.Status);
            Assert.AreEqual(eRunStatus.Stopped, act3.Status);
            Assert.AreEqual(eRunStatus.Pending, act4.Status);
        }

        private void Run()
        {
            mGR.Executor.BusinessFlows[0].Reset();
            mGR.Executor.RunRunner();
        }

        private BusinessFlow CreateBusinessFlow()
        {
            BusinessFlow businessFlow = new BusinessFlow();
            mGR.Executor.BusinessFlows.Clear();
            mGR.Executor.BusinessFlows.Add(businessFlow);
            businessFlow.RunStatus = eRunStatus.Pending;

            businessFlow.Name = "BF Test Flow Control";
            businessFlow.Active = true;
            Platform p = new Platform();
            p.PlatformType = ePlatformType.Web;
            businessFlow.TargetApplications.Add(new TargetApplication() { AppName = "App1" });

            return businessFlow;
        }


    }
}
