using AutoMapper;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Exceptions;
using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JealPrototype.Application.UseCases.EasyCars;

/// <summary>
/// Use case for updating EasyCars credentials
/// </summary>
public class UpdateCredentialUseCase
{
    private readonly IEasyCarsCredentialRepository _credentialRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCredentialUseCase> _logger;

    public UpdateCredentialUseCase(
        IEasyCarsCredentialRepository credentialRepository,
        IEncryptionService encryptionService,
        IMapper mapper,
        ILogger<UpdateCredentialUseCase> logger)
    {
        _credentialRepository = credentialRepository ?? throw new ArgumentNullException(nameof(credentialRepository));
        _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CredentialResponse> ExecuteAsync(int dealershipId, int credentialId, UpdateCredentialRequest request)
    {
        // 1. Retrieve existing credential
        var credential = await _credentialRepository.GetByIdAsync(credentialId);

        if (credential == null)
        {
            _logger.LogWarning("Attempted to update non-existent credential {CredentialId}", credentialId);
            throw new CredentialNotFoundException($"Credential {credentialId} not found");
        }

        // 2. Validate ownership
        if (credential.DealershipId != dealershipId)
        {
            _logger.LogWarning(
                "Unauthorized attempt to update credential {CredentialId} by dealership {DealershipId}",
                credentialId, dealershipId);
            throw new UnauthorizedAccessException("You do not have permission to update this credential");
        }

        // 3. Prepare update values - use existing if not provided
        string accountNumberEncrypted = credential.AccountNumberEncrypted;
        string accountSecretEncrypted = credential.AccountSecretEncrypted;
        string environment = credential.Environment;
        string? yardCode = credential.YardCode;

        if (!string.IsNullOrEmpty(request.AccountNumber))
        {
            accountNumberEncrypted = await _encryptionService.EncryptAsync(request.AccountNumber);
        }

        if (!string.IsNullOrEmpty(request.AccountSecret))
        {
            accountSecretEncrypted = await _encryptionService.EncryptAsync(request.AccountSecret);
        }

        if (!string.IsNullOrEmpty(request.Environment))
        {
            environment = request.Environment;
        }

        if (request.YardCode != null)
        {
            yardCode = request.YardCode;
        }

        // Extract IV from the encrypted string (use account number's IV)
        var encryptionIV = ExtractIVFromEncrypted(accountNumberEncrypted);

        // 4. Update credential using entity method
        credential.UpdateCredentials(
            accountNumberEncrypted: accountNumberEncrypted,
            accountSecretEncrypted: accountSecretEncrypted,
            encryptionIV: encryptionIV,
            environment: environment,
            yardCode: yardCode);

        if (request.IsActive.HasValue)
        {
            credential.SetActive(request.IsActive.Value);
        }

        // 5. Save changes
        var updated = await _credentialRepository.UpdateAsync(credential);

        // 6. Log operation
        _logger.LogInformation(
            "Credential {CredentialId} updated for dealership {DealershipId}",
            credentialId, dealershipId);

        // 7. Return response
        return _mapper.Map<CredentialResponse>(updated);
    }

    private string ExtractIVFromEncrypted(string encrypted)
    {
        // Encrypted format: Base64(nonce[12] + tag[16] + ciphertext[variable])
        var bytes = Convert.FromBase64String(encrypted);
        var iv = new byte[12]; // 96 bits
        Buffer.BlockCopy(bytes, 0, iv, 0, 12);
        return Convert.ToBase64String(iv);
    }
}
