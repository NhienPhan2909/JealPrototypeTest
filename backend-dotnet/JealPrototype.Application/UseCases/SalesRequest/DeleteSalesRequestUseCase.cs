using JealPrototype.Application.DTOs.Common;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.SalesRequest;

public class DeleteSalesRequestUseCase
{
    private readonly ISalesRequestRepository _salesRequestRepository;

    public DeleteSalesRequestUseCase(ISalesRequestRepository salesRequestRepository)
    {
        _salesRequestRepository = salesRequestRepository;
    }

    public async Task<ApiResponse<bool>> ExecuteAsync(
        int salesRequestId,
        int dealershipId,
        CancellationToken cancellationToken = default)
    {
        var salesRequest = await _salesRequestRepository.GetByIdAsync(salesRequestId, cancellationToken);

        if (salesRequest == null || salesRequest.DealershipId != dealershipId)
        {
            return ApiResponse<bool>.ErrorResponse("Sales request not found or does not belong to this dealership");
        }

        await _salesRequestRepository.DeleteAsync(salesRequest, cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Sales request deleted successfully");
    }
}
