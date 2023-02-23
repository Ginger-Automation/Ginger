#region License
/*
Copyright © 2014-2023 European Support Limited

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


using GingerTest;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GingerWPFUnitTest
{
    [TestClass]
    [Level3]
    public class GingerBasicsTest
    {
        static TestContext mTC;
        static GingerAutomator mGingerAutomator; 
        static string SolutionFolder;
        static Mutex mutex = new Mutex();

        [ClassInitialize]
        public static void ClassInit(TestContext TC)
        {
            mTC = TC;
            mGingerAutomator = GingerAutomator.StartSession();
            string sampleSolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\EnvsTest");
            SolutionFolder = TestResources.GetTempFolder(@"Solutions\EnvsTest22222");
            if (Directory.Exists(SolutionFolder))
            {
                Directory.Delete(SolutionFolder, true);
            }

            CopyDir.Copy(sampleSolutionFolder, SolutionFolder);

            mGingerAutomator.OpenSolution(SolutionFolder);
        }


        [ClassCleanup]
        public static void ClassCleanup()
        {
            GingerAutomator.EndSession();            
        }

        // Run before each test
        [TestInitialize]
        public void TestInitialize()
        {
            mutex.WaitOne();
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            mutex.ReleaseMutex();
        }


        [TestMethod]
        [Timeout(60000)]        
        public void CheckTabsWhenSolutionClosed()
        {
            //Arrange

            //Act            
            mGingerAutomator.CloseSolution();
            List<string> visibileTabs = mGingerAutomator.MainWindowPOM.GetMenus();
            string tabs = string.Join(",", visibileTabs);

            //Assert
            Assert.AreEqual("xSolutionSelectionMenu,xExtraSolutionOperationsMenu,xSolutionSourceControlMenu,xUserOperationsMenu,xExtraOperationsMenu", tabs);
        }

        


    }
}
