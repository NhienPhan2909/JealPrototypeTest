using AutoMapper;
using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.DTOs.Vehicle;
using JealPrototype.Domain.Interfaces;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.ValueObjects;

namespace JealPrototype.Application.UseCases.Vehicle;

public class UpdateVehicleUseCase
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IMapper _mapper;

    public UpdateVehicleUseCase(IVehicleRepository vehicleRepository, IMapper mapper)
    {
        _vehicleRepository = vehicleRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<VehicleResponseDto>> ExecuteAsync(
        int vehicleId,
        int dealershipId,
        UpdateVehicleDto request,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleRepository.GetByIdAndDealershipAsync(vehicleId, dealershipId, cancellationToken);

        if (vehicle == null)
        {
            return ApiResponse<VehicleResponseDto>.ErrorResponse("Vehicle not found");
        }

        vehicle.Update(
            request.Make ?? vehicle.Make,
            request.Model ?? vehicle.Model,
            request.Year ?? vehicle.Year,
            request.Price ?? vehicle.Price,
            request.Mileage ?? vehicle.Mileage,
            !string.IsNullOrWhiteSpace(request.Condition)
                ? request.Condition.ToLower() == "new" ? VehicleCondition.New : VehicleCondition.Used
                : vehicle.Condition,
            !string.IsNullOrWhiteSpace(request.Status)
                ? request.Status.ToLower() switch
                {
                    "draft" => VehicleStatus.Draft,
                    "active" => VehicleStatus.Active,
                    "pending" => VehicleStatus.Pending,
                    "sold" => VehicleStatus.Sold,
                    _ => vehicle.Status
                }
                : vehicle.Status,
            request.Title ?? vehicle.Title,
            request.Description ?? vehicle.Description,
            request.Images);

        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);

        var response = _mapper.Map<VehicleResponseDto>(vehicle);
        return ApiResponse<VehicleResponseDto>.SuccessResponse(response, "Vehicle updated successfully");
    }
}
