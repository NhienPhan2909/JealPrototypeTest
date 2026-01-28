using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.Vehicle;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Vehicle;

public class CreateVehicleUseCase
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IMapper _mapper;

    public CreateVehicleUseCase(IVehicleRepository vehicleRepository, IMapper mapper)
    {
        _vehicleRepository = vehicleRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<VehicleResponseDto>> ExecuteAsync(
        int dealershipId,
        CreateVehicleDto request,
        CancellationToken cancellationToken = default)
    {
        var vehicle = Domain.Entities.Vehicle.Create(
            dealershipId,
            request.Make,
            request.Model,
            request.Year,
            request.Price,
            request.Mileage,
            request.Condition.ToLower() == "new" ? VehicleCondition.New : VehicleCondition.Used,
            request.Status.ToLower() switch
            {
                "draft" => VehicleStatus.Draft,
                "active" => VehicleStatus.Active,
                "pending" => VehicleStatus.Pending,
                "sold" => VehicleStatus.Sold,
                _ => VehicleStatus.Draft
            },
            request.Title,
            request.Description,
            request.Images);

        var createdVehicle = await _vehicleRepository.AddAsync(vehicle, cancellationToken);
        var response = _mapper.Map<VehicleResponseDto>(createdVehicle);

        return ApiResponse<VehicleResponseDto>.SuccessResponse(response, "Vehicle created successfully");
    }
}
