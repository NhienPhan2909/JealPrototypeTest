using System.ComponentModel.DataAnnotations;

namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Request DTO for testing EasyCars API connection
/// </summary>
public class TestConnectionRequest
{
    /// <summary>
    /// EasyCars Account Number (PublicID) - GUID format
    /// </summary>
    [Required(ErrorMessage = "Account Number is required")]
    [RegularExpression(@"^[0-9A-Fa-f]{8}-([0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}$", 
        ErrorMessage = "Account Number must be a valid GUID format")]
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// EasyCars Account Secret (SecretKey) - GUID format
    /// </summary>
    [Required(ErrorMessage = "Account Secret is required")]
    [RegularExpression(@"^[0-9A-Fa-f]{8}-([0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}$", 
        ErrorMessage = "Account Secret must be a valid GUID format")]
    public string AccountSecret { get; set; } = string.Empty;

    /// <summary>
    /// Environment to test: "Test" or "Production"
    /// </summary>
    [Required(ErrorMessage = "Environment is required")]
    [RegularExpression("^(Test|Production)$", 
        ErrorMessage = "Environment must be 'Test' or 'Production'")]
    public string Environment { get; set; } = string.Empty;
}
