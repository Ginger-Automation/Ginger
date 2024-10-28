#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Ginger.Environments.GingerOpsEnvWizardLib;
using Ginger.ExternalConfigurations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Ginger.Environments.GingerOpsEnvWizardLib.GingerOpsAPIResponseInfo;

namespace GingerTest.GingerOps;

[TestClass]
public class GingerOpsApiTest
{
    private Mock<HttpMessageHandler> mockHandler;
    private HttpClient client;
    private GingerOpsConfiguration _mockUserConfig;
    private GingerOpsAPI GingerOpsApi;

    [TestInitialize]
    public void Setup()
    {
        mockHandler = new Mock<HttpMessageHandler>();
        client = new HttpClient(mockHandler.Object);
        GingerOpsApi = new GingerOpsAPI();

        _mockUserConfig = new GingerOpsConfiguration()
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
        bool result = await GingerOpsAPI.RequestToken("clientId", "clientSecret", "https://address");

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
        bool result = await GingerOpsAPI.RequestToken("clientId", "clientSecret", "https://address");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsTokenValid_ShouldReturnFalse_WhenTokenIsNull()
    {
        // Arrange
        _mockUserConfig.Token = null;

        // Act
        var result = GingerOpsAPI.IsTokenValid();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsTokenValid_ShouldReturnTrue_WhenTokenIsValid()
    {
        // Arrange
        _mockUserConfig.Token = "sample.valid.token";
        GingerOpsAPI.validTo = DateTime.UtcNow.AddMinutes(5);

        // Act
        var result = GingerOpsAPI.IsTokenValid();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task FetchProjectDataFromGA_ShouldReturnNonEmptyDictionary_WhenDataIsFetched()
    {
        // Arrange
        var projectList = new List<GingerOpsAPIResponseInfo.GingerOpsProject>
        {
            new GingerOpsAPIResponseInfo.GingerOpsProject { Id = "project1" },
            new GingerOpsAPIResponseInfo.GingerOpsProject { Id = "project2" }
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

        var ProjectListGOps = new Dictionary<string, GingerOpsProject>();

        // Act
        var result = await GingerOpsApi.FetchProjectDataFromGOps(ProjectListGOps);

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

        var architectureListGOps = new Dictionary<string, GingerOpsArchitectureB>();

        // Act
        var result = await GingerOpsApi.FetchEnvironmentDataFromGOps("architectureId", architectureListGOps);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task FetchApplicationDataFromGA_ShouldReturnNonEmptyDictionary_WhenValidResponse()
    {
        // Arrange
        var envList = new List<GingerOpsAPIResponseInfo.GingerOpsEnvironmentB>
        {
            new GingerOpsAPIResponseInfo.GingerOpsEnvironmentB { Id = "env1" },
            new GingerOpsAPIResponseInfo.GingerOpsEnvironmentB { Id = "env2" }
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

        var environmentListGOps = new Dictionary<string, GingerOpsEnvironmentB>();

        // Act
        var result = await GingerOpsApi.FetchApplicationDataFromGOps("environmentId", environmentListGOps);

        // Assert
        Assert.AreNotEqual(2, result.Count);
        Assert.IsFalse(result.ContainsKey("env1"));
        Assert.IsFalse(result.ContainsKey("env2"));
    }
}
