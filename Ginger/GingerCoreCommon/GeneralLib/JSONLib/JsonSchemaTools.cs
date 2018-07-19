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
        /// Genrate sample json From NJsonSchema.Jsonschema4
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public static string JsonSchemaFaker(JsonSchema4 schema, bool UseXMlNames = false)
        {

            if (schema.HasReference)
            {
                return JsonSchemaFaker(schema.Reference, UseXMlNames);
            }

            Dictionary<string, object> JsonBody = new Dictionary<string, object>();

            foreach (KeyValuePair<string, JsonProperty> jkp in schema.ActualProperties)
            {
                string key = jkp.Key;
                if (UseXMlNames && jkp.Value.Xml != null)
                {
                    key = jkp.Value.Xml.Name;
                }
                if (jkp.Value.HasReference)
                {
                    JsonBody.Add(key, JsonSchemaFaker(jkp.Value.Reference, UseXMlNames));
                }
                else
                {
                    JsonBody.Add(key, GenerateJsonObjectFromJsonSchema4(jkp.Value, UseXMlNames));
                }

            }



            return JsonConvert.SerializeObject(JsonBody);
        }


        private static object GenerateJsonObjectFromJsonSchema4(JsonProperty value, bool UseXMlNames)
        {


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
                        JsonBody.Add(key, JsonConvert.SerializeObject(GenerateJsonObjectFromJsonSchema4(jkp.Value, UseXMlNames)));

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


                        jb.Add(key, JsonConvert.SerializeObject(GenerateJsonObjectFromJsonSchema4(item.Value, UseXMlNames)));
                    }
                    if (value.Item.HasReference)
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
                                if (key.ToUpper() == "orderTerm".ToUpper())
                                {

                                }
                                if (UseXMlNames && item.Value.Xml != null)
                                {
                                    key = item.Value.Xml.Name;
                                }


                                jb.Add(key, JsonConvert.SerializeObject(GenerateJsonObjectFromJsonSchema4(item.Value, UseXMlNames)));
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


            return output;
        }
    }
}
