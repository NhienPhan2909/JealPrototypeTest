namespace JealPrototype.Infrastructure.Configuration;

/// <summary>
/// Configuration for EasyCars API client
/// </summary>
public class EasyCarsConfiguration
{
    public const string SectionName = "EasyCars";

    /// <summary>
    /// Test environment API base URL
    /// </summary>
    public string TestApiUrl { get; set; } = "https://test.easycars.com/api";

    /// <summary>
    /// Production environment API base URL
    /// </summary>
    public string ProductionApiUrl { get; set; } = "https://api.easycars.com/api";

    /// <summary>
    /// HTTP request timeout in seconds (default: 30)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Number of retry attempts for transient errors (default: 3)
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay in milliseconds for exponential backoff (default: 1000)
    /// Delays will be: 2s, 4s, 8s
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 1000;

    /// <summary>
    /// Token cache duration in seconds (default: 570 = 9m 30s)
    /// EasyCars tokens expire after 10 minutes, we cache for 9m 30s
    /// </summary>
    public int TokenCacheDurationSeconds { get; set; } = 570;

    /// <summary>
    /// Validates the configuration settings
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(TestApiUrl))
            throw new InvalidOperationException("EasyCars TestApiUrl is not configured");

        if (string.IsNullOrWhiteSpace(ProductionApiUrl))
            throw new InvalidOperationException("EasyCars ProductionApiUrl is not configured");

        if (!Uri.TryCreate(TestApiUrl, UriKind.Absolute, out _))
            throw new InvalidOperationException($"EasyCars TestApiUrl is not a valid URL: {TestApiUrl}");

        if (!Uri.TryCreate(ProductionApiUrl, UriKind.Absolute, out _))
            throw new InvalidOperationException($"EasyCars ProductionApiUrl is not a valid URL: {ProductionApiUrl}");

        if (TimeoutSeconds <= 0)
            throw new InvalidOperationException("EasyCars TimeoutSeconds must be greater than 0");

        if (RetryAttempts < 0)
            throw new InvalidOperationException("EasyCars RetryAttempts must be >= 0");

        if (RetryDelayMilliseconds < 0)
            throw new InvalidOperationException("EasyCars RetryDelayMilliseconds must be >= 0");

        if (TokenCacheDurationSeconds <= 0)
            throw new InvalidOperationException("EasyCars TokenCacheDurationSeconds must be greater than 0");
    }
}
