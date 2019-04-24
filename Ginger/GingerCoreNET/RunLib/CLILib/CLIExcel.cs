using Ginger.Run;
using System;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIExcel : ICLI
    {
        public string Identifier => throw new NotImplementedException();

        public string FileExtension => throw new NotImplementedException();

        public string CreateContent(RunsetExecutor runsetExecutor)
        {
            func1(@"C:\Yaron\AQE 2019\Ginger\DynamicRunSet.xlsx", "2 lines");
            return "aaa";
        }

        public void Execute(RunsetExecutor runsetExecutor)
        {
            throw new NotImplementedException();
        }

        public void LoadContent(string content, RunsetExecutor runsetExecutor)
        {
            throw new NotImplementedException();
        }

        void func1(string fileName, string sheetName)
        {
            //int rowIndex = 0;
            //int colIndex = 0;
            //List<string> list = new List<string>();

            //// Open the spreadsheet document for read-only access.
            //using (SpreadsheetDocument document = SpreadsheetDocument.Open(fileName, false))
            //{
            //    IEnumerable<Sheet> sheets = document.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>().Where(s => s.Name == sheetName);
            //    string relationshipId = sheets.First().Id.Value;
            //    WorksheetPart worksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(relationshipId);
            //    var rows = worksheetPart.Worksheet.GetFirstChild<SheetData>().Elements<Row>();

            //    var tempRow = worksheetPart.Worksheet.GetFirstChild<SheetData>().Elements<Row>().FirstOrDefault();
            //    var tempcols = tempRow.Elements<Cell>();
            //    var colsCount = tempRow.Elements<Cell>().Count();


            //}
        }

    }
}
