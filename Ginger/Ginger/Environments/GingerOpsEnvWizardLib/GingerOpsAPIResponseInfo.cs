#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ginger.Environments.GingerOpsEnvWizardLib
{
    public class GingerOpsAPIResponseInfo
    {
        public class GingerOpsEnvironmentA
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("parameter")]
            public List<GOpsEnvParameter> EnvParameters { get; set; }

        }

        public class GingerOpsEnvironmentB
        {
            [JsonProperty("environmentId")]
            public string Id { get; set; }

            [JsonProperty("environmentName")]
            public string Name { get; set; }

            [JsonProperty("application")]
            public List<GingerOpsApplication> GingerOpsApplications { get; set; }

        }


        public class GingerOpsArchitectureA
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }


        }

        public class GingerOpsArchitectureB
        {
            [JsonProperty("architectureId")]
            public string Id { get; set; }

            [JsonProperty("architectureName")]
            public string Name { get; set; }

            [JsonProperty("environment")]
            public List<GingerOpsEnvironmentA> GingerOpsEnvironment { get; set; }


        }

        public class GingerOpsProject
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("architecture")]
            public List<GingerOpsArchitectureA> GingerOpsArchitecture { get; set; }
        }

        public class GingerOpsApplication
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("parameter")]
            public List<GOpsApplicationParameter> GOpsApplicationParameters { get; set; }
        }
        public class GOpsApplicationParameter
        {
            [JsonProperty("value")]
            public string Value { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class GOpsEnvParameter
        {
            [JsonProperty("value")]
            public string Value { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
}
