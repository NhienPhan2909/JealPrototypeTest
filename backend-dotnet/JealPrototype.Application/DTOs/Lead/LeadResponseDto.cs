namespace JealPrototype.Application.DTOs.Lead;

public class LeadResponseDto
{
    public int Id { get; set; }
    public int DealershipId { get; set; }
    public int? VehicleId { get; set; }
    public string? VehicleTitle { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string Status { get; set; } = "received";
    public DateTime CreatedAt { get; set; }
}
