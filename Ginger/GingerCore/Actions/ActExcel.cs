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
using Amdocs.Ginger.Common.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Windows;
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCore.Variables;
using System.IO;
using System.Reflection;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

//TODO: add and use below with ReadCellDataNew - need to be tested
// using DocumentFormat.OpenXml.Packaging;
// using DocumentFormat.OpenXml.Spreadsheet;

namespace GingerCore.Actions
{
    public class ActExcel : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Excel Action"; } }
        public override string ActionUserDescription { get { return "Read/Write Excel"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action in case you need to Read/Write/etc. excel sheet from/on a system drives.");
            TBH.AddLineBreak();
            TBH.AddText("This action contains list of options which will allow you to read/write excel file and also read excel rows with Where conditions.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("Read Excel :- If you want to read excel sheet from system then select Read data from Excel Action type dropdown, Then browse the file by clicking Browse button.Once you "+
             "browse the file then all the sheets will get bind to Sheet Name dropdown,Select sheet name and click on view button.If you want to put where condition on excel sheet then type column name and it's value e.g. ColName=1 for integer column and ColName='colname' for string column");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("Write Excel :- Similar as read action,to write an excel, select Write data from Excel Action type dropdown, Then browse the file by clicking Browse button,select sheet name and then add where condition on column which you want to write, Then add primary key column if you have any.Then finally to write an excel add column name =Variable name in Variable to col textbox and run the action");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("Note:- Column name should not be the same name as the variable name.");
        }        

        public override string ActionEditPage { get { return "ActExcelEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

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
        public enum eExcelActionType
        {
            [EnumValueDescription("Read Data")]
            ReadData,
            [EnumValueDescription("Write Data")]
            WriteData,
            [EnumValueDescription("Read Cell Data")]
            ReadCellData
        }

        public new static partial class Fields
        {
            public static string ExcelFileName = "ExcelFileName";
            public static string SheetName = "SheetName";
            public static string SelectRowsWhere = "SelectRowsWhere";
            public static string SelectAllRows = "SelectAllRows";
            public static string PrimaryKeyColumn = "PrimaryKeyColumn";
            public static string SetDataUsed = "SetDataUsed";
            public static string ColMappingRules = "ColMappingRules";
            public static string ExcelActionType = "ExcelActionType";
        }

        [IsSerializedForLocalRepository]
        public string ExcelFileName
        {
            get
            {
                return GetInputParamValue("ExcelFileName");
            }
            set
            {
                AddOrUpdateInputParamValue("ExcelFileName", value);
                OnPropertyChanged(nameof(ExcelFileName));
            }
        }

        [IsSerializedForLocalRepository]
        public string SheetName
        {
            get
            {
                return GetInputParamValue("SheetName");
            }
            set
            {
                AddOrUpdateInputParamValue("SheetName", value);
            }
        }

        [IsSerializedForLocalRepository]
        public string SelectRowsWhere
        {
            get
            {
                return GetInputParamValue("SelectRowsWhere");
            }
            set
            {
                AddOrUpdateInputParamValue("SelectRowsWhere", value);
                OnPropertyChanged(nameof(SelectRowsWhere));
            }
        }

        [IsSerializedForLocalRepository]
        public string PrimaryKeyColumn
        {
            get
            {
                return GetInputParamValue("PrimaryKeyColumn");
            }
            set
            {
                AddOrUpdateInputParamValue("PrimaryKeyColumn", value);
                OnPropertyChanged(nameof(PrimaryKeyColumn));
            }
        }

        [IsSerializedForLocalRepository]
        public string SetDataUsed
        {
            get
            {
                return GetInputParamValue("SetDataUsed");
            }
            set
            {
                AddOrUpdateInputParamValue("SetDataUsed", value);
                OnPropertyChanged(nameof(SetDataUsed));
            }
        }
        
        [IsSerializedForLocalRepository]
        public eExcelActionType ExcelActionType { set; get; }

        [IsSerializedForLocalRepository]
        public bool SelectAllRows { set; get; }

        [IsSerializedForLocalRepository]
        public string ColMappingRules
        {
            get
            {
                return GetInputParamValue("ColMappingRules");
            }
            set
            {
                AddOrUpdateInputParamValue("ColMappingRules", value);
            }
        }

        public override string ActionType
        {
            get { return "Excel" + ExcelActionType.ToString(); }
        }

        public override System.Drawing.Image Image { get { return Resources.Excel16x16; } }

         

        public override void Execute()
        {
            switch (ExcelActionType)
            {
                case eExcelActionType.ReadData:
                    ReadData();
                    break;

                case eExcelActionType.WriteData:
                    if (check_file_open(GetExcelFileNameForDriver()))                    
                        WriteData();                    
                    break;
                case eExcelActionType.ReadCellData:
                    ReadCellData();
                    break;

                default:

                    break;
            }
        }

        //TODO: uncommente and check its working fine before replacing with SQL style
        //private void ReadCellDataNew()
        //{               
        //        string SheetName = GetValueForDriverParam("SheetName").Trim();
        //        //if (!SheetName.EndsWith("$")) SheetName += "$";
                
        //        string where = GetValueForDriverParam("SelectRowsWhere");
                
        //        string xlsFile = GetExcelFileNameForDriver();
        //        string s = GetCellValue(xlsFile, SheetName, "C21");

        //        // Add in Act.cs a function UpdateActualResult(s);  // so we will not have hard coded Actual - repalce everywhere
        //        AddOrUpdateInputParam("Actual", s);
            
            
        //}

        //public static string GetCellValue(string fileName, string sheetName, string addressName)
        //{
        //    string value = null;

        //    // Open the spreadsheet document for read-only access.
        //    using (SpreadsheetDocument document =
        //        SpreadsheetDocument.Open(fileName, false))
        //    {
        //        // Retrieve a reference to the workbook part.
        //        WorkbookPart wbPart = document.WorkbookPart;

        //        // Find the sheet with the supplied name, and then use that 
        //        // Sheet object to retrieve a reference to the first worksheet.
        //        Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName).FirstOrDefault();

        //        // Throw an exception if there is no sheet.
        //        if (theSheet == null)
        //        {
        //            throw new ArgumentException("Sheet not found");
        //        }

                

        //        // Retrieve a reference to the worksheet part.
        //        WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));

        //        Cell theCell = wsPart.Worksheet.Descendants<Cell>().Where(c => c.CellReference == addressName).FirstOrDefault();
        //        string s1 = theCell.CellValue.ToString();

        //        // Use its Worksheet property to get a reference to the cell 
        //        // whose address matches the address you supplied.
                

        //        // If the cell does not exist, return an empty string.
        //        if (theCell != null)
        //        {
        //            value = theCell.InnerText;

        //            // If the cell represents an integer number, you are done. 
        //            // For dates, this code returns the serialized value that 
        //            // represents the date. The code handles strings and 
        //            // Booleans individually. For shared strings, the code 
        //            // looks up the corresponding value in the shared string 
        //            // table. For Booleans, the code converts the value into 
        //            // the words TRUE or FALSE.
        //            if (theCell.DataType != null)
        //            {
        //                switch (theCell.DataType.Value)
        //                {
        //                    case CellValues.SharedString:

        //                        // For shared strings, look up the value in the
        //                        // shared strings table.
        //                        var stringTable =
        //                            wbPart.GetPartsOfType<SharedStringTablePart>()
        //                            .FirstOrDefault();

        //                        // If the shared string table is missing, something 
        //                        // is wrong. Return the index that is in
        //                        // the cell. Otherwise, look up the correct text in 
        //                        // the table.
        //                        if (stringTable != null)
        //                        {
        //                            value =
        //                                stringTable.SharedStringTable
        //                                .ElementAt(int.Parse(value)).InnerText;
        //                        }
        //                        break;

        //                    case CellValues.Boolean:
        //                        switch (value)
        //                        {
        //                            case "0":
        //                                value = "FALSE";
        //                                break;
        //                            default:
        //                                value = "TRUE";
        //                                break;
        //                        }
        //                        break;
        //                }
        //            }
        //        }
        //    }
        //    return value;
        //}


        private void ReadCellData()
        {            
            string ConnString = GetConnectionString();
            string sql = "";
            using (OleDbConnection Conn = new OleDbConnection(ConnString))
            {
                Conn.Open();
                OleDbCommand Cmd = new OleDbCommand();
                Cmd.Connection = Conn;

                string SheetName = GetInputParamCalculatedValue("SheetName").Trim();
                if (!SheetName.EndsWith("$")) SheetName += "$";
                
                string where = GetInputParamCalculatedValue("SelectRowsWhere");
                if (!string.IsNullOrEmpty(where))
                {                    
                   //  sql += " WHERE " + where;
                }


                // something like  "SELECT * FROM [MySheet$C11]";  //not working
                // or range "SELECT * FROM [MySheet$A1:C200]";  //working
                // sql = "Select * from [" + SheetName + where + "]";
                sql = "Select * from [" + SheetName + where + "]";

                Cmd.CommandText = sql;
                DataTable dt = new DataTable();

                OleDbDataAdapter da = new OleDbDataAdapter();
                da.SelectCommand = Cmd;
                try
                {
                    da.Fill(dt);
                    if (SelectAllRows == false)
                    {
                        string CellValue = dt.Rows[0][0].ToString();
                        AddOrUpdateReturnParamActual("Actual", CellValue);
                    }
                    else
                    {
                        for (int j=0;j< dt.Rows.Count;j++)
                        {
                            DataRow r = dt.Rows[j];
                            //Read data to return values
                              // in case the user didn't select cols then get all excel output columns
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                AddOrUpdateReturnParamActualWithPath("Actual", ((object)r[i]).ToString(),(j+1).ToString() + (i+1).ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    Error = ex.Message;
                }
                finally
                {
                    Conn.Close();
                }

                if (dt.Rows.Count == 0)
                {
                    this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    Error = "No rows found in excel file matching criteria - " + sql;
                }
            }
        }

        private string GetConnectionString()
        {
            string s = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + GetExcelFileNameForDriver() + ";Extended Properties=\"Excel 12.0 Xml;HDR=YES;\"";
            return s;
        }

        public List<string> GetSheets()
        {
            string ConnString = GetConnectionString();

            using (OleDbConnection Conn = new OleDbConnection(ConnString))
            {
                try
                {
                    Conn.Open();
                    DataTable c = Conn.GetSchema("Tables");
                    // remove the last $ sign = not user friendly
                    List<string> returnList= c.AsEnumerable().Select(r => r.Field<string>("TABLE_NAME").Substring(0,r.Field<string>("TABLE_NAME").Length -1))
                               .ToList();
                    return returnList; 

                }
                catch 
                {
                    return new List<string>();
                }
            }
        }
        public void ReadData()
        {
            //TODO: check what is required on the machine and maybe support for other versions
            string ConnString = GetConnectionString();
            string sql = "";
            using (OleDbConnection Conn = new OleDbConnection(ConnString))
            {
                try
                {
                    Conn.Open();
                }
                catch (Exception ex)
                {
                    System.Threading.Thread.Sleep(3000);
                    Conn.Open();
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.StackTrace}");
                }

                OleDbCommand Cmd = new OleDbCommand();
                Cmd.Connection = Conn;

                string SheetName = GetInputParamCalculatedValue("SheetName").Trim();
                if (!SheetName.EndsWith("$")) SheetName += "$";
                if(SelectAllRows == false)
                    sql = "Select TOP 1 * from [" + SheetName + "]";
                else
                    sql = "Select * from [" + SheetName + "]";

                string where = GetInputParamCalculatedValue("SelectRowsWhere");

                if (!string.IsNullOrEmpty(where))
                {
                    sql += " WHERE " + where;
                }

                Cmd.CommandText = sql;
                DataTable dt = new DataTable();

                OleDbDataAdapter da = new OleDbDataAdapter();
                da.SelectCommand = Cmd;
                try
                {
                    da.Fill(dt);

                    // we expect only 1 record
                    if (dt.Rows.Count == 1 && SelectAllRows == false)
                    {
                        DataRow r = dt.Rows[0];
                        
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            AddOrUpdateReturnParamActual(dt.Columns[i].ColumnName, ((object)r[i]).ToString());
                        }
                        
                        if (!String.IsNullOrEmpty(GetInputParamCalculatedValue("SetDataUsed")))
                        {
                            string rowKey = r[GetInputParamCalculatedValue("PrimaryKeyColumn")].ToString();
                            string updateSQL = @"UPDATE [" + GetInputParamCalculatedValue("SheetName") + "$] SET " + GetInputParamCalculatedValue("SetDataUsed") + " WHERE " + GetInputParamCalculatedValue("PrimaryKeyColumn") + "=" + rowKey + ";";
                            OleDbCommand myCommand = new OleDbCommand();
                            myCommand.Connection = Conn;
                            myCommand.CommandText = updateSQL;
                            myCommand.ExecuteNonQuery();
                        }
                    }
                    else if (dt.Rows.Count > 0 && SelectAllRows == true)
                    {
                        for(int j = 0;j< dt.Rows.Count;j++)
                        {
                            DataRow r = dt.Rows[j];
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                AddOrUpdateReturnParamActualWithPath(dt.Columns[i].ColumnName, ((object)r[i]).ToString(), "" + (j + 1).ToString());                                
                            }
                        }
                        if (!String.IsNullOrEmpty(GetInputParamCalculatedValue("SetDataUsed")))
                        {
                            string updateSQL = @"UPDATE [" + GetInputParamCalculatedValue("SheetName") + "$] SET " + GetInputParamCalculatedValue("SetDataUsed");

                            if (!string.IsNullOrEmpty(where))
                            {
                                updateSQL += " WHERE " + where + ";";
                            }

                            OleDbCommand myCommand = new OleDbCommand();
                            myCommand.Connection = Conn;
                            myCommand.CommandText = updateSQL;
                            myCommand.ExecuteNonQuery();
                        }
                    }
                    else if(dt.Rows.Count != 1 && SelectAllRows == false)
                    {
                        Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                        Error = "Excel Query should return only one row" + Environment.NewLine + sql + Environment.NewLine + "Returned: " + dt.Rows.Count + " Records";
                    }                   
                }
                catch (Exception ex)
                {
                    this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    Error = ex.Message;
                }
                finally
                {
                    Conn.Close();
                }

                if (dt.Rows.Count == 0)
                {
                    this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    Error = "No rows found in excel file matching criteria - " + sql;
                }
            }
        }

        public bool check_file_open(String name)
        {
            try
            {
                FileStream fs = new FileStream(name, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                fs.Close();
                return true;
            }
            catch (Exception ex)
            {
                Error = "Write Operation canceled due to error: "+ ex.Message;
                return false;
            }
        }


        public void WriteData()
        {            
            string sql = "";
            string ConnString = GetConnectionString();
            string result = System.Text.RegularExpressions.Regex.Replace(GetInputParamCalculatedValue("ColMappingRules"), @",(?=[^']*'(?:[^']*'[^']*')*[^']*$)", "~^GINGER-EXCEL-COMMA-REPLACE^~");
            string[] varColMaps = result.Split(',');
            
            string sSetDataUsed = "";

            using (OleDbConnection Conn = new OleDbConnection(ConnString))
            {
                try
                {
                    Conn.Open();
                }
                catch (Exception ex)
                {
                    System.Threading.Thread.Sleep(3000);
                    Conn.Open();
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.StackTrace}");
                }

                OleDbCommand Cmd = new OleDbCommand();
                Cmd.Connection = Conn;

                string SheetName = GetInputParamCalculatedValue("SheetName").Trim();

                if(String.IsNullOrEmpty(SheetName))
                {
                    this.Error += "Sheet Name is empty or not selected. Please Select correct sheet name on action configurations";
                    Conn.Close();
                    return;
                }

                if (!SheetName.EndsWith("$")) SheetName += "$";
                if (SelectAllRows == false)
                    sql = "Select TOP 1 * from [" + SheetName + "]";
                else
                    sql = "Select * from [" + SheetName + "]";

                string where = GetInputParamCalculatedValue("SelectRowsWhere");
                if (!string.IsNullOrEmpty(where))
                {
                      sql += " WHERE " + where;
                }
                Cmd.CommandText = sql;
                DataTable dt = new DataTable();

                OleDbDataAdapter da = new OleDbDataAdapter();
                da.SelectCommand = Cmd;
                string updateSQL = "";
                try
                {
                    da.Fill(dt);

                    if (!string.IsNullOrEmpty(GetInputParamCalculatedValue("SetDataUsed")))
                        sSetDataUsed = @", " + GetInputParamCalculatedValue("SetDataUsed");

                    // we expect only 1 record
                    if (dt.Rows.Count == 1 && SelectAllRows == false)
                    {                       
                        DataRow r = dt.Rows[0];                        
                        //Read data to variables
                        foreach (string vc in varColMaps)
                        {
                            string strPrimaryKeyColumn = GetInputParamCalculatedValue("PrimaryKeyColumn");
                            if (strPrimaryKeyColumn.Contains("`")) strPrimaryKeyColumn = strPrimaryKeyColumn.Replace("`", "");

                            string rowKey = r[strPrimaryKeyColumn].ToString();

                            int res;
                            int.TryParse(rowKey, out res);

                            if (res == 0 || r[strPrimaryKeyColumn].GetType() == typeof(System.String))
                            {
                                rowKey = "'" + rowKey + "'";
                            }

                            //TODO: fix me in OO Style

                            //Do mapping
                            string ColName = vc.Split('=')[0];
                            string Value = vc.Split('=')[1];
                            Value = Value.Replace("~^GINGER-EXCEL-COMMA-REPLACE^~", ",");
                            string txt = Value;

                            //keeping the translation of vars to support prevoius implementation
                            VariableBase var = RunOnBusinessFlow.GetHierarchyVariableByName(Value);
                            if (var != null)
                            {
                                ValueExpression VE = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList);
                                VE.Value = var.Value;
                                var.Value = VE.ValueCalculated;
                                txt = var.Value;
                            }

                            //remove '' from value
                            txt = txt.TrimStart(new char[] { '\''});
                            txt = txt.TrimEnd(new char[] { '\'' });

                            //TODO: create one long SQL to do the update in one time and not for each var
                            updateSQL = @"UPDATE [" + GetInputParamCalculatedValue("SheetName") + "$] SET " +
                                                    ColName + " = '" + txt + "'" + sSetDataUsed +
                                                        " WHERE " + GetInputParamCalculatedValue("PrimaryKeyColumn") + "=" + rowKey + ";";

                            this.ExInfo += updateSQL + Environment.NewLine;

                            OleDbCommand myCommand = new OleDbCommand();
                            myCommand.Connection = Conn;
                            myCommand.CommandText = updateSQL;
                            myCommand.ExecuteNonQuery();
                        }                        
                         // Do the update that row is used                        
                    }
                    else if (dt.Rows.Count > 0 && SelectAllRows == true)
                    {
                        updateSQL = @"UPDATE [" + GetInputParamCalculatedValue("SheetName") + "$] SET ";
                        foreach (string vc in varColMaps)
                        {
                            //TODO: fix me in OO Style

                            //Do mapping
                            string ColName = vc.Split('=')[0];
                            string Value = vc.Split('=')[1];
                            Value = Value.Replace("~^GINGER-EXCEL-COMMA-REPLACE^~", ",");
                            string txt = Value;

                            //keeping the translation of vars to support prevoius implementation
                            VariableBase var = RunOnBusinessFlow.GetHierarchyVariableByName(Value);
                            if (var != null)
                            {
                                ValueExpression VE = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList);
                                VE.Value = var.Value;
                                var.Value = VE.ValueCalculated;
                                if (var != null)
                                    txt = var.Value;
                                else
                                    txt = Value;
                            }

                            //remove '' from value
                            txt = txt.TrimStart(new char[] { '\'' });
                            txt = txt.TrimEnd(new char[] { '\'' });

                            //TODO: create one long SQL to do the update in one time and not for each var
                            updateSQL = updateSQL + ColName + " = '" + txt + "',";
                        }
                        updateSQL = updateSQL.Substring(0, updateSQL.Length - 1);
                        updateSQL = updateSQL + sSetDataUsed;
                        if (!string.IsNullOrEmpty(where))
                        {
                            updateSQL += " WHERE " + where + ";";
                        }
                        this.ExInfo += updateSQL + Environment.NewLine;

                        OleDbCommand myCommand = new OleDbCommand();
                        myCommand.Connection = Conn;
                        myCommand.CommandText = updateSQL;
                        myCommand.ExecuteNonQuery();
                    }
                    else if(dt.Rows.Count == 0)
                        this.ExInfo = "No Rows updated with given criteria";
                }
                catch (Exception ex)
                {
                    // Reporter.ToLog(eLogLevel.ERROR, "Wrting into excel got error " + ex.Message);
                    this.Error = "Error when trying to update the excel: " + ex.Message + Environment.NewLine + "UpdateSQL=" + updateSQL;
                }
                finally
                {
                    Conn.Close();
                }

                // then show a message if needed
                if (dt.Rows.Count == 0)
                {
                    //TODO: reporter
                    // Reporter.ToUser("No rows found in excel file matching criteria - " + sql);                
                    //  throw new Exception("No rows found in excel file matching criteria - " + sql);
                }
            }
        }

        public DataTable GetExcelSheetData(string Where)
        {

            //TODO: check what is required on the machine and maybe support for other version
            string ConnString = GetConnectionString();
            string sSheetName="";

            using (OleDbConnection Conn = new OleDbConnection(ConnString))
            {
                try
                {
                    Conn.Open();

                    OleDbCommand Cmd = new OleDbCommand();
                    Cmd.Connection = Conn;
                    if (GetInputParamCalculatedValue("SheetName") == null)
                        return new DataTable();
                    if (GetInputParamCalculatedValue("SheetName").Trim().IndexOf("$") == GetInputParamCalculatedValue("SheetName").Trim().Length - 1)
                        sSheetName = GetInputParamCalculatedValue("SheetName");
                    else
                        sSheetName = GetInputParamCalculatedValue("SheetName") + "$";
                    string sql = "Select * from [" + sSheetName + "]";
                    if (Where != null)
                    {
                        sql = sql + " WHERE " + Where;
                    }
                    Cmd.CommandText = sql;
                    DataTable dt = new DataTable();

                    OleDbDataAdapter da = new OleDbDataAdapter();
                    da.SelectCommand = Cmd;

                    da.Fill(dt);

                    return dt;
                }
                catch (Exception ex)
                {
                    switch(ex.Message)
                    {
                        case "Syntax error in FROM clause.":
                            break;
                        case "No value given for one or more required parameters.":
                            Reporter.ToUser(eUserMsgKeys.ExcelBadWhereClause);
                            break;
                        default:                            
                            Reporter.ToUser(eUserMsgKeys.StaticErrorMessage, ex.Message);
                            break;
                    }
                    return null;
                }
            }
        }

        internal static ObservableList<ActReturnValue> GetVarColsFromString(string sVarCols)
        {
            ObservableList<ActReturnValue> VarCols = new ObservableList<ActReturnValue>();

            string[] VarColMap = sVarCols.Split(',');


            foreach (var c in VarColMap)
            {
                VarCols.Add(new ActReturnValue() { Param = c });
            }



            return VarCols;
        }
        internal static string[] GetVarColsMapsFromString(string sVarCols)
        {


            string[] VarColMap = sVarCols.Split(',');



            return VarColMap;
        }

        public string GetExcelFileNameForDriver()
        {
            string ExcelFileNameAbsolutue = GetInputParamCalculatedValue("ExcelFileName");

            if (string.IsNullOrEmpty(ExcelFileNameAbsolutue))
            {
                return "";
            }

            ExcelFileNameAbsolutue = ExcelFileNameAbsolutue.ToUpper();

            if (ExcelFileNameAbsolutue.Contains(@"~\"))
            {
                ExcelFileNameAbsolutue = ExcelFileNameAbsolutue.Replace(@"~\", SolutionFolder);
            }
            return ExcelFileNameAbsolutue;
        }

        public DataTable GetExcelSheetDataWithWhere()
        {
            return GetExcelSheetData(GetInputParamCalculatedValue("SelectRowsWhere"));
        }
    }
}
