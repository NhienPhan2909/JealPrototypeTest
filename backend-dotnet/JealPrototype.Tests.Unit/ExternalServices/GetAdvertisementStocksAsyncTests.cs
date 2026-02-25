using System.Net;
using System.Text;
using System.Text.Json;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Exceptions;
using JealPrototype.Infrastructure.Configuration;
using JealPrototype.Infrastructure.ExternalServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace JealPrototype.Tests.Unit.ExternalServices;

/// <summary>
/// Unit tests for Story 2.1: GetAdvertisementStocksAsync method
/// </summary>
public class GetAdvertisementStocksAsyncTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<ILogger<EasyCarsApiClient>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly EasyCarsConfiguration _config;
    private readonly EasyCarsApiClient _sut;

    public GetAdvertisementStocksAsyncTests()
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
            TokenCacheDurationSeconds = 570
        };

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri(_config.TestApiUrl)
        };
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Setup cache to return token (bypass token acquisition)
        _mockCache
            .Setup(c => c.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        _sut = new EasyCarsApiClient(
            _mockHttpClientFactory.Object,
            _mockCache.Object,
            Options.Create(_config),
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetAdvertisementStocksAsync_WithSuccessResponse_ReturnsStockList()
    {
        // Arrange
        var stockItems = new List<StockItem>
        {
            new StockItem
            {
                StockNumber = "ST001",
                VIN = "1HGBH41JXMN109186",
                Make = "Honda",
                Model = "Accord",
                Year = 2023,
                Price = 35000,
                Status = "Available"
            },
            new StockItem
            {
                StockNumber = "ST002",
                VIN = "5FNRL6H78LB012345",
                Make = "Honda",
                Model = "Odyssey",
                Year = 2024,
                Price = 45000,
                Status = "Available"
            }
        };

        var responseContent = new StockResponse
        {
            ResponseCode = 0,
            ResponseMessage = "Success",
            Stocks = stockItems
        };

        SetupHttpResponseWithToken(responseContent);

        // Act
        var result = await _sut.GetAdvertisementStocksAsync(
            "test-account",
            "test-secret",
            "Test",
            1,
            null,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("ST001", result[0].StockNumber);
        Assert.Equal("Honda", result[0].Make);
        Assert.Equal("Accord", result[0].Model);
        Assert.Equal("ST002", result[1].StockNumber);
    }

    [Fact]
    public async Task GetAdvertisementStocksAsync_WithEmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        var responseContent = new StockResponse
        {
            ResponseCode = 0,
            ResponseMessage = "Success",
            Stocks = new List<StockItem>()
        };

        SetupHttpResponseWithToken(responseContent);

        // Act
        var result = await _sut.GetAdvertisementStocksAsync(
            "test-account",
            "test-secret",
            "Test",
            1,
            null,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAdvertisementStocksAsync_WithNullStocks_ReturnsEmptyList()
    {
        // Arrange
        var responseContent = new StockResponse
        {
            ResponseCode = 0,
            ResponseMessage = "Success",
            Stocks = null
        };

        SetupHttpResponseWithToken(responseContent);

        // Act
        var result = await _sut.GetAdvertisementStocksAsync(
            "test-account",
            "test-secret",
            "Test",
            1,
            null,
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAdvertisementStocksAsync_WithYardCode_IncludesYardCodeInRequest()
    {
        // Arrange
        var responseContent = new StockResponse
        {
            ResponseCode = 0,
            ResponseMessage = "Success",
            Stocks = new List<StockItem>()
        };

        HttpRequestMessage? capturedRequest = null;
        var requestCount = 0;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
            {
                requestCount++;
                if (requestCount == 2) // Capture second request (first is token)
                {
                    capturedRequest = req;
                }
            })
            .ReturnsAsync(() =>
            {
                if (requestCount == 1)
                {
                    var tokenResponse = new EasyCarsTokenResponse { ResponseCode = 0, Token = "test-token" };
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonSerializer.Serialize(tokenResponse), Encoding.UTF8, "application/json")
                    };
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
                };
            });

        // Act
        await _sut.GetAdvertisementStocksAsync(
            "test-account",
            "test-secret",
            "Test",
            1,
            "YARD-01",
            CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Contains("yardCode=YARD-01", capturedRequest.RequestUri?.ToString());
    }

    [Fact]
    public async Task GetAdvertisementStocksAsync_WithoutYardCode_ExcludesYardCodeFromRequest()
    {
        // Arrange
        var responseContent = new StockResponse
        {
            ResponseCode = 0,
            ResponseMessage = "Success",
            Stocks = new List<StockItem>()
        };

        HttpRequestMessage? capturedRequest = null;
        var requestCount = 0;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
            {
                requestCount++;
                if (requestCount == 2) // Capture second request (first is token)
                {
                    capturedRequest = req;
                }
            })
            .ReturnsAsync(() =>
            {
                if (requestCount == 1)
                {
                    var tokenResponse = new EasyCarsTokenResponse { ResponseCode = 0, Token = "test-token" };
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonSerializer.Serialize(tokenResponse), Encoding.UTF8, "application/json")
                    };
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
                };
            });

        // Act
        await _sut.GetAdvertisementStocksAsync(
            "test-account",
            "test-secret",
            "Test",
            1,
            null,
            CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.DoesNotContain("yardCode", capturedRequest.RequestUri?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task GetAdvertisementStocksAsync_WithAuthenticationError_ThrowsEasyCarsAuthenticationException()
    {
        // Arrange
        var responseContent = new StockResponse
        {
            ResponseCode = 1,
            ResponseMessage = "Invalid credentials",
            Stocks = null
        };

        SetupHttpResponseWithToken(responseContent);

        // Act & Assert
        await Assert.ThrowsAsync<EasyCarsAuthenticationException>(() =>
            _sut.GetAdvertisementStocksAsync(
                "test-account",
                "test-secret",
                "Test",
                1,
                null,
                CancellationToken.None));
    }

    [Fact]
    public async Task GetAdvertisementStocksAsync_WithTemporaryError_ThrowsEasyCarsTemporaryException()
    {
        // Arrange
        var responseContent = new StockResponse
        {
            ResponseCode = 5,
            ResponseMessage = "Temporary service unavailable",
            Stocks = null
        };

        SetupHttpResponseWithToken(responseContent);

        // Act & Assert
        await Assert.ThrowsAsync<EasyCarsTemporaryException>(() =>
            _sut.GetAdvertisementStocksAsync(
                "test-account",
                "test-secret",
                "Test",
                1,
                null,
                CancellationToken.None));
    }

    [Fact]
    public async Task GetAdvertisementStocksAsync_WithValidationError_ThrowsEasyCarsValidationException()
    {
        // Arrange
        var responseContent = new StockResponse
        {
            ResponseCode = 7,
            ResponseMessage = "Invalid request parameters",
            Stocks = null
        };

        SetupHttpResponseWithToken(responseContent);

        // Act & Assert
        await Assert.ThrowsAsync<EasyCarsValidationException>(() =>
            _sut.GetAdvertisementStocksAsync(
                "test-account",
                "test-secret",
                "Test",
                1,
                null,
                CancellationToken.None));
    }

    [Fact]
    public async Task GetAdvertisementStocksAsync_WithFatalError_ThrowsEasyCarsFatalException()
    {
        // Arrange
        var responseContent = new StockResponse
        {
            ResponseCode = 9,
            ResponseMessage = "Fatal system error",
            Stocks = null
        };

        SetupHttpResponseWithToken(responseContent);

        // Act & Assert
        await Assert.ThrowsAsync<EasyCarsFatalException>(() =>
            _sut.GetAdvertisementStocksAsync(
                "test-account",
                "test-secret",
                "Test",
                1,
                null,
                CancellationToken.None));
    }

    [Fact]
    public async Task GetAdvertisementStocksAsync_WithCompleteStockData_MapsAllFields()
    {
        // Arrange
        var stockItem = new StockItem
        {
            // Primary Identification
            StockNumber = "ST12345",
            VIN = "1HGBH41JXMN109186",
            RegoNum = "ABC-123",
            YardCode = "YARD-01",
            StockType = "Used",
            
            // Vehicle Details
            Make = "Honda",
            Model = "Accord",
            Badge = "VTi-L",
            Year = 2023,
            Body = "Sedan",
            Colour = "Silver",
            Doors = 4,
            Seats = 5,
            Transmission = "Automatic",
            FuelType = "Petrol",
            EngineSize = "2.4L",
            Cylinders = 4,
            DriveType = "FWD",
            Odometer = 25000,
            RegistrationExpiry = "2025-12-31",
            
            // Pricing
            Price = 35000,
            CostPrice = 30000,
            RetailPrice = 38000,
            WeeklyCost = 150,
            PriceType = "Drive Away",
            NegotiablePrice = true,
            TaxIncluded = true,
            
            // Marketing
            Description = "Excellent condition sedan",
            KeyFeatures = "Navigation, Leather, Sunroof",
            FeaturedVehicle = true,
            Priority = 10,
            Status = "Available",
            Condition = "Excellent",
            
            // Images
            ImageURLs = new List<string> { "https://example.com/image1.jpg" },
            ImageCount = 5,
            
            // Timestamps
            DateAdded = new DateTime(2024, 1, 15),
            DateUpdated = new DateTime(2024, 1, 20),
            DaysInStock = 30,
            
            // Features
            AirConditioning = true,
            CruiseControl = true,
            ABS = true,
            Airbags = true,
            
            // Dealer Info
            DealerName = "Test Dealer",
            LocationCity = "Sydney",
            LocationState = "NSW",
            ContactPhone = "02-1234-5678",
            ContactEmail = "sales@dealer.com"
        };

        var responseContent = new StockResponse
        {
            ResponseCode = 0,
            ResponseMessage = "Success",
            Stocks = new List<StockItem> { stockItem }
        };

        SetupHttpResponseWithToken(responseContent);

        // Act
        var result = await _sut.GetAdvertisementStocksAsync(
            "test-account",
            "test-secret",
            "Test",
            1,
            null,
            CancellationToken.None);

        // Assert
        Assert.Single(result);
        var mappedItem = result[0];
        
        Assert.Equal("ST12345", mappedItem.StockNumber);
        Assert.Equal("1HGBH41JXMN109186", mappedItem.VIN);
        Assert.Equal("Honda", mappedItem.Make);
        Assert.Equal("Accord", mappedItem.Model);
        Assert.Equal(2023, mappedItem.Year);
        Assert.Equal(35000, mappedItem.Price);
        Assert.Equal("Available", mappedItem.Status);
        Assert.Equal(4, mappedItem.Doors);
        Assert.True(mappedItem.AirConditioning);
        Assert.Equal("Test Dealer", mappedItem.DealerName);
    }

    [Fact]
    public async Task GetAdvertisementStocksAsync_UsesGetHttpMethod()
    {
        // Arrange
        var responseContent = new StockResponse
        {
            ResponseCode = 0,
            ResponseMessage = "Success",
            Stocks = new List<StockItem>()
        };

        HttpRequestMessage? capturedRequest = null;
        var requestCount = 0;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
            {
                requestCount++;
                if (requestCount == 2) // Capture second request (first is token)
                {
                    capturedRequest = req;
                }
            })
            .ReturnsAsync(() =>
            {
                if (requestCount == 1)
                {
                    var tokenResponse = new EasyCarsTokenResponse { ResponseCode = 0, Token = "test-token" };
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonSerializer.Serialize(tokenResponse), Encoding.UTF8, "application/json")
                    };
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
                };
            });

        // Act
        await _sut.GetAdvertisementStocksAsync(
            "test-account",
            "test-secret",
            "Test",
            1,
            null,
            CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Get, capturedRequest.Method);
    }

    [Fact]
    public async Task GetAdvertisementStocksAsync_UsesCorrectEndpoint()
    {
        // Arrange
        var responseContent = new StockResponse
        {
            ResponseCode = 0,
            ResponseMessage = "Success",
            Stocks = new List<StockItem>()
        };

        HttpRequestMessage? capturedRequest = null;
        var requestCount = 0;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
            {
                requestCount++;
                if (requestCount == 2) // Capture second request (first is token)
                {
                    capturedRequest = req;
                }
            })
            .ReturnsAsync(() =>
            {
                if (requestCount == 1)
                {
                    var tokenResponse = new EasyCarsTokenResponse { ResponseCode = 0, Token = "test-token" };
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonSerializer.Serialize(tokenResponse), Encoding.UTF8, "application/json")
                    };
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(responseContent), Encoding.UTF8, "application/json")
                };
            });

        // Act
        await _sut.GetAdvertisementStocksAsync(
            "test-account",
            "test-secret",
            "Test",
            1,
            null,
            CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Contains("/Stock/GetAdvertisementStocks", capturedRequest.RequestUri?.ToString());
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, object content)
    {
        var json = JsonSerializer.Serialize(content);
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);
    }

    private void SetupHttpResponseWithToken(object stockResponseContent)
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
                        Content = new StringContent(JsonSerializer.Serialize(stockResponseContent), Encoding.UTF8, "application/json")
                    };
                }
            });
    }
}
