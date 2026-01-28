namespace JealPrototype.Domain.Entities;

public class HeroMedia : BaseEntity
{
    public int DealershipId { get; set; }
    public string MediaType { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }

    public virtual Dealership? Dealership { get; set; }
}
