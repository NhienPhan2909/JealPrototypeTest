using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.Lead;
using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Lead;

public class CreateLeadUseCase
{
    private readonly ILeadRepository _leadRepository;
    private readonly IDealershipRepository _dealershipRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public CreateLeadUseCase(
        ILeadRepository leadRepository,
        IDealershipRepository dealershipRepository,
        IVehicleRepository vehicleRepository,
        IEmailService emailService,
        IMapper mapper)
    {
        _leadRepository = leadRepository;
        _dealershipRepository = dealershipRepository;
        _vehicleRepository = vehicleRepository;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<LeadResponseDto>> ExecuteAsync(
        int dealershipId,
        CreateLeadDto request,
        CancellationToken cancellationToken = default)
    {
        var lead = Domain.Entities.Lead.Create(
            dealershipId,
            request.Name,
            request.Email,
            request.Phone,
            request.Message,
            request.VehicleId);

        var createdLead = await _leadRepository.AddAsync(lead, cancellationToken);

        // Send email notification
        var dealership = await _dealershipRepository.GetByIdAsync(dealershipId, cancellationToken);
        if (dealership != null)
        {
            string? vehicleTitle = null;
            if (request.VehicleId.HasValue)
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId.Value, cancellationToken);
                vehicleTitle = vehicle?.Title;
            }

            try
            {
                await _emailService.SendLeadNotificationAsync(
                    dealership.Email.Value,
                    request.Name,
                    request.Email,
                    request.Phone,
                    request.Message,
                    vehicleTitle,
                    cancellationToken);
            }
            catch
            {
                // Log error but don't fail the request
            }
        }

        var response = _mapper.Map<LeadResponseDto>(createdLead);
        return ApiResponse<LeadResponseDto>.SuccessResponse(response, "Lead created successfully");
    }
}
