using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common
{
    public class UnitTestWorkspaceReporter : IWorkSpaceReporter
    {
        public void ConsoleWriteLine(string message)
        {
            Console.WriteLine(message);
        }

        public void ShowMessageToUser(string message)
        {
            Console.WriteLine(message);
        }

        public void ShowMessageToUser(UserMessage userMessage)
        {
            Console.WriteLine(userMessage.Message);
        }

        public void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null, bool writeAlsoToConsoleIfNeeded = true, bool writeOnlyInDebugMode = false)
        {
            Console.WriteLine(messageToLog);
            // TODO:  ???
        }
    }
}
