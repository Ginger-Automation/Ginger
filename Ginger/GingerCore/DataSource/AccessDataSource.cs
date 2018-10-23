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

using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
using System.Reflection;

namespace GingerCore.DataSource
{
    public class AccessDataSource : DataSourceBase
    {
        OleDbConnection myAccessConn;
        string mFilePath = "";

        public override void Init(string sFilePath,string sMode="Read")
        {
            string strAccessConn = "";
            mFilePath = sFilePath;

            if (sMode == "Read")
                strAccessConn = "Provider=Microsoft.Jet.OLEDB.4.0;Mode=" + sMode + ";Data Source=" + mFilePath;
            else
                strAccessConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + mFilePath;

            try
            {
                  myAccessConn = new OleDbConnection(strAccessConn);
                  DSC = this;
                  myAccessConn.Open();                
            }
            catch(Exception ex)
            {
                  Console.WriteLine("Error: Failed to create a database connection. \n{0}", ex.Message);
                  return;
            }

           

        }
        public override void Close()
        {
            try
            {
                if (myAccessConn.State == System.Data.ConnectionState.Open)
                     myAccessConn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Failed to close the database connection. \n{0}", ex.Message);
                return;
            }
        }

        public override ObservableList<DataSourceTable> GetTablesList()
        {         
            
            ObservableList<DataSourceTable> mDataSourceTableDetails = new ObservableList<DataSourceTable>();
            try
            {
                DataTable dt = myAccessConn.GetSchema("Tables");
                foreach (DataRow row in dt.Rows)
                {
                    string tablename = (string)row[2];
                    if (row["TABLE_TYPE"].ToString() == "TABLE")
                    {
                        string strAccessSelect = "SELECT  * FROM " + tablename ;
                        OleDbCommand myAccessCommand = new OleDbCommand(strAccessSelect, myAccessConn);
                        OleDbDataAdapter myDataAdapter = new OleDbDataAdapter();
                        myDataAdapter.SelectCommand = myAccessCommand;
                        DataTable dtTable = new DataTable();
                        myDataAdapter.Fill(dtTable);
                        dtTable.TableName = tablename;
                        mDataSourceTableDetails.Add(CheckDSTableDesign(dtTable));
                    }                    
                }               
                return mDataSourceTableDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Failed to retrieve the required data from the DataBase.\n{0}", ex.Message);
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
                myAccessConn.Close();
                Init(mFilePath,"Write");
                myCommand.CommandText = "ALTER TABLE " + tablename + " ADD COLUMN [GINGER_ID] AUTOINCREMENT";
                myCommand.Connection = myAccessConn;
                myCommand.ExecuteNonQuery();
                myAccessConn.Close();
                Init(mFilePath);
            }
            sTableDetail.DSC = this;
            return sTableDetail;            
        }

        public override void AddColumn(string tableName, string columnName, string columnType)
        {
            myAccessConn.Close();
            Init(mFilePath, "Write");
            OleDbCommand myCommand = new OleDbCommand();
            myCommand.Connection = myAccessConn;
            myCommand.CommandText = "ALTER TABLE " + tableName + " ADD COLUMN ["+ columnName + "] " + columnType;
            myCommand.ExecuteNonQuery();
            myAccessConn.Close();
            Init(mFilePath, "Read");
        }

        public override void RemoveColumn(string tableName, string columnName)
        {
            myAccessConn.Close();
            Init(mFilePath, "Write");
            OleDbCommand myCommand = new OleDbCommand();
            myCommand.Connection = myAccessConn;
            myCommand.CommandText = "ALTER TABLE " + tableName + " DROP COLUMN [" + columnName + "]";
            myCommand.ExecuteNonQuery();
            myAccessConn.Close();
            Init(mFilePath, "Read");
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
            string strAccessSelect = "SELECT * FROM " + tableName;
            OleDbCommand myAccessCommand = new OleDbCommand(strAccessSelect, myAccessConn);
            OleDbDataAdapter myDataAdapter = new OleDbDataAdapter(myAccessCommand);
            DataSet myDataSet = new DataSet();
            myDataAdapter.Fill(myDataSet, tableName);
            
            foreach (DataColumn column in myDataSet.Tables[0].Columns)
            {
                mColumnNames.Add(column.ToString());                
            }
            return mColumnNames;
        }

        public override DataTable GetQueryOutput(string query)
        {            
            OleDbCommand myAccessCommand = new OleDbCommand(query, myAccessConn);
            OleDbDataAdapter myDataAdapterTest = new OleDbDataAdapter(myAccessCommand);

            myDataAdapterTest.AcceptChangesDuringUpdate = true;

            DataTable dataTable = new DataTable();
            myDataAdapterTest.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            myDataAdapterTest.FillSchema(dataTable,SchemaType.Mapped);
            myDataAdapterTest.Fill(dataTable);
            return dataTable;
        }

