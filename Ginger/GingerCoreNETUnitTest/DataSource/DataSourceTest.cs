#region License
/*
Copyright © 2014-2022 European Support Limited

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
using GingerCore.DataSource;
using GingerCoreNET.DataSource;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;
using GingerCore;
using Ginger.Run;
using GingerCore.Actions;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;

namespace UnitTests.NonUITests
{
    [TestClass]
    public class DataSourceTest
    {
        static GingerLiteDB liteDB = new GingerCoreNET.DataSource.GingerLiteDB();
        ObservableList<DataSourceTable> sourceTables;
        static string excelFilePath = "";
        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            string Connectionstring = "filename=" + TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "BasicSimple" + Path.DirectorySeparatorChar + "DataSources" + Path.DirectorySeparatorChar + "LiteDB.db") + "; mode=Exclusive";
            excelFilePath = TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "ExportedDS.xlsx");

            liteDB.FileFullPath = Connectionstring;


           
          
        }

        [TestMethod]
        public void GetTableList()
        {
            //Act 
            sourceTables = liteDB.GetTablesList();
            
            //Assert
            Assert.AreEqual(sourceTables.Count, 2);
        }

        [TestMethod]
        public void AddTableAndDeleteTable()
        {
            //Act : add table
            liteDB.AddTable("NewTableAs", "GINGER_ID, GINGER_KEY_NAME, GINGER_KEY_VALUE, GINGER_LAST_UPDATED_BY, GINGER_LAST_UPDATE_DATETIME");
            
            //Assert
            Assert.AreEqual(liteDB.IsTableExist("NewTableAs"), true);

            //Act : delete table
            liteDB.DeleteTable("NewTableAs");
            
            //Assert
            Assert.AreEqual(liteDB.IsTableExist("NewTableAs"), false);
        }

        [TestMethod]
        public void AddAndDeleteColunmfromTable()
        {
            //Arrange
            List<string> ColunmList;
            bool IsColunmExists = false;

            //Act :add colunm
            liteDB.AddColumn("MyCustomizedDataTable", "New", "Text");
            ColunmList = liteDB.GetColumnList("MyCustomizedDataTable");
            IsColunmExists = ColunmList.Any(s => s.Contains("New"));
            
            //Assert
            Assert.AreEqual(IsColunmExists, true);

            //Act : delete colunm
            liteDB.RemoveColumn("MyCustomizedDataTable", "New");
            ColunmList = liteDB.GetColumnList("NewTableAs");
            IsColunmExists = ColunmList.Any(s => s.Contains("New"));
            //Assert
            Assert.AreEqual(IsColunmExists, false);
        }

        [TestMethod]
        public void RenameTable()
        {
            //Arrange
            string NewName = "TestRename";

            //Act
            liteDB.RenameTable("MyCustomizedDataTable", NewName);
            Assert.AreEqual(liteDB.IsTableExist(NewName), true);
            liteDB.RenameTable(NewName, "MyCustomizedDataTable");

            //Assert
            Assert.AreEqual(liteDB.IsTableExist("MyCustomizedDataTable"), true);
        }

        
        public void GetResult()
        {
            //Arrange
            string Query = "db.MyCustomizedDataTable.find limit 1";

            //Act
            object res = liteDB.GetResult(Query);

            //Assert
            Assert.AreEqual("System.Collections.Generic.Dictionary`2[System.String,LiteDB.BsonValue]" , res.ToString());

        }
        
        [TestMethod]
        public void SaveTable()
        {
            // Arrange
            ObservableList<DataSourceTable> dataSourceTableList = liteDB.GetTablesList();
            DataSourceTable dataSource = null;
            List<string> mColumnNames = null;
            foreach (DataSourceTable dataSourceTable in dataSourceTableList)
            {
                if (dataSourceTable.Name == "MyCustomizedDataTable")
                {
                    dataSource = dataSourceTable;
                }
            }
            DataTable dataTable = liteDB.GetQueryOutput(dataSource.Name);
            dataSource.DataTable = dataTable;
            liteDB.AddRow(mColumnNames, dataSource);
            dataTable = liteDB.GetQueryOutput(dataSource.Name); 

            //Act
            liteDB.SaveTable(dataTable);
            var a = liteDB.GetResult("db.MyCustomizedDataTable.count");

            //Assert
            Assert.AreEqual( "1" , a, "RowCount");
        }

        [TestMethod]
        public void GetTable()
        {
            // Arrange
            string TableName = "MyCustomizedDataTable";

            // Act
            DataTable dataTable = liteDB.GetTable(TableName);

            //Assert
            Assert.AreEqual(dataTable.TableName, TableName);

        }

        [TestMethod]
        public void AddRow()
        {
            // Arrange
            ObservableList<DataSourceTable> dataSourceTableList = liteDB.GetTablesList();
            DataSourceTable dataSource = null;
            List<string> mColumnNames = null;
            foreach (DataSourceTable dataSourceTable in dataSourceTableList)
            {
                if (dataSourceTable.Name == "MyCustomizedDataTable")
                {
                    dataSource = dataSourceTable;
                }
            }
            DataTable dataTable = liteDB.GetQueryOutput(dataSource.Name);
            dataSource.DataTable = dataTable;
            liteDB.AddRow(mColumnNames, dataSource);
            dataTable = liteDB.GetQueryOutput(dataSource.Name);

            //Act
            liteDB.SaveTable(dataTable);
            var a = liteDB.GetResult("db.MyCustomizedDataTable.count");

            //Assert
            Assert.AreEqual( "1", a, "RowCount");
        }
        
       
        [TestMethod]
        public void ExecuteGetValueNextAvailable()
        {
            //Arrange
            ActDSTableElement actDSTable = new ActDSTableElement();
            string query = "db.MyCustomizedDataTable.find limit 1";
            actDSTable.ControlAction = ActDSTableElement.eControlAction.GetValue;
            actDSTable.Customized = true;
            actDSTable.ByNextAvailable = true;
            actDSTable.LocateColTitle = "GINGER_ID";
            actDSTable.MarkUpdate = false;
            actDSTable.AddNewReturnParams = true;

            //Act
            liteDB.Execute(actDSTable, query);

            ActReturnValue value = actDSTable.GetReturnValue(actDSTable.LocateColTitle);

            //Assert
            Assert.AreEqual( "1", value.Actual);
        }

        
        [TestMethod]
        public void ExecuteGetValueByRowNum()
        {
            //Arrange
            ActDSTableElement actDSTable = new ActDSTableElement();
            string query = "db.MyCustomizedDataTable.find limit 1";
            actDSTable.ControlAction = ActDSTableElement.eControlAction.GetValue;
            actDSTable.Customized = true;
            actDSTable.ByNextAvailable = false;
            actDSTable.ByRowNum = true;
            actDSTable.LocateRowValue = "0";
            actDSTable.LocateColTitle = "GINGER_ID";
            actDSTable.MarkUpdate = false;
            actDSTable.AddNewReturnParams = true;

            //Act
            liteDB.Execute(actDSTable, query);
            ActReturnValue value = actDSTable.GetReturnValue(actDSTable.LocateColTitle);

            //Assert
            Assert.AreEqual("1", value.Actual);
        }

        
        [TestMethod]
        public void ExecuteGetValueByQuery()
        {
            //Arrange
            ActDSTableElement actDSTable = new ActDSTableElement();
            string query = "db.MyCustomizedDataTable.find limit 1";
            actDSTable.ControlAction = ActDSTableElement.eControlAction.GetValue;
            actDSTable.Customized = false;
            actDSTable.ByNextAvailable = false;
            actDSTable.ByRowNum = false;
            actDSTable.ByQuery = true;
            actDSTable.MarkUpdate = false;
            actDSTable.AddNewReturnParams = true;

            //Act
            liteDB.Execute(actDSTable, query);
            ActReturnValue value = actDSTable.GetReturnValue("GINGER_ID");

            //Assert
            Assert.AreEqual("1", value.Actual);
        }

        [TestMethod]
        public void ExecuteMarkAsDone()
        {
            //Add row
            ObservableList<DataSourceTable> dataSourceTableList = liteDB.GetTablesList();
            DataSourceTable dataSource = null;
            List<string> mColumnNames = null;
            foreach (DataSourceTable dataSourceTable in dataSourceTableList)
            {
                if (dataSourceTable.Name == "MyCustomizedDataTable")
                {
                    dataSource = dataSourceTable;
                }
            }
            DataTable dataTable = liteDB.GetQueryOutput(dataSource.Name);
            dataSource.DataTable = dataTable;
            liteDB.AddRow(mColumnNames, dataSource);
            liteDB.SaveTable(dataTable);
            
            //Clean up
            ActDSTableElement actDSTable = new ActDSTableElement();
            string query = "db.MyCustomizedDataTable.update GINGER_USED= \"False\"";
            actDSTable.ControlAction = ActDSTableElement.eControlAction.MarkAllUnUsed;
            liteDB.Execute(actDSTable, query);

            //Arrange
            query = "db.MyCustomizedDataTable.update GINGER_USED= \"True\"";
            actDSTable.ControlAction = ActDSTableElement.eControlAction.MarkAsDone;
            actDSTable.DSTableName = "MyCustomizedDataTable";
            actDSTable.Customized = true;
            actDSTable.ByNextAvailable = false;
            actDSTable.ByRowNum = true;
            actDSTable.LocateRowValue = "0";
            actDSTable.LocateColTitle = "GINGER_ID";
            actDSTable.MarkUpdate = true;
            actDSTable.AddNewReturnParams = true;

            //Act
            liteDB.Execute(actDSTable, query);

            //Assert
            query = "db.MyCustomizedDataTable.find GINGER_USED= \"True\"";
            DataTable dt = liteDB.GetQueryOutput(query);

            Assert.AreEqual(1, dt.Rows.Count);
        }

        [TestMethod]
        public void CopyTable()
        {
            // Assert
            string TableName = "MyCustomizedDataTable";
            string NewTablename = TableName + "_Copy";

            //Act
            liteDB.CopyTable(TableName);

            //Assert
            Assert.AreEqual(liteDB.IsTableExist(NewTablename), true);

            //Act : delete table
            liteDB.DeleteTable(NewTablename);

            //Assert
            Assert.AreEqual(liteDB.IsTableExist(NewTablename), false);
        }

        [TestMethod]
        public void GetQueryOutput()
        {
            //Arrange
            string Query = "db.MyCustomizedDataTable.find limit 1";

            //Act
            DataTable res = liteDB.GetQueryOutput(Query);

            //Assert
            Assert.AreEqual( 1, res.Rows.Count);
        }

        [TestMethod]
        public void ExportToExcel()
        {
            //Arrange
            string Query = "db.MyCustomizedDataTable.find limit 1";

            //Act
            DataTable dt = liteDB.GetQueryOutput(Query);
            bool res = GingerCoreNET.GeneralLib.General.ExportToExcel(dt, excelFilePath, "LiteDbExportedData");

            //// TODO:  Fetch and validate data from exported excel using excel action

            //Assert
            Assert.AreEqual(true, res);
        }
        
        [TestMethod]
        public void CommitDb()
        {
            // Arrange
            ObservableList<DataSourceTable> dataSourceTableList = liteDB.GetTablesList();
            DataSourceTable dataSource = null;
            List<string> mColumnNames = null;
            string Query = "db.MyCustomizedDataTable.find limit 1";

            foreach (DataSourceTable dataSourceTable in dataSourceTableList)
            {
                if (dataSourceTable.Name == "MyCustomizedDataTable")
                {
                    dataSource = dataSourceTable;
                }
            }
            DataTable dataTable = liteDB.GetQueryOutput(dataSource.Name);
            dataSource.DataTable = dataTable;
            liteDB.AddRow(mColumnNames, dataSource);
            liteDB.SaveTable(dataSource.DataTable);
            int oldRowCount = liteDB.GetRowCount(dataSource.Name); 

            //Act
            liteDB.DeleteDBTableContents(dataSource.Name);
            DataTable newDt = liteDB.GetQueryOutput(Query);
            liteDB.SaveTable(newDt);
            var newRowCount = liteDB.GetRowCount(dataSource.Name);

            //Assert
            Assert.AreEqual(newRowCount, 0, "RowCountValidation");
            Assert.AreNotEqual(oldRowCount, newRowCount, "RowCountValidation");

        }

    }
}
