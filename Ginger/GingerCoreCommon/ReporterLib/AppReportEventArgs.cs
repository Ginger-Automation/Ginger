using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common.ReporterLib
{
    public class AppReportEventArgs
    {
        eAppReportType mReportType;
        public eAppReportType ReportType
        {
            get
            {
                return mReportType;
            }
        }

        string mReportMessage;
        public string ReportMessage
        {
            get
            {
                return mReportMessage;
            }
        }

        eAppReporterLogLevel mReportLogLevel;
        public eAppReporterLogLevel ReportLogLevel//TODO: group to 1 class
        {
            get
            {
                return mReportLogLevel;
            }
        }

        bool mLogOnlyOnDebugMode;
        public bool LogOnlyOnDebugMode
        {
            get
            {
                return mLogOnlyOnDebugMode;
            }
        }

        Exception mReportExceptionToRecord;
        public Exception ReportExceptionToRecord
        {
            get
            {
                return mReportExceptionToRecord;
            }
        }

        eAppReporterMessageType mReportMessageType;
        public eAppReporterMessageType ReportMessageType
        {
            get
            {
                return mReportMessageType;
            }
        }


        public AppReportEventArgs(eAppReportType reportType, string reportMessage, eAppReporterLogLevel reportLogLevel = eAppReporterLogLevel.INFO, Exception reportExceptionToRecord = null, bool logOnlyOnDebugMode = false, eAppReporterMessageType reportMessageType = eAppReporterMessageType.INFO)
        {
            mReportType = reportType;

            mReportMessage = reportMessage;

            mReportLogLevel = reportLogLevel;
            mLogOnlyOnDebugMode = logOnlyOnDebugMode;
            mReportExceptionToRecord = reportExceptionToRecord;

            mReportMessageType = reportMessageType;
        }
    }
}
