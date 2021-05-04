using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Linq;
using GingerCore.Variables;
using System.Data.OleDb;
using System.Reflection;

namespace Amdocs.Ginger.CoreNET.ActionsLib
{
    public class ActExcelNPOI : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Excel Action"; } }
        public override string ActionUserDescription { get { return "Read/Write Excel"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to Read/Write/etc. excel sheet from/on a system drives.");
            TBH.AddLineBreak();
            TBH.AddText("This action contains list of options which will allow you to read/write excel file and also read excel rows with Where conditions.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("Read Excel :- If you want to read excel sheet from system then select Read data from Excel Action type dropdown, Then browse the file by clicking Browse button.Once you " +
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

        public override eImageType Image { get { return eImageType.ExcelFile; } }



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
            //string ConnString = GetExcelFileNameForDriver();
            //string sql = "";

            //using (OleDbConnection Conn = new OleDbConnection(ConnString))
            //{
            //    Conn.Open();
            //    OleDbCommand Cmd = new OleDbCommand();
            //    Cmd.Connection = Conn;

            //    string SheetName = GetInputParamCalculatedValue("SheetName").Trim();
            //    if (!SheetName.EndsWith("$")) SheetName += "$";

            //    string where = GetInputParamCalculatedValue("SelectRowsWhere");
            //    if (!string.IsNullOrEmpty(where))
            //    {
            //        //  sql += " WHERE " + where;
            //    }


            //    // something like  "SELECT * FROM [MySheet$C11]";  //not working
            //    // or range "SELECT * FROM [MySheet$A1:C200]";  //working
            //    // sql = "Select * from [" + SheetName + where + "]";
            //    sql = "Select * from [" + SheetName + where + "]";

            //    Cmd.CommandText = sql;
            //    DataTable dt = new DataTable();

            //    OleDbDataAdapter da = new OleDbDataAdapter();
            //    da.SelectCommand = Cmd;
            //    try
            //    {
            //        da.Fill(dt);
            //        if (SelectAllRows == false)
            //        {
            //            string CellValue = dt.Rows[0][0].ToString();
            //            AddOrUpdateReturnParamActual("Actual", CellValue);
            //        }
            //        else
            //        {
            //            for (int j = 0; j < dt.Rows.Count; j++)
            //            {
            //                DataRow r = dt.Rows[j];
            //                //Read data to return values
            //                // in case the user didn't select cols then get all excel output columns
            //                for (int i = 0; i < dt.Columns.Count; i++)
            //                {
            //                    AddOrUpdateReturnParamActualWithPath("Actual", ((object)r[i]).ToString(), (j + 1).ToString() + (i + 1).ToString());
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            //        Error = ex.Message;
            //    }
            //    finally
            //    {
            //        Conn.Close();
            //    }

            //    if (dt.Rows.Count == 0)
            //    {
            //        this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            //        Error = "No rows found in excel file matching criteria - " + sql;
            //    }
            //}
        }

        public List<string> GetSheets()
        {
            List<string> returnList = new List<string>();
            var fileExtension = WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(ExcelFileName);
            string sheetName;
            ISheet sheet = null;
            using (var fs = new FileStream(fileExtension, FileMode.Open, FileAccess.Read))
            {
                var wb = new XSSFWorkbook(fs);
                for(int i = 0; i < wb.NumberOfSheets; i++)
                {
                    returnList.Add(wb.GetSheetAt(i).SheetName);
                }
            }
            ////remove the last $ sign = not user friendly
            //List<string> returnList = c.AsEnumerable().Select(r => r.Field<string>("TABLE_NAME").Substring(0, r.Field<string>("TABLE_NAME").Length - 1)).ToList();
            //return returnList;
            //string ConnString = GetConnectionString();

            //using (OleDbConnection Conn = new OleDbConnection(ConnString))
            //{
            //    try
            //    {
            //        Conn.Open();
            //        DataTable c = Conn.GetSchema("Tables");
            //        // remove the last $ sign = not user friendly
            //        List<string> returnList = c.AsEnumerable().Select(r => r.Field<string>("TABLE_NAME").Substring(0, r.Field<string>("TABLE_NAME").Length - 1))
            //                   .ToList();
            //        return returnList;

            //    }
            //    catch (Exception ex)
            //    {
            //        Reporter.ToLog(eLogLevel.ERROR, "Failed to get Excel Sheets", ex);
            //        return new List<string>();
            //    }
            //}
            return returnList;
        }
        public void ReadData()
        {
            string sql = ""; // to delete
            string fileName = GetExcelFileNameForDriver();
            string sheetName = SheetName;
            string sSetDataUsed = GetInputParamCalculatedValue("SetDataUsed");
            var workbook = GetExcelWorkbook(fileName);
            var sheet = workbook.GetSheet(sheetName);
            DataTable dt = ConvertSheetToDataTable(sheet);

            dt.DefaultView.RowFilter = SelectRowsWhere ?? "";
            DataTable dtFiltered = GetFilteredDataTable(dt, SelectAllRows);
            List<Tuple<string, object>> setDataUsedList = GetSetDataUsed(sSetDataUsed);
            try
            {
                for (int j = 0; j < dtFiltered.Rows.Count; j++)
                {
                    DataRow r = dtFiltered.Rows[j];

                    for (int i = 0; i < dtFiltered.Columns.Count; i++)
                    {
                        AddOrUpdateReturnParamActual(dtFiltered.Columns[i].ColumnName, ((object)r[i]).ToString());
                    }

                    if (setDataUsedList.Count > 0)
                    {
                        int rowKey = Convert.ToInt32(r[GetInputParamCalculatedValue("PrimaryKeyColumn")]);
                        setDataUsedList.ForEach(x =>
                        sheet.GetRow(dt.Rows.IndexOf(dt.Select(SelectRowsWhere)[j]) + 1).GetCell(dt.Columns[x.Item1].Ordinal).SetCellValue((string)x.Item2));
                    }
                }

                if (setDataUsedList.Count > 0)
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Create))
                    {
                        workbook.Write(fs);
                        fs.Close();
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

            }
            if (dt.Rows.Count == 0)
            {
                this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                Error = "No rows found in excel file matching criteria - " + sql;
            }
        }

        private List<Tuple<string, object>> GetSetDataUsed(string setDataUsed)
        {
            List<Tuple<string, object>> columnNameAndValue = new List<Tuple<string, object>>();
            if (setDataUsed == null)
            {
                return columnNameAndValue;
            }
            bool isError = false;
            string[] data = setDataUsed.Split(',');
            data.ToList().ForEach(d =>
            {
                string[] setData = d.Split('=');
                if (setData.Length == 2)
                {
                    string rowToSet = setData[0].Replace("[|]", "");
                    object valueToSet = setData[1].Replace("'", "");
                    columnNameAndValue.Add(new Tuple<string,object>(rowToSet, valueToSet));
                }
                else
                {
                    Error = "Invalid 'SetDataUsed' data";
                    isError = true;
                }

            });
            return isError ? null : columnNameAndValue;
        }

        private DataTable GetFilteredDataTable(DataTable dataTable, bool selectAllRows)
        {
            return selectAllRows ? dataTable.DefaultView.ToTable() : dataTable.DefaultView.ToTable().AsEnumerable().Take(1).CopyToDataTable();
        }

        private DataTable ConvertSheetToDataTable(ISheet sheet)
        {
            try
            {
                var dtExcelTable = new DataTable();
                dtExcelTable.Rows.Clear();
                dtExcelTable.Columns.Clear();
                var headerRow = sheet.GetRow(0);
                int colCount = headerRow.LastCellNum;
                for (var c = 0; c < colCount; c++)
                    dtExcelTable.Columns.Add(headerRow.GetCell(c).ToString());
                var i = 1;
                var currentRow = sheet.GetRow(i);
                while (currentRow != null)
                {
                    var dr = dtExcelTable.NewRow();
                    for (var j = 0; j < currentRow.Cells.Count; j++)
                    {
                        var cell = currentRow.GetCell(j);

                        if (cell != null)
                            switch (cell.CellType)
                            {
                                case CellType.Numeric:
                                    dr[j] = DateUtil.IsCellDateFormatted(cell)
                                        ? cell.DateCellValue.ToString(CultureInfo.InvariantCulture)
                                        : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
                                    break;
                                case CellType.String:
                                    dr[j] = cell.StringCellValue;
                                    break;
                                case CellType.Blank:
                                    dr[j] = string.Empty;
                                    break;
                            }
                    }
                    dtExcelTable.Rows.Add(dr);
                    i++;
                    currentRow = sheet.GetRow(i);
                }
                return dtExcelTable;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        private IWorkbook GetExcelWorkbook(string fullFilePath)
        {
            var fileExtension = Path.GetExtension(fullFilePath);
            //ISheet sheet = null;
            IWorkbook workbook = null;
            switch (fileExtension)
            {
                case ".xlsx":
                    using (var fs = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read))
                    {
                        workbook = new XSSFWorkbook(fs);
                        //sheet = (XSSFSheet)wb.GetSheet(SheetName);
                    }
                    break;
                case ".xls":
                    using (var fs = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read))
                    {
                        workbook = new HSSFWorkbook(fs);
                        //sheet = (HSSFSheet)wb.GetSheet(SheetName);
                    }
                    break;
            }
            return workbook;
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
                Error = "Write Operation canceled due to error: " + ex.Message;
                return false;
            }
        }
        private string GetConnectionString()
        {
            string s = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + GetExcelFileNameForDriver() + ";Extended Properties=\"Excel 12.0 Xml;HDR=YES;\"";
            return s;
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
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.StackTrace}", ex);
                }

                OleDbCommand Cmd = new OleDbCommand();
                Cmd.Connection = Conn;

                string SheetName = GetInputParamCalculatedValue("SheetName").Trim();

                if (String.IsNullOrEmpty(SheetName))
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

                            //keeping the translation of vars to support previous implementation
                            VariableBase var = RunOnBusinessFlow.GetHierarchyVariableByName(Value);
                            if (var != null)
                            {
                                var.Value = ValueExpression.Calculate(var.Value);
                                txt = var.Value;
                            }

                            //remove '' from value
                            txt = txt.TrimStart(new char[] { '\'' });
                            txt = txt.TrimEnd(new char[] { '\'' });

                            //TODO: create one long SQL to do the update in one time and not for each var
                            updateSQL = @"UPDATE [" + GetInputParamCalculatedValue("SheetName") + "$] SET " +
                                                    ColName + " = '" + txt + "'" + sSetDataUsed +
                                                        " WHERE " + GetInputParamCalculatedValue("PrimaryKeyColumn") + "=" + rowKey + ";";

                            this.ExInfo += updateSQL;

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

                            //keeping the translation of vars to support previous implementation
                            VariableBase var = RunOnBusinessFlow.GetHierarchyVariableByName(Value);
                            if (var != null)
                            {

                                var.Value = ValueExpression.Calculate(var.Value);
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
                        this.ExInfo += updateSQL;

                        OleDbCommand myCommand = new OleDbCommand();
                        myCommand.Connection = Conn;
                        myCommand.CommandText = updateSQL;
                        myCommand.ExecuteNonQuery();
                    }
                    else if (dt.Rows.Count == 0)
                        this.ExInfo = "No Rows updated with given criteria";
                }
                catch (Exception ex)
                {
                    // Reporter.ToLog(eAppReporterLogLevel.ERROR, "Writing into excel got error " + ex.Message);
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
        public void WriteData2()
        {
            //string sql = "";
            //string ConnString = GetConnectionString();

            string result = System.Text.RegularExpressions.Regex.Replace(GetInputParamCalculatedValue("ColMappingRules"), @",(?=[^']*'(?:[^']*'[^']*')*[^']*$)", "~^GINGER-EXCEL-COMMA-REPLACE^~");
            //string[] varColMaps = result.Split(',');

            //string sSetDataUsed = "";

            //using (OleDbConnection Conn = new OleDbConnection(ConnString))
            //{
            //    try
            //    {
            //        Conn.Open();
            //    }
            //    catch (Exception ex)
            //    {
            //        System.Threading.Thread.Sleep(3000);
            //        Conn.Open();
            //        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.StackTrace}", ex);
            //    }

            //    OleDbCommand Cmd = new OleDbCommand();
            //    Cmd.Connection = Conn;

            //string SheetName = GetInputParamCalculatedValue("SheetName").Trim();

            


            string where = GetInputParamCalculatedValue("SelectRowsWhere");
            string fileName = GetExcelFileNameForDriver();
            string sheetName = SheetName.Trim();
            if (String.IsNullOrEmpty(SheetName))
            {
                this.Error += "Sheet Name is empty or not selected. Please Select correct sheet name on action configurations";
                return;
            }
            string sSetDataUsed = GetInputParamCalculatedValue("SetDataUsed");
            var workbook = GetExcelWorkbook(fileName);
            var sheet = workbook.GetSheet(sheetName);
            DataTable dt = ConvertSheetToDataTable(sheet);

            dt.DefaultView.RowFilter = SelectRowsWhere ?? "";
            DataTable dtFiltered = GetFilteredDataTable(dt, SelectAllRows);
            List<Tuple<string, object>> setDataUsedList = GetSetDataUsed(sSetDataUsed);
            List<Tuple<string, object>> varColMapsList = GetSetDataUsed(result);
            if (varColMapsList != null)
            {
                setDataUsedList.AddRange(varColMapsList);
            }
            try
            {
                for (int j = 0; j < dtFiltered.Rows.Count; j++)
                {
                    DataRow r = dtFiltered.Rows[j];
                    if (setDataUsedList.Count > 0)
                    {
                        int rowKey = Convert.ToInt32(r[GetInputParamCalculatedValue("PrimaryKeyColumn")]);
                        setDataUsedList.ForEach(x =>
                        sheet.GetRow(dt.Rows.IndexOf(dt.Select(SelectRowsWhere)[j]) + 1).GetCell(dt.Columns[x.Item1].Ordinal).SetCellValue((string)x.Item2));
                    }
                }

                if (setDataUsedList.Count > 0)
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Create))
                    {
                        workbook.Write(fs);
                        fs.Close();
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

            }
            //string updateSQL = "";

            //try
            //{

            //    // we expect only 1 record
            //    if (dt.Rows.Count == 1 && SelectAllRows == false)
            //    {
            //        DataRow r = dt.Rows[0];
            //        //Read data to variables
            //        foreach (string vc in varColMaps)
            //        {
            //            //string strPrimaryKeyColumn = GetInputParamCalculatedValue("PrimaryKeyColumn");
            //            //if (strPrimaryKeyColumn.Contains("`")) strPrimaryKeyColumn = strPrimaryKeyColumn.Replace("`", "");

            //            //string rowKey = r[strPrimaryKeyColumn].ToString();

            //            //int res;
            //            //int.TryParse(rowKey, out res);

            //            //if (res == 0 || r[strPrimaryKeyColumn].GetType() == typeof(System.String))
            //            //{
            //            //    rowKey = "'" + rowKey + "'";
            //            //}

            //            //TODO: fix me in OO Style

            //            //Do mapping
            //            string ColName = vc.Split('=')[0];
            //            string Value = vc.Split('=')[1];
            //            Value = Value.Replace("~^GINGER-EXCEL-COMMA-REPLACE^~", ",");
            //            string txt = Value;

            //            //keeping the translation of vars to support previous implementation
            //            VariableBase var = RunOnBusinessFlow.GetHierarchyVariableByName(Value);
            //            if (var != null)
            //            {
            //                var.Value = ValueExpression.Calculate(var.Value);
            //                txt = var.Value;
            //            }

            //            //remove '' from value
            //            txt = txt.TrimStart(new char[] { '\'' });
            //            txt = txt.TrimEnd(new char[] { '\'' });

            //            //TODO: create one long SQL to do the update in one time and not for each var
            //            updateSQL = @"UPDATE [" + GetInputParamCalculatedValue("SheetName") + "$] SET " +
            //                                    ColName + " = '" + txt + "'" + sSetDataUsed +
            //                                        " WHERE " + GetInputParamCalculatedValue("PrimaryKeyColumn") + "=" + rowKey + ";";

            //            this.ExInfo += updateSQL;
            //        }
            //        // Do the update that row is used                        
            //    }
            //    else if (dt.Rows.Count > 0 && SelectAllRows == true)
            //    {
            //        updateSQL = @"UPDATE [" + GetInputParamCalculatedValue("SheetName") + "$] SET ";
            //        foreach (string vc in varColMaps)
            //        {
            //            //TODO: fix me in OO Style

            //            //Do mapping
            //            string ColName = vc.Split('=')[0];
            //            string Value = vc.Split('=')[1];
            //            Value = Value.Replace("~^GINGER-EXCEL-COMMA-REPLACE^~", ",");
            //            string txt = Value;

            //            //keeping the translation of vars to support previous implementation
            //            VariableBase var = RunOnBusinessFlow.GetHierarchyVariableByName(Value);
            //            if (var != null)
            //            {

            //                var.Value = ValueExpression.Calculate(var.Value);
            //                if (var != null)
            //                    txt = var.Value;
            //                else
            //                    txt = Value;
            //            }

            //            //remove '' from value
            //            txt = txt.TrimStart(new char[] { '\'' });
            //            txt = txt.TrimEnd(new char[] { '\'' });

            //            //TODO: create one long SQL to do the update in one time and not for each var
            //            updateSQL = updateSQL + ColName + " = '" + txt + "',";
            //        }
            //        updateSQL = updateSQL.Substring(0, updateSQL.Length - 1);
            //        updateSQL = updateSQL + sSetDataUsed;
            //        if (!string.IsNullOrEmpty(where))
            //        {
            //            updateSQL += " WHERE " + where + ";";
            //        }
            //        this.ExInfo += updateSQL;

            //    }
            //    else if (dt.Rows.Count == 0)
            //        this.ExInfo = "No Rows updated with given criteria";
            //}
            //catch (Exception ex)
            //{
            //    // Reporter.ToLog(eAppReporterLogLevel.ERROR, "Writing into excel got error " + ex.Message);
            //    this.Error = "Error when trying to update the excel: " + ex.Message + Environment.NewLine + "UpdateSQL=" + updateSQL;
            //}
            //finally
            //{
            //}

            //// then show a message if needed
            //if (dt.Rows.Count == 0)
            //{
            //    //TODO: reporter
            //    // Reporter.ToUser("No rows found in excel file matching criteria - " + sql);                
            //    //  throw new Exception("No rows found in excel file matching criteria - " + sql);
            //}
        
    }

    public DataTable GetExcelSheetData(string Where)
        {
            try
            {
                if (GetInputParamCalculatedValue("SheetName") == null)
                {
                    return new DataTable();
                }
                var workbook = GetExcelWorkbook(GetExcelFileNameForDriver());
                var sheet = workbook.GetSheet(SheetName);
                DataTable dt = ConvertSheetToDataTable(sheet);
                dt.DefaultView.RowFilter = Where;
                return dt.DefaultView.ToTable();
            }
            catch (Exception ex)
            {
                switch (ex.Message)
                {
                    case "Syntax error in FROM clause.":
                        break;
                    case "No value given for one or more required parameters.":
                        Reporter.ToUser(eUserMsgKey.ExcelBadWhereClause);
                        break;
                    default:
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, ex.Message);
                        break;
                }
                return null;
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
            return WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(ExcelFileNameAbsolutue);
        }

        public DataTable GetExcelSheetDataWithWhere()
        {
            return GetExcelSheetData(GetInputParamCalculatedValue("SelectRowsWhere"));
        }
    }
}
