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

using System.Collections.Generic;
using Amdocs.Ginger.Common.Repository.PlugInsLib;
using Newtonsoft.Json;

namespace Amdocs.Ginger.Common
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PluginServiceInfo
    {        
        [JsonProperty]
        public string ServiceId { get; set; }
      
        [JsonProperty]
        public string Description { get; set; }

        [JsonProperty]
        public bool IsSession { get; set; }

        [JsonProperty]
        public List<string> Interfaces { get; set; } = new List<string>();


        readonly List<PluginServiceActionInfo> mActions = new List<PluginServiceActionInfo>();

        [JsonProperty]
        public List<PluginServiceActionInfo> Actions { get { return mActions; } }

        [JsonProperty]
        public List<PluginServiceConfigInfo> Configs { get; set; } = new List<PluginServiceConfigInfo>();


    }
}
