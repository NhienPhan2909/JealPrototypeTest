namespace JealPrototype.Application.DTOs.Dealership;

public class CreateDealershipDto
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string? Hours { get; set; }
    public string? About { get; set; }
    public string? WebsiteUrl { get; set; }
}
