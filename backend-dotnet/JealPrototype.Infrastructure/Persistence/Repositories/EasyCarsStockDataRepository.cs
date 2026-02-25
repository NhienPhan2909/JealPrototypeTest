using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Interfaces;
using JealPrototype.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JealPrototype.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for managing EasyCars stock data (raw JSON storage)
/// </summary>
public class EasyCarsStockDataRepository : IEasyCarsStockDataRepository
{
    private readonly ApplicationDbContext _context;

    public EasyCarsStockDataRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<EasyCarsStockData?> FindByVehicleIdAsync(int vehicleId, CancellationToken cancellationToken = default)
    {
        return await _context.EasyCarsStockData
            .FirstOrDefaultAsync(x => x.VehicleId == vehicleId, cancellationToken);
    }

    public async Task<EasyCarsStockData> UpsertAsync(int vehicleId, string stockItemJson, string apiVersion = "1.0", CancellationToken cancellationToken = default)
    {
        var existing = await FindByVehicleIdAsync(vehicleId, cancellationToken);

        if (existing != null)
        {
            existing.UpdateStockData(stockItemJson);
            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }
        else
        {
            var newStockData = EasyCarsStockData.Create(vehicleId, stockItemJson, apiVersion);
            return await AddAsync(newStockData, cancellationToken);
        }
    }

    public async Task<EasyCarsStockData> AddAsync(EasyCarsStockData stockData, CancellationToken cancellationToken = default)
    {
        await _context.EasyCarsStockData.AddAsync(stockData, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return stockData;
    }

    public async Task<EasyCarsStockData> UpdateAsync(EasyCarsStockData stockData, CancellationToken cancellationToken = default)
    {
        _context.EasyCarsStockData.Update(stockData);
        await _context.SaveChangesAsync(cancellationToken);
        return stockData;
    }
}
