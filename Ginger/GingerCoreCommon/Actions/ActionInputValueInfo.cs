using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Amdocs.Ginger.Plugin.Core;
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
            //We need to handle each new param type in Plugin Edit page 

            get
            {
                // return only know types else throw
                if (ParamType == typeof(string)) return "string";
                if (ParamType == typeof(Int32)) return "int";
                if (ParamType == typeof(List<string>)) return "List<string>";
                if (ParamType == typeof(IGingerAction)) return "IGingerAction";


                // Check if it is a List 
                if (ParamType.IsGenericType && ParamType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type itemType = ParamType.GetGenericArguments()[0];
                    string listTypeInfo = "List<" + itemType.FullName + ">" + GetListItemProperties(itemType);
                    return listTypeInfo;
                }

                //return ParamType.Name;
                throw new Exception("Unknown param type to handle: " + ParamType.FullName);
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
                    case "List<string>":
                        ParamType = typeof(List<string>);
                        break;
                        // TODO: the rest
                }
            }
        }

        private string GetListItemProperties(Type itemType)
        {
            string s = "";
            foreach(PropertyInfo PI in itemType.GetProperties())
            {
                if (s.Length > 0) s += ",";
                s += PI.Name + ":" + PI.PropertyType.Name;
            }            
            return "{Properties=" + s + "}" ;
        }

        public Type ParamType { get; set; }
        
        
    }
}
