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
//using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
//using Amdocs.Ginger.CoreNET.GingerConsoleLib;
//using Amdocs.Ginger.CoreNET.RosLynLib;
//using GingerCoreNET.CommandProcessorLib;
//using GingerCoreNET.RunLib;
//using GingerCoreNETUnitTest.RunTestslib;
//using GingerPlugInsNET.ActionsLib;
//using GingerTestHelper;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using UnitTestsCP.GeneralLib;

//namespace GingerCoreNETUnitTests.CommandProcessorLib
//{
//    [Level3]
//    [TestClass]
//    public class CommandProcessorTest
//    {

//        static GingerGrid GG;

//        [ClassInitialize]
//        public static void ClassInitialize(TestContext TestContext)
//        {
//            // We start a Ginger grid 
//            int HubPort = SocketHelper.GetOpenPort();
//            GG = new GingerGrid(HubPort);
//            GG.Start();

//            DummyWorkSpace ws = new DummyWorkSpace();
//            WorkSpace.Init(ws);

//            //WorkSpace.Instance.PlugInsManager.AddPluginFromDLL(@"C:\Yaron\TFS\Ginger\Devs\GingerNextVer_Dev\SeleniumPlugin\bin\Debug\netstandard2.0\SeleniumPlugin.dll");
//        }

//        [ClassCleanup]
//        public static void ClassCleanup()
//        {
//            GG.Stop();
//        }


//        [TestMethod]
//        public void RunStandAloneAction()
//        {
//            //Arrange
//            GingerConsoleScriptGlobals g = new GingerConsoleScriptGlobals();
//            string Pluginfolder = Path.Combine(Common.GetTestResourcesFolder(), @"PluginPackages\StandAlonePluginPackage.1.0.0");
//            g.LoadPluginPackage(Pluginfolder);
//            g.StartService("ExcelService", "ExcelService_ID1", SocketHelper.GetLocalHostIP(), GG.Port);
//            GingerNodeInfo GNI = (from x in GG.NodeList where x.Name == "ExcelService_ID1" select x).SingleOrDefault();
//            GingerNodeProxy GNA = GG.CreateGingerNodeAgent(GNI);
//            GNA.Reserve();
//            GingerAction GA = new GingerAction("ReadExcel");
//            GA.InputParams["FileName"].Value = Common.GetTestResourcesFile(@"Excel\Names.xlsx");
//            GA.InputParams["column"].Value = "B";
//            GA.InputParams["row"].Value = 2;

//            //Act
//            GNA.RunAction(GA);

//            //assert
//            Assert.AreEqual("Mark", GA.Output.Values[0].ValueString);
//        }

//        [TestMethod]
//        public void RunStandAloneActionScript()
//        {
//            //Arrange
//            string Pluginfolder = Common.GetTestResourcesFolder(@"PluginPackages\StandAlonePluginPackage.1.0.0");

//            string script = CommandProcessor.CreateLoadPluginScript(Pluginfolder);
//            script += CommandProcessor.CreateStartServiceScript("ExcelService", "ExcelService_ID2", SocketHelper.GetLocalHostIP(), GG.Port) + Environment.NewLine;
//            GingerConsoleHelper.Execute(script);

//            Stopwatch st = Stopwatch.StartNew();
//            GingerNodeInfo GNI = null;
//            while (GNI == null && st.ElapsedMilliseconds < 10000)
//            {
//                Thread.Sleep(1000);
//                GNI = (from x in GG.NodeList where x.Name == "ExcelService_ID2" select x).SingleOrDefault();                
//            }            
            

//            GingerNodeProxy GNA = GG.CreateGingerNodeAgent(GNI);
//            GNA.Reserve();
//            GingerAction GA = new GingerAction("ReadExcel");
//            GA.InputParams["FileName"].Value = Common.GetTestResourcesFile(@"Excel\Names.xlsx");
//            GA.InputParams["column"].Value = "B";
//            GA.InputParams["row"].Value = 2;

//            //Act
//            GNA.RunAction(GA);

//            GNA.Shutdown();

//            //assert
//            Assert.AreEqual("Mark", GA.Output.Values[0].ValueString);


//            //// Arrange            
//            //CommandProcessor CP = new CommandProcessor();
//            //string f1 = Path.Combine(Common.GetTestResourcesFolder(), "CommandProcessorFiles", "StartSeleniumDriver.dat" );


//            ////Act

//            //// run on seperate task so test can continue
//            //Task t = Task.Factory.StartNew(() =>
//            //{
//            //    CP.RunCommand(f1);
//            //});

//            //int retry = 0;
//            //while (GG.NodeList.Count == 0 && retry <10)  
//            //{
//            //    Thread.Sleep(1000);
//            //    retry++;
//            //}

//            //GingerNodeAgent GNA = new GingerNodeAgent(GG.NodeList[0]);
//            //GNA.Connect();
//            //GNA.StartDriver();
//            //GNA.CloseDriver();

//            //Assert.AreEqual(GG.NodeList.Count, 1);

//        }
//}
//}
