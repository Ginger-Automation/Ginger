#region License
/*
Copyright © 2014-2025 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.ActionsLib;
using Amdocs.Ginger.Repository;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using NPOI.OpenXmlFormats.Dml.Diagram;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace GingerCore.Actions
{
    public class ActExcel : ActWithoutDriver
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
            TBH.AddText("Read Excel Cells:- User option to Read Excel data by cells, Select Read Cell Data from Action type dropdown, Then browse the file by clicking Browse button.Once you " +
             "browse the file then all the sheets will get bind to Sheet Name dropdown,Select sheet name and click on view button.If you want to put where condition on excel sheet then type cell location in the excel. for one cell: Like A2, for multi cells: Like A2:D4");
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
            ReadData = 0,
            [EnumValueDescription("Write Data")]
            WriteData = 1,
            [EnumValueDescription("Read Cell Data")]
            ReadCellData = 2,
            [EnumValueDescription("Get Sheet Details")]
            GetSheetDetails = 3
        }

        // --- NEW PROPERTIES ---
        public string RowIndex
        {
            get { return GetInputParamValue(nameof(RowIndex)); }
            set { AddOrUpdateInputParamValue(nameof(RowIndex), value); OnPropertyChanged(nameof(RowIndex)); }
        }
        public enum eDataSelectionMethod
        {
            [EnumValueDescription("By Parameters")]
            ByParameters = 0,
            [EnumValueDescription("By Cell Address")]
            ByCellAddress = 1
        }

        // --- PROPERTIES ---

        public eDataSelectionMethod DataSelectionMethod
        {
            get { return GetOrCreateInputParam<eDataSelectionMethod>(nameof(DataSelectionMethod), eDataSelectionMethod.ByParameters); }
            set { AddOrUpdateInputParamValue(nameof(DataSelectionMethod), value.ToString()); OnPropertyChanged(nameof(DataSelectionMethod)); }
        }
     
        public bool PullCellAddress
        {
            get
            {
                bool value = false;
                bool.TryParse(GetOrCreateInputParam(nameof(PullCellAddress)).Value, out value);
                return value;
            }
            set { AddOrUpdateInputParamValue(nameof(PullCellAddress), value.ToString()); OnPropertyChanged(nameof(PullCellAddress)); }
        }

        public string ColumnIndex
        {
            get { return GetInputParamValue(nameof(ColumnIndex)); }
            set { AddOrUpdateInputParamValue(nameof(ColumnIndex), value); OnPropertyChanged(nameof(ColumnIndex)); }
        }

        // --- EXISTING PROPERTIES ---
        public string ExcelFileName
        {
            get { return GetInputParamValue(nameof(ExcelFileName)); }
            set { AddOrUpdateInputParamValue(nameof(ExcelFileName), value); OnPropertyChanged(nameof(ExcelFileName)); }
        }
        public string CalculatedFileName
        {
            get
            {
                string file = GetInputParamCalculatedValue(nameof(ExcelFileName)) ?? "";
                return WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(file);
            }
        }
        public string SheetName
        {
            get { return GetInputParamValue(nameof(SheetName)); }
            set { AddOrUpdateInputParamValue(nameof(SheetName), value); OnPropertyChanged(nameof(SheetName)); }
        }
        public string HeaderRowNum
        {
            get { return GetInputParamValue(nameof(HeaderRowNum)) ?? "1"; }
            set { AddOrUpdateInputParamValue(nameof(HeaderRowNum), value); OnPropertyChanged(nameof(HeaderRowNum)); }
        }
        public string CalculatedSheetName
        {
            get { return (GetInputParamCalculatedValue(nameof(SheetName)) ?? "").Trim(); }
        }
        public string CalculatedHeaderRowNum
        {
            get { return GetInputParamCalculatedValue(nameof(HeaderRowNum)) ?? "1"; }
        }
        public string SelectRowsWhere
        {
            get { return GetInputParamValue(nameof(SelectRowsWhere)); }
            set { AddOrUpdateInputParamValue(nameof(SelectRowsWhere), value); OnPropertyChanged(nameof(SelectRowsWhere)); }
        }

        public string SelectCellAddress
        {
            get { return GetInputParamValue(nameof(SelectCellAddress)); }
            set { AddOrUpdateInputParamValue(nameof(SelectCellAddress), value); OnPropertyChanged(nameof(SelectCellAddress)); }
        }

        public string CalculatedFilter
        {
            get
            {
                if (DataSelectionMethod == eDataSelectionMethod.ByCellAddress)
                {
                    return GetInputParamCalculatedValue(nameof(SelectCellAddress));
                }

                string filter = GetInputParamCalculatedValue(nameof(SelectRowsWhere));
                return filter == null ? filter : filter.Replace("\"", "'");
            }
        }
        public string PrimaryKeyColumn
        {
            get { return GetInputParamValue(nameof(PrimaryKeyColumn)); }
            set { AddOrUpdateInputParamValue(nameof(PrimaryKeyColumn), value); OnPropertyChanged(nameof(PrimaryKeyColumn)); }
        }
        public string SetDataUsed
        {
            get { return GetInputParamValue(nameof(SetDataUsed)); }
            set { AddOrUpdateInputParamValue(nameof(SetDataUsed), value); OnPropertyChanged(nameof(SetDataUsed)); }
        }
        private string CalculatedSetDataUsed
        {
            get
            {
                string setData = GetInputParamCalculatedValue(nameof(SetDataUsed));
                return setData == null ? setData : setData.Replace("\"", "'");
            }
        }
        public eExcelActionType ExcelActionType
        {
            get { return GetOrCreateInputParam<eExcelActionType>(nameof(ExcelActionType), eExcelActionType.ReadData); }
            set { AddOrUpdateInputParamValue(nameof(ExcelActionType), value.ToString()); }
        }
        public bool SelectAllRows
        {
            get
            {
                bool value = false;
                bool.TryParse(GetOrCreateInputParam(nameof(SelectAllRows)).Value, out value);
                return value;
            }
            set { AddOrUpdateInputParamValue(nameof(SelectAllRows), value.ToString()); }
        }
        public string ColMappingRules
        {
            get { return GetInputParamValue(nameof(ColMappingRules)); }
            set { AddOrUpdateInputParamValue(nameof(ColMappingRules), value); OnPropertyChanged(nameof(ColMappingRules)); }
        }
        private string CalculatedColMappingRules
        {
            get
            {
                string mapping = GetInputParamCalculatedValue(nameof(ColMappingRules));
                return mapping == null ? mapping : mapping.Replace("\"", "'");
            }
        }

        public override string ActionType
        {
            get { return "Excel" + ExcelActionType.ToString(); }
        }

        public override eImageType Image { get { return eImageType.ExcelFile; } }

        IExcelOperations excelOperator = null;

        public override void Execute()
        {
            if (!CheckMandatoryFieldsExists([nameof(CalculatedFileName), nameof(CalculatedSheetName)]))
            {
                return;
            }
            using (excelOperator = new ExcelNPOIOperations())
            {
                if (ExcelActionType == eExcelActionType.ReadData)
                {
                    ReadData();
                }
                else if (ExcelActionType == eExcelActionType.WriteData)
                {
                    WriteData();
                }
                else if (ExcelActionType == eExcelActionType.ReadCellData)
                {
                    ReadCellData();
                }
                else if (ExcelActionType == eExcelActionType.GetSheetDetails)
                {
                    GetSheetDetails();
                }
                else
                {
                    Reporter.ToLog(eLogLevel.INFO, "Only action type can be selected");
                }
            }
        }

        // ----------------------------------------------------------------------------------
        // HELPER: Convert Column Number to Letter (1->A, 2->B)
        // ----------------------------------------------------------------------------------
        private string GetColumnName(int columnNumber)
        {
            string columnName = "";
            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - 1) / 26;
            }
            return columnName;
        }

        // ----------------------------------------------------------------------------------
        // NEW IMPLEMENTATIONS
        // ----------------------------------------------------------------------------------
        private void GetSheetDetails()
        {
            try
            {
                // 1. Read the data to get the dimensions
                // Note: We set "UseHeaderRow" to true so it respects your "Header Row Number" input
                DataTable dt = excelOperator.ReadData(CalculatedFileName, CalculatedSheetName, "", true, CalculatedHeaderRowNum);

                if (dt != null)
                {
                    // --- PART 1: COUNTS 
                    int rowCount = 0;
                    int colCount = dt.Columns.Count;
                    foreach (DataRow row in dt.Rows)
                    {
                        // Check if row has at least one non-empty value
                        if (row.ItemArray.Any(item => item != null && !string.IsNullOrWhiteSpace(item.ToString())))
                        {
                            rowCount++;
                        }
                    }

                    AddOrUpdateReturnParamActual("TotalRowCount", rowCount.ToString());
                    AddOrUpdateReturnParamActual("TotalColumnCount", colCount.ToString());

                    // --- PART 2: ADDRESS RANGES

                    // Calculate Start Indices
                    int startRowIndex = 0;
                    int.TryParse(CalculatedHeaderRowNum, out startRowIndex);
                    if (startRowIndex < 1) { startRowIndex = 1; } // Default to 1 if invalid

                    int startColIndex = 1; // Excel starts at 'A' (1)

                    // Calculate End Indices
                    // The last row is the Start Row + the number of data rows found
                    int endRowIndex = startRowIndex + rowCount;
                    int endColIndex = startColIndex + (colCount - 1);
                    if (endColIndex < 1) 
                    {
                        endColIndex = 1;
                    }

                    // Generate Address Strings
                    string startCellAddress = GetColumnName(startColIndex) + startRowIndex;      // e.g. "A1"
                    string endCellAddress = GetColumnName(endColIndex) + endRowIndex;            // e.g. "D10"
                    string rangeAddress = $"{startCellAddress}:{endCellAddress}";                // e.g. "A1:D10"

                    // Add to Output Values
                    AddOrUpdateReturnParamActual("StartRow", startRowIndex.ToString());
                    AddOrUpdateReturnParamActual("EndRow", endRowIndex.ToString());
                    AddOrUpdateReturnParamActual("StartColumn", GetColumnName(startColIndex));
                    AddOrUpdateReturnParamActual("EndColumn", GetColumnName(endColIndex));

                    AddOrUpdateReturnParamActual("FirstCellAddress", startCellAddress);
                    AddOrUpdateReturnParamActual("LastCellAddress", endCellAddress);
                    AddOrUpdateReturnParamActual("UsedRangeAddress", rangeAddress);
                }
                else
                {
                    // Handle empty sheet case
                    AddOrUpdateReturnParamActual("TotalRowCount", "0");
                    AddOrUpdateReturnParamActual("TotalColumnCount", "0");
                    AddOrUpdateReturnParamActual("UsedRangeAddress", "");
                }
            }
            catch (Exception ex)
            {
                Error = "Error retrieving sheet details: " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Error in GetSheetDetails", ex);
            }
        }


        
        // ----------------------------------------------------------------------------------
        // READ DATA (Enhanced to put Address in Path)
        // ----------------------------------------------------------------------------------
        public void ReadData()
        {
            try
            {
                if (DataSelectionMethod == eDataSelectionMethod.ByCellAddress)
                {
                    DataTable dtAddr = excelOperator.ReadCellData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, SelectAllRows, CalculatedHeaderRowNum);
                    if (dtAddr == null) { Error = "No data found at " + CalculatedFilter; return; }

                    // Parse start address (e.g. "A5") to calculate paths
                    var startCoord = ParseCellAddress(CalculatedFilter.Split(':')[0]);

                    for (int j = 0; j < dtAddr.Rows.Count; j++)
                    {
                        DataRow r = dtAddr.Rows[j];
                        int currentRowNum = startCoord.Row + j;

                        for (int i = 0; i < dtAddr.Columns.Count; i++)
                        {
                            string colName = dtAddr.Columns[i].ColumnName;
                            string val = r[i].ToString();

                            string cellAddress = GetColumnName(startCoord.Col + i) + currentRowNum;

                            if (PullCellAddress)
                            {
                                AddOrUpdateReturnParamActualWithPath(colName, val, cellAddress);
                                AddOrUpdateReturnParamActual($"{colName}_CellAddress", cellAddress);
                            }
                            else
                            {
                                AddOrUpdateReturnParamActualWithPath(colName, val, cellAddress);
                            }
                        }
                    }
                    return;
                }

                // --- Standard By Parameters Logic ---
                DataTable excelDataTable = excelOperator.ReadData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, SelectAllRows, CalculatedHeaderRowNum);
                int headerRow = 1;
                int.TryParse(CalculatedHeaderRowNum, out headerRow);
                if (excelDataTable != null && excelDataTable.Rows.Count > 0)
                {
                    for (int currentRow = 0; currentRow < excelDataTable.Rows.Count; currentRow++)
                    {
                        DataRow r = excelDataTable.Rows[currentRow];
                         int currentRowNum = headerRow + 1 + currentRow;


                        for (int currentCol = 0; currentCol < excelDataTable.Columns.Count; currentCol++)
                        {
                            if (SelectAllRows)
                            {
                                AddOrUpdateReturnParamActualWithPath(excelDataTable.Columns[currentCol].ColumnName, r[currentCol].ToString(), "" + (currentRow + 1).ToString());
                             

                            }
                            else
                            {
                                AddOrUpdateReturnParamActual(excelDataTable.Columns[currentCol].ColumnName, r[currentCol].ToString());
                         
                            }

                            if (PullCellAddress)
                            {
                                string cellAddress = GetColumnName(currentCol + 1) + currentRowNum;

                                AddOrUpdateReturnParamActual($"{excelDataTable.Columns[currentCol].ColumnName}_CellAddress", cellAddress);
                            }

                        }
                    }
                    bool isUpdated = true;
                    if (SelectAllRows)
                    {
                        if (!String.IsNullOrWhiteSpace(SetDataUsed))
                        {
                            isUpdated = excelOperator.UpdateExcelData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, FieldsValueToTupleList(CalculatedSetDataUsed), CalculatedHeaderRowNum);
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrWhiteSpace(SetDataUsed))
                        {
                            if (string.IsNullOrWhiteSpace(PrimaryKeyColumn))
                            {
                                Error += "Missing or Invalid Primary Key";
                                return;
                            }
                            isUpdated = excelOperator.UpdateExcelData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, FieldsValueToTupleList(CalculatedSetDataUsed), CalculatedHeaderRowNum, CalculatedPrimaryKeyFilter(excelDataTable.Rows[0]));
                        }
                    }
                    if (!isUpdated)
                    {
                        Error = "Failed updated excel data: " + CalculatedSetDataUsed;
                    }
                }
                else
                {
                    Error = SelectAllRows ? "No Rows Found" : "No Rows Found with given filter";
                }
            }
            catch (Exception ex) { Error = ex.Message; Reporter.ToLog(eLogLevel.ERROR, "Error while reading data from Excel", ex); }
        }
        private (int Col, int Row) ParseCellAddress(string address)
        {
            try
            {
                if (string.IsNullOrEmpty(address)) return (1, 1);
                var match = Regex.Match(address, @"([A-Z]+)([0-9]+)");
                if (!match.Success) return (1, 1);

                string colStr = match.Groups[1].Value;
                int row = int.Parse(match.Groups[2].Value);

                int colIndex = 0;
                foreach (char c in colStr)
                {
                    colIndex = colIndex * 26 + (c - 'A' + 1);
                }
                return (colIndex, row);
            }
            catch { return (1, 1); }
        }
        private void ReadCellData()
        {
            try
            {
                DataTable excelDataTable = excelOperator.ReadCellData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, SelectAllRows, CalculatedHeaderRowNum);

                var startCoord = ParseCellAddress(CalculatedFilter.Split(':')[0]);

                if (!string.IsNullOrEmpty(CalculatedFilter) && !SelectAllRows)
                {
                    if (excelDataTable == null) { Error = "Given Cell [" + CalculatedFilter + "] contains empty value"; return; }
                    else
                    {
                        string CellValue = excelDataTable.Rows[0][0].ToString();
                        AddOrUpdateReturnParamActual(excelDataTable.Columns[0].ColumnName, CellValue);
                    }
                }
                else
                {
                    for (int j = 0; j < excelDataTable.Rows.Count; j++)
                    {
                        DataRow r = excelDataTable.Rows[j];
                        int currentRowNum = startCoord.Row + j;

                        for (int i = 0; i < excelDataTable.Columns.Count; i++)
                        {
                         
                            if (PullCellAddress)
                            {
                                // Calculate proper address (e.g. A5)
                                string cellAddress = GetColumnName(startCoord.Col + i) + currentRowNum;
                                AddOrUpdateReturnParamActualWithPath(excelDataTable.Columns[i].ColumnName, r[i].ToString(), cellAddress);
                            }
                            else
                            {
                                AddOrUpdateReturnParamActualWithPath(excelDataTable.Columns[i].ColumnName, r[i].ToString(), (j + 1).ToString() + (i + 1).ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Error += eUserMsgKey.ExcelInvalidFieldData + ", " + ex.Message; Reporter.ToLog(eLogLevel.ERROR, "Error while reading cell data from Excel", ex); }
        }

        public void WriteData()
        {
            try
            {
                // ---------------------------------------------------------------------------
                // 1. "BY CELL ADDRESS" MODE (Direct Write)
                // ---------------------------------------------------------------------------
                if (DataSelectionMethod == eDataSelectionMethod.ByCellAddress)
                {
                    string val = ValueExpression.Calculate(ColMappingRules);

                    // We cannot use standard WriteData because "A5" is not a valid DataTable filter.
                    // We call the new WriteCellData method directly.
                    bool isUpdated = excelOperator.WriteCellData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, val);

                    if (isUpdated)
                    {
                        this.ExInfo = "Write action done";
                    }
                    else
                    {
                        this.Error = "Failed to write to cell " + CalculatedFilter;
                    }
                    return; 
                }

                // ---------------------------------------------------------------------------
                // 2. "BY PARAMETERS" MODE (Database/Filter Logic)
                // ---------------------------------------------------------------------------
                List<Tuple<string, object>> cellValuesToUpdateList = [.. FieldsValueToTupleList(CalculatedSetDataUsed), .. FieldsValueToTupleList(CalculatedColMappingRules)];

                // This ReadData call is safe here because CalculatedFilter is a valid query (e.g., "ID=1")
                DataTable excelDataTable = excelOperator.ReadData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, SelectAllRows, CalculatedHeaderRowNum);

                if (excelDataTable == null)
                {
                    Error = "Table return no Rows with given filter";
                    return;
                }
                if (excelDataTable.Rows.Count == 1 && !SelectAllRows)
                {
                    DataRow r = excelDataTable.Rows[0];
                    this.ExInfo = "Write action done";
                    if (cellValuesToUpdateList.Count > 0)
                    {
                        bool isUpdated = string.IsNullOrEmpty(CalculatedPrimaryKeyFilter(r)) ? excelOperator.WriteData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, "", cellValuesToUpdateList, CalculatedHeaderRowNum) :
                            excelOperator.WriteData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, "", cellValuesToUpdateList, CalculatedHeaderRowNum, CalculatedPrimaryKeyFilter(r));
                    }
                }
                else if (excelDataTable.Rows.Count > 0 && SelectAllRows)
                {
                    bool isUpdated = excelOperator.WriteData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, "", cellValuesToUpdateList, CalculatedHeaderRowNum);
                    this.ExInfo += "write action done";
                }
                else if (excelDataTable.Rows.Count == 0)
                {
                    this.ExInfo = "No Rows updated with given criteria";
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error while writing to Excel", ex);
                this.Error = "Error when trying to update the excel";
            }
        }

        public bool CheckMandatoryFieldsExists(List<string> fields)
        {
            foreach (string field in fields)
            {
                if (String.IsNullOrWhiteSpace((string)this[field]))
                {
                    string calculated = "Calculated";
                    int indexOfField = field.IndexOf(calculated);
                    var splitBetCapLetters = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
                    string substr = indexOfField != -1 ? field[(indexOfField + calculated.Length)..] : field;
                    string actualFieldValue = splitBetCapLetters.Replace(substr, " ");
                    this.Error = $"The Mandatory field : {actualFieldValue} cannot be empty";
                    return false;
                }
            }
            return true;
        }
        public object this[string propertyName] { get { return this.GetType().GetProperty(propertyName).GetValue(this, null); } }
        private List<Tuple<string, object>> FieldsValueToTupleList(string updatedFieldsValue)
        {
            List<Tuple<string, object>> columnNameAndValue = [];
            if (String.IsNullOrEmpty(updatedFieldsValue))
            {
                return columnNameAndValue;
            }
            string result = System.Text.RegularExpressions.Regex.Replace(updatedFieldsValue, @",(?=[^']*'(?:[^']*'[^']*')*[^']*$)", "~^GINGER-EXCEL-COMMA-REPLACE^~");
            string[] varColMaps = result.Split(',');
            varColMaps.ToList().ForEach(c =>
            {
                result = System.Text.RegularExpressions.Regex.Replace(c, @"=(?=[^']*'(?:[^']*'[^']*')*[^']*$)", "~^GINGER-EXCEL-EQUAL-REPLACE^~");
                string[] setData = result.Split('=');
                if (setData.Length == 2)
                {
                    string rowToSet = setData[0].Replace("[", "").Replace("]", "");
                    string valueToSet = setData[1].Replace("~^GINGER-EXCEL-COMMA-REPLACE^~", ",").Replace("~^GINGER-EXCEL-EQUAL-REPLACE^~", "=").TrimStart('\'').TrimEnd('\'');
                    string fieldValue = valueToSet;
                    VariableBase var = RunOnBusinessFlow.GetHierarchyVariableByName(Value);
                    if (var != null) { var.Value = ValueExpression.Calculate(valueToSet); fieldValue = var.Value; }
                    columnNameAndValue.Add(new Tuple<string, object>(rowToSet, fieldValue));
                }
            });
            return columnNameAndValue;
        }
        private string CalculatedPrimaryKeyFilter(DataRow dataRow)
        {
            string pk = GetInputParamCalculatedValue(nameof(PrimaryKeyColumn)).Replace("`", "");
            return dataRow[pk].GetType() == typeof(int) ? pk + "=" + dataRow[pk].ToString() : pk + "=" + "'" + dataRow[pk].ToString() + "'";
        }
    }
}