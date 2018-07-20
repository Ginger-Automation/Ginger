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

//using Amdocs.Ginger.CoreNET.PlugInsLib;
//using GingerTestHelper;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Collections.Generic;
//using System.IO;
//using UnitTestsCP.GeneralLib;

//namespace GingerCoreNETUnitTest.PluginsLib
//{
//    [TestClass]
//    [Level1]
//    public class PluginPackageTest
//    {        

//        [ClassInitialize]
//        public static void ClassInitialize(TestContext TestContext)
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
//        public void LoadSeleniumPluginPackage1_0_0()
//        {
//            // Keep loading and testing old selenium package to make sure we didn't break backword compatibility

//            //Arrange            
//            PluginPackage PP = new PluginPackage();            
//            PP.Folder = Path.Combine(Common.GetTestResourcesFolder(), @"PluginPackages\SeleniumPluginPackage.1.0.0");


//            // Act            
//            // PP.Load();

//            List<string> d = PP.GetDrivers();
//            // ObservableList<Capability> Plugins = PP.Capabilites;
//            // PlugInDriverBase d = PP.get  (PlugInDriverBase)Plugins[0].Instance;            

//            //Assert
//            Assert.AreEqual(3, d.Count);
//            // Assert.AreEqual("Driver", Plugins[0].CapabilityType);
//            Assert.AreEqual("Selenium Chrome Driver", d[0]);
            
//        }

        
      
      






//    }
//}
