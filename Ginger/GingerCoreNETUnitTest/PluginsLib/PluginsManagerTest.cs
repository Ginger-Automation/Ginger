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
using System.IO;
using System.Linq;

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

        //[TestMethod]
        //public void AddPluginPackage()
        
        [TestMethod]
        public void InstalledPluginPackageFromOnline()
        {
            //Arrange   

            // TODO: create a simple plugin for unit test which will download faster.

            string PluginId = "PACT";
            string PluginVersion = "1.6";
            string path = TestResources.getGingerUnitTesterTempFolder(@"Solutions\PluginsManagerSR1");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            SolutionRepository solutionRepository = new SolutionRepository();
            solutionRepository.AddItemInfo<PluginPackage>("*.Ginger.PluginPackage.xml", @"~\Plugins", true, "Plugins", PropertyNameForFileName: nameof(PluginPackage.PluginId));
            solutionRepository.CreateRepository(path);
            solutionRepository.Open(path);            
            
            PluginsManager pluginsManager = new PluginsManager(solutionRepository);
            ObservableList<PluginPackage> pluginPackages =  solutionRepository.GetAllRepositoryItems<PluginPackage>();

            // Act            
            var p = pluginsManager.GetOnlinePluginsIndex();
            OnlinePluginPackage onlinePluginPackage = (from x in p where x.Id == PluginId select x).SingleOrDefault();
            //OnlinePluginPackageRelease onlinePluginPackageRelease 
            pluginsManager.InstallPluginPackage(onlinePluginPackage, onlinePluginPackage.Releases[0]);
            //string folder = Path.Combine(Common.GetTestResourcesFolder(), @"PluginPackages\SeleniumPluginPackage.1.0.0");

            
            //Assert
            Assert.AreEqual(1, pluginPackages.Count);
            Assert.AreEqual("PACT", pluginPackages[0].PluginId);            
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
