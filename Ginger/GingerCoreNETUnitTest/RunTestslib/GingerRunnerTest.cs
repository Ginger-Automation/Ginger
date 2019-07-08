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
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using Amdocs.Ginger.CoreNET.Execution;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions.PlugIns;
using GingerCore.Platforms;
using GingerCoreNET.DriversLib;
using GingerCoreNET.RunLib;
using GingerCoreNETUnitTests.RunTestslib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;

namespace GingerCoreNETUnitTest.RunTestslib
{
    [TestClass]
    public class GingerRunnerTest
    {
        static DummyDriver mDummyDriver;
        static GingerGrid mGingerGrid;
        static GingerRunner mGingerRunner;

        const string cWebApp = "Web";
        static string mPluginId = "DummyPlugin";
        static  string mServiceId = "DummyService";
        static Agent agent;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;

            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());
        
            mGingerGrid = WorkSpace.Instance.LocalGingerGrid;

            // Start one DummyDriver - in process, so we can test whats going on everywhere
            mDummyDriver = new DummyDriver();            
            GingerNode GN = new GingerNode(mDummyDriver);
            GN.StartGingerNode("N1", HubIP: SocketHelper.GetLocalHostIP(), HubPort: mGingerGrid.Port);

            // Wait for the Grid to be up and the node connected
            // max 30 seconds
            Stopwatch st = Stopwatch.StartNew();            
            while (mGingerGrid.NodeList.Count == 0 && st.ElapsedMilliseconds < 30000)
            {
                Thread.Sleep(100);
            }

            if (mGingerGrid.NodeList.Count == 0)
            {
                throw new Exception(">>>>>>>>>>>>>>>> Service not started <<<<<<<<<<<<<<<<<<<<< " + mPluginId + "." + mServiceId);
            }

            WorkSpace.Instance.PlugInsManager.PluginServiceIsSeesionDictionary.Add(mPluginId + "." + mServiceId, true);
            

            //TODO: handle no GG node found

            agent = new Agent();
            agent.Name = "agent 1";
            agent.AgentType = Agent.eAgentType.Service;
            agent.PluginId = mPluginId;
            agent.ServiceId = mServiceId;

            mGingerRunner = new GingerRunner();
            mGingerRunner.ApplicationAgents.Add(new ApplicationAgent() { AppName = cWebApp, Agent = agent });

        }

        public static void ClassCleanup()
        {
            agent.Close();
            mGingerGrid.Stop();
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
        [Timeout(60000)]
        public void RunFlow()
        {
            //Arrange  
            BusinessFlow BF = new BusinessFlow("BF1");
            BF.TargetApplications.Add(new TargetApplication() { AppName = cWebApp });
            BF.Activities[0].TargetApplication = cWebApp;
            ActPlugIn a1 = new ActPlugIn() { PluginId = mPluginId, ServiceId = mServiceId, ActionId = "A1" , Active = true};            
            BF.Activities[0].Acts.Add(a1);

            //Act            
            mGingerRunner.RunBusinessFlow(BF);
            Console.WriteLine("a1.Error = " + a1.Error);  

            //Assert
            Assert.IsTrue(string.IsNullOrEmpty(a1.Error), "Action.Error=null");            
            Assert.AreEqual(eRunStatus.Passed, a1.Status, "a1.Status");
            Assert.AreEqual(eRunStatus.Passed, BF.Activities[0].Status, "Activity Status = Pass");
            
        }


    }
}
