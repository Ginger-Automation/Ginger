using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.ActionsLib;
using Amdocs.Ginger.CoreNET.Repository;
using Ginger.SolutionGeneral;
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
            if (WorkSpace.Instance.Solution != null)
            {
                if (WorkSpace.Instance.Solution.SolutionOperations == null)
                {
                    WorkSpace.Instance.Solution.SolutionOperations = new SolutionOperations(WorkSpace.Instance.Solution);
                }
            }
            else
            {
                WorkSpace.Instance.Solution = new Solution();
                WorkSpace.Instance.Solution.SolutionOperations = new SolutionOperations(WorkSpace.Instance.Solution);
            }
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
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Actual).ToList()), "1,Mark,Cohen,2109 Fox Dr");
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Param).ToList()), "ID,First,Last,Address");
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
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Actual).ToList()), "1,Mark,Cohen,2109 Fox Dr");
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Param).ToList()), "ID,First,Last,Address");
        }

        [TestMethod]
        public void ReadExcelAllRowsTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ExcelFileName),
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SelectRowsWhere), "Last='Bond'");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SheetName), "Sheet1");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.ReadData;
            actExcel.AddNewReturnParams = true;
            actExcel.SelectAllRows = true;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Actual).ToList()), "3,Mike,Bond,AZ");
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Path).ToList()), "1,1,1,1");
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Param).ToList()), "ID,First,Last,Address");
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
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Path).ToList()), "1,1,1,1");
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Param).ToList()), "ID,First,Last,Address");
        }
        [TestMethod]
        public void ReadExcelAllWithFilterUsingQuotesTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ExcelFileName),
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SheetName), "Sheet1");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SelectRowsWhere), "First=\"Mark\"");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.ReadData;
            actExcel.AddNewReturnParams = true;
            actExcel.SelectAllRows = true;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Actual).ToList()), "1,Mark,Cohen,2109 Fox Dr");
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Path).ToList()), "1,1,1,1");
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Param).ToList()), "ID,First,Last,Address");
        }
        [TestMethod]
        public void ReadExcelGetFormulaValueTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ExcelFileName),
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SheetName), "Sheet1");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SelectRowsWhere), "First='Con'");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.ReadData;
            actExcel.AddNewReturnParams = true;
            actExcel.SelectAllRows = true;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Actual).ToList()), "4,Con,Cat,ConCat");
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Path).ToList()), "1,1,1,1");
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Param).ToList()), "ID,First,Last,Address");
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
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Actual).ToList()), "1,Mark,Cohen,2109 Fox Dr,4,Adam,Cohen,NY");
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Path).ToList()), "1,1,1,1,2,2,2,2");
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Param).ToList()), "ID,First,Last,Address,ID,First,Last,Address");
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
            Assert.AreEqual(actExcel.ActReturnValues[0].Actual, "1");
            Assert.AreEqual(actExcel.ActReturnValues[0].Param, "ID");
        }
        [TestMethod]
        public void ReadCellDataExcelAllCellTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ExcelFileName),
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "Names.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SheetName), "Sheet1");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SelectRowsWhere), "B2:D4");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.ReadCellData;
            actExcel.AddNewReturnParams = true;
            actExcel.SelectAllRows = true;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreEqual(actExcel.ActReturnValues.Count, 9);
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Actual).ToList()), "Mark,Cohen,2109 Fox Dr,Julia,Smith,LA,Mike,Bond,AZ");
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Path).ToList()), "11,12,13,21,22,23,31,32,33");
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Param).ToList()), "First,Last,Address,First,Last,Address,First,Last,Address");
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
                string current = string.Join(',', dr.ItemArray.Select(x => x).ToList());
                actual = string.Join(',', actual, current);
            }
            Assert.AreEqual(actual.TrimStart(','), "1,Simon,Cohen,2109 Fox Dr,4,Simon,Cohen,NY");
        }

        [TestMethod]
        public void ReadExcelFirstRowWithLongDigitsTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ExcelFileName),
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "LongDigitsTesting.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SheetName), "Sheet1");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.ReadData;
            actExcel.AddNewReturnParams = true;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreEqual(actExcel.ActReturnValues.Count, 2);
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Actual).ToList()), "30465673520871400000,758480194459400");
            Assert.AreEqual(string.Join(',', actExcel.ActReturnValues.Select(x => x.Param).ToList()), "SIM,IMEI");
        }

        [TestMethod]
        public void ReadCellDataExcelOneCellWithLongDigitsTest()
        {
            //Arrange            
            ActExcel actExcel = new ActExcel();
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.ExcelFileName),
                TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "LongDigitsTesting.xlsx"));
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SheetName), "Sheet1");
            actExcel.AddOrUpdateInputParamValueAndCalculatedValue(nameof(ActExcel.SelectRowsWhere), "A2");
            actExcel.ExcelActionType = ActExcel.eExcelActionType.ReadCellData;
            actExcel.AddNewReturnParams = true;

            //Act
            actExcel.Execute();

            //Assert
            Assert.AreEqual(actExcel.ActReturnValues.Count, 1);
            Assert.AreEqual(actExcel.ActReturnValues[0].Actual, "30465673520871400000");
            Assert.AreEqual(actExcel.ActReturnValues[0].Param, "SIM");
        }
    }
}
