using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.Dealership;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Dealership;

public class GetDealershipByUrlUseCase
{
    private readonly IDealershipRepository _dealershipRepository;
    private readonly IMapper _mapper;

    public GetDealershipByUrlUseCase(IDealershipRepository dealershipRepository, IMapper mapper)
    {
        _dealershipRepository = dealershipRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<DealershipResponseDto>> ExecuteAsync(
        string websiteUrl,
        CancellationToken cancellationToken = default)
    {
        var dealership = await _dealershipRepository.GetByWebsiteUrlAsync(websiteUrl, cancellationToken);

        if (dealership == null)
        {
            return ApiResponse<DealershipResponseDto>.ErrorResponse("Dealership not found");
        }

        var response = _mapper.Map<DealershipResponseDto>(dealership);
        return ApiResponse<DealershipResponseDto>.SuccessResponse(response);
    }
}
