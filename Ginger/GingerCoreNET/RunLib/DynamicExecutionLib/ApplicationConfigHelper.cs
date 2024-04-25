using amdocs.ginger.GingerCoreNET;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib
{
    public static class ApplicationConfigHelper
    {
        private static void ValidateApplicationConfig(ApplicationConfig ApplicationConfig)
        {
            if(ApplicationConfig == null || (string.IsNullOrEmpty(ApplicationConfig.Name)))
            { 
                throw new ArgumentException("Both Target Application GUID and Applicaition Name cannot be null or empty. Atleast one of them should have a value");
            }
        }

        private static ePlatformType GetTargetAppPlatformFromGinger(ApplicationConfig application)
        {

             var TargetApplicationInGinger = WorkSpace.Instance.Solution.ApplicationPlatforms
                .FirstOrDefault((ApplicationPlatform) =>
                {
                    return ApplicationPlatform.AppName.Equals(application.Name);
                });

            return TargetApplicationInGinger == null
                ? throw new InvalidOperationException("The mention Application Platform does not exist in ginger. Please make sure that the mentioned Application Platform exists in ginger")
                : TargetApplicationInGinger.Platform;
        }

        public static EnvApplication CreateEnvApplicationFromConfig(ApplicationConfig application)
        {
            ValidateApplicationConfig(application);
            EnvApplication envApplication = new()
            {
                Name = application.Name,
                AppVersion = application.AppVersion,
                Url = application.URL,
                Platform = GetTargetAppPlatformFromGinger(application),
            };
            
            return envApplication;
        }


        public static void UpdateEnvApplicationFromConfig(ref EnvApplication AppFromGinger , ApplicationConfig App)
        {
            ValidateApplicationConfig(App);

            AppFromGinger.Platform = GetTargetAppPlatformFromGinger(App);
            AppFromGinger.AppVersion = App.AppVersion;
            AppFromGinger.Url = App.URL;
            AppFromGinger.Name = App.Name;
        }
    }
}
