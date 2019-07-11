using Amdocs.Ginger.CoreNET.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using GingerTestHelper;
using System.Globalization;
using Ginger.Reports.GingerExecutionReport;
using System.IO;

namespace Ginger.Reports.Tests
{
    [TestClass]
    public class ReportsTest
    {
        [TestMethod]  [Timeout(60000)]
        public void ActivityReportTest()
        {


            string ActivityReportFile = GingerTestHelper.TestResources.GetTestResourcesFile(@"Reports" + Path.DirectorySeparatorChar + "Activity.txt");
            try
            {

                ActivityReport AR = (ActivityReport)JsonLib.LoadObjFromJSonFile(ActivityReportFile, typeof(ActivityReport));
                Assert.AreEqual("Passed", AR.RunStatus);
                Assert.AreEqual(2044, AR.Elapsed);
            }

            catch (Exception Ex)
            {
                Assert.Fail(Ex.Message);
            }
        }

        [TestMethod]  [Timeout(60000)]
        public void BusinessflowReportTest()
        {


            string BusinessFlowReportFile = GingerTestHelper.TestResources.GetTestResourcesFile(@"Reports" + Path.DirectorySeparatorChar + "BusinessFlow.txt");
            try
            {

                BusinessFlowReport BFR = (BusinessFlowReport)JsonLib.LoadObjFromJSonFile(BusinessFlowReportFile, typeof(BusinessFlowReport));
                Assert.AreEqual("Failed", BFR.RunStatus);
                Assert.AreEqual(float.Parse("36.279", CultureInfo.InvariantCulture), BFR.ElapsedSecs.Value);
            }

            catch (Exception Ex)
            {
                Assert.Fail(Ex.Message);
            }
        }

        [Ignore]
        [TestMethod]
        //[Timeout(60000)]
        public void GenrateLastExecutionHTMLReportTest()
        {
            string BusinessFlowReportFolder = GingerTestHelper.TestResources.GetTestResourcesFolder(@"Reports" + Path.DirectorySeparatorChar + "AutomationTab_LastExecution" + Path.DirectorySeparatorChar);
            ReportInfo RI = new ReportInfo(BusinessFlowReportFolder);
            //Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(RI);

            string templatesFolder = (ExtensionMethods.getGingerEXEFileName() + @"Reports" + Path.DirectorySeparatorChar + "GingerExecutionReport" + + Path.DirectorySeparatorChar).Replace("Ginger.exe", "");
            HTMLReportConfiguration selectedHTMLReportConfiguration = new HTMLReportConfiguration("DefaultTemplate", true);

            HTMLReportsConfiguration hTMLReportsConfiguration = new HTMLReportsConfiguration();

            // !!!!!!!!!!!!!!!!!
            string hTMLOutputFolder = @"C:\HTMLReports\";


            string report = Ginger.Reports.GingerExecutionReport.ExtensionMethods.NewFunctionCreateGingerExecutionReport(RI,false,selectedHTMLReportConfiguration, templatesFolder:templatesFolder, hTMLReportsConfiguration: hTMLReportsConfiguration,hTMLOutputFolder: hTMLOutputFolder);

        }

    }
}
