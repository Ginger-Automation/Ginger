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

namespace GingerCoreNETUnitTest.Database
{

    [TestClass]

    public class DataBaseUnitTest
    {
        public static MSAccessDBCon db = new MSAccessDBCon();

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            string FilePath = TestResources.GetTestResourcesFile(@"SignUp.accdb");
            param.Add("ConnectionString", @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FilePath +";");
            db.KeyvalParamatersList = param;
            Boolean testconn = db.OpenConnection(param);
            
        }

        [TestMethod]
        public void OpenConnection()
        {
            //Arrange
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("ConnectionString", @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source= C:\GingerSourceControl\Solutions\Ginger_Regression_Testing\Documents\WOEI\SignUp.accdb;");

            //Act
            Boolean testconn = db.OpenConnection(param);

            //Assert
            Assert.AreEqual(testconn, true);
        }

        [TestMethod]
        public void GetTableList()
        {
            //Arrange
            
            List<string> Tables= null;
            
            //Act

             Tables= db.GetTablesList();
           
            //Assert
            Assert.AreEqual(Tables.Count, 2);
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
            Assert.AreEqual(Columns.Count, 9);
        }

        [TestMethod]
        public void RunUpdateCommand()
        {
            //Arrange
            string upadateCommand = "update Person set LName=\"EFGH\" where ID=1";
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
            result = db.GetSingleValue("Person", "FName", "ID=2");
            
            //Assert
            Assert.AreEqual(result, "LMPO");
        }

        [TestMethod]
        public void DBQuery()
        {
            //Arrange
            List<object> result = null;
            
            //Act
            result = db.DBQuery("Select * from Person");
            
            //Assert
            Assert.AreEqual(result.Count, 2);
        }

        [TestMethod]
        public void GetRecordCount()
        {
            //Arrange
             int a = 0;
            
            //Act
                a = db.GetRecordCount("Person");
            
            //Assert
            Assert.AreEqual(a, 2);
        }

        [TestCleanup]
        public void CloseConnection()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("ConnectionString", @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source= C:\GingerSourceControl\Solutions\Ginger_Regression_Testing\Documents\WOEI\SignUp.accdb;");

            db.CloseConnection();
        }
    }
}
