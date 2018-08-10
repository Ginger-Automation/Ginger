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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
using Amdocs.Ginger.Repository;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace GingerCoreNETUnitTest.PluginsLib
{
    [TestClass]
    [Level1]
    public class SolutionRepositoryPluginTest
    {

        static SolutionRepository SR;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            DummyWorkSpace ws = new DummyWorkSpace();
            WorkSpace.Init(ws);

            string folder = TestResources.getGingerUnitTesterTempFolder("Solutions", "PluginTest");

            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }

            SR = new SolutionRepository();
            SR.AddItemInfo<PluginPackage>("*.Ginger.PluginPackage.xml", @"~\Plugins", true, "Plugins", addToRootFolders: true, PropertyNameForFileName: nameof(PluginPackage.PluginID));
            SR.CreateRepository(folder);
            SR.Open(folder);
            WorkSpace.Instance.SolutionRepository = SR;
            string pluginFolder = TestResources.GetTestResourcesFolder(@"PluginPackages\GingerOfficePlugin");


            //string txt = WorkSpace.Instance.PlugInsManager.CreatePluginPackageInfo("GingerOfficePlugin", "1.0.0");
            //System.IO.File.WriteAllText(pluginFolder + @"\Ginger.PluginPackage.json", txt);


            WorkSpace.Instance.PlugInsManager.AddPluginPackage(pluginFolder);


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
        public void GetPlugins()
        {
            //Arrange            

            // Act            
            ObservableList<PluginPackage> Plugins = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();

            //Assert            
            Assert.AreEqual("GingerOfficePlugin", Plugins[0].PluginID, "PluginID=GingerOfficePlugin");
            Assert.AreEqual("1.0.0", Plugins[0].PluginPackageVersion, "Version=1.0");

        }

        [TestMethod]
        public void GetPluginStandAloneActions()
        {
            //Arrange            
            ObservableList<PluginPackage> Plugins = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
            PluginPackage p = (from x in Plugins where x.PluginID == "GingerOfficePlugin" select x).SingleOrDefault();

            // Act            
            ObservableList<StandAloneAction> list = p.GetStandAloneActions();

            //Assert                        
            Assert.AreEqual(6, list.Count, "There are 6 stand alone actions");
        }

    }       
}
