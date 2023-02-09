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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using GingerCore.Variables;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore;
using GingerCore.FlowControlLib;

namespace Ginger.Run
{
    //This Class is for storing the Business Flow executed, it can have different Run Description, and a copy of the original BF
    // It is save as part of RunSetConfig
    public class BusinessFlowExecutionSummary : RepositoryItemBase
    {       
       
        public BusinessFlowExecutionSummary()
        {
        }
        //TODO: [IsSerializedForLocalRepository] for the BF need to include also the action status elapsed etc = FULL save

        [IsSerializedForLocalRepository]
        public string BusinessFlowName { get; set; }
        [IsSerializedForLocalRepository]
        public string BusinessFlowRunDescription { get; set; }
        [IsSerializedForLocalRepository]
        public int Activities { get; set; }
        [IsSerializedForLocalRepository]
        public int Actions { get; set; }
        [IsSerializedForLocalRepository]
        public int Validations { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<VariableBase> ExecutionVariabeles = new ObservableList<VariableBase>();

        [IsSerializedForLocalRepository]
        public ObservableList<FlowControl> ExecutionBFFlowControls = new ObservableList<FlowControl>();
        
        public string PublishStatus 
        { 
            get
            {
                if (BusinessFlow != null)
                    return BusinessFlow.GetPublishStatusString();
                else
                    return "Not Published";
            }
        }        


        [IsSerializedForLocalRepository]
        public Amdocs.Ginger.CoreNET.Execution.eRunStatus Status { get; set; }
        
        public BusinessFlow BusinessFlow { get; set; }        

        public bool Selected { get; set; }
        
        public string BusinessFlowExecLoggerFolder { get; set; }

        public bool BusinessFlowExecLoggerPopulated
        {
            get
            {
                return BusinessFlowExecLoggerFolder == null || BusinessFlowExecLoggerFolder == string.Empty ? false : true;
            }
        }

        public override  string GetNameForFileName()
        {
            return BusinessFlowName;
        }

        public string CurrentActivity
        {
            get 
            {
                if (BusinessFlow != null && BusinessFlow.CurrentActivity != null)
                    return BusinessFlow.CurrentActivity.ActivityName;
                else
                    return string.Empty;
            }
        }

        public string CurrentAction
        {
            get 
            {
                if (BusinessFlow != null && BusinessFlow.CurrentActivity != null && BusinessFlow.CurrentActivity.Acts != null && BusinessFlow.CurrentActivity.Acts.CurrentItem != null)
                    return ((IAct)(BusinessFlow.CurrentActivity.Acts.CurrentItem)).Description;
                else
                    return string.Empty;
            }
        }
        
        public string GingerRunnerName { get; set; }

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

        public override string GetItemType()
        {
            return nameof(BusinessFlowExecutionSummary);
        }
    }
}
