#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Oracle.ManagedDataAccess.Client;
using System.ComponentModel;
using Microsoft.Win32;
using System.Reflection;
using Npgsql;
using GingerCore.DataSource;
using GingerCore.NoSqlBase;
using MySql.Data.MySqlClient;

namespace GingerCore.Environments
{
    public class Database : RepositoryItemBase
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
        public bool mKeepConnectionOpen= true;
        [IsSerializedForLocalRepository]
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
       
        [IsSerializedForLocalRepository]
        public string Name { get; set; }

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
                        ProjEnvironment = new Environments.ProjEnvironment();
                    if (BusinessFlow == null)
                        BusinessFlow = new GingerCore.BusinessFlow();
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
                VE.Value = ConnectionString;
                return mVE.ValueCalculated;
            }
        }

        private string mTNS;
        [IsSerializedForLocalRepository]
        public string TNS  {  get  { return mTNS; } set { mTNS = value; OnPropertyChanged(Fields.TNS); } }
        public string TNSCalculated
        {
            get
            {
                VE.Value = TNS;
                return mVE.ValueCalculated;
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
                VE.Value = User;
                return mVE.ValueCalculated;
            }
        }

        private string mPass;
        [IsSerializedForLocalRepository]
        public string Pass { get { return mPass; } set { mPass = value; OnPropertyChanged(Fields.Pass); } }
        public string PassCalculated
        {
            get
            {
                VE.Value = Pass;
                return mVE.ValueCalculated;
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


        public string GetConnectionString()
        {
            string connStr = null;
            bool res;
            res = false;

            if (String.IsNullOrEmpty(ConnectionStringCalculated) == false)
            {
                connStr = ConnectionStringCalculated.Replace("{USER}", UserCalculated);

                String deCryptValue = EncryptionHandler.DecryptString(PassCalculated, ref res);
                if (res == true)
                    { connStr = connStr.Replace("{PASS}", deCryptValue); }
                else
                    { connStr = connStr.Replace("{PASS}", PassCalculated); }
            }
            else
            {
                String strConnString = TNSCalculated;
                String strProvider;
                connStr = "Data Source=" + TNSCalculated + ";User Id=" + UserCalculated + ";";

                String deCryptValue = EncryptionHandler.DecryptString(PassCalculated, ref res);

                if (res == true) { connStr = connStr + "Password=" + deCryptValue + ";"; }
                else { connStr = connStr + "Password=" + PassCalculated + ";"; }

                if (DBType == eDBTypes.MSAccess)
                {
                    if (strConnString.Contains(".accdb")) strProvider = "Provider=Microsoft.ACE.OLEDB.12.0;";
                    else strProvider = "Provider=Microsoft.Jet.OLEDB.4.0;";

                    connStr = strProvider + connStr;
                }
                else if (DBType == eDBTypes.DB2)
                {
                    connStr = "Server=" + TNSCalculated + ";Database=" + Name + ";UID=" + UserCalculated + "PWD=" + deCryptValue;
                }
                else if (DBType == eDBTypes.PostgreSQL)
                {
                    string[] host = TNSCalculated.Split(':');
                    if (host.Length == 2)
                    {
                        connStr = String.Format("Server ={0};Port={1};User Id={2}; Password={3};Database={4};", host[0], host[1], UserCalculated, deCryptValue, Name);
                    }
                    else
                    {
                        //    connStr = "Server=" + TNS + ";Database=" + Name + ";UID=" + User + "PWD=" + deCryptValue;
                        connStr = String.Format("Server ={0};User Id={1}; Password={2};Database={3};", TNSCalculated, UserCalculated, deCryptValue, Name);
                    }
                }
                else if (DBType == eDBTypes.MySQL)
                {
                    connStr = "Server=" + TNSCalculated + ";Database=" + Name + ";UID=" + UserCalculated + ";PWD=" + deCryptValue;
                }
            }

            return connStr;
        }

        //private CancellationTokenSource CTS = null;
        private DateTime LastConnectionUsedTime;


        private bool MakeSureConnectionIsOpen()
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

            string connectConnectionString = GetConnectionString();

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
                            OracleConnection oc = new OracleConnection();
                            oc.ConnectionString = connectConnectionString;
                            oc.Open();
                            oConn = oc;
                            break;
                        }
                        catch (Exception e)
                        {
                            String Temp = e.Message;
                            //if (Temp.Contains ("ORA-03111"))
                            if (Temp.Contains("ORA-03111") || Temp.Contains("ORA-01017"))
                            {
                                factory = DbProviderFactories.GetFactory("System.Data.OleDb");
                                oConn = factory.CreateConnection();
                                oConn.ConnectionString = "Provider=msdaora;" + connectConnectionString;
                                oConn.Open();
                                break;
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
                        // anything better than below?
                        // TODO: working only with mdb access97, not with accmdb
                        factory = DbProviderFactories.GetFactory("System.Data.OleDb");
                        oConn = factory.CreateConnection();
                        oConn.ConnectionString = connectConnectionString;
                        oConn.Open();
                        break;

                    case eDBTypes.DB2:

                        String DB2Cpath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\IBM\DB2\GLOBAL_PROFILE", "DB2PATH", "DNE");

                        if (System.IO.Directory.Exists(DB2Cpath))
                        {
                            String db2ConnectionString = GetConnectionString();

                            var DLL = Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + @"DLLs\IBM.Data.DB2.dll");

                            var class1Type = DLL.GetType("IBM.Data.DB2.DB2Connection");

                            //Now you can use reflection or dynamic to call the method. I will show you the dynamic way
                            object[] param = new object[1];
                            param[0] = db2ConnectionString;
                            dynamic c = Activator.CreateInstance(class1Type, param);
                            oConn = (DbConnection)c;
                            oConn.Open();
                        }
                        else
                        {
                            throw new Exception("DB2 Connect or IBM DB2 Drivers not installed.");
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
                            LastConnectionUsedTime = DateTime.Now;
                        return true;
                        break;

                    case eDBTypes.MySQL:
                        oConn = new MySqlConnection();
                        oConn.ConnectionString = connectConnectionString;
                        oConn.Open();
                        break;
                }
                if ((oConn != null) && (oConn.State == ConnectionState.Open))
                {
                    LastConnectionUsedTime = DateTime.Now;
                    return true;
                }
                
                
            }
            catch (Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "DB connection failed, DB type: " + DBType.ToString() + "; Connection String =" + connectConnectionString, e);
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
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to close DB Connection", e);
                throw (e);
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
                            }

                            rc.Add(tableName);
                        }
                    }
                }
                catch (Exception e)
                {
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to get table list for DB:" + DBType.ToString(), e);
                    throw (e);
                }
            }           
            return rc;
        }


        public List<string> GetTablesColumns(string table)
        {
            DbDataReader reader = null;
            List<string> rc = new List<string>() { "" };
            if ((oConn == null || string.IsNullOrEmpty(table))&& (DBType != Database.eDBTypes.Cassandra))
            {
                return rc;
            }
            if (DBType == Database.eDBTypes.Cassandra)
            {
                NoSqlBase.NoSqlBase NoSqlDriver = null;
                NoSqlDriver = new GingerCassandra(this);
                rc = NoSqlDriver.GetColumnList(table);
            }
            else 
            {
                try
                {
                    DbCommand command = oConn.CreateCommand();
                    // Do select with zero records
                    command.CommandText = "select * from " + table + " where 1 = 0";
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
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, "", e);
                    //Reporter.ToUser(eUserMsgKeys.DbTableError, "table columns", e.Message);
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
                        Reporter.ToLog(eAppReporterLogLevel.ERROR,"Commit failed for:"+updateCmd, e);
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
                    reader.Close();
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
                Reporter.ToLog(eAppReporterLogLevel.ERROR,"Failed to execute query:"+ SQL,e, writeOnlyInDebugMode:true);
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
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to execute query:" + SQL, e,writeOnlyInDebugMode: true);
                    throw e;
                }
                finally
                {
                    reader.Close();
                }
            }
            
            return rc;
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