        public override void RunQuery(string query)
        {
            myAccessConn.Close();
            Init(mFilePath, "Write");
            OleDbCommand myCommand = new OleDbCommand();
            myCommand.Connection = myAccessConn;
            myCommand.CommandText = query;
            myCommand.ExecuteNonQuery();
            myAccessConn.Close();
            Init(mFilePath, "Read");
        }

        public override void AddTable(string TableName,string columnlist="")
        {
            myAccessConn.Close();
            Init(mFilePath, "Write");
            OleDbCommand myCommand = new OleDbCommand();
            myCommand.Connection = myAccessConn;
            myCommand.CommandText = "CREATE TABLE " + TableName + "(" + columnlist + ")";
            myCommand.ExecuteNonQuery();
            myAccessConn.Close();
            Init(mFilePath, "Read");
        }

        public override bool ExporttoExcel(string TableName,string sExcelPath, String sSheetName)
        {
            myAccessConn.Close();
            Init(mFilePath, "Read");
            DataTable dsTable = GetQueryOutput("select * from " + TableName);
            ExportDSToExcel(dsTable, sExcelPath, sSheetName);
            return false;
        }

        public override bool IsTableExist(string TableName)
        {
            DataTable dt = myAccessConn.GetSchema("Tables");
            foreach (DataRow row in dt.Rows)
            {
                if (TableName == (string)row[2])
                    return true;
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
                //DataSet TableDataSet = GetQueryOutput("Select * rom ");
                myAccessConn.Close();
                Init(mFilePath, "Write");
                OleDbCommand myCommand = new OleDbCommand();
                myCommand.Connection = myAccessConn;
                myCommand.CommandText = "SELECT * INTO " + CopyTableName + " FROM " + tableName;
                myCommand.ExecuteNonQuery();
                myAccessConn.Close();
                Init(mFilePath, "Read");
            }
            return CopyTableName;
        }
        public override void RenameTable(string TableName, string NewTableName)
        {
            if(TableName != NewTableName)
            {
                myAccessConn.Close();
                Init(mFilePath, "Write");
                OleDbCommand myCommand = new OleDbCommand();
                myCommand.Connection = myAccessConn;
                myCommand.CommandText = "SELECT * INTO " + NewTableName + " FROM " + TableName;
                myCommand.ExecuteNonQuery();
                myAccessConn.Close();
                Init(mFilePath, "Read");
                DeleteTable(TableName);
            }            
        }
        
        public override void DeleteTable(string TableName)
        {
            try
            {
                myAccessConn.Close();
                Init(mFilePath, "Write");
                OleDbCommand myCommand = new OleDbCommand();
                myCommand.Connection = myAccessConn;
                myCommand.CommandText = "DROP TABLE " + TableName ;
                myCommand.ExecuteNonQuery();
                myAccessConn.Close();
                Init(mFilePath, "Read");
            }
            catch (Exception e)
            {
                Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);
            }
        }
             
        public override void SaveTable(DataTable dataTable)
        {
            try
            {
                DataTable dtChange = dataTable.GetChanges();
                if (dtChange == null)
                    return;
                myAccessConn.Close();
                Init(mFilePath, "Write");

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
                                    updateCommand = updateCommand + row.Table.Columns[iRow] + "='" + row.ItemArray[iRow].ToString().Replace("'", "''") + "',";
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
                            Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}");
                            //Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);
                        }
                    }
                }
                dataTable.AcceptChanges();
                myAccessConn.Close();
                Init(mFilePath);
            }
            catch (Exception e)
            {
                dataTable.RejectChanges();
                Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);
            }
        }
        private void ExportDSToExcel(DataTable table, string sFilePath,string sSheetName="")
        {
            SpreadsheetDocument workbook;

            if (sSheetName == "")
                sSheetName = table.TableName;

            if (File.Exists(sFilePath))
                workbook = SpreadsheetDocument.Open(sFilePath, true);
            else
            {
                workbook = SpreadsheetDocument.Create(sFilePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
                workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();
                workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();
            }

            uint sheetId = 1;
                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                    sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                    DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                    DocumentFormat.OpenXml.Spreadsheet.Sheet oSheet = sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Where(s => s.Name == sSheetName).FirstOrDefault();
                    if (oSheet != null)
                        oSheet.Remove();

                    if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                    {
                        sheetId =
                            sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }

                    DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = sSheetName };
                    sheets.Append(sheet);
                    
                    DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                    List<string> columns = new List<string>();
                    foreach (DataColumn column in table.Columns)
                    {
                        columns.Add(column.ColumnName);

                        DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                        cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                        cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(column.ColumnName);
                        headerRow.AppendChild(cell);
                    }

                    sheetData.AppendChild(headerRow);

                    foreach (DataRow dsrow in table.Rows)
                    {
                        DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                        foreach (String col in columns)
                        {
                            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(dsrow[col].ToString()); //
                            newRow.AppendChild(cell);
                        }

                        sheetData.AppendChild(newRow);
                    }
            workbook.Close();
        }
    }
}
