﻿using System;
using System.Text;

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
        public abstract void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null);
                
        public void ToConsole(eLogLevel logLevel, string message)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[").Append(logLevel).Append("]").Append(message).Append(Environment.NewLine);

            switch (logLevel)
            {
                case eLogLevel.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case eLogLevel.FATAL:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case eLogLevel.DEBUG:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                case eLogLevel.INFO:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case eLogLevel.WARN:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
               
            }
            Console.WriteLine(stringBuilder.ToString());
        }

        public abstract eUserMsgSelection ToUser(string messageText, string caption, eUserMsgOption buttonsType, eUserMsgIcon messageImage, eUserMsgSelection defualtResualt);

        public abstract void ToStatus(eStatusMsgType messageType, string statusText);        
    }
}
