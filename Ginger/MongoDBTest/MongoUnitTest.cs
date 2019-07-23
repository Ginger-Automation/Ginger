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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using MongoDB;
using System;
using System.Data;

namespace MongoDBTest
{
    [Ignore]
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
            Assert.AreEqual(1,Tables.Count);
            Assert.AreEqual("mycollection", Tables[0]);
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
            Assert.AreEqual(3,Columns.Count);
            Assert.AreEqual("name", Columns[0]);
            Assert.AreEqual("age", Columns[1]);
            Assert.AreEqual("website", Columns[2]);
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
            DataTable result = null;

            //Act
            result = db.DBQuery("db.mycollection.count");

            //Assert
            Assert.AreEqual(result.Rows.Count, 16);
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
