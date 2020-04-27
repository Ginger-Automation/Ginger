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

using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
using System.Reflection;
using Amdocs.Ginger.Repository;
using System.Runtime.CompilerServices;

namespace GingerCore.DataSource
{
    public class AccessDataSource : DataSourceBase
    {
        private static readonly Object thisObj = new object();

        private string GetConnectionString(string sMode = "Write")
        {
            string strAccessConn = "";

            if (sMode == "Read")
                strAccessConn = @"Provider=Microsoft.Jet.OLEDB.4.0;Mode=" + sMode + ";Data Source=" + FileFullPath;
            else
                strAccessConn = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + FileFullPath;

            return strAccessConn;
        }

        public override ObservableList<DataSourceTable> GetTablesList()
        {
            ObservableList<DataSourceTable> mDataSourceTableDetails = new ObservableList<DataSourceTable>();
            try
            {
                using (OleDbConnection connObj = new OleDbConnection(GetConnectionString("Read")))
                {
                    if (connObj.State != System.Data.ConnectionState.Open)
                    {
                        connObj.Open();
                    }
                    DataTable dataTable = connObj.GetSchema("Tables");
                    foreach (DataRow row in dataTable.Rows)
                    {
                        string tablename = (string)row[2];
                        if (row["TABLE_TYPE"].ToString() == "TABLE")
                        {
                            string strAccessSelect = "SELECT  * FROM " + tablename;
                            OleDbDataAdapter myDataAdapter = new OleDbDataAdapter(strAccessSelect, connObj);
                            DataTable dtTable = new DataTable();
                            myDataAdapter.Fill(dtTable);
                            dtTable.TableName = tablename;
                            mDataSourceTableDetails.Add(CheckDSTableDesign(dtTable));
                        }
                    }
                }
                return mDataSourceTableDetails;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to retrieve the required tables data from the DataBase", ex);
                return null;
            }
        }

        private DataSourceTable CheckDSTableDesign(DataTable dtTable)
        {
            string tablename = dtTable.TableName;
            DataSourceTable sTableDetail = new DataSourceTable();
            sTableDetail.Name = tablename;

            int iCount = 0;
            int iIdCount = 0;
            int iUpdateCount = 0;
            foreach (DataColumn column in dtTable.Columns)
            {

                if (column.ToString() == "GINGER_KEY_NAME" || column.ToString() == "GINGER_KEY_VALUE")
                    iCount++;
                else if (column.ToString() == "GINGER_ID")
                    iIdCount++;
                else if (column.ToString() == "GINGER_LAST_UPDATE_DATETIME" || column.ToString() == "GINGER_LAST_UPDATED_BY")
                    iUpdateCount++;
            }
            if (iCount == 2 && dtTable.Columns.Count == 2 + iIdCount + iUpdateCount)
                sTableDetail.DSTableType = DataSourceTable.eDSTableType.GingerKeyValue;
            else
                sTableDetail.DSTableType = DataSourceTable.eDSTableType.Customized;

            OleDbCommand myCommand = new OleDbCommand();
            if (iIdCount == 0)
            {
                var query = "ALTER TABLE " + tablename + " ADD COLUMN [GINGER_ID] AUTOINCREMENT";
                RunQuery(query);
            }
            sTableDetail.DSC = this;
            return sTableDetail;            
        }

        public override void AddColumn(string tableName, string columnName, string columnType)
        {
            var query = "ALTER TABLE " + tableName + " ADD COLUMN [" + columnName + "] " + columnType;
            RunQuery(query);
        }

        public override void RemoveColumn(string tableName, string columnName)
        {
            var query = "ALTER TABLE " + tableName + " DROP COLUMN [" + columnName + "]";
            RunQuery(query);
        }

        public override void UpdateTableList(ObservableList<DataSourceTable> dsTableList)
        {
            ObservableList<DataSourceTable> dsTableListLatest = GetTablesList();
            foreach (DataSourceTable dsTableLatest in dsTableListLatest)
            {
                bool sTblFound = false;
                foreach (DataSourceTable dsTable in dsTableList)
                    if (dsTable.Name == dsTableLatest.Name)
                        sTblFound = true;
                if(sTblFound == false)
                      DeleteTable(dsTableLatest.Name);
            }
        }

        public override List<string> GetColumnList(string tableName)
        {
            List<string> mColumnNames = new List<string>();
            try
            {
                using (OleDbConnection connObj = new OleDbConnection(GetConnectionString("Read")))
                {
                    if (connObj.State != System.Data.ConnectionState.Open)
                    {
                        connObj.Open();
                    }
                    string strAccessSelect = "SELECT * FROM " + tableName;
                    OleDbDataAdapter myDataAdapter = new OleDbDataAdapter(strAccessSelect, connObj);
                    DataSet myDataSet = new DataSet();
                    myDataAdapter.Fill(myDataSet, tableName);

                    foreach (DataColumn column in myDataSet.Tables[0].Columns)
                    {
                        mColumnNames.Add(column.ToString());
                    }
                }
                return mColumnNames;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to retrieve the required columns data from the DataBase", ex);
                return null;
            }
        }

