using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.Logger
{
    public class WebReportFilter
    {
        public ReportLevelType ReportLevel { set; get; }
        public string Guid { set; get; }

    }

    public enum ReportLevelType
    {
        RunSet=1,
        Runner=2,
        BusinessFlow=3,
        ActivityGroup=4,
        Activity=5,
        Action=6
    }

}
