using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using GingerCore.Variables;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public enum eStatusConverterOptions
    {
        [EnumValueDescription("None")]
        None = 0,
        [EnumValueDescription("Always Passed")]
        AlwaysPass = 1,
        [EnumValueDescription("Ignore Failed")]
        IgnoreFail = 2,
        [EnumValueDescription("Invert Status")]
        InvertStatus = 3,
    }
    public interface IAct
    {
       
  
        Guid Guid { get; set; }
        string ItemName { get; set; }
        string Description { get; set; }
        string ActionType { get;  }

        string RunDescription { get; set; }
         DateTime StartTimeStamp { get; set; }
         DateTime EndTimeStamp { get; set; }
         int RetryMechanismCount { get; set; }
        long? Elapsed { get; set; }
        eStatusConverterOptions StatusConverter { get; set; }
        eRunStatus? Status { get; set; }
        string Error { get; set; }
        string ExInfo { get; set; }
        ObservableList<ActInputValue> InputValues { get; set; }
        ObservableList<ActReturnValue> ReturnValues { get;  }
        ObservableList<IFlowControl> FlowControls { get; set; }
        List<String> ScreenShots  { get; set; }
        bool Active { get; set; }
        bool FailIgnored { get; set; }
        ObservableList<ActReturnValue> ActReturnValues { get;  }
        string LocateValue { get; set; }

        List<ObservableList<ActInputValue>> GetInputValueListForVEProcessing();

        ObservableList<VariableDependency> VariablesDependencies { get; set; }
        bool SupportSimulation { get; set; }
        String ActionDescription { get;  }
    }
}
