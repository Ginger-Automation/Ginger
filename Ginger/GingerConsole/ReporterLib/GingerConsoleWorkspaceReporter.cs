using Amdocs.Ginger.Common;
using GingerCoreNET.ReporterLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.GingerConsole.ReporterLib
{
    class GingerConsoleWorkspaceReporter : WorkSpaceReporterBase
    {
        
        public override MessageBoxResult MessageBoxShow(string messageText, string caption, MessageBoxButton buttonsType, MessageBoxImage messageImage, MessageBoxResult defualtResualt)
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

        public override void ToStatus(eStatusMessageType messageType, string statusText)
        {
            Console.WriteLine(statusText);
        }
        

        public override void ToLog(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null, bool writeAlsoToConsoleIfNeeded = true, bool writeOnlyInDebugMode = false)
        {
            throw new NotImplementedException();
        }
        
        // TODO: override WriteToConsole with color and...
        
    }
}
