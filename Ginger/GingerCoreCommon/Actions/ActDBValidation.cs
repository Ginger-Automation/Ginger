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
using System.IO;
using System.Linq;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions.Common;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

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

        public override string ActionEditPage { get { return "ActDatabaseEditPage"; } }
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
       
        public string ImportFile
        {
            get
            {
                return GetInputParamValue(nameof(ImportFile));
            }
        }
        public string QueryFile
        {
            get
            {
                return GetInputParamValue(nameof(QueryFile));
            }
        }
        public string QueryTypeRadioButton
        {
            get
            {
                return GetInputParamValue(nameof(QueryTypeRadioButton));
            }
        }
        public string CommitDB
        {
            get
            {
                return GetInputParamValue(nameof(CommitDB));
            }
        }
        [IsSerializedForLocalRepository]
        public string AppName { set; get; }

        private string mDBName;

        [IsSerializedForLocalRepository]
        public string DBName { get { return mDBName; } set { mDBName = value; OnPropertyChanged(nameof (DBName)); } }

        [IsSerializedForLocalRepository]
        public string Keyspace { set; get; }

        [IsSerializedForLocalRepository]
        public string Table { set; get; }

        [IsSerializedForLocalRepository]
        public string Column { set; get; }

        [IsSerializedForLocalRepository]
        public string Where { set; get; }
        
        [IsSerializedForLocalRepository]
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
                OnPropertyChanged(nameof(SQL));
            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> QueryParams = new ObservableList<ActInputValue>();

        public enum eDatabaseTye
        {
            Relational=0,
            NoSQL=1,
        }

        public eDatabaseTye mDatabaseType = eDatabaseTye.Relational;

        public eDatabaseTye DatabaseType
        {
            get
            {try
                {
                    if (DB.DBType == Database.eDBTypes.Cassandra || DB.DBType == Database.eDBTypes.Couchbase || DB.DBType == Database.eDBTypes.MongoDb)
                        return eDatabaseTye.NoSQL;
                    else return eDatabaseTye.Relational;
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
        }
        
        public bool CommitDB_Value
        {
            get
            {
                bool returnValue = true;
                if (Boolean.TryParse(CommitDB, out returnValue))
                {
                    return returnValue;
                }
                else
                    return false;
            }
        }

        [IsSerializedForLocalRepository]
        public eDBValidationType DBValidationType { get; set; }

        public string Params
        {
            get
            {
                string s = "DB Action - " + "AppName=" + AppName + ", DBName=" + DBName + " - ";
                switch (DBValidationType)
                {
                    case eDBValidationType.FreeSQL:
                        s = s + "Free SQL - " + SQL ;
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
            get { return "DB Action - " + DBValidationType.ToString() ; }
        }

        public override void Execute()
        {
            if (String.IsNullOrEmpty(GetInputParamValue("SQL")))
            {
                AddOrUpdateInputParamValue("SQL", GetInputParamValue("Value"));
            }

            if (SetDBConnection() == false)
                return;//Failed to find the DB in the Environment

            //switch (DatabaseType)
            //{
            //    case eDatabaseTye.Relational:
            //        {
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
                DB.CloseConnection();
            }
        }
                //    }
                //    break;

                //case eDatabaseTye.NoSQL:
                //    HandleNoSQLDBAction();
                //    break;
                    //    //TODO: mark as assert calculator not found...
           

        //private void HandleNoSQLDBAction()
        //{
        //    // FIXME !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        //    //NoSqlBase.NoSqlBase NoSqlDriver = null;

        //    switch (this.DB.DBType)
        //    {
        //        case Database.eDBTypes.Cassandra:
        //            //NoSqlDriver = new GingerCassandra(DBValidationType, DB, this);
        //            //NoSqlDriver.PerformDBAction();

        //            break;
        //        case Database.eDBTypes.Couchbase:
        //            NoSqlDriver = new GingerCouchbase(DBValidationType, DB, this);
        //            NoSqlDriver.PerformDBAction();

        //            break;
        //        case Database.eDBTypes.MongoDb:
        //            NoSqlDriver = new GingerMongoDb(DBValidationType, DB, this);
        //            NoSqlDriver.PerformDBAction();

        //            break;
        //    }
        //}

        private bool SetDBConnection()
        {
            //TODO: add on null or not found throw exception so it will fail            
            string AppNameCalculated = ValueExpression.Calculate(this.AppName);
            EnvApplication App = (from a in RunOnEnvironment.Applications where a.Name == AppNameCalculated select a).FirstOrDefault();
            if (App == null)
            {
                Error= "The mapped Environment Application '" + AppNameCalculated + "' was not found in the '" + RunOnEnvironment.Name +"' Environment which was selected for execution.";
                return false;
            }
            
            string DBNameCalculated = ValueExpression.Calculate(DBName);
            DB = (Database)(from d in App.Dbs where d.Name == DBNameCalculated select d).FirstOrDefault();
            if (DB ==null)
            {
                Error = "The mapped DB '" + DBNameCalculated + "' was not found in the '" + AppNameCalculated + "' Environment Application.";
                return false;
            }
            DB.DSList = DSList;
            DB.ProjEnvironment = RunOnEnvironment;
            DB.BusinessFlow = RunOnBusinessFlow;

            return true;
        }

        private void RecordCountHandler()
        {
            string count = GetRecordCount() + "";
            this.AddOrUpdateReturnParamActual("Record Count", count);
        }

        int GetRecordCount()
        {
            string SQL = GetInputParamCalculatedValue("SQL");
            if (string.IsNullOrEmpty(SQL))
            {
                Error = "GetRecordCount missing SQL: " + Environment.NewLine + SQL + Environment.NewLine + "Error = Missing Query";
            }
            int count = DB.GetRecordCount(SQL);
            return count;
        }

        private void UpdateSqlHndler()
        {
            string SQL = string.Empty;
            try
            {
                if (QueryTypeRadioButton == ActDBValidation.eQueryType.SqlFile.ToString())
                {
                    // FIXME !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    string filePath = "aa";// Amdocs.Ginger.Repository.SolutionRepository.ConvertSolutionRelativePath(QueryFile);  

                    FileInfo scriptFile = new FileInfo(filePath);
                   SQL = scriptFile.OpenText().ReadToEnd();
                }
                else
                {
                    SQL = GetInputParamCalculatedValue("SQL");
                }

                if (string.IsNullOrEmpty(SQL))
                    Error = "Fail to run Update SQL: " + Environment.NewLine + SQL + Environment.NewLine + "Error = Missing Query";
               
                string val = DB.UpdateDB(SQL, CommitDB_Value);
                    this.AddOrUpdateReturnParamActual("Impacted Lines", val);
                
            }
            catch (Exception e)
            {
                if (string.IsNullOrEmpty(Error))
                    this.Error = "Fail to run Update SQL: " + Environment.NewLine + SQL + Environment.NewLine + "Error= " + e.Message;
                
            }
        }
        private void SimpleSQLOneValueHandler()
        {
            string val = GetSingleValue();
            this.AddOrUpdateReturnParamActual(Column , val);
        }

        string GetSingleValue()
        {
            if (string.IsNullOrEmpty(Where))
            {
                Where = "rownum<2";
            }
            string val = DB.GetSingleValue(Table, Column, Where);
            return val;
        }

        private void FreeSQLHandler()
        {
            int? queryTimeout =  Timeout;
            string SQL = string.Empty;
            string ErrorString = string.Empty;
            try
            {
                if (QueryTypeRadioButton == ActDBValidation.eQueryType.SqlFile.ToString())
                {
                    // FIXME: !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    string filePath = "aa";// Amdocs.Ginger.Repository.SolutionRepository.ConvertSolutionRelativePath(GetInputParamValue(ActDBValidation.Fields.QueryFile));

                    FileInfo scriptFile = new FileInfo(filePath);
                    SQL = scriptFile.OpenText().ReadToEnd();
                }
                else
                {
                    SQL = GetInputParamCalculatedValue("SQL");                
                }
                if (string.IsNullOrEmpty(SQL))
                    this.Error = "Fail to run Free SQL: " + Environment.NewLine + SQL + Environment.NewLine + "Error= Missing SQL Query.";

                updateQueryParams();
                foreach (ActInputValue param in QueryParams)
                    SQL = SQL.Replace("<<" + param.ItemName + ">>", param.ValueForDriver);

                DataTable DBResponse = DB.QueryDatabase(SQL, queryTimeout);

                int row = 0;
                int col = 0;
                foreach (DataRow dataRow in DBResponse.Rows)
                {
                    row++;
                    foreach(object obj in dataRow.ItemArray)
                    {
                        col++;
                        this.AddOrUpdateReturnParamActualWithPath("p1", obj.ToString(), row + "/" + col);
                    }
                }

                //List<string> headers=( List<string>) DBResponse.ElementAt(0);
                //List<List<string>> Records = (List<List<string>>)DBResponse.ElementAt (1);


                //if (Records.Count == 0) return;
            
                //int recordcount = Records.Count;
                //for (int j = 0; j < Records.Count; j++)
                   
                //{   List<string> currentrow = Records.ElementAt(j);


                //for (int k = 0; k < currentrow.Count; k++)
                //    {
                //        if (recordcount == 1 )
                //        {
                //            this.AddOrUpdateReturnParamActual (headers.ElementAt (k), currentrow.ElementAt (k));
                //        }
                //        else
                //        {
                //            // Add the record number in the path col
                //            this.AddOrUpdateReturnParamActualWithPath (headers.ElementAt (k), currentrow.ElementAt (k), (j+1).ToString() + "");
                //        }
                //    }   
                //}
                
            }
            catch (Exception e)
            {
                if (string.IsNullOrEmpty(ErrorString))
                    this.Error = "Fail to run Free SQL: " + Environment.NewLine + SQL + Environment.NewLine + "Error= " + e.Message;  
                else
                    this.Error = "Fail to execute query: " + Environment.NewLine + SQL + Environment.NewLine + "Error= " + ErrorString;

                if (e.Message.ToUpper().Contains("COULD NOT LOAD FILE OR ASSEMBLY 'ORACLE.MANAGEDDATAACCESS"))
                {
                    string message = Database.GetMissingDLLErrorDescription();
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

        public DataTable GetResultView()
        {
            DataTable DBResponse = null;
            bool b = SetDBConnection();
            if (!b)
            {
                throw new Exception("GetResultView " + Error);                
            }
            // DB.Connect();
            switch (DBValidationType)
            {
                case eDBValidationType.FreeSQL:                    
                    DBResponse = DB.QueryDatabase(SQL, 1000);                    
                    break;
                case eDBValidationType.SimpleSQLOneValue:
                    string value = GetSingleValue();
                    DBResponse = new DataTable();
                    DBResponse.Columns.Add("Result");
                    DBResponse.Rows.Add(new string[] { value });                    
                    break;
                case eDBValidationType.RecordCount:
                    string count = GetRecordCount() + "";
                    DBResponse = new DataTable();
                    DBResponse.Columns.Add("Count");
                    DBResponse.Rows.Add(new string[] { count });
                    break;
                case eDBValidationType.UpdateDB:
                    string rc = DB.UpdateDB(SQL, false); //  .FreeSQL(SQL, 1000);
                    // TODO: fix me
                    break;
            }

            return DBResponse;

        }       
    }
}
