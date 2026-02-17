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
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using GingerCore.NoSqlBase;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static GingerCore.Environments.Database;

namespace GingerCore.Environments
{
    public class DatabaseOperations : IDatabaseOperations
    {
        public Database Database;

        public DatabaseOperations(Database database)
        {
            this.Database = database;
            this.Database.DatabaseOperations = this;
        }
        private DbConnection oConn = null;
        private DbTransaction tran = null;

        ValueExpression mVE = null;
        ValueExpression VE
        {
            get
            {
                if (mVE == null)
                {
                    if (Database.ProjEnvironment == null)
                    {
                        Database.ProjEnvironment = new Environments.ProjEnvironment();
                    }

                    if (Database.BusinessFlow == null)
                    {
                        Database.BusinessFlow = new GingerCore.BusinessFlow();
                    }

                    mVE = new ValueExpression(Database.ProjEnvironment, Database.BusinessFlow, Database.DSList);
                }

                return mVE;
            }
            set
            {
                mVE = value;
            }
        }

        public bool IsPassValueExp()
        {
            return ValueExpression.IsThisAValueExpression(this.Database.Pass);
        }
        public string ConnectionStringCalculated
        {
            get
            {
                return GetCalculatedWithDecryptTrue(Database.ConnectionString);
            }
        }

        public string TNSCalculated
        {
            get
            {
                return GetCalculatedWithDecryptTrue(Database.TNS);
            }
        }
        public string UserCalculated
        {
            get
            {
                return GetCalculatedWithDecryptTrue(Database.User);
            }
        }
        public string PassCalculated
        {
            get
            {
                return GetCalculatedWithDecryptTrue(Database.Pass);
            }
        }

        public string NameBeforeEdit;
        public string GetCalculatedWithDecryptTrue(string value)
        {
            VE.Value = value;
            mVE.DecryptFlag = true;
            string valueCalculated = mVE.ValueCalculated;
            mVE.DecryptFlag = false;
            return valueCalculated;
        }
        public bool CheckUserCredentialsInTNS()
        {
            if (!string.IsNullOrEmpty(TNSCalculated) && TNSCalculated.ToLower().Contains("data source=") && TNSCalculated.ToLower().Contains("password=") && TNSCalculated.ToLower().Contains("user id="))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void SplitUserIdPassFromTNS()
        {
            try
            {
                SqlConnectionStringBuilder scSB = new SqlConnectionStringBuilder(); ;
                scSB.ConnectionString = Database.TNS;
                Database.TNS = scSB.DataSource;
                Database.User = scSB.UserID;
                Database.Pass = scSB.Password;
                Database.ConnectionString = scSB.ConnectionString;
            }
            catch (Exception ex) when (ex is ArgumentException or FormatException or InvalidOperationException)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "TNS is not a full SQL connection string, leaving values as-is", ex);
            }
        }

        public string GetConnectionString()
        {
            string connStr;

            if (String.IsNullOrEmpty(ConnectionStringCalculated))
            {
                connStr = CreateConnectionString();
            }
            else
            {
                connStr = ConnectionStringCalculated;
            }

            connStr = connStr.Replace("{USER}", UserCalculated);

            return ReplacePasswordInConnectionString(connStr);
        }

        private string ReplacePasswordInConnectionString(string connStr)
        {
            String deCryptValue = EncryptionHandler.DecryptwithKey(PassCalculated);

            return connStr.Replace("{PASS}", string.IsNullOrEmpty(deCryptValue) ? PassCalculated : deCryptValue);
        }

