using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.Environments.GingerAnalyticsEnvWizardLib
{
    public class GingerAnalyticsAPIResponseInfo
    {
        public class GingerAnalyticsEnvironmentA
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("parameter")]
            public List<GAEnvParameter> EnvParameters { get; set; }

        }

        public class GingerAnalyticsEnvironmentB
        {
            [JsonProperty("environmentId")]
            public string Id { get; set; }

            [JsonProperty("environmentName")]
            public string Name { get; set; }

            [JsonProperty("application")]
            public List<GingerAnalyticsApplication> GingerAnalyticsApplications { get; set; }

        }


        public class GingerAnalyticsArchitectureA
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }


        }

        public class GingerAnalyticsArchitectureB
        {
            [JsonProperty("architectureId")]
            public string Id { get; set; }

            [JsonProperty("architectureName")]
            public string Name { get; set; }

            [JsonProperty("environment")]
            public List<GingerAnalyticsEnvironmentA> GingerAnalyticsEnvironment { get; set; }


        }

        public class GingerAnalyticsProject
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("architecture")]
            public List<GingerAnalyticsArchitectureA> GingerAnalyticsArchitecture { get; set; }
        }

        public class GingerAnalyticsApplication
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("parameter")]
            public List<GAApplicationParameter> GAApplicationParameters { get; set; }
        }
        public class GAApplicationParameter
        {
            [JsonProperty("value")]
            public string Value { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class GAEnvParameter
        {
            [JsonProperty("value")]
            public string Value { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class Root
        {
            public List<GingerAnalyticsProject> GingerAnalyticsProject { get; set; }
        }

    }
}
