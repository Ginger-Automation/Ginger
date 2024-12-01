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
            if (ApplicationConfig == null || (string.IsNullOrEmpty(ApplicationConfig.Name)))
            {
                throw new ArgumentException("Both Target Application GUID and Application Name cannot be null or empty. Atleast one of them should have a value");
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
                ? throw new InvalidOperationException($"The Application Platform : {application.Name} does not exist in the ginger solution. Please make sure that the Application Platform exists in the ginger solution")
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


        public static void UpdateEnvApplicationFromConfig(ref EnvApplication AppFromGinger, ApplicationConfig App)
        {
            ValidateApplicationConfig(App);

            AppFromGinger.Platform = (AppFromGinger.Platform.Equals(ePlatformType.NA)) ? ePlatformType.NA : GetTargetAppPlatformFromGinger(App);
            AppFromGinger.AppVersion = App.AppVersion;
            AppFromGinger.Url = App.URL;
            AppFromGinger.Name = App.Name;
        }
    }
}
