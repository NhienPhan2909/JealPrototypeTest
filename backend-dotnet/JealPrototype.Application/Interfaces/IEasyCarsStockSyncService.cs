using JealPrototype.Application.DTOs.EasyCars;

namespace JealPrototype.Application.Interfaces;

/// <summary>
/// Service for orchestrating EasyCars stock synchronization
/// Coordinates API data retrieval, mapping, and audit logging
/// </summary>
public interface IEasyCarsStockSyncService
{
    /// <summary>
    /// Synchronizes stock data from EasyCars to local database for specified dealership
    /// </summary>
    /// <param name="dealershipId">ID of the dealership to sync</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sync result with status, counts, and any errors</returns>
    Task<SyncResult> SyncStockAsync(int dealershipId, CancellationToken cancellationToken = default);
}
