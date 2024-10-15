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

using System;
using System.Collections.Generic;
using System.Linq;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using GingerCore.DataSource;

namespace GingerCore.Environments
{
    public class Database : RepositoryItemBase, IDatabase
    {
        public IDatabaseOperations DatabaseOperations;

        public string GetConnectionStringToolTip()
        {
            return DBType switch
            {
                eDBTypes.DB2 => "Server={Server URL};Database={Database Name};UID={User Name};PWD={User Password};",
                eDBTypes.PostgreSQL => "Server={Server URL};User Id={User Name}; Password={User Password};Database={Database Name};",
                eDBTypes.MySQL => "Server={Server URL};Database={Database Name};UID={User Name};PWD={User Password};",
                eDBTypes.CosmosDb => "AccountEndpoint={End Point URL};AccountKey={Account Key};",
                eDBTypes.Hbase => "Server={Server URL};Port={Port No};User Id={User Name}; Password={Password};Database={Database Name};",
                eDBTypes.MongoDb => "mongodb://database_username:password@server_url/DBName",
                _ => "Data Source={Data Source};User Id={User Name};Password={User Password};",
            };
        }
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
            CosmosDb,
            Hbase
        }


        // used only for UI , if the database connection fails the status changes to Fail and
        // if it passes the status changes accordingly

        private eRunStatus mTestConnectionStatus = eRunStatus.Pending;
        public eRunStatus TestConnectionStatus
        {

            get
            {
                return mTestConnectionStatus;
            }

            set
            {
                mTestConnectionStatus = value;
                OnPropertyChanged(nameof(TestConnectionStatus));
            }
        }

        public enum eConfigType
        {
            Manual = 0,
            ConnectionString = 1,
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
                if (mKeepConnectionOpen != value)
                {
                    mKeepConnectionOpen = value;
                    OnPropertyChanged(nameof(KeepConnectionOpen));
                }
            }
        }

        public static eImageType Image
        {
            get
            {
                return eImageType.Database;
            }
        }

        private string mName;
        [IsSerializedForLocalRepository]
        public string Name { get { return mName; } set { if (mName != value) { mName = value; OnPropertyChanged(nameof(Name)); } } }

        [IsSerializedForLocalRepository]
        public string Description { get; set; }
        public eDBTypes mDBType;
        [IsSerializedForLocalRepository]
        public eDBTypes DBType
        {
            get { return mDBType; }
            set
            {
                if (mDBType != value)
                {
                    mDBType = value;
                    OnPropertyChanged(nameof(DBType)); // why we previously sent the property name as Sytem.Type rather than the DBType?
                }
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


        private string mConnectionString;
        [IsSerializedForLocalRepository]
        public string ConnectionString { get { return mConnectionString; } set { if (mConnectionString != value) { mConnectionString = value; OnPropertyChanged(nameof(ConnectionString)); } } }

        private string mTNS;
        [IsSerializedForLocalRepository]
        public string TNS { get { return mTNS; } set { if (mTNS != value) { mTNS = value; OnPropertyChanged(nameof(TNS)); } } }

        private string mDBVer;
        [IsSerializedForLocalRepository]
        public string DBVer { get { return mDBVer; } set { if (mDBVer != value) { mDBVer = value; OnPropertyChanged(nameof(DBVer)); } } }

        private string mUser;
        [IsSerializedForLocalRepository]
        public string User { get { return mUser; } set { if (mUser != value) { mUser = value; OnPropertyChanged(nameof(User)); } } }

        private string mPass;
        [IsSerializedForLocalRepository]
        public string Pass { get { return mPass; } set { if (mPass != value) { mPass = value; OnPropertyChanged(nameof(Pass)); } } }

        // checks if oracle version is lower than 10.2
        private bool mIsOracleVersionLow;
        [IsSerializedForLocalRepository]
        public bool IsOracleVersionLow
        {
            get { return mIsOracleVersionLow; }
            set
            {
                mIsOracleVersionLow = value;
                OnPropertyChanged(nameof(IsOracleVersionLow));
            }
        }

        public static List<string> DbTypes
        {
            get
            {
                return Enum.GetNames(typeof(eDBTypes)).ToList();
            }
        }

        private DateTime LastConnectionUsedTime;


        public static List<eConfigType> GetSupportedConfigurations(eDBTypes CurrentDbType)
        {
            List<eConfigType> SupportedConfigs = [];

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

        // Encrypts if the password is not a value expression or if it is already encrypted
        public void EncryptDatabasePass()
        {
            if (!string.IsNullOrEmpty(Pass) && !this.DatabaseOperations.IsPassValueExp() && !EncryptionHandler.IsStringEncrypted(Pass))
            {
                Pass = EncryptionHandler.EncryptwithKey(Pass);
            }
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
