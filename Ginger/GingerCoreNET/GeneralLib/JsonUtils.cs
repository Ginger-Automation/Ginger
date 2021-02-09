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
