#region License
/*
Copyright © 2014-2023 European Support Limited

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
using System;
using System.Net.Http;

namespace Amdocs.Ginger.CoreNET.GeneralLib
{ 
    public static class RequestResponseFormat
    {
        public const string JSON = "application/json";
        public const string XML = "application/xml";
        public const string TEXT = "text/plain";
        public const string HTML = "text/html";
    }

    public class JsonUtils
    {
        public static string SerializeObject(object objectToSerialize, bool ignoreNullValues = true)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            if (ignoreNullValues)
            {
                jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
            }
            return JsonConvert.SerializeObject(objectToSerialize, jsonSerializerSettings);
        }

        public static StringContent SerializeObjectToJsonStringContent(Object objectToSerialize)
        {
            string jsonData = SerializeObject(objectToSerialize);
            var contentData = new StringContent(jsonData, System.Text.Encoding.UTF8, RequestResponseFormat.JSON);
            return contentData;
        }

        public static T DeserializeObject<T>(string jsonContent, bool ignoreNullValues = true)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
            if (ignoreNullValues)
            {
                jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            }
            //JsonSerializer.Deserialize<T>(respond, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }); //Need to add also case sensitive ignore option?
            return (T)JsonConvert.DeserializeObject(jsonContent, typeof(T), jsonSerializerSettings);
        }
    }
}
