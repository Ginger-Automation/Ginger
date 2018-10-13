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
using Amdocs.Ginger.Common.ReporterLib;

namespace Amdocs.Ginger.Common
{
    public class AppReporter
    {
        public static event ReportEventHandler ReportEvent;
        public delegate void ReportEventHandler(AppReportEventArgs reportEventArgs);
        public static void OnReportEvent(eAppReportType reportType, string reportMessage, eAppReporterLogLevel reportLogLevel = eAppReporterLogLevel.INFO, Exception reportExceptionToRecord = null, bool logOnlyOnDebugMode = false, eAppReporterMessageType reportMessageType = eAppReporterMessageType.INFO)
        {
            ReportEventHandler handler = ReportEvent;
            if (handler != null)
            {
                handler(new AppReportEventArgs(reportType, reportMessage, reportLogLevel, reportExceptionToRecord, logOnlyOnDebugMode, reportMessageType));
            }
        }

        internal static void ToLog(eAppReporterLogLevel logLevel, string logMessage, Exception exceptionToRecord = null, bool logOnlyOnDebugMode=false)
        {
            OnReportEvent(reportType: eAppReportType.ToLog, reportMessage: logMessage, reportLogLevel: logLevel, reportExceptionToRecord: exceptionToRecord, logOnlyOnDebugMode: logOnlyOnDebugMode);
        }

        internal static void ToConsole(string consoleMessage, Exception exceptionToRecord = null)
        {
            OnReportEvent(reportType: eAppReportType.ToConsole, reportMessage: consoleMessage, reportExceptionToRecord: exceptionToRecord);
        }

        internal static void ToUser(eAppReporterMessageType reportMessageType, string message)
        {
            OnReportEvent(reportType: eAppReportType.ToUser, reportMessage: message, reportMessageType: reportMessageType);
        }
    }
}
