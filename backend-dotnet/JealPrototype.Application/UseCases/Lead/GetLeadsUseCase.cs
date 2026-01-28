using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.Lead;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Lead;

public class GetLeadsUseCase
{
    private readonly ILeadRepository _leadRepository;
    private readonly IMapper _mapper;

    public GetLeadsUseCase(ILeadRepository leadRepository, IMapper mapper)
    {
        _leadRepository = leadRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<LeadResponseDto>>> ExecuteAsync(
        int dealershipId,
        CancellationToken cancellationToken = default)
    {
        var leads = await _leadRepository.GetByDealershipIdAsync(dealershipId, cancellationToken);
        var response = _mapper.Map<List<LeadResponseDto>>(leads);

        return ApiResponse<List<LeadResponseDto>>.SuccessResponse(response);
    }
}
