namespace JealPrototype.Application.DTOs.User;

public class UpdateUserDto
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? UserType { get; set; }
    public List<string>? Permissions { get; set; }
}
