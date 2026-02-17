#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;

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

        private static Dictionary<object, object> CachedValues = [];
        public static string JsonSchemaFaker(JsonSchema4 schema, List<object> ReferenceStack, bool UseXMlNames = false)
        {
            if (schema is null)
            {
                schema = new JsonSchema4();
            }

            if (ReferenceStack == null)
            {
                ReferenceStack = [];
            }

            // Check for circular reference BEFORE adding to stack
            if (ReferenceStack.Contains(schema))
            {
                return "{}";
            }

            ReferenceStack.Add(schema);

            if (CachedValues.ContainsKey(schema))
            {
                object returnValue;
                CachedValues.TryGetValue(schema, out returnValue);
                ReferenceStack.Remove(schema);
                return (string)returnValue;
            }

            if (schema.HasReference)
            {
                if (ReferenceStack.Contains(schema.Reference))
                {
                    ReferenceStack.Remove(schema);
                    return "{}";
                }
                else
                {
                    var result = JsonSchemaFaker(schema.Reference, ReferenceStack, UseXMlNames);
                    ReferenceStack.Remove(schema);
                    return result;
                }
            }

            Dictionary<string, object> JsonBody = [];
            if (schema.AllOf.Count == 0)
            {
                // Safely access ActualProperties with stack overflow protection
                IReadOnlyDictionary<string, JsonProperty> dctJkp;
                try
                {
                    dctJkp = schema.ActualProperties;
                }
                catch (System.StackOverflowException)
                {
                    // If ActualProperties causes stack overflow, return empty object and clean up
                    ReferenceStack.Remove(schema);
                    return "{}";
                }

                if (dctJkp != null && dctJkp.Count == 0)
                {
                    if (schema.Item?.ActualSchema?.ActualProperties != null)
                    {
                        try
                        {
                            var itemProperties = schema.Item.ActualSchema.ActualProperties;
                            if (itemProperties.Count != 0)
                            {
                                dctJkp = itemProperties;
                            }
                        }
                        catch (System.StackOverflowException)
                        {
                            // If accessing item properties causes stack overflow, use empty collection
                            ReferenceStack.Remove(schema);
                            return "{}";
                        }
                    }
                }

                foreach (KeyValuePair<string, JsonProperty> jkp in dctJkp)
                {
                    string key = jkp.Key;
                    if (UseXMlNames && jkp.Value.Xml != null)
                    {
                        key = jkp.Value.Xml.Name;
                    }
                    if (jkp.Value != null && jkp.Value.HasReference && jkp.Value.Reference != null)
                    {
                        if (jkp.Value.Reference.HasReference == false && jkp.Value.Reference.Properties.Count == 0)
                        {
                            object o1 = GenerateJsonObjectFromJsonSchema4(jkp.Value, ReferenceStack, UseXMlNames);
                            JsonBody.Add(key, o1);
                        }
                        else
                        {
                            if (!jkp.Value.Reference.Equals(jkp.Value) && !ReferenceStack.Contains(jkp.Value.Reference))
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
            }
            else
            {
                System.Text.StringBuilder property = new System.Text.StringBuilder();
                foreach (var schemaObject in schema.AllOf)
                {
                    if (string.IsNullOrEmpty(property.ToString()))
                    {
                        property.Append(JsonSchemaFaker(schemaObject, ReferenceStack, UseXMlNames));
                    }
                    else
                    {
                        property.Remove(property.Length - 1, 1);
                        var result = JsonSchemaFaker(schemaObject, ReferenceStack, UseXMlNames);
                        property.Append(string.Format(",{0}", result[1..]));
                    }
                }
                ReferenceStack.Remove(schema);
                return property.ToString();
            }

            ReferenceStack.Remove(schema);

            string output = JsonConvert.SerializeObject(JsonBody);
            CachedValues.Add(schema, output);
            return output;
        }


        private static object GenerateJsonObjectFromJsonSchema4(JsonProperty value, List<object> ReferenceStack, bool UseXMlNames)
        {
            if (CachedValues.ContainsKey(value))
            {
                object returnValue;
                CachedValues.TryGetValue(value, out returnValue);
                return returnValue;
            }

            if (ReferenceStack == null)
            {
                ReferenceStack = [];
            }

            // Check for circular reference before processing
            if (ReferenceStack.Contains(value))
            {
                return "{}";
            }

            List<object> PrivateStack = [];
            object output = "";
            switch (value.Type)
            {
                case JsonObjectType.Object:
                    Dictionary<string, object> JsonBody = [];

                    // Safely access ActualProperties with stack overflow protection
                    IReadOnlyDictionary<string, JsonProperty> actualProperties;
                    try
                    {
                        actualProperties = value.ActualProperties;
                    }
                    catch (System.StackOverflowException)
                    {
                        // Return empty object if ActualProperties causes stack overflow
                        return new Dictionary<string, object>();
                    }

                    foreach (KeyValuePair<string, JsonProperty> jkp in actualProperties)
                    {
                        string key = jkp.Key;
                        if (UseXMlNames && jkp.Value.Xml != null)
                        {
                            key = jkp.Value.Xml.Name;
                        }

                        // Check for circular reference before adding to stack
                        if (!ReferenceStack.Contains(jkp.Value))
                        {
                            if (jkp.Value.ActualSchema != null)
                            {
                                if (jkp.Value.ActualSchema.Enumeration.Count > 0)
                                {
                                    ReferenceStack.Add(jkp.Value.ActualSchema.Enumeration.FirstOrDefault());
                                }
                                else if (jkp.Value.ActualSchema.Example != null)
                                {
                                    ReferenceStack.Add(jkp.Value.ActualSchema.Example);
                                }
                                else
                                {
                                    ReferenceStack.Add(jkp.Value);
                                }
                            }
                            else
                            {
                                ReferenceStack.Add(jkp.Value);
                            }

                            PrivateStack.Add(jkp.Value);
                            object JObject = GenerateJsonObjectFromJsonSchema4(jkp.Value, ReferenceStack, UseXMlNames);
                            JsonBody.Add(key, JsonConvert.SerializeObject(JObject));
                        }
                    }
                    output = JsonBody;
                    break;

                case JsonObjectType.Array:
                    JObject jb = [];

                    // Add current array value to stack to prevent circular references
                    ReferenceStack.Add(value);
                    PrivateStack.Add(value);

                    // Safely access Item.ActualProperties
                    if (value.Item != null)
                    {
                        // Check if Item or Item.Reference is already in stack
                        bool itemInStack = ReferenceStack.Contains(value.Item);
                        bool itemReferenceInStack = value.Item.HasReference && ReferenceStack.Contains(value.Item.Reference);

                        if (!itemInStack && !itemReferenceInStack)
                        {
                            try
                            {
                                var itemProperties = value.Item.ActualProperties;
                                foreach (var item in itemProperties)
                                {
                                    string key = item.Key;
                                    if (UseXMlNames && item.Value.Xml != null)
                                    {
                                        key = item.Value.Xml.Name;
                                    }

                                    // Check for circular reference before adding
                                    if (!ReferenceStack.Contains(item.Value))
                                    {
                                        // Also check if the item's parent schema creates a cycle
                                        bool createsCycle = item.Value.ParentSchema != null && ReferenceStack.Contains(item.Value.ParentSchema);

                                        if (!createsCycle)
                                        {
                                            ReferenceStack.Add(item.Value);
                                            PrivateStack.Add(item.Value);
                                            object JsonObject = GenerateJsonObjectFromJsonSchema4(item.Value, ReferenceStack, UseXMlNames);
                                            jb.Add(key, JsonConvert.SerializeObject(JsonObject));
                                        }
                                    }
                                }
                            }
                            catch (System.StackOverflowException)
                            {
                                // If accessing item properties causes stack overflow, return empty array
                                foreach (object obj in PrivateStack)
                                {
                                    ReferenceStack.Remove(obj);
                                }
                                output = new JArray();
                                break;
                            }
                        }
                    }

                    if (value.Item?.HasReference == true && !ReferenceStack.Contains(value.Item.Reference))
                    {
                        // Add the reference to stack before processing to prevent cycles
                        ReferenceStack.Add(value.Item.Reference);
                        PrivateStack.Add(value.Item.Reference);

                        try
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

                                    // Check for circular reference including parent schema check
                                    bool alreadyInStack = ReferenceStack.Contains(item.Value);
                                    bool parentInStack = item.Value.ParentSchema != null && ReferenceStack.Contains(item.Value.ParentSchema);

                                    if (!alreadyInStack && !parentInStack)
                                    {
                                        ReferenceStack.Add(item.Value);
                                        PrivateStack.Add(item.Value);
                                        object o = GenerateJsonObjectFromJsonSchema4(item.Value, ReferenceStack, UseXMlNames);
                                        jb.Add(key, JsonConvert.SerializeObject(o));
                                    }
                                }
                            }
                        }
                        catch (System.StackOverflowException)
                        {
                            // If accessing reference properties causes stack overflow, skip
                        }
                    }

                    JArray ja = [jb];
                    output = ja;
                    if (UseXMlNames && value.Xml?.Name != null)
                    {
                        Dictionary<string, object> jsb = new Dictionary<string, object>
                        {
                            { value.Xml.Name, ja }
                        };
                        output = jsb;
                    }
                    break;

                case JsonObjectType.String:
                    if (value.Example == null && value.IsEnumeration && value.Enumeration?.Count > 0)
                    {
                        output = new JValue(value.Enumeration.FirstOrDefault());
                        break;
                    }
                    output = new JValue(value.Example ?? $"<{value.Type}>");
                    break;
                case JsonObjectType.Number:
                    output = new JValue(value.Example ?? 1);
                    break;
                case JsonObjectType.Integer:
                    output = new JValue(value.Example ?? 1);
                    break;
                case JsonObjectType.Boolean:
                    output = new JValue(value.Example ?? false);
                    break;
                case JsonObjectType.Null:
                    output = JValue.CreateNull();
                    break;
                case JsonObjectType.None:
                    if (value.ActualSchema?.IsEnumeration == true)
                    {
                        if (value.ActualSchema.Enumeration.Count > 0)
                        {
                            output = value.ActualSchema.Enumeration.FirstOrDefault();
                        }
                        else
                        {
                            output = value.ActualSchema.Example;
                        }
                    }
                    break;
                default:
                    output = new JValue(value.Example ?? "");
                    break;
            }

            foreach (object obj in PrivateStack)
            {
                ReferenceStack.Remove(obj);
            }
            CachedValues.Add(value, output);
            return output;
        }
    }
}
