//#region License
///*
//Copyright © 2014-2022 European Support Limited

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
//// using GingerCoreNET.CommandProcessorLib;
//using GingerCoreNET.RunLib;
//using GingerTestHelper;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace GingerConsoleUnitTest
//{
//    [TestClass]
//    public class CommandFileProcessorTest
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

//        [Level2]
//        [TestMethod]  [Timeout(60000)]
//        public void SeleniumChromeDriver()
//        {
//            ////Arrange            
//            //string agentName = "Chrome 1";
//            //string SeleniumPackageFolder = Path.Combine(Common.GetTestResourcesFolder(), @"PluginPackages\SeleniumPluginPackage.1.0.0");
//            //string script = CommandProcessor.CreateLoadPluginScript(SeleniumPackageFolder);            
//            //script += CommandProcessor.CreateStartNodeScript("Selenium Chrome Driver", agentName, SocketHelper.GetLocalHostIP(), mGingerGrid.Port);
//            //string tempfile = Path.GetTempFileName();
//            //System.IO.File.WriteAllText(tempfile, script);

//            ////Act
//            //CommandProcessor CP = new CommandProcessor();
//            //CP.RunCommand(tempfile);
//            //GingerNodeInfo GNI = (from x in mGingerGrid.NodeList where x.Name == agentName select x).FirstOrDefault();
//            //GingerNodeProxy GNA = new GingerNodeProxy(GNI);
//            //GNA.GingerGrid = mGingerGrid;
//            //GNA.Reserve();
//            //GNA.StartDriver();
//            //GingerAction GA = new GingerAction("GotoURL");
//            //GA.InputParams["URL"].Value = "http://www.google.com";
//            //GNA.RunAction(GA);
//            //GNA.CloseDriver();
//            //GNA.Shutdown();

//            ////Assert

//            //Assert.IsTrue(string.IsNullOrEmpty(GA.Errors));
//            //Assert.AreEqual(GNA.IsConnected, false);            

//        }

//        [Level1]
//        [TestMethod]  [Timeout(60000)]
//        public void FireFoxDriver()
//        {
//            ////Arrange
//            //string agentName = "FireFox 1";
//            //string SeleniumPackageFolder = Path.Combine(Common.GetTestResourcesFolder(),  @"PluginPackages\SeleniumPluginPackage.1.0.0");
//            //string script = CommandProcessor.CreateLoadPluginScript(SeleniumPackageFolder);
//            //script += CommandProcessor.CreateStartNodeScript("Selenium FireFox Driver", agentName, "127.0.0.1", mGingerGrid.Port);                        
//            //string tempfile = Path.GetTempFileName();
//            //System.IO.File.WriteAllText(tempfile, script);

//            ////Act
//            //CommandProcessor CP = new CommandProcessor();
//            //CP.RunCommand(tempfile);
//            //GingerNodeInfo GNI = (from x in mGingerGrid.NodeList where x.Name == agentName select x).FirstOrDefault();
//            //GingerNodeProxy GNA = new GingerNodeProxy(GNI);
//            //GNA.Reserve();
//            //GNA.StartDriver();
//            //GingerAction GA = new GingerAction("GotoURL");
//            //GA.InputParams["URL"].Value = "http://www.google.com";
//            //GNA.RunAction(GA);
//            //GNA.CloseDriver();
//            //GNA.Shutdown();

//            ////Assert

//            //Assert.AreEqual(GA.Errors, null);
//            //Assert.AreEqual(GNA.IsConnected, false);

//        }
//        [Level1]
//        [TestMethod]  [Timeout(60000)]
//        public void PACT()
//        {
//            ////Arrange
//            //string serviceID = "PACTService";
//            //string PACKPluginPackageFolder = @"C:\Users\yaronwe\Source\Repos\Ginger-PACT-Plugin\Ginger-PACT-Plugin\bin\Debug\netstandard2.0"; //Path.Combine(Common.GetTestResourcesFolder(), @"PluginPackages\SeleniumPluginPackage.1.0.0");
//            //string script = CommandProcessor.CreateLoadPluginScript(PACKPluginPackageFolder);
//            //script += CommandProcessor.CreateStartServiceScript(serviceID, "PACT Service", SocketHelper.GetLocalHostIP(), mGingerGrid.Port);
            
//            ////Act
//            //Task t = new Task(() => {
//            //    GingerConsoleHelper.Execute(script);            
//            //});
//            //t.Start();

//            //GingerNodeInfo GNI = null;
//            //int counter = 0;
//            //while (GNI == null && counter <30)
//            //{                
//            //    GNI = (from x in mGingerGrid.NodeList where x.Name == "PACT Service" select x).FirstOrDefault();
//            //    Thread.Sleep(1000);
//            //}
            

//            //GingerNodeProxy GNA = new GingerNodeProxy(GNI);
//            //GNA.Reserve();
//            //GNA.GingerGrid = mGingerGrid;


//            //GingerAction GA = new GingerAction("StartPACTServer");
//            //GA.InputParams["port"].Value = 1234;
//            //GNA.RunAction(GA);            
//            //GNA.Shutdown();

//            ////Assert

//            ////Assert.AreEqual(GA.Errors, null);
//            ////Assert.AreEqual(GNA.IsConnected, false);
//        }





//    }
//}
