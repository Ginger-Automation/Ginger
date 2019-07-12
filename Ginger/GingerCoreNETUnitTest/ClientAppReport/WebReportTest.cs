﻿using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Logger;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.SolutionGeneral;
using GingerCoreNETUnitTest.RunTestslib;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GingerCoreNETUnitTest.ClientAppReport
{
    [Ignore]
    [TestClass]
    public class WebReportTest
    {
        private SolutionRepository sr;


        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            string jsonfilepath = TestResources.GetTestResourcesFolder(@"Solutions" + Path.DirectorySeparatorChar + "ReportWebApp");
            WorkspaceHelper.CreateWorkspaceAndOpenSolution("WebReportTest" , jsonfilepath);
            
            // WorkSpace.Instance.Solution = (Solution)(ISolution)sr.RepositorySerializer.DeserializeFromFile(Path.Combine(sr.SolutionFolder, "Ginger.Solution.xml"));
        }

        [TestMethod]
        [Timeout(60000)]
        public void TestNewWebReport()
        {
            //a selected guid can be send 
            string guidStr = "";
            // a selected browser from unix can be run ,with his path
            string browserPath = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe";
            WebReportGenerator webReporterRunner = new WebReportGenerator(browserPath);
            Assert.IsTrue(webReporterRunner.RunNewHtmlReport(null, null, false).RunnersColl.Count>0);
        }

        private void OpenSolution(string sFolder)
        {
            if (Directory.Exists(sFolder))
            {
                Console.WriteLine("Opening Solution at folder: " + sFolder);
                sr = GingerSolutionRepository.CreateGingerSolutionRepository();
                WorkSpace.Instance.SolutionRepository = sr;
                sr.Open(sFolder);
            }
            else
            {
                Console.WriteLine("Directory not found: " + sFolder);
            }
        }

    }
}
