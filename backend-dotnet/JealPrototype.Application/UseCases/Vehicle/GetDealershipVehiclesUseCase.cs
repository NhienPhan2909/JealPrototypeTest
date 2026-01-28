using JealPrototype.Application.DTOs.Vehicle;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Application.UseCases.Vehicle;

public class GetDealershipVehiclesUseCase
{
    private readonly IVehicleRepository _vehicleRepository;

    public GetDealershipVehiclesUseCase(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<IEnumerable<VehicleResponseDto>> ExecuteAsync(int dealershipId)
    {
        var vehicles = await _vehicleRepository.GetByDealershipIdAsync(dealershipId);

        return vehicles.Select(vehicle => new VehicleResponseDto
        {
            Id = vehicle.Id,
            DealershipId = vehicle.DealershipId,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Price = vehicle.Price,
            Mileage = vehicle.Mileage,
            Condition = vehicle.Condition.ToString().ToLower(),
            Status = vehicle.Status.ToString().ToLower(),
            Title = vehicle.Title,
            Description = vehicle.Description,
            Images = vehicle.Images,
            CreatedAt = vehicle.CreatedAt
        });
    }
}
