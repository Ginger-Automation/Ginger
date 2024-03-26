using Amdocs.Ginger.Common;
using Ginger.ExecuterService.Contracts.V1.ExecuterHandler.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Run.RemoteExecution
{
    public sealed class ExecutionHandlerAPIClient : IExecutionHandlerAPIClient
    {
        private const string ExecutionDetailsEndPoint = "api/v1/executions";

        private readonly string _url;
        private readonly HttpClient _httpClient;

        public ExecutionHandlerAPIClient(string url)
        {
            _url = url;
            _httpClient = new();
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

                HttpRequestMessage request = new(HttpMethod.Get, $"{_url}{ExecutionDetailsEndPoint}?executionIds={executionIdsParam}");
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

                HttpResponseMessage response = await _httpClient.SendAsync(request);

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
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while getting execution details from ExecutionHandler", ex);
                throw;
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
