using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.Dealership;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Dealership;

public class GetDealershipUseCase
{
    private readonly IDealershipRepository _dealershipRepository;
    private readonly IMapper _mapper;

    public GetDealershipUseCase(IDealershipRepository dealershipRepository, IMapper mapper)
    {
        _dealershipRepository = dealershipRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<DealershipResponseDto>> ExecuteAsync(
        int dealershipId,
        CancellationToken cancellationToken = default)
    {
        var dealership = await _dealershipRepository.GetByIdAsync(dealershipId, cancellationToken);

        if (dealership == null)
        {
            return ApiResponse<DealershipResponseDto>.ErrorResponse("Dealership not found");
        }

        var response = _mapper.Map<DealershipResponseDto>(dealership);
        return ApiResponse<DealershipResponseDto>.SuccessResponse(response);
    }
}
