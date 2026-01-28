using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;

namespace JealPrototype.Domain.Interfaces;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<IEnumerable<Vehicle>> GetByDealershipIdAsync(
        int dealershipId,
        VehicleStatus? status = null,
        string? brand = null,
        int? minYear = null,
        int? maxYear = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default);
        
    Task<Vehicle?> GetByIdAndDealershipAsync(int id, int dealershipId, CancellationToken cancellationToken = default);
}
