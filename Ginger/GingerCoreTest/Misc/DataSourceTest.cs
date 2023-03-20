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

namespace UnitTests.NonUITests
{
    [TestClass]
    public class DataSourceTest
    {
        static AccessDataSource accessDataSource = new GingerCore.DataSource.AccessDataSource();
        ObservableList<DataSourceTable> sourceTables;
        static string excelFilePath = "";
        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            excelFilePath = TestResources.GetTestResourcesFile(@"Excel" + Path.DirectorySeparatorChar + "ExportedDS.xlsx");
            accessDataSource.FileFullPath = TestResources.GetTestResourcesFile(@"DataSources\GingerDataSource.mdb");
        }

        [TestMethod]
        public void GetTableList()
        {
            //Act 
            sourceTables = accessDataSource.GetTablesList();

            //Assert
            Assert.AreNotEqual(sourceTables.Count, 0);
        }

        [TestMethod]
        public void AddTableAndDeleteTable()
        {
            //Act : add table
            accessDataSource.AddTable("NewTableAs", "[GINGER_ID] AUTOINCREMENT,[GINGER_USED] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");

            //Assert
            Assert.AreEqual(accessDataSource.IsTableExist("NewTableAs"), true);

            //Act : delete table
            accessDataSource.DeleteTable("NewTableAs");

            //Assert
            Assert.AreEqual(accessDataSource.IsTableExist("NewTableAs"), false);
        }

        [TestMethod]
        public void AddAndDeleteColunmfromTable()
        {
            //Arrange
            List<string> ColunmList;
            bool IsColunmExists = false;

            //Act :add colunm
            accessDataSource.AddColumn("MyCustomizedDataTable", "New", "Text");
            ColunmList = accessDataSource.GetColumnList("MyCustomizedDataTable");
            IsColunmExists = ColunmList.Any(s => s.Contains("New"));

            //Assert
            Assert.AreEqual(IsColunmExists, true);

            //Act : delete colunm
            accessDataSource.RemoveColumn("MyCustomizedDataTable", "New");
            ColunmList = accessDataSource.GetColumnList("MyCustomizedDataTable");
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
            accessDataSource.RenameTable("MyCustomizedDataTable", NewName);
            Assert.AreEqual(accessDataSource.IsTableExist(NewName), true);
            accessDataSource.RenameTable(NewName, "MyCustomizedDataTable");

            //Assert
            Assert.AreEqual(accessDataSource.IsTableExist("MyCustomizedDataTable"), true);
        }

        [TestMethod]
        public void SaveTable()
        {
            // Arrange
            ObservableList<DataSourceTable> dataSourceTableList = accessDataSource.GetTablesList();
            DataSourceTable dataSource = null;
            List<string> mColumnNames = null;
            foreach (DataSourceTable dataSourceTable in dataSourceTableList)
            {
                if (dataSourceTable.Name == "MyCustomizedDataTable")
                {
                    dataSource = dataSourceTable;
                }
            }
            int rowsCount = accessDataSource.GetRowCount(dataSource.Name);
            DataTable dataTable = accessDataSource.GetQueryOutput("select * from MyCustomizedDataTable");
            dataSource.DataTable = dataTable;
            accessDataSource.AddRow(mColumnNames, dataSource);

            //Act
            accessDataSource.SaveTable(dataSource.DataTable);
            DataTable res = accessDataSource.GetQueryOutput("select * from MyCustomizedDataTable");

            //Assert
            Assert.AreEqual(rowsCount + 1, res.Rows.Count, "RowCount");
        }

        [TestMethod]
        public void GetTable()
        {
            // Arrange
            string TableName = "MyCustomizedDataTable";

            // Act
            DataTable dataTable = accessDataSource.GetTable(TableName);

            //Assert
            Assert.AreEqual(dataTable.TableName, TableName);

        }

        [TestMethod]
        public void AddRow()
        {
            // Arrange
            ObservableList<DataSourceTable> dataSourceTableList = accessDataSource.GetTablesList();
            DataSourceTable dataSource = null;
            List<string> mColumnNames = null;
            foreach (DataSourceTable dataSourceTable in dataSourceTableList)
            {
                if (dataSourceTable.Name == "MyCustomizedDataTable")
                {
                    dataSource = dataSourceTable;
                }
            }
            int rowsCount = accessDataSource.GetRowCount(dataSource.Name);
            DataTable dataTable = accessDataSource.GetQueryOutput("select * from " + dataSource.Name);
            dataSource.DataTable = dataTable;
            accessDataSource.AddRow(mColumnNames, dataSource);

            //Act
            accessDataSource.SaveTable(dataSource.DataTable);
            DataTable res = accessDataSource.GetQueryOutput("select * from " + dataSource.Name);

            //Assert
            Assert.AreEqual(rowsCount + 1, res.Rows.Count, "RowCount");
        }


        [TestMethod]
        public void CopyTable()
        {
            // Assert
            string TableName = "MyCustomizedDataTable";
            string NewTablename = TableName + "_Copy";

            //Act
            accessDataSource.CopyTable(TableName);

            //Assert
            Assert.AreEqual(accessDataSource.IsTableExist(NewTablename), true);

            //Act : delete table
            accessDataSource.DeleteTable(NewTablename);

            //Assert
            Assert.AreEqual(accessDataSource.IsTableExist(NewTablename), false);
        }

        [TestMethod]
        public void GetQueryOutput()
        {
            int rowsCount = accessDataSource.GetRowCount("MyCustomizedDataTable");

            //Arrange
            string Query = "select * from MyCustomizedDataTable";

            //Act
            DataTable res = accessDataSource.GetQueryOutput(Query);

            //Assert
            Assert.AreEqual(rowsCount, res.Rows.Count);
        }

        [TestMethod]
        public void ExportToExcel()
        {
            //Arrange
            string Query = "select * from MyCustomizedDataTable";

            //Act
            DataTable dt = accessDataSource.GetQueryOutput(Query);
            bool res = GingerCoreNET.GeneralLib.General.ExportToExcel(dt, excelFilePath, "accessDataSourceExportedData");

            //// TODO:  Fetch and validate data from exported excel using excel action

            //Assert
            Assert.AreEqual(true, res);
        }

        [TestMethod]
        public void CommitDb()
        {
            // Arrange
            ObservableList<DataSourceTable> dataSourceTableList = accessDataSource.GetTablesList();
            DataSourceTable dataSource = null;
            List<string> mColumnNames = null;
            foreach (DataSourceTable dataSourceTable in dataSourceTableList)
            {
                if (dataSourceTable.Name == "MyCustomizedDataTable")
                {
                    dataSource = dataSourceTable;
                }
            }
            DataTable dataTable = accessDataSource.GetQueryOutput("select * from " + dataSource.Name);
            dataSource.DataTable = dataTable;
            accessDataSource.AddRow(mColumnNames, dataSource);
            accessDataSource.SaveTable(dataSource.DataTable);
            int oldRowCount = accessDataSource.GetRowCount(dataSource.Name);

            //Act
            accessDataSource.RunQuery("DELETE from  " + dataSource.Name);
            accessDataSource.SaveTable(dataSource.DataTable);
            int newRowCount = accessDataSource.GetRowCount(dataSource.Name);

            //Assert
            Assert.AreEqual(newRowCount, 0, "RowCountValidation");
            Assert.AreNotEqual(oldRowCount, newRowCount, "RowCountValidation");
        }

    }
}
