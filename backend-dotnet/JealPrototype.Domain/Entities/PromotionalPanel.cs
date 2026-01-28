namespace JealPrototype.Domain.Entities;

public class PromotionalPanel : BaseEntity
{
    public int DealershipId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    public virtual Dealership? Dealership { get; set; }
}
