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

namespace WorkspaceHold
{        
    [Ignore] // Failing on Linux and Mac with timeout
    [TestClass]
    [Level1]
    public class GingerRunnerPluginDriverTest
    {
        static TestHelper mTestHelper = new TestHelper();
        public TestContext TestContext { get; set; }


        static BusinessFlow mBusinessFlow;
        static GingerRunner mGingerRunner;
        static string mAppName = "Memo app";

        [ClassInitialize()]
        public static void ClassInit(TestContext TestContext)
        {            
            mTestHelper.ClassInitialize(TestContext);
            Prep();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {                        
            WorkSpace.Instance.PlugInsManager.CloseAllRunningPluginProcesses();
            mTestHelper.ClassCleanup();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            //WorkSpace.LockWS();
            
            mTestHelper.TestInitialize(TestContext);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            //WorkSpace.RelWS();
            mTestHelper.TestCleanup();
        }

        static  void Prep()
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
            mGingerRunner.Executor.CurrentSolution = new Ginger.SolutionGeneral.Solution();

            Agent agent = new Agent();
            agent.AgentType = Agent.eAgentType.Service;

            ((GingerExecutionEngine)mGingerRunner.Executor).SolutionAgents = new ObservableList<Agent>();
            ((GingerExecutionEngine)mGingerRunner.Executor).SolutionAgents.Add(agent);

            mGingerRunner.ApplicationAgents.Add(new ApplicationAgent() { AppName = mAppName, Agent = agent });
            mGingerRunner.Executor.SolutionApplications = new ObservableList<ApplicationPlatform>();
            mGingerRunner.Executor.SolutionApplications.Add(new ApplicationPlatform() { AppName = mAppName, Platform = ePlatformType.NA });
            mGingerRunner.Executor.BusinessFlows.Add(mBusinessFlow);

            WorkspaceHelper.CreateWorkspaceWithTempSolution("sol1");

            // Add the plugin to solution
            string pluginFolder = TestResources.GetTestResourcesFolder(@"Plugins" + Path.DirectorySeparatorChar + "PluginDriverExample4");
            WorkSpace.Instance.PlugInsManager.Init(WorkSpace.Instance.SolutionRepository);
            WorkSpace.Instance.PlugInsManager.AddPluginPackage(pluginFolder);


            Console.WriteLine("LocalGingerGrid Status: " + WorkSpace.Instance.LocalGingerGrid.Status);
        }

        


        private void ResetBusinessFlow()
        {
            mTestHelper.Log("Reset Business Flow");
            mBusinessFlow.Activities.Clear();
            mBusinessFlow.RunStatus = eRunStatus.Pending;
        }


        
        [TestMethod]
        [Timeout(60000)]
        public void PluginSay()
        {
            mTestHelper.Log("test PluginSay");

            lock (mBusinessFlow)
            {
                //Arrange
                ResetBusinessFlow();

                Activity a1 = new Activity() { Active = true, TargetApplication = mAppName };
                mBusinessFlow.Activities.Add(a1);

                ActPlugIn act1 = new ActPlugIn() { PluginId = "Memo", ServiceId = "SpeechService", ActionId = "Say", Active = true, AddNewReturnParams = true };
                act1.AddOrUpdateInputParamValue("text", "hello");
                a1.Acts.Add(act1);

                //Act            
                mTestHelper.Log("Before Ginger Runner");
                mGingerRunner.Executor.RunRunner();
                // mGingerRunner.CloseAgents();
                mTestHelper.Log("After Ginger Runner");

                //Assert
                Assert.AreEqual(eRunStatus.Passed, act1.Status);
                Assert.AreEqual(eRunStatus.Passed, a1.Status);
                Assert.AreEqual(eRunStatus.Passed, mBusinessFlow.RunStatus);
                string outVal = act1.GetReturnValue("I said").Actual;
                Assert.AreEqual("hello", outVal, "outVal=hello");
            }
        }


        
        [TestMethod]
        [Timeout(60000)]
        public void MemoPluginSpeedTest()
        {
            // Reporter.ToConsole(eLogLevel.INFO, ">>>>> test MemoPluginSpeedTest <<<<<<<<<");
            Console.WriteLine(">>>>> test MemoPluginSpeedTest <<<<<<<<<");

            lock (mBusinessFlow)
            {
                //Arrange
                ResetBusinessFlow();

                Activity activitiy1 = new Activity() { Active = true, TargetApplication = mAppName };
                mBusinessFlow.Activities.Add(activitiy1);

                int count = 100;

                for (int i = 0; i < count; i++)
                {
                    ActPlugIn act1 = new ActPlugIn() { PluginId = "Memo", ServiceId = "SpeechService", ActionId = "Say", Active = true };
                    act1.AddOrUpdateInputParamValue("text", "hello " + i);
                    activitiy1.Acts.Add(act1);
                }

                //Act
                mGingerRunner.Executor.RunRunner();


                //Assert
                for (int i = 0; i < count; i++)
                {
                    Assert.AreEqual(eRunStatus.Passed, activitiy1.Acts[i].Status, "Status of Act #" + i);
                }
                Assert.AreEqual(eRunStatus.Passed, activitiy1.Status);
                Assert.IsTrue(activitiy1.Elapsed < 20000, "a0.Elapsed Time less than 20000ms/20sec");
            }
        }


      


    }
}
