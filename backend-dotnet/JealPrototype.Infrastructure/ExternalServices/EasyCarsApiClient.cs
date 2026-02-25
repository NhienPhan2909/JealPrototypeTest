using System.Net;
using System.Text;
using System.Text.Json;
using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Exceptions;
using JealPrototype.Application.Interfaces;
using JealPrototype.Infrastructure.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace JealPrototype.Infrastructure.ExternalServices;

/// <summary>
/// Production-grade HTTP client for EasyCars API integration with token management, retry logic, and comprehensive error handling
/// </summary>
public class EasyCarsApiClient : IEasyCarsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly EasyCarsConfiguration _config;
    private readonly ILogger<EasyCarsApiClient> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);

    public EasyCarsApiClient(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IOptions<EasyCarsConfiguration> config,
        ILogger<EasyCarsApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _config = config.Value;
        _logger = logger;

        // Validate configuration on startup
        _config.Validate();

        // Configure retry policy with exponential backoff
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => 
                !r.IsSuccessStatusCode && 
                (r.StatusCode == HttpStatusCode.ServiceUnavailable ||
                 r.StatusCode == HttpStatusCode.BadGateway ||
                 r.StatusCode == HttpStatusCode.GatewayTimeout ||
                 (int)r.StatusCode == 500))
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: _config.RetryAttempts,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromMilliseconds(_config.RetryDelayMilliseconds * Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Retry {RetryCount}/{MaxRetries} after {Delay}ms. Reason: {Reason}",
                        retryCount, _config.RetryAttempts, timeSpan.TotalMilliseconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString() ?? "Unknown");
                });
    }

    /// <inheritdoc />
    public async Task<string> GetOrRefreshTokenAsync(
        string accountNumber,
        string accountSecret,
        string environment,
        int dealershipId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetTokenCacheKey(dealershipId, environment, accountNumber);

        // Thread-safe token acquisition
        await _tokenSemaphore.WaitAsync(cancellationToken);
        try
        {
            // Check cache first
            if (_cache.TryGetValue<TokenCacheEntry>(cacheKey, out var cachedEntry) && 
                cachedEntry != null && 
                !cachedEntry.IsExpired())
            {
                _logger.LogDebug(
                    "Token cache hit for dealership {DealershipId} ({Environment})",
                    dealershipId, environment);
                return cachedEntry.Token;
            }

            // Cache miss or expired - acquire new token
            _logger.LogInformation(
                "Acquiring new token for dealership {DealershipId} ({Environment})",
                dealershipId, environment);

            var tokenResponse = await RequestTokenAsync(
                accountNumber, accountSecret, environment, cancellationToken);

            if (!tokenResponse.IsSuccess || string.IsNullOrEmpty(tokenResponse.Token))
            {
                throw new EasyCarsAuthenticationException(
                    tokenResponse.ErrorMessage ?? "Failed to acquire token");
            }

            // Cache the token
            var entry = new TokenCacheEntry
            {
                Token = tokenResponse.Token,
                AcquiredAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddSeconds(_config.TokenCacheDurationSeconds)
            };

            _cache.Set(cacheKey, entry, TimeSpan.FromSeconds(_config.TokenCacheDurationSeconds));

            _logger.LogInformation(
                "Token acquired for dealership {DealershipId} ({Environment}), expires in {Duration}m",
                dealershipId, environment, _config.TokenCacheDurationSeconds / 60);

            return entry.Token;
        }
        finally
        {
            _tokenSemaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<T> ExecuteAuthenticatedRequestAsync<T>(
        string endpoint,
        HttpMethod method,
        string accountNumber,
        string accountSecret,
        string environment,
        int dealershipId,
        object? requestBody = null,
        CancellationToken cancellationToken = default) where T : EasyCarsBaseResponse
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // Get or refresh token
            var token = await GetOrRefreshTokenAsync(
                accountNumber, accountSecret, environment, dealershipId, cancellationToken);

            var baseUrl = GetBaseUrl(environment);
            var requestUrl = $"{baseUrl}{endpoint}";

            _logger.LogDebug(
                "Sending {Method} request to {Endpoint}",
                method.Method, endpoint);

            var httpClient = _httpClientFactory.CreateClient("EasyCarsApi");

            // Build request
            var request = new HttpRequestMessage(method, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {token}");

            if (requestBody != null && (method == HttpMethod.Post || method == HttpMethod.Put))
            {
                var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogDebug(
                    "Request body: {Body}",
                    SanitizeForLog(jsonContent));
            }

            // Execute with retry policy
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var httpResponse = await httpClient.SendAsync(request, cancellationToken);

                // Handle 401 specially - refresh token and retry once
                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning(
                        "Received 401 Unauthorized, clearing token cache and refreshing");

                    var cacheKey = GetTokenCacheKey(dealershipId, environment, accountNumber);
                    _cache.Remove(cacheKey);

                    // Get new token
                    token = await GetOrRefreshTokenAsync(
                        accountNumber, accountSecret, environment, dealershipId, cancellationToken);

                    // Retry request with new token
                    request.Headers.Remove("Authorization");
                    request.Headers.Add("Authorization", $"Bearer {token}");

                    httpResponse = await httpClient.SendAsync(request, cancellationToken);
                }

                return httpResponse;
            });

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Parse response
            var typedResponse = JsonSerializer.Deserialize<T>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (typedResponse == null)
            {
                _logger.LogError(
                    "Failed to deserialize response from {Endpoint}. Content: {Content}",
                    endpoint, responseContent);
                throw new EasyCarsUnknownException("Invalid response format from EasyCars API", -1);
            }

            // Handle response codes
            HandleResponseCode(typedResponse.ResponseCode, typedResponse.ResponseMessage);

            _logger.LogInformation(
                "Request to {Endpoint} completed successfully in {ElapsedMs}ms",
                endpoint, elapsed);

            return typedResponse;
        }
        catch (EasyCarsTemporaryException)
        {
            // Let retry policy handle temporary exceptions
            throw;
        }
        catch (EasyCarsException)
        {
            // Re-throw EasyCars-specific exceptions
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "Network error calling {Endpoint}",
                endpoint);
            throw new EasyCarsException(
                $"Unable to connect to EasyCars API. Please check your internet connection and try again. Technical details: {ex.Message}",
                ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex,
                "Request to {Endpoint} timed out after {Timeout}s",
                endpoint, _config.TimeoutSeconds);
            throw new EasyCarsException(
                $"EasyCars API request timed out after {_config.TimeoutSeconds} seconds. The service may be experiencing high load.",
                ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex,
                "Failed to parse response from {Endpoint}",
                endpoint);
            throw new EasyCarsException(
                "Invalid response format from EasyCars API",
                ex);
        }
    }

    /// <inheritdoc />
    public async Task<EasyCarsTokenResponse> RequestTokenAsync(
        string accountNumber,
        string accountSecret,
        string environment,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = GetBaseUrl(environment);
            var requestUrl = $"{baseUrl}/RequestToken";

            _logger.LogInformation(
                "Requesting token from EasyCars API. Environment: {Environment}",
                environment);

            var httpClient = _httpClientFactory.CreateClient("EasyCarsApi");

            var requestBody = new
            {
                PublicID = accountNumber,
                SecretKey = accountSecret
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(requestUrl, httpContent, cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized || responseContent.Contains("\"ResponseCode\":1"))
            {
                _logger.LogWarning(
                    "EasyCars authentication failed. Environment: {Environment}, Status: {StatusCode}",
                    environment, response.StatusCode);

                return new EasyCarsTokenResponse
                {
                    ResponseCode = 1,
                    ResponseMessage = "Authentication failed",
                    ErrorMessage = "Authentication failed. Invalid credentials.",
                    ErrorCode = "AUTH_FAILED"
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "EasyCars API returned error. Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, responseContent);

                return new EasyCarsTokenResponse
                {
                    ResponseCode = 1,
                    ResponseMessage = $"API error: {response.StatusCode}",
                    ErrorMessage = $"API returned status code {response.StatusCode}",
                    ErrorCode = response.StatusCode.ToString()
                };
            }

            var tokenResponse = JsonSerializer.Deserialize<EasyCarsTokenResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (tokenResponse == null)
            {
                _logger.LogError("Failed to deserialize EasyCars token response");
                return new EasyCarsTokenResponse
                {
                    ResponseCode = 1,
                    ResponseMessage = "Invalid response",
                    ErrorMessage = "Invalid response format from EasyCars API",
                    ErrorCode = "INVALID_RESPONSE"
                };
            }

            if (tokenResponse.IsSuccess)
            {
                _logger.LogInformation(
                    "Successfully obtained token from EasyCars. Environment: {Environment}",
                    environment);
            }
            else
            {
                _logger.LogWarning(
                    "EasyCars returned failure response. ResponseCode: {ResponseCode}, Error: {Error}",
                    tokenResponse.ResponseCode, tokenResponse.ErrorMessage);
            }

            return tokenResponse;
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            _logger.LogWarning("EasyCars API request was cancelled");
            throw;
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("EasyCars API request timed out");
            return new EasyCarsTokenResponse
            {
                ResponseCode = 1,
                ResponseMessage = "Timeout",
                ErrorMessage = "Request timed out",
                ErrorCode = "TIMEOUT"
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while calling EasyCars API. Environment: {Environment}", environment);
            return new EasyCarsTokenResponse
            {
                ResponseCode = 1,
                ResponseMessage = "Network error",
                ErrorMessage = $"Network error: {ex.Message}",
                ErrorCode = "NETWORK_ERROR"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while calling EasyCars API");
            return new EasyCarsTokenResponse
            {
                ResponseCode = 1,
                ResponseMessage = "Unexpected error",
                ErrorMessage = $"Unexpected error: {ex.Message}",
                ErrorCode = "UNEXPECTED_ERROR"
            };
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidateCredentialsAsync(
        string accountNumber,
        string accountSecret,
        string environment,
        CancellationToken cancellationToken = default)
    {
        var response = await RequestTokenAsync(accountNumber, accountSecret, environment, cancellationToken);
        return response.IsSuccess;
    }

    private string GetBaseUrl(string environment)
    {
        return environment.Equals("Production", StringComparison.OrdinalIgnoreCase)
            ? _config.ProductionApiUrl
            : _config.TestApiUrl;
    }

    private static string GetTokenCacheKey(int dealershipId, string environment, string accountNumber)
    {
        return $"easycars_token:{dealershipId}:{environment}:{accountNumber}";
    }

    private void HandleResponseCode(int responseCode, string message)
    {
        switch (responseCode)
        {
            case 0:
                // Success
                return;
            case 1:
                _logger.LogError("EasyCars authentication failed: {Message}", message);
                throw new EasyCarsAuthenticationException(message);
            case 5:
                _logger.LogWarning("EasyCars temporary error: {Message}", message);
                throw new EasyCarsTemporaryException(message);
            case 7:
                _logger.LogError("EasyCars validation error: {Message}", message);
                throw new EasyCarsValidationException(message);
            case 9:
                _logger.LogError("EasyCars fatal error: {Message}", message);
                throw new EasyCarsFatalException(message);
            default:
                _logger.LogError("EasyCars unknown response code {Code}: {Message}", responseCode, message);
                throw new EasyCarsUnknownException($"Unexpected response code {responseCode}: {message}", responseCode);
        }
    }

    private static string SanitizeForLog(string content)
    {
        // Remove sensitive fields for logging
        var sanitized = content
            .Replace("\"publicID\"", "\"publicID\":[REDACTED]")
            .Replace("\"PublicID\"", "\"PublicID\":[REDACTED]")
            .Replace("\"secretKey\"", "\"secretKey\":[REDACTED]")
            .Replace("\"SecretKey\"", "\"SecretKey\":[REDACTED]")
            .Replace("\"token\"", "\"token\":[REDACTED]")
            .Replace("\"Token\"", "\"Token\":[REDACTED]");
        
        return sanitized.Length > 500 ? sanitized[..500] + "..." : sanitized;
    }

    /// <summary>
    /// Retrieves advertisement stocks from EasyCars API
    /// </summary>
    public async Task<List<StockItem>> GetAdvertisementStocksAsync(
        string accountNumber,
        string accountSecret,
        string environment,
        int dealershipId,
        string? yardCode = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Fetching advertisement stocks for dealership {DealershipId}, environment: {Environment}, yardCode: {YardCode}",
            dealershipId, environment, yardCode ?? "all");

        try
        {
            // Build endpoint with optional yardCode parameter
            var endpoint = "/Stock/GetAdvertisementStocks";
            if (!string.IsNullOrEmpty(yardCode))
            {
                endpoint += $"?yardCode={Uri.EscapeDataString(yardCode)}";
            }

            // Execute authenticated GET request
            var response = await ExecuteAuthenticatedRequestAsync<StockResponse>(
                endpoint,
                HttpMethod.Get,
                accountNumber,
                accountSecret,
                environment,
                dealershipId,
                requestBody: null,
                cancellationToken);

            // Return stocks or empty list if null
            var stocks = response.Stocks ?? new List<StockItem>();

            _logger.LogInformation(
                "Successfully retrieved {StockCount} advertisement stocks for dealership {DealershipId}",
                stocks.Count, dealershipId);

            return stocks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to retrieve advertisement stocks for dealership {DealershipId}, environment: {Environment}",
                dealershipId, environment);
            throw;
        }
    }
}
