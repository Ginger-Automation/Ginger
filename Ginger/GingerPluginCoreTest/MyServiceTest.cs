//using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
//using Amdocs.Ginger.Plugin.Core;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using PluginExample;

//namespace GingerPluginCoreTest
//{
//    [TestClass]
//    public class MyServiceTest
//    {
//        [TestMethod]
//        public void runLikeDriver()
//        {
//            //GingerSocketServer2 gingerSocketServer2 = new GingerSocketServer2();
//            //gingerSocketServer2.StartServer(15002);
//            //gingerSocketServer2.MessageHandler += srvmess;


//            //gingerSocketServer2.SendPayLoad("aaaa", "lplp");

//            //GingerNodeStarter gingerNodeStarter = new GingerNodeStarter();
//            //// gingerNodeStarter.StartNode()
            
//        }

//        [TestMethod]
//        public void StartDriver()
//        {
//            ////Arrange
//            //MyPlugin myPlugin = new MyPlugin();

//            ////Act
//            //myPlugin.StartDriver();

//            ////assert
//            //Assert.AreEqual(true, myPlugin.IsRunning , "IsRunning = true");
//        }

//        [TestMethod]
//        public void CloseDriver()
//        {
//            ////Arrange
//            //MyPlugin myPlugin = new MyPlugin();
//            //myPlugin.StartDriver();

//            ////Act
//            //myPlugin.CloseDriver();

//            ////assert
//            //Assert.AreEqual(false, myPlugin.IsRunning, "IsRunning = false");
//        }

//        [TestMethod]
//        public void RunAction()
//        {
//            //Arrange
//            MyService myService = new MyService();
//            GingerAction GA = new GingerAction();

//            //Act
//            myService.Sum(GA, 2, 3);

//            //assert
//            Assert.AreEqual(null, GA.Errors, "Errors=null");
//            //Assert.AreEqual(GA.Output["a"], "2", "a");
//            //Assert.AreEqual(GA.Output["b"], "3", "b");
//            //Assert.AreEqual(GA.Output["Total"], "5", "output Total");
//        }       
//    }
//}
