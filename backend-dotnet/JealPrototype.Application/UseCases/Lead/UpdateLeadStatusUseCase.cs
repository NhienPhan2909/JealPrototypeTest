using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.Lead;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Lead;

public class UpdateLeadStatusUseCase
{
    private readonly ILeadRepository _leadRepository;
    private readonly IMapper _mapper;

    public UpdateLeadStatusUseCase(ILeadRepository leadRepository, IMapper mapper)
    {
        _leadRepository = leadRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<LeadResponseDto>> ExecuteAsync(
        int leadId,
        int dealershipId,
        UpdateLeadStatusDto request,
        CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(leadId, cancellationToken);

        if (lead == null || lead.DealershipId != dealershipId)
        {
            return ApiResponse<LeadResponseDto>.ErrorResponse("Lead not found");
        }

        var newStatus = request.Status.ToLower() switch
        {
            "received" => LeadStatus.Received,
            "in progress" => LeadStatus.InProgress,
            "done" => LeadStatus.Done,
            _ => lead.Status
        };

        lead.UpdateStatus(newStatus);
        await _leadRepository.UpdateAsync(lead, cancellationToken);

        var response = _mapper.Map<LeadResponseDto>(lead);
        return ApiResponse<LeadResponseDto>.SuccessResponse(response, "Lead status updated successfully");
    }
}
