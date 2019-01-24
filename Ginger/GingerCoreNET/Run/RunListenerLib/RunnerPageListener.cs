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
        public override void ActionStart(uint eventTime, Act action)
        {
            UpdateStat.Invoke(this, null);
        }
        public override void ActionEnd(uint eventTime, Act action, bool offlineMode = false)
        {
            UpdateStat.Invoke(this, null);
        }

        public override void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false)
        {
            UpdateStat.Invoke(this, null);
        }

        public override void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            UpdateStat.Invoke(this, null);
        }

        public override void ActivityStart(uint eventTime, Activity activity, bool continuerun=false)
        {
            UpdateStat.Invoke(this, null);
        }

        public override void ActivityEnd(uint eventTime, Activity activity, bool offlineMode= false)
        {
            UpdateStat.Invoke(this, null);
        }

        public override void RunnerRunStart(uint eventTime, GingerRunner gingerRunner)
        {
            UpdateStat.Invoke(this, null);
        }

        public override void RunnerRunEnd(uint eventTime, GingerRunner gingerRunner, string filename = null, int runnerCount = 0)
        {
            UpdateStat.Invoke(this, null);
        }

        public override void DynamicActivityWasAddedToBusinessflow(uint eventTime, BusinessFlow businessFlow)
        {
            UpdateStat.Invoke(this, null);
        }

       
    }
}
