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
using Ginger.Reports;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.CentralExecutionLogger
{
    public class CentralExecutionLoggerHelper
    {
     

        private string EndPointUrl { get; set; }

        RestClient restClient;
        public CentralExecutionLoggerHelper(string apiUrl)
        {
            EndPointUrl = apiUrl;          
            restClient = new RestClient(apiUrl);
            restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
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

        public async Task SendRunsetExecutionDataToCentralDBAsync(AccountReportRunSet accountReportRunSet)
        {
            Reporter.ToStatus(eStatusMsgKey.PublishingToCentralDB, null,  "Publishing Execution data to central DB");
            RestRequest restRequest = (RestRequest)new RestRequest("/AccountReport/AddRunSetToAccountDB/", Method.POST) { RequestFormat = RestSharp.DataFormat.Json }.AddJsonBody(accountReportRunSet);
            string message = string.Format(" execution data to Central DB for Runset: {0} , Execution Id:{1}", accountReportRunSet.Name, accountReportRunSet.ExecutionId);
            try
            {            
                IRestResponse response = await restClient.ExecuteTaskAsync(restRequest);
                if(response.IsSuccessful)
                {                    
                    Reporter.ToLog(eLogLevel.INFO, "Successfully sent "+message);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.WARN, "Failed to send "+message +"Response: "+ response.Content);
                }                
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception when sending "+message, ex);
            }
            finally
            {
                Reporter.HideStatusMessage();
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
                    Reporter.ToLog(eLogLevel.WARN, "Failed to send " + message + "Response: " + response.Content);
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
