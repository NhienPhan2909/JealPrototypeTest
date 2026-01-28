namespace JealPrototype.Application.DTOs.BlogPost;

public class UpdateBlogPostDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Slug { get; set; }
    public string? Excerpt { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string? AuthorName { get; set; }
    public string? Status { get; set; }
}
