#region License
/*
Copyright © 2014-2019 European Support Limited

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

namespace Amdocs.Ginger.GingerConsole.ReporterLib
{
    class GingerConsoleWorkspaceReporter : WorkSpaceReporterBase
    {        
        public override eUserMsgSelection ToUser(string messageText, string caption, eUserMsgOption buttonsType, eUserMsgIcon messageImage, eUserMsgSelection defualtResualt)
        {
            string txt = caption + Environment.NewLine;
            txt += messageText;
            // TODO: change console color
            switch (buttonsType )
            {
                // TODO: !!!!!!!!!!!!!!!!!!!!!!
                // show buttons [Ok] - O
                // Y, N, C
            }
            Console.WriteLine(txt);

            Console.ReadKey();
            return defualtResualt; // TEMP !!!!!!!!!            
        }

        public override void ToStatus(eStatusMsgType messageType, string statusText)
        {
            Console.WriteLine(statusText);
        }
        

        public override void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null)
        {
            Console.WriteLine("[" + logLevel + "]" + messageToLog);
        }
        
        // TODO: override WriteToConsole with color and...        
    }
}
