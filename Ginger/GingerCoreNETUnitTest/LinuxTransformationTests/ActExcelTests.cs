using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.ActionsLib;
using Amdocs.Ginger.CoreNET.Repository;
using DocumentFormat.OpenXml.Spreadsheet;
using GingerCore.Actions;
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
        public void ReadExcelFirstRowTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("ExcelFileName",
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SheetName","Sheet1");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.ReadData;
            actExcel.AddNewReturnParams = true;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreEqual(actExcel.ActReturnValues.Count,4);
        }
        [TestMethod]
        public void ReadExcelFirstRowWithFilterTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("ExcelFileName",
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SheetName", "Sheet1");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SelectRowsWhere", "Last='Cohen'");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.ReadData;
            actExcel.AddNewReturnParams = true;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreEqual(actExcel.ActReturnValues.Count, 4);
        }

        [TestMethod]
        public void ReadExcelAllRowsTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("ExcelFileName",
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SheetName", "Sheet1");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.ReadData;
            actExcel.AddNewReturnParams = true;
            actExcel.SelectAllRows = true;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreEqual(actExcel.ActReturnValues.Count, 12);
        }
        [TestMethod]
        public void ReadExcelAllWithFilterRowsTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("ExcelFileName",
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SheetName", "Sheet1");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SelectRowsWhere", "Last='Cohen'");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.ReadData;
            actExcel.AddNewReturnParams = true;
            actExcel.SelectAllRows = true;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreEqual(actExcel.ActReturnValues.Count, 4);
        }
        [TestMethod]
        public void ReadExcelAllWithFilterRowsSetDataTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("ExcelFileName",
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SheetName", "Sheet1");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("PrimaryKeyColumn", "ID");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SelectRowsWhere", "Last='Cohen'");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SetDataUsed", "First='Jhon'");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.ReadData;
            actExcel.AddNewReturnParams = true;
            actExcel.SelectAllRows = true;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreEqual(actExcel.ActReturnValues.Count, 4);
        }
        [TestMethod]
        public void ReadCellDataExcelOneCellTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("ExcelFileName",
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SheetName", "Sheet1");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.ReadCellData;
            actExcel.AddNewReturnParams = true;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreEqual(actExcel.ActReturnValues.Count, 1);
        }
        [TestMethod]
        public void ReadCellDataExcelAllCellTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("ExcelFileName",
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SheetName", "Sheet1");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.ReadCellData;
            actExcel.AddNewReturnParams = true;
            actExcel.SelectAllRows = true;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreEqual(actExcel.ActReturnValues.Count, 12);
        }
        [TestMethod]
        public void WriteExcelOneRowWithPKTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.RunOnBusinessFlow = new GingerCore.BusinessFlow();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("ExcelFileName",
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SheetName", "Sheet1");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SelectRowsWhere", "Last='Cohen'");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("PrimaryKeyColumn", "ID");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("ColMappingRules", "First='Marco'");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.WriteData;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreNotEqual(actExcel.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed);
        }
        [TestMethod]
        public void WriteExcelMultiRowsWithFilterTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.RunOnBusinessFlow = new GingerCore.BusinessFlow();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("ExcelFileName",
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "NamesWrite.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SheetName", "Sheet1");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SelectRowsWhere", "Last='Cohen'");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("ColMappingRules", "First='Marco'");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.WriteData;
            actExcel.SelectAllRows = true;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreNotEqual(actExcel.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed);
        }
    }
}
