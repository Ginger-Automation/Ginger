using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Platforms;
using GingerTestHelper;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Common.InterfacesLib;

namespace UnitTests.NonUITests
{
    [TestClass]
    public class DataSourceUnitTest
    {
        DataSourceBase sqlite = new SQLiteDataSource();
        ObservableList<DataSourceTable> sourceTables;


        [ClassInitialize]
        public void ClassInitialize()
        {
            sqlite.FilePath = TestResources.GetTestResourcesFile(@"DataSource\SQLiteDataSource.db");
            sqlite.Init(sqlite.FilePath);
        }

        [TestMethod]

        public void AddTable()
        {
            /* Addign a table as Name : NewTable
              GINGER_ID INT | GINGER_KEY_NAME Text | GINGER_KEY_VALUE Text | GINGER_LAST_UPDATED_BY Text | GINGER_LAST_UPDATE_DATETIME Text
                            |                      |                       |                             |
                            |                      |                       |                             |
              */

            //Arrange
            string tableName = "NewTable";

            //Act
            sqlite.AddTable(tableName, "[GINGER_ID] INTEGER ,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");

            //Assert
            Assert.AreEqual(sqlite.IsTableExist(tableName), true);

        }

        [TestMethod]
        public void GetTableList()
        {
            //Getting table lists 
            sourceTables = sqlite.GetTablesList();

            //Assert
            Assert.AreEqual(sourceTables.Count, 2);
        }



        [TestMethod]
        public void AddColunmtoTable()
        {
            /* Addign a Colunm as Name : New
              GINGER_ID INT | GINGER_KEY_NAME Text | GINGER_KEY_VALUE Text | GINGER_LAST_UPDATED_BY Text | GINGER_LAST_UPDATE_DATETIME Text | New Text
                            |                      |                       |                             |                                  |
                            |                      |                       |                             |                                  |
              */

            //Arrange
            List<string> ColunmList;
            bool IsColunmExists = false;
            sqlite.AddTable("TestTable_ColunmADD", "[GINGER_ID] INTEGER ,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");

            //Act
            sqlite.AddColumn("TestTable_ColunmADD_Delete", "New", "Text");
            ColunmList = sqlite.GetColumnList("TestTable_ColunmADD_Delete");
            IsColunmExists = ColunmList.Any(s => s.Contains("New"));

            //Assert
            Assert.AreEqual(IsColunmExists, true);

        }

        [TestMethod]
        public void DeleteColunmtoTable()
        {
            /* Removing Colunm as Name : New
              GINGER_ID INT | GINGER_KEY_NAME Text | GINGER_KEY_VALUE Text | GINGER_LAST_UPDATED_BY Text | GINGER_LAST_UPDATE_DATETIME Text | 
                            |                      |                       |                             |                                  |
                            |                      |                       |                             |                                  |
              */

            //Arrange
            List<string> ColunmList;
            bool IsColunmExists = false;
            sqlite.AddTable("TestTable_ColunmADD", "[GINGER_ID] INTEGER ,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");

            //Act
            sqlite.RemoveColumn("TestTable_ColunmADD_Delete", "New");
            ColunmList = sqlite.GetColumnList("TestTable_ColunmADD_Delete");
            IsColunmExists = ColunmList.Any(s => s.Contains("New"));

            //Assert
            Assert.AreEqual(IsColunmExists, false);
            sqlite.DeleteTable("TestTable_ColunmADD_Delete");
        }

        [TestMethod]
        public void CopyTable()
        {
            //Arrange
            List<string> ColunmList;
            bool IsColunmExists = false;
            string originalName = "TestTable";

            sqlite.AddTable(originalName, "[GINGER_ID] INTEGER ,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");

            //Act
            sqlite.CopyTable(originalName);
            ColunmList = sqlite.GetColumnList("TestTable_Copy");
            IsColunmExists = ColunmList.Any(s => s.Contains("GINGER_ID"));

            //Assert
            Assert.AreEqual(IsColunmExists, true);
            sqlite.DeleteTable("TestTable_Copy");

        }

        [TestMethod]
        public void RenameTable()
        {
            //Arrange
            string originalTableName = "TestTable_Rename";
            string newName = "TestTable_RenamedTable";

            List<string> ColunmList;
            bool IsColunmExists = false;
            sqlite.AddTable(originalTableName, "[GINGER_ID] INTEGER ,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");

            //Act
            sqlite.RenameTable(originalTableName, newName);
            ColunmList = sqlite.GetColumnList(newName);
            IsColunmExists = ColunmList.Any(s => s.Contains("GINGER_ID"));

            //Assert
            Assert.AreEqual(true, IsColunmExists, "IsColunmExists");
            sqlite.DeleteTable("RenamedTableAS");

        }

        [TestMethod]
        public void SaveTable()
        {
            //Arrange
            List<string> ColunmList;
            bool IsColunmExists = false;
            sqlite.AddTable("TestTable_Save", "[GINGER_ID] INTEGER ,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");

            //Act
            ObservableList<DataSourceTable> mDSTableList;
            mDSTableList = sqlite.GetTablesList();
            foreach (DataSourceTable dsTable in mDSTableList)
            {
                if (dsTable.DataTable != null)
                    dsTable.DSC.SaveTable(dsTable.DataTable);

            }

            ColunmList = sqlite.GetColumnList("TestTable_Save");
            IsColunmExists = ColunmList.Any(s => s.Contains("GINGER_ID"));

            //Assert
            Assert.AreEqual(IsColunmExists, true);
            sqlite.DeleteTable("TestTable_Save");
        }

        [TestMethod]
        public void DeleteTable()
        {
            //Arrange
            bool IsColunmExists = false;
            AddTable();

            //Act
            sqlite.DeleteTable("NewTableAs");

            //Assert
            Assert.AreEqual(IsColunmExists, false);

        }

        [TestCleanup]
        public void CloseConnection()
        {
            //close connection
            sqlite.Close();

        }

    }
}
