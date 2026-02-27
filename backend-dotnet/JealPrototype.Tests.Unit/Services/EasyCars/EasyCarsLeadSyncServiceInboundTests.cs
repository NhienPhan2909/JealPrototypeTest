using FluentAssertions;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Interfaces;
using JealPrototype.Application.Interfaces.Security;
using JealPrototype.Application.Services.EasyCars;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JealPrototype.Tests.Unit.Services.EasyCars;

/// <summary>
/// Unit tests for EasyCarsLeadSyncService.SyncLeadsFromEasyCarsAsync (Story 3.4)
/// Covers 10 test scenarios for inbound lead synchronization.
/// </summary>
public class EasyCarsLeadSyncServiceInboundTests
{
    private readonly Mock<ILeadRepository> _mockLeadRepo;
    private readonly Mock<IVehicleRepository> _mockVehicleRepo;
    private readonly Mock<IEasyCarsCredentialRepository> _mockCredentialRepo;
    private readonly Mock<ICredentialEncryptionService> _mockEncryptionService;
    private readonly Mock<IEasyCarsApiClient> _mockApiClient;
    private readonly Mock<IEasyCarsLeadMapper> _mockLeadMapper;
    private readonly Mock<IEasyCarsSyncLogRepository> _mockSyncLogRepo;
    private readonly Mock<ILogger<EasyCarsLeadSyncService>> _mockLogger;
    private readonly Mock<ILeadStatusConflictRepository> _mockConflictRepo;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly EasyCarsLeadSyncService _sut;

    public EasyCarsLeadSyncServiceInboundTests()
    {
        _mockLeadRepo = new Mock<ILeadRepository>();
        _mockVehicleRepo = new Mock<IVehicleRepository>();
        _mockCredentialRepo = new Mock<IEasyCarsCredentialRepository>();
        _mockEncryptionService = new Mock<ICredentialEncryptionService>();
        _mockApiClient = new Mock<IEasyCarsApiClient>();
        _mockLeadMapper = new Mock<IEasyCarsLeadMapper>();
        _mockSyncLogRepo = new Mock<IEasyCarsSyncLogRepository>();
        _mockLogger = new Mock<ILogger<EasyCarsLeadSyncService>>();
        _mockConflictRepo = new Mock<ILeadStatusConflictRepository>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c["EasyCars:LeadStatusConflictResolutionStrategy"]).Returns("RemoteWins");