        public string CreateConnectionString()
        {
            try
            {
                switch (Database.DBType)
                {
                    case eDBTypes.MSAccess:
                        {
                            //var ole = new OleDbConnectionStringBuilder();
                            //ole.Provider = TNSCalculated.Contains(".accdb") 
                            //    ? "Microsoft.ACE.OLEDB.12.0":
                            //    "Microsoft.Jet.OLEDB.4.0";
                            //ole.DataSource = TNSCalculated;
                            //Database.ConnectionString = ole.ConnectionString + ";User Id={USER};Password={PASS};";

                            string strProvider;
                            strProvider = TNSCalculated.Contains(".accdb")
                                ? "Provider=Microsoft.ACE.OLEDB.12.0;"
                                : "Provider=Microsoft.Jet.OLEDB.4.0;";

                            Database.ConnectionString = strProvider + Database.ConnectionString;
                            break;
                        }

                    case eDBTypes.DB2:

                        Database.ConnectionString = $"Server={TNSCalculated};Database={Database.Name};UID={{USER}};PWD={{PASS}}";
                        //Database.ConnectionString = $"Server={TNSCalculated};Database={Database.Name};UID={{USER}};PWD={{PASS}}";
                        break;

                    case eDBTypes.PostgreSQL:
                        {
                            string postgreSQLHost = TNSCalculated;
                            int? port = null;
                            if (TNSCalculated.Contains(':', StringComparison.Ordinal))
                            {
                                var parts = TNSCalculated.Split(':', 2);
                                postgreSQLHost = parts[0];
                                if (int.TryParse(parts[1], out int p)) port = p;
                            }
                            ValidateHostPort(postgreSQLHost, port);

                            var pg = new NpgsqlConnectionStringBuilder
                            {
                                Host = postgreSQLHost,
                                Database = Database.Name ?? string.Empty,
                                Username = "{USER}",
                                Password = "{PASS}"
                            };
                            if (port.HasValue) pg.Port = port.Value;
                            Database.ConnectionString = pg.ConnectionString;
                            break;
                        }

                    case eDBTypes.MySQL:
                        {
                            string mySQLHost = TNSCalculated;
                            uint? port = null;
                            if (TNSCalculated.Contains(':', StringComparison.Ordinal))
                            {
                                var parts = TNSCalculated.Split(':', 2);
                                mySQLHost = parts[0];
                                if (uint.TryParse(parts[1], out uint p)) port = p;
                            }

                            ValidateHostPort(mySQLHost, port.HasValue ? (int?)port.Value : null);

                            var my = new MySqlConnectionStringBuilder
                            {
                                Server = mySQLHost,
                                Database = Database.Name ?? string.Empty,
                                UserID = "{USER}",
                                Password = "{PASS}"
                            };
                            if (port.HasValue) my.Port = port.Value;
                            Database.ConnectionString = my.ConnectionString;
                            break;
                        }

                    case eDBTypes.CosmosDb:
                        Database.ConnectionString = $"AccountEndpoint={Database.User};AccountKey={Database.Pass}";
                        break;

                    case eDBTypes.Hbase:
                        var host = TNSCalculated.Split(':');
                        if (host.Length == 2)
                        {
                            Database.ConnectionString = "Server=" + host[0] + ";Port=" + host[1] + ";User Id={USER}; Password={PASS};Database=" + Database.Name + ";";
                        }
                        break;
                    //{
                    //    var hostParts = TNSCalculated.Split(':');
                    //    if (hostParts.Length == 2)
                    //    {
                    //        ValidateHostPort(hostParts[0], int.TryParse(hostParts[1], out int p) ? (int?)p : null);
                    //        Database.ConnectionString = $"Server={hostParts[0]};Port={hostParts[1]};User Id={UserCalculated}; Password={{PASS}};Database={Database.Name};";
                    //    }
                    //    break;
                    //}

                    case eDBTypes.MSSQL:
                        var builder = new SqlConnectionStringBuilder
                        {
                            DataSource = TNSCalculated
                        };

                        if (!string.IsNullOrEmpty(Database.Name)) builder.InitialCatalog = Database.Name;
                        builder.IntegratedSecurity = false;
                        builder.UserID = UserCalculated;
                        builder.Password = EncryptionHandler.DecryptwithKey(PassCalculated);
                        Database.ConnectionString = builder.ConnectionString;
                        break;

                    default:
                        throw new NotImplementedException("Unhandled database type: " + Database.DBType);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to create connection string for DB type: {Database.DBType}, falling back to default", ex);
                Database.ConnectionString = "Data Source=" + Database.TNS + ";User Id={USER};Password={PASS};";
            }

            return ConnectionStringCalculated;
        }

        private static void ValidateHostPort(string hostPart, int? port)
        {
            if (string.IsNullOrWhiteSpace(hostPart))
                throw new ArgumentException("Invalid host in TNS.");

            if (port.HasValue && (port < 1 || port > 65535))
                throw new ArgumentException("Invalid port in TNS.");
        }

        private DateTime LastConnectionUsedTime;


        public bool MakeSureConnectionIsOpen()
        {
            Boolean isCoonected = true;
            if ((oConn == null) || (oConn.State != ConnectionState.Open))
            {
                isCoonected = Connect();
            }

            //make sure that the connection was not refused by the server               
            TimeSpan timeDiff = DateTime.Now - LastConnectionUsedTime;
            if (timeDiff.TotalMinutes > 5)
            {
                isCoonected = Connect();
            }
            else
            {
                LastConnectionUsedTime = DateTime.Now;
            }
            return isCoonected;
        }

        public static string GetMissingDLLErrorDescription()
        {
            string message = "Connect to the DB failed." + Environment.NewLine + "The file Oracle.ManagedDataAccess.dll is missing," + Environment.NewLine + "Please download the file, place it under the below folder, restart Ginger and retry." + Environment.NewLine + AppDomain.CurrentDomain.BaseDirectory + Environment.NewLine + "Links to download the file:" + Environment.NewLine + "https://docs.oracle.com/database/121/ODPNT/installODPmd.htm#ODPNT8149" + Environment.NewLine + "http://www.oracle.com/technetwork/topics/dotnet/downloads/odacdeploy-4242173.html";
            return message;
        }

        public Boolean Connect(bool displayErrorPopup = false)
        {
            try
            {
                Reporter.ToStatus(eStatusMsgKey.TestingDatabase, null, $"Testing Database: {Database.Name}");

                switch (Database.DBType)
                {
                    case eDBTypes.MSSQL:
                        // If a full connection string is provided use it (after resolving placeholders and decrypting password),
                        // otherwise build from TNS/User/Pass as before.
                        string connectionString = string.Empty;
                        var mailBuilder = GetMSSQLConnectionStringBuilder();

                        oConn = new SqlConnection(mailBuilder.ConnectionString);

                        oConn.Open();
                        break;

                    case eDBTypes.Oracle:
                        //TODO: Oracle connection is deprecated use another method - Switched to ODP.NET                        
                        if (Database.IsOracleVersionLow)
                        {
                            var oleDbconnectionStringBuilder = new OleDbConnectionStringBuilder()
                            {
                                Provider = "msdaora"
                            };

                            String deCryptValue = EncryptionHandler.DecryptwithKey(PassCalculated);

                            oleDbconnectionStringBuilder.ConnectionString = ConnectionStringCalculated.Replace("{USER}", UserCalculated).Replace("{PASS}", string.IsNullOrEmpty(deCryptValue) ? PassCalculated : deCryptValue);

                            oConn = OleDbFactory.Instance.CreateConnection();
                            oConn.ConnectionString = oleDbconnectionStringBuilder.ConnectionString;
                            oConn.Open();
                        }
                        else
                        {
                            oConn = WorkSpace.Instance.TargetFrameworkHelper.GetOracleConnection(GetConnectionString());
                            oConn.Open();
                        }
                        break;

                    case eDBTypes.MSAccess:
                        var oleDbConnStringBuilder = new OleDbConnectionStringBuilder(GetConnectionString());
                        oConn = new OleDbConnection
                        {
                            ConnectionString = oleDbConnStringBuilder.ConnectionString
                        };
                        oConn.Open();
                        break;

                    case eDBTypes.DB2:
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            String DB2Cpath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\IBM\DB2\GLOBAL_PROFILE", "DB2PATH", "DNE");

                            if (System.IO.Directory.Exists(DB2Cpath))
                            {
                                var DLL = Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + @"DLLs\IBM.Data.DB2.dll");

                                var class1Type = DLL.GetType("IBM.Data.DB2.DB2Connection");

                                //Now you can use reflection or dynamic to call the method. I will show you the dynamic way
                                object[] param = [GetConnectionString()];
                                dynamic c = Activator.CreateInstance(class1Type, param);
                                oConn = (DbConnection)c;
                                oConn.Open();
                            }
                            else
                            {
                                throw new DllNotFoundException("DB2 Connect or IBM DB2 Drivers not installed.");
                            }
                        }
                        else
                        {
                            throw new PlatformNotSupportedException("DB2 Connections are provided only on Windows Operationg System");
                        }
                        break;

                    case eDBTypes.PostgreSQL:
                        string postgreSQLHost = TNSCalculated;
                        int? port = null;
                        if (TNSCalculated.Contains(':', StringComparison.Ordinal))
                        {
                            var parts = TNSCalculated.Split(':', 2);
                            postgreSQLHost = parts[0];
                            if (int.TryParse(parts[1], out int p)) port = p;
                        }
                        ValidateHostPort(postgreSQLHost, port);

                        var pg = new NpgsqlConnectionStringBuilder
                        {
                            Host = postgreSQLHost,
                            Database = Database.Name ?? string.Empty,
                            Username = UserCalculated,
                            Password = EncryptionHandler.DecryptwithKey(PassCalculated)
                        };
                        if (port.HasValue) pg.Port = port.Value;
                        Database.ConnectionString = pg.ConnectionString;

                        oConn = new NpgsqlConnection(pg.ConnectionString);
                        oConn.Open();
                        break;

                    case eDBTypes.Cassandra:
                        GingerCassandra CassandraDriver = new GingerCassandra(Database);
                        bool isConnection;
                        isConnection = CassandraDriver.Connect();
                        if (isConnection == true)
                        {
                            LastConnectionUsedTime = DateTime.Now;
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    case eDBTypes.Couchbase:
                        GingerCouchbase CouchbaseDriver = new GingerCouchbase(Database);
                        bool isConnectionCB;
                        isConnectionCB = CouchbaseDriver.Connect();
                        if (isConnectionCB == true)
                        {
                            LastConnectionUsedTime = DateTime.Now;
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    case eDBTypes.MySQL:
                        string mySQLHost = TNSCalculated;
                        uint? port1 = null;
                        if (TNSCalculated.Contains(':', StringComparison.Ordinal))
                        {
                            var parts = TNSCalculated.Split(':', 2);
                            mySQLHost = parts[0];
                            if (uint.TryParse(parts[1], out uint p)) port1 = p;
                        }

                        ValidateHostPort(mySQLHost, port1.HasValue ? (int?)port1.Value : null);

                        var my = new MySqlConnectionStringBuilder
                        {
                            Server = mySQLHost,
                            Database = Database.Name ?? string.Empty,
                            UserID = UserCalculated,
                            Password = EncryptionHandler.DecryptwithKey(PassCalculated)
                        };
                        if (port1.HasValue) my.Port = port1.Value;
                        Database.ConnectionString = my.ConnectionString;

                        oConn = new MySqlConnection
                        {
                            ConnectionString = my.ConnectionString
                        };
                        oConn.Open();
                        break;

                    case eDBTypes.MongoDb:
                        GingerMongoDb MongoDriver = new GingerMongoDb(Database);

                        if (MongoDriver.Connect())
                        {
                            LastConnectionUsedTime = DateTime.Now;
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    case eDBTypes.CosmosDb:
                        Database.ConnectionString = GetConnectionString();
                        GingerCosmos objGingerCosmos = new GingerCosmos
                        {
                            Db = Database
                        };

                        if (objGingerCosmos.Connect())
                        {
                            LastConnectionUsedTime = DateTime.Now;
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    case eDBTypes.Hbase:
                        Database.ConnectionString = GetConnectionString();

                        GingerHbase ghbase = new GingerHbase(Database.TNS, Database.User, Database.Pass)
                        {
                            Db = Database
                        };

                        if (ghbase.Connect())
                        {
                            LastConnectionUsedTime = DateTime.Now;
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    default:
                        throw new NotImplementedException("Unhandled database type: " + Database.DBType);
                }

                if ((oConn != null) && (oConn.State == ConnectionState.Open))
                {
                    LastConnectionUsedTime = DateTime.Now;
                    return true;
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "DB connection failed, DB type: " + Database.DBType + "; Connection String =" + HidePasswordFromString(GetConnectionString()), e);
                throw;
            }
            finally
            {
                Reporter.HideStatusMessage();
            }

            return false;
        }

        private SqlConnectionStringBuilder GetMSSQLConnectionStringBuilder()
        {
            //if (string.IsNullOrEmpty(Database.ConnectionString))
            {
                var builder = new SqlConnectionStringBuilder();
                builder.DataSource = TNSCalculated;
                if (!string.IsNullOrEmpty(Database.Name)) builder.InitialCatalog = Database.Name;
                builder.IntegratedSecurity = false;
                builder.UserID = UserCalculated;
                builder.Password = EncryptionHandler.DecryptwithKey(PassCalculated);
                return builder;
            }
            //else
            //{
            //    // GetConnectionString will replace {USER} and {PASS} placeholders and decrypt the password
            //    string connStr = ConnectionStringCalculated;
            //    connStr = connStr.Replace("{USER}", UserCalculated);
            //    connStr = ReplacePasswordInConnectionString(connStr);

            //    // Parse and normalise the connection string using SqlConnectionStringBuilder
            //    var parsedBuilder = new SqlConnectionStringBuilder(connStr);
            //    return parsedBuilder;
            //}
        }

        public static string HidePasswordFromString(string dataString)
        {
            string passwordValue = dataString.Replace(" ", "");//remove spaces
            string passwordString = string.Empty;
            //Matching string
            if (dataString.ToLower().Contains("pwd="))
            {
                passwordString = "pwd=";
            }
            else if (dataString.ToLower().Contains("password="))
            {
                passwordString = "password=";
            }
            else
            {
                //returning origional as it does not conatain matching string
                return dataString;
            }
            //get the password value based on start and end index
            passwordValue = passwordValue[passwordValue.ToLower().IndexOf(passwordString)..];
            int startIndex = passwordValue.ToLower().IndexOf(passwordString) + passwordString.Length;
            int endIndex = -1;
            if (passwordValue.Contains(";"))
            {
                endIndex = passwordValue.ToLower().IndexOf(";");
            }
            if (endIndex == -1)
            {
                passwordValue = passwordValue[startIndex..];
            }
            else
            {
                passwordValue = passwordValue[startIndex..endIndex];
            }

            if (!string.IsNullOrEmpty(passwordValue))
            {
                dataString = dataString.Replace(passwordValue, "*****");
            }
            return dataString;
        }

        public void CloseConnection()
        {
            try
            {
                if (oConn != null)
                {
                    oConn.Close();
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to close DB Connection", e);
                throw;
            }
            finally
            {
                oConn?.Dispose();
            }
        }

        public async Task<List<string>> GetTablesListAsync(string Keyspace = null)
        {
            List<string> databaseTableNames = [""];
            if (MakeSureConnectionIsOpen())
            {
                try
                {
                    DataTable table = null;
                    if (oConn == null || oConn.State == ConnectionState.Closed)
                    {
                        Connect();
                    }
                    switch (Database.DBType)
                    {
                        case eDBTypes.Cassandra:
                            NoSqlBase.NoSqlBase NoSqlDriver = null;
                            NoSqlDriver = new GingerCassandra(Database);
                            databaseTableNames = NoSqlDriver.GetTableList(Keyspace);
                            break;

                        case eDBTypes.Couchbase:
                            NoSqlDriver = new GingerCouchbase(Database);
                            databaseTableNames = NoSqlDriver.GetTableList(Keyspace);
                            break;

                        case eDBTypes.MongoDb:
                            NoSqlDriver = new GingerMongoDb(Database);
                            databaseTableNames = NoSqlDriver.GetTableList(Keyspace);
                            break;

                        case eDBTypes.CosmosDb:
                            GingerCosmos objGingerCosmos = new GingerCosmos();
                            Database.ConnectionString = GetConnectionString();
                            objGingerCosmos.Db = Database;
                            databaseTableNames = objGingerCosmos.GetTableList(Keyspace);
                            break;
                        case eDBTypes.Hbase:
                            GingerHbase ghbase = CreateHbaseClient();
                            databaseTableNames = ghbase.GetTableList(Keyspace);
                            break;

                        case eDBTypes.Oracle:
                            string[] restr = [GetConnectedUsername()];
                            table = await oConn.GetSchemaAsync("Tables", restr);
                            foreach (DataRow row in table.Rows)
                            {
                                databaseTableNames.Add((string)row[1]);
                            }
                            break;

                        case eDBTypes.MSSQL:
                        case eDBTypes.MSAccess:
                        case eDBTypes.MySQL:
                        case eDBTypes.DB2:
                        case eDBTypes.PostgreSQL:
                            table = await oConn.GetSchemaAsync("Tables");
                            foreach (DataRow row in table.Rows)
                            {
                                databaseTableNames.Add((string)row[2]);
                            }
                            break;

                        default:
                            throw new NotImplementedException("Unhandled database type: " + Database.DBType);
                    }
                }
                catch (Exception e)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to get table list for DB:" + Database.DBType, e);
                    throw;
                }
            }
            return databaseTableNames;
        }

        /// <summary>
        /// This function is only for Oracle DB, Gets the username of the connected Oracle database.
        /// </summary>
        /// <returns>The connected database username.</returns>
        public string GetConnectedUsername()
        {
            using (DbCommand command = oConn.CreateCommand())
            {
                command.CommandText = "SELECT USER FROM DUAL";
                return command.ExecuteScalar().ToString();
            }
        }
        public List<string> databaseColumnNames;

        public async Task<List<string>> GetTablesColumns(string table)
        {
            DbDataReader reader = null;
            databaseColumnNames = [""];
            if ((oConn == null || string.IsNullOrEmpty(table)) && (Database.DBType != Database.eDBTypes.Cassandra) && (Database.DBType != Database.eDBTypes.MongoDb)
                && (Database.DBType != Database.eDBTypes.CosmosDb) && (Database.DBType != Database.eDBTypes.Hbase))
            {
                return databaseColumnNames;
            }
            if (Database.DBType == Database.eDBTypes.Cassandra)
            {

                NoSqlBase.NoSqlBase NoSqlDriver = null;
                NoSqlDriver = new GingerCassandra(Database);
                databaseColumnNames = await NoSqlDriver.GetColumnList(table);
            }
            else if (Database.DBType == Database.eDBTypes.Couchbase)
            {

                NoSqlBase.NoSqlBase NoSqlDriver = null;
                NoSqlDriver = new GingerCouchbase(Database);
                databaseColumnNames = await NoSqlDriver.GetColumnList(table);

            }
            else if (Database.DBType == Database.eDBTypes.MongoDb)
            {

                NoSqlBase.NoSqlBase NoSqlDriver = null;
                NoSqlDriver = new GingerMongoDb(Database);
                databaseColumnNames = await NoSqlDriver.GetColumnList(table);
            }
            else if (Database.DBType == Database.eDBTypes.CosmosDb)
            {
                NoSqlBase.NoSqlBase NoSqlDriver = null;
                NoSqlDriver = new GingerCosmos();
                Database.ConnectionString = GetConnectionString();
                NoSqlDriver.Db = Database;
                databaseColumnNames = await NoSqlDriver.GetColumnList(table);
            }
            else if (Database.DBType == Database.eDBTypes.Hbase)
            {
                NoSqlBase.NoSqlBase NoSqlDriver = CreateHbaseClient();
                databaseColumnNames = await NoSqlDriver.GetColumnList(table);
            }
            else
            {
                try
                {
                    DbCommand command = oConn.CreateCommand();
                    // Do select with zero records
                    command.CommandText = Database.DBType switch
                    {
                        eDBTypes.PostgreSQL => "select * from public.\"" + table + "\" where 1 = 0",
                        _ => "select * from " + table + " where 1 = 0",
                    };
                    command.CommandType = CommandType.Text;

                    reader = command.ExecuteReader();
                    // Get the schema and read the cols
                    DataTable schemaTable = reader.GetSchemaTable();
                    foreach (DataRow row in schemaTable.Rows)
                    {
                        databaseColumnNames.Add((string)row[0]);
                    }
                }
                catch (Exception e)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "", e);
                    //Reporter.ToUser(eUserMsgKey.DbTableError, "table columns", e.Message);
                    throw;
                }
                finally
                {
                    reader?.Close();

                }
            }
            return databaseColumnNames;
        }
        private GingerHbase CreateHbaseClient()
        {
            var ghbase = new GingerHbase(Database.TNS, Database.User, Database.Pass)
            {
                Db = Database
            };
            ghbase.Connect();
            return ghbase;
        }
        public string fUpdateDB(string updateCmd, bool commit)
        {
            string result = "";
            if (oConn == null)
            {
                Connect();
            }
            if (MakeSureConnectionIsOpen())
            {
                using (DbCommand command = oConn.CreateCommand())
                {
                    try
                    {
                        if (commit)
                        {
                            tran = oConn.BeginTransaction();
                            // to Command object for a pending local transaction
                            command.Connection = oConn;
                            command.Transaction = tran;
                        }
                        command.CommandText = updateCmd;
                        command.CommandType = CommandType.Text;

                        result = command.ExecuteNonQuery().ToString();
                        if (commit)
                        {
                            tran.Commit();
                        }
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Reporter.ToLog(eLogLevel.ERROR, "Commit failed for:" + updateCmd, e);
                        throw;
                    }
                }
            }
            return result;
        }

        public string fTableColWhere(string Table, string Column, string Where)
        {
            String rc = null;
            if (!Database.DBType.Equals(eDBTypes.CosmosDb))
            {
                string sql = "SELECT {0} FROM {1} WHERE {2}";
                sql = String.Format(sql, Column, Table, Where);
                DbDataReader reader = null;
                if (MakeSureConnectionIsOpen())
                {
                    try
                    {
                        DbCommand command = oConn.CreateCommand();
                        command.CommandText = sql;
                        command.CommandType = CommandType.Text;

                        // Retrieve the data.
                        reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            rc = reader[0].ToString();
                            break; // We read only first row
                        }
                    }
                    catch (Exception e)
                    {
                        throw;
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Close();
                        }
                    }
                }
            }
            return rc;
        }

