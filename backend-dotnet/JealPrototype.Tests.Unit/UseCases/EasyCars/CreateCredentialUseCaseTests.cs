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

public class CreateCredentialUseCaseTests
{
    private readonly Mock<IEasyCarsCredentialRepository> _mockCredentialRepository;
    private readonly Mock<IDealershipRepository> _mockDealershipRepository;
    private readonly Mock<IEncryptionService> _mockEncryptionService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CreateCredentialUseCase>> _mockLogger;
    private readonly CreateCredentialUseCase _useCase;

    public CreateCredentialUseCaseTests()
    {
        _mockCredentialRepository = new Mock<IEasyCarsCredentialRepository>();
        _mockDealershipRepository = new Mock<IDealershipRepository>();
        _mockEncryptionService = new Mock<IEncryptionService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CreateCredentialUseCase>>();

        _useCase = new CreateCredentialUseCase(
            _mockCredentialRepository.Object,
            _mockDealershipRepository.Object,
            _mockEncryptionService.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_CreatesCredential()
    {
        // Arrange
        var dealershipId = 1;
        var request = new CreateCredentialRequest
        {
            AccountNumber = "12345678-1234-1234-1234-123456789012",
            AccountSecret = "87654321-4321-4321-4321-210987654321",
            Environment = "Test",
            YardCode = "YARD123"
        };

        var dealership = Dealership.Create("Test Dealership", "123 Test St", "1234567890", "test@test.com");
        var encryptedAccountNumber = Convert.ToBase64String(new byte[28]);
        var encryptedAccountSecret = Convert.ToBase64String(new byte[28]);
        
        var createdCredential = EasyCarsCredential.Create(
            dealershipId, encryptedAccountNumber, encryptedAccountSecret, 
            "fake_iv", "Test", yardCode: "YARD123");
        
        var expectedResponse = new CredentialResponse
        {
            Id = 1,
            DealershipId = dealershipId,
            Environment = "Test",
            IsActive = true,
            YardCode = "YARD123"
        };

        _mockDealershipRepository
            .Setup(x => x.GetByIdAsync(dealershipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dealership);

        _mockCredentialRepository
            .Setup(x => x.ExistsForDealershipAsync(dealershipId))
            .ReturnsAsync(false);

        _mockEncryptionService
            .Setup(x => x.EncryptAsync(request.AccountNumber))
            .ReturnsAsync(encryptedAccountNumber);

        _mockEncryptionService
            .Setup(x => x.EncryptAsync(request.AccountSecret))
            .ReturnsAsync(encryptedAccountSecret);

        _mockCredentialRepository
            .Setup(x => x.CreateAsync(It.IsAny<EasyCarsCredential>()))
            .ReturnsAsync(createdCredential);

        _mockMapper
            .Setup(x => x.Map<CredentialResponse>(It.IsAny<EasyCarsCredential>()))
            .Returns(expectedResponse);

        // Act
        var result = await _useCase.ExecuteAsync(dealershipId, request);

        // Assert
        result.Should().NotBeNull();
        result.DealershipId.Should().Be(dealershipId);
        _mockCredentialRepository.Verify(x => x.CreateAsync(It.IsAny<EasyCarsCredential>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DealershipNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var dealershipId = 1;
        var request = new CreateCredentialRequest
        {
            AccountNumber = "12345678-1234-1234-1234-123456789012",
            AccountSecret = "87654321-4321-4321-4321-210987654321",
            Environment = "Test"
        };

        _mockDealershipRepository
            .Setup(x => x.GetByIdAsync(dealershipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Dealership?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            _useCase.ExecuteAsync(dealershipId, request));

        _mockCredentialRepository.Verify(x => x.CreateAsync(It.IsAny<EasyCarsCredential>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_CredentialsAlreadyExist_ThrowsDuplicateCredentialException()
    {
        // Arrange
        var dealershipId = 1;
        var request = new CreateCredentialRequest
        {
            AccountNumber = "12345678-1234-1234-1234-123456789012",
            AccountSecret = "87654321-4321-4321-4321-210987654321",
            Environment = "Test"
        };

        var dealership = Dealership.Create("Test Dealership", "123 Test St", "1234567890", "test@test.com");

        _mockDealershipRepository
            .Setup(x => x.GetByIdAsync(dealershipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dealership);

        _mockCredentialRepository
            .Setup(x => x.ExistsForDealershipAsync(dealershipId))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateCredentialException>(() => 
            _useCase.ExecuteAsync(dealershipId, request));

        _mockCredentialRepository.Verify(x => x.CreateAsync(It.IsAny<EasyCarsCredential>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_EncryptsCredentialsBeforeStorage()
    {
        // Arrange
        var dealershipId = 1;
        var request = new CreateCredentialRequest
        {
            AccountNumber = "12345678-1234-1234-1234-123456789012",
            AccountSecret = "87654321-4321-4321-4321-210987654321",
            Environment = "Production"
        };

        var dealership = Dealership.Create("Test Dealership", "123 Test St", "1234567890", "test@test.com");
        var encryptedAccountNumber = Convert.ToBase64String(new byte[28]);
        var encryptedAccountSecret = Convert.ToBase64String(new byte[28]);

        _mockDealershipRepository
            .Setup(x => x.GetByIdAsync(dealershipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dealership);

        _mockCredentialRepository
            .Setup(x => x.ExistsForDealershipAsync(dealershipId))
            .ReturnsAsync(false);

        _mockEncryptionService
            .Setup(x => x.EncryptAsync(request.AccountNumber))
            .ReturnsAsync(encryptedAccountNumber);

        _mockEncryptionService
            .Setup(x => x.EncryptAsync(request.AccountSecret))
            .ReturnsAsync(encryptedAccountSecret);

        _mockCredentialRepository
            .Setup(x => x.CreateAsync(It.IsAny<EasyCarsCredential>()))
            .ReturnsAsync(It.IsAny<EasyCarsCredential>());

        _mockMapper
            .Setup(x => x.Map<CredentialResponse>(It.IsAny<EasyCarsCredential>()))
            .Returns(new CredentialResponse());

        // Act
        await _useCase.ExecuteAsync(dealershipId, request);

        // Assert
        _mockEncryptionService.Verify(x => x.EncryptAsync(request.AccountNumber), Times.Once);
        _mockEncryptionService.Verify(x => x.EncryptAsync(request.AccountSecret), Times.Once);
    }
}
