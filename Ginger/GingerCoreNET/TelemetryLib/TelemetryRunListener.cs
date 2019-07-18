using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Run;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.TelemetryLib
{
    public class TelemetryRunListener : RunListenerBase
    {
        public override void ActionEnd(uint eventTime, Act action, bool offlineMode = false)
        {
            WorkSpace.Instance.Telemetry.SaveTelemetry("actionend", new { ActionType = action.ActionType, Guid = action.Guid, Name = action.GetType().Name, Elasped = action.Elapsed });
        }

        public override void ActivityEnd(uint eventTime, Activity activity, bool offlineMode = false)
        {
            WorkSpace.Instance.Telemetry.SaveTelemetry("activityend",
                new
                {
                    Guid = activity.Guid,
                    Elapsed = activity.Elapsed,
                    Platform = activity.CurrentAgent.Platform,
                    ActionCount = activity.Acts.Count,
                    ActionsPass = (from x in activity.Acts where x.Status == Execution.eRunStatus.Passed select x).Count(),
                    ActionsFail = (from x in activity.Acts where x.Status == Execution.eRunStatus.Failed select x).Count(),
                });
        }

        public override void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup, bool offlineMode = false)
        {
            WorkSpace.Instance.Telemetry.SaveTelemetry("activitygroupend", new { Guid = activityGroup.Guid, Elapsed = activityGroup.Elapsed });
        }

        public override void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            WorkSpace.Instance.Telemetry.SaveTelemetry("businessflowend", new { Guid = businessFlow.Guid, Elapsed = businessFlow.Elapsed });
        }
        
        public override void RunnerRunEnd(uint eventTime, GingerRunner gingerRunner, string filename = null, int runnerCount = 0)
        {
            WorkSpace.Instance.Telemetry.SaveTelemetry("runnerrunend", new { Guid = gingerRunner.Guid, Elapsed = gingerRunner.Elapsed });
        }
        


    }
}
