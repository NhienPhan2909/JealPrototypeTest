namespace JealPrototype.Domain.Entities;

public class SystemSettings
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    private SystemSettings() { }
    
    public static SystemSettings Create(string key, string value, string? description = null)
    {
        return new SystemSettings
        {
            Key = key,
            Value = value,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    
    public void UpdateValue(string value)
    {
        Value = value;
        UpdatedAt = DateTime.UtcNow;
    }
}
