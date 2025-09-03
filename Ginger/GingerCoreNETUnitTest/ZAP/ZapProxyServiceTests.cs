using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.CoreNET.External.ZAP;
using GingerCoreNET.External.ZAP;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OWASPZAPDotNetAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GingerCoreNET.Tests.External.ZAP
{
    [TestClass]
    public class ZapProxyServiceTests
    {
        private Mock<IZapClient> _mockZap;
        private ZapProxyService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockZap = new Mock<IZapClient>();
            _service = new ZapProxyService(_mockZap.Object);
        }


        [TestMethod]
        public void GetPortFromUrl_WithValidHttpUrl_ReturnsPort()
        {
            var port = ZapProxyService.GetPortFromUrl("http://localhost:9090");
            Assert.AreEqual(9090, port);
        }

        [TestMethod]
        public void GetPortFromUrl_WithoutPort_ReturnsNull()
        {
            var port = ZapProxyService.GetPortFromUrl("http://localhost");
            Assert.IsNull(port);
        }

        [TestMethod]
        public void GetHostFromUrl_WithUrlAndPort_ReturnsHost()
        {
            var host = ZapProxyService.GetHostFromUrl("http://example.com:8080");
            Assert.AreEqual("example.com", host);
        }

        [TestMethod]
        public void GetHostFromUrl_WithOnlyHost_ReturnsSame()
        {
            var host = ZapProxyService.GetHostFromUrl("myserver");
            Assert.AreEqual("myserver", host);
        }

        [TestMethod]
        public void IsZapRunning_WhenApiReturnsVersion_ReturnsTrue()
        {
            _mockZap.Setup(c => c.Version())
                          .Returns(new ApiResponseElement("version", "1.0.0"));

            Assert.IsTrue(_service.IsZapRunning());
        }

        [TestMethod]
        public void IsZapRunning_WhenApiThrows_ReturnsFalse()
        {
            _mockZap.Setup(c => c.Version())
                          .Throws(new Exception("ZAP not available"));

            Assert.IsFalse(_service.IsZapRunning());
        }

        [TestMethod]
        public async Task WaitTillPassiveScanCompleted_WhenZeroRecords_Completes()
        {
            _mockZap.Setup(c => c.RecordsToScan())
                          .Returns(new ApiResponseElement("recordsToScan", "0"));

            await _service.WaitTillPassiveScanCompleted();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GenerateZapReport_WhenExceptionThrown_Rethrows()
        {
            _mockZap.Setup(c => c.GenerateReport(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()
            )).Throws(new Exception("Report failed"));

            _service.GenerateZapReport("http://test.com", "C:\\Reports", "report.html");
        }

        [TestMethod]
        public void AddUrlToScanTree_WhenUrlAdded_Success()
        {
            _mockZap.Setup(c => c.AccessUrl("http://test.com", "false"))
                          .Returns(new ApiResponseElement("OK", "OK"));

            _mockZap.Setup(c => c.Urls("http://test.com"))
                          .Returns(new ApiResponseList("urls", new List<IApiResponse>
                          {
                              new ApiResponseElement("url", "http://test.com")
                          }));

            _service.AddUrlToScanTree("http://test.com");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddUrlToScanTree_WhenUrlNotAdded_Throws()
        {
            _mockZap.Setup(c => c.AccessUrl("http://bad.com", "false"))
                    .Returns(new ApiResponseElement("OK", "OK"));
            _mockZap.Setup(c => c.Urls("http://bad.com"))
                    .Returns(new ApiResponseList("urls", new List<IApiResponse>()));
            _service.AddUrlToScanTree("http://bad.com");
        }

        [TestMethod]
        public void GetUrlsFromScanTree_ReturnsList()
        {
            _mockZap.Setup(c => c.Urls("http://test.com"))
                          .Returns(new ApiResponseList("urls", new List<IApiResponse>
                          {
                              new ApiResponseElement("url", "http://test.com"),
                              new ApiResponseElement("url", "http://another.com")
                          }));

            var result = _service.GetUrlsFromScanTree("http://test.com");
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void PerformActiveScan_Completes()
        {
            _mockZap.Setup(c => c.Scan(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()
            )).Returns(new ApiResponseElement("scanId", "1"));

            _mockZap.SetupSequence(c => c.ScanStatus("1"))
                          .Returns(new ApiResponseElement("status", "50"))
                          .Returns(new ApiResponseElement("status", "100"));

            _service.PerformActiveScan("http://test.com");
        }

        [TestMethod]
        public void EvaluateScanResultWeb_WhenAlertNotAllowed_ReturnsFalse()
        {
            var alertSummary = new ApiResponseSet("alerts", new Dictionary<string, IApiResponse>
            {
                { "SQL Injection", new ApiResponseElement("SQL Injection", "2") }
            });

            _mockZap.Setup(c => c.AlertsSummary("http://test.com"))
                          .Returns(alertSummary);

            var allowed = new ObservableList<OperationValues>
            {
                new OperationValues { Value = "Cross Site Scripting" }
            };

            var result = _service.EvaluateScanResultWeb("http://test.com", allowed);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void EvaluateScanResultWeb_WhenOnlyAllowedAlertsPresent_ReturnsTrue()
        {
            var alertSummary = new ApiResponseSet("alerts", new Dictionary<string, IApiResponse>
            {
                { "Allowed Alert", new ApiResponseElement("Allowed Alert", "2") }
            });
            _mockZap.Setup(c => c.AlertsSummary("http://allowed.com")).Returns(alertSummary);
            var allowed = new ObservableList<OperationValues> { new OperationValues { Value = "Allowed Alert" } };
            var result = _service.EvaluateScanResultWeb("http://allowed.com", allowed);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EvaluateScanResultAPI_WhenVulnerabilityExceedsThreshold_ReturnsFalse()
        {
            var alertSummary = new ApiResponseSet("alerts", new Dictionary<string, IApiResponse>
            {
                { "SQL Injection", new ApiResponseElement("SQL Injection", "5") }
            });

            _mockZap.Setup(c => c.AlertsSummary("http://test.com"))
                          .Returns(alertSummary);

            var thresholds = new ObservableList<OperationValues>
            {
                new OperationValues { Value = "SQL Injection" }
            };

            var result = _service.EvaluateScanResultAPI("http://test.com", thresholds);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetAlertSummary_ReturnsAlerts()
        {
            var alertSummary = new ApiResponseSet("alerts", new Dictionary<string, IApiResponse>
            {
                { "XSS", new ApiResponseElement("XSS", "3") },
                { "SQL Injection", new ApiResponseElement("SQL Injection", "1") }
            });

            _mockZap.Setup(c => c.AlertsSummary("http://test.com"))
                          .Returns(alertSummary);

            var result = _service.GetAlertSummary("http://test.com");
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(t => t == ("XSS", 3)));
        }
    }
}
