using JealPrototype.Domain.Enums;

namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// DTO for sync history list item
/// </summary>
public class SyncHistoryDto
{
    public int Id { get; set; }
    public DateTime SyncedAt { get; set; }
    public SyncStatus Status { get; set; }
    public int ItemsProcessed { get; set; }
    public int ItemsSucceeded { get; set; }
    public int ItemsFailed { get; set; }
    public long DurationMs { get; set; }
}

/// <summary>
/// Paginated response for sync history
/// </summary>
public class SyncHistoryResponse
{
    public List<SyncHistoryDto> Logs { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
