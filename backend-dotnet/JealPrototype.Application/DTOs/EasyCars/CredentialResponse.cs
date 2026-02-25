namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Response DTO for EasyCars credentials (metadata only, no secrets)
/// </summary>
public class CredentialResponse
{
    /// <summary>
    /// Credential ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Dealership ID that owns this credential
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
    /// When the credential was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the credential was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// When the last sync occurred (if any)
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }
}
