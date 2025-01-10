#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Ginger.ExecuterService.Contracts.V1.ExecuterHandler.Requests;
using Ginger.ExecuterService.Contracts.V1.ExecuterHandler.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Run.RemoteExecution
{
    public sealed class ExecutionHandlerAPIClient : IExecutionHandlerAPIClient
    {
        private const string ExecutionDetailsEndPoint = "api/v1/executions";
        private const string StartExecutionEndPoint = "api/v1/executions";

        private HttpClient? _httpClient;

        public string URL { get; set; }

        private HttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new();
                }
                return _httpClient;
            }
        }

        public ExecutionHandlerAPIClient() : this(string.Empty) { }

        public ExecutionHandlerAPIClient(string url)
        {
            URL = url;
        }

        public struct ExecutionDetailsOptions : IEquatable<ExecutionDetailsOptions>
        {
            public bool IncludeRequestDetails { get; init; }
            public bool IncludeExecutionErrors { get; init; }
            public bool IncludeExecutionOutputValues { get; init; }
            public bool IncludeExecutionFlowsDetails { get; init; }
            public bool IncludeExecutionRunnersDetails { get; init; }
            public bool IncludeExecutionLog { get; init; }

            public readonly bool Equals(ExecutionDetailsOptions other)
            {
                return
                    IncludeRequestDetails == other.IncludeRequestDetails &&
                    IncludeExecutionErrors == other.IncludeExecutionErrors &&
                    IncludeExecutionOutputValues == other.IncludeExecutionOutputValues &&
                    IncludeExecutionFlowsDetails == other.IncludeExecutionFlowsDetails &&
                    IncludeExecutionRunnersDetails == other.IncludeExecutionRunnersDetails &&
                    IncludeExecutionLog == other.IncludeExecutionLog;
            }
        }

        public async Task<ExecutionDetailsResponse?> GetExecutionDetailsAsync(string executionId, ExecutionDetailsOptions options)
        {
            return (await GetExecutionDetailsAsync(new[] { executionId }, options)).FirstOrDefault();
        }

        public async Task<IEnumerable<ExecutionDetailsResponse?>> GetExecutionDetailsAsync(IEnumerable<string> executionIds, ExecutionDetailsOptions options)
        {
            if (executionIds == null || !executionIds.Any())
            {
                throw new ArgumentException($"{nameof(executionIds)} cannot be null or empty.");
            }

            List<ExecutionDetailsResponse?> executionDetails = [];
            executionIds = executionIds.Where(id => Guid.TryParse(id, out Guid _)).ToList();

            if (!executionIds.Any())
            {
                return executionDetails;
            }

            try
            {
                string executionIdsParam = string.Join(',', executionIds);
                MediaTypeWithQualityHeaderValue acceptHeader = new("*/*");

                string requestUri = $"{(URL.EndsWith('/') ? URL : URL + "/")}{ExecutionDetailsEndPoint}?executionIds={executionIdsParam}";
                HttpRequestMessage request = new(HttpMethod.Get, requestUri);
                request.Headers.Accept.Add(acceptHeader);
                if (options.IncludeRequestDetails)
                {
                    request.Headers.Add("IncludeRequestDetails", "true");
                }
                if (options.IncludeExecutionErrors)
                {
                    request.Headers.Add("IncludeExecutionErrors", "true");
                }
                if (options.IncludeExecutionOutputValues)
                {
                    request.Headers.Add("IncludeExecutionOutputValues", "true");
                }
                if (options.IncludeExecutionFlowsDetails)
                {
                    request.Headers.Add("IncludeExecutionFlowsDetails", "true");
                }
                if (options.IncludeExecutionRunnersDetails)
                {
                    request.Headers.Add("IncludeExecutionRunnersDetails", "true");
                }
                if (options.IncludeExecutionLog)
                {
                    request.Headers.Add("IncludeExecutionLog", "true");
                }

                HttpResponseMessage response = await HttpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return executionDetails;
                }
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"ExecutionHandler API returned unsuccessful response code({response.StatusCode}).");
                }

                string responseJson = await response.Content.ReadAsStringAsync();
                JsonSerializerOptions serializerOptions = new()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };
                serializerOptions.Converters.Add(new JsonStringEnumConverter());
                executionDetails.AddRange(JsonSerializer.Deserialize<IEnumerable<ExecutionDetailsResponse?>>(responseJson, serializerOptions)!);

                return executionDetails;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while getting execution details from ExecutionHandler", ex);
                throw;
            }
        }

        public async Task<bool> StartExectuionAsync(AddExecutionRequest executionRequest)
        {
            try
            {
                string requestUri = $"{(URL.EndsWith('/') ? URL : URL + "/")}{StartExecutionEndPoint}";
                HttpRequestMessage request = new(HttpMethod.Post, requestUri)
                {
                    Content = new StringContent(JsonSerializer.Serialize(executionRequest), Encoding.UTF8, "application/json")
                };

                HttpResponseMessage response = await HttpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Starting remote execution failed, got back unsuccessful response code('{(int)response.StatusCode}').");
                    return false;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.INFO, $"Execution to handler submitted successfully {response?.Headers?.Location?.Query?.TrimStart('?')}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while starting remote execution.", ex);
                return false;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
