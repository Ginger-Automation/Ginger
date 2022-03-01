#region License
/*
Copyright © 2014-2022 European Support Limited

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
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Amdocs.Ginger.CoreNET.ActionsLib
{
    public class ExcelNPOIOperations : IExcelOperations
    {
        string mFileName { get; set; }
        DataTable mExcelDataTable { get; set; }
        DataTable mFilteredDataTable { get; set; }
        IWorkbook mWorkbook { get; set; }
        ISheet mSheet { get; set; }
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
                {
                    if (headerRow.GetCell(c) == null)
                    {
                        dtExcelTable.Columns.Add("Col " + c);
                        continue;
                    }
                    if (!dtExcelTable.Columns.Contains(headerRow.GetCell(c).ToString()))
                    {
                        dtExcelTable.Columns.Add(RemoveSpecialCharactersInColumnHeader(headerRow.GetCell(c).ToString()));
                    }
                }
                var i = 1;
                var currentRow = sheet.GetRow(i);
                while (currentRow != null)
                {
                    var dr = dtExcelTable.NewRow();
                    for (var j = 0; j < dr.ItemArray.Length; j++)
                    {
                        var cell = currentRow.GetCell(j);
                        if (cell != null)
                        {
                            dr[j] = GetCellValue(cell, cell.CellType);
                        }
                    }
                    dtExcelTable.Rows.Add(dr);
                    i++;
                    currentRow = sheet.GetRow(i);
                }
                return dtExcelTable;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "Can't convert sheet to data, " + ex.Message);
                return null;
            }
        }

        private object GetCellValue(ICell cell, CellType cellType)
        {
            object cellVal;
            switch(cellType)
            {
                case CellType.Numeric:
                    cellVal = DateUtil.IsCellDateFormatted(cell)
                        ? cell.DateCellValue.ToString(CultureInfo.InvariantCulture)
                        : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
                    break;
                case CellType.String:
                    cellVal = cell.StringCellValue;
                    break;
                case CellType.Boolean:
                    cellVal = cell.BooleanCellValue;
                    break;
                case CellType.Formula:
                    cellVal = GetCellValue(cell, cell.CachedFormulaResultType);
                    break;
                case CellType.Blank:
                    cellVal = null;
                    break;
                default:
                    cellVal = cell.RichStringCellValue;
                    break;
            }
            return cellVal;
        }

        public DataTable ReadData(string fileName, string sheetName, string filter, bool selectedRows)
        {
            filter = filter ?? "";
            try
            {
                if (!GetExcelSheet(fileName, sheetName))
                {
                    return null;
                }
                mExcelDataTable = ConvertSheetToDataTable(mSheet);
                mExcelDataTable.DefaultView.RowFilter = filter;
                mFilteredDataTable = GetFilteredDataTable(mExcelDataTable, selectedRows);
                return mFilteredDataTable;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "Can't read sheet data, " + ex.Message);
                return null;
            }
        }

        private bool GetExcelSheet(string fileName, string sheetName)
        {
            mWorkbook = GetExcelWorkbook(fileName);
            if (mWorkbook == null)
            {
                Reporter.ToLog(eLogLevel.WARN, "File name not Exists.");
                return false;
            }
            mSheet = mWorkbook.GetSheet(sheetName);
            if (mSheet == null)
            {
                Reporter.ToLog(eLogLevel.WARN, "Sheet name not Exists.");
                return false;
            }
            return true;
        }

        private DataTable GetFilteredDataTable(DataTable dataTable, bool selectAllRows)
        {
            return selectAllRows ? dataTable.DefaultView.ToTable() : dataTable.DefaultView.ToTable().AsEnumerable().Take(1).CopyToDataTable();
        }

        public IWorkbook GetExcelWorkbook(string fullFilePath)
        {
            IWorkbook workbook = null;
            try
            {
                using (var fs = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read))
                {
                    workbook = WorkbookFactory.Create(fs);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Invalid Excel Path Name" + fullFilePath, ex);
                return null;
            }
            return workbook;
        }

        public bool UpdateExcelData(string fileName, string sheetName, string filter, List<Tuple<string, object>> updateCellValuesList, string primaryKey = null, string key = null)
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
                UpdateCellsData(updateCellValuesList, mExcelDataTable, filter, fileName);
            }
            return true;
        }

        public DataTable ReadCellData(string fileName, string sheetName, string filter, bool selectedRows)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return ReadData(fileName, sheetName, filter, selectedRows);
            }
            Regex regex = new Regex(@"(^[A-Z]+\d+$)|(^[A-Z]+\d+:[A-Z]+\d+$)");
            Match match = regex.Match(filter);
            if (!match.Success)
            {
                return null;
            }
            if (!GetExcelSheet(fileName, sheetName))
            {
                return null;
            }
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
            var headerRow = mSheet.GetRow(0);
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
                        switch (cell.CellType)
                        {
                            case CellType.Numeric:
                                dr[dtColCount] = DateUtil.IsCellDateFormatted(cell)
                                    ? cell.DateCellValue.ToString(CultureInfo.InvariantCulture)
                                    : cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
                                break;
                            case CellType.String:
                                dr[dtColCount] = cell.StringCellValue;
                                break;
                            case CellType.Blank:
                                dr[dtColCount] = null;
                                break;
                            default:
                                dr[dtColCount] = cell.RichStringCellValue;
                                break;
                        }
                    }
                    dtColCount++;
                }
                dtExcelTable.Rows.Add(dr);
                i++;
                currentRow = mSheet.GetRow(i);
            }
            return dtExcelTable;
        }

        public bool WriteData(string fileName, string sheetName, string filter, string setDataUsed, List<Tuple<string, object>> updateCellValuesList, string primaryKey = null, string key = null)
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
            return UpdateCellsData(updateCellValuesList, mExcelDataTable, filter, fileName);
        }

        private bool UpdateCellsData(List<Tuple<string, object>> updateCellList, DataTable mExcelDataTable, string filter, string fileName)
        {
            if (updateCellList.Count > 0)
            {
                foreach (var cell in updateCellList)
                {
                    int columnIndex = mExcelDataTable.Columns[cell.Item1.Replace("[", "").Replace("]", "").Trim()].Ordinal;
                    List<DataRow> filteredList = mExcelDataTable.Select(filter).ToList();
                    foreach (DataRow objDataRow in filteredList)
                    {
                        int rowIndex = mExcelDataTable.Rows.IndexOf(objDataRow) + 1;
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
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    mWorkbook.Write(fs);
                    fs.Close();
                }
                return true;
            }
            return false;
        }

        public List<string> GetSheets(string fileName)
        {
            List<string> sheets = new List<string>();
            mFileName = fileName;
            var wb = GetExcelWorkbook(mFileName);
            if (wb == null)
            {
                return sheets;
            }
            for (int i = 0; i < wb.NumberOfSheets; i++)
            {
                sheets.Add(wb.GetSheetAt(i).SheetName);
            }
            return sheets.OrderBy(itm => itm).ToList();
        }

        public void Dispose()
        {
            mSheet = null;
            mWorkbook.Close();
            mWorkbook = null;
        }
        private string RemoveSpecialCharactersInColumnHeader(string columnHeader)
        {
            string specialCharactersToRemove = "./[]()";
            foreach (char sc in specialCharactersToRemove)
            {
                if (columnHeader.Contains(sc.ToString()))
                {
                    columnHeader = columnHeader.Replace(sc.ToString(), string.Empty);
                }
            }
            return columnHeader.Trim();
        }
    }
}
