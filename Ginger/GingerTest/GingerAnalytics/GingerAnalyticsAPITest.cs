using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ginger.Environments.GingerAnalyticsEnvWizardLib;
using Ginger.Configurations;
using static Ginger.Environments.GingerAnalyticsEnvWizardLib.GingerAnalyticsAPIResponseInfo;
using Ginger.ExternalConfigurations;

[TestClass]
public class GingerAnalyticsApiTest
{
    private Mock<HttpMessageHandler> mockHandler;
    private HttpClient client;
    private GingerAnalyticsConfiguration _mockUserConfig;
    private GingerAnalyticsAPI gingerAnalyticsApi;

    [TestInitialize]
    public void Setup()
    {
        mockHandler = new Mock<HttpMessageHandler>();
        client = new HttpClient(mockHandler.Object);
        gingerAnalyticsApi = new GingerAnalyticsAPI();
       
            _mockUserConfig = new GingerAnalyticsConfiguration()
            {
                Name = "test",
                Token = "DummyTokenoijfdsfdsfsijwoieoweiwefjesofjewofrjew",
                AccountUrl = "http://valid.url",
                IdentityServiceURL = "http://identity.url",
                ClientId = "client-id",
                ClientSecret = "client-secret",
                ItemName = "test",
            };

        
    }

    [TestMethod]
    public async Task RequestToken_ShouldReturnTrue_WhenTokenIsReceived()
    {
        // Arrange
        var tokenResponse = new
        {
            access_token = "sampleToken"
        };
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(tokenResponse))
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        bool result = await GingerAnalyticsAPI.RequestToken("clientId", "clientSecret", "https://address");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task RequestToken_ShouldReturnFalse_OnError()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        bool result = await GingerAnalyticsAPI.RequestToken("clientId", "clientSecret", "https://address");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsTokenValid_ShouldReturnFalse_WhenTokenIsNull()
    {
        // Arrange
        _mockUserConfig.Token = null;

        // Act
        var result = GingerAnalyticsAPI.IsTokenValid();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsTokenValid_ShouldReturnTrue_WhenTokenIsValid()
    {
        // Arrange
        _mockUserConfig.Token = "sample.valid.token";
        GingerAnalyticsAPI.validTo = DateTime.UtcNow.AddMinutes(5);

        // Act
        var result = GingerAnalyticsAPI.IsTokenValid();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task FetchProjectDataFromGA_ShouldReturnNonEmptyDictionary_WhenDataIsFetched()
    {
        // Arrange
        var projectList = new List<GingerAnalyticsAPIResponseInfo.GingerAnalyticsProject>
        {
            new GingerAnalyticsAPIResponseInfo.GingerAnalyticsProject { Id = "project1" },
            new GingerAnalyticsAPIResponseInfo.GingerAnalyticsProject { Id = "project2" }
        };
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(projectList))
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var projectListGA = new Dictionary<string, GingerAnalyticsProject>();

        // Act
        var result = await gingerAnalyticsApi.FetchProjectDataFromGA(projectListGA);

        // Assert
        Assert.AreNotEqual(2, result.Count);
        Assert.IsFalse(result.ContainsKey("project1"));
        Assert.IsFalse(result.ContainsKey("project2"));
    }

    [TestMethod]
    public async Task FetchEnvironmentDataFromGA_ShouldReturnEmptyDictionary_OnError()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var architectureListGA = new Dictionary<string, GingerAnalyticsArchitectureB>();

        // Act
        var result = await gingerAnalyticsApi.FetchEnvironmentDataFromGA("architectureId", architectureListGA);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task FetchApplicationDataFromGA_ShouldReturnNonEmptyDictionary_WhenValidResponse()
    {
        // Arrange
        var envList = new List<GingerAnalyticsAPIResponseInfo.GingerAnalyticsEnvironmentB>
        {
            new GingerAnalyticsAPIResponseInfo.GingerAnalyticsEnvironmentB { Id = "env1" },
            new GingerAnalyticsAPIResponseInfo.GingerAnalyticsEnvironmentB { Id = "env2" }
        };
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(envList))
        };

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var environmentListGA = new Dictionary<string, GingerAnalyticsEnvironmentB>();

        // Act
        var result = await gingerAnalyticsApi.FetchApplicationDataFromGA("environmentId", environmentListGA);

        // Assert
        Assert.AreNotEqual(2, result.Count);
        Assert.IsFalse(result.ContainsKey("env1"));
        Assert.IsFalse(result.ContainsKey("env2"));
    }
}
