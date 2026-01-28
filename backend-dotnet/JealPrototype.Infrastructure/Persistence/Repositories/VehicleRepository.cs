using Microsoft.EntityFrameworkCore;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using JealPrototype.Domain.Interfaces;

namespace JealPrototype.Infrastructure.Persistence.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly ApplicationDbContext _context;

    public VehicleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Vehicle?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Vehicle>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles.ToListAsync(cancellationToken);
    }

    public async Task<Vehicle> AddAsync(Vehicle entity, CancellationToken cancellationToken = default)
    {
        await _context.Vehicles.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(Vehicle entity, CancellationToken cancellationToken = default)
    {
        _context.Vehicles.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Vehicle entity, CancellationToken cancellationToken = default)
    {
        _context.Vehicles.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public void Update(Vehicle entity)
    {
        _context.Vehicles.Update(entity);
    }

    public void Delete(Vehicle entity)
    {
        _context.Vehicles.Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Vehicle>> GetByDealershipIdAsync(
        int dealershipId,
        VehicleStatus? status = null,
        string? brand = null,
        int? minYear = null,
        int? maxYear = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Vehicles.Where(v => v.DealershipId == dealershipId);

        if (status.HasValue)
            query = query.Where(v => v.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(brand))
            query = query.Where(v => v.Make.ToLower() == brand.ToLower());

        if (minYear.HasValue)
            query = query.Where(v => v.Year >= minYear.Value);

        if (maxYear.HasValue)
            query = query.Where(v => v.Year <= maxYear.Value);

        if (minPrice.HasValue)
            query = query.Where(v => v.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(v => v.Price <= maxPrice.Value);

        return await query.OrderByDescending(v => v.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<Vehicle?> GetByIdAndDealershipAsync(int id, int dealershipId, CancellationToken cancellationToken = default)
    {
        return await _context.Vehicles
            .FirstOrDefaultAsync(v => v.Id == id && v.DealershipId == dealershipId, cancellationToken);
    }
}
