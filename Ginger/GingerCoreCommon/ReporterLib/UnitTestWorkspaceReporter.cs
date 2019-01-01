using System;
using System.Collections.Generic;
using System.Text;
using GingerCoreNET.ReporterLib;

namespace Amdocs.Ginger.Common
{
    // Reporter for running unit tests
    public class UnitTestWorkspaceReporter : IWorkSpaceReporter
    {
        public void ConsoleWriteLine(string message)
        {
            Console.WriteLine("Toconsole: " + message);
        }

        public MessageBoxResult MessageBoxShow(string messageText, string caption, MessageBoxButton buttonsType, MessageBoxImage messageImage, MessageBoxResult defualtResualt)
        {
            string txt = caption + Environment.NewLine;
            txt += messageText;
            Console.WriteLine("ToUser: " + txt);
            return defualtResualt;
        }

        public void ShowMessageToUser(string message)
        {
            Console.WriteLine("ToUser: " + message);
        }

        public void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null, bool writeAlsoToConsoleIfNeeded = true, bool writeOnlyInDebugMode = false)
        {
            // For unit tests we write to console too
            Console.WriteLine("ToLog: " + messageToLog);
        }
    }
}
