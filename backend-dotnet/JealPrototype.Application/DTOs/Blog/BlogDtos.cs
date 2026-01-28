namespace JealPrototype.Application.DTOs.Blog;

public class BlogDto
{
    public int Id { get; set; }
    public int DealershipId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime PublishedDate { get; set; }
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateBlogDto
{
    public int DealershipId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime PublishedDate { get; set; }
    public string Author { get; set; } = string.Empty;
}

public class UpdateBlogDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? Author { get; set; }
}
