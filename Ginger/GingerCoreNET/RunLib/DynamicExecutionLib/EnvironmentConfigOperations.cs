#region License
/*
Copyright © 2014-2026 European Support Limited

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

using Amdocs.Ginger.Common;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using Ginger.Run;
using GingerCore.Environments;
using GingerCore.Variables;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib
{
    public static class EnvironmentConfigOperations
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
                EnvironmentConfig configEnv = new()
                {
                    Name = projEnvironment.Name,
                    Exist = true,
                    Guid = projEnvironment.Guid
                };


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
                                    DBType = DatabaseConfigHelper.ConverDBTypeToDBConfigType(((Database)db).DBType)
                                }
                                );

                        });


                        application.Variables?.ForEach((variable) =>
                        {
                            parameters.Add(

                                new()
                                {
                                    Guid = variable.Guid,
                                    Name = variable.Name,
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
                        });

                    }
                }
                resultConfigEnvironments.Add(configEnv);
            }
            return resultConfigEnvironments;
        }
        public static void UpdateExistingEnvironmentDetails(IEnumerable<EnvironmentConfig> ExistingEnvironments, ObservableList<ProjEnvironment> AllEnvironmentsInGinger)
        {

            ExistingEnvironments?.ForEach((env) =>
            {
                var envFromGinger = DynamicExecutionManager.FindItemByIDAndName(
                        new Tuple<string, Guid?>(nameof(ProjEnvironment.Guid), env.Guid),
                        new Tuple<string, string>(nameof(ProjEnvironment.Name), env.Name),
                        AllEnvironmentsInGinger
                    );

                CheckIfNameIsUnique<ApplicationConfig>(env.Applications);
                env.Applications.ForEach((app) =>
                {

                    var appFromGinger = DynamicExecutionManager.FindItemByIDAndName(
                        new Tuple<string, Guid?>(nameof(EnvApplication.Guid), app.Guid),
                        new Tuple<string, string>(nameof(EnvApplication.Name), app.Name),
                        envFromGinger.Applications,
                        false
                    );

                    if (appFromGinger == null)
                    {
                        appFromGinger = ApplicationConfigHelper.CreateEnvApplicationFromConfig(app);
                        envFromGinger.Applications.Add(appFromGinger);
                    }
                    else
                    {
                        ApplicationConfigHelper.UpdateEnvApplicationFromConfig(ref appFromGinger, app);
                    }


                    var dbsFromGinger = new ObservableList<Database>(appFromGinger.Dbs.Select((d) => (Database)d));
                    CheckIfNameIsUnique<DatabaseConfig>(app.DBs);
                    CheckIfNameIsUnique<ParameterConfig>(app.Parameters);

                    app.DBs.ForEach((db) =>
                    {
                        var dbFromGinger = DynamicExecutionManager.FindItemByIDAndName(
                         new Tuple<string, Guid?>(nameof(Database.Guid), db.Guid),
                         new Tuple<string, string>(nameof(Database.Name), db.Name),
                          dbsFromGinger,
                          false
                         );

                        if (dbFromGinger == null)
                        {
                            dbFromGinger = DatabaseConfigHelper.CreateDatabaseFromConfig(db);
                            appFromGinger.Dbs.Add(dbFromGinger);
                        }
                        else
                        {
                            DatabaseConfigHelper.UpdateDatabaseFromConfig(db, ref dbFromGinger);
                        }
                    });

                    app.Parameters.ForEach((param) =>
                    {
                        var parameterFromGinger = DynamicExecutionManager.FindItemByIDAndName(
                                new Tuple<string, Guid?>(nameof(VariableBase.Guid), param.Guid),
                                new Tuple<string, string>(nameof(VariableBase.Name), param.Name),
                                appFromGinger.Variables,
                                false
                             );

                        if (parameterFromGinger == null)
                        {
                            parameterFromGinger = ParameterConfigHelper.CreateParameterFromConfig(param);

                            appFromGinger.Variables.Add(parameterFromGinger);
                        }
                        else
                        {
                            ParameterConfigHelper.UpdateParameterFromConfig(param, ref parameterFromGinger);
                        }
                    });

                });

            });

        }

        public static void AddNewEnvironmentDetails(IEnumerable<EnvironmentConfig> NewEnvironmentsInConfig, ObservableList<ProjEnvironment> AllEnvironmentsInGinger)
        {
            NewEnvironmentsInConfig?.ForEach((Environment) =>
             {
                 ValidateEnvironmentConfig(AllEnvironmentsInGinger, Environment);
                 var NewEnvironment = new ProjEnvironment()
                 {
                     Name = Environment.Name,
                 };


                 Environment.Applications?.ForEach((Application) =>
                 {
                     var EnvApplication = ApplicationConfigHelper.CreateEnvApplicationFromConfig(Application);

                     NewEnvironment.Applications.Add(EnvApplication);

                     Application.Parameters?.ForEach((Parameter) =>
                     {
                         var ParameterToBeAdded = ParameterConfigHelper.CreateParameterFromConfig(Parameter);
                         EnvApplication.Variables.Add(ParameterToBeAdded);
                     });

                     Application.DBs?.ForEach((DatabaseConfig) =>
                     {
                         EnvApplication.Dbs.Add(DatabaseConfigHelper.CreateDatabaseFromConfig(DatabaseConfig));
                     });
                 });

                 AllEnvironmentsInGinger.Add(NewEnvironment);
             });
        }


        public static void ValidateEnvironmentConfig(ObservableList<ProjEnvironment> AllEnvironmentsInGinger, EnvironmentConfig EnvironmentConfig)
        {
            var doesEnvironmentExist = AllEnvironmentsInGinger.Any((existingEnvironmentInGinger) => existingEnvironmentInGinger.Guid.Equals(EnvironmentConfig.Guid));
            if (!doesEnvironmentExist)
            {
                doesEnvironmentExist = AllEnvironmentsInGinger.Any((existingEnvironmentInGinger) => existingEnvironmentInGinger.Name.Equals(EnvironmentConfig.Name));
            }

            if (doesEnvironmentExist)
            {
                throw new InvalidOperationException($"The Environment {EnvironmentConfig.Name} already exists in Ginger. Please make sure that the Name and GUID is unique");
            }
        }


        public static void CheckIfNameIsUnique<T>(IEnumerable<T> List)
        {
            var distinctCount = List.Select((env) => typeof(T).GetProperty("Name").GetValue(env)).Distinct().Count();
            var actualCount = List.Count();
            if (distinctCount != actualCount)
            {
                throw new InvalidOperationException($"{typeof(T).Name.Replace("Config", "")} Name should be distinct in the list");
            }
        }
    }

}
