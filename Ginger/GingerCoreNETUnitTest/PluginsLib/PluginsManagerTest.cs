#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Amdocs.Ginger.Repository;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreNETUnitTest.PluginsLib
{
    [TestClass]
    [Level1]
    public class PluginsManagerTest
    {

        

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            
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
        public void AddPluginPackage()
        {
            // Arrange
            PluginsManager mPlugInsManager = new PluginsManager();
            string folder = TestResources.GetTestResourcesFolder(@"PluginPackages\GingerOfficePlugin");

            //Act
            mPlugInsManager.AddPluginPackage(folder);
            ObservableList<StandAloneAction> list = mPlugInsManager.GetStandAloneActions();

            //Assert
            Assert.AreEqual(5, list.Count, "There are 5 stand alone actions");
        }
        [TestMethod]
        public void GetInstalledPluginPackages()
        {
            //Arrange       
            //string folder = Path.Combine(Common.GetTestResourcesFolder(), @"PluginPackages\SeleniumPluginPackage.1.0.0");

            // Act            
            //mPlugInsManager.AddPluginPackage(folder);
            //int count = mPlugInsManager.GetInstalledPluginPackages().Count;

            //Assert
            //Assert.AreEqual(1, count);
            // Assert.AreEqual("Selenium Driver", list[0].Name );
            //Assert.AreEqual("SeleniumDriver", d.Name);

        }

        [TestMethod]
        public void GenstalledPluginPackages()
        {
            //Arrange       
            //string folder = Path.Combine(Common.GetTestResourcesFolder(), @"PluginPackages\SeleniumPluginPackage.1.0.0");

            // Act            
            //mPlugInsManager.AddPluginPackage(folder);
            // ObservableList<StandAloneAction> list = mPlugInsManager.GetStandAloneActions(); // .AddPluginPackage .GetInstalledPluginPackages().Count;

            //Assert
            // Assert.AreEqual(1, count);
            // Assert.AreEqual("Selenium Driver", list[0].Name );
            //Assert.AreEqual("SeleniumDriver", d.Name);

        }
    }
}
