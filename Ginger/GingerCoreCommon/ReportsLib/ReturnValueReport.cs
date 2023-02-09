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
using Newtonsoft.Json;
namespace Ginger.Reports
{
    // Json will serialize onlu marked attr and not all
    [JsonObject(MemberSerialization.OptIn)]
    public class ReturnValueReport
    {
        private ActReturnValue mActReturnValue;

        public ReturnValueReport(ActReturnValue ARV)
        {            
            this.mActReturnValue = ARV;
        }

        public int Seq { get; set; }

        [JsonProperty]
        public string Param { get { return mActReturnValue.Param; } }

        [JsonProperty]
        public string Path { get { return mActReturnValue.Path; } }

        [JsonProperty]
        public string StoreToValue { get { return mActReturnValue.StoreToValue; } }

        [JsonProperty]
        public string Actual { get { return mActReturnValue.Actual; } }

        [JsonProperty]
        public string Expected { get { return mActReturnValue.ExpectedCalculated; } }

        [JsonProperty]
        public string Status { get { return mActReturnValue.Status.ToString() ; } }        
    }
}
