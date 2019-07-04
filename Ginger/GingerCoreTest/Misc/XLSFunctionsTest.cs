#region License
/*
Copyright © 2014-2019 European Support Limited

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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace UnitTests.NonUITests
{
    
    [TestClass]
    [Ignore]
    public class XLSFunctionsTest
    {
        BusinessFlow mBF;
        GingerRunner mGR;
        string XLSFile;

        [TestInitialize]

        public void TestInitialize()
        {

            XLSFile = TestResources.GetTestResourcesFile("Names.xlsx");

            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF Excel";
            mBF.Active = true;
            Activity aaa = new Activity();
            aaa.Active=true;
            aaa.ActivityName="Activity 1";
            mBF.Activities.Add(aaa);

            Platform p = new Platform();
            p.PlatformType = ePlatformType.Web;            
           
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "App1" });

            mGR = new GingerRunner();
            Platform p2 = new Platform() { PlatformType = ePlatformType.Web };
            Agent a = new Agent();
            //a.Driver = new InternalBrowser(mBF);
            a.DriverType = Agent.eDriverType.SeleniumFireFox;
            p2.Agent = a;

            mGR.SolutionAgents = new ObservableList<Agent>();
            mGR.SolutionAgents.Add(a); 
            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "App1", Agent = a });
            // mGR.Platforms.Add(p2);
            mGR.BusinessFlows.Add(mBF);
        }

        //[TestMethod]  [Timeout(60000)]
        //public void OLD_ReadDataFromXLS()
        //{
        //    //TODO:  Obsolete remove old style


        //    // Arrange
        //    ExcelFunctions xf = new ExcelFunctions();

        //    //Act
        //    string XLSFile =  Common.getGingerUnitTesterDocumentsFolder() + "Names.xlsx";
        //    DataTable dt = xf.GetExcelSheetData(XLSFile, "Sheet1");

        //    //Assert
        //   Assert.AreEqual(dt.Rows.Count,98);

        //}
        [Level2]
        [TestMethod]  [Timeout(60000)]
        public void ReadDataFromXLSGetRowCount()
        {            
            // Arrange
            ActExcel actX = new ActExcel();
            
            //Act
            string XLSFile = TestResources.GetTestResourcesFile("Names.xlsx");
            // actX.ExcelFileName = XLSFile;
            actX.AddOrUpdateInputParamCalculatedValue("ExcelFileName", XLSFile);
            actX.AddOrUpdateInputParamCalculatedValue("SheetName", "Sheet1");
            // actX.SheetName = "Sheet1";
            DataTable dt = actX.GetExcelSheetData(null);

            //Assert
           Assert.AreEqual(dt.Rows.Count, 98); ;
           

        }

        /// <summary>
        /// Functionality checked: 1) read 1 val from Excel; 2) validate return value in action; 3) populate variable w return value
        /// </summary>
        
        //[TestMethod]  [Timeout(60000)]
        //public void Read1ValueValidateAndPopulateVariable()
        //{

        //    //TODO: add logic for making sure required data is always present in spreadsheet.
        //    // DATA
        //    string mExpectedValue = "ZZZJohn";
        //    string mColumnNameInExcel="First";
        //    string mFileName = "Names.xlsx";
        //    string mSheetName = "Sheet1";
        //    string mSelectRowsWhere = "ID=1";
        //    string mPrimaryKeyColumn = "ID";
        //    ActExcel.eExcelActionType mExcelActionType = ActExcel.eExcelActionType.ReadData;             

        //    //VARIABLE
        //    GingerCore.Variables.VariableString v = new GingerCore.Variables.VariableString();
        //    v.Name="Var1";
        //    //mBF.Variables.Add(v);
        //    mBF.AddVariable(v);
            
        //    //ACT        
        //    string XLSFile = Common.getGingerUnitTesterDocumentsFolder() + mFileName;
        //    ActExcel actX = new ActExcel();
        //    actX.Active = true;
        //    actX.AddNewReturnParams = true;
        //    actX.ExcelActionType = mExcelActionType;
        //    actX.LocateBy = eLocateBy.NA;
        //    actX.ExcelFileName = XLSFile;
        //    actX.SheetName = mSheetName;
        //    actX.SelectRowsWhere = mSelectRowsWhere;
        //    actX.PrimaryKeyColumn = mPrimaryKeyColumn;
        //    // validate return value & populate variable w it
        //    actX.AddOrUpdateReturnParamExpected(mColumnNameInExcel, mExpectedValue);
        //    actX.ReturnValues[actX.ReturnValues.Count-1].StoreToVariable=v.Name;

        //    //DataTable dt = actX.GetExcelSheetData(actX.SelectRowsWhere);
        //    //Assert.AreEqual(dt.Rows[0].ItemArray[1], mExpectedValue); // 0= ID; 1=First; 2=Last; 3=Used

        //    mBF.Activities[0].Acts.Add(actX);

        //    mGR.RunRunner();            

        //    //Assert
        //   Assert.AreEqual(actX.ReturnValues[0].Status, ActReturnValue.eStatus.Passed); //return value from Excel was expected
        //   Assert.AreEqual(v.Value, mExpectedValue); // variable was successfully populated from Excel

        //}

        private void ResetBusinessFlow()
        {
            mBF.Activities.Clear();
            mBF.RunStatus = eRunStatus.Pending;
        }
    }
 }
