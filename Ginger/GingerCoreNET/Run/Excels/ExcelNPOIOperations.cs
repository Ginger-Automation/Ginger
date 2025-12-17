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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Amdocs.Ginger.CoreNET.ActionsLib
{
    public class ExcelNPOIOperations : IExcelOperations
    {
        string mFileName { get; set; }
        DataTable mExcelDataTable { get; set; }
        DataTable mFilteredDataTable { get; set; }
        IWorkbook mWorkbook { get; set; }
        ISheet mSheet { get; set; }
        private Regex regex = new Regex(@"(^[A-Z]+\d+$)|(^[A-Z]+\d+:[A-Z]+\d+$)");
        /// <summary>
        /// Reading The Rows and Columns of the Excel Sheet
        /// </summary>
        /// <param name="sheet"> A specific sheet in the Excel</param>
        /// <param name="rowHeaderNumber"> row number at which the header columns are found </param>
        /// <param name="rowLimit">If the 'View Data / View Filtered Data' is selected on the Excel Action Page, the rowLimit is set , which means the user will only see AT MOST 'rowLimit' number of rows apart from the Column Header row </param>
        /// <returns></returns>
        private DataTable ConvertSheetToDataTable(ISheet sheet, int rowHeaderNumber, int rowLimit = -1)
        {
            try
            {
                var dtExcelTable = new DataTable();
                IRow headerRow = this.GetHeaderRow(sheet, rowHeaderNumber);
                /*
                 initialColNumber -> is used to locate the first column number of the first header column
                 */
                int initialColNumber = -1;
                SetHeaderColumns(headerRow, ref initialColNumber, dtExcelTable);
                /*
                 intialColNumber is used to also locate where to start and end the reading of the row data. 
                 */
                SetRowsForDataTable(sheet, dtExcelTable, initialColNumber, rowHeaderNumber, rowLimit);
                return dtExcelTable;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, $"Can't convert sheet to data, {ex.Message}");
                throw;
            }
        }

        private object GetCellValue(ICell cell, CellType cellType)
        {
            var cellVal = cellType switch
            {
                CellType.Numeric => HandleNumericCellType(cell),
                CellType.String => cell.StringCellValue,
                CellType.Boolean => cell.BooleanCellValue,
                CellType.Formula => GetCellValue(cell, cell.CachedFormulaResultType),
                CellType.Blank => null,
                CellType.Error => cell.ErrorCellValue,
                _ => cell.RichStringCellValue,
            };
            return cellVal;
        }


        // Read the whole row and col data with/without filter
        public DataTable ReadData(string fileName, string sheetName, string filter, bool selectedRows, string headerRowNumber = "1")
        {
            filter = filter ?? "";
            try
            {
                GetExcelSheet(fileName, sheetName);
                mExcelDataTable = ConvertSheetToDataTable(mSheet, int.Parse(string.IsNullOrEmpty(headerRowNumber) ? "1" : headerRowNumber));
                mExcelDataTable.DefaultView.RowFilter = filter;
                mFilteredDataTable = GetFilteredDataTable(mExcelDataTable, selectedRows);
                return mFilteredDataTable;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "Can't read sheet data, " + ex.Message);
                throw;
            }
        }

        public DataTable ReadDataWithRowLimit(string fileName, string sheetName, string filter, bool selectedRows, string headerRowNumber = "1", int rowLimit = -1)
        {
            filter = filter ?? "";
            try
            {
                GetExcelSheet(fileName, sheetName);
                mExcelDataTable = ConvertSheetToDataTable(mSheet, int.Parse(string.IsNullOrEmpty(headerRowNumber) ? "1" : headerRowNumber), rowLimit);
                mExcelDataTable.DefaultView.RowFilter = filter;
                mFilteredDataTable = string.IsNullOrEmpty(filter) ? mExcelDataTable : GetFilteredDataTable(mExcelDataTable, selectedRows);
                return mFilteredDataTable;

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "Can't read sheet data, " + ex.Message);
                throw;
            }
        }

        /*
        This function is used as a validator , checks if the file path and/or sheet name exists
        */
        private void GetExcelSheet(string fileName, string sheetName)
        {
            lock (lockObj)
            {
                Thread.Sleep(100);
                GetExcelWorkbook(fileName);
                if (mWorkbook == null)
                {
                    Reporter.ToLog(eLogLevel.WARN, "File name not Exists.");
                    throw new ArgumentException("File does not exist or is currently being used by some other application, Please verify if the File Path is valid");
                }
                mSheet = mWorkbook.GetSheet(sheetName);
                if (mSheet == null)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Sheet name not exists.");
                    throw new ArgumentException("Sheet name does not exist , Please verify if the entered Sheet Name is valid");
                }
            }
        }

        private DataTable GetFilteredDataTable(DataTable dataTable, bool selectAllRows)
        {
            return selectAllRows ? dataTable.DefaultView.ToTable() : dataTable.DefaultView.ToTable().AsEnumerable().Take(1).CopyToDataTable();
        }

        private static readonly Object lockObj = new object();
        public IWorkbook GetExcelWorkbook(string fullFilePath)
        {
            try
            {
                using (var fs = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read))
                {
                    mWorkbook = WorkbookFactory.Create(fs);
                }
                return mWorkbook;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Invalid Excel Path Name" + fullFilePath, ex);
                return null;
            }
        }

        public bool UpdateExcelData(string fileName, string sheetName, string filter, List<Tuple<string, object>> updateCellValuesList, string HeaderRowNum, string primaryKey = null, string key = null)
        {
            if (updateCellValuesList.Count > 0)
            {
                var headerRow = mSheet.GetRow(0);
                foreach (string colName in updateCellValuesList.Select(x => x.Item1))
                {
                    if (!headerRow.Cells.Any(x => x.RichStringCellValue.ToString().Equals(colName)))
                    {
                        return false;
                    }
                }
                if (primaryKey != null)
                {
                    filter = primaryKey;
                }
                UpdateCellsData(updateCellValuesList, mExcelDataTable, filter, fileName, HeaderRowNum);
            }
            return true;
        }


        // Returns a cell's data only if the filter is set otherwise works like the ReadData() function
        public DataTable ReadCellData(string fileName, string sheetName, string filter, bool selectedRows, string headerRowNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(filter))
                {
                    return ReadData(fileName, sheetName, filter, selectedRows, headerRowNumber);
                }
                Match match = regex.Match(filter);
                if (!match.Success)
                {
                    return null;
                }
                GetExcelSheet(fileName, sheetName);
                CellReference cellFrom;
                CellReference cellTo;
                if (filter.Contains(":"))
                {
                    string[] filterArray = filter.Split(':');
                    cellFrom = new CellReference(filterArray[0]);
                    cellTo = new CellReference(filterArray[1]);
                }
                else
                {
                    cellFrom = new CellReference(filter);
                    cellTo = new CellReference(filter);
                }
                var dtExcelTable = new DataTable();
                dtExcelTable.Rows.Clear();
                dtExcelTable.Columns.Clear();
                var headerRow = this.GetHeaderRow(mSheet, int.Parse(string.IsNullOrEmpty(headerRowNumber) ? "1" : headerRowNumber));
                int colCount = headerRow.LastCellNum;
                if (cellFrom.Col > colCount || cellTo.Col > colCount)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Invalid filter expresion, please check");
                    return null;
                }
                for (var c = cellFrom.Col; c <= cellTo.Col; c++)
                {
                    if (headerRow.GetCell(c) != null && !dtExcelTable.Columns.Contains(headerRow.GetCell(c).ToString()))
                    {
                        dtExcelTable.Columns.Add(headerRow.GetCell(c).ToString());
                    }
                }
                var i = cellFrom.Row;
                var currentRow = mSheet.GetRow(i);
                int dtColCount = 0;
                while (i <= cellTo.Row)
                {
                    var dr = dtExcelTable.NewRow();
                    dtColCount = 0;
                    for (var j = cellFrom.Col; j <= cellTo.Col; j++)
                    {
                        ICell cell;
                        if (currentRow != null)
                        {
                            cell = currentRow.GetCell(j);
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.WARN, "Invalid filter expresion, please check");
                            return null;
                        }
                        if (cell != null)
                        {
                            dr[dtColCount] = GetCellValue(cell, cell.CellType);
                        }
                        dtColCount++;
                    }
                    dtExcelTable.Rows.Add(dr);
                    i++;
                    currentRow = mSheet.GetRow(i);
                }
                return dtExcelTable;


            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "Can't read cell data, " + ex.Message);
                throw;
            }
        }

        public bool WriteData(string fileName, string sheetName, string filter, string setDataUsed, List<Tuple<string, object>> updateCellValuesList, string HeaderRowNum, string primaryKey = null, string key = null)
        {
            if (!String.IsNullOrWhiteSpace(primaryKey))
            {
                if (string.IsNullOrWhiteSpace(filter))
                {
                    filter = primaryKey;
                }
                else
                {
                    filter = $"({filter}) and ({primaryKey})";
                }
            }
            return UpdateCellsData(updateCellValuesList, mExcelDataTable, filter, fileName, HeaderRowNum);
        }

        private bool UpdateCellsData(List<Tuple<string, object>> updateCellList, DataTable mExcelDataTable, string filter, string fileName, string HeaderRowNum)
        {
            if (updateCellList.Count > 0)
            {
                foreach (var cell in updateCellList)
                {
                    int columnIndex = mExcelDataTable.Columns[cell.Item1.Replace("[", "").Replace("]", "").Trim()].Ordinal;
                    List<DataRow> filteredList = mExcelDataTable.Select(filter).ToList();
                    foreach (DataRow objDataRow in filteredList)
                    {
                        int rowIndex = mExcelDataTable.Rows.IndexOf(objDataRow) + int.Parse(HeaderRowNum);
                        if (mSheet.GetRow(rowIndex) != null)
                        {
                            ICell targetCell = mSheet.GetRow(rowIndex).GetCell(columnIndex);
                            if (targetCell == null)
                            {
                                targetCell = mSheet.GetRow(rowIndex).CreateCell(columnIndex);
                            }
                            targetCell.SetCellValue(cell.Item2.ToString().Trim());
                        }
                    }
                }
                lock (lockObj)
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Create))
                    {
                        mWorkbook.Write(fs);
                        fs.Close();
                    }
                }
                return true;
            }
            return false;
        }

        public List<string> GetSheets(string fileName)
        {
            lock (lockObj)
            {
                Thread.Sleep(100);
                List<string> sheets = [];
                mFileName = fileName;
                GetExcelWorkbook(mFileName);
                if (mWorkbook == null)
                {
                    return sheets;
                }
                for (int i = 0; i < mWorkbook.NumberOfSheets; i++)
                {
                    sheets.Add(mWorkbook.GetSheetAt(i).SheetName);
                }
                return sheets.OrderBy(itm => itm).ToList();
            }
        }

        public void Dispose()
        {
            mSheet = null;
            mWorkbook?.Close();
            mWorkbook = null;
        }

        private object HandleNumericCellType(ICell cell)
        {
            if (DateUtil.IsCellDateFormatted(cell))
            {
                return cell.DateCellValue.ToString(CultureInfo.InvariantCulture);
            }
            else if (cell.CellStyle.GetDataFormatString().Contains("yy") || cell.CellStyle.GetDataFormatString().Contains("mm") || cell.CellStyle.GetDataFormatString().Contains("dd"))
            {
                return cell.DateCellValue.ToString(cell.CellStyle.GetDataFormatString().Replace("-mm", "-MM").Replace("h", "H"), CultureInfo.InvariantCulture);
            }
            else if ((cell.NumericCellValue.ToString().Length > 15 || String.Equals(cell.CellStyle.GetDataFormatString(), "General", StringComparison.OrdinalIgnoreCase)))
            {
                return ((decimal)cell.NumericCellValue).ToString(CultureInfo.InvariantCulture);
            }
            return cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
        }

        // This function returns the Column Titles (Header Columns)
        private IRow GetHeaderRow(ISheet sheet, int headerRowNumber = 1)
        {
            IRow header = sheet.GetRow(headerRowNumber - 1);
            if (header == null)
            {
                throw new InvalidDataException($"Could not Find Header Columns at the Row Number : {headerRowNumber}, Please Enter the Appropriate Header Row Number");
            }
            return header;
        }


        private void SetHeaderColumns(IRow headerRow, ref int initialColNumber, DataTable dtExcelTable)
        {
            int colCount = headerRow.LastCellNum;
            for (var c = 0; c < colCount; c++)
            {
                ICell cell = headerRow.GetCell(c);
                if (cell != null)
                {
                    if (initialColNumber == -1)
                    {
                        initialColNumber = c;
                    }
                    dtExcelTable.Columns.Add(GingerCoreNET.GeneralLib.General.RemoveSpecialCharactersInColumnHeader(cell.ToString()).Trim());
                }
                else
                {
                    dtExcelTable.Columns.Add("Col " + c);
                }
            }
        }
        /// <summary>
        ///  Collects the row data (Apart from the Column Header)
        /// </summary>
        /// <param name="sheet">A Particular Sheet on Excel</param>
        /// <param name="dtExcelTable">The Row Data will be collected in this</param>
        /// <param name="initialColNumber">Used to locate the first Column Number from where the row data exists</param>
        /// <param name="startRowNumber">Used to locate the first Row Number from where the row data begins</param>
        /// <param name="rowLimit">If the 'View Data / View Filtered Data' is selected on the Excel Action Page, the rowLimit is set , which means the user will only see AT MOST 'rowLimit' number of rows apart from the Column Header row </param>

        private void SetRowsForDataTable(ISheet sheet, DataTable dtExcelTable, int initialColNumber, int startRowNumber, int rowLimit)
        {
            var currentRowNumber = startRowNumber;
            var currentRow = sheet.GetRow(currentRowNumber);
            while (HasDataTableReachedRowLimit(currentRow, rowLimit, startRowNumber, currentRowNumber))
            {
                var dr = dtExcelTable.NewRow();
                for (var currentColNumber = initialColNumber; currentColNumber < (initialColNumber + dr.ItemArray.Length); currentColNumber++)
                {
                    var cell = currentRow.GetCell(currentColNumber);
                    if (cell != null)
                    {
                        dr[currentColNumber - initialColNumber] = GetCellValue(cell, cell.CellType);
                    }
                }
                dtExcelTable.Rows.Add(dr);
                currentRowNumber++;
                currentRow = sheet.GetRow(currentRowNumber);
            }

        }
        /// <summary>
        /// This function checks if the Reader has reached the row limit. if the 'View Data / View Filtered Data is selected it stops after the 'currentRowNumber' has reached 'rowLimit' or  the row length of the Sheet is lesser than the rowLimit
        /// in order case it only checks if the excel sheet has no more data to be read.
        /// </summary>
        /// <param name="currentRow"></param>
        /// <param name="rowLimit">If the 'View Data / View Filtered Data' is selected on the Excel Action Page, the rowLimit is set , which means the user will only see AT MOST 'rowLimit' number of rows apart from the Column Header row</param>
        /// <param name="startRowNumber">Start Row Number in the Excel Sheet </param>
        /// <param name="currentRowNumber">This variable denotes the current row , that is being read</param>
        private bool HasDataTableReachedRowLimit(IRow currentRow, int rowLimit, int startRowNumber, int currentRowNumber)
        {
            return (rowLimit == -1) ? currentRow != null : currentRow != null && (startRowNumber + rowLimit - currentRowNumber) > 0;
        }

        /// <summary>
        /// This method writes data into a specific cell in excel sheet
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sheetName"></param>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <param name="headerRowNumber"></param>
        /// <returns></returns>
        public bool WriteCellData(string fileName, string sheetName, string address, string value, string headerRowNumber)
        {
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook workbook = null;
                    if (fileName.EndsWith(".xlsx"))
                    {
                        workbook = new XSSFWorkbook(fs);
                    }
                    else if (fileName.EndsWith(".xls"))
                    {
                        workbook = new HSSFWorkbook(fs);
                    }

                    if (workbook == null)
                    {
                        return false;
                    }

                    ISheet sheet = workbook.GetSheet(sheetName);
                    if (sheet == null)
                    {
                        return false;
                    }

                    // Using NPOI CellReference to parse "A5" -> Row 4, Col 0
                    NPOI.SS.Util.CellReference cellRef = new NPOI.SS.Util.CellReference(address);
                    IRow row = sheet.GetRow(cellRef.Row) ?? sheet.CreateRow(cellRef.Row);
                    ICell cell = row.GetCell(cellRef.Col) ?? row.CreateCell(cellRef.Col);

                    // Set Value
                    cell.SetCellValue(value);

                    // Force formula recalculation if needed
                    sheet.ForceFormulaRecalculation = true;

                    // Save
                    using (FileStream fsOut = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        workbook.Write(fsOut);
                    }
                    workbook.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing cell data: " + ex.Message);
                return false;
            }
        }
    }
}