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