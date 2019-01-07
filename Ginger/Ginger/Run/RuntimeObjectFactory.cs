using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.InterfacesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.Run
{
    public class RuntimeObjectFactory : IRuntimeObjectFactory
    {


        public IGingerRunner RunExecutioFrom(Amdocs.Ginger.Common.eExecutedFrom executedFrom)
        {
            return new GingerRunner(executedFrom);
        }
        public IGingerRunner CreateGingerRunner()
        {
            return new GingerRunner();
        }

    }
}
