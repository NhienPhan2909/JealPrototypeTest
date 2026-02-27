using FluentAssertions;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Exceptions;
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
/// Unit tests for EasyCarsLeadSyncService (Story 3.3)
/// Covers 13 test scenarios for outbound lead synchronization.
/// </summary>
public class EasyCarsLeadSyncServiceTests
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

    public EasyCarsLeadSyncServiceTests()
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

    #region Create Path Tests

    [Fact]
    public async Task SyncLeadToEasyCarsAsync_WithNewLead_CallsCreateLeadApi()
    {
        // Arrange
        var lead = CreateTestLead(hasEasyCarsLeadNumber: false);
        SetupSuccessfulCreateSync(lead);

        // Act
        await _sut.SyncLeadToEasyCarsAsync(lead.Id);

        // Assert
        _mockApiClient.Verify(a => a.CreateLeadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<CreateLeadRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockApiClient.Verify(a => a.UpdateLeadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<UpdateLeadRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncLeadToEasyCarsAsync_WithNewLead_StoresLeadNumberAndCustomerNo()
    {
        // Arrange
        var lead = CreateTestLead(hasEasyCarsLeadNumber: false);
        SetupSuccessfulCreateSync(lead, leadNumberResponse: "LEAD-999", customerNoResponse: "CUST-888");

        // Act
        await _sut.SyncLeadToEasyCarsAsync(lead.Id);

        // Assert
        lead.EasyCarsLeadNumber.Should().Be("LEAD-999");
        lead.EasyCarsCustomerNo.Should().Be("CUST-888");
    }

    [Fact]
    public async Task SyncLeadToEasyCarsAsync_WithNewLead_SetsLastSyncedToEasyCars()
    {
        // Arrange
        var lead = CreateTestLead(hasEasyCarsLeadNumber: false);
        SetupSuccessfulCreateSync(lead);

        // Act
        await _sut.SyncLeadToEasyCarsAsync(lead.Id);

        // Assert
        lead.LastSyncedToEasyCars.Should().NotBeNull();
    }

    [Fact]
    public async Task SyncLeadToEasyCarsAsync_WithNewLead_ReturnsSuccessResult()
    {
        // Arrange
        var lead = CreateTestLead(hasEasyCarsLeadNumber: false);
        SetupSuccessfulCreateSync(lead);

        // Act
        var result = await _sut.SyncLeadToEasyCarsAsync(lead.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ItemsSucceeded.Should().Be(1);
    }

    #endregion

    #region Update Path Tests

    [Fact]
    public async Task SyncLeadToEasyCarsAsync_WithExistingEasyCarsLeadNumber_CallsUpdateLeadApi()
    {
        // Arrange
        var lead = CreateTestLead(hasEasyCarsLeadNumber: true);
        SetupSuccessfulUpdateSync(lead);

        // Act
        await _sut.SyncLeadToEasyCarsAsync(lead.Id);

        // Assert
        _mockApiClient.Verify(a => a.UpdateLeadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<UpdateLeadRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockApiClient.Verify(a => a.CreateLeadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<CreateLeadRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SyncLeadToEasyCarsAsync_WithExistingLead_PassesLeadNumberToUpdateRequest()
    {
        // Arrange
        var lead = CreateTestLead(hasEasyCarsLeadNumber: true);
        SetupSuccessfulUpdateSync(lead);

        // Act
        await _sut.SyncLeadToEasyCarsAsync(lead.Id);

        // Assert
        _mockLeadMapper.Verify(m => m.MapToUpdateLeadRequest(
            lead,
            "LEAD-001",
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Vehicle?>()), Times.Once);
    }

    #endregion

    #region Vehicle Loading Tests

    [Fact]
    public async Task SyncLeadToEasyCarsAsync_WithLeadHavingVehicleId_FetchesVehicle()
    {
        // Arrange
        var vehicleId = 42;
        var lead = CreateTestLead(hasEasyCarsLeadNumber: false, vehicleId: vehicleId);
        SetupSuccessfulCreateSync(lead);
        _mockVehicleRepo.Setup(r => r.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehicle?)null);

        // Act
        await _sut.SyncLeadToEasyCarsAsync(lead.Id);

        // Assert
        _mockVehicleRepo.Verify(r => r.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncLeadToEasyCarsAsync_WithLeadHavingNoVehicleId_DoesNotFetchVehicle()
    {
        // Arrange
        var lead = CreateTestLead(hasEasyCarsLeadNumber: false, vehicleId: null);
        SetupSuccessfulCreateSync(lead);

        // Act
        await _sut.SyncLeadToEasyCarsAsync(lead.Id);

        // Assert
        _mockVehicleRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Failure Tests

    [Fact]
    public async Task SyncLeadToEasyCarsAsync_WithLeadNotFound_ReturnsFailureResult()
    {
        // Arrange
        _mockLeadRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Lead?)null);

        // Act
        var result = await _sut.SyncLeadToEasyCarsAsync(999);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("not found"));
    }

    [Fact]
    public async Task SyncLeadToEasyCarsAsync_WithNoCredentials_ReturnsFailureResult()
    {
        // Arrange
        var lead = CreateTestLead(hasEasyCarsLeadNumber: false);
        _mockLeadRepo.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        _mockCredentialRepo.Setup(r => r.GetByDealershipIdAsync(lead.DealershipId))
            .ReturnsAsync((EasyCarsCredential?)null);

        // Act
        var result = await _sut.SyncLeadToEasyCarsAsync(lead.Id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("No EasyCars credentials"));
    }

    [Fact]
    public async Task SyncLeadToEasyCarsAsync_WhenApiThrowsException_ReturnsFailureResult()
    {
        // Arrange
        var lead = CreateTestLead(hasEasyCarsLeadNumber: false);
        SetupCredentials(lead);
        _mockLeadMapper.Setup(m => m.MapToCreateLeadRequest(
            It.IsAny<Lead>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Vehicle?>()))
            .Returns(new CreateLeadRequest());
        _mockApiClient.Setup(a => a.CreateLeadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<CreateLeadRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API connection failed"));
        _mockSyncLogRepo.Setup(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog log, CancellationToken ct) => log);

        // Act
        var result = await _sut.SyncLeadToEasyCarsAsync(lead.Id);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("API connection failed"));
    }

    #endregion

    #region Sync Log Tests

    [Fact]
    public async Task SyncLeadToEasyCarsAsync_WhenTemporaryException_ReturnsFailureAndCreatesLog()
    {
        // Arrange
        var lead = CreateTestLead(hasEasyCarsLeadNumber: false);
        SetupCredentials(lead);
        _mockLeadMapper.Setup(m => m.MapToCreateLeadRequest(
            It.IsAny<Lead>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Vehicle?>()))
            .Returns(new CreateLeadRequest());
        _mockApiClient.Setup(a => a.CreateLeadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<CreateLeadRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EasyCarsTemporaryException("Temporary service unavailable"));

        // Act
        var result = await _sut.SyncLeadToEasyCarsAsync(lead.Id);

        // Assert - transient failure returns IsFailed and creates sync log
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Contains("Temporary service unavailable"));
        _mockSyncLogRepo.Verify(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncLeadToEasyCarsAsync_OnSuccess_CreatesSyncLog()
    {
        // Arrange
        var lead = CreateTestLead(hasEasyCarsLeadNumber: false);
        SetupSuccessfulCreateSync(lead);

        // Act
        await _sut.SyncLeadToEasyCarsAsync(lead.Id);

        // Assert
        _mockSyncLogRepo.Verify(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SyncLeadToEasyCarsAsync_OnFailure_CreatesSyncLog()
    {
        // Arrange
        var lead = CreateTestLead(hasEasyCarsLeadNumber: false);
        SetupCredentials(lead);
        _mockLeadMapper.Setup(m => m.MapToCreateLeadRequest(
            It.IsAny<Lead>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Vehicle?>()))
            .Returns(new CreateLeadRequest());
        _mockApiClient.Setup(a => a.CreateLeadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<CreateLeadRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API error"));
        _mockSyncLogRepo.Setup(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog log, CancellationToken ct) => log);

        // Act
        await _sut.SyncLeadToEasyCarsAsync(lead.Id);

        // Assert - sync log created even on failure
        _mockSyncLogRepo.Verify(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helper Methods

    private Lead CreateTestLead(bool hasEasyCarsLeadNumber = false, int? vehicleId = null)
    {
        var lead = Lead.Create(1, "John Smith", "john@example.com", "0412345678", "Interested in your vehicles", vehicleId);
        if (hasEasyCarsLeadNumber)
            lead.UpdateEasyCarsData("LEAD-001", "CUST-100", null);
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

    private void SetupCredentials(Lead lead)
    {
        var credential = CreateTestCredential();
        _mockLeadRepo.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lead);
        _mockCredentialRepo.Setup(r => r.GetByDealershipIdAsync(lead.DealershipId))
            .ReturnsAsync(credential);
        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.ClientIdEncrypted))
            .ReturnsAsync("client-id");
        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.ClientSecretEncrypted))
            .ReturnsAsync("client-secret");
        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.AccountNumberEncrypted))
            .ReturnsAsync("account-number");
        _mockEncryptionService.Setup(e => e.DecryptAsync(credential.AccountSecretEncrypted))
            .ReturnsAsync("account-secret");
        _mockLeadRepo.Setup(r => r.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockSyncLogRepo.Setup(r => r.AddAsync(It.IsAny<EasyCarsSyncLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EasyCarsSyncLog log, CancellationToken ct) => log);
    }

    private void SetupSuccessfulCreateSync(Lead lead, string leadNumberResponse = "LEAD-001", string customerNoResponse = "CUST-100")
    {
        SetupCredentials(lead);
        _mockLeadMapper.Setup(m => m.MapToCreateLeadRequest(
            It.IsAny<Lead>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Vehicle?>()))
            .Returns(new CreateLeadRequest());
        _mockApiClient.Setup(a => a.CreateLeadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<CreateLeadRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateLeadResponse { LeadNumber = leadNumberResponse, CustomerNo = customerNoResponse });
    }

    private void SetupSuccessfulUpdateSync(Lead lead)
    {
        SetupCredentials(lead);
        _mockLeadMapper.Setup(m => m.MapToUpdateLeadRequest(
            It.IsAny<Lead>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Vehicle?>()))
            .Returns(new UpdateLeadRequest());
        _mockApiClient.Setup(a => a.UpdateLeadAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<int>(), It.IsAny<UpdateLeadRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateLeadResponse { LeadNumber = "LEAD-001" });
    }

    #endregion
}
