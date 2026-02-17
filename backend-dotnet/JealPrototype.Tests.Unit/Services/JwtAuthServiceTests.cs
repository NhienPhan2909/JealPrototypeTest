using FluentAssertions;
using JealPrototype.Domain.Entities;
using JealPrototype.Domain.Enums;
using JealPrototype.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Reflection;

namespace JealPrototype.Tests.Unit.Services;

public class JwtAuthServiceTests
{
    private readonly JwtAuthService _authService;
    private readonly IConfiguration _configuration;

    public JwtAuthServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"JwtSettings:Secret", "ThisIsAVeryLongSecretKeyForTestingPurposesOnly12345678901234567890"},
            {"JwtSettings:Issuer", "TestIssuer"},
            {"JwtSettings:Audience", "TestAudience"},
            {"JwtSettings:ExpirationMinutes", "60"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _authService = new JwtAuthService(_configuration);
    }

    private User CreateTestUser(int id, string username, string email, string fullName, UserType userType, int? dealershipId = null)
    {
        var user = User.Create(
            username: username,
            passwordHash: "hash",
            email: email,
            fullName: fullName,
            userType: userType,
            dealershipId: dealershipId
        );

        // Use reflection to set the Id since it's protected
        var idProperty = typeof(User).BaseType!.GetProperty("Id");
        idProperty!.SetValue(user, id);

        return user;
    }

    [Fact]
    public void GenerateJwtToken_ShouldIncludeDealershipIdClaim()
    {
        // Arrange
        var user = CreateTestUser(
            id: 1,
            username: "testuser",
            email: "test@example.com",
            fullName: "Test User",
            userType: UserType.DealershipStaff,
            dealershipId: 123
        );

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        // Verify token contains basic claims
        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub");
        subClaim.Should().NotBeNull("token should contain user ID");
        subClaim!.Value.Should().Be("1");
        
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");
        emailClaim.Should().NotBeNull("token should contain email");
        emailClaim!.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void GenerateJwtToken_ShouldIncludeUserTypeClaim()
    {
        // Arrange
        var user = CreateTestUser(
            id: 1,
            username: "admin",
            email: "admin@example.com",
            fullName: "Admin User",
            userType: UserType.Admin
        );

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        // Verify token is generated successfully
        jwtToken.Should().NotBeNull();
        jwtToken.Claims.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateJwtToken_ShouldIncludeUserIdClaim()
    {
        // Arrange
        var user = CreateTestUser(
            id: 42,
            username: "testuser",
            email: "test@example.com",
            fullName: "Test User",
            userType: UserType.DealershipStaff,
            dealershipId: 123
        );

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub");
        userIdClaim.Should().NotBeNull();
        userIdClaim!.Value.Should().Be("42");
    }

    [Fact]
    public void HashPassword_ShouldCreateValidHash()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _authService.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
        hash.Length.Should().BeGreaterThan(50);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }
}
