using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.Dealership;
using JealPrototype.Domain.Interfaces;
using JealPrototype.Domain.ValueObjects;

namespace JealPrototype.Application.UseCases.Dealership;

public class CreateDealershipUseCase
{
    private readonly IDealershipRepository _dealershipRepository;
    private readonly IMapper _mapper;

    public CreateDealershipUseCase(IDealershipRepository dealershipRepository, IMapper mapper)
    {
        _dealershipRepository = dealershipRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<DealershipResponseDto>> ExecuteAsync(
        CreateDealershipDto request,
        CancellationToken cancellationToken = default)
    {
        var dealership = Domain.Entities.Dealership.Create(
            request.Name,
            request.Address,
            request.Phone,
            request.Email,
            request.LogoUrl,
            request.Hours,
            request.About,
            request.WebsiteUrl);

        var createdDealership = await _dealershipRepository.AddAsync(dealership, cancellationToken);
        var response = _mapper.Map<DealershipResponseDto>(createdDealership);

        return ApiResponse<DealershipResponseDto>.SuccessResponse(response, "Dealership created successfully");
    }
}
