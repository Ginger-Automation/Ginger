using Amdocs.Ginger.Common;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using static Amdocs.Ginger.CoreNET.External.WireMock.WireMockMapping;

namespace Amdocs.Ginger.CoreNET.External.WireMock
{
    public class WireMockMappingController
    {
        public WireMockAPI mockAPI;
        public WireMockMappingController()
        {
            mockAPI = new WireMockAPI();
        }

        //Deserializing the workmock response for grid view
        public async Task<ObservableList<Mapping>> DeserializeWireMockResponseAsync()
        {
            try
            {
                string jsonResponse = await mockAPI.ViewMappingAsync();
                WireMockResponse wireMockResponse = JsonConvert.DeserializeObject<WireMockResponse>(jsonResponse);
                return wireMockResponse?.Mappings ?? new ObservableList<Mapping>();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "An error occurred while deserializing the WireMock response.", ex);
                return [];
            }
        }

        // Download button for downloading mapping
        public async Task<string> DownloadWireMockMappingsAsync()
        {
            try
            {
                string jsonResponse = await mockAPI.ViewMappingAsync();
                return jsonResponse;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "An error occurred while downloading WireMock mappings.", ex);
                return string.Empty;
            }
        }

        //creating the mapping by providing the json body
        public async Task<string> CreateMappingAsync(string jsonBody)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonBody))
                {
                    Reporter.ToLog(eLogLevel.WARN, "JSON body is null or empty.");
                    return string.Empty;
                }

                string response = await mockAPI.CreateStubAsync(jsonBody);
                return response;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "An error occurred while creating the WireMock mapping.", ex);
                return string.Empty;
            }
        }
        // checking for api model if wiremock mapping is present
        public async Task<bool> IsWireMockMappingPresentAsync()
        {
            try
            {
                string jsonResponse = await mockAPI.ViewMappingAsync();
                WireMockResponse wireMockResponse = JsonConvert.DeserializeObject<WireMockResponse>(jsonResponse);
                return wireMockResponse?.Mappings?.Count > 0;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "An error occurred while checking for WireMock mappings.", ex);
                return false;
            }
        }

        // create the woremock mapping from api learning
        public async Task<string> CreateMappingFromApiLearningAsync(string targetUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(targetUrl))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Target URL cannot be null or empty");
                    return string.Empty;
                }

                string response = await mockAPI.StartRecordingAsync(targetUrl);
                return response;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "An error occurred while creating the WireMock mapping from API learning.", ex);
                return string.Empty;
            }
        }

        // show api model apis if wiremock mapping are present already
        public async Task<ObservableList<Mapping>> ShowApiModelApisAsync()
        {
            try
            {
                string jsonResponse = await mockAPI.ViewMappingAsync();
                WireMockResponse wireMockResponse = JsonConvert.DeserializeObject<WireMockResponse>(jsonResponse);
                return wireMockResponse?.Mappings ?? new ObservableList<Mapping>();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "An error occurred while showing API model APIs.", ex);
                return [];
            }
        }

        // if production server is down, do a mock server request
        public async Task<string> MockServerRequestAsync(string requestUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(requestUrl))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Request URL cannot be null or empty");
                    return string.Empty;
                }

                bool isServerUp = await mockAPI.TestWireMockConnectionAsync(requestUrl);
                if (!isServerUp)
                {
                    string response = await mockAPI.GetStubAsync(requestUrl);
                    return response;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Production server is up, no need to mock the request.");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "An error occurred while making a mock server request.", ex);
                return string.Empty;
            }
        }

    }
}
