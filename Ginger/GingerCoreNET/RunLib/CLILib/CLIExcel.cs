#region License
/*
Copyright © 2014-2023 European Support Limited

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

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Ginger.Run;
using Ginger.SolutionGeneral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIExcel : ICLI
    {
        bool ICLI.IsFileBasedConfig { get { return true; } }

        public string Verb => throw new NotImplementedException();

        public string FileExtension => throw new NotImplementedException();

        public string CreateConfigurationsContent(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            //zzz !!!!!!
            func1(@"C:\Yaron\AQE 2019\Ginger\DynamicRunSet.xlsx", "2 lines");
            return "aaa";
        }

        public async Task Execute(RunsetExecutor runsetExecutor)
        {
            throw new NotImplementedException();
        }

        public void LoadGeneralConfigurations(string content, CLIHelper cliHelper)
        {
            throw new NotImplementedException();
        }

        public void LoadRunsetConfigurations(string content, CLIHelper cliHelper, RunsetExecutor runsetExecutor)
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
                throw ex;
            }
            return value;
        }
    }
}
