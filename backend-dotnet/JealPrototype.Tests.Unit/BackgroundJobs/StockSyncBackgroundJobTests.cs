using FluentAssertions;
using JealPrototype.Application.BackgroundJobs;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JealPrototype.Tests.Unit.BackgroundJobs;

public class StockSyncBackgroundJobTests
{
    private readonly Mock<IEasyCarsStockSyncService> _mockSyncService;
    private readonly Mock<IEasyCarsCredentialRepository> _mockCredentialRepo;
    private readonly Mock<IDealershipSettingsRepository> _mockSettingsRepo;
    private readonly Mock<ISystemSettingsRepository> _mockSystemSettingsRepo;
    private readonly Mock<ILogger<StockSyncBackgroundJob>> _mockLogger;
    private readonly StockSyncBackgroundJob _job;
    
    public StockSyncBackgroundJobTests()
    {
        _mockSyncService = new Mock<IEasyCarsStockSyncService>();
        _mockCredentialRepo = new Mock<IEasyCarsCredentialRepository>();
        _mockSettingsRepo = new Mock<IDealershipSettingsRepository>();
        _mockSystemSettingsRepo = new Mock<ISystemSettingsRepository>();
        _mockLogger = new Mock<ILogger<StockSyncBackgroundJob>>();
        
        _job = new StockSyncBackgroundJob(
            _mockSyncService.Object,
            _mockCredentialRepo.Object,
            _mockSettingsRepo.Object,
            _mockSystemSettingsRepo.Object,
            _mockLogger.Object);
    }
    
    [Fact]
    public async Task ExecuteAsync_WithEligibleDealerships_ProcessesAllSuccessfully()
    {
        // Arrange
        _mockSystemSettingsRepo.Setup(x => x.GetBoolValueAsync("easycar_sync_enabled", true, default))
            .ReturnsAsync(true);
        
        _mockSettingsRepo.Setup(x => x.GetDealershipsWithAutoSyncEnabledAsync(default))
            .ReturnsAsync(new List<int> { 1, 2, 3 });
        
        _mockCredentialRepo.Setup(x => x.GetByDealershipIdAsync(It.IsAny<int>()))
            .ReturnsAsync(EasyCarsCredential.Create(1, "encrypted_account", "encrypted_secret", "iv123", "Test"));
        
        _mockSyncService.Setup(x => x.SyncStockAsync(It.IsAny<int>(), default))
            .ReturnsAsync(SyncResult.Success(10, 5000));
        
        // Act
        await _job.ExecuteAsync();
        
        // Assert
        _mockSyncService.Verify(x => x.SyncStockAsync(1, default), Times.Once);
        _mockSyncService.Verify(x => x.SyncStockAsync(2, default), Times.Once);
        _mockSyncService.Verify(x => x.SyncStockAsync(3, default), Times.Once);
    }
    
    [Fact]
    public async Task ExecuteAsync_WithNoDealerships_CompletesWithoutProcessing()
    {
        // Arrange
        _mockSystemSettingsRepo.Setup(x => x.GetBoolValueAsync("easycar_sync_enabled", true, default))
            .ReturnsAsync(true);
        
        _mockSettingsRepo.Setup(x => x.GetDealershipsWithAutoSyncEnabledAsync(default))
            .ReturnsAsync(new List<int>());
        
        // Act
        await _job.ExecuteAsync();
        
        // Assert
        _mockSyncService.Verify(x => x.SyncStockAsync(It.IsAny<int>(), default), Times.Never);
    }
    
    [Fact]
    public async Task ExecuteAsync_WithGlobalSyncDisabled_ExitsEarly()
    {
        // Arrange
        _mockSystemSettingsRepo.Setup(x => x.GetBoolValueAsync("easycar_sync_enabled", true, default))
            .ReturnsAsync(false);
        
        // Act
        await _job.ExecuteAsync();
        
        // Assert
        _mockSyncService.Verify(x => x.SyncStockAsync(It.IsAny<int>(), default), Times.Never);
        _mockSettingsRepo.Verify(x => x.GetDealershipsWithAutoSyncEnabledAsync(default), Times.Never);
    }
    
