namespace JealPrototype.Domain.Entities;

public class DealershipSettings
{
    public int Id { get; private set; }
    public int DealershipId { get; private set; }
    public bool EasyCarAutoSyncEnabled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    public Dealership Dealership { get; private set; } = null!;
    
    private DealershipSettings() { }
    
    public static DealershipSettings Create(int dealershipId, bool autoSyncEnabled = true)
    {
        return new DealershipSettings
        {
            DealershipId = dealershipId,
            EasyCarAutoSyncEnabled = autoSyncEnabled,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    public void SetAutoSyncEnabled(bool enabled)
    {
        EasyCarAutoSyncEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;
    }
}
