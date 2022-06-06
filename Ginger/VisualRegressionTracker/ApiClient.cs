using System;
using System.Net;
using System.Net.Http;

namespace VisualRegressionTracker
{
    public partial class ApiClient 
    {
        public ApiClient(string baseUrl) : this(baseUrl, new HttpClient()) {}

        public string ApiKey { get; set; }
        public string Project { get; set; }


        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
        {
            request.Headers.Add("apiKey", new[] { ApiKey });
            request.Headers.Add("project", new[] { Project });
        }
    }
}