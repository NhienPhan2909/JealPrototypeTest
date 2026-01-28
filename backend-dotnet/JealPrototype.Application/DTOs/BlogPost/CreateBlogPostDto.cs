namespace JealPrototype.Application.DTOs.BlogPost;

public class CreateBlogPostDto
{
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? Slug { get; set; }
    public string? Excerpt { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public string AuthorName { get; set; } = null!;
    public string Status { get; set; } = "draft";
}
