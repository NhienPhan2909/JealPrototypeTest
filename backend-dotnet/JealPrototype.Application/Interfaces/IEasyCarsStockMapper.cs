using JealPrototype.Application.DTOs.EasyCars;

namespace JealPrototype.Application.Interfaces;

/// <summary>
/// Service for mapping EasyCars StockItem data to Vehicle entity
/// </summary>
public interface IEasyCarsStockMapper
{
    /// <summary>
    /// Maps a StockItem from EasyCars API to a Vehicle entity and syncs images.
    /// Handles duplicate detection, field mapping, raw data storage, and image sync.
    /// </summary>
    /// <param name="stockItem">StockItem from EasyCars API</param>
    /// <param name="dealershipId">Dealership ID for the vehicle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>StockMapResult containing the vehicle and image sync counts</returns>
    Task<StockMapResult> MapToVehicleAsync(StockItem stockItem, int dealershipId, CancellationToken cancellationToken = default);
}
