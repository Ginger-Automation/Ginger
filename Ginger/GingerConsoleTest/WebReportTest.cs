using Amdocs.Ginger.CoreNET.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerConsoleTest
{
    [TestClass]
   public class WebReportTest
    {
        [TestMethod]
        [Timeout(60000)]
        public void TestNewWebReport()
        {
            string guidStr = "";
            WebReportGenerator webReporterRunner = new WebReportGenerator();
            webReporterRunner.RunNewHtmlReport();
        }
    }
}
