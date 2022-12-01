#region License
/*
Copyright © 2014-2022 European Support Limited

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
using static GingerCore.BusinessFlow;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;

namespace Amdocs.Ginger.CoreNET.TelemetryLib
{
    public class TelemetryRunListener : RunListenerBase
    {
        public override void ActionEnd(uint eventTime, Act action, bool offlineMode = false)
        {
            //if (action is ActPlugIn)
            //{
            //    ActPlugIn actPlugIn = ((ActPlugIn)action);


            //    WorkSpace.Instance.Telemetry.Add("actionend",
            //    new
            //    {
            //        ActionType = action.ActionType,
            //        Guid = action.Guid,
            //        Name = action.GetType().Name,
            //        Elasped = action.Elapsed,
            //        Status = action.Status.ToString(),
            //        Plugin = actPlugIn.PluginId,
            //        ServiceId = actPlugIn.ServiceId,
            //        ActionID = actPlugIn.ActionId
            //    });
            //}
            //else
            //{
            //    WorkSpace.Instance.Telemetry.Add("actionend",
            //    new
            //    {
            //        ActionType = action.ActionType,
            //        Guid = action.Guid,
            //        Name = action.GetType().Name,
            //        Elasped = action.Elapsed,
            //        Status = action.Status.ToString()                    
            //    });
            //}

            if (action is ActPlugIn)
            {
                ActPlugIn actPlugIn = ((ActPlugIn)action);
                UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.Plugins.ToString(), true, true, actPlugIn.ItemName);
            }

            WorkSpace.Instance.Telemetry.TelemetrySession.OVerallExecutedActions += 1;

            UsedActionDetail usedActionDetail = WorkSpace.Instance.Telemetry.TelemetrySession.UsedActionTypes.Where(x => x.Name == action.ActionType).FirstOrDefault();
            if (usedActionDetail != null)
            {
                int index = WorkSpace.Instance.Telemetry.TelemetrySession.UsedActionTypes.IndexOf(usedActionDetail);
                usedActionDetail.CountTotal += 1;
                if (action.Status == eRunStatus.Passed)
                {
                    WorkSpace.Instance.Telemetry.TelemetrySession.PassedActionsCount += 1;
                    usedActionDetail.CountPassed += 1;
                }
                if (action.Status == eRunStatus.Failed)
                {
                    WorkSpace.Instance.Telemetry.TelemetrySession.FailedActionsCount += 1;
                    usedActionDetail.CountFailed += 1;
                }
                WorkSpace.Instance.Telemetry.TelemetrySession.UsedActionTypes[index] = usedActionDetail;

            }
            else
            {
                usedActionDetail = new UsedActionDetail(action.ActionType, 1, 0, 0);



                if (action.Status == eRunStatus.Passed)
                {
                    WorkSpace.Instance.Telemetry.TelemetrySession.PassedActionsCount += 1;
                    usedActionDetail.CountPassed += 1;
                }
                if (action.Status == eRunStatus.Failed)
                {
                    WorkSpace.Instance.Telemetry.TelemetrySession.FailedActionsCount += 1;
                    usedActionDetail.CountFailed += 1;
                }
                WorkSpace.Instance.Telemetry.TelemetrySession.UsedActionTypes.Add(usedActionDetail);
            }
            if (action.ActionType.Contains("API"))
            {
                UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.ApiModel.ToString(), true, true);
            }
            try
            {
                if (((GingerCore.Actions.Common.ActUIElement)action).ElementLocateBy.ToString() == "POMElement")
                {
                    ObservableList<ApplicationPOMModel> pomModels = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>();
                    foreach (ApplicationPOMModel pomModel in pomModels)
                    {
                        //var appPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(pomModel.TargetApplicationKey);
                        if (pomModel.MappedUIElements.Where(x => ((GingerCore.Actions.Common.ActUIElement)action).ElementLocateValue.Contains(x.Guid.ToString())).FirstOrDefault() != null)
                        {
                            UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.POM.ToString(), true, true, pomModel.ItemImageType.ToString());
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }



            //if (WorkSpace.Instance.Telemetry.TelemetrySession.UsedActionTypes.ContainsKey(action.ActionType))
            //{
            //    WorkSpace.Instance.Telemetry.TelemetrySession.UsedActionTypes[action.ActionType.ToString()] += 1;
            //}
            //else
            //{
            //    WorkSpace.Instance.Telemetry.TelemetrySession.UsedActionTypes[action.ActionType] = 1;
            //}
        }


        public override void ActivityEnd(uint eventTime, Activity activity, bool offlineMode = false)
        {
            //WorkSpace.Instance.Telemetry.Add("activityend",
            //    new
            //    {
            //        Guid = activity.Guid,
            //        Elapsed = activity.Elapsed,
            //        Platform = getAgentPlatform(activity),
            //        ActionCount = activity.Acts.Count,
            //        ActionsPass = (from x in activity.Acts where x.Status == Execution.eRunStatus.Passed select x).Count(),
            //        ActionsFail = (from x in activity.Acts where x.Status == Execution.eRunStatus.Failed select x).Count(),
            //        Status = activity.Status.ToString()
            //    });

            WorkSpace.Instance.Telemetry.TelemetrySession.OVerallExecutedActivities += 1;
            WorkSpace.Instance.Telemetry.TelemetrySession.AutomatedPlatforms.Add(getAgentPlatform(activity).ToString());
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
            //WorkSpace.Instance.Telemetry.Add("activitygroupend", new { Guid = activityGroup.Guid, Elapsed = activityGroup.Elapsed });
            WorkSpace.Instance.Telemetry.TelemetrySession.OverallExecutedActivityGroups += 1;
        }

        public override void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            //WorkSpace.Instance.Telemetry.Add("businessflowend", new { Guid = businessFlow.Guid, Elapsed = businessFlow.Elapsed, Status = businessFlow.RunStatus.ToString() });
            WorkSpace.Instance.Telemetry.TelemetrySession.OVerallExecutedBuisnessFlows += 1;
            WorkSpace.Instance.Telemetry.TelemetrySession.OverallExecutionTimeNumber += businessFlow.Elapsed;

            if (businessFlow.RunStatus == eRunStatus.Passed)
            {
                WorkSpace.Instance.Telemetry.TelemetrySession.PassedBuisnessFlowsCount += 1;
            }
            if (businessFlow.RunStatus == eRunStatus.Failed)
            {
                WorkSpace.Instance.Telemetry.TelemetrySession.FailedBuisnessFlowsCount += 1;
            }
        }

        public override void RunnerRunEnd(uint eventTime, GingerRunner gingerRunner, string filename = null, int runnerCount = 0, bool offlineMode = false)
        {
            //WorkSpace.Instance.Telemetry.Add("runnerrunend", new { Guid = gingerRunner.Guid, Elapsed = gingerRunner.Executor.Elapsed, Status = gingerRunner.Status });
            WorkSpace.Instance.Telemetry.TelemetrySession.OverallExecutedRunsets += 1;
        }



    }
}
