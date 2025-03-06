#region License
/*
Copyright © 2014-2025 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using GingerCore;
using GingerCore.DataSource;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using static Ginger.Run.RunSetActions.RunSetActionBase;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionSendDataToExternalSourceOperations : IRunSetActionSendDataToExternalSourceOperations
    {
        public RunSetActionSendDataToExternalSource RunSetActionSendDataToExternalSource;

        public RunSetActionSendDataToExternalSourceOperations(RunSetActionSendDataToExternalSource runSetActionSendDataToExternalSource)
        {
            this.RunSetActionSendDataToExternalSource = runSetActionSendDataToExternalSource;
            this.RunSetActionSendDataToExternalSource.RunSetActionSendDataToExternalSourceOperations = this;
        }
        private ValueExpression mValueExpression = null;

        public void Execute(IReportInfo RI)
        {
            Context mContext = new Context
            {
                RunsetAction = RunSetActionSendDataToExternalSource
            };
            mValueExpression = new ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, mContext, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>());

            if (!string.IsNullOrEmpty(RunSetActionSendDataToExternalSource.RequestBodyJson))
            {
                mValueExpression.Value = RunSetActionSendDataToExternalSource.RequestBodyJson;
            }
            else
            {
                mValueExpression.Value = RequestBodyWithParametersToJson();
            }
            string JsonOutput = mValueExpression.ValueCalculated;

            Reporter.ToStatus(eStatusMsgKey.PublishingToCentralDB, null, "Sending Execution data to External Source");
            var options = new RestClientOptions(RunSetActionSendDataToExternalSource.EndPointUrl)
            {
                ThrowOnAnyError = false,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                FailOnDeserializationError = false,
                ThrowOnDeserializationError = false
            };
            RestClient restClient = new RestClient(options);

            RestRequest restRequest = new RestRequest
            {
                Method = Method.Post,
                RequestFormat = RestSharp.DataFormat.Json
            };
            foreach (ActInputValue actInputValue in RunSetActionSendDataToExternalSource.RequestHeaders)
            {
                mValueExpression.Value = actInputValue.Value;
                restRequest.AddHeader(actInputValue.Param, mValueExpression.ValueCalculated);
            }
            restRequest.AddJsonBody(JsonOutput);

            try
            {
                RestResponse response = restClient.Execute(restRequest);
                if (response.IsSuccessful)
                {
                    Reporter.ToLog(eLogLevel.INFO, "Successfully sent data to External Source");
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send execution data to External Source Response: " + response.Content);
                    RunSetActionSendDataToExternalSource.Errors = "Failed to send execution data to External Source Response: " + response.Content;
                    RunSetActionSendDataToExternalSource.Status = eRunSetActionStatus.Failed;
                    return;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception when sending", ex);
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
        }

        private string RequestBodyWithParametersToJson()
        {
            Dictionary<string, string> requestBodyParams = [];
            foreach (ActInputValue AIV in RunSetActionSendDataToExternalSource.RequestBodyParams)
            {
                requestBodyParams.Add(AIV.Param, AIV.Value);
            }
            return JsonConvert.SerializeObject(requestBodyParams, Formatting.Indented);
        }
    }
}
