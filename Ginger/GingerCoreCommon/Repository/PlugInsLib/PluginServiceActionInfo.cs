using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Common.Actions;
using Newtonsoft.Json;

namespace Amdocs.Ginger.Common.Repository.PlugInsLib
{

    [JsonObject(MemberSerialization.OptIn)]
    public class PluginServiceActionInfo
    {
        [JsonProperty]
        public string ActionId { get; set; }

        [JsonProperty]
        public string Description { get; set; }

        [JsonProperty]
        public string Interface { get; internal set; }

        [JsonProperty]
        public List<ActionInputValueInfo> InputValues = new List<ActionInputValueInfo>();
    }
}
