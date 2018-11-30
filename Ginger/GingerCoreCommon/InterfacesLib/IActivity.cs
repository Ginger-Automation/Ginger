using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore.Variables;
using Amdocs.Ginger.CoreNET.Execution;
namespace Amdocs.Ginger.Common
{
    public enum eActivityAutomationStatus
    {
        Development = 0,
        Automated = 1
    }

    public enum eActionRunOption
    {
        [EnumValueDescription("Stop Actions Run on Failure")]
        StopActionsRunOnFailure = 0,
        [EnumValueDescription("Continue Actions Run on Failure")]
        ContinueActionsRunOnFailure = 1,
    }

    public enum eItemParts
    {
        All,
        Details,
        Actions,
        Variables
    }

    public interface IActivity
    {
        Guid Guid { get; set; }
        bool Active { get; set; }
        string ActivityName { get; set; }
        string Description { get; set; }
        string RunDescription { get; set; }
        DateTime StartTimeStamp { get; set; }
        DateTime EndTimeStamp { get; set; }
        long? Elapsed { get; set; }
        Single? ElapsedSecs { get; }
        eRunStatus? Status { get; set; }
        ObservableList<VariableBase> Variables { get; set; }
        ObservableList<IAct> Acts { get; set; }
        eActivityAutomationStatus? AutomationStatus { get; set; }
        bool AddDynamicly { get; set; }
        Guid ParentGuid { get; set; }
        string ActivitiesGroupID { get; set; }
        string Screen { get; set; }
        string ItemName { get; set; }
        object CurrentAgent { get; set; }

        ObservableList<VariableBase> GetVariables();
        ObservableList<VariableDependency> VariablesDependencies

    }
}
