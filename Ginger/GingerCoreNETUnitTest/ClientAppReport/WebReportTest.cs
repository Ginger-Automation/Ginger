using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Logger;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GingerCoreNETUnitTest.ClientAppReport
{

    [TestClass]
    public class WebReportTest
    {
        
        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            string reportWebAppSolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions" + Path.DirectorySeparatorChar + "ReportWebApp");
            WorkspaceHelper.CreateWorkspaceAndOpenSolution(reportWebAppSolutionFolder);                     
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            
        }


        [Ignore]  // TODO: create a test which only generate the report package HTMLs - no need to open browser, then verify folder content
        [TestMethod]
        [Timeout(60000)]
        public void TestNewWebReport()
        {
            //a selected guid can be send 
            string guidStr = "";
            // a selected browser from unix can be run ,with his path
            string browserPath = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe";  // Will not work on Linux
            WebReportGenerator webReporterRunner = new WebReportGenerator(browserPath);
            Assert.IsTrue(webReporterRunner.RunNewHtmlReport(string.Empty, null, null, false).RunnersColl.Count>0);
        }

        [Ignore]  // TODO: create a test which run a runset with runset operation which includes CreateReport - see other runset execution examples
        [TestMethod]
        [Timeout(60000)]
        public void RunSetwithWebReport()
        {
            
        }

        

    }
}
