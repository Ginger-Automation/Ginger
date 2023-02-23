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

using Amdocs.Ginger.Common;
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
            Reporter.ToLog(eLogLevel.DEBUG, "GitHTTPClient url= " + url);
            using (var client = new HttpClient())
            {                
                // Simulate a browser header                
                //client.DefaultRequestHeaders.Add("User-Agent", GingerUtils.Workspace.OS.UserAgent);
                client.DefaultRequestHeaders.Add("User-Agent", "Ginger-App");             
                var result = client.GetAsync(url).Result;
                Reporter.ToLog(eLogLevel.DEBUG, "result= " + result);
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
            Reporter.ToLog(eLogLevel.DEBUG, "Git GetJSON");
            try
            {
                T t = default(T);                
                string packagesjson = GetResponseString(url).Result;                
                if (packagesjson.Contains("Error: Forbidden"))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Git returned error Forbidden:" + url);                    
                    return t;
                }
                T obj = JsonConvert.DeserializeObject<T>(packagesjson);
                return obj;
            }
            catch(Exception ex)
            {                
                Reporter.ToLog(eLogLevel.ERROR, "Git Get JSON error", ex);
                return default(T);
            }
        }
    }
}
