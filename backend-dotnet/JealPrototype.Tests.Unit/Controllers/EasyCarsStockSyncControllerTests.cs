using FluentAssertions;
using JealPrototype.API.Controllers;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace JealPrototype.Tests.Unit.Controllers;

public class EasyCarsStockSyncControllerTests
{
    private readonly Mock<IEasyCarsSyncLogRepository> _mockSyncLogRepository;
    private readonly Mock<IEasyCarsCredentialRepository> _mockCredentialRepository;
    private readonly Mock<ILogger<EasyCarsStockSyncController>> _mockLogger;
    private readonly EasyCarsStockSyncController _controller;

    public EasyCarsStockSyncControllerTests()
    {
        _mockSyncLogRepository = new Mock<IEasyCarsSyncLogRepository>();
        _mockCredentialRepository = new Mock<IEasyCarsCredentialRepository>();
        _mockLogger = new Mock<ILogger<EasyCarsStockSyncController>>();

        _controller = new EasyCarsStockSyncController(
            _mockSyncLogRepository.Object,
            _mockCredentialRepository.Object,
            _mockLogger.Object);

        // Setup user claims for authentication
        var claims = new List<Claim>
        {
            new Claim("DealershipId", "1")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetSyncStatus_ReturnsOkWithStatus()
    {
        // Arrange
        var dealershipId = 1;
        var syncLog = EasyCarsSyncLog.Create(
            dealershipId,
            SyncStatus.Success,
            10,
            10,
            0,
            new List<string>(),
            5000);

        var credential = EasyCarsCredential.Create(
            dealershipId,
            "encryptedNumber",
            "encryptedSecret",
            "iv",
            "Test");

        _mockSyncLogRepository
            .Setup(x => x.GetLastSyncAsync(dealershipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(syncLog);

        _mockCredentialRepository
            .Setup(x => x.GetByDealershipIdAsync(dealershipId))
            .ReturnsAsync(credential);

        // Act
        var result = await _controller.GetSyncStatus(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<SyncStatusDto>().Subject;
        response.Status.Should().Be(SyncStatus.Success);
        response.ItemsProcessed.Should().Be(10);
        response.HasCredentials.Should().BeTrue();
    }

    [Fact]
    public async Task GetSyncStatus_NoSyncHistory_ReturnsStatusWithNulls()
    {
        // Arrange
        var dealershipId = 1;

        _mockSyncLogRepository
            .Setup(x => x.GetLastSyncAsync(dealershipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog?)null);

        _mockCredentialRepository
            .Setup(x => x.GetByDealershipIdAsync(dealershipId))
            .ReturnsAsync((EasyCarsCredential?)null);

        // Act
        var result = await _controller.GetSyncStatus(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<SyncStatusDto>().Subject;
        response.LastSyncedAt.Should().BeNull();
        response.HasCredentials.Should().BeFalse();
    }

    [Fact]
    public async Task GetSyncHistory_ReturnsPaginatedLogs()
    {
        // Arrange
        var dealershipId = 1;
        var logs = new List<EasyCarsSyncLog>
        {
            EasyCarsSyncLog.Create(dealershipId, SyncStatus.Success, 10, 10, 0, new List<string>(), 5000),
            EasyCarsSyncLog.Create(dealershipId, SyncStatus.PartialSuccess, 10, 8, 2, new List<string>(), 6000)
        };

        _mockSyncLogRepository
            .Setup(x => x.GetPagedHistoryAsync(dealershipId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((logs, 2));

        // Act
        var result = await _controller.GetSyncHistory(1, 10, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<SyncHistoryResponse>().Subject;
        response.Logs.Should().HaveCount(2);
        response.Total.Should().Be(2);
        response.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetSyncLogDetails_ReturnsDetails()
    {
        // Arrange
        var dealershipId = 1;
        var logId = 1;
        var errors = new List<string> { "Error 1", "Error 2" };
        var syncLog = EasyCarsSyncLog.Create(
            dealershipId,
            SyncStatus.Failed,
            10,
            8,
            2,
            errors,
            5000);

        _mockSyncLogRepository
            .Setup(x => x.GetByIdAsync(logId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(syncLog);

        // Act
        var result = await _controller.GetSyncLogDetails(logId, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<SyncLogDetailsDto>().Subject;
        response.Status.Should().Be(SyncStatus.Failed);
        response.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSyncLogDetails_NotFound_ReturnsNotFound()
    {
        // Arrange
        var logId = 999;

        _mockSyncLogRepository
            .Setup(x => x.GetByIdAsync(logId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog?)null);

        // Act
        var result = await _controller.GetSyncLogDetails(logId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task TriggerSync_NoCredentials_ReturnsBadRequest()
    {
        // Arrange
        var dealershipId = 1;

        _mockCredentialRepository
            .Setup(x => x.GetByDealershipIdAsync(dealershipId))
            .ReturnsAsync((EasyCarsCredential?)null);

        // Act
        var result = await _controller.TriggerSync(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}
