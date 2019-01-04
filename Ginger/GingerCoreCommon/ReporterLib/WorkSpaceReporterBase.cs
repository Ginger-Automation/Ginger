using System;
using System.Text;
using GingerCoreNET.ReporterLib;

namespace Amdocs.Ginger.Common
{
    /// <summary>
    /// Base class for implementation of WorkSpace reporter
    /// For Ginger WPF it will be implemented with MessageBox, write to log and console
    /// For GingerConsole, the message will apear in console
    /// For Unit Test there is no UI all goes to console with default response
    /// </summary>
    public abstract class WorkSpaceReporterBase
    {
        public abstract void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null, bool writeAlsoToConsoleIfNeeded = true, bool writeOnlyInDebugMode = false);
                
        public void ConsoleWriteLine(eLogLevel logLevel, string message)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[").Append(logLevel).Append("]").Append(message).Append(Environment.NewLine);
            Console.WriteLine(stringBuilder.ToString());
        }

        public abstract MessageBoxResult MessageBoxShow(string messageText, string caption, MessageBoxButton buttonsType, MessageBoxImage messageImage, MessageBoxResult defualtResualt);

        public abstract void ToStatus(eStatusMessageType messageType, string statusText);

        
    }
}
