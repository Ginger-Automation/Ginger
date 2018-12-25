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

//using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
//using Amdocs.Ginger.Repository;
//using GingerCoreNET.Drivers;
//using GingerCoreNET.DriversLib;
//using GingerCoreNET.RunLib;
//using GingerCoreNETUnitTests.RunTestslib;
//using GingerPlugInsNET.ActionsLib;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Threading.Tasks;

//namespace GingerCoreNETUnitTest.RunTestslib
//{
//    [TestClass]
//    public class MultiRunTest
//    {
//        static GingerGrid GG;
//        static SolutionRepository SR;


//        [ClassInitialize]
//        public static void ClassInitialize(TestContext TestContext)
//        {
//            // We start a Ginger grid - we need to be careful with ports when running multiple Grids, Nodes

//            int GingerHubPort = SocketHelper.GetOpenPort();
//            GG = new GingerGrid(GingerHubPort);
//            GG.Start();

//            // Add 2 Ginger Nodes with Dummy Driver

//            // TODO: check how to externalize  // make it NodeInfo and drivers capabilities
//            DummyDriver DummyDriver1 = new DummyDriver();
//            DriverCapabilities DC = new DriverCapabilities();
//            DC.OS = "Windows";    //TODO: use const
//            DC.Platform = "Web";   //TODO: use const
//            GingerNode GN = new GingerNode(DC, DummyDriver1);
//            GN.StartGingerNode("N1", HubIP: SocketHelper.GetLocalHostIP(), HubPort: GingerHubPort);                        


//            DummyDriver DummyDriver2 = new DummyDriver();
//            DriverCapabilities DC2 = new DriverCapabilities();
//            DC2.OS = "Mac";
//            DC2.Platform = "Java";
//            GingerNode GingerNode2 = new GingerNode(DC2, DummyDriver2);
//            GingerNode2.StartGingerNode("N2", HubIP: SocketHelper.GetLocalHostIP(), HubPort: GingerHubPort);
//        }


//        // TODO: move this to sahres ports handling across all tests + using mutex, and verifying port is not taken
        

//        [ClassCleanup]
//        public static void ClassCleanup()
//        {
//            GG.Stop();
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
//        public void ParallelRunTest()
//        {
//            //Arrange
//            GingerAction GA1 = new GingerAction("A1");
//            GingerAction GA2 = new GingerAction("A1");
//            GingerAction GA3 = new GingerAction("A1");
//            GingerAction GA4 = new GingerAction("A1");
//            GingerAction GA5 = new GingerAction("A1");
//            GingerAction GA6 = new GingerAction("A1");
//            // Act

//            // We run on 2 drivers in parralel


//            Task t1 = Task.Factory.StartNew(() =>
//            {
//                GingerNodeInfo GNI1 = GG.NodeList[0];
//                GingerNodeProxy GNA1 = new GingerNodeProxy(GNI1);
//                GNA1.GingerGrid = GG;

//                GNA1.Reserve();
//                GNA1.StartDriver();
                
//                GNA1.RunAction(GA1);                
//                GNA1.RunAction(GA2);                
//                GNA1.RunAction(GA3);

//                GNA1.CloseDriver();
//            });

//            Task t2 = Task.Factory.StartNew(() =>
//            {
//                GingerNodeInfo GNI2 = GG.NodeList[1];
//                GingerNodeProxy GNA2 = new GingerNodeProxy(GNI2);
//                GNA2.GingerGrid = GG;

//                GNA2.Reserve();
//                GNA2.StartDriver();

//                GNA2.RunAction(GA4);
//                GNA2.RunAction(GA5);
//                GNA2.RunAction(GA6);

//                GNA2.CloseDriver();
//            });

//            t1.Wait();
//            t2.Wait();


//            // Assert
//            Assert.AreEqual(GA1.Errors, null);
//            Assert.AreEqual(GA1.ExInfo , "A1 Result");

//            Assert.AreEqual(GA2.Errors, null);
//            Assert.AreEqual(GA2.ExInfo, "A1 Result");

//            Assert.AreEqual(GA3.Errors, null);
//            Assert.AreEqual(GA3.ExInfo, "A1 Result");

//            Assert.AreEqual(GA4.Errors, null);
//            Assert.AreEqual(GA4.ExInfo, "A1 Result");

//            Assert.AreEqual(GA5.Errors, null);
//            Assert.AreEqual(GA5.ExInfo, "A1 Result");

//            Assert.AreEqual(GA6.Errors, null);
//            Assert.AreEqual(GA6.ExInfo, "A1 Result");

            
//        }

//        //[TestMethod]
//        //public void GingerMultiRunTest()
//        //{
//        // Test how gingerMulturun with BFs and run work
//        //GingersMultiRun GMR = new GingersMultiRun();
//        //GingerRunner GR1 = new GingerRunner();
//        //BusinessFlow BF1 = new BusinessFlow("BF1");
//        //DummyDriver DD1 = new DummyDriver();

//        //BF1.Activities[0].Acts.Add(GA1);
//        //GMR.Gingers.Add(GR1);


//        //GingerRunner GR2 = new GingerRunner();            
//        //GMR.Gingers.Add(GR2);
//        //}



//    }
//}
