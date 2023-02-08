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

using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Plugin.Core.ActionsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Ginger.Plugin.Platform.WebService.Execution
{
    public class WebServicePlatformActionHandler : IPlatformActionHandler
    {
        enum RequestMethod
        {
            Copy,
            Delete,
            Get,
            head,
            Link,
            Lock,
            Options,
            Patch,
            Post,
            Profin,
            Purge,
            Put,
            Unlink,
            Unlock,
            View


        }
        IHTTPClient RestClient = null;
        IWebServicePlatform Platformservice;


        public void HandleRunAction(IPlatformService service, ref NodePlatformAction platformAction)
        {
            Platformservice = (IWebServicePlatform)service;
 
            RestClient = Platformservice.RestClient;

            try
            {
                GingerHttpRequestMessage Request = GetRequest(platformAction);

                GingerHttpResponseMessage Response = RestClient.PerformHttpOperation(Request);
                platformAction.Output.Add("Header: Status Code ", Response.StatusCode.ToString());

                foreach(var RespHeader in Response.Headers)
                {
                    platformAction.Output.Add("Header: " + RespHeader.Key,RespHeader.Value);
                }

                platformAction.Output.Add("Request:", Response.RequestBodyString);
                platformAction.Output.Add("Response:", Response.Resposne);

            }

            catch (Exception ex)
            {
                platformAction.addError(ex.Message);
            }

        }

        private GingerHttpRequestMessage GetRequest(NodePlatformAction platformAction)
        {
            GingerHttpRequestMessage Request = new GingerHttpRequestMessage();

            Request.URL = new Uri(platformAction.InputParams["EndPointURL"].ToString());

            if (platformAction.ActionType == "ActWebAPISoap")
            {
                Request.Method = "POST";
                Request.ContentType = "XML";
            }
            else
            {
                Request.Method = platformAction.InputParams["RequestType"].ToString();
                Request.ContentType = platformAction.InputParams.ContainsKey("ContentType") ? platformAction.InputParams["ContentType"].ToString() : "";

                if (platformAction.InputParams["RequestKeyValues"] is Newtonsoft.Json.Linq.JArray RObj)
                {
                    Request.RequestKeyValues = new List<RestAPIKeyBodyValues>();
                    foreach (Newtonsoft.Json.Linq.JToken Jt in RObj.Children())
                    {


                        RestAPIKeyBodyValues RKV = new RestAPIKeyBodyValues();

                        if (Jt["ValueType"].ToString() == "1")
                        {
                            RKV.ValueType = RestAPIKeyBodyValues.eValueType.File;

                            Byte[] FileBytes = (Byte[])Jt["FileBytes"];
                            string Path = Jt["Value"].ToString();
                            RKV.Filename = System.IO.Path.GetFileName(Path);



                            ByteArrayContent fileContent = new ByteArrayContent(FileBytes);
                            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                            RKV.Content = fileContent;
                        }
                        RKV.Value = Jt["Value"].ToString();
                        RKV.Param = Jt["Param"].ToString();



                        Request.RequestKeyValues.Add(RKV);
                    }
                }

            }
            Request.BodyString = platformAction.InputParams.ContainsKey("RequestBody") ? platformAction.InputParams["RequestBody"].ToString() : "";
         

     

                if (platformAction.InputParams["Headers"] is Newtonsoft.Json.Linq.JObject JsonObj)
            {
                foreach (Newtonsoft.Json.Linq.JProperty Jt in JsonObj.Children())
                {
                    Request.Headers.Add(new KeyValuePair<string, string>(Jt.Name, Jt.Value.ToString()));
                }
            }


            return Request;
        }



    }
}
