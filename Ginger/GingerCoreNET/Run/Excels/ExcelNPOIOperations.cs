using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore.Actions;
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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Amdocs.Ginger.CoreNET.ActionsLib
{
    public class ExcelNPOIOperations : IExcelOperations
    {
        string mFileName { get; set; }
        DataTable mExcelDataTable { get; set; }
        DataTable mFilteredDataTable { get; set; }
        List<Tuple<string, object>> UpdateCellList { get; set; }
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
                        dtExcelTable.Columns.Add(headerRow.GetCell(c).ToString().Trim());
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
                                case CellType.Boolean:
                                    dr[j] = cell.BooleanCellValue;
                                    break;
                                case CellType.Formula:
                                    dr[j] = cell.CellFormula;
                                    break;
                                case CellType.Blank:
                                    dr[j] = null;
                                    break;
                                default:
                                    dr[j] = cell.RichStringCellValue;
                                    break;
                            }
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

        private List<Tuple<string, object>> GetSetDataUsed(string setDataUsed)
        {
            List<Tuple<string, object>> columnNameAndValue = new List<Tuple<string, object>>();
            if (String.IsNullOrEmpty(setDataUsed))
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
                    string rowToSet = setData[0].Replace("[", "").Replace("]", "");
                    object valueToSet = setData[1].Replace("'", "");
                    columnNameAndValue.Add(new Tuple<string, object>(rowToSet, valueToSet));
                }
                else
                {
                    Reporter.ToLog(eLogLevel.INFO, "Invalid data added to 'SetDataUsed' text box");
                    isError = true;
                }

            });
            return isError ? null : columnNameAndValue;
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
                var fileExtension = Path.GetExtension(fullFilePath);
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

        public bool UpdateExcelData(string fileName, string sheetName, string filter, string setDataUsed, string primaryKey = null, string key = null)
        {
            UpdateCellList = GetSetDataUsed(setDataUsed);
            if (UpdateCellList.Count > 0)
            {
                var headerRow = mSheet.GetRow(0);
                foreach (string colName in UpdateCellList.Select(x => x.Item1))
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
                UpdateCellList.ForEach(x =>
                mExcelDataTable.Select(filter).ToList().ForEach(dr =>
                mSheet.GetRow(mExcelDataTable.Rows.IndexOf(dr) + 1).GetCell(mExcelDataTable.Columns[x.Item1].Ordinal).SetCellValue((string)x.Item2)));

                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    mWorkbook.Write(fs);
                    fs.Close();
                }
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
            UpdateCellList = GetSetDataUsed(setDataUsed);
            UpdateCellList.AddRange(updateCellValuesList);
            if (!String.IsNullOrWhiteSpace(primaryKey))
            {
                if (string.IsNullOrWhiteSpace(filter))
                {
                    filter = primaryKey;
                }
                else
                {
                    filter = filter + $"and {primaryKey}";
                }
            }
            if (UpdateCellList.Count > 0)
            {
                //UpdateCellList.ForEach(x =>
                //mExcelDataTable.Select(filter).ToList().ForEach(dr =>
                //mSheet.GetRow(mExcelDataTable.Rows.IndexOf(dr) + 1).GetCell(mExcelDataTable.Columns[x.Item1.Replace("[","").Replace("]","").Trim()].Ordinal).SetCellValue(x.Item2.ToString().Trim())));

                foreach (var cell in UpdateCellList)
                {
                    List<DataRow> filteredList = mExcelDataTable.Select(filter).ToList();
                    foreach (DataRow objDataRow in filteredList)
                    {
                        if (mSheet.GetRow(mExcelDataTable.Rows.IndexOf(objDataRow) + 1) != null)
                        {
                            ICell targetCell = mSheet.GetRow(mExcelDataTable.Rows.IndexOf(objDataRow) + 1).
                                        GetCell(mExcelDataTable.Columns[cell.Item1.Replace("[", "").Replace("]", "").Trim()].Ordinal);
                            if (targetCell == null)
                            {
                                targetCell = mSheet.GetRow(mExcelDataTable.Rows.IndexOf(objDataRow) + 1).
                                            CreateCell(mExcelDataTable.Columns[cell.Item1.Replace("[", "").Replace("]", "").Trim()].Ordinal);
                            }
                            mSheet.GetRow(mExcelDataTable.Rows.IndexOf(objDataRow) + 1).GetCell(mExcelDataTable.Columns[cell.Item1.Replace("[", "").Replace("]", "").Trim()].Ordinal).SetCellValue(cell.Item2.ToString().Trim());
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
    }
}
