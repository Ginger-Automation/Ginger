using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.External.Configurations;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.External.WireMock
{
    public class WireMockAPI
    {
        private WireMockConfiguration mockConfiguration;
        private readonly string _baseUrl;
        private const string MappingEndpoint = "/mappings";
        private const string StartRecordingEndpoint = "/recordings/start";
        private const string StopRecordingEndpoint = "/recordings/stop";

        public WireMockAPI()
        {
            mockConfiguration = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<WireMockConfiguration>().Count == 0 ? new WireMockConfiguration() : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<WireMockConfiguration>();
            _baseUrl = mockConfiguration.WireMockUrl;
        }

        //Test Connection API
        public async Task<bool> TestWireMockConnectionAsync(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error testing WireMock connection", ex);
                return false;
            }
        }

        // View Mapping API
        public async Task<string> ViewMappingAsync()
        {
            try
            {

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"{_baseUrl}{MappingEndpoint}");
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                Reporter.ToLog(eLogLevel.ERROR, "Error in showing wiremock mapping", ex);
                return null;
            }
        }

        // Recording API
        public async Task<string> StartRecordingAsync(string targetUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var content = new StringContent($"{{\"targetBaseUrl\": \"{targetUrl}\"}}", Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync($"{_baseUrl}{StartRecordingEndpoint}", content);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                Reporter.ToLog(eLogLevel.ERROR, "Error in starting WireMock recording", ex);
                return null;
            }
        }

        public async Task<string> StopRecordingAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.PostAsync($"{_baseUrl}{StopRecordingEndpoint}", null);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                Reporter.ToLog(eLogLevel.ERROR, "Error in stopping WireMock recording", ex);
                return null;
            }
        }

        // Creating a stub API
        public async Task<string> CreateStubAsync(string stubMapping, string contentType = null)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    if (string.IsNullOrEmpty(contentType))
                    {
                        contentType = "application/json";
                    }
                    var content = new StringContent(stubMapping, Encoding.UTF8, contentType);
                    HttpResponseMessage response = await client.PostAsync($"{_baseUrl}{MappingEndpoint}", content);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                Reporter.ToLog(eLogLevel.ERROR, "Error in creating WireMock stubbing", ex);
                return null;
            }
        }

        // Reading a stub API
        public async Task<string> GetStubAsync(string stubId)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"{_baseUrl}{MappingEndpoint}/{stubId}");
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                Reporter.ToLog(eLogLevel.ERROR, "Error in getting WireMock stubbing", ex);
                return null;
            }
        }

        // Updating a stub API
        public async Task<string> UpdateStubAsync(string stubId, string stubMapping)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var content = new StringContent(stubMapping, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PutAsync($"{_baseUrl}{MappingEndpoint}/{stubId}", content);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                Reporter.ToLog(eLogLevel.ERROR, "Error in updating WireMock stubbing", ex);
                return null;
            }
        }

        // Deleting a stub API
        public async Task<HttpResponseMessage> DeleteStubAsync(string stubId)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.DeleteAsync($"{_baseUrl}{MappingEndpoint}/{stubId}");
                    return response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                Reporter.ToLog(eLogLevel.ERROR, "Error in deleting WireMock stubbing", ex);
                return null;
            }
        }

        // Delete all Mapping
        public async Task<HttpResponseMessage> DeleteAllMappingsAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.DeleteAsync($"{_baseUrl}{MappingEndpoint}");
                    return response.EnsureSuccessStatusCode();

                }
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.WireMockAPIError);
                Reporter.ToLog(eLogLevel.ERROR, "Error in deleting all WireMock mapping", ex);
                return null;
            }
        }
    }
}
