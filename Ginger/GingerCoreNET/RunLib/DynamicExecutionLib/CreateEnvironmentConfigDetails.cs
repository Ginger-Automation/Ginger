using amdocs.ginger.GingerCoreNET;
using Ginger.ExecuterService.Contracts;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using GingerCore.Environments;
using GingerCore.Variables;
using System;
using System.Linq;
using static GingerCore.Environments.Database;
namespace Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib
{
    public class CreateEnvironmentConfigDetails
    {
        public static VariableBase CreateParameterConfig(ParameterConfig Parameter)
        {
            return Parameter.ParameterType.Trim().ToLower() switch
            {
                "string" => new VariableString()
                {
                    Value = Parameter.Value,
                    Name = Parameter.Name,
                },
                "dynamic" => new VariableDynamic()
                {
                    Value = Parameter.Value,
                    Name = Parameter.Name
                },
                "number" => new VariableNumber()
                {
                    Value = Parameter.Value,
                    Name = Parameter.Name
                },
                "password string" or "password" => new VariablePasswordString()
                {
                    Value = Parameter.Value,
                    Name = Parameter.Name
                },
                _ => throw new Exception("The Parameter should either be of Type String, Password String, Dynamic , Number"),
            };

        }
        public static Database CreateDatabaseConfig(DatabaseConfig databaseConfig)
        {
            static eDBTypes GetDbType(eDBConfigType eDBConfigType)
            {
                return eDBConfigType switch
                {
                    eDBConfigType.Oracle => eDBTypes.Oracle,
                    eDBConfigType.MSAccess => eDBTypes.MSAccess,
                    eDBConfigType.MSSQL => eDBTypes.MSSQL,
                    eDBConfigType.Cassandra => eDBTypes.Cassandra,
                    eDBConfigType.MongoDb => eDBTypes.MongoDb,
                    eDBConfigType.CosmosDb => eDBTypes.CosmosDb,
                    eDBConfigType.MySQL => eDBTypes.MySQL,
                    eDBConfigType.PostgreSQL => eDBTypes.PostgreSQL,
                    eDBConfigType.Couchbase => eDBTypes.Couchbase,
                    eDBConfigType.DB2 => eDBTypes.DB2,
                    eDBConfigType.Hbase => eDBTypes.Hbase,
                    _ => throw new Exception($"Mentioned DB Type does not exist"),
                };
            }

            return new()
            {
                ConnectionString = databaseConfig.ConnectionString,
                Name = databaseConfig.Name,
                KeepConnectionOpen = databaseConfig.KeepConnectionOpen,
                DBType = GetDbType(databaseConfig.DBType)
            };

        }

        public static EnvApplication CreateApplicationConfig(ApplicationConfig application)
        {
            var Platform = WorkSpace.Instance.Solution.ApplicationPlatforms
                .First((ApplicationPlatform) =>
                {
                    return ApplicationPlatform.Guid.Equals(application.ApplicationPlatformGUID) || ApplicationPlatform.AppName.Equals(application.ApplicationPlatformName);
                }).Platform;

            EnvApplication envApplication = new()
            {
                Name = application.Name,
                AppVersion = application.AppVersion,
                Url = application.URL,
                Platform = Platform,
                ParentGuid = application.ApplicationPlatformGUID
            };

            return envApplication;
        }
    }
}
