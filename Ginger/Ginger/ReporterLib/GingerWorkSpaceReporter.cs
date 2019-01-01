using Amdocs.Ginger.Common;
using System;
using System.Windows;

namespace Ginger.ReporterLib
{
    public class GingerWorkSpaceReporter : IWorkSpaceReporter
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void ConsoleWriteLine(string message)
        {
            Console.WriteLine(message + System.Environment.NewLine);
        }

        public void ShowMessageToUser(string message)
        {
            MessageBox.Show(message);
        }

        public void ShowMessageToUser(UserMessage userMessage)
        {    
            // Open nice page with message
            MessageBox.Show(userMessage.Message);
        }

        public void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null, bool writeAlsoToConsoleIfNeeded = true, bool writeOnlyInDebugMode = false)
        {
            //try
            //{
            //    if (writeOnlyInDebugMode)
            //        if (CurrentAppLogLevel != eAppReporterLoggingLevel.Debug)
            //            return;
            //    switch (logLevel)
            //    {
            //        case eAppReporterLogLevel.DEBUG:
            //            log.Debug(messageToLog, exceptionToLog);
            //            break;
            //        case eAppReporterLogLevel.ERROR:
            //            log.Error(messageToLog, exceptionToLog);
            //            OnErrorReportedEvent();
            //            break;
            //        case eAppReporterLogLevel.FATAL:
            //            log.Fatal(messageToLog, exceptionToLog);
            //            break;
            //        case eAppReporterLogLevel.INFO:
            //            log.Info(messageToLog, exceptionToLog);
            //            break;
            //        case eAppReporterLogLevel.WARN:
            //            log.Warn(messageToLog, exceptionToLog);
            //            break;
            //        default:
            //            log.Info(messageToLog, exceptionToLog);
            //            break;
            //    }

            //    //if (writeAlsoToConsoleIfNeeded && AddAllReportingToConsole)
            //    ToConsole(logLevel.ToString() + ": " + messageToLog, exceptionToLog);
            //}
            //catch (Exception ex)
            //{
            //    //failed to write to log
            //}
        }
    }
}
