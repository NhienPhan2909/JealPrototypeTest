using JealPrototype.Domain.Enums;

namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Detailed DTO for a specific sync log entry
/// Includes full error messages
/// </summary>
public class SyncLogDetailsDto
{
    public int Id { get; set; }
    public int DealershipId { get; set; }
    public DateTime SyncedAt { get; set; }
    public SyncStatus Status { get; set; }
    public int ItemsProcessed { get; set; }
    public int ItemsSucceeded { get; set; }
    public int ItemsFailed { get; set; }
    public List<string> Errors { get; set; } = new();
    public long DurationMs { get; set; }
}
