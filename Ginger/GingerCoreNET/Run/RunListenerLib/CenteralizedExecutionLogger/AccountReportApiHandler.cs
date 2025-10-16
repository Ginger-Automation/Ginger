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
using AccountReport.Contracts.ResponseModels;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.CoreNET.External.GingerPlay;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using AutoMapper;
using Ginger.Reports;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
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
        private const string UPLOAD_ARTIFACTS = "api/AccountReport/UploadArtifacts/";
        private const string EXECUTION_ID_VALIDATION = "api/HtmlReport/ExecutionIdValidation/";
        private const string GET_BUSINESSFLOW_EXECUTION_DATA = "api/HtmlReport/GetAccountReportBusinessflowsByExecutionId/";
        private const string GET_RUNSET_EXECUTION_DATA = "api/HtmlReport/GetRunsetHLExecutionInfo/";
        private const string GET_RUNNER_EXECUTION_DATA = "api/HtmlReport/GetAccountReportRunnersByExecutionId/";
        private const string GET_ACCOUNT_HTML_REPORT = "/api/HtmlReport/GetAccountHtmlReport/";
        private const string SEND_EXECUTIONLOG = "api/AccountReport/executionlog/";
        private const string GET_RUNSET_EXECUTION_DATA_RUNSET_ID = "api/HtmlReport/GetRunsetHLExecutionInfoByRunsetId/";
        private const string GET_RUNSET_EXECUTION_DATA_SOLUTION_ID = "api/HtmlReport/GetRunsetsExecutionInfoBySolutionID/";
        // Instance-level flag indicating the RunSet was successfully sent to the central DB.
        // Volatile to ensure visibility across threads. Prefer per-execution tracking if multiple
        // concurrent runs are supported in the same process.
        private volatile bool _isRunSetDataSent = true;

        private readonly Guid LogId = Guid.NewGuid();

        public AccountReportApiHandler(string apiUrl)
        {
            if (!string.IsNullOrEmpty(apiUrl))
            {
                EndPointUrl = apiUrl;
                try
                {
                    var options = new RestClientOptions(apiUrl)
                    {
                        ThrowOnAnyError = false,
                        RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                        FailOnDeserializationError = false,
                        ThrowOnDeserializationError = false
                    };

                    restClient = new RestClient(options);

                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Centralized DB endpoint url is Invalid", ex);
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

        public bool IsConfigurationChanged()
        {
            return !string.Equals(EndPointUrl, GingerPlayEndPointManager.GetAccountReportServiceUrl());
        }
        public async Task<bool> SendRunsetExecutionDataToCentralDBAsync(AccountReportRunSet accountReportRunSet, bool isUpdate = false)
        {
            if (restClient != null)
            {
                if (!isUpdate)
                {
                    Reporter.ToLog(eLogLevel.INFO, string.Format("Starting to publish execution data to central DB for Runset- '{0}'", accountReportRunSet.Name));
                    _isRunSetDataSent = true; //Resetting the flag again
                }
                else
                {
                    if (!_isRunSetDataSent)
                    {
                        // If this is an update and the RunSet was not previously sent successfully,
                        // do not allow finishing update.
                        return false;
                    }
                    Reporter.ToLog(eLogLevel.INFO, string.Format("Finishing to publish execution data to central DB for Runset- '{0}'", accountReportRunSet.Name));
                }
                string message = string.Format("execution data to Central DB for the Runset:'{0}' (Execution Id:'{1}')", accountReportRunSet.Name, accountReportRunSet.ExecutionId);
                bool isResponseSuccessful = false;
                // Awaiting SendRestRequestAndGetResponse works fine on Windows but crashes on Linux runtime due to an issue related to RestSharp; need to replace it.
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    isResponseSuccessful = await SendRestRequestAndGetResponse(SEND_RUNSET_EXECUTION_DATA, accountReportRunSet, isUpdate).ConfigureAwait(false);
                }
                else
                {
                    isResponseSuccessful = SendRestRequestAndGetResponse(SEND_RUNSET_EXECUTION_DATA, accountReportRunSet, isUpdate).Result;
                }

                if (isResponseSuccessful)
                {
                    // Mark flag true only after successful send
                    _isRunSetDataSent = true;
                    Reporter.ToLog(eLogLevel.INFO, $"Successfully sent {message}");
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to send data for RunSet Execution {message}, Further execution data won't be sent");
                    _isRunSetDataSent = false;
                }
                return isResponseSuccessful;
            }
            else
            {
                Reporter.ToLog(eLogLevel.WARN, "Rest Client is null as endpoint url is not provided");
            }

            return false;
        }

        public async Task SendRunnerExecutionDataToCentralDBAsync(AccountReportRunner accountReportRunner, bool isUpdate = false)
        {
            if (restClient != null && _isRunSetDataSent)
            {
                string message = string.Format("execution data to Central DB for the Runner:'{0}' (Execution Id:'{1}', Parent Execution Id:'{2}')", accountReportRunner.Name, accountReportRunner.Id, accountReportRunner.AccountReportDbRunSetId);
                try
                {
                    bool responseIsSuccess = await SendRestRequestAndGetResponse(SEND_RUNNER_EXECUTION_DATA, accountReportRunner, isUpdate).ConfigureAwait(false);
                    if (responseIsSuccess)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, $"Successfully sent {message}");
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to send {message}");
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Exception when sending {message}", ex);
                }
            }
        }

        public async Task SendBusinessflowExecutionDataToCentralDBAsync(AccountReportBusinessFlow accountReportBusinessFlow, bool isUpdate = false)
        {
            if (restClient != null && _isRunSetDataSent)
            {
                string message = string.Format("execution data to Central DB for the Business Flow:'{0}' (Execution Id:'{1}', Parent Execution Id:'{2}')", accountReportBusinessFlow.Name, accountReportBusinessFlow.Id, accountReportBusinessFlow.AccountReportDbRunnerId);
                bool responseIsSuccess = await SendRestRequestAndGetResponse(SEND_BUSINESSFLOW_EXECUTION_DATA, accountReportBusinessFlow, isUpdate).ConfigureAwait(false);
                if (responseIsSuccess)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"Successfully sent {message}");
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to send {message}");
                }
            }
        }

        public async Task SendActivityGroupExecutionDataToCentralDBAsync(AccountReportActivityGroup accountReportActivityGroup, bool isUpdate = false)
        {
            if (restClient != null && _isRunSetDataSent)
            {
                RestRequest restRequest = new RestRequest(SEND_ACTIVITYGROUP_EXECUTION_DATA, isUpdate ? Method.Put : Method.Post) { RequestFormat = RestSharp.DataFormat.Json }.AddJsonBody(accountReportActivityGroup);
                string message = string.Format("execution data to Central DB for the Activities Group:'{0}' (Execution Id:'{1}', Parent Execution Id:'{2}')", accountReportActivityGroup.Name, accountReportActivityGroup.Id, accountReportActivityGroup.AccountReportDbBusinessFlowId);
                try
                {
                    bool responseIsSuccess = await SendRestRequestAndGetResponse(SEND_ACTIVITYGROUP_EXECUTION_DATA, accountReportActivityGroup, isUpdate).ConfigureAwait(false);
                    if (responseIsSuccess)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, $"Successfully sent {message} ");
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to send {message}");
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Exception when sending {message}", ex);
                }
            }
        }

        public async Task SendActivityExecutionDataToCentralDBAsync(AccountReportActivity accountReportActivity, bool isUpdate = false)
        {
            if (restClient != null && _isRunSetDataSent)
            {
                string message = string.Format("execution data to Central DB for the Activity:'{0}' (Execution Id:'{1}', Parent Execution Id:'{2}')", accountReportActivity.Name, accountReportActivity.Id, accountReportActivity.AccountReportDbActivityGroupId);
                try
                {
                    bool responseIsSuccess = await SendRestRequestAndGetResponse(SEND_ACTIVITY_EXECUTION_DATA, accountReportActivity, isUpdate).ConfigureAwait(false);
                    if (responseIsSuccess)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, $"Successfully sent {message}");
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to send {message}");
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Exception when sending {message}", ex);
                }
            }
        }

        public async Task SendActionExecutionDataToCentralDBAsync(AccountReportAction accountReportAction, bool isUpdate = false)
        {
            if (restClient != null && _isRunSetDataSent)
            {
                string message = string.Format("execution data to Central DB for the Action:'{0}' (Execution Id:'{1}', Parent Activity Id:'{2}')", accountReportAction.Name, accountReportAction.Id, accountReportAction.AccountReportDbActivityId);
                try
                {
                    bool responseIsSuccess = await SendRestRequestAndGetResponse(SEND_ACTION_EXECUTION_DATA, accountReportAction, isUpdate).ConfigureAwait(false);
                    if (responseIsSuccess)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, $"Successfully sent {message}");
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to send {message}");
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Exception when sending {message}", ex);
                }
            }
        }

        public bool ExecutionIdValidation(Guid executionId)
        {
            if (restClient != null)
            {
                RestRequest restRequest = new RestRequest(EXECUTION_ID_VALIDATION + executionId, Method.Get);
                string message = string.Format("execution id : {0}", executionId);
                try
                {
                    RestResponse response = restClient.Execute(restRequest);
                    if (response.IsSuccessful)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, $"Successfully validated execution id {message}");
                        return Convert.ToBoolean(response.Content);
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to validate {message} Response: {response.Content}");
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Exception while validating execution id {message}", ex);
                }
                return true;
            }
            return false;
        }

        public async Task SendScreenShotsToCentralDBAsync(Guid executionId, List<string> filePaths)
        {
            if (restClient != null && _isRunSetDataSent)
            {
                if (filePaths != null && filePaths.Count > 0)
                {
                    try
                    {
                        Reporter.ToStatus(eStatusMsgKey.PublishingToCentralDB, null, "Uploading execution screenshots to central DB");

                        long fileSize = 0;

                        List<string> temp = [];

                        foreach (string screenshot in filePaths)
                        {
                            fileSize += new System.IO.FileInfo(screenshot).Length;
                            temp.Add(screenshot);
                            if ((5 * 1000000) < fileSize) // 5 MB
                            {
                                await UploadImageAsync(executionId, temp);
                                temp = [];
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
            if (restClient != null && _isRunSetDataSent)
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
                        Reporter.ToLog(eLogLevel.DEBUG, $"Successfully uploaded {message}");
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to upload {message} Response: {response.Content}");
                    }

                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Exception occurred during uploading {message}", ex);
                }
            }
        }

        public async Task SendArtifactsToCentralDBAsync(Guid executionId, ObservableList<ArtifactDetails> artifactDetails)
        {
            if (restClient != null && _isRunSetDataSent)
            {
                if (artifactDetails != null && artifactDetails.Count > 0)
                {
                    try
                    {
                        Reporter.ToStatus(eStatusMsgKey.PublishingToCentralDB, null, "Uploading execution artifacts to central DB");

                        long fileSize = 0;

                        List<ArtifactDetails> temp = [];

                        foreach (ArtifactDetails artifact in artifactDetails)
                        {
                            if (File.Exists(artifact.ArtifactReportStoragePath))
                            {
                                fileSize += new System.IO.FileInfo(artifact.ArtifactReportStoragePath).Length;
                                temp.Add(artifact);
                                if ((5 * 1000000) < fileSize) // 5 MB
                                {
                                    await UploadArtifactsAsync(executionId, temp);
                                    temp = [];
                                    fileSize = 0;
                                }
                            }

                        }

                        if (temp.Any())
                        {
                            await UploadArtifactsAsync(executionId, temp);
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Exception occurred during uploading artifacts", ex);
                    }
                    finally
                    {
                        Reporter.HideStatusMessage();
                    }
                }
            }
        }


        public async Task UploadArtifactsAsync(Guid executionId, List<ArtifactDetails> artifactDetails)
        {
            if (restClient != null && _isRunSetDataSent)
            {
                string message = string.Format("artifact/s to Central DB for Execution Id:'{0}'", executionId);
                try
                {
                    RestRequest restRequest = new RestRequest(UPLOAD_ARTIFACTS, Method.Post);
                    restRequest.AddHeader("Content-Type", "multipart/form-data");
                    restRequest.AddHeader("ExecutionId", executionId.ToString());

                    foreach (ArtifactDetails artifact in artifactDetails)
                    {
                        restRequest.AddFile(artifact.ArtifactReportStorageName, artifact.ArtifactReportStoragePath);
                    }
                    RestResponse response = await restClient.ExecuteAsync(restRequest);

                    if (response.IsSuccessful)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, $"Successfully uploaded {message}");
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to upload {message} Response: {response.Content}");
                    }

                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Exception occurred during uploading {message}", ex);
                }
            }
        }

        private async Task<bool> SendRestRequestAndGetResponse(string api, AccountReportBase accountReport, bool isUpdate = false)
        {
            try
            {
                Method method = isUpdate ? Method.Put : Method.Post;
                RestRequest restRequest = new RestRequest(api, method) { RequestFormat = RestSharp.DataFormat.Json }.AddJsonBody(accountReport);

                RestResponse response = await restClient.ExecuteAsync(restRequest);
                if (response.IsSuccessful)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"Successfully sent {api}");
                    return true;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to send {api} Response:{response.Content}", response.ErrorException);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Exception when sending {api}", ex);
                return false;
            }
        }

        public List<AccountReportBusinessFlow> GetBusinessflowExecutionDataFromCentralDB(Guid executionId)
        {
            List<AccountReportBusinessFlow> accountReportBusinessFlows = [];
            if (restClient != null)
            {
                RestRequest restRequest = new RestRequest(GET_BUSINESSFLOW_EXECUTION_DATA + executionId, Method.Get);
                string message = string.Format("execution id : {0}", executionId);
                try
                {
                    RestResponse response = restClient.Execute(restRequest);
                    if (response.IsSuccessful)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, $"Successfully validated execution id {message}");
                        accountReportBusinessFlows = JsonConvert.DeserializeObject<List<AccountReportBusinessFlow>>(response.Content);
                        return accountReportBusinessFlows;
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to validate {message} Response: {response.Content} ");
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Exception while validating execution id {message}", ex);
                }
                return accountReportBusinessFlows;
            }
            return accountReportBusinessFlows;
        }

        public List<RunsetHLInfoResponse> GetRunsetExecutionDataFromCentralDB(Guid executionId)
        {
            List<RunsetHLInfoResponse> accountReportrunset = [];
            if (restClient != null)
            {
                RestRequest restRequest = new RestRequest(GET_RUNSET_EXECUTION_DATA + executionId, Method.Get);
                string message = string.Format("execution id : {0}", executionId);
                try
                {
                    RestResponse response = restClient.Execute(restRequest);
                    if (response.IsSuccessful)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, $"Successfully validated execution id {message}");
                        accountReportrunset = JsonConvert.DeserializeObject<List<RunsetHLInfoResponse>>(response.Content);
                        return accountReportrunset;
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to validate {message} Response: {response.Content} ");
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Exception while validating execution id {message}", ex);
                }
                return accountReportrunset;
            }
            return accountReportrunset;
        }

        public string GetRunsetExecutionDataByRunSetIDFromCentralDB(Guid solutionId,Guid runSetId)
        {
            if (restClient != null)
            {
                var path = $"{GET_RUNSET_EXECUTION_DATA_RUNSET_ID}{solutionId}/{runSetId}/";
                RestRequest restRequest = new RestRequest(path, Method.Get);
                string message = string.Format("solution id : {0} runSetId :{1}", solutionId,runSetId);
                try
                {
                    RestResponse response = restClient.Execute(restRequest);
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            Reporter.ToLog(eLogLevel.DEBUG, $"Not found Execution Info againts runsetGuid  : {runSetId} GetSolutionRunsetsExecutionInfo() :{response}");

                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Error occurred during GetSolutionRunsetsExecutionInfo() :{response}");
                        }
                    }
                    else
                    {
                        return response.Content;
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Exception while validating execution id {message}", ex);
                }
                return string.Empty;
            }
            return string.Empty;
        }

        public string GetRunsetExecutionDataBySolutionIDFromCentralDB(Guid solutionId)
        {
            if (restClient != null)
            {
                var path = $"{GET_RUNSET_EXECUTION_DATA_SOLUTION_ID}{solutionId}/";
                RestRequest restRequest = new RestRequest(path, Method.Get);
                string message = string.Format("solution id : {0}", solutionId);
                try
                {
                    RestResponse response = restClient.Execute(restRequest);
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            Reporter.ToLog(eLogLevel.DEBUG, $"Not found Execution Info againts solutionGuid  : {solutionId} GetSolutionRunsetsExecutionInfo() :{response}");

                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Error occurred during GetSolutionRunsetsExecutionInfo() :{response}");
                        }
                    }
                    else
                    {
                        return response.Content;
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Exception while validating execution id {message}", ex);
                }
                return string.Empty;
            }
            return string.Empty;
        }

        public List<AccountReportRunner> GetRunnerExecutionDataFromCentralDB(Guid executionId)
        {
            List<AccountReportRunner> accountReportrunset = [];
            if (restClient != null)
            {
                RestRequest restRequest = new RestRequest(GET_RUNNER_EXECUTION_DATA + executionId, Method.Get);
                string message = string.Format("execution id : {0}", executionId);
                try
                {
                    RestResponse response = restClient.Execute(restRequest);
                    if (response.IsSuccessful)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, $"Successfully validated execution id {message}");
                        accountReportrunset = JsonConvert.DeserializeObject<List<AccountReportRunner>>(response.Content);
                        return accountReportrunset;
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to validate {message} Response: {response.Content} ");
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Exception while validating execution id {message}", ex);
                }
                return accountReportrunset;
            }
            return accountReportrunset;
        }

        public async Task<AccountReportRunSetClient> GetAccountHTMLReportAsync(Guid executionId)
        {
            if (restClient == null)
            {
                return null!;
            }

            RestRequest request = new($"{GET_ACCOUNT_HTML_REPORT}{executionId}", Method.Get);
            try
            {
                RestResponse response = await restClient.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<AccountReportRunSetClient>(response.Content);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to get account HTML report for execution id '{executionId}'.");
                    return null!;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to get account HTML report for execution id '{executionId}'.", ex);
                return null!;
            }
        }

        public async Task<bool> SendExecutionLogToCentralDBAsync(string apiUrl, AccountReport.Contracts.RequestModels.ExecutionLogRequest executionLogRequest)
        {
            bool isSuccess = false;
            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                Reporter.ToLog(eLogLevel.ERROR, "SendExecutionLogToCentralDBAsync: ApiUrl is required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(executionLogRequest.LogData))
            {
                Reporter.ToLog(eLogLevel.DEBUG, "SendExecutionLogToCentralDBAsync: logData is empty, skipping");
                return false;
            }

            if (restClient != null && _isRunSetDataSent)
            {
                executionLogRequest.LogId = LogId;
                string message = string.Format("execution log to Central DB (API URL:'{0}', Execution Id:'{1}', Instance Id:'{2}', Log Id:'{3}')", apiUrl, executionLogRequest.ExecutionId, executionLogRequest.InstanceId, executionLogRequest.LogId);
                try
                {
                    string FinalAPIUrl = $"{apiUrl.TrimEnd('/')}/{SEND_EXECUTIONLOG}";
                    bool IsSuccessful = await SendExecutionLogRestRequestAndGetResponse(FinalAPIUrl, executionLogRequest).ConfigureAwait(false);
                    if (IsSuccessful)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, $"Successfully sent {message}");
                        return true;
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to send {message}");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Exception when sending {message}", ex);
                    return false;
                }
            }
            return isSuccess;
        }

        private async Task<bool> SendExecutionLogRestRequestAndGetResponse(string api, AccountReport.Contracts.RequestModels.ExecutionLogRequest payload)
        {
            try
            {
                Method method = Method.Post;
                RestRequest restRequest = new RestRequest(api, method)
                {
                    RequestFormat = RestSharp.DataFormat.Json
                }.AddJsonBody(payload);

                RestResponse response = await restClient.ExecuteAsync(restRequest);
                if (response.IsSuccessful)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"Successfully sent {api}");
                    return true;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to send {api} Response:{response.Content}", response.ErrorException);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Exception when sending {api}", ex);
                return false;
            }
        }
    }
}
