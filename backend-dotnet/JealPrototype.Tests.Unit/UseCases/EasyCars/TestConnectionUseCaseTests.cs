using FluentAssertions;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Interfaces;
using JealPrototype.Application.UseCases.EasyCars;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JealPrototype.Tests.Unit.UseCases.EasyCars;

public class TestConnectionUseCaseTests
{
    private readonly Mock<IEasyCarsApiClient> _mockApiClient;
    private readonly Mock<ILogger<TestConnectionUseCase>> _mockLogger;
    private readonly TestConnectionUseCase _useCase;

    public TestConnectionUseCaseTests()
    {
        _mockApiClient = new Mock<IEasyCarsApiClient>();
        _mockLogger = new Mock<ILogger<TestConnectionUseCase>>();
        _useCase = new TestConnectionUseCase(_mockApiClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCredentials_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new TestConnectionRequest
        {
            AccountNumber = "12345678-1234-1234-1234-123456789012",
            AccountSecret = "87654321-4321-4321-4321-210987654321",
            Environment = "Test"
        };

        var tokenResponse = new EasyCarsTokenResponse
        {
            ResponseCode = 0,
            Token = "jwt-token-here",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Message = "Success"
        };

        _mockApiClient.Setup(x => x.RequestTokenAsync(
            request.AccountNumber,
            request.AccountSecret,
            request.Environment,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenResponse);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("successful");
        result.Environment.Should().Be("Test");
        result.TokenExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidCredentials_ReturnsAuthFailureResponse()
    {
        // Arrange
        var request = new TestConnectionRequest
        {
            AccountNumber = "12345678-1234-1234-1234-123456789012",
            AccountSecret = "wrong-secret-key",
            Environment = "Production"
        };

        var tokenResponse = new EasyCarsTokenResponse
        {
            ResponseCode = 1,
            ErrorMessage = "Authentication failed",
            ErrorCode = "AUTH_FAILED"
        };

        _mockApiClient.Setup(x => x.RequestTokenAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenResponse);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Authentication failed");
        result.Environment.Should().Be("Production");
        result.ErrorCode.Should().Be("AUTH_FAILED");
    }

    [Fact]
    public async Task ExecuteAsync_WithTimeout_ReturnsTimeoutResponse()
    {
        // Arrange
        var request = new TestConnectionRequest
        {
            AccountNumber = "12345678-1234-1234-1234-123456789012",
            AccountSecret = "87654321-4321-4321-4321-210987654321",
            Environment = "Test"
        };

        var tokenResponse = new EasyCarsTokenResponse
        {
            ResponseCode = 1,
            ErrorMessage = "Request timed out after 10 seconds",
            ErrorCode = "TIMEOUT"
        };

        _mockApiClient.Setup(x => x.RequestTokenAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenResponse);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("timed out");
        result.ErrorCode.Should().Be("TIMEOUT");
    }

    [Fact]
    public async Task ExecuteAsync_WithNetworkError_ReturnsNetworkErrorResponse()
    {
        // Arrange
        var request = new TestConnectionRequest
        {
            AccountNumber = "12345678-1234-1234-1234-123456789012",
            AccountSecret = "87654321-4321-4321-4321-210987654321",
            Environment = "Production"
        };

        var tokenResponse = new EasyCarsTokenResponse
        {
            ResponseCode = 1,
            ErrorMessage = "Connection refused",
            ErrorCode = "NETWORK_ERROR"
        };

        _mockApiClient.Setup(x => x.RequestTokenAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenResponse);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Unable to reach");
        result.ErrorCode.Should().Be("NETWORK_ERROR");
        result.Details.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiClientThrowsTaskCanceledException_ReturnsTimeoutResponse()
    {
        // Arrange
        var request = new TestConnectionRequest
        {
            AccountNumber = "12345678-1234-1234-1234-123456789012",
            AccountSecret = "87654321-4321-4321-4321-210987654321",
            Environment = "Test"
        };

        _mockApiClient.Setup(x => x.RequestTokenAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException());

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("TIMEOUT");
    }

    [Fact]
    public async Task ExecuteAsync_WhenApiClientThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        var request = new TestConnectionRequest
        {
            AccountNumber = "12345678-1234-1234-1234-123456789012",
            AccountSecret = "87654321-4321-4321-4321-210987654321",
            Environment = "Test"
        };

        _mockApiClient.Setup(x => x.RequestTokenAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("UNEXPECTED_ERROR");
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ThrowsTaskCanceledException()
    {
        // Arrange
        var request = new TestConnectionRequest
        {
            AccountNumber = "12345678-1234-1234-1234-123456789012",
            AccountSecret = "87654321-4321-4321-4321-210987654321",
            Environment = "Test"
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockApiClient.Setup(x => x.RequestTokenAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException(null, null, cts.Token));

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => 
            _useCase.ExecuteAsync(request, cts.Token));
    }
}
