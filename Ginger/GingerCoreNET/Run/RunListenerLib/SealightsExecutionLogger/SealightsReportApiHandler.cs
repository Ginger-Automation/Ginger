#region License
/*
Copyright © 2014-2020 European Support Limited

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

using AccountReport.Contracts;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using AutoMapper;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib.SealightsExecutionLogger
{


    public class SealightsReportApiHandler
    {

        public Context mContext;
        IValueExpression mVE;
        private string EndPointUrl { get; set; }
        public string TestSessionId;
        private string Token = string.Empty;

        RestClient restClient;
        private const string SEND_CREATEION_TEST_SESSION = "sl-api/v1/test-sessions";

        public SealightsReportApiHandler(Context context)
        {
            mContext = context;
            mVE = new GingerCore.ValueExpression(mContext.Environment, mContext.BusinessFlow, new ObservableList<GingerCore.DataSource.DataSourceBase>(), false, "", false);

            EndPointUrl = WorkSpace.Instance.Solution.LoggerConfigurations.SealightsURL;
            Token = WorkSpace.Instance.Solution.LoggerConfigurations.SealightsAgentToken;

            mVE.Value = EndPointUrl;  // Calculate the value Expression
            EndPointUrl = mVE.ValueCalculated;

            restClient = new RestClient(EndPointUrl);
            restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;            
        }
           
        public void SendCreationTestSessionToSealightsAsync()
        {
            if (restClient != null)
            {
                Reporter.ToLog(eLogLevel.INFO, string.Format("Starting execution data to Sealights"));

                string message = string.Format("execution data to Sealights");
                bool responseIsSuccess = SendRestRequestCreateSession(SEND_CREATEION_TEST_SESSION, Method.POST);
                if (responseIsSuccess)
                {
                    Reporter.ToLog(eLogLevel.INFO, "Successfully sent " + message);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + message);
                }
            }
            else
            {
                Reporter.ToLog(eLogLevel.WARN, "Rest Client is null as endpoint url is not provided");
            }
        }

        public async Task SendingTestEventsToSealightsAsync(string name, DateTime startTime, DateTime endTime, string status)
        {
            if (restClient != null)
            {                
                Reporter.ToLog(eLogLevel.INFO, string.Format("Starting to Send Test Events to Sealights"));

                string message = string.Format("Sending Test Events to Sealights");
                bool responseIsSuccess = await SendRestRequestTestEvents(SEND_CREATEION_TEST_SESSION, Method.POST, name, startTime, endTime, status).ConfigureAwait(false);
                if (responseIsSuccess)
                {
                    Reporter.ToLog(eLogLevel.INFO, "Successfully sent " + message);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + message);
                }
            }
            else
            {
                Reporter.ToLog(eLogLevel.WARN, "Rest Client is null as endpoint url is not provided");
            }
        }

        public async Task SendDeleteSessionToSealightsAsync()
        {
            if (restClient != null)
            {
                Reporter.ToLog(eLogLevel.INFO, string.Format("Starting sealights delete session"));

                string message = string.Format("execution data to Sealights");
                bool responseIsSuccess = await SendRestRequestDeleteSession(SEND_CREATEION_TEST_SESSION, Method.DELETE).ConfigureAwait(false);
                if (responseIsSuccess)
                {
                    Reporter.ToLog(eLogLevel.INFO, "Successfully sent " + message);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + message);
                }
            }
            else
            {
                Reporter.ToLog(eLogLevel.WARN, "Rest Client is null as endpoint url is not provided");
            }
        }



        private bool SendRestRequestCreateSession(string api, Method apiMethod)
        {
            try
            {
                mVE = new GingerCore.ValueExpression(mContext.Environment, mContext.BusinessFlow, new ObservableList<GingerCore.DataSource.DataSourceBase>(), false, "", false);

                RestRequest restRequest = (RestRequest)new RestRequest(api, apiMethod) { RequestFormat = RestSharp.DataFormat.Json };

                restRequest.AddHeader("Content-Type", "application/json");
                restRequest.AddHeader("Authorization", "Bearer " + Token);

                string labId = WorkSpace.Instance.Solution.LoggerConfigurations.SealightsLabId;
                string testStage = WorkSpace.Instance.Solution.LoggerConfigurations.SealightsTestStage;
                string bsId = WorkSpace.Instance.Solution.LoggerConfigurations.SealightsBuildSessionID;
                string sessionTimeout = WorkSpace.Instance.Solution.LoggerConfigurations.SealightsSessionTimeout;

                // Calculate the value Expression
                mVE.Value = labId;  
                labId = mVE.ValueCalculated;
                mVE.Value = testStage;
                testStage = mVE.ValueCalculated;
                mVE.Value = bsId;
                bsId = mVE.ValueCalculated;
                mVE.Value = sessionTimeout;
                sessionTimeout = mVE.ValueCalculated;

                //  Check Sealights's values on run-set levels
                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealighsLabId != null)
                {
                    labId = WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealighsLabId;

                    mVE.Value = labId;
                    labId = mVE.ValueCalculated;
                }
                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealighsBuildSessionID != null)
                {
                    bsId = WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealighsBuildSessionID;

                    mVE.Value = bsId;
                    bsId = mVE.ValueCalculated;
                }
                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsTestStage != null)
                {
                    testStage = WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsTestStage;

                    mVE.Value = testStage;
                    testStage = mVE.ValueCalculated;
                }

                if (string.IsNullOrEmpty(labId) && string.IsNullOrEmpty(bsId))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Sealights - LabId and bsid are empty");
                    return false;
                }
                else if (string.IsNullOrEmpty(testStage))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Sealights - Test Stage is empty");
                    return false;
                }

                restRequest.AddJsonBody(new { labId = labId, testStage = testStage, bsId = bsId, sessionTimeout = sessionTimeout }); // Anonymous type object is converted to Json body

                IRestResponse response = restClient.Execute(restRequest);

                dynamic objResponse = JsonConvert.DeserializeObject(response.Content);
                TestSessionId = objResponse.data.testSessionId.ToString(); 
                
                if (response.IsSuccessful)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Successfully sent " + api);
                    return true;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + api + "Response: " + response.Content);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception when sending " + api, ex);
                return false;
            }
        }


        private async Task<bool> SendRestRequestTestEvents(string api, Method apiMethod, string name, DateTime startTime, DateTime endTime, string status)
        {
            try
            {
                api += "/" + TestSessionId;

                RestRequest restRequest = (RestRequest)new RestRequest(api, apiMethod) { RequestFormat = RestSharp.DataFormat.Json };

                restRequest.AddHeader("Content-Type", "application/json");
                restRequest.AddHeader("Authorization", "Bearer " + Token);

                long unixStartTime = new DateTimeOffset(startTime).ToUnixTimeMilliseconds();
                long unixEndTime = new DateTimeOffset(endTime).ToUnixTimeMilliseconds();
                             
                restRequest.AddJsonBody( new[] { new { name = name, start = unixStartTime, end = unixEndTime, status = status } } ); // Anonymous type object is converted to Json body

                IRestResponse response = await restClient.ExecuteAsync(restRequest);

                if (response.IsSuccessful)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Successfully sent " + api);
                    return true;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + api + "Response: " + response.Content);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception when sending " + api, ex);
                return false;
            }
        }


        private async Task<bool> SendRestRequestDeleteSession(string api, Method apiMethod)
        {
            try
            {
                api += "/" + TestSessionId;

                RestRequest restRequest = (RestRequest)new RestRequest(api, apiMethod) { RequestFormat = RestSharp.DataFormat.Json };

                restRequest.AddHeader("Content-Type", "application/json");
                restRequest.AddHeader("Authorization", "Bearer " + Token);

                IRestResponse response = await restClient.ExecuteAsync(restRequest);

                if (response.IsSuccessful)
                {
                    TestSessionId = null;

                    Reporter.ToLog(eLogLevel.DEBUG, "Successfully sent " + api);
                    return true;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + api + "Response: " + response.Content);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception when sending " + api, ex);
                return false;
            }
        }
    }
}