        public override DataTable GetQueryOutput(string query)
        {
            try
            {
                DataTable dataTable = new DataTable();
                using (OleDbConnection connObj = new OleDbConnection(GetConnectionString("Read")))
                {
                    if (connObj.State != System.Data.ConnectionState.Open)
                    {
                        connObj.Open();
                    }
                    OleDbDataAdapter myDataAdapterTest = new OleDbDataAdapter(query, connObj);
                    myDataAdapterTest.AcceptChangesDuringUpdate = true;
                    myDataAdapterTest.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                    myDataAdapterTest.Fill(dataTable);
                }
                return dataTable;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to retrieve the required data from the DataBase", ex);
                return null;
            }

        }

        public override bool RunQuery(string query)
        {
            try
            {
                using (OleDbConnection connObj = new OleDbConnection(GetConnectionString("Write")))
                {
                    if (connObj.State != System.Data.ConnectionState.Open)
                    {
                        connObj.Open();
                    }
                    OleDbCommand myCommand = new OleDbCommand();
                    myCommand.Connection = connObj;
                    myCommand.CommandText = query;
                    myCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Execute Query", ex);
                return false;
            }

            return true;
        }

        public override void AddTable(string TableName,string columnlist="")
        {
            var query = "CREATE TABLE " + TableName + "(" + columnlist + ")";
            RunQuery(query);
        }

        public override bool ExporttoExcel(string TableName,string sExcelPath, String sSheetName)
        {
            DataTable dsTable = GetQueryOutput("select * from " + TableName);
            bool result;
            lock(thisObj)
            {
                result = ExportDSToExcel(dsTable, sExcelPath, sSheetName);
            }
            return result;
        }

        public override bool IsTableExist(string TableName)
        {
            using (OleDbConnection connObj = new OleDbConnection(GetConnectionString("Write")))
            {
                if (connObj.State == ConnectionState.Closed)
                {
                    connObj.Open();
                }
                DataTable dt = connObj.GetSchema("Tables");
                foreach (DataRow row in dt.Rows)
                {
                    if (TableName == (string)row[2])
                        return true;
                }
            }
            return false;
        }
        public override string CopyTable(string tableName)
        {
            string CopyTableName = tableName;

            while (IsTableExist(CopyTableName))
                CopyTableName = CopyTableName + "_Copy";

            if (CopyTableName != tableName)
            {
                var query = "SELECT * INTO " + CopyTableName + " FROM " + tableName;
                RunQuery(query);
            }
            return CopyTableName;
        }
        public override void RenameTable(string TableName, string NewTableName)
        {
            if(!TableName.Equals(NewTableName, StringComparison.OrdinalIgnoreCase))
            {
                var query = "SELECT * INTO " + NewTableName + " FROM " + TableName;
                if (RunQuery(query))
                {
                    DeleteTable(TableName);     // AccessDB does not support renaming table using alter query so we copy data to new table and delete old
                }
            }
        }
        
        public override void DeleteTable(string TableName)
        {
            var query = "DROP TABLE " + TableName;
            RunQuery(query);
        }
             
        public override void SaveTable(DataTable dataTable)
        {
            try
            {
                DataTable dtChange = dataTable.GetChanges();
                if (dtChange == null)
                    return;

                foreach (DataRow row in dataTable.Rows)
                {
                    if (row.RowState == DataRowState.Modified)
                    {
                        string updateCommand = "UPDATE " + row.Table.TableName + " SET ";
                        for (int iRow = 0; iRow < row.ItemArray.Count(); iRow++)
                            if (row.Table.Columns[iRow].ColumnName != "GINGER_ID")
                            {
                                if (row.Table.Columns[iRow].ColumnName == "GINGER_LAST_UPDATED_BY")
                                    updateCommand = updateCommand + row.Table.Columns[iRow] + "='" + System.Environment.UserName + "',";
                                else if (row.Table.Columns[iRow].ColumnName == "GINGER_LAST_UPDATE_DATETIME")
                                    updateCommand = updateCommand + row.Table.Columns[iRow] + "='" + DateTime.Now.ToString() + "',";
                                else
                                    updateCommand = updateCommand  + "["+ row.Table.Columns[iRow] + "] ='" + row.ItemArray[iRow].ToString().Replace("'", "''") + "',";
                            }
                        updateCommand = updateCommand.Substring(0, updateCommand.Length - 1);
                        updateCommand = updateCommand + " where GINGER_ID = " + row["GINGER_ID", DataRowVersion.Original];
                        RunQuery(updateCommand);
                    }
                    else if (row.RowState == DataRowState.Added)
                    {
                        string insertCommand = "INSERT INTO " + row.Table.TableName + " (";
                        for (int iRow = 0; iRow < row.ItemArray.Count(); iRow++)
                            if (row.Table.Columns[iRow].ColumnName != "GINGER_ID")
                                insertCommand = insertCommand + "[" + row.Table.Columns[iRow].ColumnName + "],";
                        insertCommand = insertCommand.Substring(0, insertCommand.Length - 1) + ") VALUES (";
                        for (int iRow = 0; iRow < row.ItemArray.Count(); iRow++)
                            if (row.Table.Columns[iRow].ColumnName != "GINGER_ID")
                            {
                                if (row.Table.Columns[iRow].ColumnName == "GINGER_LAST_UPDATED_BY")
                                    insertCommand = insertCommand + "'" + System.Environment.UserName + "',";
                                else if (row.Table.Columns[iRow].ColumnName == "GINGER_LAST_UPDATE_DATETIME")
                                    insertCommand = insertCommand + "'" + DateTime.Now.ToString() + "',";
                                else
                                    insertCommand = insertCommand + "'" + row.ItemArray[iRow].ToString().Replace("'","''") + "',";
                            }
                        insertCommand = insertCommand.Substring(0, insertCommand.Length - 1) + ");";

                        RunQuery(insertCommand);
                    }
                    else if (row.RowState == DataRowState.Deleted)
                    {
                        //row.AcceptChanges();                        
                        string deleteCommand = "DELETE from  " + row.Table.TableName;
                        deleteCommand = deleteCommand + " where GINGER_ID = " + row["GINGER_ID", DataRowVersion.Original];
                        //myDataAdapterTest.DeleteCommand = new OleDbCommand(deleteCommand);
                        try
                        {
                            RunQuery(deleteCommand);
                        }
                        catch (Exception e)
                        {
                            dataTable.RejectChanges();
                            Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
                            //Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);
                        }
                    }
                }
                dataTable.AcceptChanges();
            }
            catch (Exception e)
            {
                dataTable.RejectChanges();
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);
            }
        }
        private bool ExportDSToExcel(DataTable table, string sFilePath, string sSheetName)
        {
            sFilePath = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(sFilePath);
            return GingerCoreNET.GeneralLib.General.ExportToExcel(table, sFilePath, sSheetName);
        }

        
        public override DataTable GetTable(string TableName)
        {
            return GetQueryOutput("Select * from " + TableName);
        }

