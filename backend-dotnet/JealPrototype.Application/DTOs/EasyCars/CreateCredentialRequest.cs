using System.ComponentModel.DataAnnotations;

namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Request DTO for creating EasyCars credentials
/// </summary>
public class CreateCredentialRequest
{
    /// <summary>
    /// EasyCars Client ID - used for token authentication
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// EasyCars Client Secret - used for token authentication
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// EasyCars Account Number (PublicID) - GUID format
    /// </summary>
    [Required]
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// EasyCars Account Secret (SecretKey) - GUID format
    /// </summary>
    [Required]
    public string AccountSecret { get; set; } = string.Empty;

    /// <summary>
    /// Environment: Test or Production
    /// </summary>
    [Required]
    [RegularExpression("^(Test|Production)$", ErrorMessage = "Environment must be 'Test' or 'Production'")]
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Optional yard code for multi-location dealerships
    /// </summary>
    [MaxLength(50)]
    public string? YardCode { get; set; }
}
