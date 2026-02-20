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
using Amdocs.Ginger.CoreNET.External.GingerPlay;
using OpenQA.Selenium.DevTools.V137.Audits;
using System;

namespace GingerCoreNET.GeneralLib
{
    public static class GingerPlayUtils
    {
        /// <summary>
        /// Checks if Ginger Play is properly configured in the current solution.
        /// </summary>
        public static bool IsGingerPlayGatewayUrlConfigured()
        {
            GingerPlayConfiguration gingerPlayConfiguration = WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<GingerPlayConfiguration>();
            return gingerPlayConfiguration != null
                && (!string.IsNullOrEmpty(gingerPlayConfiguration.GingerPlayGatewayUrl) || !string.IsNullOrEmpty(gingerPlayConfiguration.CentralizedAccountReportURL) || !string.IsNullOrEmpty(gingerPlayConfiguration.CentralizedHTMLReportServiceURL
                ))
                && gingerPlayConfiguration.GingerPlayEnabled
                && gingerPlayConfiguration.GingerPlayReportServiceEnabled;
        }

        public static bool IsGingerPlayBackwardUrlConfigured()
        {
            GingerPlayConfiguration gingerPlayConfiguration = WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<GingerPlayConfiguration>();
            return gingerPlayConfiguration != null && (!string.IsNullOrEmpty(gingerPlayConfiguration.CentralizedAccountReportURL)
                   && !string.IsNullOrEmpty(gingerPlayConfiguration.CentralizedHTMLReportServiceURL));
        }

        public static string GetGingerPlayGatewayURLIfConfigured()
        {
            try
            {
                GingerPlayConfiguration gpConfig = WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<GingerPlayConfiguration>();
                if (gpConfig != null && !string.IsNullOrEmpty(gpConfig.GingerPlayGatewayUrl))
                {
                    return gpConfig.GingerPlayGatewayUrl;
                }

                return null;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Gateway URL is null or no configuration found", ex);
                return string.Empty;
            }
        }

        public static bool IsGingerPlayEnabled()
        {
            try
            {
                GingerPlayConfiguration gpConfig = WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<GingerPlayConfiguration>();
                if (gpConfig != null)
                {
                    return gpConfig.GingerPlayEnabled;
                }

                return false;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to check if Ginger Play is Enabled", ex);
                return false;
            }
        }
    }
}