        _sut = new EasyCarsLeadSyncService(
            _mockLeadRepo.Object,
            _mockVehicleRepo.Object,
            _mockCredentialRepo.Object,
            _mockEncryptionService.Object,
            _mockApiClient.Object,
            _mockLeadMapper.Object,
            _mockSyncLogRepo.Object,
            _mockLogger.Object,
            _mockConflictRepo.Object,
            _mockConfiguration.Object
        );
    }

    [Fact]
    public async Task SyncLeadsFromEasyCarsAsync_WithMultipleLeads_CallsGetLeadDetailForEach()
    {
        // Arrange
        var leads = new List<Lead> { CreateTestLead("LEAD-001"), CreateTestLead("LEAD-002") };
        SetupInboundSync(leads);

        // Act
        await _sut.SyncLeadsFromEasyCarsAsync(1);

        // Assert
        _mockApiClient.Verify(a => a.GetLeadDetailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task SyncLeadsFromEasyCarsAsync_WithMultipleLeads_CallsUpdateLeadFromResponseForEach()
    {
        // Arrange
        var leads = new List<Lead> { CreateTestLead("LEAD-001"), CreateTestLead("LEAD-002") };
        SetupInboundSync(leads);

        // Act
        await _sut.SyncLeadsFromEasyCarsAsync(1);

        // Assert
        _mockLeadMapper.Verify(m => m.UpdateLeadFromResponse(
            It.IsAny<Lead>(), It.IsAny<LeadDetailResponse>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task SyncLeadsFromEasyCarsAsync_WithMultipleLeads_CallsSaveChangesOnce()
    {
        // Arrange
        var leads = new List<Lead> { CreateTestLead("LEAD-001"), CreateTestLead("LEAD-002") };
        SetupInboundSync(leads);

        // Act
        await _sut.SyncLeadsFromEasyCarsAsync(1);

        // Assert
        _mockLeadRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncLeadsFromEasyCarsAsync_OnSuccess_ReturnsSuccessResult()
    {
        // Arrange
        var leads = new List<Lead> { CreateTestLead("LEAD-001"), CreateTestLead("LEAD-002") };
        SetupInboundSync(leads);

        // Act
        var result = await _sut.SyncLeadsFromEasyCarsAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ItemsSucceeded.Should().Be(2);
        _mockLeadRepo.Verify(r => r.Update(It.IsAny<Lead>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SyncLeadsFromEasyCarsAsync_WithOneFailingLead_ContinuesAndReturnsPartialSuccess()
    {
        // Arrange
        var lead1 = CreateTestLead("LEAD-001");
        var lead2 = CreateTestLead("LEAD-002");
        var leads = new List<Lead> { lead1, lead2 };
        SetupCredentials(1);
        _mockLeadRepo.Setup(r => r.GetLeadsWithEasyCarsNumberAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leads);
        _mockApiClient.SetupSequence(a => a.GetLeadDetailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeadDetailResponse { LeadNumber = "LEAD-001" })
            .ThrowsAsync(new Exception("API error"));
        _mockLeadRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.SyncLeadsFromEasyCarsAsync(1);

        // Assert
        result.IsPartialSuccess.Should().BeTrue();
        result.ItemsSucceeded.Should().Be(1);
        result.ItemsFailed.Should().Be(1);
    }

    [Fact]
    public async Task SyncLeadsFromEasyCarsAsync_WithNoLeads_ReturnsSuccessWithZeroItems()
    {
        // Arrange
        SetupCredentials(1);
        _mockLeadRepo.Setup(r => r.GetLeadsWithEasyCarsNumberAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Lead>());

        // Act
        var result = await _sut.SyncLeadsFromEasyCarsAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ItemsProcessed.Should().Be(0);
        _mockLeadRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncLeadsFromEasyCarsAsync_WithNoCredentials_ReturnsFailureResult()
    {
        // Arrange
        _mockCredentialRepo.Setup(r => r.GetByDealershipIdAsync(1))
            .ReturnsAsync((EasyCarsCredential?)null);
        _mockSyncLogRepo.Setup(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog log, CancellationToken ct) => log);

        // Act
        var result = await _sut.SyncLeadsFromEasyCarsAsync(1);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("No EasyCars credentials"));
        _mockSyncLogRepo.Verify(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncLeadsFromEasyCarsAsync_OnSuccess_CreatesSyncLog()
    {
        // Arrange
        var leads = new List<Lead> { CreateTestLead("LEAD-001") };
        SetupInboundSync(leads);

        // Act
        await _sut.SyncLeadsFromEasyCarsAsync(1);

        // Assert
        _mockSyncLogRepo.Verify(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncLeadsFromEasyCarsAsync_WithPartialFailure_CreatesSyncLogWithCounts()
    {
        // Arrange
        var lead1 = CreateTestLead("LEAD-001");
        var lead2 = CreateTestLead("LEAD-002");
        var leads = new List<Lead> { lead1, lead2 };
        SetupCredentials(1);
        _mockLeadRepo.Setup(r => r.GetLeadsWithEasyCarsNumberAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leads);
        _mockApiClient.SetupSequence(a => a.GetLeadDetailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeadDetailResponse { LeadNumber = "LEAD-001" })
            .ThrowsAsync(new Exception("API error"));
        _mockLeadRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _sut.SyncLeadsFromEasyCarsAsync(1);

        // Assert
        _mockSyncLogRepo.Verify(r => r.AddAsync(
            It.Is<EasyCarsSyncLog>(log => log.ItemsSucceeded == 1 && log.ItemsFailed == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncLeadsFromEasyCarsAsync_WithAllLeadsFailing_ReturnsFailureResult()
    {
        // Arrange
        var lead1 = CreateTestLead("LEAD-001");
        var lead2 = CreateTestLead("LEAD-002");
        var leads = new List<Lead> { lead1, lead2 };
        SetupCredentials(1);
        _mockLeadRepo.Setup(r => r.GetLeadsWithEasyCarsNumberAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leads);
        _mockApiClient.Setup(a => a.GetLeadDetailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API error"));

        // Act
        var result = await _sut.SyncLeadsFromEasyCarsAsync(1);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.ItemsFailed.Should().Be(2);
    }

    #region Helper Methods

    private Lead CreateTestLead(string easyCarsLeadNumber)
    {
        var lead = Lead.Create(1, "John Smith", "john@example.com", "0412345678", "Interested in your vehicles", null);
        lead.UpdateEasyCarsData(easyCarsLeadNumber, "CUST-100", null);
        return lead;
    }

    private EasyCarsCredential CreateTestCredential()
    {
        return EasyCarsCredential.Create(
            dealershipId: 1,
            clientIdEncrypted: "enc-client-id",
            clientSecretEncrypted: "enc-client-secret",
            accountNumberEncrypted: "enc-account-number",
            accountSecretEncrypted: "enc-account-secret",
            encryptionIV: "enc-iv",
            environment: "Test"
        );
    }

    private void SetupCredentials(int dealershipId)
    {
        var credential = CreateTestCredential();
        _mockCredentialRepo.Setup(r => r.GetByDealershipIdAsync(dealershipId))
            .ReturnsAsync(credential);
        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.ClientIdEncrypted))
            .ReturnsAsync("client-id");
        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.ClientSecretEncrypted))
            .ReturnsAsync("client-secret");
        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.AccountNumberEncrypted))
            .ReturnsAsync("account-number");
        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.AccountSecretEncrypted))
            .ReturnsAsync("account-secret");
        _mockSyncLogRepo.Setup(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog log, CancellationToken ct) => log);
    }

    private void SetupInboundSync(List<Lead> leads)
    {
        SetupCredentials(1);
        _mockLeadRepo.Setup(r => r.GetLeadsWithEasyCarsNumberAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leads);
        _mockApiClient.Setup(a => a.GetLeadDetailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeadDetailResponse { LeadNumber = "LEAD-001" });
        _mockLeadRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    #endregion
}
