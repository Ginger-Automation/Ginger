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
        public static MSAccessDBCon accessDB;
        static string mAccessDBFile = null;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {            
            mAccessDBFile = TestResources.GetTestResourcesFile(@"SignUp.accdb");         
            accessDB = new MSAccessDBCon();
            accessDB.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + mAccessDBFile + ";";
        }

        [TestMethod]
        public void TestConnection()
        {
            //Arrange            

            //Act
            Boolean testconn = accessDB.TestConnection();            

            //Assert
            Assert.IsTrue(testconn, "testconn");
        }

        [TestMethod]
        public void GetTableList()
        {
            //Arrange
            
            List<string> Tables= null;
            
            //Act
             Tables= accessDB.GetTablesList();
           
            //Assert
            Assert.AreEqual(1,Tables.Count);
            Assert.AreEqual("Person", Tables[0]);
        }

        [TestMethod]
        public void GetTablesColumns()
        {
            //Arrange
            List<string> Columns = null;
            string tablename = "Person";
            
            //Act
            Columns = accessDB.GetTablesColumns(tablename);
           
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
            result = accessDB.RunUpdateCommand(upadateCommand, false);
            updatedval = accessDB.GetSingleValue("Person", "LName", "ID=1");

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
            result = accessDB.GetSingleValue("Person", "FName", "ID=2");
            
            //Assert
            Assert.AreEqual( "LMPO", result);
        }

        [TestMethod]
        public void DBQuery()
        {
            //Arrange
            DataTable result = null;
            
            //Act
            result = accessDB.DBQuery("Select * from Person");
            
            //Assert
            Assert.AreEqual(2,result.Rows.Count);
        }

        [TestMethod]
        public void GetRecordCount()
        {
            //Arrange
             int recordCount = 0;
            
            //Act
             recordCount = accessDB.GetRecordCount("Person");
            
            //Assert
            Assert.AreEqual(2,recordCount, "recordCount");
        }

        
    }
}
