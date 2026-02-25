namespace JealPrototype.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for encryption service.
/// </summary>
public class EncryptionSettings
{
    public const string SectionName = "EncryptionSettings";

    /// <summary>
    /// Base64-encoded 256-bit (32 bytes) encryption key.
    /// Should be loaded from environment variable: EASYCARS_ENCRYPTION_KEY
    /// </summary>
    public string EncryptionKey { get; set; } = string.Empty;

    /// <summary>
    /// Optional key version for key rotation support.
    /// </summary>
    public int KeyVersion { get; set; } = 1;
}
