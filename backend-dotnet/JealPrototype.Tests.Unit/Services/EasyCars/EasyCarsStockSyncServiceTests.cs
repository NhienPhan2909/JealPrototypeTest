using FluentAssertions;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Interfaces;
using JealPrototype.Application.Interfaces.Security;
using JealPrototype.Application.Services.EasyCars;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JealPrototype.Tests.Unit.Services.EasyCars;

/// <summary>
/// Unit tests for EasyCarsStockSyncService (Story 2.3)
/// Covers all 16+ test scenarios specified in AC10
/// </summary>
public class EasyCarsStockSyncServiceTests
{
    private readonly Mock<IEasyCarsCredentialRepository> _mockCredentialRepo;
    private readonly Mock<ICredentialEncryptionService> _mockEncryptionService;
    private readonly Mock<IEasyCarsApiClient> _mockApiClient;
    private readonly Mock<IEasyCarsStockMapper> _mockMapper;
    private readonly Mock<IEasyCarsSyncLogRepository> _mockSyncLogRepo;
    private readonly Mock<ILogger<EasyCarsStockSyncService>> _mockLogger;
    private readonly EasyCarsStockSyncService _sut;

    public EasyCarsStockSyncServiceTests()
    {
        _mockCredentialRepo = new Mock<IEasyCarsCredentialRepository>();
        _mockEncryptionService = new Mock<ICredentialEncryptionService>();
        _mockApiClient = new Mock<IEasyCarsApiClient>();
        _mockMapper = new Mock<IEasyCarsStockMapper>();
        _mockSyncLogRepo = new Mock<IEasyCarsSyncLogRepository>();
        _mockLogger = new Mock<ILogger<EasyCarsStockSyncService>>();

        _sut = new EasyCarsStockSyncService(
            _mockCredentialRepo.Object,
            _mockEncryptionService.Object,
            _mockApiClient.Object,
            _mockMapper.Object,
            _mockSyncLogRepo.Object,
            _mockLogger.Object
        );
    }

    #region Test Suite: Successful Sync (5+ tests)

    [Fact]
    public async Task SyncStockAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var dealershipId = 1;
        var credential = CreateTestCredential(dealershipId);
        var stockItems = CreateTestStockItems(3);

        SetupSuccessfulSync(dealershipId, credential, stockItems);

        // Act
        var result = await _sut.SyncStockAsync(dealershipId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be(SyncStatus.Success);
        result.ItemsProcessed.Should().Be(3);
        result.ItemsSucceeded.Should().Be(3);
        result.ItemsFailed.Should().Be(0);
        result.Errors.Should().BeEmpty();
        result.DurationMs.Should().BeGreaterThanOrEqualTo(0); // Changed from > to >= since mocks are fast
    }

    [Fact]
    public async Task SyncStockAsync_WithMultipleStockItems_ProcessesAll()
    {
        // Arrange
        var dealershipId = 1;
        var credential = CreateTestCredential(dealershipId);
        var stockItems = CreateTestStockItems(10);

        SetupSuccessfulSync(dealershipId, credential, stockItems);

        // Act
        var result = await _sut.SyncStockAsync(dealershipId);

        // Assert
        result.ItemsProcessed.Should().Be(10);
        result.ItemsSucceeded.Should().Be(10);
        _mockMapper.Verify(m => m.MapToVehicleAsync(
            It.IsAny<StockItem>(),
            dealershipId,
            It.IsAny<CancellationToken>()), Times.Exactly(10));
    }

