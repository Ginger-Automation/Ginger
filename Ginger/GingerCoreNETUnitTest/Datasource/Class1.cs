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

namespace GingerCoreNETUnitTest.Datasource
{
    class Class1
    {
        GingerLiteDB liteDB = new GingerCoreNET.DataSource.GingerLiteDB();
        ObservableList<DataSourceTable> sourceTables;

        [ClassInitialize]
        public void ClassInitialize()
        {
            string Connectionstring = TestResources.GetTestResourcesFile(@"Solutions\BasicSimple\DataSources" + Path.DirectorySeparatorChar + "LiteDB.db");
            liteDB.Init(Connectionstring);
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
            liteDB.AddTable("NewTableAs", "[GINGER_ID] AUTOINCREMENT,[GINGER_KEY_NAME] Text,[GINGER_KEY_VALUE] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text");
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
            liteDB.AddColumn("NewTableAs", "New", "Text");
            ColunmList = liteDB.GetColumnList("NewTableAs");
            IsColunmExists = ColunmList.Any(s => s.Contains("New"));
            //Assert
            Assert.AreEqual(IsColunmExists, true);

            //Act : delete colunm
            liteDB.RemoveColumn("NewTableAs", "New");
            ColunmList = liteDB.GetColumnList("NewTableAs");
            IsColunmExists = ColunmList.Any(s => s.Contains("New"));
            //Assert
            Assert.AreEqual(IsColunmExists, false);
        }

    }
}
