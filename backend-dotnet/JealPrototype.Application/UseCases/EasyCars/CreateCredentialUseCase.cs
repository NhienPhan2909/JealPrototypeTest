using AutoMapper;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Exceptions;
using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JealPrototype.Application.UseCases.EasyCars;

/// <summary>
/// Use case for creating EasyCars credentials
/// </summary>
public class CreateCredentialUseCase
{
    private readonly IEasyCarsCredentialRepository _credentialRepository;
    private readonly IDealershipRepository _dealershipRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCredentialUseCase> _logger;

    public CreateCredentialUseCase(
        IEasyCarsCredentialRepository credentialRepository,
        IDealershipRepository dealershipRepository,
        IEncryptionService encryptionService,
        IMapper mapper,
        ILogger<CreateCredentialUseCase> logger)
    {
        _credentialRepository = credentialRepository ?? throw new ArgumentNullException(nameof(credentialRepository));
        _dealershipRepository = dealershipRepository ?? throw new ArgumentNullException(nameof(dealershipRepository));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CredentialResponse> ExecuteAsync(int dealershipId, CreateCredentialRequest request)
    {
        // 1. Validate dealership exists
        var dealership = await _dealershipRepository.GetByIdAsync(dealershipId);
        if (dealership == null)
        {
            _logger.LogWarning("Attempted to create credentials for non-existent dealership {DealershipId}", dealershipId);
            throw new NotFoundException($"Dealership {dealershipId} not found");
        }

        // 2. Check for existing credentials
        var exists = await _credentialRepository.ExistsForDealershipAsync(dealershipId);
        if (exists)
        {
            _logger.LogWarning("Attempted to create duplicate credentials for dealership {DealershipId}", dealershipId);
            throw new DuplicateCredentialException(
                "Credentials already exist for this dealership. Use PUT to update.");
        }

        // 3. Encrypt credentials
        var encryptedClientId = await _encryptionService.EncryptAsync(request.ClientId);
        var encryptedClientSecret = await _encryptionService.EncryptAsync(request.ClientSecret);
        var encryptedAccountNumber = await _encryptionService.EncryptAsync(request.AccountNumber);
        var encryptedAccountSecret = await _encryptionService.EncryptAsync(request.AccountSecret);

        // Extract IV from encrypted string (first 12 bytes of decoded base64 = 16 chars in base64)
        var encryptionIV = ExtractIVFromEncrypted(encryptedAccountNumber);

        // 4. Create entity using factory method
        var credential = EasyCarsCredential.Create(
            dealershipId: dealershipId,
            clientIdEncrypted: encryptedClientId,
            clientSecretEncrypted: encryptedClientSecret,
            accountNumberEncrypted: encryptedAccountNumber,
            accountSecretEncrypted: encryptedAccountSecret,
            encryptionIV: encryptionIV,
            environment: request.Environment,
            isActive: true,
            yardCode: request.YardCode);

        // 5. Save to database
        var saved = await _credentialRepository.CreateAsync(credential);

        // 6. Log operation (without sensitive data)
        _logger.LogInformation(
            "Credential created for dealership {DealershipId} in {Environment} environment",
            dealershipId,
            request.Environment);

        // 7. Map and return response
        return _mapper.Map<CredentialResponse>(saved);
    }

    private string ExtractIVFromEncrypted(string encrypted)
    {
        // Encrypted format: Base64(nonce[12] + tag[16] + ciphertext[variable])
        // Decode base64 and extract first 12 bytes (nonce/IV)
        var bytes = Convert.FromBase64String(encrypted);
        var iv = new byte[12]; // 96 bits
        Buffer.BlockCopy(bytes, 0, iv, 0, 12);
        return Convert.ToBase64String(iv);
    }
}
