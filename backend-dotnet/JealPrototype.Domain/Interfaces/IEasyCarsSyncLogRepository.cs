using JealPrototype.Domain.Entities;

namespace JealPrototype.Domain.Interfaces;

/// <summary>
/// Repository for EasyCars sync log operations
/// </summary>
public interface IEasyCarsSyncLogRepository
{
    /// <summary>
    /// Adds a new sync log entry
    /// </summary>
    Task<EasyCarsSyncLog> AddAsync(EasyCarsSyncLog syncLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent sync log for a dealership
    /// </summary>
    Task<EasyCarsSyncLog?> GetLastSyncAsync(int dealershipId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sync log history for a dealership
    /// </summary>
    Task<List<EasyCarsSyncLog>> GetSyncHistoryAsync(int dealershipId, int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific sync log by ID
    /// </summary>
    Task<EasyCarsSyncLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated sync log history for a dealership
    /// </summary>
    Task<(List<EasyCarsSyncLog> Logs, int Total)> GetPagedHistoryAsync(int dealershipId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Gets the most recent sync log for a dealership filtered by SyncType.</summary>
    Task<EasyCarsSyncLog?> GetLastSyncByTypeAsync(int dealershipId, string syncType, CancellationToken cancellationToken = default);

    /// <summary>Gets paginated sync log history filtered by SyncType.</summary>
    Task<(List<EasyCarsSyncLog> Logs, int Total)> GetPagedHistoryByTypeAsync(int dealershipId, string syncType, int page, int pageSize, CancellationToken cancellationToken = default);
}
