using Amdocs.Ginger.CoreNET.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using GingerTestHelper;
using System.Globalization;

namespace Ginger.Reports.Tests
{
    [TestClass]
    public class ReportsTest
    {
        [TestMethod]
        public void ActivityReportTest()
        {


            string ActivityReportFile = GingerTestHelper.TestResources.GetTestResourcesFile(@"Reports\Activity.txt");
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

        [TestMethod]
        public void BusinessflowReportTest()
        {


            string BusinessFlowReportFile = GingerTestHelper.TestResources.GetTestResourcesFile(@"Reports\BusinessFlow.txt");
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

       
    }
}
