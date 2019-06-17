﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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

        Dictionary<string, Type> mAttrTypeDictionary = null;
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
                        if(mAttrTypeDictionary == null)
                        {
                            CreateAttrTypeDictionary();    
                        }
                        Type attrType = null;
                        bool found = mAttrTypeDictionary.TryGetValue(value, out attrType);
                        
                        if (found)
                        {
                            attr = (Attribute)Activator.CreateInstance(attrType);
                        }
                        else
                        {
                            throw new Exception("Cannot create attribute: " + value);
                        }
                       
                        attrs.Add(attr);
                    }
                    else
                    {
                        PropertyInfo propertyInfo = attr.GetType().GetProperty(name);
                        // TODO: try to make it generic !!!!!
                        if (propertyInfo.PropertyType == typeof(int))
                        {
                            propertyInfo.SetValue(attr, p.Value.Value<int>());
                        }
                        else if (propertyInfo.PropertyType == typeof(string))
                        {
                            propertyInfo.SetValue(attr, p.Value.Value<string>());
                        }
                        else if (propertyInfo.PropertyType == typeof(List<int>))
                        {
                            List<int> list = new List<int>();                            
                            JArray jarr = (JArray)p.Value;
                            foreach (int val in jarr.Children())
                            {
                                list.Add(val); 
                            }
                            propertyInfo.SetValue(attr, list);
                        }
                        else if(propertyInfo.PropertyType == typeof(bool))
                        {
                            propertyInfo.SetValue(attr, p.Value.Value<bool>());
                        }
                        else if(propertyInfo.PropertyType == typeof(object))
                        {
                            propertyInfo.SetValue(attr, p.Value.Value<object>());
                        }
                        else if(propertyInfo.PropertyType.IsEnum)
                        {                            
                            propertyInfo.SetValue(attr, Enum.Parse(propertyInfo.PropertyType, p.Value.ToString()));
                        }
                        else
                        {
                            throw new Exception("ReadJson Cannot convert attr: " + attr);
                        }
                        //TODO:  Else List<int>  etc... err


                    }
                }
            }
            
            return attrs;
        }

        private void CreateAttrTypeDictionary()
        {
            mAttrTypeDictionary = new Dictionary<string, Type>();
            Assembly assembly = typeof(IParamProperty).Assembly;
            var actionParamPropertyAttributeTypes = from type in assembly.GetTypes()
                                                    where typeof(IParamProperty).IsAssignableFrom(type) && type.IsInterface == false
                                                    select type;

            foreach (Type t in actionParamPropertyAttributeTypes)
            {                
                IParamProperty attr = (IParamProperty)Activator.CreateInstance(t);
                mAttrTypeDictionary.Add(attr.PropertyName, t);                
            }
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
                IParamProperty actionParamProperty = (IParamProperty)attr;
                string propertyName = actionParamProperty.PropertyName;                
                serializer.Serialize(writer, propertyName);

                foreach (var property in properties)
                {
                    if (property.Name == nameof(Attribute.TypeId) || property.Name == nameof(IParamProperty.PropertyName))
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
