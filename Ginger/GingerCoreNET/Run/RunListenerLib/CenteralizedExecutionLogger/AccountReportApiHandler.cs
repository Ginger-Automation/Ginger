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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using AutoMapper;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger
{
    public class AccountReportApiHandler
    {


        private string EndPointUrl { get; set; }

        RestClient restClient;
        private const string SEND_RUNSET_EXECUTION_DATA = "api/AccountReport/runset/";
        private const string SEND_ACTION_EXECUTION_DATA = "api/AccountReport/action/";
        private const string SEND_ACTIVITY_EXECUTION_DATA = "api/AccountReport/activity/";
        private const string SEND_ACTIVITYGROUP_EXECUTION_DATA = "api/AccountReport/activitygroup/";
        private const string SEND_BUSINESSFLOW_EXECUTION_DATA = "api/AccountReport/businessflow/";
        private const string SEND_RUNNER_EXECUTION_DATA = "api/AccountReport/runner/";
        private const string UPLOAD_FILES = "api/AccountReport/UploadFiles/";
        private const string EXECUTION_ID_VALIDATION = "api/AccountReport/ExecutionIdValidation/";

        public AccountReportApiHandler(string apiUrl)
        {
            if (!string.IsNullOrEmpty(apiUrl))
            {
                EndPointUrl = apiUrl;
                try
                {
                    restClient = new RestClient(apiUrl);
                    restClient.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Centralized DB endpoint url is Invalid");
                } 
            }
        }
        public AccountReportApiHandler()
        {
            //created deafult constructor to access only MapDataToAccountReportObject
        }
        public AccountReportRunSet MapDataToAccountReportObject(LiteDbRunSet runSet)
        {
            AccountReportRunSet accountReportRunSet = new AccountReportRunSet();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<LiteDbReportBase, AccountReportBase>().ForMember(dest => dest.ElapsedEndTimeStamp, src => src.MapFrom(x => x.Elapsed)).IncludeAllDerived();
                cfg.CreateMap<LiteDbRunSet, AccountReportRunSet>().ForMember(dest => dest.ExecutedByUser, src => src.MapFrom(x => x.ExecutedbyUser));
                cfg.CreateMap<LiteDbRunner, AccountReportRunner>();

                cfg.CreateMap<LiteDbBusinessFlow, AccountReportBusinessFlow>();
                cfg.CreateMap<LiteDbActivityGroup, AccountReportActivityGroup>();
                cfg.CreateMap<LiteDbActivity, AccountReportActivity>();
                cfg.CreateMap<LiteDbAction, AccountReportAction>();
                //cfg.CreateMap<KeyValuePair<string, int>, AccountReport.Contracts.Helpers.DictObject>().ConvertUsing(x =>
                cfg.CreateMap<KeyValuePair<string, int>, AccountReport.Contracts.Helpers.DictObject>()
                .ForMember(dest => dest.Key, src => src.MapFrom(x => x.Key))
                .ForMember(dest => dest.Value, src => src.MapFrom(x => x.Value));

            });

            IMapper iMapper = config.CreateMapper();

            var destination = iMapper.Map<LiteDbRunSet, AccountReportRunSet>(runSet);
            return destination;
        }

        public async Task SendRunsetExecutionDataToCentralDBAsync(AccountReportRunSet accountReportRunSet, bool isUpdate = false)
        {
            if (restClient != null)
            {
                if (!isUpdate)
                {
                    Reporter.ToLog(eLogLevel.INFO, string.Format("Starting to publish execution data to central DB for Runset- '{0}'", accountReportRunSet.Name));
                }
                else
                {
                    Reporter.ToLog(eLogLevel.INFO, string.Format("Finishing to publish execution data to central DB for Runset- '{0}'", accountReportRunSet.Name));
                }
                string message = string.Format("execution data to Central DB for the Runset:'{0}' (Execution Id:'{1}')", accountReportRunSet.Name, accountReportRunSet.Id);
                bool responseIsSuccess = await SendRestRequestAndGetResponse(SEND_RUNSET_EXECUTION_DATA, accountReportRunSet, isUpdate).ConfigureAwait(false);
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

        public async Task SendRunnerExecutionDataToCentralDBAsync(AccountReportRunner accountReportRunner, bool isUpdate = false)
        {
            if (restClient != null)
            {
                string message = string.Format("execution data to Central DB for the Runner:'{0}' (Execution Id:'{1}', Parent Execution Id:'{2}')", accountReportRunner.Name, accountReportRunner.Id, accountReportRunner.AccountReportDbRunSetId);
                try
                {
                    bool responseIsSuccess = await SendRestRequestAndGetResponse(SEND_RUNNER_EXECUTION_DATA, accountReportRunner, isUpdate).ConfigureAwait(false);
                    if (responseIsSuccess)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Successfully sent " + message);
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + message);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception when sending " + message, ex);
                }
            }
        }

        public async Task SendBusinessflowExecutionDataToCentralDBAsync(AccountReportBusinessFlow accountReportBusinessFlow, bool isUpdate = false)
        {
            if (restClient != null)
            {
                string message = string.Format("execution data to Central DB for the Business Flow:'{0}' (Execution Id:'{1}', Parent Execution Id:'{2}')", accountReportBusinessFlow.Name, accountReportBusinessFlow.Id, accountReportBusinessFlow.AccountReportDbRunnerId);
                bool responseIsSuccess = await SendRestRequestAndGetResponse(SEND_BUSINESSFLOW_EXECUTION_DATA, accountReportBusinessFlow, isUpdate).ConfigureAwait(false);
                if (responseIsSuccess)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Successfully sent " + message);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + message);
                }
            }
        }

        public async Task SendActivityGroupExecutionDataToCentralDBAsync(AccountReportActivityGroup accountReportActivityGroup, bool isUpdate = false)
        {
            if (restClient != null)
            {
                RestRequest restRequest = (RestRequest)new RestRequest(SEND_ACTIVITYGROUP_EXECUTION_DATA, isUpdate ? Method.Put : Method.Post) { RequestFormat = RestSharp.DataFormat.Json }.AddJsonBody(accountReportActivityGroup);
                string message = string.Format("execution data to Central DB for the Activities Group:'{0}' (Execution Id:'{1}', Parent Execution Id:'{2}')", accountReportActivityGroup.Name, accountReportActivityGroup.Id, accountReportActivityGroup.AccountReportDbBusinessFlowId);
                try
                {
                    bool responseIsSuccess = await SendRestRequestAndGetResponse(SEND_ACTIVITYGROUP_EXECUTION_DATA, accountReportActivityGroup, isUpdate).ConfigureAwait(false);
                    if (responseIsSuccess)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Successfully sent " + message);
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + message);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception when sending " + message, ex);
                }
            }
        }

        public async Task SendActivityExecutionDataToCentralDBAsync(AccountReportActivity accountReportActivity, bool isUpdate = false)
        {
            if (restClient != null)
            {
                string message = string.Format("execution data to Central DB for the Activity:'{0}' (Execution Id:'{1}', Parent Execution Id:'{2}')", accountReportActivity.Name, accountReportActivity.Id, accountReportActivity.AccountReportDbActivityGroupId);
                try
                {
                    bool responseIsSuccess = await SendRestRequestAndGetResponse(SEND_ACTIVITY_EXECUTION_DATA, accountReportActivity, isUpdate).ConfigureAwait(false);
                    if (responseIsSuccess)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Successfully sent " + message);
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + message);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception when sending " + message, ex);
                }
            }
        }

        public async Task SendActionExecutionDataToCentralDBAsync(AccountReportAction accountReportAction, bool isUpdate = false)
        {
            if (restClient != null)
            {
                string message = string.Format("execution data to Central DB for the Action:'{0}' (Execution Id:'{1}', Parent Activity Id:'{2}')", accountReportAction.Name, accountReportAction.Id, accountReportAction.AccountReportDbActivityId);
                try
                {
                    bool responseIsSuccess = await SendRestRequestAndGetResponse(SEND_ACTION_EXECUTION_DATA, accountReportAction, isUpdate).ConfigureAwait(false);
                    if (responseIsSuccess)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Successfully sent " + message);
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + message);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception when sending " + message, ex);
                }
            }
        }

        public bool ExecutionIdValidation(Guid executionId)
        {
            if (restClient != null)
            {
                RestRequest restRequest = (RestRequest)new RestRequest(EXECUTION_ID_VALIDATION + executionId, Method.Get);
                string message = string.Format("execution id : {0}", executionId);
                try
                {
                    RestResponse response = restClient.Execute(restRequest);
                    if (response.IsSuccessful)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Successfully validated execution id " + message);
                        return Convert.ToBoolean(response.Content);
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to validate " + message + "Response: " + response.Content);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception while validating execution id " + message, ex);
                }
                return true;
            }
            return false;
        }

        public async Task SendScreenShotsToCentralDBAsync(Guid executionId, List<string> filePaths)
        {
            if (restClient != null)
            {
                if (filePaths != null && filePaths.Count > 0)
                {
                    try
                    {
                        Reporter.ToStatus(eStatusMsgKey.PublishingToCentralDB, null, "Uploading execution screenshots to central DB");

                        long fileSize = 0;

                        List<string> temp = new List<string>();

                        foreach (string screenshot in filePaths)
                        {
                            fileSize += new System.IO.FileInfo(screenshot).Length;
                            temp.Add(screenshot);
                            if ((5 * 1000000) < fileSize) // 5 MB
                            {
                                await UploadImageAsync(executionId, temp);
                                temp = new List<string>();
                                fileSize = 0;
                            }

                        }

                        if (temp.Any())
                        {
                            await UploadImageAsync(executionId, temp);
                        }
                    }
                    finally
                    {
                        Reporter.HideStatusMessage();
                    }
                }
            }
        }


        public async Task UploadImageAsync(Guid executionId, List<string> filePaths)
        {
            if (restClient != null)
            {
                string message = string.Format("screenshot/s to Central DB for Execution Id:'{0}'", executionId);
                try
                {
                    RestRequest restRequest = new RestRequest(UPLOAD_FILES, Method.Post);
                    restRequest.AddHeader("Content-Type", "multipart/form-data");
                    restRequest.AddHeader("ExecutionId", executionId.ToString());

                    foreach (string item in filePaths)
                    {
                        restRequest.AddFile(Path.GetFileName(item), item);
                    }
                    RestResponse response = await restClient.ExecuteAsync(restRequest);

                    if (response.IsSuccessful)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Successfully uploaded " + message);
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to upload " + message + "Response: " + response.Content);
                    }

                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception occured during uploading " + message, ex);
                }
            }
        }

        private async Task<bool> SendRestRequestAndGetResponse(string api, AccountReportBase accountReport, bool isUpdate = false)
        {
            try
            {
                Method method = isUpdate ? Method.Put : Method.Post;
                RestRequest restRequest = (RestRequest)new RestRequest(api, method) { RequestFormat = RestSharp.DataFormat.Json }.AddJsonBody(accountReport);
                RestResponse response = await restClient.ExecuteAsync(restRequest);
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
    }
}
