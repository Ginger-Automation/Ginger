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
        //ObservableList<DataSourceBase> dataSourceBases;
        ProjEnvironment environment = new ProjEnvironment();

        [TestInitialize]
        public void TestInitialize()
        {
            sqlite.FilePath = TestResources.GetTestResourcesFile(@"DataSource\SQLiteDataSource.db");
            sqlite.Init(sqlite.FilePath);
        }

        [TestMethod]
        public void AddTable()
        {
            /* Addign a table as Name : NewTableAs
              GINGER_ID INT | GINGER_KEY_NAME Text | GINGER_KEY_VALUE Text | GINGER_LAST_UPDATED_BY Text | GINGER_LAST_UPDATE_DATETIME Text
                            |                      |                       |                             |
                            |                      |                       |                             |
              */
            sqlite.AddTable("NewTableAs", "[GINGER_ID] INTEGER ,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");

            //Assert
            Assert.AreEqual(sqlite.IsTableExist("NewTableAs"), true);

            sqlite.DeleteTable("NewTableAs");

        }

        [TestMethod]
        public void GetTableList()
        {
            //Getting table lists 
            sourceTables = sqlite.GetTablesList();

            //Assert
            Assert.AreEqual(sourceTables.Count, 3);
        }

        

        [TestMethod]
        public void AddAndDeleteColunmfromTable()
        {
            /* Addign a Colunm as Name : New
              GINGER_ID INT | GINGER_KEY_NAME Text | GINGER_KEY_VALUE Text | GINGER_LAST_UPDATED_BY Text | GINGER_LAST_UPDATE_DATETIME Text | New Text
                            |                      |                       |                             |                                  |
                            |                      |                       |                             |                                  |
              */
            List<string> ColunmList;
            bool IsColunmExists = false;
            sqlite.AddTable("TestTable_ColunmADD_Delete", "[GINGER_ID] INTEGER ,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");

            sqlite.AddColumn("TestTable_ColunmADD_Delete", "New", "Text");
            ColunmList = sqlite.GetColumnList("TestTable_ColunmADD_Delete");
            IsColunmExists = ColunmList.Any(s => s.Contains("New"));
            
            //Assert
            Assert.AreEqual(IsColunmExists, true);

            /* Removing Colunm as Name : New
              GINGER_ID INT | GINGER_KEY_NAME Text | GINGER_KEY_VALUE Text | GINGER_LAST_UPDATED_BY Text | GINGER_LAST_UPDATE_DATETIME Text | 
                            |                      |                       |                             |                                  |
                            |                      |                       |                             |                                  |
              */
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
            List<string> ColunmList;
            bool IsColunmExists = false;
            // Copy Table
            sqlite.AddTable("TestTable_Copy", "[GINGER_ID] INTEGER ,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");

            sqlite.CopyTable("TestTable_Copy");
            ColunmList = sqlite.GetColumnList("TestTable_Copy");
            IsColunmExists = ColunmList.Any(s => s.Contains("GINGER_ID"));

            //Assert
            Assert.AreEqual(IsColunmExists, true);
            sqlite.DeleteTable("TestTable_Copy");

        }

        [TestMethod]
        public void RenameTable()
        {
            List<string> ColunmList;
            bool IsColunmExists = false;
            sqlite.AddTable("TestTable_Rename", "[GINGER_ID] INTEGER ,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");

            //Rename table
            sqlite.RenameTable("TestTable_Rename", "RenamedTableAS");
            ColunmList = sqlite.GetColumnList("RenamedTableAS");
            IsColunmExists = ColunmList.Any(s => s.Contains("GINGER_ID"));

            //Assert
            Assert.AreEqual(IsColunmExists, true);
            sqlite.DeleteTable("RenamedTableAS");

        }
        
            [TestMethod]
        public void SaveTable()
        {
            // Saving the existing tables

            List<string> ColunmList;
            bool IsColunmExists = false;
            sqlite.AddTable("TestTable_Save", "[GINGER_ID] INTEGER ,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");

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
            bool IsColunmExists = false;
            AddTable();

            //Delete table
            sqlite.DeleteTable("NewTableAs");

            //Assert
            Assert.AreEqual(IsColunmExists, false);

        }
        [TestMethod]
        public void CloseConnection()
        {
            //close connection
            sqlite.Close();

        }

    }
}
