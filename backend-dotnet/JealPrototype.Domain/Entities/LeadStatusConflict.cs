namespace JealPrototype.Domain.Entities;

public class LeadStatusConflict : BaseEntity
{
    public int DealershipId { get; private set; }
    public int LeadId { get; private set; }
    public string EasyCarsLeadNumber { get; private set; } = null!;
    public string LocalStatus { get; private set; } = null!;    // e.g. "InProgress"
    public int RemoteStatus { get; private set; }               // EasyCars int, e.g. 50
    public DateTime DetectedAt { get; private set; }
    public bool IsResolved { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public string? ResolvedBy { get; private set; }
    public string? Resolution { get; private set; }             // "local" or "remote"

    private LeadStatusConflict() { }

    public static LeadStatusConflict Create(
        int dealershipId,
        int leadId,
        string easyCarsLeadNumber,
        string localStatus,
        int remoteStatus)
    {
        if (dealershipId <= 0) throw new ArgumentException("Invalid dealership ID");
        if (leadId <= 0) throw new ArgumentException("Invalid lead ID");
        if (string.IsNullOrWhiteSpace(easyCarsLeadNumber)) throw new ArgumentException("EasyCars lead number required");

        return new LeadStatusConflict
        {
            DealershipId = dealershipId,
            LeadId = leadId,
            EasyCarsLeadNumber = easyCarsLeadNumber,
            LocalStatus = localStatus,
            RemoteStatus = remoteStatus,
            DetectedAt = DateTime.UtcNow,
            IsResolved = false
        };
    }

    public void Resolve(string resolution, string resolvedBy)
    {
        if (IsResolved) throw new InvalidOperationException("Conflict already resolved");
        if (resolution != "local" && resolution != "remote")
            throw new ArgumentException("Resolution must be 'local' or 'remote'");

        IsResolved = true;
        Resolution = resolution;
        ResolvedBy = resolvedBy;
        ResolvedAt = DateTime.UtcNow;
    }
}
