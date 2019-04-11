using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Amdocs.Ginger.Plugin.Core
{
    public class AttrsPropertyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(List<Attribute>))
            {             
                return true;
            }
            else
            {
                return false;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // we currently support only writing of JSON
            List<Attribute> attrs = new List<Attribute>();
            JArray a = JArray.Load(reader);

            foreach (JObject o in a.Children<JObject>())
            {
                // first property is the Attr Class PropertyName as defined for interface IActionParamProperty

                Attribute attr = null;
                foreach (JProperty p in o.Properties())
                {
                    string name = p.Name;
                    // TODO: get value based on prop type                    
                    
                    if (p.Name == "Property")
                    {
                        string value = (string)p.Value;

                        // TODO: user reflection to create the class !!!!!!!!!!!!!!!!!!
                        switch (value)
                        {
                            case "Max":
                                attr = new MaxAttribute();
                                break;
                            case "Min":
                                attr = new MinAttribute();
                                break;
                            case "Mandatory":
                                attr = new MandatoryAttribute();
                                break;
                            case "InvalidValue":
                                attr = new InvalidValueAttribute();
                                break;
                            default:
                                // ERRR
                                break;
                        }
                        attrs.Add(attr);
                    }
                    else
                    {
                        // string value = (string)p.Value;
                        PropertyInfo propertyInfo = attr.GetType().GetProperty(name);

                        if (propertyInfo.PropertyType == typeof(int))
                        {
                            propertyInfo.SetValue(attr, p.Value.Value<int>());
                        }
                        else if (propertyInfo.PropertyType == typeof(string))
                        {
                            propertyInfo.SetValue(attr, p.Value.Value<string>());
                        }
                        else
                        {
                            

                            // ERR !!!!!!!!!!!!!!!
                        }
                        //TODO:  Else List<int>  etc... err


                    }
                }
            }
            
            return attrs;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)  
            {
                serializer.Serialize(writer, null);
                return;
            }

            List<Attribute> attrs = (List<Attribute>)value;
            if(attrs.Count == 0)
            {
                serializer.Serialize(writer, null);
                return;
            }
            
            writer.WriteStartArray();
            foreach (Attribute attr in attrs)
            {                
                var properties = attr.GetType().GetProperties();
                writer.WriteStartObject();
                // First write property name so it will be first in the list
                writer.WritePropertyName("Property");
                IActionParamProperty actionParamProperty = (IActionParamProperty)attr;
                string propertyName = actionParamProperty.PropertyName;                
                serializer.Serialize(writer, propertyName);

                foreach (var property in properties)
                {
                    if (property.Name == nameof(Attribute.TypeId) || property.Name == nameof(IActionParamProperty.PropertyName))
                    {
                        continue;
                    }
                    // write property name
                    writer.WritePropertyName(property.Name);                    
                    serializer.Serialize(writer, property.GetValue(attr, null));
                }

                writer.WriteEndObject();
            }
            writer.WriteEndArray();            
        }
    }
}
