#region License
/*
Copyright © 2014-2019 European Support Limited

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
