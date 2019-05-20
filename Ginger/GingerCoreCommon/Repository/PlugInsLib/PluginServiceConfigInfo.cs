using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Amdocs.Ginger.Common.Repository.PlugInsLib
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PluginServiceConfigInfo
    {

        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Description { get; set; }
        [JsonProperty]
        public string Type { get; set; }
        [JsonProperty]
        public string DefaultValue { get; set; }
        [JsonProperty]
        public List<string> OptionalValues { get; set; } = new List<string>();
    }
}
