using JealPrototype.Application.DTOs.EasyCars;
using JealPrototype.Infrastructure.Configuration;
using JealPrototype.Infrastructure.ExternalServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace JealPrototype.Tests.Integration;

/// <summary>
/// Integration tests for Story 3.1: Lead API operations using EasyCars test credentials.
/// These tests make real HTTP calls to the EasyCars Test environment.
/// Run with: dotnet test --filter "Category=Integration"
/// </summary>
[Trait("Category", "Integration")]
public class EasyCarsLeadApiIntegrationTests
{
    // EasyCars test credentials
    private const string ClientId = "AA20EE61-5CFA-458D-9AFB-C4E929EA18E6";
    private const string ClientSecret = "7326AF23-714A-41A5-A74F-EC77B4E4F2F2";
    private const string AccountNumber = "AA20EE61-5CFA-458D-9AFB-C4E929EA18E6";
    private const string AccountSecret = "7326AF23-714A-41A5-A74F-EC77B4E4F2F2";
    private const string Environment = "Test";
    private const int DealershipId = 1;

    private readonly EasyCarsApiClient _client;

    public EasyCarsLeadApiIntegrationTests()
    {
        var config = new EasyCarsConfiguration
        {
            TestApiUrl = "https://testmy.easycars.com.au/TestECService",
            ProductionApiUrl = "https://my.easycars.net.au/ECService",
            TimeoutSeconds = 30,
            RetryAttempts = 3,
            RetryDelayMilliseconds = 1000,
            TokenCacheDurationSeconds = 570
        };

        var httpClientFactory = new DefaultHttpClientFactory();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<EasyCarsApiClient>();

        _client = new EasyCarsApiClient(
            httpClientFactory,
            cache,
            Options.Create(config),
            logger);
    }

    [Fact]
    public async Task CreateUpdateGetLead_WithTestCredentials_CompletesFullCycle()
    {
        // Arrange
        var createRequest = new CreateLeadRequest
        {
            AccountNumber = AccountNumber,
            AccountSecret = AccountSecret,
            CustomerName = "Integration Test Customer",
            CustomerEmail = "integration.test@example.com",
            CustomerPhone = "02-9999-9999",
            VehicleMake = "Toyota",
            VehicleModel = "Camry",
            VehicleYear = 2023,
            Comments = "Integration test lead - created by automated test"
        };

        // Act - Create
        var createResult = await _client.CreateLeadAsync(
            ClientId, ClientSecret, Environment, DealershipId, createRequest);

        Assert.NotNull(createResult);
        Assert.NotNull(createResult.LeadNumber);

        var leadNumber = createResult.LeadNumber!;

        // Act - Update
        var updateRequest = new UpdateLeadRequest
        {
            LeadNumber = leadNumber,
            AccountNumber = AccountNumber,
            AccountSecret = AccountSecret,
            CustomerName = "Integration Test Customer",
            CustomerEmail = "integration.test@example.com",
            Comments = "Integration test lead - updated by automated test"
        };

        var updateResult = await _client.UpdateLeadAsync(
            ClientId, ClientSecret, Environment, DealershipId, updateRequest);

        Assert.NotNull(updateResult);
        Assert.Equal(leadNumber, updateResult.LeadNumber);

        // Act - Get
        var getResult = await _client.GetLeadDetailAsync(
            ClientId, ClientSecret,
            AccountNumber, AccountSecret,
            Environment, DealershipId,
            leadNumber);

        Assert.NotNull(getResult);
        Assert.Equal(leadNumber, getResult.LeadNumber);
        Assert.Contains("updated", getResult.Comments, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Minimal IHttpClientFactory implementation for integration tests
    /// </summary>
    private class DefaultHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new HttpClient();
    }
}
