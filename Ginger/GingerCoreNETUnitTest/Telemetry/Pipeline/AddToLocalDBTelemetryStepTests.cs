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
    public class AddToLocalDBTelemetryStepTests
    {
        [TestMethod]
        public async Task Process_ExceptionAddingRecord_NotForwardedToNextStep()
        {
            string[] records =
            [
                "test-record-1",
                "test-record-2",
                "test-record-3"
            ];
            Mock<ITelemetryDB<string>> mockLocalDB = new(MockBehavior.Strict);
            mockLocalDB
                .Setup(m => m.AddAsync(It.IsAny<string>()))
                .Throws<Exception>();
            ILogger logger = TelemetryStepTestUtils.NewConsoleLogger();
            var forwardedRecordCount = 0;
            Mock<ITelemetryStep<string>> mockSendToCollectorStep = new();
            mockSendToCollectorStep
                .Setup(m => m.Process(It.IsAny<string>()))
                .Callback(() => forwardedRecordCount++);
            AddToLocalDBTelemetryStep<string> step = new(bufferSize: 1, mockLocalDB.Object, mockSendToCollectorStep.Object, logger);
            step.StartConsumer();

            foreach (var record in records)
            {
                step.Process(record);
            }
            await Task.Delay(100);

            Assert.AreEqual(expected: 0, actual: forwardedRecordCount);
        }

        [TestMethod]
        public async Task Process_ExceptionAddingRecord_RemainingForwardedToNextStep()
        {
            string[] records =
            [
                "test-record-1",
                "test-record-2",
                "test-record-3"
            ];
            Mock<ITelemetryDB<string>> mockLocalDB = new(MockBehavior.Strict);
            //if second record then throw exception
            mockLocalDB
                .Setup(m => m.AddAsync(It.Is<string>(r => string.Equals(r, records[1]))))
                .Throws<Exception>();
            //if not second record then return success
            mockLocalDB
                .Setup(m => m.AddAsync(It.Is<string>(r => !string.Equals(r, records[1]))))
                .Returns(Task.CompletedTask);
            var logger = TelemetryStepTestUtils.NewConsoleLogger();
            var forwardedRecordCount = 0;
            Mock<ITelemetryStep<string>> mockSendToCollectorStep = new();
            mockSendToCollectorStep
                .Setup(m => m.Process(It.IsAny<string>()))
                .Callback(() => forwardedRecordCount++);
            AddToLocalDBTelemetryStep<string> step = new(bufferSize: 1, mockLocalDB.Object, mockSendToCollectorStep.Object, logger);
            step.StartConsumer();

            foreach (var record in records)
            {
                step.Process(record);
            }
            await Task.Delay(100);

            Assert.AreEqual(expected: records.Length - 1, actual: forwardedRecordCount);
        }
    }
}
