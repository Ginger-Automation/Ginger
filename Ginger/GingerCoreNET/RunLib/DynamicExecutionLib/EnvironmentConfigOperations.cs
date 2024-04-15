using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.ExecuterService.Contracts;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using Ginger.Run;
using GingerCore.Environments;
using GingerCore.Variables;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using static GingerCore.Environments.Database;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib
{
    public class EnvironmentConfigOperations
    {
        public static List<EnvironmentConfig> ConvertToEnvironmentRunsetConfig(ProjEnvironment runsetExecutionEnvironment, IList<GingerRunner> GingerRunners)
        {
            List<ProjEnvironment> AllEnvUsedinRunnersAndRunset = [runsetExecutionEnvironment];
            GingerRunners
                .Where((runner) => !runner.ProjEnvironment.Guid.Equals(runsetExecutionEnvironment.Guid))
                .Select((filteredProj) => filteredProj.ProjEnvironment)
                .ForEach(AllEnvUsedinRunnersAndRunset.Add);

            List<EnvironmentConfig> resultConfigEnvironments = [];

            foreach (ProjEnvironment projEnvironment in AllEnvUsedinRunnersAndRunset)
            {
                EnvironmentConfig configEnv = new();
                configEnv.Name = projEnvironment.Name;
                configEnv.Exist = true;
                configEnv.Guid = projEnvironment.Guid;


                if (projEnvironment.Applications != null && projEnvironment.Applications.Count > 0)
                {
                    configEnv.Applications = [];

                    foreach (var application in projEnvironment.Applications)
                    {
                        List<DatabaseConfig> dataBases = [];

                        List<ParameterConfig> parameters = [];


                        application.Dbs?.ForEach((db) =>
                        {
                            dataBases.Add(
                                new()
                                {
                                    ConnectionString = ((Database)db).ConnectionString,
                                    Guid = ((Database)db).Guid,
                                    KeepConnectionOpen = ((Database)db).KeepConnectionOpen,
                                    Name = ((Database)db).Name,
                                    DBType = ConvertDBTypeToDBConfig(((Database)db).DBType)
                                }
                                ) ;

                        });


                        application.Variables?.ForEach((variable) =>
                        {
                            parameters.Add(

                                new()
                                {
                                    Guid = variable.Guid,
                                    Name = variable.Name,
                                    ParameterType = variable.VariableUIType.Replace("Variable", "").Trim(),
                                    Value = variable.Value,
                                }
                                );

                        });

                        configEnv.Applications.Add(new()
                        {
                            Name = application.Name,
                            AppVersion = application.AppVersion,
                            DBs = dataBases,
                            Guid = application.Guid,
                            Parameters = parameters,
                            URL = application.Url,
                            ApplicationPlatformGUID = application.ParentGuid,
                            ApplicationPlatformName = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault((app)=>app.Guid.Equals(application.ParentGuid))?.AppName ?? string.Empty
                        });

                    }
                }
                resultConfigEnvironments.Add(configEnv);
            }
            return resultConfigEnvironments;
        }

        private static eDBConfigType ConvertDBTypeToDBConfig(eDBTypes dBType)
        {
            
            return dBType switch
            {
                eDBTypes.Oracle => eDBConfigType.Oracle,
                eDBTypes.MSAccess => eDBConfigType.MSAccess,
                eDBTypes.MSSQL => eDBConfigType.MSSQL,
                eDBTypes.Cassandra => eDBConfigType.Cassandra,
                eDBTypes.MongoDb => eDBConfigType.MongoDb,
                eDBTypes.CosmosDb => eDBConfigType.CosmosDb,
                eDBTypes.MySQL => eDBConfigType.MySQL,
                eDBTypes.PostgreSQL => eDBConfigType.PostgreSQL,
                eDBTypes.Couchbase => eDBConfigType.Couchbase,
                eDBTypes.DB2 => eDBConfigType.DB2,
                eDBTypes.Hbase => eDBConfigType.Hbase,
                _ => throw new Exception($"Mentioned DB Type does not exist"),
            };
        }
        
        public static void UpdateExistingEnvironmentDetails(IEnumerable<EnvironmentConfig> ExistingEnvironments, ObservableList<ProjEnvironment> AllEnvironmentsInGinger)
        {
            ExistingEnvironments.ForEach((env) =>
            {
                var envFromGinger = DynamicExecutionManager.FindItemByIDAndName(
                        new Tuple<string, Guid?>(nameof(ProjEnvironment.Guid), env.Guid),
                        new Tuple<string, string>(nameof(ProjEnvironment.Name), env.Name),
                        AllEnvironmentsInGinger
                    );

                env.Applications.ForEach((app) =>
                {

                    var appFromGinger = DynamicExecutionManager.FindItemByIDAndName(
                        new Tuple<string, Guid?>(nameof(EnvApplication.Guid), app.Guid),
                        new Tuple<string, string>(nameof(EnvApplication.Name), app.Name),
                        envFromGinger.Applications
                    );

                    appFromGinger.AppVersion = app.AppVersion;
                    appFromGinger.Url = app.URL;

                    var dbsFromGinger = new ObservableList<Database>(appFromGinger.Dbs.Select((d) => (Database)d));
                    app.DBs.ForEach((db) =>
                    {
                        var dbFromGinger = DynamicExecutionManager.FindItemByIDAndName(
                         new Tuple<string, Guid?>(nameof(Database.Guid), db.Guid),
                         new Tuple<string, string>(nameof(Database.Name), db.Name),
                          dbsFromGinger
                         );

                        dbFromGinger.Name = db.Name;
                        dbFromGinger.ConnectionString = db.ConnectionString;
                        dbFromGinger.KeepConnectionOpen = db.KeepConnectionOpen;
                    });

                    app.Parameters.ForEach((param) =>
                    {
                        var parameterFromGinger = DynamicExecutionManager.FindItemByIDAndName(
                                new Tuple<string, Guid?>(nameof(VariableBase.Guid), param.Guid),
                                new Tuple<string, string>(nameof(VariableBase.Name), param.Name),
                                appFromGinger.Variables
                             );

                        parameterFromGinger.Name = param.Name;
                        parameterFromGinger.Value = param.Value;
                    });

                });

            });

        }

        public static void AddNewEnvironmentDetails(IEnumerable<EnvironmentConfig> NewEnvironmentsInConfig, ObservableList<ProjEnvironment> AllEnvironmentsInGinger)
        {
            NewEnvironmentsInConfig?.ForEach((Environment) =>
            {
                ValidateEnvironmentConfigDetails.ValidateEnvironmentConfig(AllEnvironmentsInGinger, Environment);
                var NewEnvironment = new ProjEnvironment()
                {
                    Name = Environment.Name,
                };


                Environment.Applications?.ForEach((Application) =>
                {
                    ValidateEnvironmentConfigDetails.ValidateApplicationConfig(Application);

                    var EnvApplication = CreateEnvironmentConfigDetails.CreateApplicationConfig(Application);
                
                    NewEnvironment.Applications.Add(EnvApplication);

                    Application.Parameters?.ForEach((Parameter) =>
                    {
                        var ParameterToBeAdded = CreateEnvironmentConfigDetails.CreateParameterConfig(Parameter);
                        EnvApplication.Variables.Add(ParameterToBeAdded);
                    });

                    Application.DBs?.ForEach((DatabaseConfig) =>
                    {
                        ValidateEnvironmentConfigDetails.ValidateDatabaseConfig(DatabaseConfig);
                        
                        EnvApplication.Dbs.Add(CreateEnvironmentConfigDetails.CreateDatabaseConfig(DatabaseConfig));
                    });
                });

                AllEnvironmentsInGinger.Add(NewEnvironment);
            });

        }
    }

}
