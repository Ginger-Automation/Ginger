using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Collections.Generic;


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
                //JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> dictionary =
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                return dictionary;
            }
        }
    }
}
