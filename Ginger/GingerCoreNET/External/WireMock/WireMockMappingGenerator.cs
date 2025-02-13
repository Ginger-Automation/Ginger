using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.External.Configurations;
using Amdocs.Ginger.Repository;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace Amdocs.Ginger.CoreNET.External.WireMock
{
    public class WireMockMappingGenerator
    {
        private static readonly WireMockAPI WireMockAPI = new();
        public static WireMockConfiguration mockConfiguration;
        public static string baseurl;
        public async static void CreateWireMockMapping(ApplicationAPIModel model)
        {
            mockConfiguration = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<WireMockConfiguration>().Count == 0 ? new WireMockConfiguration() : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<WireMockConfiguration>();
            string trimmedUrl = RemovingURLPathQuery(model.URLDomain) + TrimApiEndpointUrl(model.EndpointURL);
            baseurl = mockConfiguration.WireMockUrl;

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

            //if (ReqitemType == ApplicationAPIUtils.eRequestContentType.JSon)
            //{
            //    string mappingJson = JsonConvert.SerializeObject(stubTemplate).Replace("Content_Type", "Content-Type");
            //    await WireMockAPI.CreateStubAsync(mappingJson, ReqcontentType);
            //}
            //else if (ReqitemType == ApplicationAPIUtils.eRequestContentType.XML)
            //{

            //    string mappingJson = JsonConvert.SerializeObject(stubTemplate).Replace("Content_Type", "Content-Type");
            //    await WireMockAPI.CreateStubAsync(mappingJson, ReqcontentType);


            //}
            //else if (ReqitemType == ApplicationAPIUtils.eRequestContentType.TextPlain)
            //{
            //    string mappingJson = JsonConvert.SerializeObject(stubTemplate).Replace("Content_Type", "Content-Type");
            //    await WireMockAPI.CreateStubAsync(mappingJson, ReqcontentType);
            //}
            //else if (ReqitemType == ApplicationAPIUtils.eRequestContentType.XwwwFormUrlEncoded)
            //{
            //    string mappingJson = JsonConvert.SerializeObject(stubTemplate).Replace("Content_Type", "Content-Type");
            //    await WireMockAPI.CreateStubAsync(mappingJson, ReqcontentType);
            //}
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
                // Handle the exception if the URL is not in a valid format
                Reporter.ToLog(eLogLevel.ERROR, "Invalid URL format", ex);
                return string.Empty;
            }
        }
    }

}
