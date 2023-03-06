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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using System;

namespace Ginger.ReporterLib
{
    public class GingerWorkSpaceReporter : WorkSpaceReporterBase
    {
        
        
        public override Amdocs.Ginger.Common.eUserMsgSelection ToUser(string messageText, string caption, eUserMsgOption buttonsType, eUserMsgIcon messageImage, eUserMsgSelection defualtResault)
        {
            eUserMsgSelection result = defualtResault;  // if user just close the window we return the default defined result

            if (WorkSpace.Instance.RunningFromUnitTest)
            {
                result = eUserMsgSelection.Yes;
            }

            if (! WorkSpace.Instance.RunningInExecutionMode && !WorkSpace.Instance.RunningFromUnitTest)
            {
                App.MainWindow.Dispatcher.Invoke(() =>
                {
                    UserMessageBox messageBoxWindow = new UserMessageBox(messageText, caption, buttonsType, messageImage, defualtResault);
                    messageBoxWindow.ShowDialog();
                    result = messageBoxWindow.messageBoxResult;
                });
            }
            else
            {
                //not showing pop up message because running from config file and not wanting to get stuck
                ToLog(eLogLevel.WARN, string.Format("Not showing the User Message: '{0}' because application loaded in execution mode. Returning default selection value: '{1}'", messageText, defualtResault.ToString()));
            }

            return result;
        }

        public override void ToStatus(eStatusMsgType messageType, string statusText)
        {
            // TODO: Add icon, other info? tooltip seperate
            if (!WorkSpace.Instance.RunningInExecutionMode)
            {
                App.MainWindow.ShowStatus(messageType, statusText);
            }
            // Check if we need to write to console or already done !!!!
        }

        public override void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null)
        {
            Amdocs.Ginger.CoreNET.log4netLib.GingerLog.ToLog(logLevel, messageToLog, exceptionToLog);
            
        }
    }
}
