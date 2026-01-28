namespace JealPrototype.Application.DTOs.SalesRequest;

public class SalesRequestResponseDto
{
    public int Id { get; set; }
    public int DealershipId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Make { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public int Kilometers { get; set; }
    public string? AdditionalMessage { get; set; }
    public string Status { get; set; } = "received";
    public DateTime CreatedAt { get; set; }
}
