using System.Net;
using System.Text;
using System.Text.Json;
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
using Xunit;

namespace JealPrototype.Tests.Unit.ExternalServices;

/// <summary>
/// Unit tests for Story 3.1: Lead API operations (CreateLead, UpdateLead, GetLeadDetail)
/// </summary>
public class EasyCarsApiClientLeadTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<ILogger<EasyCarsApiClient>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly EasyCarsConfiguration _config;
    private readonly EasyCarsApiClient _sut;

    public EasyCarsApiClientLeadTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockCache = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<EasyCarsApiClient>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        _config = new EasyCarsConfiguration
        {
            TestApiUrl = "https://test.easycarsapi.com",
            ProductionApiUrl = "https://api.easycarsapi.com",
            TimeoutSeconds = 30,
            RetryAttempts = 3,
            RetryDelayMilliseconds = 0,
            TokenCacheDurationSeconds = 570
        };

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri(_config.TestApiUrl)
        };
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Setup cache to support CreateEntry (for token caching)
        _mockCache
            .Setup(c => c.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        _sut = new EasyCarsApiClient(
            _mockHttpClientFactory.Object,
            _mockCache.Object,
            Options.Create(_config),
            _mockLogger.Object);
    }

    private void SetupHttpResponseWithToken(object apiResponseContent)
    {
        var requestCount = 0;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                requestCount++;
                if (requestCount == 1)
                {
                    // First request is token acquisition
                    var tokenResponse = new EasyCarsTokenResponse
                    {
                        ResponseCode = 0,
                        ResponseMessage = "Success",
                        Token = "test-jwt-token"
                    };
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonSerializer.Serialize(tokenResponse), Encoding.UTF8, "application/json")
                    };
                }
                else
                {
                    // Subsequent requests are actual API calls
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonSerializer.Serialize(apiResponseContent), Encoding.UTF8, "application/json")
                    };
                }
            });
    }

    [Fact]
    public async Task CreateLeadAsync_WithValidRequest_ReturnsLeadNumberAndCustomerNo()
    {
        // Arrange
        var responseContent = new CreateLeadResponse
        {
            ResponseCode = 0,
            ResponseMessage = "Success",
            LeadNumber = "LEAD-001",
            CustomerNo = "CUST-123"
        };

        SetupHttpResponseWithToken(responseContent);

        var request = new CreateLeadRequest
        {
            AccountNumber = "ACC-001",
            AccountSecret = "SECRET-001",
            CustomerName = "John Smith",
            CustomerEmail = "john.smith@example.com"
        };

        // Act
        var result = await _sut.CreateLeadAsync(
            "client-id", "client-secret",
            "Test", 1,
            request);

        // Assert
        result.Should().NotBeNull();
        result.LeadNumber.Should().Be("LEAD-001");
        result.CustomerNo.Should().Be("CUST-123");
    }

    [Fact]
    public async Task CreateLeadAsync_WithAuthFailure_ThrowsEasyCarsAuthenticationException()
    {
        // Arrange
        var responseContent = new { Code = 1, ResponseMessage = "Authentication failed" };
        SetupHttpResponseWithToken(responseContent);

        var request = new CreateLeadRequest
        {
            AccountNumber = "ACC-001",
            AccountSecret = "BAD-SECRET",
            CustomerName = "John Smith",
            CustomerEmail = "john.smith@example.com"
        };

        // Act & Assert
        await Assert.ThrowsAsync<EasyCarsAuthenticationException>(() =>
            _sut.CreateLeadAsync("client-id", "client-secret", "Test", 1, request));
    }

    [Fact]
    public async Task CreateLeadAsync_WithValidationError_ThrowsEasyCarsValidationException()
    {
        // Arrange
        var responseContent = new { Code = 7, ResponseMessage = "Missing required field" };
        SetupHttpResponseWithToken(responseContent);

        var request = new CreateLeadRequest
        {
            AccountNumber = "ACC-001",
            AccountSecret = "SECRET-001",
            CustomerName = string.Empty,
            CustomerEmail = string.Empty
        };

        // Act & Assert
        await Assert.ThrowsAsync<EasyCarsValidationException>(() =>
            _sut.CreateLeadAsync("client-id", "client-secret", "Test", 1, request));
    }

    [Fact]
    public async Task UpdateLeadAsync_WithValidRequest_ReturnsLeadNumber()
    {
        // Arrange
        var responseContent = new UpdateLeadResponse
        {
            ResponseCode = 0,
            ResponseMessage = "Success",
            LeadNumber = "LEAD-001"
        };

        SetupHttpResponseWithToken(responseContent);

        var request = new UpdateLeadRequest
        {
            LeadNumber = "LEAD-001",
            AccountNumber = "ACC-001",
            AccountSecret = "SECRET-001",
            CustomerName = "John Smith",
            CustomerEmail = "john.smith@example.com",
            Comments = "Updated comment"
        };

        // Act
        var result = await _sut.UpdateLeadAsync(
            "client-id", "client-secret",
            "Test", 1,
            request);

        // Assert
        result.Should().NotBeNull();
        result.LeadNumber.Should().Be("LEAD-001");
    }

    [Fact]
    public async Task UpdateLeadAsync_WithAuthFailure_ThrowsEasyCarsAuthenticationException()
    {
        // Arrange
        var responseContent = new { Code = 1, ResponseMessage = "Authentication failed" };
        SetupHttpResponseWithToken(responseContent);

        var request = new UpdateLeadRequest
        {
            LeadNumber = "LEAD-001",
            AccountNumber = "ACC-001",
            AccountSecret = "BAD-SECRET",
            CustomerName = "John Smith",
            CustomerEmail = "john.smith@example.com"
        };

        // Act & Assert
        await Assert.ThrowsAsync<EasyCarsAuthenticationException>(() =>
            _sut.UpdateLeadAsync("client-id", "client-secret", "Test", 1, request));
    }

    [Fact]
    public async Task GetLeadDetailAsync_WithValidLeadNumber_ReturnsLeadDetail()
    {
        // Arrange
        var responseContent = new LeadDetailResponse
        {
            ResponseCode = 0,
            ResponseMessage = "Success",
            LeadNumber = "LEAD-001",
            CustomerNo = "CUST-123",
            CustomerName = "John Smith",
            CustomerEmail = "john.smith@example.com",
            CustomerPhone = "02-1234-5678",
            CustomerMobile = "0412-345-678",
            VehicleMake = "Toyota",
            VehicleModel = "Camry",
            VehicleYear = 2023,
            VehiclePrice = 45000m,
            VehicleType = 1,
            VehicleInterest = 2,
            FinanceStatus = 1,
            Rating = 2,
            StockNumber = "ST-001",
            Comments = "Test comment",
            LeadStatus = 10,
            CreatedDate = "2024-01-01",
            UpdatedDate = "2024-01-15"
        };

        SetupHttpResponseWithToken(responseContent);

        // Act
        var result = await _sut.GetLeadDetailAsync(
            "client-id", "client-secret",
            "ACC-001", "SECRET-001",
            "Test", 1,
            "LEAD-001");

        // Assert
        result.Should().NotBeNull();
        result.LeadNumber.Should().Be("LEAD-001");
        result.CustomerNo.Should().Be("CUST-123");
        result.CustomerName.Should().Be("John Smith");
        result.CustomerEmail.Should().Be("john.smith@example.com");
        result.VehicleMake.Should().Be("Toyota");
        result.VehicleModel.Should().Be("Camry");
        result.VehicleYear.Should().Be(2023);
        result.VehiclePrice.Should().Be(45000m);
        result.LeadStatus.Should().Be(10);
    }

    [Fact]
    public async Task GetLeadDetailAsync_WithInvalidLeadNumber_ThrowsEasyCarsValidationException()
    {
        // Arrange
        var responseContent = new { Code = 7, ResponseMessage = "Lead not found" };
        SetupHttpResponseWithToken(responseContent);

        // Act & Assert
        await Assert.ThrowsAsync<EasyCarsValidationException>(() =>
            _sut.GetLeadDetailAsync(
                "client-id", "client-secret",
                "ACC-001", "SECRET-001",
                "Test", 1,
                "INVALID-LEAD"));
    }

    [Fact]
    public async Task GetLeadDetailAsync_WithTemporaryError_ThrowsEasyCarsTemporaryException()
    {
        // Arrange
        var responseContent = new { Code = 5, ResponseMessage = "Service temporarily unavailable" };
        SetupHttpResponseWithToken(responseContent);

        // Act & Assert
        await Assert.ThrowsAsync<EasyCarsTemporaryException>(() =>
            _sut.GetLeadDetailAsync(
                "client-id", "client-secret",
                "ACC-001", "SECRET-001",
                "Test", 1,
                "LEAD-001"));
    }
}
