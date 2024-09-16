#region License
/*
Copyright © 2014-2024 European Support Limited

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
using System.Globalization;
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
            if (!string.IsNullOrEmpty(baseURI) && WorkSpace.Instance.Solution.LoggerConfigurations.PublishLogToCentralDB == ExecutionLoggerConfiguration.ePublishToCentralDB.Yes)
            {
                using (var httpClient = new HttpClient())
                {
                    var endpoint = baseURI + "api/AccountReport/GetRunsetsExecutionInfoBySolutionID/" + soluionGuid;
                    var response = httpClient.GetAsync(endpoint).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            Reporter.ToLog(eLogLevel.DEBUG, $"Not found Execution Info againts solutionGuid  : {soluionGuid} GetSolutionRunsetsExecutionInfo() :{response.Content.ReadAsStringAsync().Result.ToString()}");
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Error occurred during GetSolutionRunsetsExecutionInfo() :{response.Content.ReadAsStringAsync().Result.ToString()}");
                        }

                    }
                    else
                    {
                        runSetReports = ConvertResponsInRunsetReport(response.Content.ReadAsStringAsync().Result);
                    }
                }
            }
            return runSetReports;
        }
        public List<RunSetReport> GetRunsetExecutionInfo(Guid soluionGuid, Guid runsetGuid)
        {
            var runSetReports = new List<RunSetReport>();
            var baseURI = GetReportDataServiceUrl();
            if (!string.IsNullOrEmpty(baseURI) && WorkSpace.Instance.Solution.LoggerConfigurations.PublishLogToCentralDB == ExecutionLoggerConfiguration.ePublishToCentralDB.Yes)
            {
                using (var httpClient = new HttpClient())
                {
                    var endpoint = baseURI + "api/AccountReport/GetRunsetExecutionInfoByRunsetID/" + soluionGuid + "/" + runsetGuid;
                    var response = httpClient.GetAsync(endpoint).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            Reporter.ToLog(eLogLevel.DEBUG, $"Not found Execution Info againts runsetGuid  : {runsetGuid} GetSolutionRunsetsExecutionInfo() :{response.Content.ReadAsStringAsync().Result.ToString()}");

                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Error occurred during GetSolutionRunsetsExecutionInfo() :{response.Content.ReadAsStringAsync().Result.ToString()}");
                        }

                    }
                    else
                    {
                        runSetReports = ConvertResponsInRunsetReport(response.Content.ReadAsStringAsync().Result);
                    }
                }
            }
            return runSetReports;
        }
        public static string GetReportDataServiceUrl()
        {
            var baseURI = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.FirstOrDefault(x => (x.IsSelected)).CentralLoggerEndPointUrl;

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
                    RunSetGuid = runsetHLInfo.EntityId,
                    Name = runsetHLInfo.RunsetName,
                    Description = "",
                    StartTimeStamp = DateTime.Parse(runsetHLInfo.StartTime, CultureInfo.InvariantCulture).ToUniversalTime(),
                    EndTimeStamp = DateTime.Parse(runsetHLInfo.EndTime, CultureInfo.InvariantCulture).ToUniversalTime(),
                    Elapsed = runsetHLInfo.Duration,
                    ExecutionDurationHHMMSS = GingerCoreNET.GeneralLib.General.TimeConvert((runsetHLInfo.Duration / 1000).ToString()),
                    RunSetExecutionStatus = runStatus,
                    DataRepMethod = ExecutionLoggerConfiguration.DataRepositoryMethod.Remote,
                    SourceApplication = runsetHLInfo.SourceApplication,
                    SourceApplicationUser = runsetHLInfo.SourceApplicationUser
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
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Centralized Html Report Service URL is missing in General Report Configurations.\nPlease go to Configurations > Reports > Execution Logger Configurations to configure the HTML Report URL");
            }
        }
        public static string GetReportHTMLServiceUrl()
        {
            var baseURI = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.FirstOrDefault(x => (x.IsSelected)).CentralizedHtmlReportServiceURL;

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

        public static string GetOnlineHTMLReportlink(Guid? executionGuid)
        {
            var htmlServiceUrl = GetReportHTMLServiceUrl();
            if (!string.IsNullOrEmpty(htmlServiceUrl))
            {
                return htmlServiceUrl + "?ExecutionId=" + executionGuid;
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, "Centralized Html Report Service URL is missing in General Report Configurations.\nPlease go to Configurations > Reports > Execution Logger Configurations to configure the HTML Report URL");
                return string.Empty;
            }
        }

    }
}
