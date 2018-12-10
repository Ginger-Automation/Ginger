using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.CoreNET.Execution;
using Ginger.Run;

namespace Amdocs.Ginger.Common.InterfacesLib
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
        IExecutionLogger ExecutionLogger { get; }
        object CurrentSolution { get; set; }
        ObservableList<IAgent> SolutionAgents { get; set; }
        ObservableList<IDataSourceBase> DSList { get; set; }
        object SolutionApplications { get; set; }
        object SolutionFolder { get; set; }
        eRunStatus Status { get; set; }
        ObservableList <IBusinessFlow> BusinessFlows { get; set; }
        IEnumerable<BusinessFlowRun> BusinessFlowsRunList { get; }
        string Name { get; set; }
        bool UseSpecificEnvironment { get; set; }
        object ProjEnvironment { get; set; }
        Guid Guid { get; }
        IEnumerable<object> ApplicationAgents { get; set; }
        bool IsRunning { get; set; }
        
        ObservableList<BusinessFlowExecutionSummary> GetAllBusinessFlowsExecutionSummary(bool GetSummaryOnlyForExecutedFlow = false, string GingerRunnerName = "");
        void SetExecutionEnvironment(IProjEnvironment runsetExecutionEnvironment, ObservableList<IProjEnvironment> observableList);
        void RunRunner();
        //void ContinueRun(object runner, object lastStoppedAction);
        void ResetRunnerExecutionDetails(bool doNotResetBusFlows);
        void ResetRunnerExecutionDetails();
        void CloseAgents();
        void StopRun();
        void ContinueRun(object runner, eContinueFrom lastStoppedAction);
        void UpdateApplicationAgents();
    }
}
