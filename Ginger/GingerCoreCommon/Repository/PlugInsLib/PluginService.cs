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

        List<PluginServiceAction> mActions = new List<PluginServiceAction>();

        [JsonProperty]
        public List<PluginServiceAction> Actions { get { return mActions; } }
    }
}
