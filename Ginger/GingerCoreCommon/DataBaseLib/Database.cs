
#region License
/*
Copyright © 2014-2019 European Support Limited

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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.DataBaseLib;
using Amdocs.Ginger.Plugin.Core.Database;
using Amdocs.Ginger.Repository;
using GingerCore.DataSource;

namespace GingerCore.Environments
{
    public class Database : RepositoryItemBase 
    {
        IDatabase databaseImpl;


        // TODO: change to Service ID

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


        // TODO: remove !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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
        public eDBTypes DBType 
        { 
            get 
            { 
                return mDBType; 
            }
            set 
            {
                if (mDBType != value)
                {
                    databaseImpl = null;
                    mDBType = value;
                    OnPropertyChanged(Fields.Type);

                    // TODO: remove from here
                    if (DBType == eDBTypes.Cassandra)
                    {
                        DBVer = "2.2";
                    }
                    else
                    {
                        DBVer = "";
                    }
                }
            } 
        }


        IValueExpression mVE = null;
        IValueExpression VE
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

                    mVE = RepositoryItemHelper.RepositoryItemFactory.CreateValueExpression(ProjEnvironment, BusinessFlow, DSList);
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

        public string NameBeforeEdit;

       
        //public string GetConnectionString()
        //{
        //    string connStr = null;
        //    bool res;
        //    res = false;

        //    if (String.IsNullOrEmpty(ConnectionStringCalculated) == false)
        //    {
                

        //        connStr = ConnectionStringCalculated.Replace("{USER}", UserCalculated);

        //        // FIXME !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //        String deCryptValue = "aaa"; // EncryptionHandler.DecryptString(PassCalculated, ref res, false);
        //        if (res == true)                
        //        {
        //            connStr = connStr.Replace("{PASS}", deCryptValue);
        //        }                
        //        else
        //        {
        //            connStr = connStr.Replace("{PASS}", PassCalculated);
        //        }
        //    }
        //    else
        //    {
        //        String strConnString = TNSCalculated;
        //        String strProvider;
        //        connStr = "Data Source=" + TNSCalculated + ";User Id=" + UserCalculated + ";";

        //        // FIXME !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //        String deCryptValue = "aaa"; // EncryptionHandler.DecryptString(PassCalculated, ref res, false);  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        //        if (res == true) { connStr = connStr + "Password=" + deCryptValue + ";"; }
        //        else { connStr = connStr + "Password=" + PassCalculated + ";"; }

        //        if (DBType == eDBTypes.MSAccess)
        //        {
        //            if (strConnString.Contains(".accdb")) strProvider = "Provider=Microsoft.ACE.OLEDB.12.0;";
        //            else strProvider = "Provider=Microsoft.Jet.OLEDB.4.0;";

        //            connStr = strProvider + connStr;
        //        }
        //        else if (DBType == eDBTypes.DB2)
        //        {
        //            connStr = "Server=" + TNSCalculated + ";Database=" + Name + ";UID=" + UserCalculated + "PWD=" + deCryptValue;
        //        }
        //        else if (DBType == eDBTypes.PostgreSQL)
        //        {
        //            string[] host = TNSCalculated.Split(':');
        //            if (host.Length == 2)
        //            {
        //                connStr = String.Format("Server ={0};Port={1};User Id={2}; Password={3};Database={4};", host[0], host[1], UserCalculated, deCryptValue, Name);
        //            }
        //            else
        //            {
        //                //    connStr = "Server=" + TNS + ";Database=" + Name + ";UID=" + User + "PWD=" + deCryptValue;
        //                connStr = String.Format("Server ={0};User Id={1}; Password={2};Database={3};", TNSCalculated, UserCalculated, deCryptValue, Name);
        //            }
        //        }
        //        else if (DBType == eDBTypes.MySQL)
        //        {
        //            connStr = "Server=" + TNSCalculated + ";Database=" + Name + ";UID=" + UserCalculated + ";PWD=" + deCryptValue;
        //        }
                
        //    }

        //    return connStr;
        //}
        
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




        public Boolean TestConnection()
        {
            LoadDBAssembly();            
            bool b = databaseImpl.TestConnection();
            return b;
        }

        public static IDBProvider iDBProvider { get; set; }

        void LoadDBAssembly()
        {
            if (databaseImpl != null) 
            {
                return;
            };  //TODO: Add check that the db is as DBType else replace or any prop change then reset conn string


            if (iDBProvider == null)
            {
                throw new ArgumentNullException("iDBProvider cannot be null and must be initialized");
            }
            
            databaseImpl = iDBProvider.GetDBImpl(this);
            databaseImpl.ConnectionString = ConnectionString; 
        }

        public Boolean Connect(bool displayErrorPopup = false)
        {
            LoadDBAssembly();
            if (databaseImpl != null)
            {
                return databaseImpl.TestConnection();                
            }
            else
            {
                return false;
            }

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

       

        public List<string> GetTablesList(string Keyspace = null)
        {
            LoadDBAssembly();
            return databaseImpl.GetTablesList();
        }


        public List<string> GetTablesColumns(string table)
        {
            LoadDBAssembly();
            return databaseImpl.GetTablesColumns(table);            
        }
        
        public string fUpdateDB(string updateCmd, bool commit)
        {
            LoadDBAssembly();
            string dataTable = databaseImpl.RunUpdateCommand(updateCmd, commit);
            return dataTable;            
        }

        public string GetSingleValue(string Table, string Column, string Where)
        {
            LoadDBAssembly();
            string vv = databaseImpl.GetSingleValue(Table, Column, Where);  // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            return vv;            
        }


        public DataTable FreeSQL(string SQL, int? timeout=null)
        {            
            LoadDBAssembly();
            DataTable dataTable = databaseImpl.DBQuery(SQL);
            return dataTable;           
        }


        internal int GetRecordCount(string SQL)
        {
            int count = databaseImpl.GetRecordCount(SQL);
            return count;
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
