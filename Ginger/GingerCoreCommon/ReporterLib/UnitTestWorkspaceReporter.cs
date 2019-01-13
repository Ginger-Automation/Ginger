using System;

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

        public override eUserMsgSelection ToUser(string messageText, string caption, eUserMsgOption buttonsType, eUserMsgIcon messageImage, eUserMsgSelection defualtResault)
        {
            string txt = caption + Environment.NewLine;
            txt += messageText;
            Console.WriteLine("ToUser: " + txt);
            return defualtResault;
        }
        

        public override void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null)
        {
            // For unit tests we write to console too
            Console.WriteLine("ToLog: " + messageToLog);
        }

        public override void ToStatus(eStatusMsgType messageType, string statusText)
        {
            Console.WriteLine("ToStatus: " + messageType + " " + statusText);
        }
    }
}
