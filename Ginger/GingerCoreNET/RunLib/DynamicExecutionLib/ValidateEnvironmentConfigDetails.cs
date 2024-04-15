using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using GingerCore.Environments;
using System;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib
{
    public class ValidateEnvironmentConfigDetails
    {

        public static void ValidateEnvironmentConfig(ObservableList<ProjEnvironment> AllEnvironmentsInGinger, EnvironmentConfig EnvironmentConfig)
        {
            var doesEnvironmentExist = AllEnvironmentsInGinger.Any((existingEnvironmentInGinger) => existingEnvironmentInGinger.Guid.Equals(EnvironmentConfig.Guid));
            if (!doesEnvironmentExist)
            {
                doesEnvironmentExist = AllEnvironmentsInGinger.Any((existingEnvironmentInGinger) => existingEnvironmentInGinger.Name.Equals(EnvironmentConfig.Name));
            }

            if (doesEnvironmentExist)
            {
                throw new Exception($"The Environment {EnvironmentConfig.Name} already exists in Ginger. Please make sure that the Name and GUID is unique");
            }
        }


        // EnvApplication should be unique by name and guid
        public static void ValidateApplicationConfig(ApplicationConfig application)
        {

            bool doesApplicationExistInGinger = WorkSpace.Instance.Solution.ApplicationPlatforms
                .Any((ApplicationPlatform) =>
                {
                    return ApplicationPlatform.Guid.Equals(application.ApplicationPlatformGUID) || ApplicationPlatform.AppName.Equals(application.ApplicationPlatformName);
                });


            if (!doesApplicationExistInGinger)
            {
                throw new Exception("The mention Application Platform does not exist in ginger. Please make sure that the mentioned Application Platform exists in ginger");
            }

        }

        public static void ValidateDatabaseConfig(DatabaseConfig databaseConfig)
        {

            if (databaseConfig == null) 
            {
                return;
            }
            if(databaseConfig.DBType == null ||  string.IsNullOrEmpty(databaseConfig.ConnectionString) || string.IsNullOrEmpty(databaseConfig.Name))
            {
                throw new Exception("Either DBType, ConnectionString or Name is not mentioned of the Database");
            }

        }
    }
}
