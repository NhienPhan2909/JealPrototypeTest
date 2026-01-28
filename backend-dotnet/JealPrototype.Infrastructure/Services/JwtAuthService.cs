using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using JealPrototype.Domain.Entities;
using JealPrototype.Application.Interfaces;

namespace JealPrototype.Infrastructure.Services;

public class JwtAuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    public JwtAuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJwtToken(User user)
    {
        var secretKey = _configuration["JwtSettings:Secret"] 
            ?? throw new InvalidOperationException("JWT secret key not configured");
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email.Value),
            new("user_type", user.UserType.ToString()),
            new("full_name", user.FullName)
        };

        if (user.DealershipId.HasValue)
            claims.Add(new Claim("dealership_id", user.DealershipId.Value.ToString()));

        foreach (var permission in user.Permissions)
            claims.Add(new Claim("permission", permission.ToString()));

        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "1440");

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }
}
