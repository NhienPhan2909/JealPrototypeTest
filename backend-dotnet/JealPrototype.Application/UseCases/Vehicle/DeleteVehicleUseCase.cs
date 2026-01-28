using JealPrototype.Application.DTOs.Common;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Vehicle;

public class DeleteVehicleUseCase
{
    private readonly IVehicleRepository _vehicleRepository;

    public DeleteVehicleUseCase(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<ApiResponse<bool>> ExecuteAsync(
        int vehicleId,
        int dealershipId,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleRepository.GetByIdAndDealershipAsync(vehicleId, dealershipId, cancellationToken);

        if (vehicle == null)
        {
            return ApiResponse<bool>.ErrorResponse("Vehicle not found");
        }

        await _vehicleRepository.DeleteAsync(vehicle, cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Vehicle deleted successfully");
    }
}
