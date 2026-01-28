namespace JealPrototype.Application.DTOs.User;

public class CreateUserDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string UserType { get; set; } = null!;
    public int? DealershipId { get; set; }
    public List<string>? Permissions { get; set; }
}
