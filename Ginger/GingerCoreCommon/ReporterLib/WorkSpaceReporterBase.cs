#region License
/*
Copyright © 2014-2023 European Support Limited

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
using System.Text;

namespace Amdocs.Ginger.Common
{
    /// <summary>
    /// Base class for implementation of WorkSpace reporter
    /// For Ginger WPF it will be implemented with MessageBox, write to log and console
    /// For GingerConsole, the message will appear in console
    /// For Unit Test there is no UI all goes to console with default response
    /// </summary>
    public abstract class WorkSpaceReporterBase
    {
        public abstract void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null);
                
        public void ToConsole(eLogLevel logLevel, string message)
        {            
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[").Append(logLevel).Append(" | ").Append(DateTime.Now.ToString("HH:mm:ss:fff_dd-MMM")).Append("] ").Append(message).Append(Environment.NewLine);

            switch (logLevel)
            {
                case eLogLevel.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case eLogLevel.FATAL:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case eLogLevel.DEBUG:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case eLogLevel.INFO:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case eLogLevel.WARN:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
               
            }
            Console.WriteLine(stringBuilder.ToString());
            Console.ResetColor();
        }

        public abstract eUserMsgSelection ToUser(string messageText, string caption, eUserMsgOption buttonsType, eUserMsgIcon messageImage, eUserMsgSelection defualtResualt);

        public abstract void ToStatus(eStatusMsgType messageType, string statusText);        
    }
}
