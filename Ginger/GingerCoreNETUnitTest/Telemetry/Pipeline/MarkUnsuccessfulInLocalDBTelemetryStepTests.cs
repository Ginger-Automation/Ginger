using Amdocs.Ginger.CoreNET.Telemetry.Pipeline;
using Amdocs.Ginger.CoreNET.Telemetry;
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
    public class MarkUnsuccessfulInLocalDBTelemetryStepTests
    {
        [TestMethod]
        public async Task Process_ExceptionMarkingRecord_RemainingMarkedInLocalDB()
        {
            string[] records =
            [
                "test-record-1",
                "test-record-2",
                "test-record-3",
            ];
            int recordsMarkedInLocalDB = 0;
            Mock<ITelemetryDB<string>> mockLocalDB = new(MockBehavior.Strict);
            //if second record then throw exception
            mockLocalDB
                .Setup(m => m.MarkFailedToUpload(It.Is<string>(r => string.Equals(r, records[1]))))
                .Throws<Exception>();
            //if not second record then return success
            mockLocalDB
                .Setup(m => m.MarkFailedToUpload(It.Is<string>(r => !string.Equals(r, records[1]))))
                .Callback(() => recordsMarkedInLocalDB++)
                .Returns(Task.CompletedTask);
            ILogger logger = TelemetryStepTestUtils.NewConsoleLogger();
            MarkUnsuccessfulInLocalDBTelemetryStep<string> step = new(bufferSize: 1, mockLocalDB.Object, logger);
            step.StartConsumer();

            foreach (string record in records)
            {
                step.Process(record);
            }
            await Task.Delay(100);

            Assert.AreEqual(expected: records.Length - 1, actual: recordsMarkedInLocalDB);
        }
    }
}
