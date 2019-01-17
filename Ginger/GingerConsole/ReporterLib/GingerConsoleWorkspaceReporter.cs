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
