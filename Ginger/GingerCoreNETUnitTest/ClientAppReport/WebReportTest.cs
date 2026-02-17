#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
            Assert.IsTrue(webReporterRunner.RunNewHtmlReport(string.Empty, null, null, false).RunnersColl.Count > 0);
        }

        [Ignore]  // TODO: create a test which run a runset with runset operation which includes CreateReport - see other runset execution examples
        [TestMethod]
        [Timeout(60000)]
        public void RunSetwithWebReport()
        {

        }



    }
}
