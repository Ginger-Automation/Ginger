using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Plugin.Core;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.PlugIns;
using GingerCoreNET.Drivers.CommunicationProtocol;
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

        [Ignore]
        [TestMethod]
        // [Timeout(60000)]
        public void SendActionToRemoteGrid()
        {
            // Arrange
            ActPlugIn actPlugin = new ActPlugIn() { ServiceId = "DummyService", ActionId = "A1" };

            //Act
            GingerNodeProxy.RemoteGridIP = RemoteGridIP;  // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            GingerNodeProxy.RemoteGridPort = RemoteGridPort; // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            GingerNodeInfo gingerNodeInfo = new GingerNodeInfo() { };
            GingerNodeProxy gingerNodeProxy = new GingerNodeProxy(gingerNodeInfo, true);

            NewPayLoad actionPayLoad = ExecuteOnPlugin.CreateActionPayload(actPlugin);
            NewPayLoad actionResult = gingerNodeProxy.RunAction(actionPayLoad);
            ExecuteOnPlugin.ParseActionResult(actionResult, actPlugin);

            //Assert
            Assert.AreEqual(RemoteGingerGrid.NodeList.Count, 2);
            Assert.AreEqual("A1 Result", actPlugin.ExInfo);

        }

        [Ignore]   // use for when we know the remote grid, TODO: enhance the test to start GG on process and plugins then run the test
        [TestMethod]        
        public void SendActionToRemoteGridOnProcess()
        {            
            // TODO: start the service batch + plugin to connect, for now we use manual bat file for testing

            // Arrange
            ActPlugIn actPlugin = new ActPlugIn() { ServiceId = "DummyService", ActionId = "Sum" };
            actPlugin.GetOrCreateInputParam("a").Value = "4";
            actPlugin.GetOrCreateInputParam("a").ParamType = typeof(int);
            actPlugin.GetOrCreateInputParam("b").Value = "3";
            actPlugin.GetOrCreateInputParam("b").ParamType = typeof(int);

            //Act
            GingerNodeProxy.RemoteGridIP = RemoteGridIP;  // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            GingerNodeProxy.RemoteGridPort = 15555; // RemoteGridPort; // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            GingerNodeInfo gingerNodeInfo = new GingerNodeInfo() { };
            GingerNodeProxy gingerNodeProxy = new GingerNodeProxy(gingerNodeInfo, true);            

            NewPayLoad actionPayLoad = ExecuteOnPlugin.CreateActionPayload(actPlugin);
            for (int i = 0; i < 1000; i++)
            {
                NewPayLoad actionResult = gingerNodeProxy.RunAction(actionPayLoad);
                ExecuteOnPlugin.ParseActionResult(actionResult, actPlugin);
            }

            //Assert
            Assert.AreEqual(RemoteGingerGrid.NodeList.Count, 2);
            // Assert.AreEqual("A1 Result", actPlugin.ExInfo);
            
        }

        [Ignore]
        [TestMethod]
        public void StartAgentOnRemoteGingerGrid()
        {
            // Arrange
            Agent agent = new Agent();
            agent.AgentType = Agent.eAgentType.Service;
            agent.ServiceId = "SeleniumChromeService";
            agent.AgentOperations.StartDriver();

            // ActPlugIn actPlugin = new ActPlugIn() { ServiceId = "SeleniumChromeService", ActionId = "StartDriver" };

            //Act
            //GingerNodeProxy.RemoteGridIP = "10.122.168.174";  // cmildev174
            //GingerNodeProxy.RemoteGridPort = 15001; // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            //GingerNodeInfo gingerNodeInfo = new GingerNodeInfo() { };
            //GingerNodeProxy gingerNodeProxy = new GingerNodeProxy(gingerNodeInfo, true);
            ActBrowserElement actBrowserElement = new ActBrowserElement();
            // actBrowserElement.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowserElement.InputValues.Add(new Amdocs.Ginger.Repository.ActInputValue() { Param = nameof(ActBrowserElement.ControlAction), Value= "GotoURL", ValueForDriver = "GotoURL" });
            actBrowserElement.ValueForDriver = "http://www.facebook.com";
            // agent.RunAction(actBrowserElement);

            ExecuteOnPlugin.ExecutePlugInActionOnAgent(agent, actBrowserElement);

            //NewPayLoad actionPayLoad = ExecuteOnPlugin.CreateActionPayload(actPlugin);
            //NewPayLoad actionResult = gingerNodeProxy.RunAction(actionPayLoad);
            //ExecuteOnPlugin.ParseActionResult(actionResult, actPlugin);

            //Assert
            Assert.AreEqual(RemoteGingerGrid.NodeList.Count, 2);
            // Assert.AreEqual("A1 Result", actPlugin.ExInfo);

        }

    }
}
