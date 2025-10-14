using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.External.Configurations;
using Amdocs.Ginger.CoreNET.External.GingerPlay;

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
    }
}