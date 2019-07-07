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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions.PlugIns;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace UnitTests.NonUITests.GingerRunnerTests
{
    [Ignore]
    [TestClass]
    [Level1]
    public class GingerRunnerPluginDriverTest
    {
        static BusinessFlow mBusinessFlow;
        static GingerRunner mGingerRunner;
        static string mAppName = "Memo app";

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            AutoLogProxy.Init("Unit Tests");
            
            // Create new solution
            mBusinessFlow = new BusinessFlow();
            mBusinessFlow.Activities = new ObservableList<Activity>();
            mBusinessFlow.Name = "MyDriver BF";
            mBusinessFlow.Active = true;
            Platform p = new Platform();
            p.PlatformType = ePlatformType.NA;             
            mBusinessFlow.TargetApplications.Add(new TargetApplication() { AppName = mAppName });

            mGingerRunner = new GingerRunner();
            mGingerRunner.CurrentSolution = new Ginger.SolutionGeneral.Solution();

            Agent agent = new Agent();
            agent.AgentType = Agent.eAgentType.Service;

            mGingerRunner.SolutionAgents = new ObservableList<Agent>();
            mGingerRunner.SolutionAgents.Add(agent);

            mGingerRunner.ApplicationAgents.Add(new ApplicationAgent() { AppName = mAppName, Agent = agent });
            mGingerRunner.SolutionApplications = new ObservableList<ApplicationPlatform>();
            mGingerRunner.SolutionApplications.Add(new ApplicationPlatform() { AppName = mAppName, Platform = ePlatformType.NA });
            mGingerRunner.BusinessFlows.Add(mBusinessFlow);


            // Add the plugin to solution
            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();

            string solutionfolder = TestResources.GetTestTempFolder("sol1");
            if (Directory.Exists(solutionfolder))
            {
                Directory.Delete(solutionfolder,true);
            }            
            WorkSpace.Instance.SolutionRepository.CreateRepository(solutionfolder);
            WorkSpace.Instance.SolutionRepository.Open(solutionfolder);

            string pluginFolder = TestResources.GetTestResourcesFolder(@"Plugins\PluginDriverExample4");
            WorkSpace.Instance.PlugInsManager.AddPluginPackage(pluginFolder); 
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // mGingerRunner.StopAgents();
        }


        [TestMethod]  [Timeout(60000)]
        public void PluginSay()
        {
            //Arrange
            ResetBusinessFlow();
            WorkSpace.Instance.LocalGingerGrid.NodeList.Clear();

            Activity a1 = new Activity() { Active = true, TargetApplication = mAppName };                        
            mBusinessFlow.Activities.Add(a1);

            ActPlugIn act1 = new ActPlugIn() { PluginId = "Memo", ServiceId = "MemoService", ActionId = "Say",  Active = true };
            act1.AddOrUpdateInputParamValue("text", "hello");
            a1.Acts.Add(act1);

            //Act            
            mGingerRunner.RunRunner();
            string outVal = act1.GetReturnValue("I said").Actual;

            //Assert
            Assert.AreEqual("hello", outVal, "outVal=hello");
            Assert.AreEqual(eRunStatus.Passed, mBusinessFlow.RunStatus);
            Assert.AreEqual(eRunStatus.Passed, a1.Status);            
        }



        [TestMethod]  [Timeout(60000)]
        public void SpeedTest()
        {
            //Arrange
            ResetBusinessFlow();
            WorkSpace.Instance.LocalGingerGrid.NodeList.Clear();

            Activity activitiy1 = new Activity() { Active = true, TargetApplication = mAppName };
            mBusinessFlow.Activities.Add(activitiy1);
            
            for (int i = 0; i < 1000; i++)
            {
                ActPlugIn act1 = new ActPlugIn() { PluginId = "Memo", ServiceId = "MemoService", ActionId = "Say", Active = true };
                act1.AddOrUpdateInputParamValue("text", "hello " + i);
                activitiy1.Acts.Add(act1);
            }
            
            //Act
            mGingerRunner.RunRunner();


            //Assert
            Assert.AreEqual(mBusinessFlow.RunStatus, eRunStatus.Passed, "mBF.RunStatus");
            Assert.AreEqual(eRunStatus.Passed, activitiy1.Status);            
            Assert.IsTrue(activitiy1.Elapsed < 20000, "a0.Elapsed Time less than 20000ms/10sec");
        }


        private void ResetBusinessFlow()
        {
            mBusinessFlow.Activities.Clear();
            mBusinessFlow.RunStatus = eRunStatus.Pending;
        }


    }
}
