using JealPrototype.Application.Exceptions;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JealPrototype.Application.UseCases.EasyCars;

/// <summary>
/// Use case for deleting EasyCars credentials
/// </summary>
public class DeleteCredentialUseCase
{
    private readonly IEasyCarsCredentialRepository _credentialRepository;
    private readonly ILogger<DeleteCredentialUseCase> _logger;

    public DeleteCredentialUseCase(
        IEasyCarsCredentialRepository credentialRepository,
        ILogger<DeleteCredentialUseCase> logger)
    {
        _credentialRepository = credentialRepository ?? throw new ArgumentNullException(nameof(credentialRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(int dealershipId, int credentialId)
    {
        // 1. Retrieve credential
        var credential = await _credentialRepository.GetByIdAsync(credentialId);

        if (credential == null)
        {
            _logger.LogWarning("Attempted to delete non-existent credential {CredentialId}", credentialId);
            throw new CredentialNotFoundException($"Credential {credentialId} not found");
        }

        // 2. Validate ownership
        if (credential.DealershipId != dealershipId)
        {
            _logger.LogWarning(
                "Unauthorized attempt to delete credential {CredentialId} by dealership {DealershipId}",
                credentialId, dealershipId);
            throw new UnauthorizedAccessException("You do not have permission to delete this credential");
        }

        // 3. Delete credential
        await _credentialRepository.DeleteAsync(credentialId);

        // 4. Log operation
        _logger.LogInformation(
            "Credential {CredentialId} deleted for dealership {DealershipId}",
            credentialId, dealershipId);
    }
}
