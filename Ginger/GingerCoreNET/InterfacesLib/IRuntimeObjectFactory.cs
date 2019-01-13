using Amdocs.Ginger.Common;
using Ginger.Run;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.InterfacesLib
{

    // Cleanup delete !!!!!!!!!!!!!!!!!
    public interface IRuntimeObjectFactory
    {
        GingerRunner RunExecutioFrom(eExecutedFrom eExecutedFrom);
        
    }

}
