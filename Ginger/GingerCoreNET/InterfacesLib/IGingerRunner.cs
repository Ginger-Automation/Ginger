using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace Amdocs.Ginger.CoreNET.InterfacesLib
{
    public enum eContinueLevel
    {
        StandalonBusinessFlow,
        Runner
    }
    public enum eContinueFrom
    {
        LastStoppedAction,
        SpecificAction,
        SpecificActivity,
        SpecificBusinessFlow
    }
    public interface IGingerRunner
    {
        double? Elapsed { get;  }
        ExecutionLogger ExecutionLogger { get; }
        ISolution CurrentSolution { get; set; }
        ObservableList<IAgent> SolutionAgents { get; set; }
        ObservableList<DataSourceBase> DSList { get; set; }
        ObservableList<ApplicationPlatform> SolutionApplications { get; set; }
        string SolutionFolder { get; set; }
        eRunStatus Status { get; set; }
        ObservableList <BusinessFlow> BusinessFlows { get; set; }
        ObservableList<BusinessFlowRun> BusinessFlowsRunList { get; set; }
        string Name { get; set; }
        bool UseSpecificEnvironment { get; set; }
        ProjEnvironment ProjEnvironment { get; set; }
        Guid Guid { get; }
        ObservableList<IApplicationAgent> ApplicationAgents { get; set; }
        bool IsRunning { get; }
        int ExecutionLogBusinessFlowCounter { get; set; }
        string ExecutionLogFolder { get; set; }
     
        eRunStatus RunsetStatus { get;  }

        ObservableList<BusinessFlowExecutionSummary> GetAllBusinessFlowsExecutionSummary(bool GetSummaryOnlyForExecutedFlow = false, string GingerRunnerName = "");
        void SetExecutionEnvironment(ProjEnvironment runsetExecutionEnvironment, ObservableList<ProjEnvironment> observableList);
        void RunRunner(bool run= false);
        //void ContinueRun(object runner, object lastStoppedAction);
        void ResetRunnerExecutionDetails(bool doNotResetBusFlows=false);
        
        void CloseAgents();
        void StopRun();
        bool ContinueRun(eContinueLevel continueLevel, eContinueFrom continueFrom, BusinessFlow specificBusinessFlow = null, Activity specificActivity = null, IAct specificAction = null);
        void UpdateApplicationAgents();
        void CalculateBusinessFlowFinalStatus(BusinessFlow BF, bool considrePendingAsSkipped = false);
        void CalculateActivityFinalStatus(Activity activity);
        void SetActivityGroupsExecutionStatus(BusinessFlow automateTab = null, bool offlineMode = false, ExecutionLogger executionLogger = null);

    }
}
