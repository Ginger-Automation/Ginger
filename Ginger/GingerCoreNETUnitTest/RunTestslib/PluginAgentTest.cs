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
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCoreNET.RunLib;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

// FIXME to use local HTML test page

namespace WorkspaceHold
{        
    [Level3]
    [TestClass]
    public class PluginAgentTest
    {
        static TestHelper mTestHelper = new TestHelper();
        public TestContext TestContext { get; set; }

        static GingerGrid mGingerGrid;
        

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            mTestHelper.ClassInitialize(TestContext);
            

            // Create temp solution
            SolutionRepository solutionRepository;
            string path = Path.Combine(TestResources.GetTestTempFolder(@"Solutions" + Path.DirectorySeparatorChar + "AgentTestSolution"));
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            solutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
            solutionRepository.CreateRepository(path);
            WorkSpace.Instance.SolutionRepository = solutionRepository;

            // add Example4 Plugin to solution
            string pluginPath = Path.Combine(TestResources.GetTestResourcesFolder(@"Plugins" + Path.DirectorySeparatorChar + "PluginDriverExample4"));
            WorkSpace.Instance.PlugInsManager.Init(solutionRepository);
            WorkSpace.Instance.PlugInsManager.AddPluginPackage(pluginPath);

            mGingerGrid = WorkSpace.Instance.LocalGingerGrid;
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {                        
            mTestHelper.ClassCleanup();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            mTestHelper.TestInitialize(TestContext);
        }

        [TestCleanup]
        public void TestCleanUp()
        {            
            mTestHelper.TestCleanup();
        }

        
        [Ignore] // Fail on Linux
        [TestMethod]
        [Timeout(60000)]
        public void StartLocalDriverFromPlugin()
        {
            //Arrange  
            Agent agent = new Agent() { Name = "agent 1" };            
            agent.AgentType = Agent.eAgentType.Service;
            agent.PluginId = "Memo";
            agent.ServiceId = "DictionaryService";

            //Act
            mTestHelper.Log("agent.StartDriver()");
            agent.AgentOperations.StartDriver();

            int count = mGingerGrid.NodeList.Count;

            mTestHelper.Log("agent.Close();");
            agent.AgentOperations.Close();
            ObservableList<GingerNodeInfo> list = mGingerGrid.NodeList;

            //Assert
            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(1, count);
        }

        [Ignore]
        [TestMethod]
        [Timeout(60000)]
        public void StartX3LocalDriverFromPlugin()
        {
            //Arrange  
            Agent a1 = new Agent() { Name = "a1", AgentType = Agent.eAgentType.Service, PluginId = "Memo", ServiceId = "DictionaryService"};
            Agent a2 = new Agent() { Name = "a2", AgentType = Agent.eAgentType.Service, PluginId = "Memo", ServiceId = "DictionaryService" };
            Agent a3 = new Agent() { Name = "a3", AgentType = Agent.eAgentType.Service, PluginId = "Memo", ServiceId = "DictionaryService" };


            //Act
            a1.AgentOperations.StartDriver();
            a2.AgentOperations.StartDriver();
            a3.AgentOperations.StartDriver();

            int count = mGingerGrid.NodeList.Count;

            a1.AgentOperations.Close();
            a2.AgentOperations.Close();
            a3.AgentOperations.Close();

            ObservableList<GingerNodeInfo> list = mGingerGrid.NodeList;

            //Assert
            Assert.AreEqual(3, count);
            Assert.AreEqual(0, list.Count);
        }


      


        // FIXME and use when we have the selenium plugin working 
        //[Ignore]
        //[Priority(9)]
        //[TestMethod]
        //[Timeout(60000)]
        //public void StartFiveLocalDriverFromPluginParallerRun()
        //{
        //    ////Arrange  
        //    //string PluginPackageFolder = GetSeleniumPluginFolder();
        //    //PluginsManager PM = new PluginsManager();
        //    //PM.AddPluginPackage(PluginPackageFolder);

        //    //List<Agent> agents = new List<Agent>();
        //    //agents.Add(new Agent() { Name = "a1p", PluginDriverName = "Selenium Chrome Driver", PluginPackageName = PluginPackageFolder, LocalGingerGrid = mGingerGrid, PlugInsManager = PM });
        //    //agents.Add(new Agent() { Name = "a2p", PluginDriverName = "Selenium Chrome Driver", PluginPackageName = PluginPackageFolder, LocalGingerGrid = mGingerGrid, PlugInsManager = PM });
        //    //agents.Add(new Agent() { Name = "a3p", PluginDriverName = "Selenium Chrome Driver", PluginPackageName = PluginPackageFolder, LocalGingerGrid = mGingerGrid, PlugInsManager = PM });
        //    //agents.Add(new Agent() { Name = "a4p", PluginDriverName = "Selenium Chrome Driver", PluginPackageName = PluginPackageFolder, LocalGingerGrid = mGingerGrid, PlugInsManager = PM });
        //    //agents.Add(new Agent() { Name = "a5p", PluginDriverName = "Selenium Chrome Driver", PluginPackageName = PluginPackageFolder, LocalGingerGrid = mGingerGrid, PlugInsManager = PM });

        //    ////TODO: check why if we do start driver inside the parallel it fails, meanwhile keep it outside
        //    //foreach (Agent a in agents)
        //    //{
        //    //    a.StartDriver();
        //    //}


        //    ////Act            
        //    //Parallel.ForEach(agents, agent =>
        //    //{
        //    //    // agent.StartDriver();  // TODO: fix me to work
        //    //    for (int i = 0; i < 2; i++)
        //    //    {
        //    //        DriverAction driverAction = new DriverAction();
        //    //        driverAction.ID = "GotoURL"; // nameof( IWebBrowser.Navigate);
        //    //        driverAction.InputValues.Add(new ActInputValue() { Param = "URL", ValueForDriver = "http://www.google.com" });
        //    //        agent.RunAction(driverAction);

        //    //        DriverAction driverAction2 = new DriverAction();
        //    //        driverAction2.ID = "GotoURL"; // nameof( IWebBrowser.Navigate);
        //    //        driverAction2.InputValues.Add(new ActInputValue() { Param = "URL", ValueForDriver = "http://www.facebook.com" });
        //    //        agent.RunAction(driverAction2);
        //    //    }

        //    //    agent.CloseDriver();
        //    //});


        //    //ObservableList<GingerNodeInfo> list = mGingerGrid.NodeList;

        //    ////Assert
        //    //Assert.AreEqual(list.Count, 0);
        //}

        


    }
}
