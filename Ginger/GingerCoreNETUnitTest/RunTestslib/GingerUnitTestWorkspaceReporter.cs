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
            throw new NotImplementedException();
        }

        public override eUserMsgSelection ToUser(string messageText, string caption, eUserMsgOption buttonsType, eUserMsgIcon messageImage, eUserMsgSelection defualtResualt)
        {
            throw new NotImplementedException();
        }
    }

}
