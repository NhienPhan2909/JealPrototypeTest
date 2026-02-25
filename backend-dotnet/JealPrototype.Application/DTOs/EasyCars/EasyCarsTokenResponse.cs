namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Response from EasyCars RequestToken API
/// </summary>
public class EasyCarsTokenResponse : EasyCarsBaseResponse
{
    /// <summary>
    /// JWT token if authentication successful
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Token expiration timestamp
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Success message
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Error message if authentication failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Error code if authentication failed
    /// </summary>
    public string? ErrorCode { get; set; }
}
