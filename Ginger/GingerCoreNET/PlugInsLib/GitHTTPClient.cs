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
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.PlugInsLib
{
    class GitHTTPClient
    {

        public static async Task<string> GetResponseString(string url)
        {
            using (var client = new HttpClient())
            {
                // Simulate a browser
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
                
                var result = client.GetAsync(url).Result;

                if (result.IsSuccessStatusCode)
                {
                    var json = await result.Content.ReadAsStringAsync();
                    return json;
                }
                else
                {
                    return "Error: " + result.ReasonPhrase;
                }
            }
        }

        internal static T GetJSON<T>(string url)
        {            
            string packagesjson = GetResponseString(url).Result;

            T list = JsonConvert.DeserializeObject<T>(packagesjson);
            return list;
        }
    }
}
