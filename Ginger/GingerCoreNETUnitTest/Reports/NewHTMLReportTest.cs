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
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Amdocs.Ginger.Run;
using Ginger.Reports;
using Ginger.Reports.HTMLReports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GingerCoreNETUnitTest.Reports
{
    public class NewHTMLReportTest
    {
        static GingerRunner mGingerRunner;


        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            mGingerRunner = new GingerRunner();

            // add ExecutionDumperListener so json.txt files will be created            
            string dumpFolder = TestResources.GetTempFolder("dumper");

            ExecutionDumperListener executionDumperListener = new ExecutionDumperListener(dumpFolder);
            ((GingerExecutionEngine)mGingerRunner.Executor).RunListeners.Add(executionDumperListener);
        }

        [ClassCleanup]
        public static void ClassCleanup()
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


        [TestMethod]
        public void RunFlowAndCreateReport()
        {
            //Arrange
            BusinessFlow mBF = new BusinessFlow
            {
                Activities = [],
                Name = "BF TEst timeline events listener",
                Active = true
            };
            Activity activitiy1 = new Activity() { Active = true };
            activitiy1.Active = true;
            mBF.Activities.Add(activitiy1);
            ActDummy action1 = new ActDummy() { Description = "Dummay action 1", Active = true };
            activitiy1.Acts.Add(action1);
            mGingerRunner.Executor.BusinessFlows.Add(mBF);
            RunListenerBase.Start();
            mGingerRunner.Executor.RunBusinessFlow(mBF);

            //Act
            NewHTMLReport newHTMLReport = new NewHTMLReport();
            ReportInfo reportInfo = new ReportInfo("aaa");  // !!!!!!!!!!!!!!!
            string s = newHTMLReport.CreateReport(reportInfo);


            //Assert
            // Assert.AreEqual(1, events.Count, "Events count");            
        }


        [TestMethod]
        public void CreateReportFromFolder()
        {
            // Arrange
            string folder = TestResources.GetTestResourcesFolder("Reports" + Path.DirectorySeparatorChar + "AutomationTab_LastExecution");

            //Act
            ReportInfo reportInfo = new ReportInfo(folder);
            NewHTMLReport rep = new NewHTMLReport();
            string s = rep.CreateReport(reportInfo);



            // TODO: need to create same report like HTMLDetailedReport

            // System.IO.File.WriteAllText(@"c:\temp\rep1.html", s);


            //HTMLDetailedReport hTMLDetailedReport = new HTMLDetailedReport();

            //HTMLReportConfiguration hTMLReportConfiguration = new HTMLReportConfiguration();
            //hTMLReportConfiguration.RunSetFieldsToSelect = HTMLReportConfiguration.GetReportLevelMembers(typeof(RunSetReport));
            //hTMLReportConfiguration.EmailSummaryViewFieldsToSelect = HTMLReportConfiguration.GetReportLevelMembers(typeof(RunSetReport));
            //hTMLReportConfiguration.GingerRunnerFieldsToSelect = HTMLReportConfiguration.GetReportLevelMembers(typeof(GingerReport));
            //hTMLReportConfiguration.BusinessFlowFieldsToSelect = HTMLReportConfiguration.GetReportLevelMembers(typeof(BusinessFlowReport));
            //hTMLReportConfiguration.ActivityGroupFieldsToSelect = HTMLReportConfiguration.GetReportLevelMembers(typeof(ActivityGroupReport));
            //hTMLReportConfiguration.ActivityFieldsToSelect = HTMLReportConfiguration.GetReportLevelMembers(typeof(ActivityReport));
            //hTMLReportConfiguration.ActionFieldsToSelect = HTMLReportConfiguration.GetReportLevelMembers(typeof(ActionReport));


            //GingerExecutionReport gingerExecutionReport = new GingerExecutionReport();
            //gingerExecutionReport.TemplatesFolder = @"C:\Users\yaronwe\source\repos\Ginger\Ginger\Ginger\Reports\GingerExecutionReport\";
            //gingerExecutionReport.HTMLReportMainFolder = @"c:\temp\rep1";
            //gingerExecutionReport.CreateSummaryViewReport(reportInfo);
            // gingerExecutionReport.CreateSummaryViewReport( .CreateBusinessFlowLevelReport(reportInfo);

            // string s = ExtensionMethods.CreateSummaryViewReport( .CreateGingerExecutionReport(reportInfo, true);
            // string s = hTMLDetailedReport.CreateReport(reportInfo);
            //System.IO.File.WriteAllText(@"c:\temp\rep1.html", s);

            // gingerExecutionReport.cre .CreateGingerExecutionReport()
            //HTMLSummaryReport
        }
    }
}
