using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.RunLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib
{

    /// <summary>
    /// We have an interface for node comm monitor so we can have nice UI fwhen running from WPF but will be able also to create Monitor dumper for linux to help in debug
    /// </summary>
    public interface IGingerNodeMonitor
    {
        void ShowMonitor(GingerNodeProxy gingerNodeProxy);
        void Add(GingerSocketLog gingerSocketLog);
        void CloseMonitor();
    }
}
