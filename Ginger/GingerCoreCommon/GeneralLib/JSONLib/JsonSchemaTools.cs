#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Newtonsoft.Json.Linq;
using NJsonSchema;
using System.Collections.Generic;

namespace Amdocs.Ginger.Common
{
    public class JsonSchemaTools
    {
        /// <summary>
        /// Generate sample json From NJsonSchema.Jsonschema4
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        /// 

        private static Dictionary<object, object> CachedValues = new Dictionary<object, object>();
        public static string JsonSchemaFaker(JsonSchema4 schema, List<object> ReferenceStack, bool UseXMlNames = false)
        {
            if (ReferenceStack == null)
            {
                ReferenceStack = new List<object>();
            }
            ReferenceStack.Add(schema);
            if (CachedValues.ContainsKey(schema))
            {
                object returnValue;
                CachedValues.TryGetValue(schema, out returnValue);
                return (string)returnValue;
            }


      

            if (schema.HasReference)
            {

                if (ReferenceStack.Contains(schema.Reference))
                {
                    return string.Empty;
                }
                else
                {
                    
                    return JsonSchemaFaker(schema.Reference, ReferenceStack, UseXMlNames);
                }


            }



            Dictionary<string, object> JsonBody = new Dictionary<string, object>();
            foreach (KeyValuePair<string, JsonProperty> jkp in schema.ActualProperties)
            {
                //code
                string key = jkp.Key;
                if (UseXMlNames && jkp.Value.Xml != null)
                {
                    key = jkp.Value.Xml.Name;
                }
                if (jkp.Value.HasReference && !ReferenceStack.Contains(jkp.Value.Reference))
                {
                    if (jkp.Value.Reference.HasReference == false && jkp.Value.Reference.Properties.Count == 0)
                    {

                        object o1 = GenerateJsonObjectFromJsonSchema4(jkp.Value, ReferenceStack, UseXMlNames);
                        JsonBody.Add(key, o1);
                    }
                    else
                    {
                        if (!jkp.Value.Reference.Equals(jkp.Value))
                        {
                            string property = JsonSchemaFaker(jkp.Value.Reference, ReferenceStack, UseXMlNames);
                            object o = JsonConvert.DeserializeObject(property);
                            JsonBody.Add(key, o);
                        }
                    }
                }
                else
                {
                    object o = GenerateJsonObjectFromJsonSchema4(jkp.Value, ReferenceStack, UseXMlNames);
                    JsonBody.Add(key, o);
                }

            }

                ReferenceStack.Remove(schema);

            string output = JsonConvert.SerializeObject(JsonBody);
            CachedValues.Add(schema, output);
            return output;
        }


        private static object GenerateJsonObjectFromJsonSchema4(JsonProperty value, List<object> ReferenceStack, bool UseXMlNames)
        {

            if(CachedValues.ContainsKey(value))
            {
                object returnValue;
                CachedValues.TryGetValue(value,out returnValue);
                return returnValue;
            }
            if (ReferenceStack == null)
            {
                ReferenceStack = new List<object>();
            }

            List<object> PrivateStack = new List<object>();
            object output = "";
            switch (value.Type)
            {
                case JsonObjectType.Object:

                    Dictionary<string, object> JsonBody = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, JsonProperty> jkp in value.ActualProperties)
                    {
                        string key = jkp.Key;
                        if (UseXMlNames && jkp.Value.Xml != null)
                        {
                            key = jkp.Value.Xml.Name;
                        }
                        ReferenceStack.Add(jkp.Value);
                        PrivateStack.Add(jkp.Value);
                        object JObject = GenerateJsonObjectFromJsonSchema4(jkp.Value, ReferenceStack, UseXMlNames);
                        JsonBody.Add(key, JsonConvert.SerializeObject(JObject));
                    
                    }
                    output = JsonBody;
                    break;
                case JsonObjectType.Array:



                    JObject jb = new JObject();
                    foreach (var item in value.Item.ActualProperties)
                    {
                        string key = item.Key;
                        if (UseXMlNames && item.Value.Xml != null)
                        {
                            key = item.Value.Xml.Name;
                        }
                        ReferenceStack.Add(item.Value);
                        PrivateStack.Add(item.Value);
                        object JsonObject = GenerateJsonObjectFromJsonSchema4(item.Value, ReferenceStack, UseXMlNames);
                        jb.Add(key, JsonConvert.SerializeObject(JsonObject));
                       
                    }
                    if (value.Item.HasReference)
                    {
                        if (!ReferenceStack.Contains(value.Item.Reference))
                        {

                            foreach (var item in value.Item.Reference.ActualProperties)
                            {
                                if (item.Value.Equals(value))
                                {
                                    jb.Add(item.Key, "");
                                }
                                else
                                {

                                    string key = item.Key;
                                    if (UseXMlNames && item.Value.Xml != null)
                                    {
                                        key = item.Value.Xml.Name;
                                    }

                                    object o = GenerateJsonObjectFromJsonSchema4(item.Value, ReferenceStack, UseXMlNames);

                                    jb.Add(key, (JToken)o);
                                }
                            }
                        }
                    }

                    JArray ja = new JArray();
                    ja.Add(jb);


                    output = ja;
                    if (UseXMlNames)
                    {
                        Dictionary<string, object> jsb = new Dictionary<string, object>();
                        jsb.Add(value.Xml.Name, ja);
                        output = jsb;
                    }
                    break;

                case JsonObjectType.String:
                    output = new JValue("sample");
                    break;
                case JsonObjectType.Number:
                    output = new JValue(1);
                    break;
                case JsonObjectType.Integer:
                    output = new JValue(1);
                    break;
                case JsonObjectType.Boolean:
                    output = new JValue(false);
                    break;
                case JsonObjectType.Null:
                    output = JValue.CreateNull();
                    break;

                default:
                    output = new JValue(""); ;
                    break;

            }

            foreach(object obj in PrivateStack)
            {
                ReferenceStack.Remove(obj);
            }
            CachedValues.Add(value, output);
            return output;
        }
    }
}
