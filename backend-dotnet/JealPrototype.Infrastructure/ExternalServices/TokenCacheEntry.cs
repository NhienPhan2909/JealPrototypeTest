namespace JealPrototype.Infrastructure.ExternalServices;

/// <summary>
/// Token cache entry for storing EasyCars JWT tokens
/// </summary>
public class TokenCacheEntry
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime AcquiredAt { get; set; }

    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiresAt;
    }
}