    [Fact]
    public async Task SyncStockAsync_CreatesAuditLogEntry()
    {
        // Arrange
        var dealershipId = 1;
        var credential = CreateTestCredential(dealershipId);
        var stockItems = CreateTestStockItems(5);

        SetupSuccessfulSync(dealershipId, credential, stockItems);

        // Act
        await _sut.SyncStockAsync(dealershipId);

        // Assert
        _mockSyncLogRepo.Verify(r => r.AddAsync(
            It.Is<EasyCarsSyncLog>(log =>
                log.DealershipId == dealershipId &&
                log.Status == SyncStatus.Success &&
                log.ItemsProcessed == 5 &&
                log.ItemsSucceeded == 5 &&
                log.ItemsFailed == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncStockAsync_WithExistingVehicles_UpdatesThem()
    {
        // Arrange - Simulates idempotency (mapper handles duplicate detection)
        var dealershipId = 1;
        var credential = CreateTestCredential(dealershipId);
        var stockItems = CreateTestStockItems(3);

        SetupSuccessfulSync(dealershipId, credential, stockItems);

        // Act - Run sync twice
        var result1 = await _sut.SyncStockAsync(dealershipId);
        var result2 = await _sut.SyncStockAsync(dealershipId);

        // Assert - Both syncs succeed (idempotent)
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        _mockMapper.Verify(m => m.MapToVehicleAsync(
            It.IsAny<StockItem>(),
            dealershipId,
            It.IsAny<CancellationToken>()), Times.Exactly(6)); // 3 items x 2 runs
    }

    [Fact]
    public async Task SyncStockAsync_ReturnsCorrectDuration()
    {
        // Arrange
        var dealershipId = 1;
        var credential = CreateTestCredential(dealershipId);
        var stockItems = CreateTestStockItems(2);

        SetupSuccessfulSync(dealershipId, credential, stockItems);

        // Add delay to mapper to simulate work
        _mockMapper.Setup(m => m.MapToVehicleAsync(
            It.IsAny<StockItem>(),
            dealershipId,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockItem item, int id, CancellationToken ct) =>
            {
                Task.Delay(50, ct).Wait(ct);
                return CreateTestMapResult(id);
            });

        // Act
        var result = await _sut.SyncStockAsync(dealershipId);

        // Assert
        result.DurationMs.Should().BeGreaterThanOrEqualTo(100); // 2 items x 50ms
    }

    #endregion

    #region Test Suite: Partial Failures (4+ tests)

    [Fact]
    public async Task SyncStockAsync_WithSomeFailures_ReturnsPartialSuccess()
    {
        // Arrange
        var dealershipId = 1;
        var credential = CreateTestCredential(dealershipId);
        var stockItems = CreateTestStockItems(5);

        SetupPartialFailureSync(dealershipId, credential, stockItems, failEveryOther: true);

        // Act
        var result = await _sut.SyncStockAsync(dealershipId);

        // Assert
        result.Status.Should().Be(SyncStatus.PartialSuccess);
        result.IsPartialSuccess.Should().BeTrue();
        result.ItemsProcessed.Should().Be(5);
        result.ItemsSucceeded.Should().Be(3); // Items 0, 2, 4 succeed
        result.ItemsFailed.Should().Be(2); // Items 1, 3 fail
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task SyncStockAsync_WithSomeFailures_ProcessesRemainingItems()
    {
        // Arrange
        var dealershipId = 1;
        var credential = CreateTestCredential(dealershipId);
        var stockItems = CreateTestStockItems(10);

        // Fail first 3 items, succeed remaining 7
        SetupCredentialRetrieval(dealershipId, credential);
        SetupApiCall(dealershipId, credential, stockItems);

        var callCount = 0;
        _mockMapper.Setup(m => m.MapToVehicleAsync(
            It.IsAny<StockItem>(),
            dealershipId,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockItem item, int id, CancellationToken ct) =>
            {
                if (callCount++ < 3)
                    throw new Exception($"Mapping failed for {item.StockNumber}");
                return CreateTestMapResult(id);
            });

        _mockSyncLogRepo.Setup(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog log, CancellationToken ct) => log);

        // Act
        var result = await _sut.SyncStockAsync(dealershipId);

        // Assert
        result.ItemsSucceeded.Should().Be(7);
        result.ItemsFailed.Should().Be(3);
        _mockMapper.Verify(m => m.MapToVehicleAsync(
            It.IsAny<StockItem>(),
            dealershipId,
            It.IsAny<CancellationToken>()), Times.Exactly(10)); // All items processed
    }

    [Fact]
    public async Task SyncStockAsync_WithSomeFailures_IncludesErrorMessages()
    {
        // Arrange
        var dealershipId = 1;
        var credential = CreateTestCredential(dealershipId);
        var stockItems = CreateTestStockItems(3);

        SetupCredentialRetrieval(dealershipId, credential);
        SetupApiCall(dealershipId, credential, stockItems);

        _mockMapper.SetupSequence(m => m.MapToVehicleAsync(
            It.IsAny<StockItem>(),
            dealershipId,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestMapResult(dealershipId))
            .ThrowsAsync(new Exception("Invalid VIN format"))
            .ReturnsAsync(CreateTestMapResult(dealershipId));

        _mockSyncLogRepo.Setup(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog log, CancellationToken ct) => log);

        // Act
        var result = await _sut.SyncStockAsync(dealershipId);

        // Assert
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("STOCK002");
        result.Errors[0].Should().Contain("Invalid VIN format");
    }

    [Fact]
    public async Task SyncStockAsync_WithSomeFailures_LogsFailedItems()
    {
        // Arrange
        var dealershipId = 1;
        var credential = CreateTestCredential(dealershipId);
        var stockItems = CreateTestStockItems(2);

        SetupPartialFailureSync(dealershipId, credential, stockItems, failEveryOther: true);

        // Act
        await _sut.SyncStockAsync(dealershipId);

        // Assert - Verify warning log for failed item
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("STOCK002")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Test Suite: Complete Failures (4+ tests)

    [Fact]
    public async Task SyncStockAsync_WithNoCredentials_ReturnsFailed()
    {
        // Arrange
        var dealershipId = 1;
        _mockCredentialRepo.Setup(r => r.GetByDealershipIdAsync(dealershipId))
            .ReturnsAsync((EasyCarsCredential?)null);

        _mockSyncLogRepo.Setup(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog log, CancellationToken ct) => log);

        // Act
        var result = await _sut.SyncStockAsync(dealershipId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("No active credentials"));
    }

    [Fact]
    public async Task SyncStockAsync_WithApiFailure_ReturnsFailed()
    {
        // Arrange
        var dealershipId = 1;
        var credential = CreateTestCredential(dealershipId);

        SetupCredentialRetrieval(dealershipId, credential);

        _mockApiClient.Setup(a => a.GetAdvertisementStocksAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            dealershipId,
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API connection timeout"));

        _mockSyncLogRepo.Setup(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog log, CancellationToken ct) => log);

        // Act
        var result = await _sut.SyncStockAsync(dealershipId);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Status.Should().Be(SyncStatus.Failed);
        result.Errors.Should().Contain(e => e.Contains("API connection timeout"));
    }

    [Fact]
    public async Task SyncStockAsync_WithAllMappingFailures_ReturnsFailed()
    {
        // Arrange
        var dealershipId = 1;
        var credential = CreateTestCredential(dealershipId);
        var stockItems = CreateTestStockItems(5);

        SetupCredentialRetrieval(dealershipId, credential);
        SetupApiCall(dealershipId, credential, stockItems);

        // All mappers fail
        _mockMapper.Setup(m => m.MapToVehicleAsync(
            It.IsAny<StockItem>(),
            dealershipId,
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Mapper failed"));

        _mockSyncLogRepo.Setup(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog log, CancellationToken ct) => log);

        // Act
        var result = await _sut.SyncStockAsync(dealershipId);

        // Assert - Service handles all failures gracefully
        result.Status.Should().Be(SyncStatus.Failed);
        result.IsFailed.Should().BeTrue();
        result.ItemsSucceeded.Should().Be(0);
        result.Errors.Should().NotBeEmpty(); // At least one error message
    }

    [Fact]
    public async Task SyncStockAsync_WithSyncLogFailure_CompletesSuccessfully()
    {
        // Arrange - Even if audit log fails, sync should succeed
        var dealershipId = 1;
        var credential = CreateTestCredential(dealershipId);
        var stockItems = CreateTestStockItems(2);

        SetupCredentialRetrieval(dealershipId, credential);
        SetupApiCall(dealershipId, credential, stockItems);

        _mockMapper.Setup(m => m.MapToVehicleAsync(
            It.IsAny<StockItem>(),
            dealershipId,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestMapResult(dealershipId));

        _mockSyncLogRepo.Setup(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.SyncStockAsync(dealershipId);

        // Assert - Sync succeeds even though audit log failed
        result.IsSuccess.Should().BeTrue();
        result.ItemsSucceeded.Should().Be(2);
    }

    #endregion

    #region Test Suite: Edge Cases (3+ tests)

    [Fact]
    public async Task SyncStockAsync_WithNoStockItems_ReturnsSuccess()
    {
        // Arrange
        var dealershipId = 1;
        var credential = CreateTestCredential(dealershipId);
        var emptyList = new List<StockItem>();

        SetupSuccessfulSync(dealershipId, credential, emptyList);

        // Act
        var result = await _sut.SyncStockAsync(dealershipId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ItemsProcessed.Should().Be(0);
        result.ItemsSucceeded.Should().Be(0);
        result.ItemsFailed.Should().Be(0);
    }

    [Fact]
    public async Task SyncStockAsync_WithCancellationToken_HandlesGracefully()
    {
        // Arrange - Cancellation happens but is caught at top level
        var dealershipId = 1;
        var credential = CreateTestCredential(dealershipId);
        var stockItems = CreateTestStockItems(10);

        SetupCredentialRetrieval(dealershipId, credential);
        SetupApiCall(dealershipId, credential, stockItems);

        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        _mockMapper.Setup(m => m.MapToVehicleAsync(
            It.IsAny<StockItem>(),
            dealershipId,
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        _mockSyncLogRepo.Setup(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog log, CancellationToken ct) => log);

        // Act
        var result = await _sut.SyncStockAsync(dealershipId, cts.Token);

        // Assert - Returns Failed result (exception caught at top level)
        result.Status.Should().Be(SyncStatus.Failed);
        result.Errors.Should().Contain(e => e.Contains("canceled") || e.Contains("cancelled") || e.Contains("OperationCanceledException"));
    }

    [Fact]
    public async Task SyncStockAsync_RunTwice_IsIdempotent()
    {
        // Arrange
        var dealershipId = 1;
        var credential = CreateTestCredential(dealershipId);
        var stockItems = CreateTestStockItems(5);

        SetupSuccessfulSync(dealershipId, credential, stockItems);

        // Act - Run sync twice with same data
        var result1 = await _sut.SyncStockAsync(dealershipId);
        var result2 = await _sut.SyncStockAsync(dealershipId);

        // Assert - Both succeed with same results
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.ItemsProcessed.Should().Be(result2.ItemsProcessed);
        result1.ItemsSucceeded.Should().Be(result2.ItemsSucceeded);
    }

    #endregion

    #region Helper Methods

    private EasyCarsCredential CreateTestCredential(int dealershipId)
    {
        return EasyCarsCredential.Create(
            dealershipId,
            "encrypted-account-123",
            "encrypted-secret-456",
            "iv-789",
            "Test",
            true,
            "YARD001"
        );
    }

    private List<StockItem> CreateTestStockItems(int count)
    {
        var items = new List<StockItem>();
        for (int i = 1; i <= count; i++)
        {
            items.Add(new StockItem
            {
                StockNumber = $"STOCK{i:D3}",
                VIN = $"VIN{i:D17}",
                Make = "Toyota",
                Model = "Camry",
                Year = 2020 + i,
                Price = 25000m + (i * 1000),
                Odometer = 10000 + (i * 500),
                Status = "Available"
            });
        }
        return items;
    }

    private Vehicle CreateTestVehicle(int dealershipId)
    {
        return Vehicle.Create(
            dealershipId,
            "Toyota",
            "Camry",
            2022,
            25000m,
            10000,
            VehicleCondition.New,
            VehicleStatus.Active,
            "2022 Toyota Camry",
            "Excellent condition with low mileage"
        );
    }

    private StockMapResult CreateTestMapResult(int dealershipId) =>
        new(CreateTestVehicle(dealershipId), 0, 0);

    private void SetupSuccessfulSync(int dealershipId, EasyCarsCredential credential, List<StockItem> stockItems)
    {
        SetupCredentialRetrieval(dealershipId, credential);
        SetupApiCall(dealershipId, credential, stockItems);

        _mockMapper.Setup(m => m.MapToVehicleAsync(
            It.IsAny<StockItem>(),
            dealershipId,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestMapResult(dealershipId));

        _mockSyncLogRepo.Setup(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog log, CancellationToken ct) => log);
    }

    private void SetupPartialFailureSync(int dealershipId, EasyCarsCredential credential, List<StockItem> stockItems, bool failEveryOther)
    {
        SetupCredentialRetrieval(dealershipId, credential);
        SetupApiCall(dealershipId, credential, stockItems);

        var callCount = 0;
        _mockMapper.Setup(m => m.MapToVehicleAsync(
            It.IsAny<StockItem>(),
            dealershipId,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockItem item, int id, CancellationToken ct) =>
            {
                if (failEveryOther && callCount++ % 2 == 1)
                    throw new Exception($"Mapping failed for {item.StockNumber}");
                return CreateTestMapResult(id);
            });

        _mockSyncLogRepo.Setup(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog log, CancellationToken ct) => log);
    }

    private void SetupCredentialRetrieval(int dealershipId, EasyCarsCredential credential)
    {
        _mockCredentialRepo.Setup(r => r.GetByDealershipIdAsync(dealershipId))
            .ReturnsAsync(credential);

        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.AccountNumberEncrypted))
            .ReturnsAsync("decrypted-account-123");

        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.AccountSecretEncrypted))
            .ReturnsAsync("decrypted-secret-456");
    }

    private void SetupApiCall(int dealershipId, EasyCarsCredential credential, List<StockItem> stockItems)
    {
        _mockApiClient.Setup(a => a.GetAdvertisementStocksAsync(
            "decrypted-account-123",
            "decrypted-secret-456",
            credential.Environment,
            dealershipId,
            credential.YardCode,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(stockItems);
    }

    #endregion
}

