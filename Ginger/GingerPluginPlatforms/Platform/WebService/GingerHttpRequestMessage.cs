using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Ginger.Plugin.Platform.WebService
{
    public class GingerHttpRequestMessage
    {
        public enum eContentType
        {

            JSon,

            TextPlain,

            XML,

            XwwwFormUrlEncoded,

            FormData,

            PDF
        }
        public enum eCookieMode
        {
            Session,
            None,
            New
        }
        public enum ehttpVersion
        {
            HTTPV11,
            HTTPV10,
            HTTPV20,
        }
        public Uri URL { get; set; }
        public String Method { get; set; }
        public string ContentType { get; set; }
        public ehttpVersion HttpVersion { get; set; }
        public eCookieMode CookieMode { get; set; }
        public List<RestAPIKeyBodyValues> RequestKeyValues { get; set; }
        public string BodyString { get; set; }

        public List<KeyValuePair<String, string>> Headers = new List<KeyValuePair<string, string>>();

        public eContentType BodyContentType;
      
       
    }
}
