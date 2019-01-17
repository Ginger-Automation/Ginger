using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;
using System.Data.SQLite;
using System.Data.SqlClient;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
using System.Reflection;

namespace GingerCore.DataSource
{
    public class SQLiteDataSource : DataSourceBase
    {
        string mFilePath = "";
        private SQLiteConnection sqlite;

        public override void Init(string sFilePath, string sMode = "Read")
        {
            string strAccessConn = "";
            mFilePath = sFilePath;

            //strAccessConn = "Data Source= database.db; Version = 3; New = True; Compress = True";

            if (sMode == "Read")
            {
                strAccessConn = "Data Source=" + mFilePath ;
            }
            else
            {
                strAccessConn = "Data Source=" + mFilePath ;
            }

            try
            {
                if (!String.IsNullOrEmpty(mFilePath))
                {
                    using (sqlite = new SQLiteConnection(strAccessConn))
                    {
                        sqlite = new SQLiteConnection(strAccessConn);
                        DSC = this;
                        sqlite.Open();
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Failed to create a database connection. \n{0}", ex.Message);
                return;
            }
        }
        public override void AddColumn(string tableName, string columnName, string columnType)
        {
            sqlite.Close();
            Init(mFilePath, "Write");
            SQLiteCommand myCommand = new SQLiteCommand();
            myCommand.Connection = sqlite;
            myCommand.CommandText = "ALTER TABLE " + tableName + " ADD COLUMN [" + columnName + "] " + columnType;
            myCommand.ExecuteNonQuery();
            sqlite.Close();
            Init(mFilePath, "Read");
        }

        public override void AddTable(string tableName, string columnList = "")
        {
            sqlite.Close();
            Init(mFilePath, "Write");
            SQLiteCommand myCommand = new SQLiteCommand();
            myCommand.Connection = sqlite;
            myCommand.CommandText = "CREATE TABLE " + tableName + "(" + columnList + ")";
            myCommand.ExecuteNonQuery();
            sqlite.Close();
            Init(mFilePath, "Read");
        }

        public override void Close()
        {
            try
            {
                if (sqlite.State == System.Data.ConnectionState.Open)
                    sqlite.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Failed to close the database connection. \n{0}", ex.Message);
                return;
            }
        }

        public override string CopyTable(string tableName)
        {
            string CopyTableName = tableName;

            while (IsTableExist(CopyTableName))
                CopyTableName = CopyTableName + "_Copy";

            if (CopyTableName != tableName)
            {
                //DataSet TableDataSet = GetQueryOutput("Select * rom ");
                sqlite.Close();
                Init(mFilePath, "Write");
                SQLiteCommand myCommand = new SQLiteCommand();
                myCommand.Connection = sqlite;
                myCommand.CommandText = "SELECT * INTO " + CopyTableName + " FROM " + tableName;
                myCommand.ExecuteNonQuery();
                sqlite.Close();
                Init(mFilePath, "Read");
            }
            return CopyTableName;
        }

        public override void DeleteTable(string tableName)
        {
            try
            {
                sqlite.Close();
                Init(mFilePath, "Write");
                SQLiteCommand myCommand = new SQLiteCommand();
                myCommand.Connection = sqlite;
                myCommand.CommandText = "DROP TABLE " + tableName;
                myCommand.ExecuteNonQuery();
                sqlite.Close();
                Init(mFilePath, "Read");
            }
            catch (Exception e)
            {
                Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);
            }
        }

        public override bool ExporttoExcel(string TableName, string sExcelPath, string sSheetName)
        {
            sqlite.Close();
            Init(mFilePath, "Read");
            DataTable dsTable = GetQueryOutput("select * from " + TableName);
            ExportDSToExcel(dsTable, sExcelPath, sSheetName);
            return false;
        }
        private void ExportDSToExcel(DataTable table, string sFilePath, string sSheetName = "")
        {
            SpreadsheetDocument workbook;

            if (sSheetName == "")
                sSheetName = table.TableName;

            if (File.Exists(sFilePath))
                workbook = SpreadsheetDocument.Open(sFilePath, true);
            else
            {
                workbook = SpreadsheetDocument.Create(sFilePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
                workbook.AddWorkbookPart();
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
    
    public override List<string> GetColumnList(string tableName)
        {
            List<string> mColumnNames = new List<string>();
            string strAccessSelect = "SELECT * FROM " + tableName;
            SQLiteCommand myAccessCommand = new SQLiteCommand(strAccessSelect, sqlite);
            SQLiteDataAdapter myDataAdapter = new SQLiteDataAdapter(myAccessCommand);
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
            var table = new DataTable();
            SQLiteCommand myAccessCommand = new SQLiteCommand(query, sqlite);
            
            table.Load(myAccessCommand.ExecuteReader());
            return table;
        }

        public override ObservableList<DataSourceTable> GetTablesList()
        {
            ObservableList<DataSourceTable> mDataSourceTableDetails = new ObservableList<DataSourceTable>();
            try
            {
                DataTable dt = sqlite.GetSchema("Tables");
                
                foreach (DataRow row in dt.Rows)
                {
                    string tablename = (string)row[2];
                    if (row["TABLE_TYPE"].ToString() == "table")
                    {
                        string strAccessSelect = "SELECT  * FROM " + tablename;
                        SQLiteCommand myAccessCommand = new SQLiteCommand(strAccessSelect);
                        myAccessCommand.Connection = sqlite;
                        
                        SQLiteDataAdapter myDataAdapter = new SQLiteDataAdapter(myAccessCommand);
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

            SQLiteCommand myCommand = new SQLiteCommand();
            if (iIdCount == 0)
            {
                sqlite.Close();
                Init(mFilePath, "Write");
                myCommand.CommandText = "ALTER TABLE " + tablename + " ADD COLUMN [GINGER_ID] AUTOINCREMENT";
                myCommand.Connection = sqlite;
                myCommand.ExecuteNonQuery();
                sqlite.Close();
                Init(mFilePath);
            }
            sTableDetail.DSC = this;
            return sTableDetail;
        }

        public override bool IsTableExist(string tableName)
        {
            DataTable dt = sqlite.GetSchema("Tables");
            foreach (DataRow row in dt.Rows)
            {
                if (tableName == (string)row[2])
                    return true;
            }
            return false;
        }

        public override void RemoveColumn(string tableName, string columnName)
        {
            sqlite.Close();
            Init(mFilePath, "Write");
            SQLiteCommand myCommand = new SQLiteCommand();
            List<string> listCol = GetColumnList(tableName);
            listCol.Remove(columnName);
            string cols= String.Join(",", listCol.ToArray());
           
            myCommand.Connection = sqlite;
            string cmd = "CREATE TABLE t1_backup AS SELECT "+cols+" FROM " + tableName +";" + "DROP TABLE " + tableName +";"+ "ALTER TABLE t1_backup RENAME TO " +tableName+";" ;
            myCommand.CommandText = cmd;
            myCommand.ExecuteNonQuery();
            sqlite.Close();
            Init(mFilePath, "Read");
        }

        public override void RenameTable(string tableName, string newTableName)
        {
            if (tableName != newTableName)
            {
                sqlite.Close();
                Init(mFilePath, "Write");
                SQLiteCommand myCommand = new SQLiteCommand();
                myCommand.Connection = sqlite;
                myCommand.CommandText = "SELECT * INTO " + newTableName + " FROM " + tableName;
                myCommand.ExecuteNonQuery();
                sqlite.Close();
                Init(mFilePath, "Read");
                DeleteTable(tableName);
            }
        }

        public override void RunQuery(string query)
        {
            sqlite.Close();
            Init(mFilePath, "Write");
            SQLiteCommand myCommand = new SQLiteCommand();
            myCommand.Connection = sqlite;
            myCommand.CommandText = query;
            myCommand.ExecuteNonQuery();
            sqlite.Close();
            Init(mFilePath, "Read");
        }

        public override void SaveTable(DataTable dataTable)
        {
            try
            {
                DataTable dtChange = dataTable.GetChanges();
                if (dtChange == null)
                    return;
                sqlite.Close();
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
                                    updateCommand = updateCommand + "[" + row.Table.Columns[iRow] + "] ='" + row.ItemArray[iRow].ToString().Replace("'", "''") + "',";
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
                                    insertCommand = insertCommand + "'" + row.ItemArray[iRow].ToString().Replace("'", "''") + "',";
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
                            //Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);
                        }
                    }
                }
                dataTable.AcceptChanges();
                sqlite.Close();
                Init(mFilePath);
            }
            catch (Exception e)
            {
                dataTable.RejectChanges();
                Reporter.ToUser(eUserMsgKeys.GeneralErrorOccured, e.Message + Environment.NewLine + e.InnerException);
            }
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
                if (sTblFound == false)
                    DeleteTable(dsTableLatest.Name);
            }
        }
    }
}
