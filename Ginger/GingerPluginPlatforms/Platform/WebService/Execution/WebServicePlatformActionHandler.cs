using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Plugin.Core.ActionsLib;
using System;
using System.Collections.Generic;
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
        IRestClient RestClient = null;
        IWebServicePlatform Platformservice;
    

        public void HandleRunAction(IPlatformService service, ref NodePlatformAction platformAction)
        {
            Platformservice = (IWebServicePlatform)service;
            RestClient = Platformservice.RestClient;

            try
            {
                GingerHttpRequestMessage Request = GetRequest(platformAction);

                GingerHttpResponseMessage Response = RestClient.PerformGetOperation(Request);
                platformAction.Output.Add("Response", Response.Resposne);

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
            Request.Method = platformAction.InputParams["RequestType"].ToString();

            Request.BodyString= platformAction.InputParams.ContainsKey("RequestBody")? platformAction.InputParams["RequestBody"].ToString():"";

          if (platformAction.InputParams["Headers"] is Newtonsoft.Json.Linq.JObject JsonObj)
            {
                foreach(Newtonsoft.Json.Linq.JProperty Jt in JsonObj.Children())
                {
                    Request.Headers.Add(new KeyValuePair<string, string>(Jt.Name, Jt.Value.ToString()));
                }
            }

            return Request;
        }
    }
}
