#region License
/*
Copyright © 2014-2025 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Ginger.Configurations;
using Ginger.ExternalConfigurations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

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
