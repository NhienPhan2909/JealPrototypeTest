namespace JealPrototype.Application.Interfaces;

public interface IGoogleReviewsService
{
    Task<GoogleReviewsResult> GetReviewsAsync(int dealershipId);
}

public class GoogleReviewsResult
{
    public List<GoogleReview> Reviews { get; set; } = new();
    public string GoogleMapsUrl { get; set; } = string.Empty;
    public int TotalRatings { get; set; }
    public double AverageRating { get; set; }
    public string? Message { get; set; }
}

public class GoogleReview
{
    public string AuthorName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public long Time { get; set; }
    public string RelativeTimeDescription { get; set; } = string.Empty;
    public string ProfilePhotoUrl { get; set; } = string.Empty;
}
