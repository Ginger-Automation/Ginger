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
    public class SendToCollectorTelemetryStepTests
    {
        [TestMethod]
        public async Task Process_ExceptionAddingRecords_NoneDeletedFromLocalDB()
        {
            string[] records =
            [
                "test-record-1",
                "test-record-2",
                "test-record-3",
            ];
            Mock<ITelemetryCollector<string>> mockTelemetryCollector = new(MockBehavior.Strict);
            mockTelemetryCollector
                .Setup(m => m.AddAsync(It.IsAny<IEnumerable<string>>()))
                .Throws<Exception>();
            ILogger logger = TelemetryStepTestUtils.NewConsoleLogger();
            int recordsForwardedForDeleteFromLocalDBStep = 0;
            Mock<ITelemetryStep<string>> mockDeleteFromLocalDBStep = new(MockBehavior.Strict);
            mockDeleteFromLocalDBStep
                .Setup(m => m.Process(It.IsAny<string>()))
                .Callback(() => recordsForwardedForDeleteFromLocalDBStep++);
            Mock<ITelemetryStep<string>> mockMarkUnsuccessfulInLocalDBStep = new(MockBehavior.Strict);
            mockMarkUnsuccessfulInLocalDBStep
                .Setup(m => m.Process(It.IsAny<string>()));
            SendToCollectorTelemetryStep<string> step = new(
                bufferSize: 1, 
                mockTelemetryCollector.Object, 
                mockDeleteFromLocalDBStep.Object, 
                mockMarkUnsuccessfulInLocalDBStep.Object, 
                logger);
            step.StartConsumer();

            foreach (string record in records)
            {
                step.Process(record);
            }
            await Task.Delay(100);

            Assert.AreEqual(expected: 0, recordsForwardedForDeleteFromLocalDBStep);
        }

        [TestMethod]
        public async Task Process_ExceptionAddingRecords_AllMarkedUnsuccessfulInLocalDB()
        {
            string[] records =
            [
                "test-record-1",
                "test-record-2",
                "test-record-3",
            ];
            Mock<ITelemetryCollector<string>> mockTelemetryCollector = new(MockBehavior.Strict);
            mockTelemetryCollector
                .Setup(m => m.AddAsync(It.IsAny<IEnumerable<string>>()))
                .Throws<Exception>();
            ILogger logger = TelemetryStepTestUtils.NewConsoleLogger();
            Mock<ITelemetryStep<string>> mockDeleteFromLocalDBStep = new(MockBehavior.Strict);
            mockDeleteFromLocalDBStep
                .Setup(m => m.Process(It.IsAny<string>()));
            int recordsMarkedUnsuccessfulInLocalDB = 0;
            Mock<ITelemetryStep<string>> mockMarkUnsuccessfulInLocalDBStep = new(MockBehavior.Strict);
            mockMarkUnsuccessfulInLocalDBStep
                .Setup(m => m.Process(It.IsAny<string>()))
                .Callback(() => recordsMarkedUnsuccessfulInLocalDB++);
            SendToCollectorTelemetryStep<string> step = new(
                bufferSize: 1,
                mockTelemetryCollector.Object,
                mockDeleteFromLocalDBStep.Object,
                mockMarkUnsuccessfulInLocalDBStep.Object,
                logger);
            step.StartConsumer();

            foreach (string record in records)
            {
                step.Process(record);
            }
            await Task.Delay(100);

            Assert.AreEqual(expected: records.Length, recordsMarkedUnsuccessfulInLocalDB);
        }

        [TestMethod]
        public async Task Process_RecordsAddedSuccessfully_AllDeletedFromLocalDB()
        {
            string[] records =
            [
                "test-record-1",
                "test-record-2",
                "test-record-3",
            ];
            Mock<ITelemetryCollector<string>> mockTelemetryCollector = new(MockBehavior.Strict);
            mockTelemetryCollector
                .Setup(m => m.AddAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new ITelemetryCollector<string>.AddResult()
                {
                    Successful = true,
                });
            ILogger logger = TelemetryStepTestUtils.NewConsoleLogger();
            int recordsForwardedForDeleteFromLocalDBStep = 0;
            Mock<ITelemetryStep<string>> mockDeleteFromLocalDBStep = new(MockBehavior.Strict);
            mockDeleteFromLocalDBStep
                .Setup(m => m.Process(It.IsAny<string>()))
                .Callback(() => recordsForwardedForDeleteFromLocalDBStep++);
            Mock<ITelemetryStep<string>> mockMarkUnsuccessfulInLocalDBStep = new(MockBehavior.Strict);
            mockMarkUnsuccessfulInLocalDBStep
                .Setup(m => m.Process(It.IsAny<string>()));
            SendToCollectorTelemetryStep<string> step = new(
                bufferSize: 1,
                mockTelemetryCollector.Object,
                mockDeleteFromLocalDBStep.Object,
                mockMarkUnsuccessfulInLocalDBStep.Object,
                logger);
            step.StartConsumer();

            foreach (string record in records)
            {
                step.Process(record);
            }
            await Task.Delay(100);

            Assert.AreEqual(expected: records.Length, recordsForwardedForDeleteFromLocalDBStep);
        }

        [TestMethod]
        public async Task Process_RecordsAddedSuccessfully_NoneMarkedUnsuccessfulInLocalDB()
        {
            string[] records =
            [
                "test-record-1",
                "test-record-2",
                "test-record-3",
            ];
            Mock<ITelemetryCollector<string>> mockTelemetryCollector = new(MockBehavior.Strict);
            mockTelemetryCollector
                .Setup(m => m.AddAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(new ITelemetryCollector<string>.AddResult()
                {
                    Successful = true,
                });
            ILogger logger = TelemetryStepTestUtils.NewConsoleLogger();
            Mock<ITelemetryStep<string>> mockDeleteFromLocalDBStep = new(MockBehavior.Strict);
            mockDeleteFromLocalDBStep
                .Setup(m => m.Process(It.IsAny<string>()));
            int recordsMarkedUnsuccessfulInLocalDB = 0;
            Mock<ITelemetryStep<string>> mockMarkUnsuccessfulInLocalDBStep = new(MockBehavior.Strict);
            mockMarkUnsuccessfulInLocalDBStep
                .Setup(m => m.Process(It.IsAny<string>()))
                .Callback(() => recordsMarkedUnsuccessfulInLocalDB++);
            SendToCollectorTelemetryStep<string> step = new(
                bufferSize: 1,
                mockTelemetryCollector.Object,
                mockDeleteFromLocalDBStep.Object,
                mockMarkUnsuccessfulInLocalDBStep.Object,
                logger);
            step.StartConsumer();

            foreach (string record in records)
            {
                step.Process(record);
            }
            await Task.Delay(100);

            Assert.AreEqual(expected: 0, recordsMarkedUnsuccessfulInLocalDB);
        }
    }
}
