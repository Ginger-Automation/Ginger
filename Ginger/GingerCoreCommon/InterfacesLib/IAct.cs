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
using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using GingerCore.FlowControlLib;
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
        int MaxNumberOfRetries { get; set; }
        long? Elapsed { get; set; }
        eStatusConverterOptions StatusConverter { get; set; }
        eRunStatus? Status { get; set; }
        string Error { get; set; }
        string ExInfo { get; set; }
        ObservableList<ActInputValue> InputValues { get; set; }
        ObservableList<ActReturnValue> ReturnValues { get;  }
        ObservableList<FlowControl> FlowControls { get; set; }
        ObservableList<String> ScreenShots  { get; set; }
        bool Active { get; set; }
        bool FailIgnored { get; set; }
        ObservableList<ActReturnValue> ActReturnValues { get;  }
        string LocateValue { get; set; }

        List<ObservableList<ActInputValue>> GetInputValueListForVEProcessing();

        ObservableList<VariableDependency> VariablesDependencies { get; set; }
        bool SupportSimulation { get; set; }
        String ActionDescription { get;  }
        string ExecutionLogFolder { get; set; }
        bool EnableActionLogConfig { get; set; }
        ActionLogConfig ActionLogConfig { get; set; }
        Single? ElapsedSecs { get; }
    }
}
