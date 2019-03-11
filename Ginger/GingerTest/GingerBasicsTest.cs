#region License
/*
Copyright © 2014-2019 European Support Limited

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


//using GingerCore.Environments;
//using GingerTestHelper;
//using GingerWPFUnitTest.GeneralLib;
//using GingerWPFUnitTest.POMs;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading;

//namespace GingerWPFUnitTest
//{
//    [TestClass]
//    [Level3]
//    public class GingerBasicsTest
//    {        
//        static TestContext mTC;
//        static GingerAutomator mGingerAutomator = GingerAutomator.Instance; // TestAssemblyInit.mGingerAutomator; //. new GingerAutomator();
//        static string SolutionFolder;
//        // Mutex mutex = new Mutex();

//        [ClassInitialize]
//        public static void ClassInit(TestContext TC)
//        {            
//            mTC = TC;
//            // mGingerAutomator.StartGinger();
//            //string sampleSolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\EnvsTest");
//            //SolutionFolder = TestResources.getGingerUnitTesterTempFolder(@"Solutions\EnvsTest22222");
//            //if (Directory.Exists(SolutionFolder))
//            //{
//            //    Directory.Delete(SolutionFolder,true);
//            //}

//            //CopyDir.Copy(sampleSolutionFolder, SolutionFolder);
            
//            //mGingerAutomator.OpenSolution(SolutionFolder);            
//        }


//        [ClassCleanup]        
//        public static void ClassCleanup()
//        {
//            //mGingerAutomator.CloseGinger();
//            //mGingerAutomator = null;
//        }

//        // Run before each test
//        [TestInitialize]
//        public void TestInitialize()
//        {            
//             // mutex.WaitOne();
//            //GingerAutomator.TestMutex.WaitOne();
//        }

//        [TestCleanup]
//        public void TestCleanUp()
//        {                  
//             // mutex.ReleaseMutex();
//            //GingerAutomator.TestMutex.ReleaseMutex();
//        }

        
//        [TestMethod]  [Timeout(60000)]  
//        [Ignore]
//        public void CheckTabsWhenSolutionClosed()
//        {
//            //Arrange

//            //Act            
//            mGingerAutomator.CloseSolution();
//            List<string> visibileTabs = mGingerAutomator.MainWindowPOM.GetVisibleRibbonTabs();
//            string tabs = string.Join(",", visibileTabs);

//            //Assert
//            Assert.AreEqual("HomeRibbon,SolutionRibbon,SupportRibbon", tabs);
//        }
        
//        [TestMethod]  [Timeout(60000)]
//        [Ignore]
//        public void CheckTabsWhenSolutionOpen()
//        {
//            //Arrange

//            //Act            
//            mGingerAutomator.OpenSolution(SolutionFolder);
//            List<string> visibileTabs = mGingerAutomator.MainWindowPOM.GetVisibleRibbonTabs();
//            string tabs = string.Join(",", visibileTabs);

//            //Assert
//            Assert.AreEqual("HomeRibbon,SolutionRibbon,AutomateRibbon,RunRibbon,xConfigurations,xResources,SupportRibbon", tabs);
//        }


        
      

//    }
//}
