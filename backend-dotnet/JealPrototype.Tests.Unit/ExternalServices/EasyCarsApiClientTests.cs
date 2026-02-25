using System.Net;
using System.Text.Json;
using FluentAssertions;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Infrastructure.Configuration;
using JealPrototype.Infrastructure.ExternalServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace JealPrototype.Tests.Unit.ExternalServices;

public class EasyCarsApiClientTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IMemoryCache> _mockMemoryCache;
    private readonly Mock<IOptions<EasyCarsConfiguration>> _mockOptions;
    private readonly Mock<ILogger<EasyCarsApiClient>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly EasyCarsConfiguration _configuration;

    public EasyCarsApiClientTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockMemoryCache = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<EasyCarsApiClient>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        _mockHttpClientFactory.Setup(x => x.CreateClient("EasyCarsApi"))
            .Returns(_httpClient);

        _configuration = new EasyCarsConfiguration
        {
            TestApiUrl = "https://test.easycars.com/api",
            ProductionApiUrl = "https://api.easycars.com/api",
            TimeoutSeconds = 30,
            RetryAttempts = 3,
            RetryDelayMilliseconds = 1000,
            TokenCacheDurationSeconds = 570
        };

        _mockOptions = new Mock<IOptions<EasyCarsConfiguration>>();
        _mockOptions.Setup(x => x.Value).Returns(_configuration);
    }

    private EasyCarsApiClient CreateClient()
    {
        return new EasyCarsApiClient(
            _mockHttpClientFactory.Object,
            _mockMemoryCache.Object,
            _mockOptions.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task RequestTokenAsync_WithValidCredentials_ReturnsSuccessResponse()
    {
        // Arrange
        var tokenResponse = new EasyCarsTokenResponse
        {
            ResponseCode = 0,
            Token = "jwt-token-here",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var responseContent = JsonSerializer.Serialize(tokenResponse);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseContent)
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var client = CreateClient();

        // Act
        var result = await client.RequestTokenAsync(
            "12345678-1234-1234-1234-123456789012",
            "87654321-4321-4321-4321-210987654321",
            "Test");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ResponseCode.Should().Be(0);
        result.Token.Should().Be("jwt-token-here");
    }

    [Fact]
    public async Task RequestTokenAsync_WithInvalidCredentials_ReturnsAuthFailureResponse()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("{\"ResponseCode\":1,\"ErrorMessage\":\"Invalid credentials\"}")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var client = CreateClient();

        // Act
        var result = await client.RequestTokenAsync(
            "invalid-account",
            "invalid-secret",
            "Test");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ResponseCode.Should().Be(1);
        result.ErrorCode.Should().Be("AUTH_FAILED");
    }

    [Fact(Skip = "Http timeout mocking is complex - covered by use case test")]
    public async Task RequestTokenAsync_WithTimeout_ReturnsTimeoutResponse()
    {
        // This test is skipped because mocking HttpClient timeout behavior
        // is complex and the timeout handling is already covered by
        // TestConnectionUseCaseTests.ExecuteAsync_WithTimeout_ReturnsTimeoutResponse
    }

    [Fact]
    public async Task RequestTokenAsync_WithNetworkError_ReturnsNetworkErrorResponse()
    {
        // Arrange
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var client = CreateClient();

        // Act
        var result = await client.RequestTokenAsync(
            "12345678-1234-1234-1234-123456789012",
            "87654321-4321-4321-4321-210987654321",
            "Test");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NETWORK_ERROR");
    }

    [Fact]
    public async Task RequestTokenAsync_WithProductionEnvironment_UsesProductionUrl()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ResponseCode\":0,\"Token\":\"jwt\"}")
            });

        var client = CreateClient();

        // Act
        await client.RequestTokenAsync(
            "12345678-1234-1234-1234-123456789012",
            "87654321-4321-4321-4321-210987654321",
            "Production");

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.ToString().Should().Contain("api.easycars.com");
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WithValidCredentials_ReturnsTrue()
    {
        // Arrange
        var tokenResponse = new EasyCarsTokenResponse
        {
            ResponseCode = 0,
            Token = "jwt-token"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var client = CreateClient();

        // Act
        var result = await client.ValidateCredentialsAsync(
            "12345678-1234-1234-1234-123456789012",
            "87654321-4321-4321-4321-210987654321",
            "Test");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WithInvalidCredentials_ReturnsFalse()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("{\"ResponseCode\":1}")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var client = CreateClient();

        // Act
        var result = await client.ValidateCredentialsAsync(
            "invalid-account",
            "invalid-secret",
            "Test");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithInvalidConfiguration_ThrowsInvalidOperationException()
    {
        // Arrange
        var invalidConfig = new EasyCarsConfiguration
        {
            TestApiUrl = null!,  // Invalid URL
            ProductionApiUrl = "https://api.easycars.com/api"
        };

        var mockOptions = new Mock<IOptions<EasyCarsConfiguration>>();
        mockOptions.Setup(x => x.Value).Returns(invalidConfig);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            new EasyCarsApiClient(
                _mockHttpClientFactory.Object,
                _mockMemoryCache.Object,
                mockOptions.Object,
                _mockLogger.Object));
    }
}

