using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace JealPrototype.Application.UseCases.EasyCars;

/// <summary>
/// Use case for testing EasyCars credential connection
/// </summary>
public class TestConnectionUseCase
{
    private readonly IEasyCarsApiClient _easyCarsApiClient;
    private readonly ILogger<TestConnectionUseCase> _logger;

    public TestConnectionUseCase(
        IEasyCarsApiClient easyCarsApiClient,
        ILogger<TestConnectionUseCase> logger)
    {
        _easyCarsApiClient = easyCarsApiClient;
        _logger = logger;
    }

    /// <summary>
    /// Tests connection to EasyCars API with provided credentials
    /// </summary>
    /// <param name="request">Test connection request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Test connection response</returns>
    public async Task<TestConnectionResponse> ExecuteAsync(
        TestConnectionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Testing EasyCars connection. Environment: {Environment}",
            request.Environment);

        try
        {
            var tokenResponse = await _easyCarsApiClient.RequestTokenAsync(
                request.AccountNumber,
                request.AccountSecret,
                request.Environment,
                cancellationToken);

            if (tokenResponse.IsSuccess)
            {
                _logger.LogInformation(
                    "EasyCars connection test succeeded. Environment: {Environment}",
                    request.Environment);

                return TestConnectionResponse.CreateSuccess(
                    request.Environment,
                    tokenResponse.ExpiresAt);
            }

            if (tokenResponse.ErrorCode == "TIMEOUT")
            {
                _logger.LogWarning(
                    "EasyCars connection test timed out. Environment: {Environment}",
                    request.Environment);

                return TestConnectionResponse.CreateTimeout(request.Environment);
            }

            if (tokenResponse.ErrorCode == "NETWORK_ERROR")
            {
                _logger.LogWarning(
                    "EasyCars connection test failed with network error. Environment: {Environment}, Error: {Error}",
                    request.Environment, tokenResponse.ErrorMessage);

                return TestConnectionResponse.CreateNetworkError(
                    request.Environment,
                    tokenResponse.ErrorMessage ?? "Unknown network error");
            }

            _logger.LogWarning(
                "EasyCars connection test failed. Environment: {Environment}, ErrorCode: {ErrorCode}",
                request.Environment, tokenResponse.ErrorCode);

            return TestConnectionResponse.CreateAuthFailure(
                request.Environment,
                tokenResponse.ErrorCode);
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Test connection operation was cancelled");
            throw;
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Test connection timed out");
            return TestConnectionResponse.CreateTimeout(request.Environment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during test connection");
            return TestConnectionResponse.CreateError(
                request.Environment,
                "An unexpected error occurred. Please try again.",
                "UNEXPECTED_ERROR");
        }
    }
}
