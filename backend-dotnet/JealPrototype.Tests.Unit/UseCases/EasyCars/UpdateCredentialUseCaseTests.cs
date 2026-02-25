using AutoMapper;
using FluentAssertions;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Exceptions;
using JealPrototype.Application.Interfaces;
using JealPrototype.Application.UseCases.EasyCars;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JealPrototype.Tests.Unit.UseCases.EasyCars;

public class UpdateCredentialUseCaseTests
{
    private readonly Mock<IEasyCarsCredentialRepository> _mockRepository;
    private readonly Mock<IEncryptionService> _mockEncryptionService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<UpdateCredentialUseCase>> _mockLogger;
    private readonly UpdateCredentialUseCase _useCase;

    public UpdateCredentialUseCaseTests()
    {
        _mockRepository = new Mock<IEasyCarsCredentialRepository>();
        _mockEncryptionService = new Mock<IEncryptionService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<UpdateCredentialUseCase>>();

        _useCase = new UpdateCredentialUseCase(
            _mockRepository.Object,
            _mockEncryptionService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidUpdate_UpdatesCredential()
    {
        // Arrange
        var dealershipId = 1;
        var credentialId = 1;
        var request = new UpdateCredentialRequest
        {
            Environment = "Production",
            YardCode = "NEW_YARD"
        };

        var encryptedData = Convert.ToBase64String(new byte[28]);
        var credential = EasyCarsCredential.Create(
            dealershipId,
            encryptedData,
            encryptedData,
            "fake_iv",
            "Test",
            true,
            "OLD_YARD");

        var expectedResponse = new CredentialResponse
        {
            Id = credentialId,
            DealershipId = dealershipId,
            Environment = "Production",
            IsActive = true,
            YardCode = "NEW_YARD"
        };

        _mockRepository
            .Setup(x => x.GetByIdAsync(credentialId))
            .ReturnsAsync(credential);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EasyCarsCredential>()))
            .ReturnsAsync(credential);

        _mockMapper
            .Setup(x => x.Map<CredentialResponse>(It.IsAny<EasyCarsCredential>()))
            .Returns(expectedResponse);

        // Act
        var result = await _useCase.ExecuteAsync(dealershipId, credentialId, request);

        // Assert
        result.Should().NotBeNull();
        result.Environment.Should().Be("Production");
        result.YardCode.Should().Be("NEW_YARD");
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<EasyCarsCredential>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CredentialNotFound_ThrowsCredentialNotFoundException()
    {
        // Arrange
        var dealershipId = 1;
        var credentialId = 1;
        var request = new UpdateCredentialRequest { Environment = "Production" };

        _mockRepository
            .Setup(x => x.GetByIdAsync(credentialId))
            .ReturnsAsync((EasyCarsCredential?)null);

        // Act & Assert
        await Assert.ThrowsAsync<CredentialNotFoundException>(() => 
            _useCase.ExecuteAsync(dealershipId, credentialId, request));

        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<EasyCarsCredential>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WrongDealership_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var dealershipId = 1;
        var credentialId = 1;
        var request = new UpdateCredentialRequest { Environment = "Production" };

        var encryptedData = Convert.ToBase64String(new byte[28]);
        var credential = EasyCarsCredential.Create(
            dealershipId: 2, // Different dealership
            encryptedData,
            encryptedData,
            "fake_iv",
            "Test",
            true,
            null);

        _mockRepository
            .Setup(x => x.GetByIdAsync(credentialId))
            .ReturnsAsync(credential);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _useCase.ExecuteAsync(dealershipId, credentialId, request));

        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<EasyCarsCredential>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateAccountNumber_EncryptsNewValue()
    {
        // Arrange
        var dealershipId = 1;
        var credentialId = 1;
        var newAccountNumber = "11111111-1111-1111-1111-111111111111";
        var request = new UpdateCredentialRequest
        {
            AccountNumber = newAccountNumber
        };

        var encryptedData = Convert.ToBase64String(new byte[28]);
        var newEncryptedData = Convert.ToBase64String(new byte[28]);
        
        var credential = EasyCarsCredential.Create(
            dealershipId,
            encryptedData,
            encryptedData,
            "fake_iv",
            "Test",
            true,
            null);

        _mockRepository
            .Setup(x => x.GetByIdAsync(credentialId))
            .ReturnsAsync(credential);

        _mockEncryptionService
            .Setup(x => x.EncryptAsync(newAccountNumber))
            .ReturnsAsync(newEncryptedData);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EasyCarsCredential>()))
            .ReturnsAsync(credential);

        _mockMapper
            .Setup(x => x.Map<CredentialResponse>(It.IsAny<EasyCarsCredential>()))
            .Returns(new CredentialResponse());

        // Act
        await _useCase.ExecuteAsync(dealershipId, credentialId, request);

        // Assert
        _mockEncryptionService.Verify(x => x.EncryptAsync(newAccountNumber), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_UpdateIsActive_ChangesStatus()
    {
        // Arrange
        var dealershipId = 1;
        var credentialId = 1;
        var request = new UpdateCredentialRequest
        {
            IsActive = false
        };

        var encryptedData = Convert.ToBase64String(new byte[28]);
        var credential = EasyCarsCredential.Create(
            dealershipId,
            encryptedData,
            encryptedData,
            "fake_iv",
            "Test",
            true,
            null);

        var expectedResponse = new CredentialResponse
        {
            Id = credentialId,
            DealershipId = dealershipId,
            IsActive = false
        };

        _mockRepository
            .Setup(x => x.GetByIdAsync(credentialId))
            .ReturnsAsync(credential);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<EasyCarsCredential>()))
            .ReturnsAsync(credential);

        _mockMapper
            .Setup(x => x.Map<CredentialResponse>(It.IsAny<EasyCarsCredential>()))
            .Returns(expectedResponse);

        // Act
        var result = await _useCase.ExecuteAsync(dealershipId, credentialId, request);

        // Assert
        result.IsActive.Should().BeFalse();
    }
}
