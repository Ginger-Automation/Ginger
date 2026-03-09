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
using Newtonsoft.Json;

namespace Amdocs.Ginger.CoreNET.External.WireMock
{
    public class WireMockMapping
    {
        public class Mapping
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("request")]
            public Request Request { get; set; }

            [JsonProperty("response")]
            public Response Response { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }
        }

        public class Request
        {
            [JsonProperty("urlPattern")]
            public string Url { get; set; }

            [JsonProperty("method")]
            public string Method { get; set; }
        }

        public class Response
        {
            [JsonProperty("status")]
            public int Status { get; set; }

            [JsonProperty("body")]
            public string Body { get; set; }
        }

        public class WireMockResponse
        {
            [JsonProperty("mappings")]
            public ObservableList<Mapping> Mappings { get; set; }

            [JsonProperty("meta")]
            public Meta Meta { get; set; }
        }

        public class Meta
        {
            [JsonProperty("total")]
            public int Total { get; set; }
        }

    }
}
