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

//using Amdocs.Ginger.Repository;
//using GingerPlugInsNET.ActionsLib;
//using GingerTestHelper;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace GingerCoreNETUnitTest.PluginsLib
//{
//    [TestClass]
//    [Level1]
//    public class StandAlonePluginTest
//    {
//        [ClassInitialize]
//        public static void ClassInit(TestContext TC)
//        {

//        }

//        [ClassCleanup]
//        public static void ClassCleanup()
//        {

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
//        public void StandAloneExcelAction()
//        {
//            //Arrange            
//            PluginPackage PP = new PluginPackage();
//            // PP.Folder = TestResources.GetTestResourcesFolder(@"PluginPackages\StandAlonePluginPackage.1.0.0");            
//            PP.Folder = @"C:\Users\yaronwe\Source\Repos\GingerOfficePlugin\GingerOfficePlugin\bin\Debug\netstandard2.0";

//            PP.ScanPackage();

//            ActionHandler AH = PP.GetStandAloneActionHandler("ReadExcelCell");
//            GingerAction GA = new GingerAction("ReadExcelCell");
//            string ExcelFileName = TestResources.GetTestResourcesFile(@"Excel\Names.xlsx");
//            GA.InputParams["FileName"].Value = ExcelFileName;
//            GA.InputParams["sheetName"].Value = "Sheet1";
//            GA.InputParams["row"].Value = "#2";
//            GA.InputParams["column"].Value = "#B";            
//            AH.GingerAction = GA;

//            // Act            
//            ActionRunner.RunAction(AH.Instance, AH.GingerAction, AH);

//            //Assert
//            Assert.AreEqual("Mark", GA.Output.Values[0].ValueString);
//        }

//        [TestMethod]
//        public void ServiceExcelAction()
//        {
//            //Arrange            
//            PluginPackage PP = new PluginPackage();
//            PP.Folder = TestResources.GetTestResourcesFolder(@"PluginPackages\StandAlonePluginPackage.1.0.0");
//            // PP.Folder = @"C:\Yaron\TFS\Ginger\Devs\GingerNextVer_Dev\GingerConsole\bin\Release\PublishOutput\";
//            PP.ScanPackage();

//            ActionHandler AH = PP.GetStandAloneActionHandler("ReadExcel");
//            GingerAction GA = new GingerAction("Excel");
//            GA.InputParams["FileName"].Value = TestResources.GetTestResourcesFile(@"Excel\Names.xlsx");
//            GA.InputParams["column"].Value = "B";
//            GA.InputParams["row"].Value = 2;
//            AH.GingerAction = GA;

//            // Act            
//            ActionRunner.RunAction(AH.Instance, AH.GingerAction, AH);

//            //Assert
//            Assert.IsTrue(string.IsNullOrEmpty(GA.Errors));
//            Assert.AreEqual("Mark", GA.Output.Values[0].ValueString);
//        }


//    }
//}
