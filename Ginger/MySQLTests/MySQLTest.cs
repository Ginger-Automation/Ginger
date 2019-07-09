using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySQLDatabase;
using System;
using System.Collections.Generic;

namespace MySQLTests
{
    [TestClass]
    public class MySQLTest
    {
        public static MYSQLDBConnection db = new MYSQLDBConnection();

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("ConnectionString", "Server=127.0.0.1;Database=sys;Uid=root;Pwd = Hello!12345");
            db.KeyvalParamatersList = param;
            Boolean testconn = db.OpenConnection(param);

        }

        [TestMethod]
        public void OpenConnection()
        {
            //Arrange
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("ConnectionString", "Server=127.0.0.1;Database=sys;Uid=root;Pwd = Hello!12345");

            //Act
            Boolean testconn = db.OpenConnection(param);

            //Assert
            Assert.IsTrue(testconn);
        }

        [TestMethod]
        public void GetTableList()
        {
            //Arrange
            List<string> Tables = null;

            //Act
            Tables = db.GetTablesList();

            //Assert
            Assert.AreEqual(4, Tables.Count);
            Assert.AreEqual("authors", Tables[1].ToString());
            Assert.AreEqual("sys_config", Tables[2].ToString());
            Assert.AreEqual("tutorials_tbl", Tables[3].ToString());
        }

        [TestMethod]
        public void GetTablesColumns()
        {
            //Arrange
            List<string> Columns = null;
            string tablename = "authors";

            //Act
            Columns = db.GetTablesColumns(tablename);

            //Assert
            Assert.AreEqual(4, Columns.Count);
            Assert.AreEqual("id", Columns[1].ToString());
            Assert.AreEqual("name", Columns[2].ToString());
            Assert.AreEqual("email", Columns[3].ToString());
        }

        [TestMethod]
        public void RunUpdateCommand()
        {
            //Arrange
            string upadateCommand = "UPDATE authors SET email='aaa@aa.com' where id=3";
            string result = null;

            //Act
            result = db.RunUpdateCommand(upadateCommand, false);

            //Assert
            Assert.AreEqual(result, "1");
        }

        [TestMethod]
        public void GetSingleValue()
        {
            //Arrange
            string result = null;

            //Act
            result = db.GetSingleValue("authors", "name", "id=2");
            
            //Assert
            Assert.AreEqual(result, "Priya");
        }

        [TestMethod]
        public void DBQuery()
        {
            //Arrange
            List<object> result = null;

            //Act
            result = db.DBQuery("SELECT * FROM authors");

            //Assert
            Assert.AreEqual(result.Count, 2);
        }

        [TestMethod]
        public void GetRecordCount()
        {
            //Arrange
            int a = 0;

            //Act
            a = db.GetRecordCount("authors");

            //Assert
            Assert.AreEqual(a, 3);
        }
    }
}
