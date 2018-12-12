using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
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


        List<PluginServiceActionInfo> mActions = new List<PluginServiceActionInfo>();

        [JsonProperty]
        public List<PluginServiceActionInfo> Actions { get { return mActions; } }

      
    }
}
