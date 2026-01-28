namespace JealPrototype.Application.DTOs.HeroMedia;

public class HeroMediaDto
{
    public int Id { get; set; }
    public int DealershipId { get; set; }
    public string MediaType { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
