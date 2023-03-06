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


using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore.FlowControlLib;
using Newtonsoft.Json;

namespace Ginger.Reports
{
    // Json will serialize onlu marked attr and not all
    [JsonObject(MemberSerialization.OptIn)]
    public class FlowControlReport
    {
        private FlowControl mFlowControl;

        public FlowControlReport(FlowControl ARV)
        {
            mFlowControl = ARV;
        }

        public int Seq { get; set; }

        [JsonProperty]
        public bool Active { get { return mFlowControl.Active; } }

        [JsonProperty]
        public string Condition { get { return mFlowControl.Condition; } }

        [JsonProperty]
        public string ConditionCalculated { get { return mFlowControl.ConditionCalculated; } }
        
        //TODO: add the rest of the properties
    }
}
