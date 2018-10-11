using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common.ReporterLib
{
    public class AppReportEventArgs
    {
        public eAppReportType ReportType;

        public string ReportMessage;

        public eAppReporterLogLevel ReportLogLevel;//TODO: group to 1 class
        public bool LogOnlyOnDebugMode;
        public Exception ReportExceptionToRecord;

        public eAppReporterMessageType ReportMessageType;


        public AppReportEventArgs(eAppReportType reportType, string reportMessage, eAppReporterLogLevel reportLogLevel = eAppReporterLogLevel.INFO, Exception reportExceptionToRecord = null, bool logOnlyOnDebugMode = false,  eAppReporterMessageType reportMessageType = eAppReporterMessageType.INFO)
        {
            this.ReportType = reportType;

            this.ReportMessage = reportMessage;

            this.ReportLogLevel = reportLogLevel;
            this.LogOnlyOnDebugMode = logOnlyOnDebugMode;
            this.ReportExceptionToRecord = reportExceptionToRecord;

            this.ReportMessageType = reportMessageType;           
        }
    }
}
