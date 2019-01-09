using System;
using GingerCoreNET.ReporterLib;

namespace Amdocs.Ginger.Common
{
    // Reporter for running unit tests
    public class UnitTestWorkspaceReporter : WorkSpaceReporterBase
    {
        public void ConsoleWriteLine(string message)
        {
            // !!!!!!! Create GingerUtils.WriteConsole(formatted)

            Console.WriteLine("Toconsole: " + message);
        }

        public override MessageBoxResult MessageBoxShow(string messageText, string caption, MessageBoxButton buttonsType, MessageBoxImage messageImage, MessageBoxResult defualtResault)
        {
            string txt = caption + Environment.NewLine;
            txt += messageText;
            Console.WriteLine("ToUser: " + txt);
            return defualtResault;
        }
        

        public override void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null, bool writeAlsoToConsoleIfNeeded = true, bool writeOnlyInDebugMode = false)
        {
            // For unit tests we write to console too
            Console.WriteLine("ToLog: " + messageToLog);
        }

        public override void ToStatus(eStatusMessageType messageType, string statusText)
        {
            Console.WriteLine("ToStatus: " + messageType + " " + statusText);
        }
    }
}
