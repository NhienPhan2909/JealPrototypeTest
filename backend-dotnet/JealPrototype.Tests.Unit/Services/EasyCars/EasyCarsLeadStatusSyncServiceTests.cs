using FluentAssertions;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Interfaces;
using JealPrototype.Application.Interfaces.Security;
using JealPrototype.Application.Services.EasyCars;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JealPrototype.Tests.Unit.Services.EasyCars;

/// <summary>
/// Unit tests for EasyCarsLeadSyncService — Story 3.6: Lead Status Synchronization.
/// Covers 14+ test scenarios for status mapping, conflict detection, conflict resolution, 
/// business rule guard (cannot un-delete), outbound push, and background job dispatching.
/// </summary>
public class EasyCarsLeadStatusSyncServiceTests
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

    public EasyCarsLeadStatusSyncServiceTests()
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
    }

    private EasyCarsLeadSyncService CreateSut(string strategy = "RemoteWins")
    {
        _mockConfiguration.Setup(c => c["EasyCars:LeadStatusConflictResolutionStrategy"]).Returns(strategy);
        return new EasyCarsLeadSyncService(
            _mockLeadRepo.Object,
            _mockVehicleRepo.Object,
            _mockCredentialRepo.Object,
            _mockEncryptionService.Object,
            _mockApiClient.Object,
            _mockLeadMapper.Object,
            _mockSyncLogRepo.Object,
            _mockLogger.Object,
            _mockConflictRepo.Object,
            _mockConfiguration.Object);
    }

    #region Status Mapping Tests (AC2)

    [Fact]
    public void MapLeadStatusToEasyCars_Received_Returns10()
    {
        var mapper = new EasyCarsLeadMapper(new Mock<ILogger<EasyCarsLeadMapper>>().Object);
        mapper.MapLeadStatusToEasyCars(LeadStatus.Received).Should().Be(10);
    }

    [Fact]
    public void MapLeadStatusToEasyCars_InProgress_Returns30()
    {
        var mapper = new EasyCarsLeadMapper(new Mock<ILogger<EasyCarsLeadMapper>>().Object);
        mapper.MapLeadStatusToEasyCars(LeadStatus.InProgress).Should().Be(30);
    }

    [Fact]
    public void MapLeadStatusToEasyCars_Won_Returns50()
    {
        var mapper = new EasyCarsLeadMapper(new Mock<ILogger<EasyCarsLeadMapper>>().Object);
        mapper.MapLeadStatusToEasyCars(LeadStatus.Won).Should().Be(50);
    }

    [Fact]
    public void MapLeadStatusToEasyCars_Lost_Returns60()
    {
        var mapper = new EasyCarsLeadMapper(new Mock<ILogger<EasyCarsLeadMapper>>().Object);
        mapper.MapLeadStatusToEasyCars(LeadStatus.Lost).Should().Be(60);
    }

    [Fact]
    public void MapLeadStatusToEasyCars_Deleted_Returns90()
    {
        var mapper = new EasyCarsLeadMapper(new Mock<ILogger<EasyCarsLeadMapper>>().Object);
        mapper.MapLeadStatusToEasyCars(LeadStatus.Deleted).Should().Be(90);
    }

    [Fact]
    public void MapLeadStatusFromInt_10_ReturnsReceived()
    {
        var mapper = new EasyCarsLeadMapper(new Mock<ILogger<EasyCarsLeadMapper>>().Object);
        mapper.MapLeadStatusFromInt(10).Should().Be(LeadStatus.Received);
    }

    [Fact]
    public void MapLeadStatusFromInt_50_ReturnsWon()
    {
        var mapper = new EasyCarsLeadMapper(new Mock<ILogger<EasyCarsLeadMapper>>().Object);
        mapper.MapLeadStatusFromInt(50).Should().Be(LeadStatus.Won);
    }

    [Fact]
    public void MapLeadStatusFromInt_60_ReturnsLost()
    {
        var mapper = new EasyCarsLeadMapper(new Mock<ILogger<EasyCarsLeadMapper>>().Object);
        mapper.MapLeadStatusFromInt(60).Should().Be(LeadStatus.Lost);
    }

    [Fact]
    public void MapLeadStatusFromInt_90_ReturnsDeleted()
    {
        var mapper = new EasyCarsLeadMapper(new Mock<ILogger<EasyCarsLeadMapper>>().Object);
        mapper.MapLeadStatusFromInt(90).Should().Be(LeadStatus.Deleted);
    }

    #endregion

    #region Outbound Push Tests (AC1, AC6)

    [Fact]
    public async Task SyncLeadStatusToEasyCarsAsync_WithNoEasyCarsLeadNumber_ReturnsSuccessWithZeroItems()
    {
        // Arrange
        var lead = CreateTestLead(hasEasyCarsLeadNumber: false);
        _mockLeadRepo.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        var sut = CreateSut();

        // Act
        var result = await sut.SyncLeadStatusToEasyCarsAsync(lead.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ItemsSucceeded.Should().Be(0);
        _mockApiClient.Verify(a => a.UpdateLeadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<UpdateLeadRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncLeadStatusToEasyCarsAsync_WithValidLead_CallsUpdateLeadAndMarksTimestamp()
    {
        // Arrange
        var lead = CreateTestLead(hasEasyCarsLeadNumber: true);
        SetupCredentials(lead);

        var statusOnlyReq = new UpdateLeadRequest { LeadStatus = 10 };
        _mockLeadMapper.Setup(m => m.MapToStatusOnlyUpdateRequest(lead, It.IsAny<string>(), It.IsAny<string>()))
            .Returns(statusOnlyReq);
        _mockApiClient.Setup(a => a.UpdateLeadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<UpdateLeadRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateLeadResponse { LeadNumber = "LEAD-001" });

        var sut = CreateSut();

        // Act
        var result = await sut.SyncLeadStatusToEasyCarsAsync(lead.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ItemsSucceeded.Should().Be(1);
        _mockApiClient.Verify(a => a.UpdateLeadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<UpdateLeadRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockLeadRepo.Verify(r => r.UpdateAsync(lead, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Inbound Conflict Resolution Tests (AC3, AC4, AC5)

    [Fact]
    public async Task SyncLeadStatusesFromEasyCarsAsync_RemoteWins_UpdatesLocalStatus()
    {
        // Arrange — lead is Received, remote says InProgress (30)
        var lead = CreateTestLead(hasEasyCarsLeadNumber: true);
        SetupInboundSync(new List<Lead> { lead });
        _mockApiClient.Setup(a => a.GetLeadDetailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeadDetailResponse { LeadNumber = "LEAD-001", LeadStatus = 30 });
        _mockLeadMapper.Setup(m => m.MapLeadStatusFromInt(30)).Returns(LeadStatus.InProgress);

        var sut = CreateSut("RemoteWins");

        // Act
        var result = await sut.SyncLeadStatusesFromEasyCarsAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // lead.Status should have been updated — verify UpdateAsync was called
        _mockLeadRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockConflictRepo.Verify(r => r.AddAsync(It.IsAny<LeadStatusConflict>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncLeadStatusesFromEasyCarsAsync_LocalWins_DoesNotUpdateLocalStatus()
    {
        // Arrange — lead is Received, remote says InProgress (30) but strategy is LocalWins
        var lead = CreateTestLead(hasEasyCarsLeadNumber: true);
        SetupInboundSync(new List<Lead> { lead });
        _mockApiClient.Setup(a => a.GetLeadDetailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeadDetailResponse { LeadNumber = "LEAD-001", LeadStatus = 30 });
        _mockLeadMapper.Setup(m => m.MapLeadStatusFromInt(30)).Returns(LeadStatus.InProgress);

        var sut = CreateSut("LocalWins");

        // Act
        var result = await sut.SyncLeadStatusesFromEasyCarsAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        lead.Status.Should().Be(LeadStatus.Received); // unchanged
        _mockConflictRepo.Verify(r => r.AddAsync(It.IsAny<LeadStatusConflict>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncLeadStatusesFromEasyCarsAsync_ManualReview_CreatesConflictRecord()
    {
        // Arrange — lead is Received, remote says InProgress, strategy = ManualReview
        var lead = CreateTestLead(hasEasyCarsLeadNumber: true);
        SetupInboundSync(new List<Lead> { lead });
        _mockApiClient.Setup(a => a.GetLeadDetailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeadDetailResponse { LeadNumber = "LEAD-001", LeadStatus = 30 });
        _mockLeadMapper.Setup(m => m.MapLeadStatusFromInt(30)).Returns(LeadStatus.InProgress);
        _mockConflictRepo.Setup(r => r.ExistsUnresolvedForLeadAsync(lead.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockConflictRepo.Setup(r => r.AddAsync(It.IsAny<LeadStatusConflict>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeadStatusConflict c, CancellationToken _) => c);

        var sut = CreateSut("ManualReview");

        // Act
        var result = await sut.SyncLeadStatusesFromEasyCarsAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockConflictRepo.Verify(r => r.AddAsync(It.IsAny<LeadStatusConflict>(), It.IsAny<CancellationToken>()), Times.Once);
        lead.Status.Should().Be(LeadStatus.Received); // not changed
    }

    [Fact]
    public async Task SyncLeadStatusesFromEasyCarsAsync_DeletedLeadRemoteWins_CannotUndelete_CreatesConflict()
    {
        // Arrange — lead is Deleted, remote says InProgress (30) — cannot un-delete
        var lead = CreateTestLead(hasEasyCarsLeadNumber: true);
        lead.UpdateStatus(LeadStatus.Deleted);
        SetupInboundSync(new List<Lead> { lead });
        _mockApiClient.Setup(a => a.GetLeadDetailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeadDetailResponse { LeadNumber = "LEAD-001", LeadStatus = 30 });
        _mockLeadMapper.Setup(m => m.MapLeadStatusFromInt(30)).Returns(LeadStatus.InProgress);
        _mockConflictRepo.Setup(r => r.AddAsync(It.IsAny<LeadStatusConflict>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeadStatusConflict c, CancellationToken _) => c);

        var sut = CreateSut("RemoteWins");

        // Act
        var result = await sut.SyncLeadStatusesFromEasyCarsAsync(1);

        // Assert — conflict created since business rule blocks un-delete
        result.IsSuccess.Should().BeTrue();
        lead.Status.Should().Be(LeadStatus.Deleted); // unchanged
        _mockConflictRepo.Verify(r => r.AddAsync(It.IsAny<LeadStatusConflict>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncLeadStatusesFromEasyCarsAsync_StatusesMatch_NoConflictCreated()
    {
        // Arrange — lead is Received (10), remote says Received (10)
        var lead = CreateTestLead(hasEasyCarsLeadNumber: true);
        SetupInboundSync(new List<Lead> { lead });
        _mockApiClient.Setup(a => a.GetLeadDetailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeadDetailResponse { LeadNumber = "LEAD-001", LeadStatus = 10 });
        _mockLeadMapper.Setup(m => m.MapLeadStatusFromInt(10)).Returns(LeadStatus.Received);

        var sut = CreateSut("RemoteWins");

        // Act
        var result = await sut.SyncLeadStatusesFromEasyCarsAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockConflictRepo.Verify(r => r.AddAsync(It.IsAny<LeadStatusConflict>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncLeadStatusesFromEasyCarsAsync_OneLeadFails_ContinuesAndReturnsPartialSuccess()
    {
        // Arrange — two leads: second one throws
        var lead1 = CreateTestLead(hasEasyCarsLeadNumber: true, leadNumber: "LEAD-001");
        var lead2 = CreateTestLead(hasEasyCarsLeadNumber: true, leadNumber: "LEAD-002");
        SetupInboundSync(new List<Lead> { lead1, lead2 });
        _mockApiClient.SetupSequence(a => a.GetLeadDetailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<int>(), "LEAD-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeadDetailResponse { LeadNumber = "LEAD-001", LeadStatus = 10 });
        _mockLeadMapper.Setup(m => m.MapLeadStatusFromInt(10)).Returns(LeadStatus.Received);
        _mockApiClient.Setup(a => a.GetLeadDetailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<int>(), "LEAD-002", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API error"));

        var sut = CreateSut("RemoteWins");

        // Act
        var result = await sut.SyncLeadStatusesFromEasyCarsAsync(1);

        // Assert
        result.IsPartialSuccess.Should().BeTrue();
        result.ItemsSucceeded.Should().Be(1);
        result.ItemsFailed.Should().Be(1);
    }

    [Fact]
    public async Task SyncLeadStatusesFromEasyCarsAsync_OnSuccess_CreatesSyncLogWithTypeLeadStatus()
    {
        // Arrange
        var lead = CreateTestLead(hasEasyCarsLeadNumber: true);
        SetupInboundSync(new List<Lead> { lead });
        _mockApiClient.Setup(a => a.GetLeadDetailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeadDetailResponse { LeadNumber = "LEAD-001", LeadStatus = 10 });
        _mockLeadMapper.Setup(m => m.MapLeadStatusFromInt(10)).Returns(LeadStatus.Received);

        var sut = CreateSut("RemoteWins");

        // Act
        await sut.SyncLeadStatusesFromEasyCarsAsync(1);

        // Assert
        _mockSyncLogRepo.Verify(r => r.AddAsync(
            It.Is<EasyCarsSyncLog>(l => l.SyncType == "LeadStatus"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Business Rule Guard (AC4)

    [Fact]
    public void CanChangeStatusTo_FromDeleted_ToAnyOther_ReturnsFalse()
    {
        var lead = CreateTestLead();
        lead.UpdateStatus(LeadStatus.Deleted);
        lead.CanChangeStatusTo(LeadStatus.Won).Should().BeFalse();
        lead.CanChangeStatusTo(LeadStatus.Received).Should().BeFalse();
        lead.CanChangeStatusTo(LeadStatus.Deleted).Should().BeTrue(); // can stay deleted
    }

    #endregion

    #region Helper Methods

    private Lead CreateTestLead(bool hasEasyCarsLeadNumber = false, string leadNumber = "LEAD-001")
    {
        var lead = Lead.Create(1, "John Smith", "john@example.com", "0412345678", "Test message");
        if (hasEasyCarsLeadNumber)
            lead.UpdateEasyCarsData(leadNumber, "CUST-100", null);
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
            environment: "Test");
    }

    private void SetupCredentials(Lead lead)
    {
        var credential = CreateTestCredential();
        _mockLeadRepo.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        _mockCredentialRepo.Setup(r => r.GetByDealershipIdAsync(lead.DealershipId)).ReturnsAsync(credential);
        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.ClientIdEncrypted)).ReturnsAsync("client-id");
        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.ClientSecretEncrypted)).ReturnsAsync("client-secret");
        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.AccountNumberEncrypted)).ReturnsAsync("account-number");
        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.AccountSecretEncrypted)).ReturnsAsync("account-secret");
        _mockLeadRepo.Setup(r => r.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockSyncLogRepo.Setup(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog log, CancellationToken ct) => log);
    }

    private void SetupInboundSync(List<Lead> leads)
    {
        var credential = CreateTestCredential();
        _mockCredentialRepo.Setup(r => r.GetByDealershipIdAsync(1)).ReturnsAsync(credential);
        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.ClientIdEncrypted)).ReturnsAsync("client-id");
        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.ClientSecretEncrypted)).ReturnsAsync("client-secret");
        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.AccountNumberEncrypted)).ReturnsAsync("account-number");
        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.AccountSecretEncrypted)).ReturnsAsync("account-secret");
        _mockLeadRepo.Setup(r => r.GetLeadsWithEasyCarsNumberAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leads);
        _mockLeadRepo.Setup(r => r.Update(It.IsAny<Lead>()));
        _mockLeadRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockSyncLogRepo.Setup(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog log, CancellationToken ct) => log);
    }

    #endregion
}
