namespace JealPrototype.Application.DTOs.Vehicle;

public class VehicleResponseDto
{
    public int Id { get; set; }
    public int DealershipId { get; set; }
    public string Make { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public string Condition { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public List<string> Images { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
