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
using Amdocs.Ginger.Common.External.Configurations;
using System;

namespace Amdocs.Ginger.CoreNET.GenAIServices
{
    public static class GingerPlayEndPointManager
    {
        private const string ACCOUNT_REPORT_SERVICE_URL = "ginger-report";

        private const string HTML_REPORT_SERVICE_URL = "ginger-html-report";

        private const string AI_SERVICE_URL = "ginger-ai";

        private const string EXECUTION_SERVICE = "ginger-execution";

        private static readonly GingerPlayConfiguration GingerPlayConfiguration = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerPlayConfiguration>().Count == 0
      ? new GingerPlayConfiguration()
      : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<GingerPlayConfiguration>();
        public static string GetAccountReportServiceUrl()
        {
            try
            {
                if (!string.IsNullOrEmpty(GingerPlayConfiguration.CentralizedAccountReportURL))
                {
                    return GingerPlayConfiguration.CentralizedAccountReportURL;
                }
                else if (!string.IsNullOrEmpty(GingerPlayConfiguration.GingerPlayGatewayUrl))
                {
                    return GingerPlayConfiguration.GingerPlayGatewayUrl + ACCOUNT_REPORT_SERVICE_URL;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while getting Account Report Service URL");
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to retrieve the Account Report Service URL. Please check the configuration.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while getting Account Report Service URL: {ex.Message}");
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to retrieve the Account Report Service URL. Please check the configuration.");
                return null;
            }

        }

        public static string GetHTMLReportServiceUrl()
        {
            try
            {
                if (!string.IsNullOrEmpty(GingerPlayConfiguration.CentralizedHTMLReportServiceURL))
                {
                    return GingerPlayConfiguration.CentralizedHTMLReportServiceURL;
                }
                else if (!string.IsNullOrEmpty(GingerPlayConfiguration.GingerPlayGatewayUrl))
                {
                    return GingerPlayConfiguration.GingerPlayGatewayUrl + HTML_REPORT_SERVICE_URL;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while getting HTML Report Service URL");
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to retrieve the HTML Report Service URL. Please check the configuration.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while getting Account Report Service URL: {ex.Message}");
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to retrieve the Account Report Service URL. Please check the configuration.");
                return null;
            }
        }

        public static string GetAIServiceUrl()
        {
            try
            {
                if (!string.IsNullOrEmpty(GingerPlayConfiguration.GingerPlayGatewayUrl))
                {
                    return GingerPlayConfiguration.GingerPlayGatewayUrl + AI_SERVICE_URL;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while getting AI Service URL");
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to retrieve the AI Service URL. Please check the configuration.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while getting AI Service URL: {ex.Message}");
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to retrieve the AI Service URL. Please check the configuration.");
                return null;
            }
        }

        public static string GetExecutionServiceUrl()
        {
            try
            {
                if (!string.IsNullOrEmpty(GingerPlayConfiguration.CentralizedExecutionHandlerURL))
                {
                    return GingerPlayConfiguration.CentralizedExecutionHandlerURL;
                }
                else if (!string.IsNullOrEmpty(GingerPlayConfiguration.GingerPlayGatewayUrl))
                {
                    return GingerPlayConfiguration.GingerPlayGatewayUrl + EXECUTION_SERVICE;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while getting Execution Service URL");
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to retrieve the Execution Service URL. Please check the configuration.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while getting Execution Service URL: {ex.Message}");
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to retrieve the Execution Service URL. Please check the configuration.");
                return null;
            }
        }
    }
}
