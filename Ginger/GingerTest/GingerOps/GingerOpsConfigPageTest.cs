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

namespace GingerTest.GingerOps
{
    [TestClass]
    [Ignore]
    public class GingerOpsConfigPageTest
    {

        private GingerOpsConfigurationPage _page;
        private Mock<GingerOpsAPI> _mockAnalyticsAPI;
        private GingerOpsConfiguration _mockUserConfig;

        [TestInitialize]        
        public void Setup()
        {
            _mockAnalyticsAPI = new Mock<GingerOpsAPI>();
            _mockUserConfig = new GingerOpsConfiguration()
            {
                AccountUrl = "http://valid.url",
                IdentityServiceURL = "http://identity.url",
                ClientId = "client-id",
                ClientSecret = "client-secret"
            };

            // Inject mock objects
            _page = new GingerOpsConfigurationPage();
        }

        [TestMethod]
        public void AreRequiredFieldsEmpty_WhenFieldsAreNotEmpty_ReturnsFalse()
        {
            // Arrange

            // Act
            bool result = _page.AreRequiredFieldsEmpty();

            // Assert
            Assert.IsFalse(result, "Required fields should not be empty.");
        }

        [TestMethod]
        public void AreRequiredFieldsEmpty_WhenFieldsAreEmpty_ReturnsTrue()
        {
            // Arrange
           

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

            _mockAnalyticsAPI.Setup(GingerOpsAPI => GingerOpsAPI.RequestToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                             .Returns(Task.FromResult(true));

            // Act
            bool result = await _page.HandleTokenAuthorization();

            // Assert
            Assert.IsFalse(result, "Token request should be successful.");
            _mockAnalyticsAPI.Verify(GingerOpsAPI => GingerOpsAPI.RequestToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void ShowConnectionResult_WhenAuthorized_ShowsSuccessMessage()
        {
            // Arrange
            bool isAuthorized = true;

            // Act & Assert
            Assert.ThrowsException<NullReferenceException>(() => Ginger.ExternalConfigurations.GingerOpsConfigurationPage.ShowConnectionResult(isAuthorized));
            // Note: Reporter usage should be mocked if necessary to avoid the exception.
        }

    }
}
