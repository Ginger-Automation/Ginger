#region License
/*
Copyright © 2014-2018 European Support Limited

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
