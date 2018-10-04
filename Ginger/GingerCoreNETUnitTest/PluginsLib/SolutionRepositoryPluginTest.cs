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
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using GingerCoreNET.RunLib;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
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

        [ClassCleanup]
        public static void ClassCleanup()
        {
            foreach (GingerNodeInfo GNI in WorkSpace.Instance.LocalGingerGrid.NodeList)
            {
                GingerNodeProxy proxy = new GingerNodeProxy(GNI);                    
                // proxy.Shutdown();
            }
            
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
        public void GetPluginServices()
        {
            //Arrange            
            ObservableList<PluginPackage> Plugins = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
            PluginPackage p = (from x in Plugins where x.PluginID == "GingerOfficePlugin" select x).SingleOrDefault();

            // Act            
            List<IGingerService> list = p.GetServices();

            //Assert                        
            Assert.AreEqual(6, list.Count, "There are 6 stand alone actions");
        }

        [TestMethod]
        public void GetPluginTextEditor()
        {
            //Arrange            
            string pluginFolder = TestResources.GetTestResourcesFolder(@"PluginPackages\ExamplePlugin");
            PluginPackage plugin =  new PluginPackage(pluginFolder);

            // Act            
            ObservableList<ITextEditor> list = plugin.GetTextFileEditors();

            //Assert                        
            Assert.AreEqual(1, list.Count, "There are one text editor");
        }

        [TestMethod]
        public void GingerOfficePluginTestAction()
        {
            ////Arrange            
            //ObservableList<PluginPackage> Plugins = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
            //PluginPackage p = (from x in Plugins where x.PluginID == "GingerOfficePlugin" select x).SingleOrDefault();
            //ObservableList<StandAloneAction> list = p.GetStandAloneActions();
            //StandAloneAction standAloneAction = list[0];
            //GingerAction GA = new GingerAction();
            //GA.InputParams["PluginID"].Value = p.PluginID;
            //GA.InputParams["PluginActionID"].Value = standAloneAction.ID;
            //GA.InputParams["A"].Value = "hi";
            //GA.InputParams["B"].Value = "yo";

            //// Act                        
            //WorkSpace.Instance.PlugInsManager.Execute(GA);

            ////Assert                        
            //Assert.AreEqual("ab", GA.Output.Values[0].Param , "Test action output");
        }


        [TestMethod]
        public void GingerOfficePluginTestActionx3()
        {
            ////Arrange            
            //ObservableList<PluginPackage> Plugins = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
            //PluginPackage p = (from x in Plugins where x.PluginID == "GingerOfficePlugin" select x).SingleOrDefault();
            //ObservableList<StandAloneAction> list = p.GetStandAloneActions();
            //StandAloneAction standAloneAction = list[0];
            //GingerAction GA = new GingerAction(standAloneAction.ID);
            //GA.InputParams["PluginID"].Value = p.PluginID;
            //GA.InputParams["PluginActionID"].Value = standAloneAction.ID;
            //GA.InputParams["A"].Value = "hi";
            //GA.InputParams["B"].Value = "yo";


            //// Act                        
            //for (int i = 0; i < 3; i++)
            //{
            //    WorkSpace.Instance.PlugInsManager.Execute(GA);
            //}


            ////Assert                
            //Assert.AreEqual(1, WorkSpace.Instance.LocalGingerGrid.NodeList.Count, "GingerGrid nodes 1 - only one service is up - reuse");
            
        }

    }       
}
