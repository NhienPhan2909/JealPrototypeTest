namespace JealPrototype.Application.DTOs.User;

public class UserResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string UserType { get; set; } = null!;
    public int? DealershipId { get; set; }
    public List<string> Permissions { get; set; } = new();
    public int? CreatedBy { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
