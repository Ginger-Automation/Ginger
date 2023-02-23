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

using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;

namespace Ginger.Reports
{
    public interface IReportInfo
    {
        //IEnumerable<ActionReport> AllActionsForReport { get; }
        //IEnumerable<ActivityReport> AllActivitiesForReport { get; }
        IEnumerable<ReturnValueReport> AllValidationsForReport { get; }
        //List<BusinessFlowReport> BusinessFlowReports { get; set; }
        //List<BusinessFlowReport> BusinessFlows { get; }
        string DateCreated { get; set; }
        string DateCreatedShort { get; set; }
        //EnvironmentReport Environment { get; set; }
        TimeSpan ExecutionElapsedTime { get; set; }
        string ExecutionEnv { get; set; }
        int TotalActionsBlockedFromAllFlows { get; }
        int TotalActionsFailedFromAllFlows { get; }
        int TotalActionsPassedFromAllFlows { get; }
        int TotalActionsSkippedFromAllFlows { get; }
        int TotalActivitesBlockedFromAllFlows { get; }
        int TotalActivitesFailedFromAllFlows { get; }
        int TotalActivitesPassedFromAllFlows { get; }
        int TotalActivitesSkippedFromAllFlows { get; }
        int TotalActivitiesCount { get; }
        int TotalBusinessFlows { get; }
        int TotalBusinessFlowsBlocked { get; }
        int TotalBusinessFlowsFailed { get; }
        int TotalBusinessFlowsPassed { get; }
        int TotalBusinessFlowsStopped { get; }
        TimeSpan TotalExecutionTime { get; set; }

        int TotalActionsCount();
        int TotalActionsCountByStatus(eRunStatus Status);
        int TotalActivitiesByRunStatus(eRunStatus RunStatus);
        int TotalValidationsCount();
        int TotalValidationsCountByStatus(ActReturnValue.eStatus Status);
    }
}
