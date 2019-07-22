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
using MSAccessDB;
using System.Collections.Generic;
using System;
using GingerTestHelper;
using System.Data;

namespace GingerCoreNETUnitTest.Database
{

    [TestClass]

    public class DataBaseUnitTest
    {
        public static MSAccessDBCon db = new MSAccessDBCon();
        static string FilePath = null;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            FilePath = TestResources.GetTestResourcesFile(@"SignUp.accdb");
            param.Add("ConnectionString", @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FilePath +";");
            db.KeyvalParamatersList = param;
            Boolean testconn = db.OpenConnection(param);
            
        }

        [TestMethod]
        public void OpenConnection()
        {
            //Arrange
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("ConnectionString", @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FilePath + ";");

            //Act
            Boolean testconn = db.OpenConnection(param);

            //Assert
            Assert.IsTrue(testconn);
        }

        [TestMethod]
        public void GetTableList()
        {
            //Arrange
            
            List<string> Tables= null;
            
            //Act
             Tables= db.GetTablesList();
           
            //Assert
            Assert.AreEqual(2,Tables.Count);
            Assert.AreEqual("Person", Tables[1]);
        }

        [TestMethod]
        public void GetTablesColumns()
        {
            //Arrange
            List<string> Columns = null;
            string tablename = "Person";
            
            //Act
            Columns = db.GetTablesColumns(tablename);
           
            //Assert
            Assert.AreEqual(9,Columns.Count);
            Assert.AreEqual("ID", Columns[1]);
            Assert.AreEqual("FName", Columns[2]);
            Assert.AreEqual("LName", Columns[3]);
            Assert.AreEqual("Password", Columns[4]);
            Assert.AreEqual("EmailId", Columns[5]);
            Assert.AreEqual("Day", Columns[6]);
            Assert.AreEqual("Year", Columns[7]);
            Assert.AreEqual("Mobile", Columns[8]);
        }

        [TestMethod]
        public void RunUpdateCommand()
        {
            //Arrange
            string upadateCommand = "update Person set LName=\"EFGH\" where ID=1";
            string result = null;
            string updatedval = null;
            
            //Act
            result = db.RunUpdateCommand(upadateCommand, false);
            updatedval = db.GetSingleValue("Person", "LName", "ID=1");

            //Assert
            Assert.AreEqual( "1", result);
            Assert.AreEqual("EFGH", updatedval);
            
        }

        [TestMethod]
        public void GetSingleValue()
        {
            //Arrange
            string result = null;
            
            //Act
            result = db.GetSingleValue("Person", "FName", "ID=2");
            
            //Assert
            Assert.AreEqual( "LMPO", result);
        }

        [TestMethod]
        public void DBQuery()
        {
            //Arrange
            DataTable result = null;
            
            //Act
            result = db.DBQuery("Select * from Person");
            
            //Assert
            Assert.AreEqual(2,result.Rows.Count);
        }

        [TestMethod]
        public void GetRecordCount()
        {
            //Arrange
             int a = 0;
            
            //Act
             a = db.GetRecordCount("Person");
            
            //Assert
            Assert.AreEqual(2,a);
        }

        [TestCleanup]
        public void CloseConnection()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("ConnectionString", FilePath);

            db.CloseConnection();
        }
    }
}
