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

using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.GingerRuntime.ReporterLib
{
    public class GingerRuntimeWorkspaceReporter : WorkSpaceReporterBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override eUserMsgSelection ToUser(string messageText, string caption, eUserMsgOption buttonsType, eUserMsgIcon messageImage, eUserMsgSelection defualtResualt)
        {
            string txt = caption + Environment.NewLine;
            txt += messageText;
            // TODO: change console color
            // switch (buttonsType )
            // {
                // TODO: !!!!!!!!!!!!!!!!!!!!!!
                // show buttons [Ok] - O
                // Y, N, C
            // }
            Console.WriteLine(txt);
            
            return defualtResualt; // TEMP !!!!!!!!!            
        }

        public override void ToStatus(eStatusMsgType messageType, string statusText)
        {
           /// Console.WriteLine(statusText); //write to Console already been done by Reporter
        }
        

        public override void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null)
        {
            try
            {                
                switch (logLevel)
                {                    
                    case eLogLevel.DEBUG:
                        log.Debug(messageToLog, exceptionToLog);
                        break;
                    case eLogLevel.ERROR:
                        log.Error(messageToLog, exceptionToLog);
                        break;
                    case eLogLevel.FATAL:
                        log.Fatal(messageToLog, exceptionToLog);
                        break;
                    case eLogLevel.INFO:
                        log.Info(messageToLog, exceptionToLog);
                        break;
                    case eLogLevel.WARN:
                        log.Warn(messageToLog, exceptionToLog);
                        break;
                    default:
                        log.Info(messageToLog, exceptionToLog);
                        break;
                }
            }
            catch (Exception ex)
            {
                //failed to write to log
                throw (ex);
            }
        }
        
        
    }
}
