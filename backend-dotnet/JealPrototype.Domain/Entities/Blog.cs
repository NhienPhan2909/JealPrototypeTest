namespace JealPrototype.Domain.Entities;

public class Blog : BaseEntity
{
    public int DealershipId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime PublishedDate { get; set; }
    public string Author { get; set; } = string.Empty;

    public virtual Dealership? Dealership { get; set; }
}
