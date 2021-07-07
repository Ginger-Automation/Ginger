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
        public AccountReportApiHandler(string apiUrl)
        {
            EndPointUrl = apiUrl;          
            restClient = new RestClient(apiUrl);
            restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
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
            Reporter.ToLog(eLogLevel.DEBUG, "Publishing Execution data to central DB");
            RestRequest restRequest = (RestRequest)new RestRequest(SEND_RUNSET_EXECUTION_DATA, isUpdate ? Method.PUT : Method.POST) { RequestFormat = RestSharp.DataFormat.Json }.AddJsonBody(accountReportRunSet);
            string message = string.Format(" execution data to Central DB for Runset : ");
            try
            {            
                IRestResponse response = await restClient.ExecuteTaskAsync(restRequest);
                if(response.IsSuccessful)
                {                    
                    Reporter.ToLog(eLogLevel.INFO, "Successfully sent "+ message);                        
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send "+message +"Response: "+ response.Content);
                }                
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception when sending "+message, ex);
            }          
        }

        public async Task SendRunnerExecutionDataToCentralDBAsync(AccountReportRunner accountReportRunner, bool isUpdate = false)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Publishing Runner Execution data to central DB");
            RestRequest restRequest = (RestRequest)new RestRequest(SEND_RUNNER_EXECUTION_DATA, isUpdate ? Method.PUT : Method.POST) { RequestFormat = RestSharp.DataFormat.Json }.AddJsonBody(accountReportRunner);            
            string message = string.Format(" execution data to Central DB for Runner: {0} , Execution Id:{1}, Parent Execution Id : {2} ", accountReportRunner.Name, accountReportRunner.Id, accountReportRunner.AccountReportDbRunSetId);
            try
            {
                IRestResponse response = await restClient.ExecuteTaskAsync(restRequest);
                if (response.IsSuccessful)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Successfully sent " + message);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + message + "Response: " + response.Content);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception when sending " + message, ex);
            }           
        }

        public async Task SendBusinessflowExecutionDataToCentralDBAsync(AccountReportBusinessFlow accountReportBusinessFlow, bool isUpdate = false)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Publishing Businessflow Execution data to central DB");
            RestRequest restRequest = (RestRequest)new RestRequest(SEND_BUSINESSFLOW_EXECUTION_DATA, isUpdate ? Method.PUT : Method.POST) { RequestFormat = RestSharp.DataFormat.Json }.AddJsonBody(accountReportBusinessFlow);
            string message = string.Format(" execution data to Central DB for Businessflow: {0} , Execution Id:{1}, Parent Execution Id : {2}", accountReportBusinessFlow.Name, accountReportBusinessFlow.Id, accountReportBusinessFlow.AccountReportDbRunnerId);
            try
            {
                IRestResponse response = await restClient.ExecuteTaskAsync(restRequest);
                if (response.IsSuccessful)
                {
                    Reporter.ToLog(eLogLevel.INFO, "Successfully sent " + message);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + message + "Response: " + response.Content);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception when sending " + message, ex);
            }           
        }

        public async Task SendActivityGroupExecutionDataToCentralDBAsync(AccountReportActivityGroup accountReportActivityGroup, bool isUpdate = false)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Publishing ActivityGroup Execution data to central DB");
            RestRequest restRequest = (RestRequest)new RestRequest(SEND_ACTIVITYGROUP_EXECUTION_DATA, isUpdate ? Method.PUT : Method.POST) { RequestFormat = RestSharp.DataFormat.Json }.AddJsonBody(accountReportActivityGroup);
            string message = string.Format(" execution data to Central DB for ActivityGroup: {0} , Execution Id:{1}, Parent Execution Id: {2}", accountReportActivityGroup.Name, accountReportActivityGroup.Id, accountReportActivityGroup.AccountReportDbBusinessFlowId);
            try
            {
                IRestResponse response = await restClient.ExecuteTaskAsync(restRequest);
                if (response.IsSuccessful)
                {
                    Reporter.ToLog(eLogLevel.INFO, "Successfully sent " + message);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + message + "Response: " + response.Content);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception when sending " + message, ex);
            }            
        }

        public async Task SendActivityExecutionDataToCentralDBAsync(AccountReportActivity accountReportActivity, bool isUpdate = false)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Publishing Activity Execution data to central DB");
            RestRequest restRequest = (RestRequest)new RestRequest(SEND_ACTIVITY_EXECUTION_DATA, isUpdate ? Method.PUT : Method.POST) { RequestFormat = RestSharp.DataFormat.Json }.AddJsonBody(accountReportActivity);
            string message = string.Format(" execution data to Central DB for Activity: {0} , Execution Id:{1}, Parent Execution Id:{2}", accountReportActivity.Name, accountReportActivity.Id, accountReportActivity.AccountReportDbActivityGroupId);
            try
            {
                IRestResponse response = await restClient.ExecuteTaskAsync(restRequest);
                if (response.IsSuccessful)
                {
                    Reporter.ToLog(eLogLevel.INFO, "Successfully sent " + message);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + message + "Response: " + response.Content);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception when sending " + message, ex);
            }            
        }

        public async Task SendActionExecutionDataToCentralDBAsync(AccountReportAction accountReportAction, bool isUpdate = false)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Publishing Action Execution data to central DB");
            RestRequest restRequest = (RestRequest)new RestRequest(SEND_ACTION_EXECUTION_DATA, isUpdate ? Method.PUT : Method.POST) { RequestFormat = RestSharp.DataFormat.Json }.AddJsonBody(accountReportAction);
           // restRequest.Parameters.Add(new Parameter("IsUpdate", isUpdate, ParameterType.HttpHeader));
            string message = string.Format(" execution data to Central DB for Action: {0} , Execution Id:{1}, Parent Activity Id : {2} ", accountReportAction.Name, accountReportAction.Id, accountReportAction.AccountReportDbActivityId);
            try
            {
                IRestResponse response = await restClient.ExecuteTaskAsync(restRequest);
                if (response.IsSuccessful)
                {
                    Reporter.ToLog(eLogLevel.INFO, "Successfully sent " + message);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + message + "Response: " + response.Content);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception when sending " + message, ex);
            }          
        }

        public async Task SendScreenShotsToCentralDBAsync(Guid executionId, List<string> filePaths)
        {
            long fileSize = 0;
    
            List<string> temp = new List<string>();

            foreach (string screenshot in filePaths)
            {
                fileSize += new System.IO.FileInfo(screenshot).Length;
                temp.Add(screenshot);
                if ((5 * 1000000) < fileSize) // 5 MB
                {
                    await UploadImageAsync(executionId,temp);
                    temp = new List<string>();
                    fileSize = 0;
                }

            }

            if (temp.Any())
            {
                await UploadImageAsync(executionId, temp);
            }    
               

        }


        public async Task UploadImageAsync(Guid executionId,  List<string> filePaths)
        {
            string message = string.Format(" execution screenshots to Central DB for Execution Id:{0}", executionId);
            try
            {
                Reporter.ToStatus(eStatusMsgKey.PublishingToCentralDB,null,  "Publishing Execution screenshots to central DB");
                RestRequest restRequest = new RestRequest("/AccountReport/UploadFiles/",Method.POST);
                restRequest.AddHeader("Content-Type", "multipart/form-data");     
                restRequest.AddHeader("ExecutionId", executionId.ToString());

                foreach (string item in filePaths)
                {
                    restRequest.AddFile(Path.GetFileName(item), item);
                }
                IRestResponse response =  await restClient.ExecuteTaskAsync(restRequest);
              
                if (response.IsSuccessful)
                {
                    Reporter.ToLog(eLogLevel.INFO, "Successfully sent " + message);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to send " + message + "Response: " + response.Content);
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception when sending " + message, ex);
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
        }
    }
}
