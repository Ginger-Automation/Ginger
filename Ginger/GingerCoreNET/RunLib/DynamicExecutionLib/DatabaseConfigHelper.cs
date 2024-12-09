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
using Ginger.ExecuterService.Contracts;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using GingerCore;
using GingerCore.Environments;
using System;
using static GingerCore.Environments.Database;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib
{
    public static class DatabaseConfigHelper
    {

        public static eDBConfigType ConverDBTypeToDBConfigType(eDBTypes dBType)
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
                _ => throw new ArgumentException($"Mentioned DB Type does not exist"),
            };
        }


        public static eDBTypes ConvertDBConfigTypeToDBType(eDBConfigType eDBConfigType)
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
                _ => throw new ArgumentException($"DB Type {eDBConfigType} does not exist"),
            };
        }

        public static void ValidateDatabaseConfig(DatabaseConfig databaseConfig)
        {
            if (databaseConfig == null || string.IsNullOrEmpty(databaseConfig.Name))
            {
                throw new ArgumentException("Database Name cannot be null");
            }
        }

        public static Database CreateDatabaseFromConfig(DatabaseConfig databaseConfig)
        {
            ValidateDatabaseConfig(databaseConfig);
            return new()
            {
                ConnectionString = DecryptConnectionString(databaseConfig),
                Name = databaseConfig.Name,
                KeepConnectionOpen = databaseConfig.KeepConnectionOpen ?? false,
                DBType = ConvertDBConfigTypeToDBType(databaseConfig.DBType)
            };
        }

        public static void UpdateDatabaseFromConfig(DatabaseConfig db, ref Database dbFromGinger)
        {
            ValidateDatabaseConfig(db);
            dbFromGinger.Name = db.Name;
            dbFromGinger.ConnectionString = DecryptConnectionString(db);
            dbFromGinger.KeepConnectionOpen = db.KeepConnectionOpen ?? false;
            dbFromGinger.DBType = ConvertDBConfigTypeToDBType(db.DBType);
        }

        public static string DecryptConnectionString(DatabaseConfig db)
        {
            if (db?.ConnectionString == null)
            {
                throw new ArgumentNullException(nameof(db), "Database config or connection string cannot be null");
            }

            if (WorkSpace.Instance?.Solution?.EncryptionKey == null)
            {
                throw new InvalidOperationException("Workspace solution encryption key not available");
            }
            string DbConnectionString = string.Empty;
            if (db.IsConnectionStringEncrypted == true)
            {
                DbConnectionString = EncryptionHandler.DecryptwithKey(db.ConnectionString, WorkSpace.Instance.Solution.EncryptionKey);
            }
            else
            {
                DbConnectionString = db.ConnectionString;
            }
            return DbConnectionString;
        }
    }
}
