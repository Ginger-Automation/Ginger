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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using Amdocs.Ginger.Plugin.Core;
using GingerCoreNET.RunLib;
using GingerCoreNETUnitTests.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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
            DummyDriver DummyDriver1 = new DummyDriver();

            Task.Factory.StartNew(() => {
                GingerNodeStarter gingerNodeStarter = new GingerNodeStarter();
                gingerNodeStarter.StartNode("N1", new DummyDriver(), SocketHelper.GetLocalHostIP(), HubPort);
                gingerNodeStarter.StartNode("N2", new DummyDriver(), SocketHelper.GetLocalHostIP(), HubPort);
                gingerNodeStarter.Listen();
            });

            Stopwatch stopwatch = Stopwatch.StartNew();
            while (GG.NodeList.Count < 2 && stopwatch.ElapsedMilliseconds < 5000)  // max 5 seconds
            {
                Thread.Sleep(50);
            }            
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


        
        [TestMethod]  [Timeout(60000)]
        public void ListGingerNodes()
        {
            //Arrange  

            //Act
            ObservableList<GingerNodeInfo> list = GG.NodeList;

            //Assert
            Assert.AreEqual(list.Count, 2);
        }


        // FIXME
        //[TestMethod]  [Timeout(60000)]
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
