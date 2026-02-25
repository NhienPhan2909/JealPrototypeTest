using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

/// <summary>
/// Repository for managing EasyCars stock data (raw JSON storage)
/// </summary>
public interface IEasyCarsStockDataRepository
{
    /// <summary>
    /// Finds stock data by vehicle ID
    /// </summary>
    Task<EasyCarsStockData?> FindByVehicleIdAsync(int vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates stock data for a vehicle (upsert operation)
    /// </summary>
    Task<EasyCarsStockData> UpsertAsync(int vehicleId, string stockItemJson, string apiVersion = "1.0", CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds new stock data record
    /// </summary>
    Task<EasyCarsStockData> AddAsync(EasyCarsStockData stockData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates existing stock data record
    /// </summary>
    Task<EasyCarsStockData> UpdateAsync(EasyCarsStockData stockData, CancellationToken cancellationToken = default);
}
