using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET
{
    /// <summary>
    /// This enum is used to hold the status for conversion
    /// </summary>
    public enum eConversionStatus
    {
        Pending,
        Running,
        Stopped,
        Finish,
        Failed
    }
}
