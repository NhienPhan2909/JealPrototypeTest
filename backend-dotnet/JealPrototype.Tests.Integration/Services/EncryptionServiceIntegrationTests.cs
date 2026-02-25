using FluentAssertions;
using JealPrototype.Application.Exceptions;
using JealPrototype.Application.Interfaces;
using JealPrototype.Tests.Integration.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace JealPrototype.Tests.Integration.Services;

public class EncryptionServiceIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly IEncryptionService _encryptionService;

    public EncryptionServiceIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        
        // Get the encryption service from DI container
        var scope = factory.Services.CreateScope();
        _encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
    }

    [Fact]
    public void EncryptionService_ShouldBeRegistered_InDependencyInjection()
    {
        // Assert
        _encryptionService.Should().NotBeNull();
        _encryptionService.Should().BeAssignableTo<IEncryptionService>();
    }

    [Fact]
    public async Task EncryptionService_ShouldLoadConfiguration_FromEnvironmentOrSettings()
    {
        // Arrange
        var testData = "Configuration test data";

        // Act - If this doesn't throw, configuration is loaded correctly
        var encrypted = await _encryptionService.EncryptAsync(testData);
        var decrypted = await _encryptionService.DecryptAsync(encrypted);

        // Assert
        decrypted.Should().Be(testData);
    }

    [Fact]
    public async Task EncryptionService_EndToEndScenario_EncryptAndDecryptCredentials()
    {
        // Arrange - Simulate EasyCars credentials
        var accountNumber = "EASYCAR-ACC-123456";
        var secretKey = "sk_test_a1b2c3d4e5f6g7h8i9j0";

        // Act - Encrypt both credentials
        var encryptedAccount = await _encryptionService.EncryptAsync(accountNumber);
        var encryptedSecret = await _encryptionService.EncryptAsync(secretKey);

        // Verify encrypted values are different from plaintext
        encryptedAccount.Should().NotBe(accountNumber);
        encryptedSecret.Should().NotBe(secretKey);

        // Decrypt and verify
        var decryptedAccount = await _encryptionService.DecryptAsync(encryptedAccount);
        var decryptedSecret = await _encryptionService.DecryptAsync(encryptedSecret);

        // Assert
        decryptedAccount.Should().Be(accountNumber);
        decryptedSecret.Should().Be(secretKey);
    }

    [Fact]
    public async Task EncryptionService_MultipleInstances_ShouldUseSameKey()
    {
        // Arrange
        var scope1 = _factory.Services.CreateScope();
        var scope2 = _factory.Services.CreateScope();
        
        var service1 = scope1.ServiceProvider.GetRequiredService<IEncryptionService>();
        var service2 = scope2.ServiceProvider.GetRequiredService<IEncryptionService>();

        var plaintext = "Test data for multiple instances";

        // Act
        var encrypted = await service1.EncryptAsync(plaintext);
        var decrypted = await service2.DecryptAsync(encrypted);

        // Assert - Service 2 should be able to decrypt what Service 1 encrypted
        decrypted.Should().Be(plaintext);
    }

    [Fact]
    public async Task EncryptionService_LargeDataSet_HandlesMultipleOperations()
    {
        // Arrange - Simulate encrypting multiple credentials
        var credentials = new Dictionary<string, string>
        {
            { "Account1", "ACC-001-123456" },
            { "Secret1", "sk_live_abc123def456" },
            { "Account2", "ACC-002-789012" },
            { "Secret2", "sk_live_ghi789jkl012" },
            { "Account3", "ACC-003-345678" },
            { "Secret3", "sk_live_mno345pqr678" }
        };

        // Act - Encrypt all credentials
        var encryptedData = new Dictionary<string, string>();
        foreach (var kvp in credentials)
        {
            encryptedData[kvp.Key] = await _encryptionService.EncryptAsync(kvp.Value);
        }

        // Decrypt all and verify
        var decryptedData = new Dictionary<string, string>();
        foreach (var kvp in encryptedData)
        {
            decryptedData[kvp.Key] = await _encryptionService.DecryptAsync(kvp.Value);
        }

        // Assert
        decryptedData.Should().BeEquivalentTo(credentials);
    }

    [Fact]
    public async Task EncryptionService_ConcurrentOperations_ShouldHandleThreadSafety()
    {
        // Arrange
        var plaintext = "Concurrent test data";
        var tasks = new List<Task<string>>();

        // Act - Perform 10 concurrent encryption operations
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_encryptionService.EncryptAsync($"{plaintext}-{i}"));
        }

        var encryptedResults = await Task.WhenAll(tasks);

        // Decrypt all results
        var decryptTasks = encryptedResults.Select(e => _encryptionService.DecryptAsync(e)).ToArray();
        var decryptedResults = await Task.WhenAll(decryptTasks);

        // Assert - All results should be unique (different IVs) and decrypt correctly
        encryptedResults.Should().OnlyHaveUniqueItems();
        for (int i = 0; i < 10; i++)
        {
            decryptedResults[i].Should().Be($"{plaintext}-{i}");
        }
    }

    [Fact]
    public async Task EncryptionService_ServiceLifetime_ScopedService()
    {
        // Arrange - Create two scopes
        var scope1 = _factory.Services.CreateScope();
        var scope2 = _factory.Services.CreateScope();

        var service1 = scope1.ServiceProvider.GetRequiredService<IEncryptionService>();
        var service2 = scope2.ServiceProvider.GetRequiredService<IEncryptionService>();

        // Assert - Should get different instances (scoped)
        service1.Should().NotBeSameAs(service2);
    }

    [Fact]
    public async Task EncryptionService_WithSpecialCharacters_IntegrationTest()
    {
        // Arrange - Test data with various special characters
        var specialData = "Test@#$%^&*()_+-=[]{}|;':\",./<>?`~éñçü你好🌍";

        // Act
        var encrypted = await _encryptionService.EncryptAsync(specialData);
        var decrypted = await _encryptionService.DecryptAsync(encrypted);

        // Assert
        decrypted.Should().Be(specialData);
    }

    [Fact]
    public async Task EncryptionService_PerformanceTest_ShouldCompleteQuickly()
    {
        // Arrange
        var plaintext = "Performance test data";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - Perform 100 encrypt/decrypt cycles
        for (int i = 0; i < 100; i++)
        {
            var encrypted = await _encryptionService.EncryptAsync(plaintext);
            var decrypted = await _encryptionService.DecryptAsync(encrypted);
            decrypted.Should().Be(plaintext);
        }

        stopwatch.Stop();

        // Assert - Should complete in reasonable time (< 2 seconds for 100 cycles)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000);
    }

    [Fact]
    public async Task EncryptionService_ErrorHandling_InvalidData()
    {
        // Act & Assert
        await FluentActions.Invoking(async () => 
                await _encryptionService.DecryptAsync("InvalidBase64Data!!!"))
            .Should().ThrowAsync<DecryptionException>()
            .WithMessage("*Invalid ciphertext format*");
    }
}
