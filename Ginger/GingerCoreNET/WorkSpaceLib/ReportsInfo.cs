using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.WorkSpaceLib
{
    public class ReportsInfo
    {
        public string EmailReportTempFolder
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.IO.Path.GetTempFileName()) + "\\Ginger_Email_Reports";
            }
        }
    }
}
