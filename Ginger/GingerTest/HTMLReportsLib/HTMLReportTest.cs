using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using GingerCore.Environments;
using GingerTestHelper;
using GingerWPFUnitTest;
using GingerWPFUnitTest.GeneralLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerTest.HTMLReportsLib
{
    [TestClass]
    public class HTMLReportTest
    {
        static GingerAutomator mGingerAutomator;
        //static SolutionRepository mSolutionRepository;
        
        static string solutionFolder;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            CreateTestSolution();

            // Init SR
            mGingerAutomator = new GingerAutomator();
            mGingerAutomator.StartGinger();
            mGingerAutomator.OpenSolution(solutionFolder);

            // WorkSpace.Instance.SolutionRepository.Open(solutionFolder);
            // mSolutionRepository = Ginger.App.CreateGingerSolutionRepository();
            // Ginger.App.InitClassTypesDictionary();            
            // mSolutionRepository.Open(solutionFolder);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            mGingerAutomator.CloseGinger();
        }

        private static void CreateTestSolution()
        {                        
            string sourceFolder = TestResources.GetTestResourcesFolder(@"Solutions\ReportSR");
            solutionFolder = TestResources.getGingerUnitTesterTempFolder(@"Solutions\ReportSR");
            if (Directory.Exists(solutionFolder))
            {
                Directory.Delete(solutionFolder, true);
            }
            CopyDir.Copy(sourceFolder, solutionFolder);

            SolutionRepository SR = new SolutionRepository();
            SR = Ginger.App.CreateGingerSolutionRepository();
            SR.Open(solutionFolder);
          
            SR.Close();
        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }



        [TestMethod,Timeout(60000)]
        public void AddHTMLReport()
        {
            //Arrange
            string ReportName = "Template #1";

            //Act            
            ObservableList<HTMLReportConfiguration> allReports = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
            HTMLReportConfiguration Report1 = (from x in allReports where x.Name == ReportName select x).SingleOrDefault();

            //Assert  
            Assert.AreEqual(Report1.Name, ReportName);

        }
    }
}
