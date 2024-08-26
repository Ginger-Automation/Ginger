using Amdocs.Ginger.CoreNET.Telemetry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GingerCoreNETUnitTest.Telemetry
{
    [TestClass]
    [TestCategory(TestCategory.IntegrationTest)]
    public class TelemetryLiteDBTests
    {
        private static TelemetryLiteDB _db;

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            _db = new(GetLiteDBFilePath());
        }

        private static string GetLiteDBFilePath()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dbFilePath = Path.Combine(currentDirectory, "testTelemetryDB.db");
            return dbFilePath;
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _db.Dispose();
            File.Delete(GetLiteDBFilePath());
        }

        [TestMethod]
        public async Task AddAsync_AddLog_NoExceptionIsThrown()
        {
            await _db.AddAsync(NewLogRecord());
        }

        [TestMethod]
        public async Task AddAsync_NullLog_ThrowArgumentNullException()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _db.AddAsync(null));
        }

        [TestMethod]
        public async Task AddAsync_ParallelAddLog_NoExceptionIsThrown()
        {
            int parallelCount = 10;
            Task[] addTasks = new Task[parallelCount];
            for (int index = 0; index < parallelCount; index++)
            {
                addTasks[index] = new Task(async () =>
                {
                    try
                    {
                        await _db.AddAsync(NewLogRecord());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });
            }

            foreach (Task addTask in addTasks)
            {
                addTask.Start();
            }
            await Task.WhenAll(addTasks);
        }

        [TestMethod]
        public async Task DeleteAsync_NullLog_ThrowArgumentNullException()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _db.DeleteAsync(null));
        }

        [TestMethod]
        public async Task DeleteAsync_LogNotFound_ReturnFalse()
        {
            TelemetryLogRecord nonExistentLog = NewLogRecord();

            bool wasFound = await _db.DeleteAsync(nonExistentLog);

            Assert.IsFalse(wasFound);
        }

        [TestMethod]
        public async Task DeleteAsync_LogFound_ReturnTrue()
        {
            TelemetryLogRecord existentLog = NewLogRecord();

            await _db.AddAsync(existentLog);
            bool wasFound = await _db.DeleteAsync(existentLog);

            Assert.IsTrue(wasFound);
        }

        [TestMethod]
        public async Task MarkFailedToUploadAsync_NullLog_ThrowArgumentNullException()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _db.MarkFailedToUpload(null));
        }

        [TestMethod]
        public async Task MarkFailedToUploadAsync_LogNotFound_ReturnFalse()
        {
            TelemetryLogRecord nonExistentLog = NewLogRecord();

            bool wasFound = await _db.MarkFailedToUpload(nonExistentLog);

            Assert.IsFalse(wasFound);
        }

        [TestMethod]
        public async Task MarkFailedToUploadAsync_LogFound_ReturnFalse()
        {
            TelemetryLogRecord existentLog = NewLogRecord();

            await _db.AddAsync(existentLog);
            bool wasFound = await _db.MarkFailedToUpload(existentLog);

            Assert.IsTrue(wasFound);
        }

        private static TelemetryLogRecord NewLogRecord()
        {
            return new TelemetryLogRecord()
            {
                AppVersion = "24.3.0",
                Level = "Info",
                Message = "Test Log message " + Guid.NewGuid().ToString(),
                UserId = "IntegrationTest",
                CreationTimestamp = DateTime.UtcNow,
                LastUpdateTimestamp = DateTime.UtcNow,
            };
        }
    }
}
