using AutoMapper;
using FluentAssertions;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Exceptions;
using JealPrototype.Application.UseCases.EasyCars;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JealPrototype.Tests.Unit.UseCases.EasyCars;

public class GetCredentialUseCaseTests
{
    private readonly Mock<IEasyCarsCredentialRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<GetCredentialUseCase>> _mockLogger;
    private readonly GetCredentialUseCase _useCase;

    public GetCredentialUseCaseTests()
    {
        _mockRepository = new Mock<IEasyCarsCredentialRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<GetCredentialUseCase>>();

        _useCase = new GetCredentialUseCase(
            _mockRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_CredentialExists_ReturnsMetadata()
    {
        // Arrange
        var dealershipId = 1;
        var credential = EasyCarsCredential.Create(
            dealershipId,
            "encrypted_account_number",
            "encrypted_account_secret",
            "fake_iv",
            "Test",
            true,
            "YARD123");

        var expectedResponse = new CredentialMetadataResponse
        {
            Id = 1,
            DealershipId = dealershipId,
            Environment = "Test",
            IsActive = true,
            YardCode = "YARD123"
        };

        _mockRepository
            .Setup(x => x.GetByDealershipIdAsync(dealershipId))
            .ReturnsAsync(credential);

        _mockMapper
            .Setup(x => x.Map<CredentialMetadataResponse>(credential))
            .Returns(expectedResponse);

        // Act
        var result = await _useCase.ExecuteAsync(dealershipId);

        // Assert
        result.Should().NotBeNull();
        result.DealershipId.Should().Be(dealershipId);
        result.Environment.Should().Be("Test");
    }

    [Fact]
    public async Task ExecuteAsync_CredentialNotFound_ThrowsCredentialNotFoundException()
    {
        // Arrange
        var dealershipId = 1;

        _mockRepository
            .Setup(x => x.GetByDealershipIdAsync(dealershipId))
            .ReturnsAsync((EasyCarsCredential?)null);

        // Act & Assert
        await Assert.ThrowsAsync<CredentialNotFoundException>(() => 
            _useCase.ExecuteAsync(dealershipId));
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotReturnSensitiveData()
    {
        // Arrange
        var dealershipId = 1;
        var credential = EasyCarsCredential.Create(
            dealershipId,
            "encrypted_account_number",
            "encrypted_account_secret",
            "fake_iv",
            "Production",
            true,
            null);

        var response = new CredentialMetadataResponse
        {
            Id = 1,
            DealershipId = dealershipId,
            Environment = "Production",
            IsActive = true
        };

        _mockRepository
            .Setup(x => x.GetByDealershipIdAsync(dealershipId))
            .ReturnsAsync(credential);

        _mockMapper
            .Setup(x => x.Map<CredentialMetadataResponse>(credential))
            .Returns(response);

        // Act
        var result = await _useCase.ExecuteAsync(dealershipId);

        // Assert - metadata response should NOT contain AccountNumber or AccountSecret
        result.Should().NotBeNull();
        var responseType = result.GetType();
        responseType.GetProperty("AccountNumber").Should().BeNull();
        responseType.GetProperty("AccountSecret").Should().BeNull();
    }
}
