using JealPrototype.Domain.Enums;

namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Result of a stock synchronization operation
/// Immutable DTO returned by IEasyCarsStockSyncService
/// </summary>
public class SyncResult
{
    public SyncStatus Status { get; init; }
    public int ItemsProcessed { get; init; }
    public int ItemsSucceeded { get; init; }
    public int ItemsFailed { get; init; }
    public List<string> Errors { get; init; } = new();
    public long DurationMs { get; init; }
    public DateTime SyncedAt { get; init; } = DateTime.UtcNow;
    public int ImagesDownloaded { get; init; }
    public int ImagesFailed { get; init; }

    /// <summary>
    /// True if all items processed successfully
    /// </summary>
    public bool IsSuccess => Status == SyncStatus.Success;

    /// <summary>
    /// True if some items succeeded and some failed
    /// </summary>
    public bool IsPartialSuccess => Status == SyncStatus.PartialSuccess;

    /// <summary>
    /// True if all items failed or sync operation failed completely
    /// </summary>
    public bool IsFailed => Status == SyncStatus.Failed;

    /// <summary>
    /// Creates a successful sync result
    /// </summary>
    public static SyncResult Success(int itemsProcessed, long durationMs, int imagesDownloaded = 0, int imagesFailed = 0) => new()
    {
        Status = SyncStatus.Success,
        ItemsProcessed = itemsProcessed,
        ItemsSucceeded = itemsProcessed,
        ItemsFailed = 0,
        DurationMs = durationMs,
        Errors = new List<string>(),
        ImagesDownloaded = imagesDownloaded,
        ImagesFailed = imagesFailed
    };

    /// <summary>
    /// Creates a partial success sync result
    /// </summary>
    public static SyncResult PartialSuccess(int itemsSucceeded, int itemsFailed,
        List<string> errors, long durationMs, int imagesDownloaded = 0, int imagesFailed = 0) => new()
    {
        Status = SyncStatus.PartialSuccess,
        ItemsProcessed = itemsSucceeded + itemsFailed,
        ItemsSucceeded = itemsSucceeded,
        ItemsFailed = itemsFailed,
        Errors = errors,
        DurationMs = durationMs,
        ImagesDownloaded = imagesDownloaded,
        ImagesFailed = imagesFailed
    };

    /// <summary>
    /// Creates a failed sync result
    /// </summary>
    public static SyncResult Failure(string errorMessage, long durationMs = 0) => new()
    {
        Status = SyncStatus.Failed,
        ItemsProcessed = 0,
        ItemsSucceeded = 0,
        ItemsFailed = 0,
        Errors = new List<string> { errorMessage },
        DurationMs = durationMs
    };

    /// <summary>
    /// Creates a failed sync result with processed items
    /// </summary>
    public static SyncResult Failure(int itemsProcessed, List<string> errors, long durationMs) => new()
    {
        Status = SyncStatus.Failed,
        ItemsProcessed = itemsProcessed,
        ItemsSucceeded = 0,
        ItemsFailed = itemsProcessed,
        Errors = errors,
        DurationMs = durationMs
    };
}
