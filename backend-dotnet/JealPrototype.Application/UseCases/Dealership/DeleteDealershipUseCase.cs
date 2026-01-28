using JealPrototype.Application.DTOs.Common;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Dealership;

public class DeleteDealershipUseCase
{
    private readonly IDealershipRepository _dealershipRepository;

    public DeleteDealershipUseCase(IDealershipRepository dealershipRepository)
    {
        _dealershipRepository = dealershipRepository;
    }

    public async Task<ApiResponse<bool>> ExecuteAsync(
        int dealershipId,
        CancellationToken cancellationToken = default)
    {
        var dealership = await _dealershipRepository.GetByIdAsync(dealershipId, cancellationToken);

        if (dealership == null)
        {
            return ApiResponse<bool>.ErrorResponse("Dealership not found");
        }

        await _dealershipRepository.DeleteAsync(dealership, cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Dealership deleted successfully");
    }
}
