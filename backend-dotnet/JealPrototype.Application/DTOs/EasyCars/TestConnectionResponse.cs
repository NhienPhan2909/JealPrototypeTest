namespace JealPrototype.Application.DTOs.EasyCars;

/// <summary>
/// Response DTO for test connection operation
/// </summary>
public class TestConnectionResponse
{
    /// <summary>
    /// Indicates if connection test was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// User-friendly message describing the result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Environment that was tested (Test or Production)
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Error code if connection failed (optional)
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Additional technical details (optional)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Token expiration time if successful (optional)
    /// </summary>
    public DateTime? TokenExpiresAt { get; set; }

    /// <summary>
    /// Creates a success response
    /// </summary>
    public static TestConnectionResponse CreateSuccess(string environment, DateTime? expiresAt = null)
    {
        return new TestConnectionResponse
        {
            Success = true,
            Message = "Connection successful! Your credentials are valid.",
            Environment = environment,
            TokenExpiresAt = expiresAt
        };
    }

    /// <summary>
    /// Creates an authentication failure response
    /// </summary>
    public static TestConnectionResponse CreateAuthFailure(string environment, string? errorCode = null)
    {
        return new TestConnectionResponse
        {
            Success = false,
            Message = "Authentication failed. Please verify your Account Number and Account Secret are correct.",
            Environment = environment,
            ErrorCode = errorCode ?? "AUTH_FAILED",
            Details = "Ensure you're using the correct credentials for the selected environment (Test or Production)."
        };
    }

    /// <summary>
    /// Creates a timeout response
    /// </summary>
    public static TestConnectionResponse CreateTimeout(string environment)
    {
        return new TestConnectionResponse
        {
            Success = false,
            Message = "Connection timed out after 10 seconds. Please check your network connection and try again.",
            Environment = environment,
            ErrorCode = "TIMEOUT"
        };
    }

    /// <summary>
    /// Creates a network error response
    /// </summary>
    public static TestConnectionResponse CreateNetworkError(string environment, string errorMessage)
    {
        return new TestConnectionResponse
        {
            Success = false,
            Message = "Unable to reach EasyCars API. Please verify you selected the correct environment.",
            Environment = environment,
            ErrorCode = "NETWORK_ERROR",
            Details = errorMessage
        };
    }

    /// <summary>
    /// Creates a generic error response
    /// </summary>
    public static TestConnectionResponse CreateError(string environment, string message, string? errorCode = null)
    {
        return new TestConnectionResponse
        {
            Success = false,
            Message = message,
            Environment = environment,
            ErrorCode = errorCode ?? "ERROR"
        };
    }
}
