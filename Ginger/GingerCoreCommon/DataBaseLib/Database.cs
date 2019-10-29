
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
                OnPropertyChanged(nameof(KeepConnectionOpen));
            }
        }


        private string mName;
        [IsSerializedForLocalRepository]
        public string Name { get { return mName; } set { mName = value; OnPropertyChanged(nameof(Name)); } }

        [IsSerializedForLocalRepository]
        public string Description { get; set; }


        private string mServiceID;
        [IsSerializedForLocalRepository]
        public string ServiceID { get { return mServiceID; } set { mServiceID = value; OnPropertyChanged(nameof(ServiceID)); } }



        // TODO: Obsolete remove after moving to DB pluguins
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
                // TODO: update service ID and add relevant DB plugin

                if (mDBType != value)
                {
                    databaseImpl = null;
                    mDBType = value;
                    OnPropertyChanged(nameof(DBType));

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
                        ProjEnvironment = new ProjEnvironment();
                    }

                    if (BusinessFlow == null)
                    {
                        BusinessFlow = new BusinessFlow();
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
        public string ConnectionString { get { return mConnectionString; } set { mConnectionString = value; OnPropertyChanged(nameof(ConnectionString)); } }
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
        public string TNS  {  get  { return mTNS; } set { mTNS = value; OnPropertyChanged(nameof(TNS)); } }
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
        public string DBVer { get { return mDBVer; } set { mDBVer = value; OnPropertyChanged(nameof(DBVer)); } }

        private string mUser;
        [IsSerializedForLocalRepository]
        public string User { get { return mUser; } set { mUser = value; OnPropertyChanged(nameof(User)); } }
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
        public string Pass { get { return mPass; } set { mPass = value; OnPropertyChanged(nameof(Pass)); } }
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
            VerifyDBImpl();            
            bool b = databaseImpl.TestConnection();
            return b;
        }

        public static IDBProvider iDBProvider { get; set; }

        void VerifyDBImpl()
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
            VerifyDBImpl();
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
            VerifyDBImpl();
            return databaseImpl.GetTablesList();
        }


        public List<string> GetTablesColumns(string table)
        {
            VerifyDBImpl();
            return databaseImpl.GetTablesColumns(table);            
        }
        
        public string UpdateDB(string updateCmd, bool commit)
        {
            VerifyDBImpl();
            string dataTable = databaseImpl.RunUpdateCommand(updateCmd, commit);
            return dataTable;            
        }

        public string GetSingleValue(string Table, string Column, string Where)
        {
            VerifyDBImpl();
            string value = databaseImpl.GetSingleValue(Table, Column, Where);  
            return value;            
        }


        public DataTable QueryDatabase(string query, int? timeout=null)
        {            
            VerifyDBImpl();
            DataTable dataTable = databaseImpl.DBQuery(query);
            return dataTable;           
        }


        internal int GetRecordCount(string query)
        {
            VerifyDBImpl();
            int count = databaseImpl.GetRecordCount(query);
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
