using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.ActionsLib;
using Amdocs.Ginger.CoreNET.Repository;
using DocumentFormat.OpenXml.Spreadsheet;
using GingerCoreNETUnitTest.RunTestslib;
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
        [TestInitialize]
        public void TestInitialize()
        {
            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
        }
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
        [TestMethod]
        public void UpdateExcel()
        {
            XSSFWorkbook hssfwb;
            using (FileStream file = new FileStream(@"C:\Ginger\File Operations\Documents\WOEI\WOEIExcel.xlsx", FileMode.Open, FileAccess.Read))
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
            sheet.GetRow(2).GetCell(3).SetCellValue(1414);
            using (FileStream out1 = new FileStream(@"C:\Ginger\File Operations\Documents\WOEI\WOEIExcel.xlsx", FileMode.Create))
            {
                hssfwb.Write(out1);
                out1.Close();
            }
        }
        [TestMethod]
        public void ReadExcelFirstRowTest()
        {
            //Arrange            
            ActExcelNPOI actExcel = new ActExcelNPOI();
            //actFileOperation.AddOrUpdateInputParamValueAndCalculatedValue("SourceFilePath", TestResources.GetTestResourcesFile(@"TextFiles" + Path.DirectorySeparatorChar + "textFileToCopy.txt"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("ExcelFileName",
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SheetName","Data");
            actExcel.ExcelActionType = ActExcelNPOI.eExcelActionType.ReadData;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreNotEqual(actExcel.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed);
            //Assert.AreEqual(actExcel.ActReturnValues.)
        }
    }
}
