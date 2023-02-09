#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

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
using GingerCore.Actions.PlugIns;

namespace Amdocs.Ginger.CoreNET.TelemetryLib
{
    public class TelemetryRunListener : RunListenerBase
    {
        public override void ActionEnd(uint eventTime, Act action, bool offlineMode = false)
        {
            if (action is ActPlugIn)
            {
                ActPlugIn actPlugIn = ((ActPlugIn)action);


                WorkSpace.Instance.Telemetry.Add("actionend",
                new
                {
                    ActionType = action.ActionType,
                    Guid = action.Guid,
                    Name = action.GetType().Name,
                    Elasped = action.Elapsed,
                    Status = action.Status.ToString(),
                    Plugin = actPlugIn.PluginId,
                    ServiceId = actPlugIn.ServiceId,
                    ActionID = actPlugIn.ActionId
                });
            }
            else
            {
                WorkSpace.Instance.Telemetry.Add("actionend",
                new
                {
                    ActionType = action.ActionType,
                    Guid = action.Guid,
                    Name = action.GetType().Name,
                    Elasped = action.Elapsed,
                    Status = action.Status.ToString()                    
                });
            }
                
        }
        

        public override void ActivityEnd(uint eventTime, Activity activity, bool offlineMode = false)
        {
            WorkSpace.Instance.Telemetry.Add("activityend",
                new
                {
                    Guid = activity.Guid,
                    Elapsed = activity.Elapsed,
                    Platform = getAgentPlatform(activity),
                    ActionCount = activity.Acts.Count,
                    ActionsPass = (from x in activity.Acts where x.Status == Execution.eRunStatus.Passed select x).Count(),
                    ActionsFail = (from x in activity.Acts where x.Status == Execution.eRunStatus.Failed select x).Count(),
                    Status = activity.Status.ToString()
                });
        }

        private object getAgentPlatform(Activity activity)
        {
            if (activity.CurrentAgent != null)
            {
                return activity.CurrentAgent.Platform;
            }
            else
            {
                return null;
            }
                
        }

        public override void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup, bool offlineMode = false)
        {
            WorkSpace.Instance.Telemetry.Add("activitygroupend", new { Guid = activityGroup.Guid, Elapsed = activityGroup.Elapsed });
        }

        public override void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            WorkSpace.Instance.Telemetry.Add("businessflowend", new { Guid = businessFlow.Guid, Elapsed = businessFlow.Elapsed, Status = businessFlow.RunStatus.ToString() });
        }
        
        public override void RunnerRunEnd(uint eventTime, GingerRunner gingerRunner, string filename = null, int runnerCount = 0, bool offlineMode = false)
        {
            WorkSpace.Instance.Telemetry.Add("runnerrunend", new { Guid = gingerRunner.Guid, Elapsed = gingerRunner.Executor.Elapsed, Status = gingerRunner.Status });
        }
        


    }
}
