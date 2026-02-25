using JealPrototype.Application.DTOs.EasyCars;

namespace JealPrototype.Application.Interfaces;

/// <summary>
/// Interface for EasyCars API client with token management and authenticated requests
/// </summary>
public interface IEasyCarsApiClient
{
    /// <summary>
    /// Requests an authentication token from EasyCars API
    /// </summary>
    /// <param name="accountNumber">EasyCars PublicID</param>
    /// <param name="accountSecret">EasyCars SecretKey</param>
    /// <param name="environment">Test or Production</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Token response from EasyCars</returns>
    Task<EasyCarsTokenResponse> RequestTokenAsync(
        string accountNumber, 
        string accountSecret, 
        string environment,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates credentials by attempting to obtain a token
    /// </summary>
    /// <param name="accountNumber">EasyCars PublicID</param>
    /// <param name="accountSecret">EasyCars SecretKey</param>
    /// <param name="environment">Test or Production</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if credentials are valid</returns>
    Task<bool> ValidateCredentialsAsync(
        string accountNumber, 
        string accountSecret, 
        string environment,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a valid JWT token from cache or acquires a new one.
    /// Automatically refreshes expired tokens.
    /// </summary>
    /// <param name="accountNumber">EasyCars PublicID</param>
    /// <param name="accountSecret">EasyCars SecretKey</param>
    /// <param name="environment">Test or Production</param>
    /// <param name="dealershipId">Dealership ID for cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Valid JWT token</returns>
    Task<string> GetOrRefreshTokenAsync(
        string accountNumber,
        string accountSecret,
        string environment,
        int dealershipId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an authenticated request to the EasyCars API.
    /// Automatically handles token acquisition, refresh, and retry logic.
    /// </summary>
    /// <typeparam name="T">Response type (must inherit from EasyCarsBaseResponse)</typeparam>
    /// <param name="endpoint">API endpoint path (e.g., "/Vehicle")</param>
    /// <param name="method">HTTP method (GET, POST, PUT, DELETE)</param>
    /// <param name="accountNumber">EasyCars PublicID</param>
    /// <param name="accountSecret">EasyCars SecretKey</param>
    /// <param name="environment">Test or Production</param>
    /// <param name="dealershipId">Dealership ID</param>
    /// <param name="requestBody">Optional request body for POST/PUT</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Typed response from EasyCars API</returns>
    Task<T> ExecuteAuthenticatedRequestAsync<T>(
        string endpoint,
        HttpMethod method,
        string accountNumber,
        string accountSecret,
        string environment,
        int dealershipId,
        object? requestBody = null,
        CancellationToken cancellationToken = default) where T : EasyCarsBaseResponse;

    /// <summary>
    /// Retrieves advertisement stocks from EasyCars API
    /// </summary>
    /// <param name="accountNumber">EasyCars PublicID</param>
    /// <param name="accountSecret">EasyCars SecretKey</param>
    /// <param name="environment">Test or Production</param>
    /// <param name="dealershipId">Dealership ID</param>
    /// <param name="yardCode">Optional yard code to filter stocks</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of stock items (empty if no results)</returns>
    Task<List<StockItem>> GetAdvertisementStocksAsync(
        string accountNumber,
        string accountSecret,
        string environment,
        int dealershipId,
        string? yardCode = null,
        CancellationToken cancellationToken = default);
}
