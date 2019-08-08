using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Plugin.Core;
using GingerCore.Actions.PlugIns;
using GingerCoreNET.RunLib;
using GingerCoreNETUnitTests.RunTestslib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GingerCoreNETUnitTest.RunTestslib
{
    [TestClass]
    public class RemoteGingerGridTest
    {
        static GingerGrid RemoteGingerGrid;
        static int RemoteGridPort;
        static string RemoteGridIP;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            RemoteGridIP = SocketHelper.GetLocalHostIP();
            RemoteGridPort = SocketHelper.GetOpenPort();
            // We start a Ginger grid 
            RemoteGingerGrid = new GingerGrid(RemoteGridPort);
            RemoteGingerGrid.Start();

            // Add 2 Ginger Nodes with Dummy Driver
            DummyDriver DummyDriver1 = new DummyDriver();

            Task.Factory.StartNew(() => 
            {
                GingerNodeStarter gingerNodeStarter = new GingerNodeStarter();
                gingerNodeStarter.StartNode("N1", new DummyDriver(), RemoteGridIP, RemoteGridPort);
                gingerNodeStarter.StartNode("N2", new DummyDriver(), RemoteGridIP, RemoteGridPort);
                gingerNodeStarter.Listen();
            });

            Stopwatch stopwatch = Stopwatch.StartNew();
            while (RemoteGingerGrid.NodeList.Count < 2 && stopwatch.ElapsedMilliseconds < 5000)  // max 5 seconds
            {
                Thread.Sleep(50);
            }
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            RemoteGingerGrid.Stop();
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
        public void ListGingerNodes()
        {
            //Arrange  

            //Act
            ObservableList<GingerNodeInfo> list = RemoteGingerGrid.NodeList;

            //Assert
            Assert.AreEqual(list.Count, 2);
        }

        [TestMethod]
        // [Timeout(60000)]
        public void SendActionToRemoteGrid()
        {
            // Arrange
            ActPlugIn actPlugIn = new ActPlugIn() { ServiceId = "DummyService", ActionId = "A1"};

            //Act
            ExecuteOnPlugin.RemoteGridIP = RemoteGridIP;  // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            ExecuteOnPlugin.RemoteGridPort = RemoteGridPort; // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            ExecuteOnPlugin.ExecuteActionOnRemoteGridPlugin(actPlugIn);


            //Assert
            Assert.AreEqual(RemoteGingerGrid.NodeList.Count, 2);
            Assert.AreEqual("A1 Result", actPlugIn.ExInfo);
            
        }

    }
}