        public List<object> FreeSQL(string SQL, int? timeout = null)
        {
            MakeSureConnectionIsOpen();
            List<string> Headers = [];
            List<List<string>> Records = [];
            bool IsConnected = false;
            List<object> ReturnList = [];

            DbDataReader reader = null;
            try
            {
                if (oConn == null)
                {
                    IsConnected = Connect();
                }

                if (IsConnected || oConn != null)
                {
                    DbCommand command = oConn.CreateCommand();
                    command.CommandText = SQL;
                    command.CommandType = CommandType.Text;
                    if (timeout is not null and > 0)
                    {
                        command.CommandTimeout = (int)timeout;
                    }

                    // Retrieve the data.
                    reader = command.ExecuteReader();

                    // Create columns headers
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        Headers.Add(reader.GetName(i));
                    }

                    while (reader.Read())
                    {
                        List<string> record = [];
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            record.Add(reader[i].ToString());
                        }
                        Records.Add(record);
                    }

                    ReturnList.Add(Headers);
                    ReturnList.Add(Records);
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to execute query:" + SQL, e);
                throw;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return ReturnList;
        }

        public string GetRecordCount(string SQL)
        {

            string sql = "SELECT COUNT(1) FROM " + SQL;

            String rc = null;
            DbDataReader reader = null;
            if (MakeSureConnectionIsOpen())
            {
                try
                {
                    DbCommand command = oConn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    // Retrieve the data.
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        rc = reader[0].ToString();
                        break; // We read only first row = count of records
                    }
                }
                catch (Exception e)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to execute query:" + SQL, e);
                    throw;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }

            return rc;
        }

    }
}
