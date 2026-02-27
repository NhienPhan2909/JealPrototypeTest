namespace JealPrototype.Domain.Enums;

public enum LeadStatus
{
    Received,
    InProgress,
    Done,      // Legacy — maps to Won (50) on outbound; keep for backward compat
    Won,
    Lost,
    Deleted
}
