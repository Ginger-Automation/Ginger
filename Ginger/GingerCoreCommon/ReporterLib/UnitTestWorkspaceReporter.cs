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
            Console.WriteLine("ToLog: " + messageToLog + " " + exceptionToLog);
        }

        public override void ToStatus(eStatusMsgType messageType, string statusText)
        {
            Console.WriteLine("ToStatus: " + messageType + " " + statusText);
        }
    }
}
