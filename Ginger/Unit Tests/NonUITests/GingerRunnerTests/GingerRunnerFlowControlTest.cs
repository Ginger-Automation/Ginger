#region License
/*
Copyright © 2014-2019 European Support Limited

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

using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Execution;
using Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.FlowControlLib;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.NonUITests.GingerRunnerTests
{
    [TestClass]
    [Level1]
    public class GingerRunnerFlowControlTest 
    {

        static BusinessFlow mBF;
        static  GingerRunner mGR;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            RepositoryItemHelper.RepositoryItemFactory = new RepositoryItemFactory();
            // Create a simple BF with simple Actions
            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF Test Flow Control";
            mBF.Active = true;
            Platform p = new Platform();
            p.PlatformType = ePlatformType.Web;            
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "App1" });

            mGR = new GingerRunner();
            Agent a = new Agent();
            a.DriverType = Agent.eDriverType.WindowsAutomation; // just a dummy driver not really for use
 
            mGR.SolutionAgents = new ObservableList<Agent>();
            mGR.SolutionAgents.Add(a);
            
            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "App1", Agent = a });
            AutoLogProxy.Init("UT Build");
            mGR.BusinessFlows.Add(mBF);
        }

        [TestMethod]  [Timeout(60000)]
        public void Simple_All_Actions_Active()
        {

            //Arrange

            ResetBusinessFlow();

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
            mGR.RunRunner();

            //Assert
           Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed);
           Assert.AreEqual(a1.Status, eRunStatus.Passed);
           Assert.AreEqual(act1.Status, eRunStatus.Passed);
           Assert.AreEqual(act2.Status, eRunStatus.Passed);
           Assert.AreEqual(act3.Status, eRunStatus.Passed);
        }

        [TestMethod]  [Timeout(60000)]
        public void Simple_One_Action_NotActive()
        {

            //Arrange
            ResetBusinessFlow();

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
            mGR.RunRunner();

            //Assert
           Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed);
           Assert.AreEqual(a1.Status, eRunStatus.Passed);
           Assert.AreEqual(act1.Status, eRunStatus.Passed);
           Assert.AreEqual(act2.Status, eRunStatus.Skipped);
           Assert.AreEqual(act3.Status, eRunStatus.Passed);
        }

        [TestMethod]  [Timeout(60000)]
        public void FlowControlTestFor_IfFailed_StopRunner()
        {
            
            //Arrange
            ResetBusinessFlow();

            Activity a1 = new Activity();
            a1.Active = true;
            a1.TargetApplication = "App1";
            mBF.Activities.Add(a1);
            
            ActDummy act1 = new ActDummy() { Description = "A1", Active = true,};
            a1.Acts.Add(act1);

            ActDummy act2 = new ActDummy() { Description = "A2", Active = false };
            a1.Acts.Add(act2);

            ActDummy act3 = new ActDummy() { Description = "A3", Active = true };
            a1.Acts.Add(act3);
            act3.FlowControls.Add(new GingerCore.FlowControlLib.FlowControl() { Condition = "1=1", FlowControlAction = eFlowControlAction.StopRun,  Active=true});
            

            ActDummy act4 = new ActDummy() { Description = "A2", Active = true };
            a1.Acts.Add(act4);
            //Act           
            mGR.ResetRunnerExecutionDetails();
            mGR.RunRunner();

            //Assert
            Assert.AreEqual(mBF.RunStatus, eRunStatus.Stopped);
            Assert.AreEqual(a1.Status, eRunStatus.Stopped);
            Assert.AreEqual(act1.Status, eRunStatus.Passed);
            Assert.AreEqual(act2.Status, eRunStatus.Skipped);
            Assert.AreEqual(act3.Status, eRunStatus.Stopped);
            Assert.AreEqual(act4.Status, eRunStatus.Pending);
        }



        private void ResetBusinessFlow()
        {
            mBF.Activities.Clear();
            mBF.RunStatus = eRunStatus.Pending;
        }



      

    }
}
