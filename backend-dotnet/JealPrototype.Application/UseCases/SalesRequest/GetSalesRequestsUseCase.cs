using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.SalesRequest;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.SalesRequest;

public class GetSalesRequestsUseCase
{
    private readonly ISalesRequestRepository _salesRequestRepository;
    private readonly IMapper _mapper;

    public GetSalesRequestsUseCase(ISalesRequestRepository salesRequestRepository, IMapper mapper)
    {
        _salesRequestRepository = salesRequestRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<SalesRequestResponseDto>>> ExecuteAsync(
        int dealershipId,
        CancellationToken cancellationToken = default)
    {
        var salesRequests = await _salesRequestRepository.GetByDealershipIdAsync(dealershipId, cancellationToken);
        var response = _mapper.Map<List<SalesRequestResponseDto>>(salesRequests);

        return ApiResponse<List<SalesRequestResponseDto>>.SuccessResponse(response);
    }
}
