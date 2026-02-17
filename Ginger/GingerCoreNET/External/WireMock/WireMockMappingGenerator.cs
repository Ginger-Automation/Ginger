#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Amdocs.Ginger.Repository;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.External.WireMock
{
    public class WireMockMappingGenerator
    {
        private static readonly WireMockAPI WireMockAPI = new();
        public static async Task CreateWireMockMapping(ApplicationAPIModel model)
        {
            string trimmedUrl = RemovingURLPathQuery(model.URLDomain) + TrimApiEndpointUrl(model.EndpointURL);

            ApplicationAPIUtils.eRequestContentType ReqitemType = model.RequestContentType;
            string ReqcontentType = GetEnumValueDescription(ReqitemType);
            if (ReqitemType == ApplicationAPIUtils.eRequestContentType.JSon)
            {
                ReqcontentType = "application/json";
            }

            ApplicationAPIUtils.eResponseContentType ResitemType = model.ResponseContentType;
            string RescontentType = GetEnumValueDescription(ResitemType);


            var stubTemplate = new
            {
                name = model.Name,
                request = new
                {

                    method = model.RequestType.ToString(),
                    urlPattern = $"{trimmedUrl}",
                    bodyPatterns = new[]
            {
                new { matches = ".*" }
            },
                    headers = new
                    {
                        Content_Type = new
                        {
                            matches = $"{ReqcontentType}.*"
                        },

                    },
                },
                response = new
                {
                    status = 200,
                    body = "This is a generic mock response", // as per the request template
                    headers = new
                    {
                        Content_Type = RescontentType,
                    }
                }
            };

            string mappingJson = JsonConvert.SerializeObject(stubTemplate).Replace("Content_Type", "Content-Type");
            await WireMockAPI.CreateStubAsync(mappingJson, ReqcontentType);
        }

        public static string TrimApiEndpointUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }
            try
            {
                //Remove content within curly braces
                url = Regex.Replace(url, @"\{.*?\}", string.Empty);

                // Remove everything after '?' to exclude query parameters
                int queryIndex = url.IndexOf('?');
                if (queryIndex >= 0)
                {
                    url = url.Substring(0, queryIndex);
                }

                // Remove trailing parameters enclosed in <{ }>
                int startIndex = url.IndexOf("<{");
                while (startIndex >= 0)
                {
                    int endIndex = url.IndexOf("}>", startIndex);
                    if (endIndex >= 0)
                    {
                        url = url.Remove(startIndex, endIndex - startIndex + 2);
                    }
                    else
                    {
                        break;
                    }
                    startIndex = url.IndexOf("<{");
                }

                // Remove angle icons
                url = Regex.Replace(url, @"<.*?>", string.Empty);

                // Normalize slashes (remove duplicate or trailing slashes)
                url = Regex.Replace(url, @"[\\/]+", "/");

                // Replace numeric IDs in the path with a wildcard pattern
                url = Regex.Replace(url, @"/\d+", "/");

                return url + ".*";
            }
            catch (UriFormatException ex)
            {
                // Handle the exception if the URL is not in a valid format
                Reporter.ToLog(eLogLevel.ERROR, "Invalid URL format", ex);
                return url;
            }
        }
        public static string GetEnumValueDescription(Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attributes = (EnumValueDescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(EnumValueDescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].ValueDescription : value.ToString();
        }

        public static string RemovingURLPathQuery(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                return uri.PathAndQuery;
            }
            catch (UriFormatException ex)
            {
                Reporter.ToLog(eLogLevel.INFO, "Invalid URL format", ex);
                return url;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Invalid URL format", ex);
                return string.Empty;
            }
        }
    }

}
