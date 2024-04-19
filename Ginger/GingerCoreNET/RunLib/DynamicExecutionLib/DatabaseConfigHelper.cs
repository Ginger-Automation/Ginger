using Ginger.ExecuterService.Contracts;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using GingerCore.Environments;
using System;
using static GingerCore.Environments.Database;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib
{
    public class DatabaseConfigHelper
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
                _ => throw new ArgumentException($"Mentioned DB Type does not exist"),
            };
        }

        public static void ValidateDatabaseConfig(DatabaseConfig databaseConfig)
        {
            if (databaseConfig == null || databaseConfig.DBType == null || string.IsNullOrEmpty(databaseConfig.ConnectionString) || !databaseConfig.KeepConnectionOpen.HasValue || string.IsNullOrEmpty(databaseConfig.Name))
            {
                throw new ArgumentException("Either DBType, ConnectionString or Name is not mentioned of the Database");
            }
        }

        public static Database CreateDatabaseFromConfig(DatabaseConfig databaseConfig)
        {
            ValidateDatabaseConfig(databaseConfig);
            return new()
            {
                ConnectionString = databaseConfig.ConnectionString,
                Name = databaseConfig.Name,
                KeepConnectionOpen = databaseConfig.KeepConnectionOpen ?? false,
                DBType = ConvertDBConfigTypeToDBType(databaseConfig.DBType)
            };
        }

        public static void UpdateDatabaseFromConfig(DatabaseConfig db , ref Database dbFromGinger)
        {
            ValidateDatabaseConfig(db);
            dbFromGinger.Name = db.Name;
            dbFromGinger.ConnectionString = db.ConnectionString;
            dbFromGinger.KeepConnectionOpen = db.KeepConnectionOpen ?? false;
            dbFromGinger.DBType = ConvertDBConfigTypeToDBType(db.DBType);
        }
    }
}
