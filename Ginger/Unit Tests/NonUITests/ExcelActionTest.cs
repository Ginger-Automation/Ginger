using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Repository;
using Ginger.Actions;
using Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level3]
    public class ExcelActionTest
    {
        static BusinessFlow mBF;
        static GingerRunner mGR;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            AutoLogProxy.Init("NonDriverActionTests");
            RepositoryItemHelper.RepositoryItemFactory = new RepositoryItemFactory();

            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF Non-Driver Action Test";
            mBF.Active = true;

            Activity activity = new Activity();
            mBF.Activities.Add(activity);
            mBF.CurrentActivity = activity;

            mGR = new GingerRunner();
            mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();
            mGR.CurrentBusinessFlow = mBF;
            mGR.BusinessFlows.Add(mBF);

            Reporter.ToLog(eLogLevel.DEBUG, "Creating the GingerCoreNET WorkSpace");
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
        }

        [TestMethod]
        public void ReadExcelTest()
        {
            //Arrange
            ActExcel action = new ActExcel();
            action.ExcelActionType = ActExcel.eExcelActionType.ReadData;
            action.SheetName = "Sheet1";
            //action.SelectRowsWhere = "";
            action.SelectAllRows = true;
            action.PrimaryKeyColumn = "Id";
            action.ExcelFileName = TestResources.GetTestResourcesFile(@"EXCELS\TestExcel.xlsx");
            action.Active = true;
            action.AddNewReturnParams = true;
            //Act
           
            mGR.RunAction(action);
            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(10, action.ReturnValues.Count);
        }

        [TestMethod]
        public void WriteExcelTest()
        {
            //Arrange
            ActExcel action = new ActExcel();
            action.ExcelActionType = ActExcel.eExcelActionType.WriteData;
            action.SheetName = "Sheet1";
            action.SelectRowsWhere = "1";
            action.SelectAllRows = false;
            action.PrimaryKeyColumn = "Id";
            action.SetDataUsed="Used= 'Y'";
            action.ExcelFileName = TestResources.GetTestResourcesFile(@"EXCELS\TestExcel.xlsx");
            action.Active = true;
            action.ColMappingRules = "Id=4";
            //Act
            mGR.RunAction(action);
            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");

            //To check Successfully Write or not 
            //Arrange
            ActExcel action1 = new ActExcel();
            action1.ExcelActionType = ActExcel.eExcelActionType.ReadData;
            action1.SheetName = "Sheet1";
            action.SelectRowsWhere = "4";
            action1.SelectAllRows = false;
            action1.PrimaryKeyColumn = "Id";
            action1.ExcelFileName = TestResources.GetTestResourcesFile(@"EXCELS\TestExcel.xlsx");
            action1.Active = true;
            action1.AddNewReturnParams = true;
            //Act
            mGR.RunAction(action1);
            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(5, action1.ReturnValues.Count);


        }
    }
}