        public override void AddRow( List<string> mColumnNames, DataSourceTable mDSTableDetails)
        {
            DataRow dr = mDSTableDetails.DataTable.NewRow();
            mColumnNames = mDSTableDetails.DSC.GetColumnList(mDSTableDetails.Name);
            foreach (string sColName in mColumnNames)
            {
                if (sColName != "GINGER_ID" && sColName != "GINGER_LAST_UPDATED_BY" && sColName != "GINGER_LAST_UPDATE_DATETIME")
                {
                        dr[sColName] = "";
                }
                else if (sColName == "GINGER_ID")
                {
                        dr[sColName] = System.DBNull.Value;
                }
            }
            mDSTableDetails.DataTable.Rows.Add(dr);
        }

        public override void DuplicateRow(List<string> mColumnNames, List<object> SelectedItemsList, DataSourceTable mDSTableDetails)
        {
            mColumnNames = mDSTableDetails.DSC.GetColumnList(mDSTableDetails.Name);
            foreach (object o in SelectedItemsList)
            {
                DataRow row = (((DataRowView)o).Row);
                DataRow dr = mDSTableDetails.DataTable.NewRow();
                foreach (string sColName in mColumnNames)
                {
                    if (sColName != "GINGER_ID" && sColName != "GINGER_LAST_UPDATED_BY" && sColName != "GINGER_LAST_UPDATE_DATETIME")
                    {
                        dr[sColName] = row[sColName];
                    }
                    else
                    {
                        dr[sColName] = System.DBNull.Value;
                    }
                }
                mDSTableDetails.DataTable.Rows.Add(dr);
            }
        }

        public override void InitConnection()
        {
            if (FilePath != null)
            {
                FileFullPath = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(FilePath);
            }
        }

        public override string AddNewCustomizedTableQuery()
        {
            return "[GINGER_ID] AUTOINCREMENT,[GINGER_USED] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text";
        }

        public override int GetRowCount(string TableName)
        {
            return GetQueryOutput("Select * from " + TableName).Rows.Count;
        }

        public override string AddNewKeyValueTableQuery()
        {
            return "[GINGER_ID] AUTOINCREMENT,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text";
        }

        public override string GetExtension()
        {
            return ".mdb";
        }

        public override DataTable GetKeyName(string mDSTableName)
        {
            return GetQueryOutput("Select GINGER_KEY_NAME from " + mDSTableName + " WHERE GINGER_KEY_NAME is not null and Trim(GINGER_KEY_NAME) <> ''");
        }
        
        public override void DeleteAll(List<object> AllItemsList, string TName = null)
        {
            foreach (object o in AllItemsList)
            {
                ((DataRowView)o).Delete();
            }
        }

        public override string AddColumnName(string colName)
        {
            return string.Format("[{0}] Text,", colName);
        }

        public override string UpdateDSReturnValues(string Name, string sColList, string sColVals)
        {
            return "INSERT INTO " + Name + "(" + sColList + "GINGER_LAST_UPDATED_BY,GINGER_LAST_UPDATE_DATETIME) VALUES (" + sColVals + "'" + System.Environment.UserName + "','" + DateTime.Now.ToString() + "')";
        }
    }
}
