using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.SalesRequest;
using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.SalesRequest;

public class CreateSalesRequestUseCase
{
    private readonly ISalesRequestRepository _salesRequestRepository;
    private readonly IDealershipRepository _dealershipRepository;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public CreateSalesRequestUseCase(
        ISalesRequestRepository salesRequestRepository,
        IDealershipRepository dealershipRepository,
        IEmailService emailService,
        IMapper mapper)
    {
        _salesRequestRepository = salesRequestRepository;
        _dealershipRepository = dealershipRepository;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<SalesRequestResponseDto>> ExecuteAsync(
        int dealershipId,
        CreateSalesRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var salesRequest = Domain.Entities.SalesRequest.Create(
            dealershipId,
            request.Name,
            request.Email,
            request.Phone,
            request.Make,
            request.Model,
            request.Year,
            request.Kilometers,
            request.AdditionalMessage);

        var created = await _salesRequestRepository.AddAsync(salesRequest, cancellationToken);

        // Send email notification
        var dealership = await _dealershipRepository.GetByIdAsync(dealershipId, cancellationToken);
        if (dealership != null)
        {
            try
            {
                await _emailService.SendSalesRequestNotificationAsync(
                    dealership.Email.Value,
                    request.Name,
                    request.Email,
                    request.Phone,
                    request.Make,
                    request.Model,
                    request.Year,
                    request.Kilometers,
                    request.AdditionalMessage,
                    cancellationToken);
            }
            catch
            {
                // Log error but don't fail the request
            }
        }

        var response = _mapper.Map<SalesRequestResponseDto>(created);
        return ApiResponse<SalesRequestResponseDto>.SuccessResponse(response, "Sales request created successfully");
    }
}
