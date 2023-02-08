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
