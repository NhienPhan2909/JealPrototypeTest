using FluentAssertions;
using JealPrototype.Tests.Integration.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace JealPrototype.Tests.Integration.Auth;

public class AuthenticationIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var loginRequest = new { Username = "manager1", Password = "Password123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturn401()
    {
        // Arrange
        var loginRequest = new { Username = "manager1", Password = "WrongPassword!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturn401()
    {
        // Arrange
        var loginRequest = new { Username = "nonexistent", Password = "Password123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_DifferentUsers_ShouldReturnDifferentTokens()
    {
        // Arrange
        var loginRequest1 = new { Username = "manager1", Password = "Password123!" };
        var loginRequest2 = new { Username = "manager2", Password = "Password123!" };

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/auth/login", loginRequest1);
        var response2 = await _client.PostAsJsonAsync("/api/auth/login", loginRequest2);

        var result1 = await response1.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        var result2 = await response2.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

        // Assert  
        result1!.Data!.Token.Should().NotBe(result2!.Data!.Token);
    }

    private class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }

    private class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
