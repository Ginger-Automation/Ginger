using Ginger.ExternalConfigurations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Threading;
using System.Net;
using Ginger.Configurations;

namespace GingerAnalyticsAPITest
{
    [TestClass]
    public class GingerAnalyticsAPITest
    {
        private Mock<HttpMessageHandler> _mockHttpHandler;
        private Mock<HttpClient> _mockHttpClient;
        private GingerAnalyticsConfiguration _mockUserConfig;


        [TestInitialize]
        public void Setup()
        {
            _mockUserConfig = new GingerAnalyticsConfiguration()
            {
                Name = "test",
                Token = "oijfdsfdsfsijwoieoweiwefjesofjewofrjew",
                AccountUrl = "http://valid.url",
                IdentityServiceURL = "http://identity.url",
                ClientId = "client-id",
                ClientSecret = "client-secret",
                ItemName = "test",
            };
            _mockHttpHandler = new Mock<HttpMessageHandler>();
            _mockHttpClient = new Mock<HttpClient>();

        }

        [TestMethod]
        public async Task RequestToken_SuccessfulTokenRequest_ReturnsTrue()
        {
            // Arrange
            var mockDiscoveryResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"token_endpoint\":\"http://mocktokenendpoint\"}"),
            };

            // Mocking a successful token request response
            var mockTokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"access_token\":\"mockAccessToken\"}"),
            };

            _mockHttpClient
                .SetupSequence(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDiscoveryResponse) 
                .ReturnsAsync(mockTokenResponse);


            // Act
            var result = await GingerAnalyticsAPI.RequestToken("client-id", "client-secret", "http://testaddress");

            // Assert
            Assert.IsTrue(result, "Token request should succeed.");
        }

        [TestMethod]
        public async Task RequestToken_FailedTokenRequest_ReturnsFalse()
        {
            // Arrange
            var mockDiscoveryResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"token_endpoint\":\"http://mocktokenendpoint\"}"),
            };

            
            var mockFailedTokenResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"invalid_client\"}"),
            };

            _mockHttpClient
                .SetupSequence(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDiscoveryResponse)  
                .ReturnsAsync(mockFailedTokenResponse);


            // Act
            var result = await GingerAnalyticsAPI.RequestToken("client-id", "client-secret", "http://mock.url");

            // Assert
            Assert.IsFalse(result, "Token request should fail.");
        }

        [TestMethod]
        public void IsTokenValid_ValidToken_ReturnsTrue()
        {
            // Arrange
            string validToken = CreateValidJwtToken();
            GingerAnalyticsAPI.gingerAnalyticsUserConfig.Token = validToken;

            // Act
            bool result = GingerAnalyticsAPI.IsTokenValid();

            // Assert
            Assert.IsTrue(result, "Token should be valid.");
        }

        [TestMethod]
        public void IsTokenValid_InvalidToken_ReturnsFalse()
        {
            // Arrange
            string invalidToken = "invalid.token.string";
            GingerAnalyticsAPI.gingerAnalyticsUserConfig.Token = invalidToken;

            // Act
            bool result = GingerAnalyticsAPI.IsTokenValid();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsTokenValid_ExpiredToken_ReturnsFalse()
        {
            // Arrange
            string expiredToken = CreateExpiredJwtToken(_mockUserConfig.Token);
            GingerAnalyticsAPI.gingerAnalyticsUserConfig.Token = expiredToken;

            // Act
            bool result = GingerAnalyticsAPI.IsTokenValid();

            // Assert
            Assert.IsFalse(result);
        }

        private string CreateValidJwtToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Convert.FromBase64String(_mockUserConfig.Token);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string CreateExpiredJwtToken(string t)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Convert.FromBase64String(_mockUserConfig.Token);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddMinutes(-70), // Expired 70 minutes ago
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
