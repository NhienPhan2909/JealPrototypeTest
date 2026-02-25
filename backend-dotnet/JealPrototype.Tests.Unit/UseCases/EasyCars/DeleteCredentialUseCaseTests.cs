using FluentAssertions;
using JealPrototype.Application.Exceptions;
using JealPrototype.Application.UseCases.EasyCars;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JealPrototype.Tests.Unit.UseCases.EasyCars;

public class DeleteCredentialUseCaseTests
{
    private readonly Mock<IEasyCarsCredentialRepository> _mockRepository;
    private readonly Mock<ILogger<DeleteCredentialUseCase>> _mockLogger;
    private readonly DeleteCredentialUseCase _useCase;

    public DeleteCredentialUseCaseTests()
    {
        _mockRepository = new Mock<IEasyCarsCredentialRepository>();
        _mockLogger = new Mock<ILogger<DeleteCredentialUseCase>>();

        _useCase = new DeleteCredentialUseCase(
            _mockRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_DeletesCredential()
    {
        // Arrange
        var dealershipId = 1;
        var credentialId = 1;
        var credential = EasyCarsCredential.Create(
            dealershipId,
            "encrypted_account_number",
            "encrypted_account_secret",
            "fake_iv",
            "Test",
            true,
            null);

        _mockRepository
            .Setup(x => x.GetByIdAsync(credentialId))
            .ReturnsAsync(credential);

        _mockRepository
            .Setup(x => x.DeleteAsync(credentialId))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(dealershipId, credentialId);

        // Assert
        _mockRepository.Verify(x => x.DeleteAsync(credentialId), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CredentialNotFound_ThrowsCredentialNotFoundException()
    {
        // Arrange
        var dealershipId = 1;
        var credentialId = 1;

        _mockRepository
            .Setup(x => x.GetByIdAsync(credentialId))
            .ReturnsAsync((EasyCarsCredential?)null);

        // Act & Assert
        await Assert.ThrowsAsync<CredentialNotFoundException>(() => 
            _useCase.ExecuteAsync(dealershipId, credentialId));

        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WrongDealership_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var dealershipId = 1;
        var credentialId = 1;
        var credential = EasyCarsCredential.Create(
            dealershipId: 2, // Different dealership
            "encrypted_account_number",
            "encrypted_account_secret",
            "fake_iv",
            "Test",
            true,
            null);

        _mockRepository
            .Setup(x => x.GetByIdAsync(credentialId))
            .ReturnsAsync(credential);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _useCase.ExecuteAsync(dealershipId, credentialId));

        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_VerifiesOwnershipBeforeDeleting()
    {
        // Arrange
        var dealershipId = 1;
        var credentialId = 1;
        var credential = EasyCarsCredential.Create(
            dealershipId,
            "encrypted_account_number",
            "encrypted_account_secret",
            "fake_iv",
            "Test",
            true,
            null);

        _mockRepository
            .Setup(x => x.GetByIdAsync(credentialId))
            .ReturnsAsync(credential);

        // Act
        await _useCase.ExecuteAsync(dealershipId, credentialId);

        // Assert - should retrieve credential first to verify ownership
        _mockRepository.Verify(x => x.GetByIdAsync(credentialId), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync(credentialId), Times.Once);
    }
}
