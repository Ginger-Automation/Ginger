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
using AccountReport.Contracts.ResponseModels;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.Reports;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;

namespace Amdocs.Ginger.CoreNET
{
    public class GingerRemoteExecutionUtils
    {
        public List<RunSetReport> GetSolutionRunsetsExecutionInfo(Guid soluionGuid)
        {
            var runSetReports = new List<RunSetReport>();
            var baseURI = GetReportDataServiceUrl();
            if (!string.IsNullOrEmpty(baseURI))
            {
                using (var httpClient = new HttpClient())
                {
                    var endpoint = baseURI + "api/AccountReport/GetRunsetsExecutionInfoBySolutionID/" + soluionGuid;
                    var response = httpClient.GetAsync(endpoint).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Error occured during GetSolutionRunsetsExecutionInfo() :" + response.Content.ReadAsStringAsync().Result.ToString());
                    }
                    else
                    {
                        runSetReports = ConvertResponsInRunsetReport(response.Content.ReadAsStringAsync().Result);
                    }
                }
            }
            return runSetReports;
        }

        public List<RunSetReport> GetRunsetExecutionInfo(Guid soluionGuid,Guid runsetGuid)
        {
            var runSetReports = new List<RunSetReport>();
            var baseURI = GetReportDataServiceUrl();
            if (!string.IsNullOrEmpty(baseURI))
            {
                using (var httpClient = new HttpClient())
                {
                    var endpoint = baseURI + "api/AccountReport/GetRunsetExecutionInfoByRunsetID/" + soluionGuid + "/" + runsetGuid;
                    var response = httpClient.GetAsync(endpoint).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Error occured during GetRunsetExecutionInfo() :" + response.Content.ReadAsStringAsync().Result.ToString());
                    }
                    else
                    {
                        runSetReports = ConvertResponsInRunsetReport(response.Content.ReadAsStringAsync().Result);
                    }
                }
            }
            return runSetReports;
        }
        private static string GetReportDataServiceUrl()
        {
            var baseURI = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().CentralizedReportDataServiceURL;

            if (!string.IsNullOrEmpty(baseURI) && !baseURI.EndsWith("/"))
            {
                baseURI += "/";
            }
            return baseURI;
        }
        private List<RunSetReport> ConvertResponsInRunsetReport(string result)
        {
            var runsetHLInfoResponses = JsonConvert.DeserializeObject<List<RunsetHLInfoResponse>>(result);
            List<RunSetReport> runSetReports = new List<RunSetReport>();

            foreach (var runsetHLInfo in runsetHLInfoResponses)
            {
                Enum.TryParse(runsetHLInfo.Status, out Execution.eRunStatus runStatus);
                runSetReports.Add(new RunSetReport()
                {
                    GUID = runsetHLInfo.ExecutionID.ToString(),
                    Name = runsetHLInfo.RunsetName,
                    Description = "",
                    StartTimeStamp = Convert.ToDateTime(runsetHLInfo.StartTime).ToUniversalTime(),
                    EndTimeStamp = Convert.ToDateTime(runsetHLInfo.EndTime).ToUniversalTime(),
                    Elapsed = runsetHLInfo.Duration,
                    ExecutionDurationHHMMSS = GingerCoreNET.GeneralLib.General.TimeConvert((runsetHLInfo.Duration/1000).ToString()),
                    RunSetExecutionStatus = runStatus,
                    DataRepMethod = ExecutionLoggerConfiguration.DataRepositoryMethod.Remote
                });
            }

            return runSetReports;
        }

        public void GenerateHTMLReport(string executionGuid)
        {
            var htmlServiceUrl = GetReportHTMLServiceUrl();
            if (!string.IsNullOrEmpty(htmlServiceUrl))
            {
                Process.Start(new ProcessStartInfo() { FileName = htmlServiceUrl + "?ExecutionId=" + executionGuid, UseShellExecute = true });
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Centralized Html Report Service URL is missing in General Report Configurations.Please Configure it, to generate the report.");
            }
        }
        private static string GetReportHTMLServiceUrl()
        {
            var baseURI = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().CentralizedHtmlReportServiceURL;

            if (!string.IsNullOrEmpty(baseURI))
            {
                if (!baseURI.EndsWith("/"))
                {
                    baseURI += "/";
                }
                if (!baseURI.EndsWith("#/"))
                {
                    baseURI += "#/";
                }
            }
            return baseURI;
        }
    }
}
