namespace JealPrototype.Application.DTOs.EasyCars;

public class LeadStatusConflictDto
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public string EasyCarsLeadNumber { get; set; } = string.Empty;
    public string LocalStatus { get; set; } = string.Empty;
    public int RemoteStatus { get; set; }
    public string RemoteStatusLabel { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public bool IsResolved { get; set; }
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
