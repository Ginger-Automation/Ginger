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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.FlowControlLib;
using GingerCore.Variables;

namespace Ginger.Run
{
    // This class is for selected business flow to run for agent
    // Being saved as part of AgentRunConfig
    public class BusinessFlowRun : RepositoryItemBase
    {
        [IsSerializedForLocalRepository]
        public string BusinessFlowName { get; set; }

        [IsSerializedForLocalRepository]
        public Guid BusinessFlowGuid { get; set; }

        [IsSerializedForLocalRepository]
        public Guid BusinessFlowInstanceGuid { get; set; }
        
        [IsSerializedForLocalRepository]
        public bool BusinessFlowIsActive { get; set; }

		[IsSerializedForLocalRepository]
		public bool BusinessFlowIsMandatory { get; set; }

        /// <summary>
        /// Used by the user to describe the logic of the BF run with a specific set of variables values
        /// </summary>
        [IsSerializedForLocalRepository]
        public string BusinessFlowRunDescription { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<VariableBase> BusinessFlowCustomizedRunVariables = new ObservableList<VariableBase>();

        [IsSerializedForLocalRepository]
        public ObservableList<FlowControl> BFFlowControls = new ObservableList<FlowControl>();

        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {
                return;
            }
        }

        

    }
}
