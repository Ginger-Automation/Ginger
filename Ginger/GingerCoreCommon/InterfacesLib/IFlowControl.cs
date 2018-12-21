using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public enum eStatus
    {
        [EnumValueDescription("Pending")]
        Pending,
        [EnumValueDescription("Action Executed")]
        Action_Executed,
        [EnumValueDescription("Action Not Executed (Condition False)")]
        Action_Not_Executed,
        [EnumValueDescription("Skipped")]
        Skipped,
        [EnumValueDescription("Action Execution Failed (Error)")]
        Action_Execution_Failed,
    }

    public enum eFlowControlAction
    {
        // Put here ONLY items which do flow control like skip actions or goto action etc... all the rest should be regular actions
        // Only actions which move the Instruction pointer of the flow, with one exception of messagebox

        [EnumValueDescription("GoTo Action")]
        GoToAction,
        [EnumValueDescription("GoTo Activity")]
        GoToActivity,
        [EnumValueDescription("GoTo Next Action")]
        GoToNextAction,
        [EnumValueDescription("GoTo Next Activity")]
        GoToNextActivity,
        [EnumValueDescription("Stop Business Flow")]
        StopBusinessFlow,
        [EnumValueDescription("Rerun Activity")]
        RerunActivity,
        [EnumValueDescription("Rerun Action")]
        RerunAction,
        [EnumValueDescription("Show Message Box")]
        MessageBox,
        [EnumValueDescription("Stop Run")]
        StopRun,
        [EnumValueDescription("Set Variable Value")]
        SetVariableValue,
        [EnumValueDescription("Run Shared Repository Activity")]
        RunSharedRepositoryActivity,
        [EnumValueDescription("Fail Action & Stop Business Flow)")]
        FailActionAndStopBusinessFlow,
        [EnumValueDescription("GoTo Activity By Name")]
        GoToActivityByName,
        [EnumValueDescription("Set Failure to be Auto-Opened Defect")]
        FailureIsAutoOpenedDefect
    }
    public enum eBusinessFlowControlAction
    {
        [EnumValueDescription("GoTo Business Flow")]
        GoToBusinessFlow,
        [EnumValueDescription("Rerun Business Flow")]
        RerunBusinessFlow,
        [EnumValueDescription("Stop Runner")]
        StopRun,
        [EnumValueDescription("Set Variable Value")]
        SetVariableValue,
    }


    public interface IFlowControl
    {
        string Condition { get; set; }
        string ConditionCalculated { get; set; }
        eFlowControlAction FlowControlAction { get; set; }
        eStatus Status { get; set; }
        eBusinessFlowControlAction BusinessFlowControlAction { get; set; }
        bool Active { get; set; }

        Guid GetGuidFromValue(bool doNotUseValueCalculated = false);
    }
}
