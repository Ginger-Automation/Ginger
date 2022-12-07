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
using GingerCore.Variables;
using System.IO;

namespace Amdocs.Ginger.CoreNET.TelemetryLib
{
    public class TelemetryRunListener : RunListenerBase
    {
        public override void ActionEnd(uint eventTime, Act action, bool offlineMode = false)
        {
            try
            {
                if (action is ActPlugIn)
                {
                    ActPlugIn actPlugIn = ((ActPlugIn)action);
                    UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.Plugins.ToString(), true, true, actPlugIn.ItemName);
                }

                switch (action.ActionType)
                {
                    case "Data Source Manipulation":
                        {
                            UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.DataSource.ToString(), true, true);
                            break;
                        }
                    case "Web API Model Action":
                        {
                            UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.ApiModel.ToString(), true, true);
                            break;
                        }
                }

                WorkSpace.Instance.Telemetry.TelemetrySession.OverallExecutedActions += 1;

                UsedActionDetail.AddOrModifyActionDetail(action);

                //Finding if Action is using a Global Variable is not working because it does noe find any variable
                var globalVariableName = action.ActInputValues.Where(x => x.Param == "VariableName").FirstOrDefault();
                var variableList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>().Where(x => x.Name == globalVariableName.Value).ToList();
                var allVariableList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>();
                if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>().Where(x => x.MappedOutputType == VariableBase.eOutputType.GlobalVariable && x.Name == globalVariableName.Value).ToList().Count > 0)
                {
                    UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.GlobalVaraibles.ToString(), true, true);
                }

                //Check if the action is from POM Element
                if (action.ActionType.Contains("UI Element Action"))
                {
                    if (((GingerCore.Actions.Common.ActUIElement)action).ElementLocateBy.ToString() == "POMElement")
                    {
                        ObservableList<ApplicationPOMModel> pomModels = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>();
                        foreach (ApplicationPOMModel pomModel in pomModels)
                        {
                            if (pomModel.MappedUIElements.Where(x => ((GingerCore.Actions.Common.ActUIElement)action).ElementLocateValue.Contains(x.Guid.ToString())).FirstOrDefault() != null)
                            {
                                UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.POM.ToString(), true, true, pomModel.ItemImageType.ToString());
                                break;
                            }
                        }
                    }
                }
                
            }
            catch(Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR ,"Failed to report to Telemetry about the ActionEnd", e);
            }
        }


        public override void ActivityEnd(uint eventTime, Activity activity, bool offlineMode = false)
        {
            WorkSpace.Instance.Telemetry.TelemetrySession.OverallExecutedActivities += 1;
            try
            {
                WorkSpace.Instance.Telemetry.TelemetrySession.ExecutedAutomatedPlatforms.Add(getAgentPlatform(activity).ToString());
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to report to Telemetry about the ActivityEnd", e);
            }
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
            try
            {
                WorkSpace.Instance.Telemetry.TelemetrySession.OverallExecutedActivityGroups += 1;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to report to Telemetry about the ActivityGroupEnd", e);
            }
        }

        public override void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            try
            {
                WorkSpace.Instance.Telemetry.TelemetrySession.OverallExecutedBusinessFlows += 1;
                WorkSpace.Instance.Telemetry.TelemetrySession.OverallExecutionTimeNumber += businessFlow.Elapsed;

                if (businessFlow.RunStatus == eRunStatus.Passed)
                {
                    WorkSpace.Instance.Telemetry.TelemetrySession.PassedBusinessFlowsCount += 1;
                }
                if (businessFlow.RunStatus == eRunStatus.Failed)
                {
                    WorkSpace.Instance.Telemetry.TelemetrySession.FailedBusinessFlowsCount += 1;
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to report to Telemetry about the BusinessFlowEnd", e);
            }
        }

        public override void RunnerRunEnd(uint eventTime, GingerRunner gingerRunner, string filename = null, int runnerCount = 0, bool offlineMode = false)
        {
            try
            {
                WorkSpace.Instance.Telemetry.TelemetrySession.OverallExecutedRunsets += 1;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to report to Telemetry about the RunnerRunEnd", e);
            }
        }



    }
}
