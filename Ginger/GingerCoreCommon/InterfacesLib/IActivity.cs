using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.InterfacesLib;
#region License
/*
Copyright © 2014-2018 European Support Limited

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
    public enum eHandlerMappingType
    {
        [EnumValueDescription("All Available Error Handlers ")]
        AllAvailableHandlers = 0,
        None = 1,
        [EnumValueDescription("Specific Error Handlers")]
        SpecificErrorHandlers = 2
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
        IAgent CurrentAgent { get; set; }
        string TargetApplication { get; set; }

        ObservableList<VariableBase> GetVariables();
        ObservableList<VariableDependency> VariablesDependencies { get; set; }
        string ExternalID { get; set; }
        string FileName { get; set; }
        ObservableList<Guid> Tags { get; set; }
        ObservableList<Guid> MappedErrorHandlers { get; set; }
        eHandlerMappingType ErrorHandlerMappingType { get; set; }
        bool SelectedForConversion { get; set; }

        void AddVariable(VariableBase stepActivityVar);
    }
}
