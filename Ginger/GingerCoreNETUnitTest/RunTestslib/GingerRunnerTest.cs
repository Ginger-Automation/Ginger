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

//using amdocs.ginger.GingerCoreNET;
//using GingerCoreNET.Drivers;
//using GingerCoreNET.Drivers.CommonActionsLib;
//using GingerCoreNET.DriversLib;
//using GingerCoreNET.RunLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.BusinessFlowLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
//using GingerCoreNETUnitTest.RunTestslib;
//using GingerCoreNETUnitTests.RunTestslib;
//using GingerPlugInsNET.ActionsLib;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Diagnostics;
//using System.Threading;
//using Amdocs.Ginger.CoreNET.Execution;
//using Amdocs.Ginger.CoreNET.RosLynLib;
//using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;

//namespace GingerCoreNETUnitTest.RunTestslib
//{
//    [TestClass]
//    public class GingerRunnerTest
//    {
//        static DummyDriver mDummyDriver;
//        static GingerGrid mGingerGrid;
//        static GingerRunner mGingerRunner;

//        const string cWebApp = "Web";



//        [ClassInitialize]
//        public static void ClassInitialize(TestContext TestContext)
//        {            
//            DummyWorkSpace ws = new DummyWorkSpace();
//            WorkSpace.Init(ws);
//            mGingerGrid = WorkSpace.Instance.LocalGingerGrid; 

//            // Start one DummyDriver - in process, so we can test whats going on everywhere
//            mDummyDriver = new DummyDriver();
//            DriverCapabilities DC = new DriverCapabilities();
//            DC.OS = "Windows";    //TODO: use const
//            DC.Platform = "Web";   //TODO: use const
//            GingerNode GN = new GingerNode(DC, mDummyDriver);
//            GN.StartGingerNode("N1", HubIP: SocketHelper.GetLocalHostIP(), HubPort: mGingerGrid.Port);

//            // Wait for the Grid to be up and the node connected
//            // max 10 seconds
//            Stopwatch st = new Stopwatch();
//            st.Start();
//            while(mGingerGrid.NodeList.Count == 0 && st.ElapsedMilliseconds < 10000)
//            {
//                Thread.Sleep(100);
//            }            

//            //TODO: handle no GG node found

//            NewAgent a1 = new NewAgent();
//            a1.Name = "a1";
//            a1.LocalGingerGrid = mGingerGrid;
//            a1.PlugInsManager = new GingerCoreNET.PlugInsLib.PluginsManager(); //WorkSpace.Instance.PlugInsManager;
//            a1.mGingerNodeProxy = new GingerNodeProxy(mGingerGrid.NodeList[0]);
//            a1.mGingerNodeProxy.GingerGrid = mGingerGrid;
//            a1.mGingerNodeProxy.Reserve();
//            a1.NodeID = "N1";
//            a1.StartDriver();

//            mGingerRunner = new GingerRunner();
//            mGingerRunner.ApplicationAgents.Add(new ApplicationAgent() { AppName = cWebApp, Agent = a1 });

//        }

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
//        public void RunFlow()
//        {
//            //Arrange  
//            BusinessFlow BF = new BusinessFlow("BF1");
//            BF.TargetApplications.Add(new TargetApplication() { AppName = cWebApp });
//            BF.Activities[0].TargetApplication = cWebApp;
//            DriverAction a1 = new DriverAction() {ID = "A1" };
//            BF.Activities[0].Acts.Add(a1);

//            //Act
//            mGingerRunner.CurrentBusinessFlow = BF;
//            mGingerRunner.RunFlow();

//            //Assert
//            Assert.AreEqual(a1.Status , eRunStatus.Passed);
//            Assert.AreEqual(BF.Activities[0].Status , eRunStatus.Passed ,  "Activity Status = Pass");
//            Assert.AreEqual(a1.Error, null, "Action.Error=null") ;
//        }


//    }
//}
