using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using MongoDB;
using System;

namespace MongoDBTest
{
    [TestClass]
    public class MongoUnitTest
    {
        public static MongoDbConnection db = new MongoDbConnection();

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("ConnectionString", "mongodb://localhost:27017/mycollection");
            db.KeyvalParamatersList = param;
            Boolean testconn = db.OpenConnection(param);

        }

        [TestMethod]
        public void OpenConnection()
        {
            //Arrange
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("ConnectionString", "mongodb://localhost:27017/mycollection");

            //Act
            Boolean testconn = db.OpenConnection(param);

            //Assert
            Assert.AreEqual(testconn, true);
        }

        [TestMethod]
        public void GetTableList()
        {
            //Arrange
            List<string> Tables = null;

            //Act
            Tables = db.GetTablesList();

            //Assert
            Assert.AreEqual(Tables.Count, 1);
        }

        [TestMethod]
        public void GetTablesColumns()
        {
            //Arrange
            List<string> Columns = null;
            string tablename = "mycollection";

            //Act
            Columns = db.GetTablesColumns(tablename);

            //Assert
            Assert.AreEqual(Columns.Count, 3);
        }

        [TestMethod]
        public void RunUpdateCommand()
        {
            //Arrange
            string upadateCommand = "db.mycollection.update({ \"name\" :  \"ff\"},{$set: { \"website\" : \"aaa.com\"} }); ";
            string result = null;

            //Act
            result = db.RunUpdateCommand(upadateCommand, false);

            //Assert
            Assert.AreEqual(result, "Success");
        }

        [TestMethod]
        public void GetSingleValue()
        {
            //Arrange
            string result = null;

            //Act
            result = db.GetSingleValue("mycollection", "age", "20");
            if (result.Contains("zzz"))
            {
                result = "zzz";
            }
            //Assert
            Assert.AreEqual(result, "zzz");
        }

        [TestMethod]
        public void DBQuery()
        {
            //Arrange
            List<object> result = null;

            //Act
            result = db.DBQuery("db.mycollection.count");

            //Assert
            Assert.AreEqual(result.Count, 16);
        }

        [TestMethod]
        public void GetRecordCount()
        {
            //Arrange
            int a = 0;

            //Act
            a = db.GetRecordCount("mycollection");

            //Assert
            Assert.AreEqual(a, 16);
        }

        

    }
}
