#region License
/*
Copyright © 2014-2025 European Support Limited

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

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.GeneralLib
{

    public class HttpUtilities
    {
        public static string Download(Uri url)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler
                {
                    Proxy = WebRequest.GetSystemWebProxy(),
                    UseProxy = true
                };

                using HttpClient httpClient = new HttpClient(handler);

                // Make the request and get the response  
                var response = httpClient.GetStringAsync(url).GetAwaiter().GetResult();
                return response;
            }
            catch (HttpRequestException ex)
            {
                throw new WebException($"Error downloading from URL: {url}", ex);
            }
        }

        public async static Task<(string response, HttpStatusCode statusCode)> GetAsync(Uri url)
        {
            if (url == null)
            {
                Reporter.ToLog(eLogLevel.ERROR, "GetAsync failed: URL is null.");
                return (null, HttpStatusCode.BadRequest);
            }

                   

            async Task<(string, HttpStatusCode)> TryFetchAsync(HttpClientHandler handler)
            {
                try
                {
                    ServicePointManager.ServerCertificateValidationCallback += (_, _, _, _) => true;
                    handler.ServerCertificateCustomValidationCallback += (_, _, _, _) => { return true; };                   
                    using var client = new HttpClient(handler);                                
                    var response = await client.GetAsync(url);
                    string content = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        Reporter.ToLog(eLogLevel.ERROR,
                            $"GET request to '{url}' failed. Status Code: {response.StatusCode}. Response: {content}");
                        return (null, response.StatusCode);
                    }

                    return (content, response.StatusCode);
                }
                catch (HttpRequestException httpEx)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"HTTP request failed for '{url}': {httpEx.Message}");
                    return (null, HttpStatusCode.ServiceUnavailable);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Unexpected error while calling '{url}'", ex);
                    return (null, HttpStatusCode.InternalServerError);
                }
            }

            // First try: using proxy (original behavior)
            var handlerWithProxy = new HttpClientHandler
            {
                Proxy = WebRequest.GetSystemWebProxy(),
                UseProxy = true,                
            };
            var result = await TryFetchAsync(handlerWithProxy);
            if (result.Item1 != null)
                return result;

            // Second try: without proxy
            var handlerWithoutProxy = new HttpClientHandler
            {
                UseProxy = false
            };
            result = await TryFetchAsync(handlerWithoutProxy);
            if (result.Item1 != null)
                return result;

            // Third try: with proxy but add API host to no-proxy list
            var hostNoProxy = url.Host;
            var systemProxy = WebRequest.GetSystemWebProxy();

            if (systemProxy is WebProxy webProxy)
            {
                var bypassList = webProxy.BypassList ?? [];
                if (!bypassList.Contains(hostNoProxy))
                {
                    var newBypassList = bypassList.Concat([hostNoProxy]).ToArray();
                    webProxy.BypassList = newBypassList;
                }
            }

            var handlerWithProxyNoBypass = new HttpClientHandler
            {
                Proxy = systemProxy,
                UseProxy = true
            };

            result = await TryFetchAsync(handlerWithProxyNoBypass);
            return result;
        }
    }
}
