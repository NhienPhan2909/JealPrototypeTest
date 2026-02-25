using AutoMapper;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Exceptions;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JealPrototype.Application.UseCases.EasyCars;

/// <summary>
/// Use case for retrieving EasyCars credentials
/// </summary>
public class GetCredentialUseCase
{
    private readonly IEasyCarsCredentialRepository _credentialRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCredentialUseCase> _logger;

    public GetCredentialUseCase(
        IEasyCarsCredentialRepository credentialRepository,
        IMapper mapper,
        ILogger<GetCredentialUseCase> logger)
    {
        _credentialRepository = credentialRepository ?? throw new ArgumentNullException(nameof(credentialRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CredentialMetadataResponse> ExecuteAsync(int dealershipId)
    {
        var credential = await _credentialRepository.GetByDealershipIdAsync(dealershipId);

        if (credential == null)
        {
            _logger.LogInformation("No credentials found for dealership {DealershipId}", dealershipId);
            throw new CredentialNotFoundException($"No credentials configured for dealership {dealershipId}");
        }

        _logger.LogInformation("Credentials retrieved for dealership {DealershipId}", dealershipId);

        return _mapper.Map<CredentialMetadataResponse>(credential);
    }
}
