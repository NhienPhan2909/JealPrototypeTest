namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Metadata response for EasyCars credentials (for GET endpoint)
/// </summary>
public class CredentialMetadataResponse
{
    /// <summary>
    /// Credential ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Dealership ID
    /// </summary>
    public int DealershipId { get; set; }

    /// <summary>
    /// Environment: Test or Production
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Whether the credential is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Optional yard code
    /// </summary>
    public string? YardCode { get; set; }

    /// <summary>
    /// Indicates if credentials are configured
    /// </summary>
    public bool HasCredentials { get; set; }

    /// <summary>
    /// When credentials were configured
    /// </summary>
    public DateTime? ConfiguredAt { get; set; }

    /// <summary>
    /// When the last sync occurred
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }
}
