using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;

namespace Ginger.Utils
{
    public class JSONManager
    {
        public static Dictionary<string, object> DeserializeJson(string json)
        {
            if (json.StartsWith("["))
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>();

                JArray a = JArray.Parse(json);

                int ArrayCount = 1;
                foreach (JObject o in a.Children<JObject>())
                {
                    dictionary.Add(ArrayCount.ToString(), o);
                    ArrayCount++;

                }
                return dictionary;
            }
            else
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> dictionary =
                    serializer.Deserialize<Dictionary<string, object>>(json);
                return dictionary;
            }
        }
    }
}
