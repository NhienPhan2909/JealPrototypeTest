namespace JealPrototype.Domain.Enums;

public enum ConflictResolutionStrategy
{
    LocalWins,     // Local status is authoritative; ignore remote differences
    RemoteWins,    // Remote (EasyCars) status is authoritative; update local
    ManualReview   // Create a LeadStatusConflict record for human resolution
}