    [Fact]
    public async Task ExecuteAsync_WithDealershipsWithoutCredentials_SkipsThem()
    {
        // Arrange
        _mockSystemSettingsRepo.Setup(x => x.GetBoolValueAsync("easycar_sync_enabled", true, default))
            .ReturnsAsync(true);
        
        _mockSettingsRepo.Setup(x => x.GetDealershipsWithAutoSyncEnabledAsync(default))
            .ReturnsAsync(new List<int> { 1, 2 });
        
        // Dealership 1 has credentials, dealership 2 doesn't
        _mockCredentialRepo.Setup(x => x.GetByDealershipIdAsync(1))
            .ReturnsAsync(EasyCarsCredential.Create(1, "encrypted_account", "encrypted_secret", "iv123", "Test"));
        _mockCredentialRepo.Setup(x => x.GetByDealershipIdAsync(2))
            .ReturnsAsync((EasyCarsCredential?)null);
        
        _mockSyncService.Setup(x => x.SyncStockAsync(1, default))
            .ReturnsAsync(SyncResult.Success(10, 5000));
        
        // Act
        await _job.ExecuteAsync();
        
        // Assert
        _mockSyncService.Verify(x => x.SyncStockAsync(1, default), Times.Once);
        _mockSyncService.Verify(x => x.SyncStockAsync(2, default), Times.Never);
    }
    
    [Fact]
    public async Task ExecuteAsync_WithIndividualDealershipFailure_ContinuesToNextDealership()
    {
        // Arrange
        _mockSystemSettingsRepo.Setup(x => x.GetBoolValueAsync("easycar_sync_enabled", true, default))
            .ReturnsAsync(true);
        
        _mockSettingsRepo.Setup(x => x.GetDealershipsWithAutoSyncEnabledAsync(default))
            .ReturnsAsync(new List<int> { 1, 2, 3 });
        
        _mockCredentialRepo.Setup(x => x.GetByDealershipIdAsync(It.IsAny<int>()))
            .ReturnsAsync(EasyCarsCredential.Create(1, "encrypted_account", "encrypted_secret", "iv123", "Test"));
        
        // Dealership 1 succeeds, 2 fails, 3 succeeds
        _mockSyncService.Setup(x => x.SyncStockAsync(1, default))
            .ReturnsAsync(SyncResult.Success(10, 5000));
        _mockSyncService.Setup(x => x.SyncStockAsync(2, default))
            .ThrowsAsync(new InvalidOperationException("Test failure"));
        _mockSyncService.Setup(x => x.SyncStockAsync(3, default))
            .ReturnsAsync(SyncResult.Success(5, 3000));
        
        // Act
        await _job.ExecuteAsync();
        
        // Assert
        _mockSyncService.Verify(x => x.SyncStockAsync(1, default), Times.Once);
        _mockSyncService.Verify(x => x.SyncStockAsync(2, default), Times.Once);
        _mockSyncService.Verify(x => x.SyncStockAsync(3, default), Times.Once);
    }
    
    [Fact]
    public async Task ExecuteAsync_WithPartialSuccess_ProcessesSuccessfully()
    {
        // Arrange
        _mockSystemSettingsRepo.Setup(x => x.GetBoolValueAsync("easycar_sync_enabled", true, default))
            .ReturnsAsync(true);
        
        _mockSettingsRepo.Setup(x => x.GetDealershipsWithAutoSyncEnabledAsync(default))
            .ReturnsAsync(new List<int> { 1 });
        
        _mockCredentialRepo.Setup(x => x.GetByDealershipIdAsync(1))
            .ReturnsAsync(EasyCarsCredential.Create(1, "encrypted_account", "encrypted_secret", "iv123", "Test"));
        
        _mockSyncService.Setup(x => x.SyncStockAsync(1, default))
            .ReturnsAsync(SyncResult.PartialSuccess(7, 3, new List<string> { "Error 1", "Error 2", "Error 3" }, 5000));
        
        // Act
        await _job.ExecuteAsync();
        
        // Assert
        _mockSyncService.Verify(x => x.SyncStockAsync(1, default), Times.Once);
    }
    
    [Fact]
    public async Task ExecuteAsync_WithAllFailures_CompletesButLogsErrors()
    {
        // Arrange
        _mockSystemSettingsRepo.Setup(x => x.GetBoolValueAsync("easycar_sync_enabled", true, default))
            .ReturnsAsync(true);
        
        _mockSettingsRepo.Setup(x => x.GetDealershipsWithAutoSyncEnabledAsync(default))
            .ReturnsAsync(new List<int> { 1, 2 });
        
        _mockCredentialRepo.Setup(x => x.GetByDealershipIdAsync(It.IsAny<int>()))
            .ReturnsAsync(EasyCarsCredential.Create(1, "encrypted_account", "encrypted_secret", "iv123", "Test"));
        
        _mockSyncService.Setup(x => x.SyncStockAsync(It.IsAny<int>(), default))
            .ReturnsAsync(SyncResult.Failure("Sync failed"));
        
        // Act
        await _job.ExecuteAsync();
        
        // Assert
        _mockSyncService.Verify(x => x.SyncStockAsync(It.IsAny<int>(), default), Times.Exactly(2));
    }
}
