using Couchbase;
using Couchbase.Configuration.Client;
using GingerCore.Actions;
using GingerCore.Environments;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Testcontainers.Couchbase;

namespace GingerCoreNETUnitTest.DatabaseTest
{
    [TestClass]
    public sealed class CouchbaseIntegrationTests
    {

        private static readonly CouchbaseContainer _couchbaseContainer = new CouchbaseBuilder().Build();

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            await _couchbaseContainer.StartAsync();
            await CreateTestData();

        }

        [TestCleanup]
        public Task DisposeAsync()
        {
            return _couchbaseContainer.DisposeAsync().AsTask();
        }
        public static async Task CreateTestData()
        {

            try
            {
                //      var clusterOptions = new ClusterOptions();
                //      clusterOptions.ConnectionString = _couchbaseContainer.GetConnectionString();
                //      clusterOptions.UserName = CouchbaseBuilder.DefaultUsername;
                //      clusterOptions.Password = CouchbaseBuilder.DefaultPassword;

                //      var cluster = await Cluster.ConnectAsync(clusterOptions)
                //.ConfigureAwait(true);

                //      // When
                //      var ping = await cluster.PingAsync()
                //          .ConfigureAwait(true);

                //      var bucket = await cluster.BucketAsync(_couchbaseContainer.Buckets.Single().Name)
                //          .ConfigureAwait(true);

                // When
                //var database = await client.Database.PutAsync()
                //    .ConfigureAwait(true);
                var cluster = new Cluster(new ClientConfiguration
                {
                    Servers = new List<Uri> { new Uri("http://" + _couchbaseContainer.GetConnectionString()) },
                    UseSsl = false
                });

                cluster.Authenticate(CouchbaseBuilder.DefaultUsername, CouchbaseBuilder.DefaultPassword);

                var clusterManager = cluster.CreateManager(CouchbaseBuilder.DefaultUsername, CouchbaseBuilder.DefaultPassword);
                var buckets = clusterManager.ListBuckets().Value;
                //var result = clusterManager.CreateBucket(new BucketSettings() { Name = "GingerBucket", BucketType = Couchbase.Core.Buckets.BucketTypeEnum.Couchbase, AuthType = Couchbase.Authentication.AuthType.None });
                //var bucket = cluster.OpenBucket("GingerBucket");

                //// Insert sample data
                //var document = new Document<dynamic>
                //{
                //    Id = "Customer",
                //    Content = new[]{
                //    new { Name = "Mahesh", Value = 1 , Dept = "AQE I&T"},
                //    new { Name = "Jinendra", Value = 2, Dept = "AQE I&T"}
                //},
                //};
                //var upsertResult = await bucket.UpsertAsync(document);
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        [TestMethod]
        public void ShouldReturnTwoCustomers()
        {
            // Given
            Database db = new Database();
            ActDBValidation mact = new ActDBValidation();
            //actDBValidation

            db.DBType = Database.eDBTypes.Couchbase;
            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
            //ATT LS DB
            //Db.DatabaseOperations.TNSCalculated
            db.TNS = "http://" + _couchbaseContainer.GetConnectionString();
            db.User = CouchbaseBuilder.DefaultUsername;
            db.Pass = CouchbaseBuilder.DefaultPassword;
            //  NoSqlBase noSqlBase = new GingerCouchbase(ActDBValidation.eDBValidationType.FreeSQL, db, mact);
            mact.DB = db;

            Boolean connectionSuccessful = mact.DB.DatabaseOperations.Connect();

            // When
            List<object> recs = new List<object>();
            if (connectionSuccessful)
            {
                mact.SQL = "SELECT * FROM `Ginger-bucket` WHERE META().id = 1";
                mact.Execute();

                //db.DatabaseOperations.CloseConnection();
            }

            //then
            Assert.AreEqual(connectionSuccessful, true, "Connected Successfully to CouchbaseDB");
            //Assert.IsNotNull(recs.Count, 0);
            Assert.AreEqual(recs.Count, 2, "Fetched 2 records from Postgre Table");
        }

    }
}
