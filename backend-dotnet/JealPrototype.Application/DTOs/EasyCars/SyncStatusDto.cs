using JealPrototype.Domain.Enums;

namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// DTO for current sync status response
/// Returns last sync information and credential status
/// </summary>
public class SyncStatusDto
{
    public DateTime? LastSyncedAt { get; set; }
    public SyncStatus? Status { get; set; }
    public int ItemsProcessed { get; set; }
    public int ItemsSucceeded { get; set; }
    public int ItemsFailed { get; set; }
    public long DurationMs { get; set; }
    public bool HasCredentials { get; set; }
}
