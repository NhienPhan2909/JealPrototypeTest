namespace JealPrototype.Application.DTOs.Lead;

public class CreateLeadDto
{
    public int DealershipId { get; set; }
    public int? VehicleId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Message { get; set; } = null!;
}
