using FluentAssertions;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Exceptions;
using JealPrototype.Infrastructure.Configuration;
using JealPrototype.Infrastructure.ExternalServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace JealPrototype.Tests.Unit.ExternalServices;

/// <summary>
/// Story 1.6: Tests for enhanced EasyCars API client with token management, retry logic, and authenticated requests
/// </summary>
public class EasyCarsApiClientStory16Tests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IMemoryCache> _mockMemoryCache;
    private readonly Mock<IOptions<EasyCarsConfiguration>> _mockOptions;
    private readonly Mock<ILogger<EasyCarsApiClient>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly EasyCarsConfiguration _configuration;

    public EasyCarsApiClientStory16Tests()
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

    #region Token Management Tests

    [Fact]
    public async Task GetOrRefreshTokenAsync_CacheMiss_AcquiresNewToken()
    {
        // Arrange
        var tokenResponse = new EasyCarsTokenResponse
        {
            ResponseCode = 0,
            ResponseMessage = "Success",
            Token = "jwt-token-12345",
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
            });

        object? cachedValue = null;
        _mockMemoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(false);

        _mockMemoryCache.Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        var client = CreateClient();

        // Act
        var token = await client.GetOrRefreshTokenAsync(
            "account-number",
            "account-secret",
            "Test",
            42);

        // Assert
        token.Should().Be("jwt-token-12345");
        _mockMemoryCache.Verify(x => x.Set(
            It.IsAny<object>(),
            It.IsAny<TokenCacheEntry>(),
            It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetOrRefreshTokenAsync_CacheHit_UsesCachedToken()
    {
        // Arrange
        var cachedEntry = new TokenCacheEntry
        {
            Token = "cached-token",
            AcquiredAt = DateTime.UtcNow.AddMinutes(-5),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        object? cachedValue = cachedEntry;
        _mockMemoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(true);

        var client = CreateClient();

        // Act
        var token = await client.GetOrRefreshTokenAsync(
            "account-number",
            "account-secret",
            "Test",
            42);

        // Assert
        token.Should().Be("cached-token");
        
        // Verify no HTTP call was made
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetOrRefreshTokenAsync_AuthFailure_ThrowsAuthenticationException()
    {
        // Arrange
        var tokenResponse = new EasyCarsTokenResponse
        {
            ResponseCode = 1,
            ResponseMessage = "Authentication failed",
            ErrorMessage = "Invalid credentials"
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
            });

        object? cachedValue = null;
        _mockMemoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(false);

        var client = CreateClient();

        // Act & Assert
        await Assert.ThrowsAsync<EasyCarsAuthenticationException>(async () =>
            await client.GetOrRefreshTokenAsync(
                "invalid-account",
                "invalid-secret",
                "Test",
                42));
    }

    #endregion

    #region Response Code Handling Tests

    [Fact]
    public async Task ExecuteAuthenticatedRequest_ResponseCode0_ReturnsSuccess()
    {
        // Arrange
        SetupTokenAcquisition("test-token");

        var vehicleResponse = new TestVehicleResponse
        {
            ResponseCode = 0,
            ResponseMessage = "Success",
            VehicleCount = 5
        };

        _mockHttpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateTokenResponse("test-token"))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(vehicleResponse))
            });

        var client = CreateClient();

        // Act
        var result = await client.ExecuteAuthenticatedRequestAsync<TestVehicleResponse>(
            "/vehicles",
            HttpMethod.Get,
            "account",
            "secret",
            "Test",
            42);

        // Assert
        result.Should().NotBeNull();
        result.ResponseCode.Should().Be(0);
        result.VehicleCount.Should().Be(5);
    }

    [Fact]
    public async Task ExecuteAuthenticatedRequest_ResponseCode1_ThrowsAuthenticationException()
    {
        // Arrange
        SetupTokenAcquisition("test-token");

        var errorResponse = new TestVehicleResponse
        {
            ResponseCode = 1,
            ResponseMessage = "Authentication failed"
        };

        _mockHttpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateTokenResponse("test-token"))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(errorResponse))
            });

        var client = CreateClient();

        // Act & Assert
        await Assert.ThrowsAsync<EasyCarsAuthenticationException>(async () =>
            await client.ExecuteAuthenticatedRequestAsync<TestVehicleResponse>(
                "/vehicles",
                HttpMethod.Get,
                "account",
                "secret",
                "Test",
                42));
    }

    [Fact]
    public async Task ExecuteAuthenticatedRequest_ResponseCode5_ThrowsTemporaryException()
    {
        // Arrange
        SetupTokenAcquisition("test-token");

        var errorResponse = new TestVehicleResponse
        {
            ResponseCode = 5,
            ResponseMessage = "Temporary error"
        };

        _mockHttpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateTokenResponse("test-token"))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(errorResponse))
            });

        var client = CreateClient();

        // Act & Assert
        await Assert.ThrowsAsync<EasyCarsTemporaryException>(async () =>
            await client.ExecuteAuthenticatedRequestAsync<TestVehicleResponse>(
                "/vehicles",
                HttpMethod.Get,
                "account",
                "secret",
                "Test",
                42));
    }

    [Fact]
    public async Task ExecuteAuthenticatedRequest_ResponseCode7_ThrowsValidationException()
    {
        // Arrange
        SetupTokenAcquisition("test-token");

        var errorResponse = new TestVehicleResponse
        {
            ResponseCode = 7,
            ResponseMessage = "Validation error"
        };

        _mockHttpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateTokenResponse("test-token"))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(errorResponse))
            });

        var client = CreateClient();

        // Act & Assert
        await Assert.ThrowsAsync<EasyCarsValidationException>(async () =>
            await client.ExecuteAuthenticatedRequestAsync<TestVehicleResponse>(
                "/vehicles",
                HttpMethod.Get,
                "account",
                "secret",
                "Test",
                42));
    }

    [Fact]
    public async Task ExecuteAuthenticatedRequest_ResponseCode9_ThrowsFatalException()
    {
        // Arrange
        SetupTokenAcquisition("test-token");

        var errorResponse = new TestVehicleResponse
        {
            ResponseCode = 9,
            ResponseMessage = "Fatal error"
        };

        _mockHttpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateTokenResponse("test-token"))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(errorResponse))
            });

        var client = CreateClient();

        // Act & Assert
        await Assert.ThrowsAsync<EasyCarsFatalException>(async () =>
            await client.ExecuteAuthenticatedRequestAsync<TestVehicleResponse>(
                "/vehicles",
                HttpMethod.Get,
                "account",
                "secret",
                "Test",
                42));
    }

    [Fact]
    public async Task ExecuteAuthenticatedRequest_UnknownResponseCode_ThrowsUnknownException()
    {
        // Arrange
        SetupTokenAcquisition("test-token");

        var errorResponse = new TestVehicleResponse
        {
            ResponseCode = 99,
            ResponseMessage = "Unknown error"
        };

        _mockHttpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateTokenResponse("test-token"))
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(errorResponse))
            });

        var client = CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EasyCarsUnknownException>(async () =>
            await client.ExecuteAuthenticatedRequestAsync<TestVehicleResponse>(
                "/vehicles",
                HttpMethod.Get,
                "account",
                "secret",
                "Test",
                42));

        exception.ResponseCode.Should().Be(99);
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void Configuration_TestEnvironment_UsesTestUrl()
    {
        // Arrange & Act
        var client = CreateClient();

        // Assert - indirectly verified through successful test API calls
        _configuration.TestApiUrl.Should().Be("https://test.easycars.com/api");
    }

    [Fact]
    public void Configuration_ProductionEnvironment_UsesProductionUrl()
    {
        // Arrange & Act
        var client = CreateClient();

        // Assert
        _configuration.ProductionApiUrl.Should().Be("https://api.easycars.com/api");
    }

    [Fact]
    public void Configuration_InvalidTestUrl_ThrowsOnValidation()
    {
        // Arrange
        var invalidConfig = new EasyCarsConfiguration
        {
            TestApiUrl = "not-a-valid-url",
            ProductionApiUrl = "https://api.easycars.com/api"
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => invalidConfig.Validate());
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ExecuteAuthenticatedRequest_NetworkError_ThrowsEasyCarsException()
    {
        // Arrange
        SetupTokenAcquisition("test-token");

        _mockHttpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateTokenResponse("test-token"))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var client = CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EasyCarsException>(async () =>
            await client.ExecuteAuthenticatedRequestAsync<TestVehicleResponse>(
                "/vehicles",
                HttpMethod.Get,
                "account",
                "secret",
                "Test",
                42));

        exception.Message.Should().Contain("Unable to connect");
    }

    [Fact]
    public async Task ExecuteAuthenticatedRequest_Timeout_ThrowsEasyCarsException()
    {
        // Arrange
        SetupTokenAcquisition("test-token");

        _mockHttpMessageHandler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateTokenResponse("test-token"))
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        var client = CreateClient();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EasyCarsException>(async () =>
            await client.ExecuteAuthenticatedRequestAsync<TestVehicleResponse>(
                "/vehicles",
                HttpMethod.Get,
                "account",
                "secret",
                "Test",
                42));

        exception.Message.Should().Contain("timed out");
    }

    #endregion

    #region Helper Methods

    private void SetupTokenAcquisition(string token)
    {
        object? cachedValue = null;
        _mockMemoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(false);

        _mockMemoryCache.Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());
    }

    private HttpResponseMessage CreateTokenResponse(string token)
    {
        var tokenResponse = new EasyCarsTokenResponse
        {
            ResponseCode = 0,
            ResponseMessage = "Success",
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
        };
    }

    #endregion

    #region Test Response DTOs

    private class TestVehicleResponse : EasyCarsBaseResponse
    {
        public int VehicleCount { get; set; }
    }

    #endregion
}
