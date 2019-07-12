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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions.PlugIns;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace UnitTests.NonUITests.GingerRunnerTests
{
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

            WorkspaceHelper.CreateWorkspaceWithTempSolution("GingerRunnerPluginDriverTest", "sol1");

            // Add the plugin to solution
            string pluginFolder = TestResources.GetTestResourcesFolder(@"Plugins" + Path.DirectorySeparatorChar +  "PluginDriverExample4");
            WorkSpace.Instance.PlugInsManager.Init(WorkSpace.Instance.SolutionRepository);
            WorkSpace.Instance.PlugInsManager.AddPluginPackage(pluginFolder);

            
            Console.WriteLine("LocalGingerGrid Status: " + WorkSpace.Instance.LocalGingerGrid.Status);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {            
            WorkSpace.Instance.PlugInsManager.CloseAllRunningPluginProcesses();
            WorkspaceHelper.ReleaseWorkspace();
        }

        private void ResetBusinessFlow()
        {
            mBusinessFlow.Activities.Clear();
            mBusinessFlow.RunStatus = eRunStatus.Pending;
        }


        [TestMethod] 
        public void PluginSay()
        {
            //Arrange
            ResetBusinessFlow();            

            Activity a1 = new Activity() { Active = true, TargetApplication = mAppName };                        
            mBusinessFlow.Activities.Add(a1);

            ActPlugIn act1 = new ActPlugIn() { PluginId = "Memo", ServiceId = "SpeechService", ActionId = "Say",  Active = true, AddNewReturnParams = true };
            act1.AddOrUpdateInputParamValue("text", "hello");
            a1.Acts.Add(act1);

            //Act            
            mGingerRunner.RunRunner();

            //Assert
            Assert.AreEqual(eRunStatus.Passed, act1.Status);
            Assert.AreEqual(eRunStatus.Passed, a1.Status);            
            Assert.AreEqual(eRunStatus.Passed, mBusinessFlow.RunStatus);
            string outVal = act1.GetReturnValue("I said").Actual;
            Assert.AreEqual("hello", outVal, "outVal=hello");            
        }



        [TestMethod]  [Timeout(300000)]
        public void MemoPluginSpeedTest()
        {
            //Arrange
            ResetBusinessFlow();            

            Activity activitiy1 = new Activity() { Active = true, TargetApplication = mAppName };
            mBusinessFlow.Activities.Add(activitiy1);
            
            for (int i = 0; i < 1000; i++)
            {
                ActPlugIn act1 = new ActPlugIn() { PluginId = "Memo", ServiceId = "SpeechService", ActionId = "Say", Active = true };
                act1.AddOrUpdateInputParamValue("text", "hello " + i);
                activitiy1.Acts.Add(act1);
            }
            
            //Act
            mGingerRunner.RunRunner();


            //Assert
            Assert.AreEqual(eRunStatus.Passed, mBusinessFlow.RunStatus , "mBF.RunStatus");
            Assert.AreEqual(eRunStatus.Passed, activitiy1.Status);            
            Assert.IsTrue(activitiy1.Elapsed < 20000, "a0.Elapsed Time less than 20000ms/20sec");
        }


        


    }
}
