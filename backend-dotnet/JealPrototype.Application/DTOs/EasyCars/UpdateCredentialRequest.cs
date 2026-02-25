using System.ComponentModel.DataAnnotations;

namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Request DTO for updating EasyCars credentials
/// </summary>
public class UpdateCredentialRequest
{
    /// <summary>
    /// EasyCars Account Number (PublicID) - GUID format (optional for updates)
    /// </summary>
    public string? AccountNumber { get; set; }

    /// <summary>
    /// EasyCars Account Secret (SecretKey) - GUID format (optional for updates)
    /// </summary>
    public string? AccountSecret { get; set; }

    /// <summary>
    /// Environment: Test or Production (optional for updates)
    /// </summary>
    [RegularExpression("^(Test|Production)$", ErrorMessage = "Environment must be 'Test' or 'Production'")]
    public string? Environment { get; set; }

    /// <summary>
    /// Optional yard code for multi-location dealerships
    /// </summary>
    [MaxLength(50)]
    public string? YardCode { get; set; }

    /// <summary>
    /// Whether the credential is active
    /// </summary>
    public bool? IsActive { get; set; }
}
