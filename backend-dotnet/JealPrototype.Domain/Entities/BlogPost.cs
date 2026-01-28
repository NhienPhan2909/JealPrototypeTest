using JealPrototype.Domain.Enums;

namespace JealPrototype.Domain.Entities;

public class BlogPost : BaseEntity
{
    public int DealershipId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public string? Excerpt { get; private set; }
    public string? FeaturedImageUrl { get; private set; }
    public string AuthorName { get; private set; } = null!;
    public BlogPostStatus Status { get; private set; } = BlogPostStatus.Draft;
    public DateTime? PublishedAt { get; private set; }
    public new DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public Dealership Dealership { get; private set; } = null!;

    private BlogPost() { }

    public static BlogPost Create(
        int dealershipId,
        string title,
        string content,
        string authorName,
        string? slug = null,
        string? excerpt = null,
        string? featuredImageUrl = null,
        BlogPostStatus status = BlogPostStatus.Draft)
    {
        if (dealershipId <= 0)
            throw new ArgumentException("Invalid dealership ID", nameof(dealershipId));

        if (string.IsNullOrWhiteSpace(title) || title.Length > 255)
            throw new ArgumentException("Title is required and must be 255 characters or less", nameof(title));

        if (string.IsNullOrWhiteSpace(content) || content.Length > 50000)
            throw new ArgumentException("Content is required and must be 50000 characters or less", nameof(content));

        if (string.IsNullOrWhiteSpace(authorName) || authorName.Length > 255)
            throw new ArgumentException("Author name is required and must be 255 characters or less", nameof(authorName));

        var finalSlug = slug ?? GenerateSlug(title);

        return new BlogPost
        {
            DealershipId = dealershipId,
            Title = title,
            Slug = finalSlug,
            Content = content,
            Excerpt = excerpt,
            FeaturedImageUrl = featuredImageUrl,
            AuthorName = authorName,
            Status = status,
            PublishedAt = status == BlogPostStatus.Published ? DateTime.UtcNow : null
        };
    }

    public void Update(
        string title,
        string content,
        string? excerpt = null,
        string? featuredImageUrl = null,
        BlogPostStatus? status = null)
    {
        Title = title;
        Content = content;
        Excerpt = excerpt;
        FeaturedImageUrl = featuredImageUrl;
        
        if (status.HasValue && status.Value != Status)
        {
            Status = status.Value;
            if (status.Value == BlogPostStatus.Published && PublishedAt == null)
                PublishedAt = DateTime.UtcNow;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateSlug(string title)
    {
        return title
            .ToLower()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "")
            .Substring(0, Math.Min(200, title.Length));
    }
}
