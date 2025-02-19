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
