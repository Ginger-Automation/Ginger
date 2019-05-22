using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Ginger.Run;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIExcel : ICLI
    {
        public string Identifier => throw new NotImplementedException();

        public string FileExtension => throw new NotImplementedException();

        public string CreateContent(RunsetExecutor runsetExecutor)
        {
            //zzz !!!!!!
            func1(@"C:\Yaron\AQE 2019\Ginger\DynamicRunSet.xlsx", "2 lines");
            return "aaa";
        }

        public void Execute(RunsetExecutor runsetExecutor)
        {
            throw new NotImplementedException();
        }

        public void LoadContent(string content, CLIHelper cliHelper, RunsetExecutor runsetExecutor)
        {
            throw new NotImplementedException();
        }

        void func1(string fileName, string sheetName)
        {            
            List<string> list = new List<string>();

            // Open the spreadsheet document for read-only access.
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(fileName, false))
            {
                IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>().Where(s => s.Name == sheetName);
                string relationshipId = sheets.First().Id.Value;
                WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(relationshipId);
                var rows = worksheetPart.Worksheet.GetFirstChild<SheetData>().Elements<Row>();

                var tempRow = worksheetPart.Worksheet.GetFirstChild<SheetData>().Elements<Row>().FirstOrDefault();
                var tempcols = tempRow.Elements<Cell>();
                var colsCount = tempRow.Elements<Cell>().Count();

                string val0 = GetCurrentCellValue(document, tempcols.ElementAt(0));
                string val1 = GetCurrentCellValue(document, tempcols.ElementAt(1));
                string val2 = GetCurrentCellValue(document, tempcols.ElementAt(2));
                string val3 = GetCurrentCellValue(document, tempcols.ElementAt(3));
            }
        }


        private string GetCurrentCellValue(SpreadsheetDocument document, Cell cell)
        {
            string value = string.Empty;
            try
            {
                SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
                value = cell.CellValue.InnerXml;
                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                {
                    value = stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return value;
        }

    }
}
