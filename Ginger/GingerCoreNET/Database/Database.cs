#region License
/*
Copyright © 2014-2020 European Support Limited

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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.ComponentModel;
using Microsoft.Win32;
using System.Reflection;
using Npgsql;
using GingerCore.DataSource;
using GingerCore.NoSqlBase;
using MySql.Data.MySqlClient;
using Amdocs.Ginger.Common.InterfacesLib;

using GingerCore.Actions;
using System.Runtime.InteropServices;
using amdocs.ginger.GingerCoreNET;

namespace GingerCore.Environments
{
    public class Database : RepositoryItemBase, IDatabase
    {        

        public enum eDBTypes
        {
            Oracle,
            MSSQL,            
            MSAccess,
            DB2,
            Cassandra,
            PostgreSQL,
            MySQL,
            Couchbase,
            MongoDb,
        }

        public enum eConfigType
        {
            Manual = 0,
            ConnectionString =1,            
        }

        public ProjEnvironment ProjEnvironment { get; set; }
       
        private BusinessFlow mBusinessFlow;
        public BusinessFlow BusinessFlow
        {
            get { return mBusinessFlow; }
            set
            {
                if (!object.ReferenceEquals(mBusinessFlow, value))
                {
                    mBusinessFlow = value;
                }
            }
        }

        public  static class Fields
        {
            public static string Name = "Name";
            public static string Description = "Description";
            public static string Type = "DBType";
            public static string ConnectionString = "ConnectionString";
            public static string TNS = "TNS";
            public static string User = "User";
            public static string Pass = "Pass";
            public static string KeepConnectionOpen = "KeepConnectionOpen";
            public static string DBVer = "DBVer";
        }

        private DbConnection oConn = null;
        private DbTransaction tran = null;

        public ObservableList<DataSourceBase> DSList { get; set; }
        public bool mKeepConnectionOpen;
        [IsSerializedForLocalRepository(true)]
        public bool KeepConnectionOpen
        {
            get
            {
                
                return mKeepConnectionOpen;
            }
            set
            {
                mKeepConnectionOpen = value;
                OnPropertyChanged(Fields.KeepConnectionOpen);
            }
        }


        private string mName;
        [IsSerializedForLocalRepository]
        public string Name { get { return mName; } set { mName = value; OnPropertyChanged(Fields.Name); } }

        [IsSerializedForLocalRepository]
        public string Description { get; set; }
        public eDBTypes mDBType;
        [IsSerializedForLocalRepository]
        public eDBTypes DBType { get { return mDBType; }
            set {
                mDBType = value;
                OnPropertyChanged(Fields.Type);
                if (DBType==eDBTypes.Cassandra)
                {
                    DBVer = "2.2";
                }
                else
                {
                    DBVer = "";
                }
            } }


        ValueExpression mVE = null;
        ValueExpression VE
        {
            get
            {
                if (mVE == null)
                {
                    if (ProjEnvironment == null)
                    {
                        ProjEnvironment = new Environments.ProjEnvironment();
                    }

                    if (BusinessFlow == null)
                    {
                        BusinessFlow = new GingerCore.BusinessFlow();
                    }

                    mVE = new ValueExpression(ProjEnvironment, BusinessFlow, DSList);
                }
                return mVE;
            }
            set
            {
                mVE = value;
            }
        }

        private string mConnectionString;
        [IsSerializedForLocalRepository]
        public string ConnectionString { get { return mConnectionString; } set { mConnectionString = value; OnPropertyChanged(Fields.ConnectionString); } }
        public string ConnectionStringCalculated
        {
            get
            {
                return GetCalculatedWithDecryptTrue(ConnectionString);
            }
        }

        private string mTNS;
        [IsSerializedForLocalRepository]
        public string TNS { get { return mTNS; } set { mTNS = value; OnPropertyChanged(Fields.TNS); } }
        public string TNSCalculated
        {
            get
            {
                return GetCalculatedWithDecryptTrue(TNS);
            }
        }

        private string mDBVer;
        [IsSerializedForLocalRepository]
        public string DBVer { get { return mDBVer; } set { mDBVer = value; OnPropertyChanged(Fields.DBVer); } }

        private string mUser;
        [IsSerializedForLocalRepository]
        public string User { get { return mUser; } set { mUser = value; OnPropertyChanged(Fields.User); } }
        public string UserCalculated
        {
            get
            {
                return GetCalculatedWithDecryptTrue(User);
            }
        }

        private string mPass;
        [IsSerializedForLocalRepository]
        public string Pass { get { return mPass; } set { mPass = value; OnPropertyChanged(Fields.Pass); } }
        public string PassCalculated
        {
            get
            {
                return GetCalculatedWithDecryptTrue(Pass);
            }
        }

        //TODO: Why it is needed?!
        public static List<string> DbTypes 
        {
            get
            {
                return Enum.GetNames(typeof(eDBTypes)).ToList();
            }
            set
            {
                //DbTypes = value;
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
            SqlConnectionStringBuilder scSB = new SqlConnectionStringBuilder();
            scSB.ConnectionString = TNS;
            TNS = scSB.DataSource;
            User = scSB.UserID;
            Pass = scSB.Password;
            ConnectionString = scSB.ConnectionString;
        }
        public string GetConnectionString()
        {
            string connStr = null;
            bool res;
            res = false;

            if (String.IsNullOrEmpty(ConnectionStringCalculated))
            {
                connStr = CreateConnectionString();
            }
            connStr = ConnectionStringCalculated.Replace("{USER}", UserCalculated);
            String deCryptValue = EncryptionHandler.DecryptString(PassCalculated, ref res, false);
            if (res == true)
            { connStr = connStr.Replace("{PASS}", deCryptValue); }
            else
            { connStr = connStr.Replace("{PASS}", PassCalculated); }
            
            return connStr;
        }

        public string CreateConnectionString()
        {
            //Default ConnectionString format
            ConnectionString = "Data Source=" + TNS + ";User Id={USER};Password={PASS};";

            //Change ConnectionString according to DBType
            if (DBType == eDBTypes.MSAccess)
            {
                string strProvider;
                if (TNSCalculated.Contains(".accdb"))
                {
                    strProvider = "Provider=Microsoft.ACE.OLEDB.12.0;";
                }
                else
                {
                    strProvider = "Provider=Microsoft.Jet.OLEDB.4.0;";
                }
                ConnectionString = strProvider + ConnectionString;
            }
            else if (DBType == eDBTypes.DB2)
            {
                ConnectionString = "Server=" + TNS + ";Database=" + Name + ";UID={USER};PWD={PASS}";
            }
            else if (DBType == eDBTypes.PostgreSQL)
            {
                string[] host = TNSCalculated.Split(':');
                if (host.Length == 2)
                {
                    ConnectionString = "Server=" + host[0] + ";Port=" + host[1] + ";User Id={USER}; Password={PASS};Database=" + Name + ";";
                }
                else
                {
                    //    connStr = "Server=" + TNS + ";Database=" + Name + ";UID=" + User + "PWD=" + deCryptValue;
                    ConnectionString = "Server=" + TNS + ";User Id={USER}; Password={PASS};Database=" + Name + ";";
                }
            }
            else if (DBType == eDBTypes.MySQL)
            {
                ConnectionString = "Server=" + TNS + ";Database=" + Name + ";UID={USER};PWD={PASS}";
            }
            return ConnectionStringCalculated;
        }
        //private CancellationTokenSource CTS = null;
        private DateTime LastConnectionUsedTime;


        public bool MakeSureConnectionIsOpen()
        {
            Boolean isCoonected = true;

            if ((oConn == null) || (oConn.State != ConnectionState.Open))
                isCoonected= Connect();

            //make sure that the connection was not refused by the server               
            TimeSpan timeDiff = DateTime.Now - LastConnectionUsedTime;
            if (timeDiff.TotalMinutes > 5)
            {
                isCoonected= Connect();                
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
            DbProviderFactory factory;
            string connectConnectionString = string.Empty;
            if (DBType != eDBTypes.Cassandra && DBType != eDBTypes.Couchbase && DBType != eDBTypes.MongoDb)
            {
                connectConnectionString = GetConnectionString();
            }
            try
            {

                switch (DBType)
                {
                    case eDBTypes.MSSQL:
                        oConn = new SqlConnection();
                        oConn.ConnectionString = connectConnectionString;
                        oConn.Open();
                        break;
                    case eDBTypes.Oracle:
                        //TODO: Oracle connection is deprecated use another method - Switched to ODP.NET
                        //Try Catch for Connecting DB Which having Oracle Version Less then 10.2                         
                        try
                        {
                            var DLL = Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + @"Oracle.ManagedDataAccess.dll");
                            var class1Type = DLL.GetType("Oracle.ManagedDataAccess.Client.OracleConnection");
                            object[] param = new object[1];
                            param[0] = connectConnectionString;
                            dynamic c = Activator.CreateInstance(class1Type, param);
                            oConn = (DbConnection)c;
                            oConn.Open();
                            break;
                        }
                        catch (Exception e)
                        {
                            String Temp = e.Message;
                            //if (Temp.Contains ("ORA-03111"))
                            if (Temp.Contains("ORA-03111"))
                            {
                     
                              
                                oConn = SqlClientFactory.Instance.CreateConnection();
                                oConn.ConnectionString = "Provider=msdaora;" + connectConnectionString;
                                oConn.Open();
                                break;
                            }
                            else if (Temp.Contains("ORA-01017"))
                            {
                                throw e;
                            }
                            else if (!System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Oracle.ManagedDataAccess.dll"))
                            {

                                throw new Exception(GetMissingDLLErrorDescription());
                            }
                            else
                            {
                                throw e;
                            }
                        }

                    case eDBTypes.MSAccess:


                        oConn = RepositoryItemHelper.RepositoryItemFactory.GetMSAccessConnection();
                        oConn.ConnectionString = connectConnectionString;
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
                                object[] param = new object[1];
                                param[0] = connectConnectionString;
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
                        oConn = new NpgsqlConnection(connectConnectionString);
                        oConn.Open();
                        break;

                    case eDBTypes.Cassandra:
                        GingerCassandra CassandraDriver = new GingerCassandra(this);
                        bool isConnection;
                        isConnection= CassandraDriver.Connect();
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
                        GingerCouchbase CouchbaseDriver = new GingerCouchbase(this);
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
                        oConn = new MySqlConnection();
                        oConn.ConnectionString = connectConnectionString;
                        oConn.Open();
                        break;
                    case eDBTypes.MongoDb:
                        bool isConnectionMDB;
                        GingerMongoDb MongoDriver = new GingerMongoDb(this);
                        isConnectionMDB = MongoDriver.Connect();
                        if (isConnectionMDB == true)
                        {
                            LastConnectionUsedTime = DateTime.Now;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                }
                if ((oConn != null) && (oConn.State == ConnectionState.Open))
                {
                    LastConnectionUsedTime = DateTime.Now;
                    return true;
                }
                
                
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "DB connection failed, DB type: " + DBType.ToString() + "; Connection String =" + HidePasswordFromString(connectConnectionString), e);
                throw (e);
            }
            return false;
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
                throw (e);
            }
            finally
            {
                oConn?.Dispose();
            }
        }

       //prep for Db edit page
       public static List<eConfigType> GetSupportedConfigurations(eDBTypes CurrentDbType)
        {
            List<eConfigType> SupportedConfigs = new List<eConfigType>();

            switch (CurrentDbType)
            {
                case eDBTypes.Cassandra:
                    SupportedConfigs.Add(eConfigType.Manual);
                    break;
                default:
                    SupportedConfigs.Add(eConfigType.ConnectionString);
                    break;
            }
            
            return SupportedConfigs;
        }

        public List<string> GetTablesList(string Keyspace = null)
        {
           
            List<string> rc = new List<string>() { "" };
            if (MakeSureConnectionIsOpen())
            {
                try
                {
                    //if (oConn == null || oConn.State == ConnectionState.Closed) Connect();
                    if (DBType == Database.eDBTypes.Cassandra)
                    {
                        NoSqlBase.NoSqlBase NoSqlDriver = null;
                        NoSqlDriver = new GingerCassandra(this);
                        rc = NoSqlDriver.GetTableList(Keyspace);
                    } else if (DBType == Database.eDBTypes.Couchbase)
                    {
                        NoSqlBase.NoSqlBase NoSqlDriver = null;
                        NoSqlDriver = new GingerCouchbase(this);
                        rc = NoSqlDriver.GetTableList(Keyspace);
                    }
                    else if (DBType == Database.eDBTypes.MongoDb)
                    {
                        NoSqlBase.NoSqlBase NoSqlDriver = null;
                        NoSqlDriver = new GingerMongoDb(this);
                        rc = NoSqlDriver.GetTableList(Keyspace);
                    }
                    else
                    {
                        DataTable table = oConn.GetSchema("Tables");
                        string tableName = "";
                        foreach (DataRow row in table.Rows)
                        {
                            switch (DBType)
                            {
                                case eDBTypes.MSSQL:
                                    tableName = (string)row[2];
                                    break;
                                case eDBTypes.Oracle:
                                    tableName = (string)row[1];
                                    break;
                                case eDBTypes.MSAccess:
                                    tableName = (string)row[2];
                                    break;
                                case eDBTypes.DB2:
                                    tableName = (string)row[2];
                                    break;
                                case eDBTypes.MySQL:
                                    tableName = (string)row[2];
                                    break;
                                case eDBTypes.PostgreSQL:
                                    tableName = (string)row[2];
                                    break;
                            }

                            rc.Add(tableName);
                        }
                    }
                }
                catch (Exception e)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to get table list for DB:" + DBType.ToString(), e);
                    throw (e);
                }
            }           
            return rc;
        }


        public List<string> GetTablesColumns(string table)
        {
            DbDataReader reader = null;
            List<string> rc = new List<string>() { "" };
            if ((oConn == null || string.IsNullOrEmpty(table))&& (DBType != Database.eDBTypes.Cassandra) && (DBType != Database.eDBTypes.MongoDb))
            {
                return rc;
            }
            if (DBType == Database.eDBTypes.Cassandra)
            {
                NoSqlBase.NoSqlBase NoSqlDriver = null;
                NoSqlDriver = new GingerCassandra(this);
                rc = NoSqlDriver.GetColumnList(table);
            }else if (DBType == Database.eDBTypes.Couchbase)
            {
                NoSqlBase.NoSqlBase NoSqlDriver = null;
                NoSqlDriver = new GingerCouchbase(this);
                rc = NoSqlDriver.GetColumnList(table);
            }
            else if (DBType == Database.eDBTypes.MongoDb)
            {
                NoSqlBase.NoSqlBase NoSqlDriver = null;
                NoSqlDriver = new GingerMongoDb(this);
                rc = NoSqlDriver.GetColumnList(table);
            }
            else 
            {
                try
                {
                    DbCommand command = oConn.CreateCommand();
                    // Do select with zero records
                    switch(DBType)
                    {
                        case eDBTypes.PostgreSQL:
                            command.CommandText = "select * from public.\"" + table + "\" where 1 = 0";
                            break;
                        default:
                            command.CommandText = "select * from " + table + " where 1 = 0";
                            break;
                    }

                    command.CommandType = CommandType.Text;

                    reader = command.ExecuteReader();
                    // Get the schema and read the cols
                    DataTable schemaTable = reader.GetSchemaTable();
                    foreach (DataRow row in schemaTable.Rows)
                    {
                        string ColName = (string)row[0];
                        rc.Add(ColName);
                    }
                }
                catch (Exception e)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "", e);
                    //Reporter.ToUser(eUserMsgKey.DbTableError, "table columns", e.Message);
                    throw (e);
                }
                finally
                {
                    reader.Close();

                }
            }
            return rc;
        }
        
        public string fUpdateDB(string updateCmd, bool commit)
        {            
            string result = "";
            //if (oConn == null) Connect();
            if(MakeSureConnectionIsOpen())
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
                        Reporter.ToLog(eLogLevel.ERROR,"Commit failed for:"+updateCmd, e);
                        throw e;
                    }
                }
            }            
            return result;
        }

        public string fTableColWhere(string Table, string Column, string Where)
        {
          

            string sql = "SELECT {0} FROM {1} WHERE {2}";
            sql = String.Format(sql, Column, Table, Where);
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
                        break; // We read only first row
                    }
                }
                catch (Exception e)
                {
                    throw e;
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


        public List<object> FreeSQL(string SQL, int? timeout=null)
        {
            MakeSureConnectionIsOpen();
            List<string>  Headers = new List<string>();
            List<List<string>> Records = new List<List<string>> ();
            bool IsConnected = false;
            List<object> ReturnList = new List<object> ();

            DbDataReader reader = null;
            try
            {
                if (oConn == null)
                    IsConnected = Connect();
                    if (IsConnected || oConn!=null)
                    {
                        DbCommand command = oConn.CreateCommand();
                        command.CommandText = SQL;
                        command.CommandType = CommandType.Text;
                        if ((timeout != null) && (timeout > 0))
                            command.CommandTimeout = (int)timeout;


                        // Retrieve the data.
                        reader = command.ExecuteReader();

                        // Create columns headers
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Headers.Add(reader.GetName(i));
                        }

                        while (reader.Read())
                        {

                            List<string> record = new List<string>();
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
                Reporter.ToLog(eLogLevel.ERROR,"Failed to execute query:"+ SQL, e);
                throw e;
            }
            finally
            {
                if(reader!=null)
                    reader.Close();
            }
                return ReturnList;            
        }


        internal string GetRecordCount(string SQL)
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
                    throw e;
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
            passwordValue = passwordValue.Substring(passwordValue.ToLower().IndexOf(passwordString));
            int startIndex = passwordValue.ToLower().IndexOf(passwordString) + passwordString.Length;
            int endIndex = -1;
            if (passwordValue.Contains(";"))
            {
                endIndex = passwordValue.ToLower().IndexOf(";");
            }
            if (endIndex == -1)
            {
                passwordValue = passwordValue.Substring(startIndex);
            }
            else
            {
                passwordValue = passwordValue.Substring(startIndex, endIndex - startIndex);
            }

            if (!string.IsNullOrEmpty(passwordValue))
            {
                dataString = dataString.Replace(passwordValue, "*****");
            }
            return dataString;
        }
        public override string ItemName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }
    }
}
