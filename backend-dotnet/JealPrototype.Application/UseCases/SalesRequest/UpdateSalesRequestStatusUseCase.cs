using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.SalesRequest;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.SalesRequest;

public class UpdateSalesRequestStatusUseCase
{
    private readonly ISalesRequestRepository _salesRequestRepository;
    private readonly IMapper _mapper;

    public UpdateSalesRequestStatusUseCase(ISalesRequestRepository salesRequestRepository, IMapper mapper)
    {
        _salesRequestRepository = salesRequestRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<SalesRequestResponseDto>> ExecuteAsync(
        int salesRequestId,
        int dealershipId,
        string status,
        CancellationToken cancellationToken = default)
    {
        var salesRequest = await _salesRequestRepository.GetByIdAsync(salesRequestId, cancellationToken);

        if (salesRequest == null || salesRequest.DealershipId != dealershipId)
        {
            return ApiResponse<SalesRequestResponseDto>.ErrorResponse("Sales request not found");
        }

        var newStatus = status.ToLower() switch
        {
            "received" => LeadStatus.Received,
            "in progress" => LeadStatus.InProgress,
            "done" => LeadStatus.Done,
            _ => salesRequest.Status
        };

        salesRequest.UpdateStatus(newStatus);
        await _salesRequestRepository.UpdateAsync(salesRequest, cancellationToken);

        var response = _mapper.Map<SalesRequestResponseDto>(salesRequest);
        return ApiResponse<SalesRequestResponseDto>.SuccessResponse(response, "Sales request status updated successfully");
    }
}
