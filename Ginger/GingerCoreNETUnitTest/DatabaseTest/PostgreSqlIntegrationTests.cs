using GingerCore.Environments;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace GingerCoreNETUnitTest.DatabaseTest
{
    [TestClass]
    public sealed class PostgreSqlIntegrationTests
    {
        private static readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .Build();

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            await _postgres.StartAsync();
            await CreateTestData();

        }

        [TestCleanup]
        public Task DisposeAsync()
        {
            return _postgres.DisposeAsync().AsTask();
        }
        public static async Task CreateTestData()
        {
            using (var connection = new NpgsqlConnection(_postgres.GetConnectionString()))
            {
                await connection.OpenAsync();

                // Create test data
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "CREATE TABLE Customer (Id serial PRIMARY KEY, Name varchar(255));" +
                                          "INSERT INTO Customer (Name) VALUES ('1'), ('Mahesh');" +
                                          "INSERT INTO Customer (Name) VALUES ('1'), ('Kale');";
                    await command.ExecuteNonQueryAsync();
                }
            }

        }

        [TestMethod]
        public void ShouldReturnTwoCustomers()
        {
            // Given
            Database db = new Database();
            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
            db.DBType = Database.eDBTypes.PostgreSQL;
            //ATT LS DB            
            db.ConnectionString = _postgres.GetConnectionString();
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
            Assert.AreEqual(connectionSuccessful, true, "Connected Successfully to PostgreSQL");
            //Assert.IsNotNull(recs.Count, 0);
            Assert.AreEqual(recs.Count,2, "Fetched 2 records from Postgre Table");
        }
    }
}