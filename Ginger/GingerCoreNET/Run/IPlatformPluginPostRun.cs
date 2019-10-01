using System;
using System.Collections.Generic;
using System.Text;
using GingerCore;
using GingerCore.Actions;

namespace Amdocs.Ginger.CoreNET.Run
{
    public interface IPlatformPluginPostRun
    {
        void PostExecute(Agent agent, Act actPlugin);
    }
}
