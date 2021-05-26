using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.ActionsLib;
using Amdocs.Ginger.CoreNET.Repository;
using GingerCore.Actions;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.IO;
using System.Linq;

namespace GingerCoreNETUnitTest.LinuxTransformationTests
{
    [TestClass]
    public class ActExcelTests
    {
        string excelPathRead = TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx");
        string excelPathWrite = TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "NamesWrite.xlsx");
        string excelPathWriteTemp = TestResources.GetTempFile("NamesWriteTemp.xlsx");
        [TestInitialize]
        public void TestInitialize()
        {
            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
            File.Copy(excelPathWrite, excelPathWriteTemp,true);
        }
        
        [TestMethod]
        public void ReadExcelFirstRowTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ExcelFileName),
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SheetName), "Sheet1");
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
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ExcelFileName),
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SheetName), "Sheet1");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SelectRowsWhere), "Last='Cohen'");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.ReadData;
            actExcel.AddNewReturnParams = true;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreEqual(string.Join(',',actExcel.ActReturnValues.Select(x=> x.Actual).ToList()), "1,Mark,Cohen,2109 Fox Dr");
        }

        [TestMethod]
        public void ReadExcelAllRowsTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ExcelFileName),
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SelectRowsWhere), "Last='Cohen'");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SheetName), "Sheet1");
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
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ExcelFileName),
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SheetName), "Sheet1");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SelectRowsWhere), "Last='Cohen'");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.ReadData;
            actExcel.AddNewReturnParams = true;
            actExcel.SelectAllRows = true;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Actual).ToList()), "1,Mark,Cohen,2109 Fox Dr");
        }
        [TestMethod]
        public void ReadExcelAllWithFilterRowsSetDataTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ExcelFileName),
                TestResources.GetTestResourcesFile(excelPathWriteTemp));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SheetName), "Sheet1");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.PrimaryKeyColumn), "ID");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SelectRowsWhere), "Last='Cohen'");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue("SetDataUsed", "First='Jhon'");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.ReadData;
            actExcel.AddNewReturnParams = true;
            actExcel.SelectAllRows = true;

            //Act
            actExcel.Execute();

            //Assert
            IExcelOperations excelOperations = new ExcelNPOIOperations();
            DataTable dt = excelOperations.ReadData(excelPathWriteTemp, actExcel.SheetName, actExcel.SelectRowsWhere, actExcel.SelectAllRows);
            Assert.AreEqual(string.Join(',', dt.Rows[0].ItemArray.Select(x => x).ToList()), "1,Jhon,Cohen,2109 Fox Dr");
        }
        [TestMethod]
        public void ReadCellDataExcelOneCellTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ExcelFileName),
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SheetName), "Sheet1");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SelectRowsWhere), "A2");
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
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ExcelFileName),
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SheetName), "Sheet1");
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
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ExcelFileName),
                TestResources.GetTestResourcesFile(excelPathWriteTemp));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SheetName), "Sheet1");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SelectRowsWhere), "Last='Cohen'");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.PrimaryKeyColumn), "ID");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ColMappingRules), "First='Marco'");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.WriteData;

            //Act
            actExcel.Execute();

            //Assert
            IExcelOperations excelOperations = new ExcelNPOIOperations();
            DataTable dt = excelOperations.ReadData(excelPathWriteTemp, actExcel.SheetName, actExcel.SelectRowsWhere, false);
            Assert.AreEqual(string.Join(',', dt.Rows[0].ItemArray.Select(x => x).ToList()), "1,Marco,Cohen,2109 Fox Dr");
        }
        [TestMethod]
        public void WriteExcelMultiRowsWithFilterTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.RunOnBusinessFlow = new GingerCore.BusinessFlow();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ExcelFileName),
                TestResources.GetTestResourcesFile(excelPathWriteTemp));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SheetName), "Sheet1");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SelectRowsWhere), "Last='Cohen'");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ColMappingRules), "First='Simon'");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.WriteData;
            actExcel.SelectAllRows = true;

            //Act
            actExcel.Execute();

            //Assert
            IExcelOperations excelOperations = new ExcelNPOIOperations();
            DataTable dt = excelOperations.ReadData(excelPathWriteTemp, actExcel.SheetName, actExcel.SelectRowsWhere, actExcel.SelectAllRows);
            string actual = "";
            foreach (DataRow dr in dt.Rows)
            {
                actual += string.Join(',', dr.ItemArray.Select(x => x).ToList());
                actual += ",";
            }
            Assert.AreEqual(actual.TrimEnd(','), "1,Simon,Cohen,2109 Fox Dr,4,Simon,Cohen,NY");
        }
    }
}
