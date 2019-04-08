using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerCoreNETUnitTest.RunTestslib
{
    class GingerUnitTestWorkspaceReporter : WorkSpaceReporterBase
    {
        public override void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null)
        {
            // throw new NotImplementedException();
        }

        public override void ToStatus(eStatusMsgType messageType, string statusText)
        {
            Console.WriteLine("ToStatus:" + messageType + " - " + statusText);
        }

        public override eUserMsgSelection ToUser(string messageText, string caption, eUserMsgOption buttonsType, eUserMsgIcon messageImage, eUserMsgSelection defualtResualt)
        {            
            Console.WriteLine("ToStatus select: " + caption + " - " + messageText + " - AutoSelect - " + defualtResualt);
            return defualtResualt;
        }
    }

}
