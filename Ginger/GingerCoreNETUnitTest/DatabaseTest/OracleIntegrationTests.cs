using GingerCore.Environments;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Testcontainers.Oracle;
using Oracle;
using Oracle.ManagedDataAccess.Client;

namespace GingerCoreNETUnitTest.DatabaseTest
{
    [TestClass]
    public class OracleIntegrationTests
    {
        private static readonly OracleContainer _oracleContainer = new OracleBuilder().Build();

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            await _oracleContainer.StartAsync();
            await CreateTestData();

        }

        [TestCleanup]
        public Task DisposeAsync()
        {
            return _oracleContainer.DisposeAsync().AsTask();
        }
        public static async Task CreateTestData()
        {
            try
            {
                var scriptContent = "CREATE TABLE Customer (Id NUMBER PRIMARY KEY, Name varchar2(50));" +
                                             "INSERT INTO Customer (Id, Name) VALUES (1, 'Mahesh'); " +
                                             "INSERT INTO Customer (Id, Name) VALUES (2, 'kale');";
                //var execResult = await _oracleContainer.ExecScriptAsync(scriptContent)
        // .ConfigureAwait(false);

                using (var connection = new OracleConnection(_oracleContainer.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    // Example: Create a table and insert test data
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = scriptContent;
                        await command.ExecuteNonQueryAsync();
                    }
                }

            }
            catch (Exception ex)
            {
            
            }
        }

        [TestMethod]
        public void ShouldReturnTwoCustomers()
        {
            // Given
            Database db = new Database();
            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
            db.DBType = Database.eDBTypes.Oracle;
            //ATT LS DB            
            db.ConnectionString = _oracleContainer.GetConnectionString();
            Boolean connectionSuccessful = db.DatabaseOperations.Connect();


            // When

            List<object> recs = new List<object>();
            if (connectionSuccessful)
            {

                string sqlQuery = "SELECT Id, Name FROM Customer";
                recs = db.DatabaseOperations.FreeSQL(sqlQuery);

                db.DatabaseOperations.CloseConnection();
            }

            //then
            Assert.AreEqual(connectionSuccessful, true, "Connected Successfully to Oracle");
            //Assert.IsNotNull(recs.Count, 0);
            Assert.AreEqual(recs.Count, 2, "Fetched 2 records from Oracle Table");
        }
    }
}
