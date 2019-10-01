using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Ginger.Plugin.Platform.WebService
{
    public struct GingerHttpResponseMessage
    {

        public string Resposne;
        public string RequestBodyString;
        public HttpStatusCode StatusCode { get; set; }
        public Dictionary<string, string> Headers;
    }
}
