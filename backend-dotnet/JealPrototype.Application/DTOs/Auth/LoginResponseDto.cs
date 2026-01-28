using System.Text.Json.Serialization;

namespace JealPrototype.Application.DTOs.Auth;

public class LoginResponseDto
{
    public string Token { get; set; } = null!;
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string UserType { get; set; } = null!;
    public int? DealershipId { get; set; }
    public List<string> Permissions { get; set; } = new();
}
