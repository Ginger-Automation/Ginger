using Amdocs.Ginger.Common.InterfacesLib;
using System;
using System.Collections.Generic;

namespace GingerCore.Platforms
{
    public interface IApplicationAgentOperations
    {
        List<IAgent> PossibleAgents { get; }

    }
}
