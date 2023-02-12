#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Amdocs.Ginger.Repository;
using GingerCore.Actions.Common;
using GingerCore.Environments;
using GingerCore.Helpers;
using GingerCore.NoSqlBase;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Enums;
using amdocs.ginger.GingerCoreNET;

namespace GingerCore.Actions
{
    // TODO: rename to DBAction
    public class ActDBValidation : ActWithoutDriver
    {
        public override string ActionDescription { get { return "DataBase Action"; } }
        public override string ActionUserDescription { get { return "Run Select/Update SQL on Database"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to pull/validate/update/etc. data from/on a database system.");
            TBH.AddLineBreak();
            TBH.AddText("This action contains list of options which will allow you to run simple or complicated SQL.");
        }

        public override string ActionEditPage { get { return "ValidationDBPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }
        public new static partial class Fields
        {
            public static string AppName = "AppName";
            public static string DBName = "DBName";
            public static string Keyspace = "Keyspace";
            public static string Table = "Table";
            public static string Column = "Column";
            public static string Where = "Where";
            public static string SQL = "SQL";
            public static string DatabaseTye = "DatabaseTye";
            public static string CommitDB = "CommitDB";
            public static string DBValidationType = "DBValidationType";
            public static string QueryTypeRadioButton = "QueryTypeRadioButton";
            public static string QueryFile = "QueryFile";
            public static string ImportFile = "ImportFile";
            public static string QueryParams = "QueryParams";
        }

        [IsSerializedForLocalRepository]
        public string AppName { set; get; }

        public string mDBName;
        [IsSerializedForLocalRepository]
        public string DBName { get { return mDBName; } set { mDBName = value; OnPropertyChanged(Fields.DBName); } }

        [IsSerializedForLocalRepository]
        public string Keyspace { set; get; }

        [IsSerializedForLocalRepository]
        public string Table { set; get; }

        [IsSerializedForLocalRepository]
        public string Column { set; get; }

        [IsSerializedForLocalRepository]
        public string Where { set; get; }

        public string PrimaryKey
        {
            get
            {
                return GetInputParamValue(nameof(PrimaryKey));
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(PrimaryKey), value);
            }
        }

        public string PartitionKey
        {
            get
            {
                return GetInputParamValue(nameof(PartitionKey));
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(PartitionKey), value);
            }
        }

        public string SQL
        {
            get
            {
                if (String.IsNullOrEmpty(GetInputParamValue("SQL")))
                {
                    AddOrUpdateInputParamValue("SQL", GetInputParamValue("Value"));
                }
                if (!string.IsNullOrEmpty(GetInputParamValue("Value")))
                {
                    RemoveInputParam("Value");
                }
                return GetInputParamValue("SQL");
            }
            set
            {
                AddOrUpdateInputParamValue("SQL", value);
            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> QueryParams = new ObservableList<ActInputValue>();

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> UpdateOperationInputValues = new ObservableList<ActInputValue>();

        public enum eDatabaseTye
        {
            Relational = 0,
            NoSQL = 1,
        }

        public eDatabaseTye mDatabaseType = eDatabaseTye.Relational;

        public eDatabaseTye DatabaseType
        {
            get
            {
                try
                {
                    if (DB != null && (DB.DBType == Database.eDBTypes.Cassandra || DB.DBType == Database.eDBTypes.Couchbase || DB.DBType == Database.eDBTypes.MongoDb || DB.DBType == Database.eDBTypes.CosmosDb))
                        return eDatabaseTye.NoSQL;
                    else
                        return eDatabaseTye.Relational;
                }

                catch
                {
                    return eDatabaseTye.Relational;
                }
            }
        }

        // No need to serialize - being calculated from env - App + DB
        public Database DB = null;

        public enum eQueryType
        {
            [EnumValueDescription("Free Text ")]
            FreeSQL,
            [EnumValueDescription("From File ")]
            SqlFile
        }

        public enum eDBValidationType
        {
            [EnumValueDescription("Free SQL")]
            FreeSQL = 1,                // SQL which result in one row one column value to be extracted
            [EnumValueDescription("Record Count")]
            RecordCount = 2,           // providing Table, and where clause return how many records
            [EnumValueDescription("Simple SQL One Value")]
            SimpleSQLOneValue = 3,     // provide table, lookupield, lookupvalue, outfield,  Like: select cutomerType from TBCustomer WHERE customer id=123    
            [EnumValueDescription("Update DB")]
            UpdateDB = 4,     // Run SQL/PL procedure 
            [EnumValueDescription("Insert")]
            Insert = 5
        }

        public bool CommitDB_Value
        {
            get
            {
                bool returnValue = true;
                if (Boolean.TryParse((GetInputParamValue(ActDBValidation.Fields.CommitDB)), out returnValue))
                {
                    return returnValue;
                }
                else
                    return false;
            }
        }

        [IsSerializedForLocalRepository]
        public eDBValidationType DBValidationType { get; set; }

        private string mInsertJson = string.Empty;

        [IsSerializedForLocalRepository]
        public string InsertJson
        {
            get
            {
                return mInsertJson;
            }
            set
            {
                mInsertJson = value;
            }
        }

        public string Params
        {
            get
            {
                string s = "DB Action - " + "AppName=" + AppName + ", DBName=" + DBName + " - ";
                switch (DBValidationType)
                {
                    case eDBValidationType.FreeSQL:
                        s = s + "Free SQL - " + SQL;
                        break;
                    case eDBValidationType.SimpleSQLOneValue:
                        s = s + "Simple SQL One Value - Table=" + Table + ", Column=" + Column + ", Where=" + Where;
                        break;
                    case eDBValidationType.RecordCount:
                        s = s + "Record Count from: " + SQL;
                        break;
                    case eDBValidationType.UpdateDB:
                        s = s + "Run update DB Procedure: " + SQL;
                        break;
                }
                return s;
            }
        }

        public override string ActionType
        {
            get { return "DB Action - " + DBValidationType.ToString(); }
        }

        public override List<ObservableList<ActInputValue>> GetInputValueListForVEProcessing()
        {
            List<ObservableList<ActInputValue>> list = new List<ObservableList<ActInputValue>>();
            list.Add(UpdateOperationInputValues);
            return list;
        }

        public override void Execute()
        {
            if (String.IsNullOrEmpty(GetInputParamValue("SQL")))
            {
                AddOrUpdateInputParamValue("SQL", GetInputParamValue("Value"));
            }

            if (SetDBConnection() == false)
                return;//Failed to find the DB in the Environment

            switch (DatabaseType)
            {
                case eDatabaseTye.Relational:
                    {
                        switch (DBValidationType)
                        {
                            case eDBValidationType.SimpleSQLOneValue:
                                SimpleSQLOneValueHandler();
                                break;

                            case eDBValidationType.FreeSQL:
                                FreeSQLHandler();
                                break;

                            case eDBValidationType.RecordCount:
                                RecordCountHandler();
                                break;
                            case eDBValidationType.UpdateDB:
                                UpdateSqlHndler();
                                break;
                            //TODO: add default and report error
                            default:
                                break;
                        }

                        if (!DB.KeepConnectionOpen)
                        {
                            DB.DatabaseOperations.CloseConnection();
                        }
                    }
                    break;

                case eDatabaseTye.NoSQL:
                    HandleNoSQLDBAction();
                    break;
                    //    //TODO: mark as assert calculator not found...
            }
        }

        NoSqlBase.NoSqlBase NoSqlDriver = null;
        private void HandleNoSQLDBAction()
        {
            switch (this.DB.DBType)
            {
                case Database.eDBTypes.Cassandra:
                    if (NoSqlDriver == null || NoSqlDriver.GetType() != typeof(GingerCassandra))
                    {
                        NoSqlDriver = new GingerCassandra(DBValidationType, DB, this);
                    }
                    break;
                case Database.eDBTypes.Couchbase:
                    if (NoSqlDriver == null || NoSqlDriver.GetType() != typeof(GingerCouchbase))
                    {
                        NoSqlDriver = new GingerCouchbase(DBValidationType, DB, this);
                    }
                    break;
                case Database.eDBTypes.MongoDb:
                    if (NoSqlDriver == null || NoSqlDriver.GetType() != typeof(GingerMongoDb))
                    {
                        NoSqlDriver = new GingerMongoDb(DBValidationType, DB, this);
                    }
                    break;
                case Database.eDBTypes.CosmosDb:
                    if (NoSqlDriver == null || NoSqlDriver.GetType() != typeof(GingerCosmos))
                    {
                        NoSqlDriver = new GingerCosmos(DBValidationType, DB, this);
                    }
                    break;
            }
            GetSqlValueFromFilePath();
            NoSqlDriver.MakeSureConnectionIsOpen();
            NoSqlDriver.PerformDBAction();
        }

        private bool SetDBConnection()
        {
            //TODO: add on null or not found throw exception so it will fail

            if (string.IsNullOrEmpty(PrimaryKey))
            {
                PrimaryKey = string.Empty;
            }
            if (string.IsNullOrEmpty(PartitionKey))
            {
                PartitionKey = string.Empty;
            }

            string AppNameCalculated = ValueExpression.Calculate(this.AppName);
            EnvApplication App = (from a in RunOnEnvironment.Applications where a.Name == AppNameCalculated select a).FirstOrDefault();
            if (App == null)
            {
                Error = "The mapped Environment Application '" + AppNameCalculated + "' was not found in the '" + RunOnEnvironment.Name + "' Environment which was selected for execution.";
                return false;
            }

            string DBNameCalculated = ValueExpression.Calculate(DBName);
            DB = (Database)(from d in App.Dbs where d.Name == DBNameCalculated select d).FirstOrDefault();
            if (DB == null)
            {
                Error = "The mapped DB '" + DBNameCalculated + "' was not found in the '" + AppNameCalculated + "' Environment Application.";
                return false;
            }
            DB.DSList = DSList;
            DB.ProjEnvironment = RunOnEnvironment;
            DB.BusinessFlow = RunOnBusinessFlow;

            DatabaseOperations databaseOperations = new DatabaseOperations(DB);
            DB.DatabaseOperations = databaseOperations;

            return true;
        }

        private void RecordCountHandler()
        {
            string calcSQL = GetInputParamCalculatedValue("SQL");
            if (string.IsNullOrEmpty(calcSQL))
                Error = "Fail to run Update SQL: " + Environment.NewLine + calcSQL + Environment.NewLine + "Error = Missing Query";
            string val = DB.DatabaseOperations.GetRecordCount(calcSQL);
            this.AddOrUpdateReturnParamActual("Record Count", val);
        }
        private void UpdateSqlHndler()
        {
            string calcSQL = string.Empty;
            try
            {
                if (GetInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton) == ActDBValidation.eQueryType.SqlFile.ToString())
                {
                    //string filePath = GetInputParamValue(ActDBValidation.Fields.QueryFile).Replace(@"~\", SolutionFolder);
                    string filePath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(GetInputParamValue(ActDBValidation.Fields.QueryFile));

                    FileInfo scriptFile = new FileInfo(filePath);
                    calcSQL = scriptFile.OpenText().ReadToEnd();
                }
                else
                {
                    calcSQL = GetInputParamCalculatedValue("SQL");
                }

                if (string.IsNullOrEmpty(calcSQL))
                    Error = "Fail to run Update SQL: " + Environment.NewLine + calcSQL + Environment.NewLine + "Error = Missing Query";

                string val = DB.DatabaseOperations.fUpdateDB(calcSQL, CommitDB_Value);
                this.AddOrUpdateReturnParamActual("Impacted Lines", val);

            }
            catch (Exception e)
            {
                if (string.IsNullOrEmpty(Error))
                    this.Error = "Fail to run Update SQL: " + Environment.NewLine + calcSQL + Environment.NewLine + "Error= " + e.Message;

            }
        }
        private void SimpleSQLOneValueHandler()
        {
            if (string.IsNullOrEmpty(Where))
                Where = "rownum<2";
            string val = DB.DatabaseOperations.fTableColWhere(Table, Column, Where);
            this.AddOrUpdateReturnParamActual(Column, val);
        }

        private void GetSqlValueFromFilePath()
        {
            if (GetInputParamValue(ActDBValidation.Fields.QueryTypeRadioButton) == ActDBValidation.eQueryType.SqlFile.ToString())
            {
                //string filePath = GetInputParamValue(ActDBValidation.Fields.QueryFile).Replace(@"~\", SolutionFolder);
                string filePath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(GetInputParamCalculatedValue(ActDBValidation.Fields.QueryFile));

                FileInfo scriptFile = new FileInfo(filePath);
                SQL = scriptFile.OpenText().ReadToEnd();
            }
        }

        private void FreeSQLHandler()
        {
            int? queryTimeout = Timeout;
            string calcSQL = GetInputParamCalculatedValue("SQL");
            string ErrorString = string.Empty;
            try
            {
                GetSqlValueFromFilePath();
                if (string.IsNullOrEmpty(calcSQL))
                    this.Error = "Fail to run Free SQL: " + Environment.NewLine + calcSQL + Environment.NewLine + "Error= Missing SQL Query.";

                updateQueryParams();
                foreach (ActInputValue param in QueryParams)
                    calcSQL = calcSQL.Replace("<<" + param.ItemName + ">>", param.ValueForDriver);

                List<object> DBResponse = DB.DatabaseOperations.FreeSQL(calcSQL, queryTimeout);

                List<string> headers = (List<string>)DBResponse.ElementAt(0);
                List<List<string>> Records = (List<List<string>>)DBResponse.ElementAt(1);


                if (Records.Count == 0) return;

                int recordcount = Records.Count;
                for (int j = 0; j < Records.Count; j++)

                {
                    List<string> currentrow = Records.ElementAt(j);


                    for (int k = 0; k < currentrow.Count; k++)
                    {
                        if (recordcount == 1)
                        {
                            this.AddOrUpdateReturnParamActual(headers.ElementAt(k), currentrow.ElementAt(k));
                        }
                        else
                        {
                            // Add the record number in the path col
                            this.AddOrUpdateReturnParamActualWithPath(headers.ElementAt(k), currentrow.ElementAt(k), (j + 1).ToString() + "");
                        }
                    }
                }

            }
            catch (Exception e)
            {
                if (string.IsNullOrEmpty(ErrorString))
                    this.Error = "Fail to run Free SQL: " + Environment.NewLine + calcSQL + Environment.NewLine + "Error= " + e.Message;
                else
                    this.Error = "Fail to execute query: " + Environment.NewLine + calcSQL + Environment.NewLine + "Error= " + ErrorString;

                if (e.Message.ToUpper().Contains("COULD NOT LOAD FILE OR ASSEMBLY 'ORACLE.MANAGEDDATAACCESS"))
                {
                    string message = DatabaseOperations.GetMissingDLLErrorDescription();
                    Reporter.ToLog(eLogLevel.WARN, message, e);
                    this.Error += Environment.NewLine + message;
                }
            }
        }

        public override eImageType Image { get { return eImageType.Database; } }

        public override ActionDetails Details
        {
            get
            {
                // We create a customized user friendly action details for actions grid and report
                ActionDetails d = base.Details;

                d.Info = this.SQL;
                // return params order by priority
                d.Params.Clear();
                d.Params.Add(new ActionParamInfo() { Param = "App", Value = AppName });
                d.Params.Add(new ActionParamInfo() { Param = "DB", Value = DBName });
                return d;
            }
        }
        private void updateQueryParams()
        {
            foreach (ActInputValue AIV in QueryParams)
            {
                if (!String.IsNullOrEmpty(AIV.Value))
                {
                    AIV.ValueForDriver = ValueExpression.Calculate(AIV.Value);
                }
            }
        }
    }
}