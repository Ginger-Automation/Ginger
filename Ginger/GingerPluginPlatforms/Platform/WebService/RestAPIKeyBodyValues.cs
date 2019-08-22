using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Ginger.Plugin.Platform.WebService
{
    public class RestAPIKeyBodyValues
    {

        public enum eValueType
        {

            Text,

            File,
        }
        public eValueType ValueType { get; set; }
        public HttpContent Content { get; set; }

        public string Param;
        public string Value;
        public string Filename;

    }
}
