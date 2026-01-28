using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.Dealership;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Dealership;

public class GetAllDealershipsUseCase
{
    private readonly IDealershipRepository _dealershipRepository;
    private readonly IMapper _mapper;

    public GetAllDealershipsUseCase(IDealershipRepository dealershipRepository, IMapper mapper)
    {
        _dealershipRepository = dealershipRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<DealershipResponseDto>>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var dealerships = await _dealershipRepository.GetAllAsync(cancellationToken);
        var response = _mapper.Map<List<DealershipResponseDto>>(dealerships);

        return ApiResponse<List<DealershipResponseDto>>.SuccessResponse(response);
    }
}
