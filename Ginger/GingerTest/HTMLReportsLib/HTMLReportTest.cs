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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace GingerTest
{
    [Ignore]  // temp fail on Azure
    [TestClass]
    [Level3]
    public class HTMLReportTest
    {
        static TestHelper mTestHelper = new TestHelper();
        public TestContext TestContext { get; set; }

        static GingerAutomator mGingerAutomator;        
        static string solutionFolder;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            mTestHelper.ClassInitialize(TestContext);

            CreateTestSolution();
            mGingerAutomator = GingerAutomator.StartSession();
            mGingerAutomator.OpenSolution(solutionFolder);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {            
            GingerAutomator.EndSession();
            mTestHelper.ClassCleanup();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            mTestHelper.TestInitialize(TestContext);
        }


        [TestCleanup]
        public void TestCleanUp()
        {
            mTestHelper.TestCleanup();
        }


        private static void CreateTestSolution()
        {                        
            string sourceFolder = TestResources.GetTestResourcesFolder(@"Solutions\ReportSR");
            solutionFolder = TestResources.GetTestTempFolder(@"Solutions\ReportSR");
            if (Directory.Exists(solutionFolder))
            {
                Directory.Delete(solutionFolder, true);
            }
            CopyDir.Copy(sourceFolder, solutionFolder);

            SolutionRepository SR = new SolutionRepository();
            SR = GingerSolutionRepository.CreateGingerSolutionRepository();
            SR.Open(solutionFolder);
          
            SR.Close();
        }

      

        
        [TestMethod]  [Timeout(60000)]
        public void AddHTMLReport()
        {
            //Arrange
            string ReportName = "Template #1";
            string screenShotFileName = mTestHelper.GetTempFileName("screen1.png");
            mGingerAutomator.MainWindowPOM.TakeScreenShot(screenShotFileName);
            mTestHelper.AddTestArtifact(screenShotFileName);

            //Act            
            ObservableList<HTMLReportConfiguration> allReports = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
            HTMLReportConfiguration Report1 = (from x in allReports where x.Name == ReportName select x).SingleOrDefault();

            //Assert  
            Assert.AreEqual(Report1.Name, ReportName);

        }
    }
}
