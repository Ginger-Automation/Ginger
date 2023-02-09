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

using Amdocs.Ginger.Common;

namespace Amdocs.Ginger.Repository
{
    public class ApplicationAPIUtils
    {
        public enum eWebApiType
        {
            REST, SOAP
        }

        public enum eNetworkCredentials
        {
            [EnumValueDescription("Default")]
            Default,
            [EnumValueDescription("Custom")]
            Custom
        }

        public enum eRequestBodyType
        {
            [EnumValueDescription("Free Text")]
            FreeText,
            [EnumValueDescription("From file")]
            TemplateFile
        }

        public enum eCretificateType
        {
            [EnumValueDescription("All Certificates")]
            AllSSL,
            [EnumValueDescription("Custom")]
            Custom
        }

        public enum eSercurityType
        {
            [EnumValueDescription("Default")]
            None,
            Ssl3,
            Tls,
            Tls11,
            Tls12,
            Tls13
        }

        public enum eAuthType
        {
            [EnumValueDescription("No Authentication")]
            NoAuthentication,
            [EnumValueDescription("Basic Authentication")]
            BasicAuthentication
        }

        public enum eRequestType
        {
            GET,
            POST,
            PUT,
            PATCH,
            DELETE,
            Copy,
            Head,
            Options,
            Link,
            Unlink,
            Purge,
            Lock,
            Unlock,
            Profin,
            View
        }

        public enum eHttpVersion
        {
            [EnumValueDescription("Version 1.0")]
            HTTPV10,
            [EnumValueDescription("Version 1.1")]
            HTTPV11,
        }

        public enum eContentType
        {
            [EnumValueDescription("application/json")]
            JSon,
            [EnumValueDescription("text/plain;charset=utf-8")]
            TextPlain,
            [EnumValueDescription("xml")]
            XML,
            [EnumValueDescription("application/x-www-form-urlencoded")]
            XwwwFormUrlEncoded,
            [EnumValueDescription("multipart/form-data")]
            FormData,
            [EnumValueDescription("application/pdf")]
            PDF
        }

        public enum eCookieMode
        {
            [EnumValueDescription("Use session cookies")]
            Session,
            [EnumValueDescription("Don't use cookies")]
            None,
            [EnumValueDescription("Use fresh cookies")]
            New,
            [EnumValueDescription("Use cookies from request header")]
            HeaderCookie
        }
    }
}
