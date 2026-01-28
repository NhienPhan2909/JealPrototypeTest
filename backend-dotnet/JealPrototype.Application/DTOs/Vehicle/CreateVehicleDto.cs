namespace JealPrototype.Application.DTOs.Vehicle;

public class CreateVehicleDto
{
    public int DealershipId { get; set; }
    public string Make { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public string Condition { get; set; } = null!;
    public string Status { get; set; } = "draft";
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public List<string>? Images { get; set; }
}
