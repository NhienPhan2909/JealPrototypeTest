namespace JealPrototype.Application.DTOs.SalesRequest;

public class CreateSalesRequestDto
{
    public int DealershipId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Make { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public int Kilometers { get; set; }
    public string? AdditionalMessage { get; set; }
}
