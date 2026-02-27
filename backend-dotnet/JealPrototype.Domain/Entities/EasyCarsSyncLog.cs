using JealPrototype.Domain.Enums;

namespace JealPrototype.Domain.Entities;

/// <summary>
/// Audit log for EasyCars stock synchronization operations
/// Tracks every sync operation for troubleshooting and monitoring
/// </summary>
public class EasyCarsSyncLog : BaseEntity
{
    public int DealershipId { get; private set; }
    public DateTime SyncedAt { get; private set; }
    public SyncStatus Status { get; private set; }
    public int ItemsProcessed { get; private set; }
    public int ItemsSucceeded { get; private set; }
    public int ItemsFailed { get; private set; }
    public string ErrorMessages { get; private set; } = "[]";
    public long DurationMs { get; private set; }
    public string ApiVersion { get; private set; } = "1.0";
    public string? SyncType { get; private set; }

    public Dealership Dealership { get; private set; } = null!;

    private EasyCarsSyncLog() { }

    /// <summary>
    /// Creates a new sync log entry for Story 2.3
    /// </summary>
    public static EasyCarsSyncLog Create(
        int dealershipId,
        SyncStatus status,
        int itemsProcessed,
        int itemsSucceeded,
        int itemsFailed,
        List<string> errors,
        long durationMs,
        string apiVersion = "1.0",
        string? syncType = null)
    {
        if (dealershipId <= 0)
            throw new ArgumentException("Dealership ID must be positive", nameof(dealershipId));

        if (itemsProcessed < 0)
            throw new ArgumentException("Items processed cannot be negative", nameof(itemsProcessed));

        if (itemsSucceeded < 0 || itemsFailed < 0)
            throw new ArgumentException("Success/failure counts cannot be negative");

        if (itemsSucceeded + itemsFailed > itemsProcessed)
            throw new ArgumentException("Success + failed counts cannot exceed total processed");

        return new EasyCarsSyncLog
        {
            DealershipId = dealershipId,
            SyncedAt = DateTime.UtcNow,
            Status = status,
            ItemsProcessed = itemsProcessed,
            ItemsSucceeded = itemsSucceeded,
            ItemsFailed = itemsFailed,
            ErrorMessages = System.Text.Json.JsonSerializer.Serialize(errors ?? new List<string>()),
            DurationMs = durationMs,
            ApiVersion = apiVersion,
            SyncType = syncType
        };
    }
}
