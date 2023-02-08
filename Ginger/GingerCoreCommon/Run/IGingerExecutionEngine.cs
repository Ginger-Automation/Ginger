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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Execution;
//using Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Run;
//using Amdocs.Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Environments;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ginger.Run
{
    public interface IGingerExecutionEngine
    {
        ObservableList<BusinessFlow> BusinessFlows { get; set; }
        //ObservableList<BusinessFlowRun> BusinessFlowsRunList { get; set; }
       // AccountReportExecutionLogger Centeralized_Logger { get; }
        IContext Context { get; set; }
        BusinessFlow CurrentBusinessFlow { get; set; }
        ISolution CurrentSolution { get; set; }
        double? Elapsed { get; }
        DateTime EndTimeStamp { get; set; }
        Guid ExecutionId { get; set; }
        int ExecutionLogBusinessFlowsCounter { get; set; }
        string ExecutionLogFolder { get; set; }
        IExecutionLoggerManager ExecutionLoggerManager { get; }
        bool HighLightElement { get; set; }
        bool IsRunning { get; set; }
        //string ItemName { get; set; }
        BusinessFlow LastFailedBusinessFlow { get; set; }
        Guid ParentExecutionId { get; set; }
        BusinessFlow PreviousBusinessFlow { get; set; }
        eRunLevel RunLevel { get; set; }
        //List<IRunListenerBase> RunListeners { get; }
        eRunStatus RunsetStatus { get; }
        //ObservableList<Agent> SolutionAgents { get; set; }
        ObservableList<ApplicationPlatform> SolutionApplications { get; set; }
        string SolutionFolder { get; set; }
        DateTime StartTimeStamp { get; set; }
        int TotalBusinessflow { get; set; }

        void CalculateActionFinalStatus(Act act);
        void CalculateActivitiesGroupFinalStatus(ActivitiesGroup AG, BusinessFlow BF);
        void CalculateActivityFinalStatus(Activity a);
        void CalculateBusinessFlowFinalStatus(BusinessFlow BF, bool considrePendingAsSkipped = false);
        void CheckAndExecutePostErrorHandlerAction();
        void ClearAgents();
        void CloseAgents();
        bool ContinueRun(eContinueLevel continueLevel, eContinueFrom continueFrom, BusinessFlow specificBusinessFlow = null, Activity specificActivity = null, IAct specificAction = null);
        Task<int> ContinueRunAsync(eContinueLevel continueLevel, eContinueFrom continueFrom, BusinessFlow specificBusinessFlow = null, Activity specificActivity = null, Act specificAction = null);
        string GetAgentsNameToRun();
        ObservableList<BusinessFlowExecutionSummary> GetAllBusinessFlowsExecutionSummary(bool GetSummaryOnlyForExecutedFlow = false, string GingerRunnerName = "");
        //List<VariableBase> GetPossibleOutputVariables(RunSetConfig runSetConfig, BusinessFlow businessFlow, bool includeGlobalVars = false, bool includePrevRunnersVars = true);
        void GiveUserFeedback();
        bool GotoNextAction();
        void HighlightActElement(Act act);
        bool IsUpdateBusinessFlowRunList { get; set; }
        void NotifyEnvironmentChanged();
        void PrepActionValueExpression(Act act, BusinessFlow businessflow = null);
        void ProcessInputValueForDriver(Act act);
        void ProcessReturnValueForDriver(Act act);
        void ResetFailedToStartFlagForAgents();
        void ResetRunnerExecutionDetails(bool doNotResetBusFlows = false, bool reSetActionErrorHandlerExecutionStatus = false);
        void ResetStatus(eContinueLevel continueLevel, GingerRunner.eResetStatus resetFrom, BusinessFlow specificBusinessFlow = null, Activity specificActivity = null, Act specificAction = null);
        void RunAction(Act act, bool checkIfActionAllowedToRun = true, bool moveToNextAction = true);
        Task<int> RunActionAsync(Act act, bool checkIfActionAllowedToRun = true, bool moveToNextAction = true);
        void RunActivity(Activity activity, bool doContinueRun = false, bool standaloneExecution = false, bool resetErrorHandlerExecutedFlag = false);
        Task<int> RunActivityAsync(Activity activity, bool Continue = false, bool standaloneExecution = false, bool resetErrorHandlerExecutedFlag = false);
        void RunBusinessFlow(BusinessFlow businessFlow, bool standaloneExecution = false, bool doContinueRun = false, bool doResetErrorHandlerExecutedFlag = false);
        Task<int> RunBusinessFlowAsync(BusinessFlow businessFlow, bool standaloneBfExecution = false, bool doContinueRun = false);
        void RunRunner(bool doContinueRun = false);
        Task<int> RunRunnerAsync();
        //void SetActivityGroupsExecutionStatus(BusinessFlow automateTab = null, bool offlineMode = false, ExecutionLoggerManager ExecutionLoggerManager = null);
        //bool SetBFOfflineData(BusinessFlow BF, IExecutionLoggerManager executionLoggerManager, string logFolderPath);
        void SetBusinessFlowActivitiesAndActionsSkipStatus(BusinessFlow businessFlow = null, bool avoidCurrentStatus = false);
        void SetCurrentActivityAgent();
        void SetExecutionEnvironment(ProjEnvironment defaultEnv, ObservableList<ProjEnvironment> allEnvs);
        void SetNextBusinessFlowsBlockedStatus();
        //void StartAgent(Agent Agent);
        void StopAgents();
        void StopRun();
        void UpdateApplicationAgents();
        void UpdateBusinessFlowsRunList();
    }
}
