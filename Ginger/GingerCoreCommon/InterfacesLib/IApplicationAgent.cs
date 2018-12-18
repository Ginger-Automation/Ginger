using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IApplicationAgent
    {
        object AgentName { get; set; }
        IAgent Agent { get; set; }
        string AppName { get; set; }
    }
}
