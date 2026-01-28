namespace JealPrototype.Application.Interfaces;

public interface IAuthService
{
    string GenerateJwtToken(Domain.Entities.User user);
    bool VerifyPassword(string password, string passwordHash);
    string HashPassword(string password);
}

