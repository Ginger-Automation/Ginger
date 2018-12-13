using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IApplicationAgent
    {
        string AgentName { get; set; }
        IAgent agent { get; set; }
        string AppName { get; }
    }
}
