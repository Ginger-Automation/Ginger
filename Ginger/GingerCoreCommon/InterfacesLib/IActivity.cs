using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore.Variables;
using Amdocs.Ginger.CoreNET.Execution;
namespace Amdocs.Ginger.Common
{
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

        ObservableList<VariableBase> GetVariables();

    }
}
