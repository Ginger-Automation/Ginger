using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.InterfacesLib
{
    public interface IRuntimeObjectFactory
    {
        IGingerRunner RunExecutioFrom(eExecutedFrom eExecutedFrom);

        IGingerRunner CreateGingerRunner();
    }

}
