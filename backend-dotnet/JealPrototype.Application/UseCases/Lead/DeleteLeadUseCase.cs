using JealPrototype.Application.DTOs.Common;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Lead;

public class DeleteLeadUseCase
{
    private readonly ILeadRepository _leadRepository;

    public DeleteLeadUseCase(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<ApiResponse<bool>> ExecuteAsync(
        int leadId,
        int dealershipId,
        CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(leadId, cancellationToken);

        if (lead == null || lead.DealershipId != dealershipId)
        {
            return ApiResponse<bool>.ErrorResponse("Lead not found or does not belong to this dealership");
        }

        await _leadRepository.DeleteAsync(lead, cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Lead deleted successfully");
    }
}
