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
