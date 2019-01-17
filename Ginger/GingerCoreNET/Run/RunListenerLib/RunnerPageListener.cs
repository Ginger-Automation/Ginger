using Amdocs.Ginger.Run;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using System;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib
{
    public class RunnerPageListener : RunListenerBase
    {
        public EventHandler UpdateStat;
        public override void GiveUserFeedback(uint eventTime)
        {          
            UpdateStat.Invoke(this, null);
        }
       

       
    }
}
