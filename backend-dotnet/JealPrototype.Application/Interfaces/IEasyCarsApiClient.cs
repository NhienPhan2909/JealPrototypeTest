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
    /// <param name="clientId">EasyCars ClientID for token auth</param>
    /// <param name="clientSecret">EasyCars ClientSecret for token auth</param>
    /// <param name="environment">Test or Production</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Token response from EasyCars</returns>
    Task<EasyCarsTokenResponse> RequestTokenAsync(
        string clientId, 
        string clientSecret, 
        string environment,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates credentials by attempting to obtain a token
    /// </summary>
    /// <param name="clientId">EasyCars ClientID for token auth</param>
    /// <param name="clientSecret">EasyCars ClientSecret for token auth</param>
    /// <param name="environment">Test or Production</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if credentials are valid</returns>
    Task<bool> ValidateCredentialsAsync(
        string clientId, 
        string clientSecret, 
        string environment,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a valid JWT token from cache or acquires a new one.
    /// Automatically refreshes expired tokens.
    /// </summary>
    /// <param name="clientId">EasyCars ClientID for token auth</param>
    /// <param name="clientSecret">EasyCars ClientSecret for token auth</param>
    /// <param name="environment">Test or Production</param>
    /// <param name="dealershipId">Dealership ID for cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Valid JWT token</returns>
    Task<string> GetOrRefreshTokenAsync(
        string clientId,
        string clientSecret,
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
    /// <param name="clientId">EasyCars ClientID for token auth</param>
    /// <param name="clientSecret">EasyCars ClientSecret for token auth</param>
    /// <param name="environment">Test or Production</param>
    /// <param name="dealershipId">Dealership ID</param>
    /// <param name="requestBody">Optional request body for POST/PUT</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Typed response from EasyCars API</returns>
    Task<T> ExecuteAuthenticatedRequestAsync<T>(
        string endpoint,
        HttpMethod method,
        string clientId,
        string clientSecret,
        string environment,
        int dealershipId,
        object? requestBody = null,
        CancellationToken cancellationToken = default) where T : EasyCarsBaseResponse;

    /// <summary>
    /// Retrieves advertisement stocks from EasyCars API
    /// </summary>
    /// <param name="clientId">EasyCars ClientID for token auth</param>
    /// <param name="clientSecret">EasyCars ClientSecret for token auth</param>
    /// <param name="accountNumber">EasyCars AccountNumber for stock request body</param>
    /// <param name="accountSecret">EasyCars AccountSecret for stock request body</param>
    /// <param name="environment">Test or Production</param>
    /// <param name="dealershipId">Dealership ID</param>
    /// <param name="yardCode">Optional yard code to filter stocks</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of stock items (empty if no results)</returns>
    Task<List<StockItem>> GetAdvertisementStocksAsync(
        string clientId,
        string clientSecret,
        string accountNumber,
        string accountSecret,
        string environment,
        int dealershipId,
        string? yardCode = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new lead in EasyCars API
    /// </summary>
    Task<CreateLeadResponse> CreateLeadAsync(
        string clientId,
        string clientSecret,
        string environment,
        int dealershipId,
        CreateLeadRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing lead in EasyCars API
    /// </summary>
    Task<UpdateLeadResponse> UpdateLeadAsync(
        string clientId,
        string clientSecret,
        string environment,
        int dealershipId,
        UpdateLeadRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves lead details from EasyCars API
    /// </summary>
    Task<LeadDetailResponse> GetLeadDetailAsync(
        string clientId,
        string clientSecret,
        string accountNumber,
        string accountSecret,
        string environment,
        int dealershipId,
        string leadNumber,
        CancellationToken cancellationToken = default);
}
