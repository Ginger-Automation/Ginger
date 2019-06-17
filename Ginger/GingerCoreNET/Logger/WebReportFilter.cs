using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Logger
{
    public class WebReportFilter
    {
        public ReportLevelType ReportLevel { set; get; }
        public string Guid { set; get; }

    }

    public enum ReportLevelType
    {
        RunSet = 1,
        Runner = 2,
        BusinessFlow = 3,
        ActivityGroup = 4,
        Activity = 5,
        Action = 6
    }
}
