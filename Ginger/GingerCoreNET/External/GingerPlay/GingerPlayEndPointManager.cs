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
using Ginger.Reports;
using System;

namespace Amdocs.Ginger.CoreNET.External.GingerPlay
{
    public static class GingerPlayEndPointManager
    {
        private static readonly string ACCOUNT_REPORT_SERVICE_URL = System.Configuration.ConfigurationManager.AppSettings["ACCOUNT_REPORT_SERVICE_URL"].ToString();

        private static readonly string HTML_REPORT_SERVICE_URL = System.Configuration.ConfigurationManager.AppSettings["HTML_REPORT_SERVICE_URL"].ToString();

        private static readonly string AI_SERVICE_URL = System.Configuration.ConfigurationManager.AppSettings["AI_SERVICE_URL"].ToString();

        private static readonly string EXECUTION_SERVICE = System.Configuration.ConfigurationManager.AppSettings["EXECUTION_SERVICE"].ToString();

        private static readonly string IDENTITY_SERVICE = System.Configuration.ConfigurationManager.AppSettings["IDENTITY_SERVICE"].ToString();

        private static readonly string REPORT_SERVICE_HEALTH_PATH = $"{ACCOUNT_REPORT_SERVICE_URL}/health";
        private static readonly string EXECUTION_SERVICE_HEALTH_PATH = $"{EXECUTION_SERVICE}/health";
        private static readonly string AI_SERVICE_HEALTH_PATH = $"{AI_SERVICE_URL}/health";
        private static readonly string GENERATE_TOKEN_URL = $"{IDENTITY_SERVICE}/connect/token";
        private static readonly string EXECUTION_SERVICE_GENERATIVEPOM_URL = "generative-ai/extract_dom_elements";


        private static readonly ExecutionLoggerConfiguration LoggerConfig = WorkSpace.Instance.Solution.LoggerConfigurations;


        private static GingerPlayConfiguration _gingerPlayConfiguration;
        private static GingerPlayConfiguration GingerPlayConfiguration
        {
            get
            {
                if (_gingerPlayConfiguration == null && WorkSpace.Instance?.SolutionRepository != null)
                {
                    _gingerPlayConfiguration = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerPlayConfiguration>().Count == 0
                        ? new GingerPlayConfiguration()
                        : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<GingerPlayConfiguration>();
                }
                return _gingerPlayConfiguration ?? new GingerPlayConfiguration();
            }
        }

        public static string GetAccountReportServiceUrl()
        {
            try
            {
                if (!string.IsNullOrEmpty(GingerPlayConfiguration.GingerPlayGatewayUrl) && GingerPlayConfiguration.GingerPlayReportServiceEnabled)
                {
                    return GingerPlayConfiguration.GingerPlayGatewayUrl + ACCOUNT_REPORT_SERVICE_URL;
                }
                else if (!string.IsNullOrEmpty(GingerPlayConfiguration.CentralizedAccountReportURL))
                {
                    return GingerPlayConfiguration.CentralizedAccountReportURL;
                }
                else if (!string.IsNullOrEmpty(LoggerConfig.GetCentralLoggerEndPointURLBackwardCompatibility()))
                {
                    // If the Central Logger URL is set, use it as the Account Report Service URL
                    GingerPlayConfiguration.CentralizedAccountReportURL = LoggerConfig.GetCentralLoggerEndPointURLBackwardCompatibility();
                    GingerPlayConfiguration.GingerPlayEnabled = true;
                    GingerPlayConfiguration.GingerPlayReportServiceEnabled = true;
                    return GingerPlayConfiguration.CentralizedAccountReportURL;
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
                if (!string.IsNullOrEmpty(GingerPlayConfiguration.GingerPlayGatewayUrl) && GingerPlayConfiguration.GingerPlayReportServiceEnabled)
                {
                    return GingerPlayConfiguration.GingerPlayGatewayUrl + HTML_REPORT_SERVICE_URL;
                }
                else if (!string.IsNullOrEmpty(GingerPlayConfiguration.CentralizedHTMLReportServiceURL))
                {
                    return GingerPlayConfiguration.CentralizedHTMLReportServiceURL;
                }
                else if (!string.IsNullOrEmpty(LoggerConfig.GetCentralizedHtmlReportServiceURLBackwardCompatibility()))
                {
                    GingerPlayConfiguration.CentralizedHTMLReportServiceURL = LoggerConfig.GetCentralizedHtmlReportServiceURLBackwardCompatibility();
                    GingerPlayConfiguration.GingerPlayEnabled = true;
                    GingerPlayConfiguration.GingerPlayReportServiceEnabled = true;
                    return GingerPlayConfiguration.CentralizedHTMLReportServiceURL;
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
                if (!string.IsNullOrEmpty(GingerPlayConfiguration.GingerPlayGatewayUrl) && GingerPlayConfiguration.GingerPlayAIServiceEnabled)
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
                if (!string.IsNullOrEmpty(GingerPlayConfiguration.GingerPlayGatewayUrl) && GingerPlayConfiguration.GingerPlayExecutionServiceEnabled)
                {
                    return GingerPlayConfiguration.GingerPlayGatewayUrl + EXECUTION_SERVICE;
                }
                else if (!string.IsNullOrEmpty(GingerPlayConfiguration.CentralizedExecutionHandlerURL))
                {
                    return GingerPlayConfiguration.CentralizedExecutionHandlerURL;
                }
                else if (!string.IsNullOrEmpty(LoggerConfig.GetExecutionServiceURLBackwardCompatibility()))
                {
                    // If the Central Logger URL is set, use it as the Execution Service URL
                    GingerPlayConfiguration.CentralizedExecutionHandlerURL = LoggerConfig.GetExecutionServiceURLBackwardCompatibility();
                    GingerPlayConfiguration.GingerPlayEnabled = true;
                    GingerPlayConfiguration.GingerPlayExecutionServiceEnabled = true;
                    return GingerPlayConfiguration.CentralizedExecutionHandlerURL;
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

        public static string GetGenerateTokenUrl()
        {
            return BuildServiceUrl(GENERATE_TOKEN_URL);
        }

        public static string GetReportServiceHealthUrl()
        {
            return BuildServiceUrl(REPORT_SERVICE_HEALTH_PATH);
        }

        public static string GetExecutionServiceHealthUrl()
        {
            return BuildServiceUrl(EXECUTION_SERVICE_HEALTH_PATH);
        }

        public static string GetAIServiceHealthUrl()
        {
            return BuildServiceUrl(AI_SERVICE_HEALTH_PATH);
        }

        public static string GetAIServicePOMExtractpath()
        {
            return EXECUTION_SERVICE_GENERATIVEPOM_URL;
        }

        private static string BuildServiceUrl(string path)
        {
            var baseUrl = GingerPlayConfiguration.GingerPlayGatewayUrl;
            if (string.IsNullOrEmpty(baseUrl))
            {
                return null;
            }
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }
            return baseUrl + path;
        }
    }
}
