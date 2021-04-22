using DocumentFormat.OpenXml.Spreadsheet;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GingerCoreNETUnitTest.LinuxTransformationTests
{
    [TestClass]
    public class ActExcelTests
    {
        [TestMethod]
        public void WriteExcel()
        {
            XSSFWorkbook hssfwb;
            using (FileStream file = new FileStream(TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "ExcelFile.xlsx"), FileMode.Create, FileAccess.Write))
            {
                IWorkbook workbook = new XSSFWorkbook();


                ISheet sheet = workbook.CreateSheet("Sheet1");
                IRow row = sheet.CreateRow(1);
                row.CreateCell(8).SetCellValue("Hello world");
                workbook.Write(file);
            }
        }
        [TestMethod]
        public void ReadExcel()
        {
            XSSFWorkbook hssfwb;
            using (FileStream file = new FileStream(@"C:\Ginger\Ginger-Linux-Transformation-Testing\Documents\WOEI\WOEIExcel.xlsx", FileMode.Open, FileAccess.Read))
            {
                hssfwb = new XSSFWorkbook(file);
            }

            ISheet sheet = hssfwb.GetSheet("Data");
            DataFormatter formatter = new DataFormatter();
            for (int row = 0; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null) //null is when the row only contains empty cells 
                {
                    Console.WriteLine(formatter.FormatCellValue(sheet.GetRow(row).GetCell(0)));
                }
            }
        }
    }
}
