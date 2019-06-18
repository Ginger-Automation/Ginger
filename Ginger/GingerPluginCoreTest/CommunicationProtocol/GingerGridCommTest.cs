//using amdocs.ginger.GingerCoreNET;
//using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
//using Amdocs.Ginger.CoreNET.Run;
//using Ginger.Plugin.Platform.Web;
//using Ginger.Run;
//using GingerCore;
//using GingerCore.Actions;
//using GingerCoreNET.Drivers.CommunicationProtocol;
//using GingerCoreNET.DriversLib;
//using GingerCoreNET.RunLib;
//using GingerPluginCoreTest.CommunicationProtocol.WebPlatformServiceFakeLib;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace GingerPluginCoreTest.CommunicationProtocol
//{

//    // this tests are for checking plugin action pack to payload send via GingerNodeProxy, execute on GingerNode and service then getting response

//    [TestClass]
//    public class GingerGridCommTest
//    {
//        static GingerGrid gingerGrid;
//        static GingerNode gingerNode;
//        static IWebPlatform webPlatform;
//        static GingerNodeProxy gingerNodeProxy;
//        static Agent agent;

//        [ClassInitialize()]
//        public static void ClassInit(TestContext context)
//        {
//            int port = SocketHelper.GetOpenPort();

//            // gingerGrid = WorkSpace.Instance.LocalGingerGrid; // new GingerGrid(port); 
//            gingerGrid = new GingerGrid(port); 
//            gingerGrid.Start();

//            // WorkSpace.Instance.LocalGingerGrid = gingerGrid;

//            webPlatform = new WebPlatformServiceFake();
//            gingerNode = new GingerNode(webPlatform);
//            gingerNode.StartGingerNode("WebPlatformServiceFake 1", SocketHelper.GetLocalHostIP(), port);

//            // Wait for node to be connected.

//            gingerNodeProxy = new GingerNodeProxy(gingerGrid.NodeList[0]);
//            gingerNodeProxy.GingerGrid = gingerGrid;

//            // GingerRunner gingerRunner = new GingerRunner();
//            agent = new Agent();
//            agent.GingerNodeProxy = gingerNodeProxy;
//            agent.Platform = GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Service;
//            // agent.PluginId = "aa";
//            agent.ServiceId = "WebPlatformServiceFake";
//            agent.AgentType = Agent.eAgentType.Service;
//            agent.DriverConfiguration = new Amdocs.Ginger.Common.ObservableList<DriverConfigParam>();

//            //agent.st
//            // agent.StartDriver();
//            gingerNodeProxy.StartDriver(agent.DriverConfiguration);

//        }


//        [ClassCleanup()]
//        public static void ClassCleanup()
//        {
//            // gingerGrid.shutdown...



//        }

//        [TestMethod]
//        public void GotoURL2()
//        {
//            //Arrange
//            string url = "aaa";

//            ActBrowserElement actBrowserElement = new ActBrowserElement();
//            actBrowserElement.ControlAction = ActBrowserElement.eControlAction.GotoURL;
//            // actBrowserElement.Value = url;   // Incorrect !!! need to use ValueForDriver
//            actBrowserElement.ValueForDriver = url;

//            //Act            
//            ExecuteOnPlugin.ExecutePlugInActionOnAgent(agent, actBrowserElement);

//            //Assert            
//            Assert.IsTrue(string.IsNullOrEmpty(actBrowserElement.Error), "No Error");

//            //FIXME !!!
//            // Assert.IsTrue(string.IsNullOrEmpty(actBrowserElement.ExInfo), "Naviagted to: " + url, "ExInfo");
//        }


//        [TestMethod]
//        public void GotoURL()
//        {
//            // Arrange
//            string url = "http://www.google.com";

//            // Act
//            webPlatform.BrowserActions.Navigate(url, "???");
//            string browserurl = webPlatform.BrowserActions.GetCurrentUrl();

//            //Assert
//            Assert.AreEqual(url, browserurl, "URL of naviagte equel browser url");

//        }
//    }
//}
