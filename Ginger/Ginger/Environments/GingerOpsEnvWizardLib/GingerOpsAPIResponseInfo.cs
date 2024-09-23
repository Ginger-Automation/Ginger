using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
