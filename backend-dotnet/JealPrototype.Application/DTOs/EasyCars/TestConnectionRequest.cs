using System.ComponentModel.DataAnnotations;

namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Request DTO for testing EasyCars API connection
/// </summary>
public class TestConnectionRequest
{
    /// <summary>
    /// EasyCars Client ID - used for token authentication
    /// </summary>
    [Required(ErrorMessage = "Client ID is required")]
    [MaxLength(200, ErrorMessage = "Client ID cannot exceed 200 characters")]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// EasyCars Client Secret - used for token authentication
    /// </summary>
    [Required(ErrorMessage = "Client Secret is required")]
    [MaxLength(200, ErrorMessage = "Client Secret cannot exceed 200 characters")]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// EasyCars Account Number (PublicID)
    /// </summary>
    [Required(ErrorMessage = "Account Number is required")]
    [MaxLength(100, ErrorMessage = "Account Number cannot exceed 100 characters")]
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// EasyCars Account Secret (SecretKey)
    /// </summary>
    [Required(ErrorMessage = "Account Secret is required")]
    [MaxLength(100, ErrorMessage = "Account Secret cannot exceed 100 characters")]
    public string AccountSecret { get; set; } = string.Empty;

    /// <summary>
    /// Environment to test: "Test" or "Production"
    /// </summary>
    [Required(ErrorMessage = "Environment is required")]
    [RegularExpression("^(Test|Production)$", 
        ErrorMessage = "Environment must be 'Test' or 'Production'")]
    public string Environment { get; set; } = string.Empty;
}
