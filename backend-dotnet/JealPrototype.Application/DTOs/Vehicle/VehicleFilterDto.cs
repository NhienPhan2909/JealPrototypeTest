namespace JealPrototype.Application.DTOs.Vehicle;

public class VehicleFilterDto
{
    public string? Status { get; set; }
    public string? Brand { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
