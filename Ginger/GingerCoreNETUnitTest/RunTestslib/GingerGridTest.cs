#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using Amdocs.Ginger.Plugin.Core;
using GingerCoreNET.Drivers;
using GingerCoreNET.DriversLib;
using GingerCoreNET.RunLib;
using GingerCoreNETUnitTests.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreNETUnitTest.RunTestslib
{
    [Level1]
    [TestClass]
    public class GingerGridTest
    {

        static GingerGrid GG;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            // We start a Ginger grid 
            int HubPort = SocketHelper.GetOpenPort();
            GG = new GingerGrid(HubPort);
            GG.Start();

            // Add 2 Ginger Nodes with Dummy Driver

            // TODO: check how to externalize  // make it NodeInfo and drivers capabilities
            DummyDriver DummyDriver1 = new DummyDriver();
            //DriverCapabilities DC = new DriverCapabilities();
            //DC.OS = "Windows";    //TODO: use const
            //DC.Platform = "Web";   //TODO: use const
            //GingerNode GN = new GingerNode(DC, DummyDriver1);
            //GN.StartGingerNode("N1", HubIP: SocketHelper.GetLocalHostIP(), HubPort: HubPort);
            GingerNodeStarter gingerNodeStarter = new GingerNodeStarter();
            gingerNodeStarter.StartNode("N1", new DummyDriver());

            gingerNodeStarter.StartNode("N2", new DummyDriver());

            // DummyDriver DummyDriver2 = new DummyDriver();
            //DriverCapabilities DC2 = new DriverCapabilities();
            //DC2.OS = "Mac";
            //DC2.Platform = "Java";
            //GingerNode GingerNode2 = new GingerNode(DC2, DummyDriver2);
            //GingerNode2.StartGingerNode("N2", HubIP: SocketHelper.GetLocalHostIP(), HubPort: HubPort);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            GG.Stop();
        }

        [TestInitialize]
        public void TestInitialize()
        {


        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }


        [Ignore]
        [TestMethod]
        public void ListGingerNodes()
        {
            //Arrange  

            //Act
            ObservableList<GingerNodeInfo> list = GG.NodeList;

            //Assert
            Assert.AreEqual(list.Count, 2);
        }


        // FIXME
        //[TestMethod]
        //// TODO: find how we can utilize categories
        //public void DummyDriverA1()
        //{
        //    //Arrange

        //    //Act
        //    GingerNodeProxy GNA = new GingerNodeProxy(GG.NodeList[0]);
        //    GNA.GingerGrid = GG; // temp need to have GNA work without ref to GG
        //    //Act
        //    GNA.Reserve();
        //    GNA.StartDriver();

        //    // DummyDriver DD = new DummyDriver();
        //    // GingerAction GA = new GingerAction();
        //    // NewPayLoad 

        //    // GNA.RunAction(GA);

        //    GNA.CloseDriver();

        //    //Assert
        //    // Assert.AreEqual(GA. .ExInfo, "A1 Result");

        //}


       

    }

}
