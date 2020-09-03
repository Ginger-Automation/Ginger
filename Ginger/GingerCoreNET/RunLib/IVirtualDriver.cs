using GingerCore.Drivers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public interface IVirtualDriver
    {
     
        bool CanStartAnotherInstance(out string errorMessage); 
        void DriverStarted(string AgentGuid);
        void DriverClosed();
    }
}
