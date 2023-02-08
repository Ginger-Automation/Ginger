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

using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Execution;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using Newtonsoft.Json;

namespace Amdocs.Ginger.CoreNET.Run.ExecutionSummary
{
    public class ExecutionSummary
    {
       public string SummaryCreationTime { get; set; }

        //public DateTime StartTime { get; set; } //we missing that data currently- will be added later
        //public DateTime EndTime { get; set; }
        public string ExecutionElapsed { get; set; }

        public Runners Runners { get;  } = new Runners();
        public BusinessFlowsSummary BusinessFlowsSummary { get; } = new BusinessFlowsSummary();
        public ActivitiesSummary ActivitiesSummary { get; } = new ActivitiesSummary();
        public ActionsSummary ActionsSummary { get; } = new ActionsSummary();

        RunsetExecutor mRunsetExecutor;
        public string Create(RunsetExecutor runsetExecutor)
        {
            SummaryCreationTime = DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss");
            ExecutionElapsed = runsetExecutor.Elapsed.ToString(@"hh\:mm\:ss");            
            mRunsetExecutor = runsetExecutor;

            AddRunners();

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            return json;
        }

        private void AddRunners()
        {
            foreach (GingerRunner runner in mRunsetExecutor.Runners)
            {
                Runners.Total++;
                Runners.Parallel = mRunsetExecutor.RunSetConfig.RunModeParallel;
                AddBusinessFlows(runner.Executor.BusinessFlows);                
            }
        }

        private void AddBusinessFlows(ObservableList<BusinessFlow> businessFlows)
        {
            foreach (BusinessFlow businessFlow in businessFlows)
            {
                BusinessFlowsSummary.Total++;
                switch (businessFlow.RunStatus)
                {
                    case eRunStatus.Passed:
                        BusinessFlowsSummary.Pass++;
                        break;
                    case eRunStatus.Failed:
                        BusinessFlowsSummary.Fail++;
                        break;
                    case eRunStatus.Blocked:
                        BusinessFlowsSummary.Blocked++;
                        break;                    
                    default:
                        // Err !!!!!!!!!!! or Reporter
                        break;
                }
                AddActivities(businessFlow.Activities);
            }
        }

        private void AddActivities(ObservableList<Activity> activities)
        {
            foreach (Activity activity in activities)
            {
                ActivitiesSummary.Total++;
                switch (activity.Status)
                {
                    case eRunStatus.Passed:
                        ActivitiesSummary.Pass++;
                        break;
                    case eRunStatus.Failed:
                        ActivitiesSummary.Fail++;
                        break;
                    case eRunStatus.Blocked:
                        ActivitiesSummary.Blocked++;
                        break;
                    default:
                        // Err !!!!!!!!!!! or Reporter
                        break;
                }
                AddActions(activity.Acts);
            }
        }


        private void AddActions(ObservableList<IAct> actions)
        {
            foreach (Act act in actions)
            {
                ActionsSummary.Total++;
                switch (act.Status)
                {
                    case eRunStatus.Passed:
                        ActionsSummary.Pass++;
                        break;
                    case eRunStatus.Failed:
                        ActionsSummary.Fail++;
                        break;
                    case eRunStatus.Blocked:
                        ActionsSummary.Blocked++;
                        break;
                    default:
                        // Err !!!!!!!!!!! or Reporter
                        break;
                }
            }
        }



    }
}
