using Amdocs.Ginger.CoreNET.Telemetry;
using Amdocs.Ginger.CoreNET.Telemetry.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCoreNETUnitTest.Telemetry.Pipeline
{
    [TestClass]
    [TestCategory(TestCategory.UnitTest)]
    [DoNotParallelize]
    public class DeleteFromLocalDBTelemetryStepTests
    {
        [TestMethod]
        public async Task Process_ExceptionDeletingRecord_RemainingDeletedFromLocalDB()
        {
            string[] records =
            [
                "test-record-1",
                "test-record-2",
                "test-record-3",
            ];
            int recordsDeletedFromLocalDB = 0;
            Mock<ITelemetryDB<string>> mockLocalDB = new(MockBehavior.Strict);
            //if second record then throw exception
            mockLocalDB
                .Setup(m => m.DeleteAsync(It.Is<string>(r => string.Equals(r, records[1]))))
                .Throws<Exception>();
            //if not second record then return success
            mockLocalDB
                .Setup(m => m.DeleteAsync(It.Is<string>(r => !string.Equals(r, records[1]))))
                .Callback(() => recordsDeletedFromLocalDB++)
                .Returns(Task.CompletedTask);
            ILogger logger = TelemetryStepTestUtils.NewConsoleLogger();
            DeleteFromLocalDBTelemetryStep<string> step = new(bufferSize: 1, mockLocalDB.Object, logger);
            step.StartConsumer();

            foreach (string record in records)
            {
                step.Process(record);
            }
            await Task.Delay(100);

            Assert.AreEqual(expected: records.Length - 1, actual: recordsDeletedFromLocalDB);
        }
    }
}
