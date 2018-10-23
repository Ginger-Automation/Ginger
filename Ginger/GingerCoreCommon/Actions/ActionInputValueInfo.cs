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
        public string ParamTypeName
        {
            get
            {
                return ParamType.Name;
            }
            set
            {
                switch (value)
                {
                    case "string":
                        ParamType = typeof(string);
                        break;
                    case "Int32":
                        ParamType = typeof(Int32);
                        break;
                    // TODO: the rest
                }
            }
        }

        public Type ParamType { get; set; }
    }
}
