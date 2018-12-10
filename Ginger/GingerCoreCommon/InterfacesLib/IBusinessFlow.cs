using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Execution;
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

namespace Amdocs.Ginger.Common
{
    public interface IBusinessFlow
    {
        eRunStatus RunStatus { get; set; }

        bool Active { get; set; }
        Guid Guid { get; set; }
        Guid InstanceGuid { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string RunDescription { get; set; }
        string Environment { get; set; }
        Single? ElapsedSecs { get; set; }
        DateTime StartTimeStamp { get; set; }
        DateTime EndTimeStamp { get; set; }
        ObservableList<IActivity> Activities { get; set; }
        ObservableList<IFlowControl> BFFlowControls { get; set; }

        ObservableList<VariableBase> Variables { get; set; }
        IActivity CurrentActivity { get; set; }
        string ExecutionLogFolder { get; set; }
        double? Elapsed { get; set; }
        object Mandatory { get; set; }
        string ExecutionFullLogFolder { get; set; }

        ObservableList<VariableBase> GetVariables();

        VariableBase GetHierarchyVariableByName(string varName, bool considerLinkedVar = true);
        ObservableList<VariableBase> GetSolutionVariables();
        string GetPublishStatusString();
        int GetValidationsCount();
        int GetActionsCount();
        ObservableList<VariableBase> GetBFandActivitiesVariabeles(bool includeParentDetails, bool includeOnlySetAsInputValue = false, bool includeOnlySetAsOutputValue = false);
        IBusinessFlow CreateCopy(bool v);
        void Reset();
        void AttachActivitiesGroupsAndActivities();
    }
}
