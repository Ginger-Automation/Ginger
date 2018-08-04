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
//using Amdocs.Ginger.Repository;
//using GingerCoreNET.PlugInsLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib;
//using GingerCoreNETUnitTest.RunTestslib;
//using GingerTestHelper;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.IO;
//using UnitTestsCP.GeneralLib;

//namespace GingerCoreNETUnitTest.PluginsLib
//{
//    [TestClass]
//    [Level1]
//    public class SolutionRepositoryPluginTest
//    {

//        static SolutionRepository SR;

//        [ClassInitialize]
//        public static void ClassInitialize(TestContext TestContext)
//        {
//            DummyWorkSpace ws = new DummyWorkSpace();            
//            WorkSpace.Init(ws);

//            WorkSpace.Instance.PlugInsManager = new PluginsManager();

//            //string s = Path.Combine(Common.GetTestResourcesFolder(), @"Solutions\BasicSimple");
//            //SR = SolutionRepository.Open(s);


//            // Cre
//            string folder = Common.getGingerUnitTesterTempFolder() + @"\Solutions\PluginTest";

//            if (Directory.Exists(folder))
//            {
//                Directory.Delete(folder, true);
//            }

//            Solution.CreateNewSolution("PluginTest", folder);
//            SR = new SolutionRepository();
//            SR.Open(folder);
//            //WorkSpace.Instance.OpenSolution(folder);
//            // WorkSpace.Instance.InitPluginsManager();
//            // string SeleniumPluginDLL = Path.Combine(Common.GetTestResourcesFolder(), @"Plugins\SeleniumPluginPackage\Selenium.GingerPlugin.dll");
//            // WorkSpace.Instance.PlugInsManager.AddPluginFromDLL(SeleniumPluginDLL);
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
//        public void GetPlugins()
//        {
//            //Arrange            

//            // Act            
//            //ObservableList<PlugInWrapper> Plugins = WorkSpace.Instance.PlugInsManager.GetPlugins();

//            //Assert
//            //Assert.AreEqual(1, Plugins.Count);
//            //Assert.AreEqual("SeleniumDriverPlugIn", Plugins[0].Name);

//        }

//        //[TestMethod]
//        //public void GetSeleniumDriver()
//        //{
//        //    //Arrange            

//        //    //Act            
//        //    PlugInDriverBase d = WorkSpace.Instance.PlugInsManager.GetDriver("SeleniumDriver");
//        //    // d.StartDriver();
//        //    // d.CloseDriver();
//        //    //Assert            
//        //    Assert.AreEqual("SeleniumDriver", d.Name);

//        //}
//    }
//}
