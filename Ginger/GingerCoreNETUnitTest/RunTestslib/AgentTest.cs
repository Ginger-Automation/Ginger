//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using Amdocs.Ginger.Common;
//using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
//using Amdocs.Ginger.Repository;
//using GingerCoreNET.PlugInsLib;
//using GingerCoreNET.RunLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
//using GingerTestHelper;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading.Tasks;
//using UnitTestsCP.GeneralLib;

//namespace GingerCoreNETUnitTest.RunTestslib
//{
//    [Level3]
//    [TestClass]
//    public class AgentTest
//    {
//        static GingerGrid mGingerGrid;

//        [ClassInitialize]
//        public static void ClassInitialize(TestContext TestContext)
//        {
//            // We start a Ginger grid 
//            int HubPort = SocketHelper.GetOpenPort();
//            mGingerGrid = new GingerGrid(HubPort);
//            mGingerGrid.Start();
            
//        }

//        [ClassCleanup]
//        public static void ClassCleanup()
//        {
//            mGingerGrid.Stop();
//        }

//        [TestInitialize]
//        public void TestInitialize()
//        {


//        }

//        [TestCleanup]
//        public void TestCleanUp()
//        {

//        }



//        [TestMethod]
//        public void StartLocalDriverFromPlugin()
//        {
//            //Arrange  
            
//            string PluginPackageFolder = GetSeleniumPluginFolder();
//            PluginsManager PM = new PluginsManager();
//            PM.AddPluginPackage(PluginPackageFolder);

//            NewAgent agent = new NewAgent();            
//            agent.Name = "Chrome 1";
//            agent.PluginDriverName = "Selenium Chrome Driver";
//            agent.PluginPackageName = PluginPackageFolder; // TODO: get from driver - add annotation
//            agent.LocalGingerGrid = mGingerGrid;            
//            agent.PlugInsManager = PM;
            

//            //Act
//            agent.StartDriver();            
//            agent.CloseDriver();
            
            
//            ObservableList<GingerNodeInfo> list = mGingerGrid.NodeList;

//            //Assert
//            Assert.AreEqual(list.Count, 0);
//        }

        

//        [TestMethod]
//        public void StartX3LocalDriverFromPlugin()
//        {
//            //Arrange  

//            string PluginPackageFolder = GetSeleniumPluginFolder();
//            PluginsManager PM = new PluginsManager();
//            PM.AddPluginPackage(PluginPackageFolder);

//            NewAgent a1 = new NewAgent() { Name = "a1",  PluginDriverName = "Selenium Chrome Driver", PluginPackageName = PluginPackageFolder , LocalGingerGrid = mGingerGrid, PlugInsManager = PM };
//            NewAgent a2 = new NewAgent() { Name = "a2", PluginDriverName = "Selenium Chrome Driver", PluginPackageName = PluginPackageFolder, LocalGingerGrid = mGingerGrid, PlugInsManager = PM };
//            NewAgent a3 = new NewAgent() { Name = "a3", PluginDriverName = "Selenium Chrome Driver", PluginPackageName = PluginPackageFolder, LocalGingerGrid = mGingerGrid, PlugInsManager = PM };


//            //Act
//            a1.StartDriver();
//            a2.StartDriver();
//            a3.StartDriver();
            
//            a1.CloseDriver();   
//            a2.CloseDriver();
//            a3.CloseDriver();

//            ObservableList<GingerNodeInfo> list = mGingerGrid.NodeList;

//            //Assert
//            Assert.AreEqual(list.Count, 0);
//        }

//        [Priority(9)]
//        [TestMethod]
//        public void StartFiveLocalDriverFromPluginParallerRun()
//        {
//            //Arrange  
//            string PluginPackageFolder = GetSeleniumPluginFolder();
//            PluginsManager PM = new PluginsManager();
//            PM.AddPluginPackage(PluginPackageFolder);

//            List<NewAgent> agents = new List<NewAgent>();
//            agents.Add(new NewAgent() { Name = "a1p", PluginDriverName = "Selenium Chrome Driver", PluginPackageName = PluginPackageFolder, LocalGingerGrid = mGingerGrid, PlugInsManager = PM });
//            agents.Add(new NewAgent() { Name = "a2p", PluginDriverName = "Selenium Chrome Driver", PluginPackageName = PluginPackageFolder, LocalGingerGrid = mGingerGrid, PlugInsManager = PM });
//            agents.Add(new NewAgent() { Name = "a3p", PluginDriverName = "Selenium Chrome Driver", PluginPackageName = PluginPackageFolder, LocalGingerGrid = mGingerGrid, PlugInsManager = PM });
//            agents.Add(new NewAgent() { Name = "a4p", PluginDriverName = "Selenium Chrome Driver", PluginPackageName = PluginPackageFolder, LocalGingerGrid = mGingerGrid, PlugInsManager = PM });
//            agents.Add(new NewAgent() { Name = "a5p", PluginDriverName = "Selenium Chrome Driver", PluginPackageName = PluginPackageFolder, LocalGingerGrid = mGingerGrid, PlugInsManager = PM });

//            //TODO: check why if we do start driver inside the parallel it fails, meanwhile keep it outside
//            foreach(NewAgent a in agents)
//            {
//                a.StartDriver();
//            }


//            //Act            
//            Parallel.ForEach(agents, agent =>
//            {
//                // agent.StartDriver();  // TODO: fix me to work
//                for (int i = 0; i < 2; i++)
//                {
//                    DriverAction driverAction = new DriverAction();
//                    driverAction.ID = "GotoURL"; // nameof( IWebBrowser.Navigate);
//                    driverAction.InputValues.Add(new ActInputValue() { Param = "URL", ValueForDriver = "http://www.google.com" });
//                    agent.RunAction(driverAction);

//                    DriverAction driverAction2 = new DriverAction();
//                    driverAction2.ID = "GotoURL"; // nameof( IWebBrowser.Navigate);
//                    driverAction2.InputValues.Add(new ActInputValue() { Param = "URL", ValueForDriver = "http://www.facebook.com" });
//                    agent.RunAction(driverAction2);
//                }

//                agent.CloseDriver();
//            });
            

//            ObservableList<GingerNodeInfo> list = mGingerGrid.NodeList;

//            //Assert
//            Assert.AreEqual(list.Count, 0);
//        }

//        private string GetSeleniumPluginFolder()
//        {
//            return Path.Combine(Common.GetTestResourcesFolder(), @"PluginPackages\SeleniumPluginPackage.1.0.0");
//        }


//    }
//}
