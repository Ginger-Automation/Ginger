using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Amdocs.Ginger.Common.Actions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ActionInputValueInfo
    {
        [JsonProperty]
        public string Param { get; set; }

        [JsonProperty]
        public string ParamTypeName { get { return ParamType.Name; } }

        public Type ParamType { get; set; }
    }
}
