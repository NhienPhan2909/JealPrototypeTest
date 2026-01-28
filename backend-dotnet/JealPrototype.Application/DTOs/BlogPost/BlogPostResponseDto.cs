namespace JealPrototype.Application.DTOs.BlogPost;

public class BlogPostResponseDto
{
    public int Id { get; set; }
    public int DealershipId { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? Excerpt { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string AuthorName { get; set; } = null!;
    public string Status { get; set; } = "draft";
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
