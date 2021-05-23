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
        public string FileName { get; set; }
        DataTable ExcelDataTable { get; set; }
        DataTable FilteredDataTable { get; set; }
        List<Tuple<string, object>> UpdateCellList { get; set; }
        IWorkbook Workbook { get; set; }
        ISheet Sheet { get; set; }

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
                    if (!dtExcelTable.Columns.Contains(headerRow.GetCell(c).ToString()))
                    {
                        dtExcelTable.Columns.Add(headerRow.GetCell(c).ToString());
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
                                    dr[j] = string.Empty;
                                    break;
                                default:
                                    dr[j] = cell.RichStringCellValue;
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

        public DataTable ReadData(string fileName, string sheetName, string filter, bool selectedRows, string primaryKey, string setData)
        {
            try
            {
                Workbook = GetExcelWorkbook(amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(fileName));
                Sheet = Workbook.GetSheet(sheetName);
                if (Sheet == null)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Incorrect Sheet name.");
                    return null;
                }
                ExcelDataTable = ConvertSheetToDataTable(Sheet);
                ExcelDataTable.DefaultView.RowFilter = filter ?? "";
                FilteredDataTable = GetFilteredDataTable(ExcelDataTable, selectedRows);
                UpdateCellList = GetSetDataUsed(setData);
                return FilteredDataTable;
            }
            catch(Exception ex)
            {
                throw;
            }
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
                    string rowToSet = setData[0].Replace("[|]", "");
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
                switch (fileExtension.ToLower())
                {
                    case ".xlsx":
                        using (var fs = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read))
                        {
                            workbook = new XSSFWorkbook(fs);
                        }
                        break;
                    case ".xls":
                        using (var fs = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read))
                        {
                            workbook = new HSSFWorkbook(fs);
                        }
                        break;
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Invalid file path: " + fullFilePath + ", " + ex.Message);
            }
            return workbook;
        }

        public bool updateExcelData(string fileName, string sheetName, string filter, string setDataUsed, string primaryKey = null, string key = null)
        {
            if (UpdateCellList.Count > 0)
            {
                var headerRow = Sheet.GetRow(0);
                foreach(string colName in UpdateCellList.Select(x => x.Item1))
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
                ExcelDataTable.Select(filter).ToList().ForEach(dr =>
                Sheet.GetRow(ExcelDataTable.Rows.IndexOf(dr) + 1).GetCell(ExcelDataTable.Columns[x.Item1].Ordinal).SetCellValue((string)x.Item2)));
                
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    Workbook.Write(fs);
                    fs.Close();
                }
            }
            return true;
        }

        public DataTable ReadCellData(string fileName, string sheetName, string filter, bool selectedRows)
        {
            fileName = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(fileName);
            if (string.IsNullOrEmpty(filter))
            {
                return ReadData(fileName, sheetName, filter, selectedRows, "", "");
            }
            Regex regex = new Regex(@"(^[A-Z]+\d+$)|(^[A-Z]+\d+:[A-Z]+\d+$)");
            Match match = regex.Match(filter);
            if (match.Success)
            {
                Console.WriteLine("MATCH VALUE: " + match.Value);
            }
            Workbook = GetExcelWorkbook(fileName);
            Sheet = Workbook.GetSheet(sheetName);
            if(Sheet == null)
            {
                throw new Exception("Invalid Sheet name");
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
            try
            {
                var dtExcelTable = new DataTable();
                dtExcelTable.Rows.Clear();
                dtExcelTable.Columns.Clear();
                var headerRow = Sheet.GetRow(0);
                int colCount = headerRow.LastCellNum;
                if(cellFrom.Col > colCount || cellTo.Col > colCount)
                {
                    throw new Exception("Invalid filter");
                }
                for (var c = cellFrom.Col; c <= cellTo.Col; c++)
                {
                    if (headerRow.GetCell(c) != null && !dtExcelTable.Columns.Contains(headerRow.GetCell(c).ToString()))
                    {
                        dtExcelTable.Columns.Add(headerRow.GetCell(c).ToString());
                    }
                }
                var i = cellFrom.Row;
                var currentRow = Sheet.GetRow(i);
                int dtColCount = 0;
                while (i <= cellTo.Row)
                {
                    var dr = dtExcelTable.NewRow();
                    dtColCount = 0;
                    for (var j = cellFrom.Col; j <= cellTo.Col; j++)
                    {
                        ICell cell;
                        if(currentRow != null)
                        {
                            cell = currentRow.GetCell(j);
                        }
                        else{
                            throw new Exception("Invalid row number");
                        }

                        if (cell != null)
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
                                    dr[dtColCount] = string.Empty;
                                    break;
                            }
                        dtColCount++;
                    }
                    dtExcelTable.Rows.Add(dr);
                    i++;
                    currentRow = Sheet.GetRow(i);
                }
                return dtExcelTable;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public bool WriteData(string fileName, string sheetName, string filter, string setDataUsed, List<Tuple<string, object>> updateCellValuesList, string primaryKey = null, string key = null)
        {
            UpdateCellList.AddRange(updateCellValuesList);
            if (UpdateCellList.Count > 0)
            {
                UpdateCellList.ForEach(x =>
                ExcelDataTable.Select(filter).ToList().ForEach(dr =>
                Sheet.GetRow(ExcelDataTable.Rows.IndexOf(dr) + 1).GetCell(ExcelDataTable.Columns[x.Item1].Ordinal).SetCellValue((string)x.Item2)));
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    Workbook.Write(fs);
                    fs.Close();
                }
                return true;
            }
            return false;
        }

        

        public List<string> GetSheets()
        {
            List<string> sheets = new List<string>();
            var wb = GetExcelWorkbook(FileName);
            for (int i = 0; i < wb.NumberOfSheets; i++)
            {
                sheets.Add(wb.GetSheetAt(i).SheetName);
            }
            return sheets;
        }
    }
}
