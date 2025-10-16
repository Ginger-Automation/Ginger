using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.External.Configurations;
using Amdocs.Ginger.CoreNET.External.GingerPlay;
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
    }
}