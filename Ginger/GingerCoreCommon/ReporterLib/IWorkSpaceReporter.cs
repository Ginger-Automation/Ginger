using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common
{
    /// <summary>
    /// Interface for implementation if WorkSpace reporter
    /// For Ginger WPF it will be implemented with MessageBox, write to log and console
    /// For GingerConsole, the message will apear in console
    /// </summary>
    public interface IWorkSpaceReporter
    {
        void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null, bool writeAlsoToConsoleIfNeeded = true, bool writeOnlyInDebugMode = false);
        void ConsoleWriteLine(string message);
        void ShowMessageToUser(string message);
        void ShowMessageToUser(UserMessage userMessage);
    }
}
