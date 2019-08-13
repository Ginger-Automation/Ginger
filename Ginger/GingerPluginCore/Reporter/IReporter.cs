using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core.Reporter
{
    public enum eLogLevel
    {
        DEBUG, INFO, WARN, ERROR, FATAL
    }
    public enum eUserMsgSelection
    {
        None,
        Yes,
        No,
        Cancel,
        OK
    }
    public interface IReporter
    {
         void ToLog2(eLogLevel logLevel, string messageToLog, Exception exceptionToLog = null);
        
    }
}
