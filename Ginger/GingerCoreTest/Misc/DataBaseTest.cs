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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace UnitTests.NonUITests
{
    [Level3]
    [TestClass]
    
    public class DataBaseTest 
    {
        static GingerRunner mGR = null;

        static BusinessFlow mBF;

        [TestInitialize]
        public void TestInitialize()
        {
            
        }

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {           
            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);

            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "DB Test";
            mBF.Active = true;

            Activity activity = new Activity();
            mBF.Activities.Add(activity);
            mBF.CurrentActivity = activity;
          
            mGR.Executor.CurrentBusinessFlow = mBF;

            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
        }

        [Ignore]
        [TestMethod]  [Timeout(60000)]
        public void TestOracleDBConnectionAndReadAllTables()
        {
            Database db = new Database();
            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
            db.DBType = Database.eDBTypes.Oracle;
            //ATT LS DB            
            db.ConnectionString = ConfigurationManager.AppSettings["OracleConnectionString"];          
            Boolean b =  db.DatabaseOperations.Connect();
            //List<string> recs = new List<string>();
            List<object> recs = new List<object>();
            if (b)
            {

                string sqlQuery = "SELECT TNAME FROM TAB";
                recs = db.DatabaseOperations.FreeSQL(sqlQuery);

                db.DatabaseOperations.CloseConnection();
            }
           Assert.AreEqual(b, true);
            //Assert.IsNotNull(recs.Count, 0);
            Assert.IsNotNull(recs.Count);
        }

        [Ignore]
        [TestMethod]  [Timeout(60000)]
        public void TestMSAccessDB()
        {
            Database db = new Database();
            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
            db.DBType = Database.eDBTypes.MSAccess;
            //    db.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\SVN\GingerSolutions\SCM\Documents\MassData\MAIN_DB.mdb";          
            db.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source="+ TestResources.GetTestResourcesFile(@"Database\GingerUnitTest.mdb");
            string dbstr = db.ConnectionString;

            Boolean b = db.DatabaseOperations.Connect();
            if (b)
            {
                List<string> tables = db.DatabaseOperations.GetTablesList();
                db.DatabaseOperations.CloseConnection();
            }            
            
           Assert.AreEqual(b, true);    
        }
        [Ignore]
        [TestMethod]  [Timeout(60000)]
        public void TestMSSQL()
        {
            Database db = new Database();
            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
            db.DBType = Database.eDBTypes.MSSQL;

           
            db.ConnectionString = ConfigurationManager.AppSettings["MSSQLString"]; 

            Boolean b = db.DatabaseOperations.Connect();
            if (b)
            {
                List<string> tables = db.DatabaseOperations.GetTablesList();
                db.DatabaseOperations.CloseConnection();
            }

           Assert.AreEqual(b, true);    
        }

        [Ignore]
        [TestMethod]  [Timeout(60000)]
        public void OracleFreeSQL()
        {
            Database db = new Database();
            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
            db.DBType = Database.eDBTypes.Oracle;
            //ATT LS DB
          
            db.ConnectionString = ConfigurationManager.AppSettings["OracleConnectionString"];    
            Boolean b = db.DatabaseOperations.Connect();
            if (b)
            {
                List<object> recs = db.DatabaseOperations.FreeSQL("SELECT TNAME FROM TAB");
                Assert.IsNotNull(recs.Count);
                db.DatabaseOperations.CloseConnection();
            }
           Assert.AreEqual(b, true);
           
            
        }
        [Ignore]
        [TestMethod]  [Timeout(60000)]
        public void OracleUpdateDB1InsertQuery()
        {
            Database db = new Database();
            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
            db.DBType = Database.eDBTypes.Oracle;
            db.ConnectionString = ConfigurationManager.AppSettings["OracleConnectionString"];    
            string impactedlines = "";
            Boolean b = db.DatabaseOperations.Connect();
            if (b)
            {
                Random randGenerator = new Random();
                int testId = randGenerator.Next();
                impactedlines = db.DatabaseOperations.fUpdateDB("INSERT INTO GINGER_DB_UNIT_TEST (TEST_ID, TEST_NAME) VALUES ('" + testId + "', 'InsertValues_" + testId + "')", true);

                db.DatabaseOperations.CloseConnection();
            }
            Assert.AreEqual(impactedlines, "1");
        }
        [Ignore]
        [TestMethod]  [Timeout(60000)]     
        public void OracleUpdateDBUpdateQuery()
        {
            Database db = new Database();
            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
            db.DBType = Database.eDBTypes.Oracle;           
            db.ConnectionString = ConfigurationManager.AppSettings["OracleConnectionString"];    
            string impactedlines = null;
            Boolean b = db.DatabaseOperations.Connect();
            if (b)
            {
                string updateStamp = DateTime.Now.ToString();
                string updateQuery = "UPDATE GINGER_DB_UNIT_TEST SET COMMENTS='"+DateTime.UtcNow.ToString()+"' where TEST_ID=(select max(test_id) AS TEST_ID from GINGER_DB_UNIT_TEST)";                
                impactedlines = db.DatabaseOperations.fUpdateDB(updateQuery, true);                
                db.DatabaseOperations.CloseConnection();
            }
           Assert.AreEqual(impactedlines, "1");
        }


        [Ignore]
        [TestMethod]  [Timeout(60000)]
        public void OracleUpdateDBDeleteQuery()
        {
            Database db = new Database();
            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
            db.DBType = Database.eDBTypes.Oracle;            
            db.ConnectionString = ConfigurationManager.AppSettings["OracleConnectionString"];    
            string impactedlines = null;
            Boolean b = db.DatabaseOperations.Connect();
            if (b)
            {
                impactedlines = db.DatabaseOperations.fUpdateDB("delete from GINGER_DB_UNIT_TEST where TEST_ID=(select min(test_id) AS TEST_ID from GINGER_DB_UNIT_TEST) ", true);
                db.DatabaseOperations.CloseConnection();
            }
           Assert.IsNotNull(impactedlines);
        }
        [Ignore]
        [TestMethod]
        [Timeout(60000)]
        public void MongoDbTestConnection()
        {
            Database db = new Database();
            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
            db.DBType = Database.eDBTypes.MongoDb;
            db.ConnectionString = ConfigurationManager.AppSettings["MongoDbConnectionString"];
            Boolean b = db.DatabaseOperations.Connect();
            if (b)
            {
                db.DatabaseOperations.CloseConnection();
            }
            Assert.AreEqual(b, true);
        }
        [Ignore]
        [TestMethod]
        [Timeout(60000)]
        public void MongoDbFreeSQL()
        {
            ActDBValidation actDB = new ActDBValidation();
            actDB.DBValidationType = ActDBValidation.eDBValidationType.FreeSQL;

            actDB.AppName = "DB";
            actDB.DBName = "MongoDb";
            actDB.SQL = "db.products.find( {} )";
            
            mBF.CurrentActivity.Acts.Add(actDB);
            mBF.CurrentActivity.Acts.CurrentItem = actDB;

            ProjEnvironment projEnvironment = new ProjEnvironment();
            projEnvironment.Name = "MongoDbApp";

            EnvApplication envApplication = new EnvApplication();
            envApplication.Name = "DB";

            Database db = new Database();
            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
            db.Name = "MongoDb";
            db.DBType = Database.eDBTypes.MongoDb;
            db.ConnectionString = ConfigurationManager.AppSettings["MongoDbConnectionString"];

            envApplication.Dbs.Add(db);
            projEnvironment.Applications.Add(envApplication);
            mGR.ProjEnvironment = projEnvironment;

            mGR.Executor.RunAction(actDB, false);

            Assert.AreEqual(eRunStatus.Passed, actDB.Status, "Action Status");

        }
        [Ignore]
        [TestMethod]
        [Timeout(60000)]
        public void MongoDbRecordCount()
        {
            ActDBValidation actDB = new ActDBValidation();
            actDB.DBValidationType = ActDBValidation.eDBValidationType.RecordCount;

            actDB.AppName = "DB";
            actDB.DBName = "MongoDb";
            actDB.SQL = "products";

            mBF.CurrentActivity.Acts.Add(actDB);
            mBF.CurrentActivity.Acts.CurrentItem = actDB;

            ProjEnvironment projEnvironment = new ProjEnvironment();
            projEnvironment.Name = "MongoDbApp";

            EnvApplication envApplication = new EnvApplication();
            envApplication.Name = "DB";

            Database db = new Database();
            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
            db.Name = "MongoDb";
            db.DBType = Database.eDBTypes.MongoDb;
            db.ConnectionString = ConfigurationManager.AppSettings["MongoDbConnectionString"];

            envApplication.Dbs.Add(db);
            projEnvironment.Applications.Add(envApplication);
            mGR.ProjEnvironment = projEnvironment;

            mGR.Executor.RunAction(actDB, false);

            Assert.AreEqual(eRunStatus.Passed, actDB.Status, "Action Status");

        }
        [Ignore]
        [TestMethod]
        [Timeout(60000)]
        public void MongoDbUpdateDB()
        {
            ActDBValidation actDB = new ActDBValidation();
            actDB.DBValidationType = ActDBValidation.eDBValidationType.UpdateDB;

            actDB.AppName = "DB";
            actDB.DBName = "MongoDb";
            actDB.SQL = "db.inventory.updateOne({ item: \"paper\" },{$set: { \"size.uom\": \"cm\", status: \"P\" }})";

            mBF.CurrentActivity.Acts.Add(actDB);
            mBF.CurrentActivity.Acts.CurrentItem = actDB;

            ProjEnvironment projEnvironment = new ProjEnvironment();
            projEnvironment.Name = "MongoDbApp";

            EnvApplication envApplication = new EnvApplication();
            envApplication.Name = "DB";

            Database db = new Database();
            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
            db.Name = "MongoDb";
            db.DBType = Database.eDBTypes.MongoDb;
            db.ConnectionString = ConfigurationManager.AppSettings["MongoDbConnectionString"];

            envApplication.Dbs.Add(db);
            projEnvironment.Applications.Add(envApplication);
            mGR.ProjEnvironment = projEnvironment;

            mGR.Executor.RunAction(actDB, false);

            Assert.AreEqual(eRunStatus.Passed, actDB.Status, "Action Status");

        }

        [Ignore]
        [TestMethod]
        [Timeout(60000)]
        public void RenameDBName()
        {
            //Arrange
            ProjEnvironment projEnvironment = new ProjEnvironment();
            projEnvironment.Name = "MongoDbApp";

            EnvApplication envApplication = new EnvApplication();
            envApplication.Name = "DB";

            Database db = new Database();
            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
            db.Name = "MongoDb";
            db.DBType = Database.eDBTypes.MongoDb;

            envApplication.Dbs.Add(db);
            projEnvironment.Applications.Add(envApplication);
            mGR.ProjEnvironment = projEnvironment;

            ActDBValidation actDB = new ActDBValidation();
            actDB.DBValidationType = ActDBValidation.eDBValidationType.UpdateDB;

            actDB.AppName = "DB";
            actDB.DBName = "MongoDb";

            mBF.CurrentActivity.Acts.Add(actDB);
            mBF.CurrentActivity.Acts.CurrentItem = actDB;
            
            //Act            
            ((DatabaseOperations)db.DatabaseOperations).NameBeforeEdit = actDB.DBName;
            db.Name = "MongoDBNew";
            //Database.UpdateDatabaseNameChangeInItem(actDB, db.NameBeforeEdit, db.Name);
            
            //Assert
            Assert.AreEqual(actDB.DBName, db.Name, "Names");

        }

        [TestMethod]
        [Timeout(60000)]
        public void TestHidePasswordFromAccessConnectionString()
        {
            //Arrange
            string ConString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:/myAccessFile.accdb;Jet OLEDB:Database Password=MyDbPassword;";
            //Act
            string Result = General.HidePasswordFromString(ConString);
            //Assert
            Assert.AreNotEqual(ConString, Result);
            Assert.AreEqual(Result.Contains("Password=*****"), true);
            Assert.AreEqual(Result.Contains("MyDbPassword"), false);
        }

        [TestMethod]
        [Timeout(60000)]
        public void TestHidePasswordFromOracleConnectionString()
        {
            //Arrange
            string ConStringUsingPwd = "SERVER=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=MyHost)(PORT=MyPort))(CONNECT_DATA=(SERVICE_NAME=MyOracleSID)));uid=myUsername;pwd=myPassword;";
            string ConStringUsingPassword = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=MyHost)(PORT=MyPort))(CONNECT_DATA=(SERVICE_NAME=MyOracleSID)));User Id=myUsername;Password=myPassword;";
            
            //Act
            string ResultPwd = General.HidePasswordFromString(ConStringUsingPwd);
            string ResultPassword = General.HidePasswordFromString(ConStringUsingPassword);
            
            //Assert
            Assert.AreNotEqual(ConStringUsingPwd, ResultPwd);
            Assert.AreEqual(ResultPwd.Contains("pwd=*****"), true);
            Assert.AreEqual(ResultPwd.Contains("myPassword"), false);

            Assert.AreNotEqual(ConStringUsingPassword, ResultPassword);
            Assert.AreEqual(ResultPassword.Contains("Password=*****"), true);
            Assert.AreEqual(ResultPassword.Contains("myPassword"), false);

        }

        [TestMethod]
        [Timeout(60000)]
        public void TestHidePasswordFromPostgreSQLConnectionString()
        {
            //Arrange
            string ConString = "Server=127.0.0.1;Port=5432;Database=myDataBase;User Id =myUsername;Password=myPassword;";
            //Act
            string Result = General.HidePasswordFromString(ConString);
            //Assert
            Assert.AreNotEqual(ConString, Result);
            Assert.AreEqual(Result.Contains("Password=*****"), true);
            Assert.AreEqual(Result.Contains("myPassword"), false);
        }

        [TestMethod]
        [Timeout(60000)]
        public void TestHidePasswordFromMySQLConnectionString()
        {
            //Arrange
            string ConString = "Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";
            //Act
            string Result = General.HidePasswordFromString(ConString);
            //Assert
            Assert.AreNotEqual(ConString, Result);
            Assert.AreEqual(Result.Contains("Pwd=*****"), true);
            Assert.AreEqual(Result.Contains("myPassword"), false);
        }

        [TestMethod]
        [Timeout(60000)]
        public void TestHidePasswordFromDB2ConnectionString()
        {
            //Arrange
            string ConString = "Server=myAddress:myPortNumber;Database=myDataBase;UID=myUsername;PWD=myPassword;";
            //Act
            string Result = General.HidePasswordFromString(ConString);
            //Assert
            Assert.AreNotEqual(ConString, Result);
            Assert.AreEqual(Result.Contains("PWD=*****"), true);
            Assert.AreEqual(Result.Contains("myPassword"), false);
        }

        [TestMethod]
        [Timeout(60000)]
        public void TestHidePasswordFromSQLServerConnectionString()
        {
            //Arrange
            string ConString = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;";
            //Act
            string Result = General.HidePasswordFromString(ConString);
            //Assert
            Assert.AreNotEqual(ConString, Result);
            Assert.AreEqual(Result.Contains("Password=*****"), true);
            Assert.AreEqual(Result.Contains("myPassword"), false);
        }
    }
}
