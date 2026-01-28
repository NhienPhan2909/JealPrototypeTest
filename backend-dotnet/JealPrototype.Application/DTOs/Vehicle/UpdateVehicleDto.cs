namespace JealPrototype.Application.DTOs.Vehicle;

public class UpdateVehicleDto
{
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public decimal? Price { get; set; }
    public int? Mileage { get; set; }
    public string? Condition { get; set; }
    public string? Status { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<string>? Images { get; set; }
}
