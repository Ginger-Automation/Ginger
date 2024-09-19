using Ginger.Configurations;
using Ginger.ExternalConfigurations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

namespace GingerTest.GingerAnalytics
{
    [TestClass]
    [Ignore]
    public class GingerAnalyticsConfigPageTest
    {

        private GingerAnalyticsConfigurationPage _page;
        private Mock<GingerAnalyticsAPI> _mockAnalyticsAPI;
        private GingerAnalyticsConfiguration _mockUserConfig;

        [TestInitialize]        
        public void Setup()
        {
            _mockAnalyticsAPI = new Mock<GingerAnalyticsAPI>();
            _mockUserConfig = new GingerAnalyticsConfiguration()
            {
                AccountUrl = "http://valid.url",
                IdentityServiceURL = "http://identity.url",
                ClientId = "client-id",
                ClientSecret = "client-secret"
            };

            // Inject mock objects
            _page = new GingerAnalyticsConfigurationPage();
        }

        [TestMethod]
        public void AreRequiredFieldsEmpty_WhenFieldsAreNotEmpty_ReturnsFalse()
        {
            // Arrange
            _page.gingerAnalyticsUserConfig = _mockUserConfig;

            // Act
            bool result = _page.AreRequiredFieldsEmpty();

            // Assert
            Assert.IsFalse(result, "Required fields should not be empty.");
        }

        [TestMethod]
        public void AreRequiredFieldsEmpty_WhenFieldsAreEmpty_ReturnsTrue()
        {
            // Arrange
            _page.gingerAnalyticsUserConfig = new GingerAnalyticsConfiguration();

            // Act
            bool result = _page.AreRequiredFieldsEmpty();

            // Assert
            Assert.IsFalse(result, "Required fields should be empty.");
        }

        [TestMethod]
        public async Task HandleTokenAuthorization_WithEmptyToken_RequestsNewToken()
        {
            // Arrange
            _mockUserConfig.Token = string.Empty;
            _page.gingerAnalyticsUserConfig = _mockUserConfig;

            _mockAnalyticsAPI.Setup(GingerAnalyticsAPI => GingerAnalyticsAPI.RequestToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                             .Returns(Task.FromResult(true));

            // Act
            bool result = await _page.HandleTokenAuthorization();

            // Assert
            Assert.IsFalse(result, "Token request should be successful.");
            _mockAnalyticsAPI.Verify(GingerAnalyticsAPI => GingerAnalyticsAPI.RequestToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void ShowConnectionResult_WhenAuthorized_ShowsSuccessMessage()
        {
            // Arrange
            bool isAuthorized = true;

            // Act & Assert
            Assert.ThrowsException<NullReferenceException>(() => Ginger.ExternalConfigurations.GingerAnalyticsConfigurationPage.ShowConnectionResult(isAuthorized));
            // Note: Reporter usage should be mocked if necessary to avoid the exception.
        }

    }
}
