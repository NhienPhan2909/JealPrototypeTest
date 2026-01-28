using System.Text.Json;
using System.Text.Json.Serialization;
using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;

namespace JealPrototype.Infrastructure.Services;

public class GoogleReviewsService : IGoogleReviewsService
{
    private readonly IDealershipRepository _dealershipRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleReviewsService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private const string GooglePlacesBaseUrl = "https://maps.googleapis.com/maps/api/place";

    public GoogleReviewsService(
        IDealershipRepository dealershipRepository,
        IConfiguration configuration,
        ILogger<GoogleReviewsService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _dealershipRepository = dealershipRepository;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _apiKey = _configuration["GooglePlacesSettings:ApiKey"];
    }

    public async Task<GoogleReviewsResult> GetReviewsAsync(int dealershipId)
    {
        var dealership = await _dealershipRepository.GetByIdAsync(dealershipId);
        if (dealership == null)
        {
            throw new KeyNotFoundException("Dealership not found");
        }

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("Google Places API key not configured");
            return new GoogleReviewsResult
            {
                Message = "Google Reviews not configured"
            };
        }

        var searchQuery = $"{dealership.Name} {dealership.Address}";
        var place = await FindPlaceByAddressAsync(searchQuery);
        
        if (place == null)
        {
            _logger.LogWarning("No Google Place found for: {SearchQuery}", searchQuery);
            return new GoogleReviewsResult
            {
                Message = "Location not found on Google Maps"
            };
        }

        var placeDetails = await GetPlaceDetailsAsync(place.PlaceId);
        
        var topReviews = placeDetails.Reviews
            .Where(r => r.Rating >= 4)
            .OrderByDescending(r => r.Rating)
            .Take(10)
            .ToList();

        return new GoogleReviewsResult
        {
            Reviews = topReviews,
            GoogleMapsUrl = placeDetails.Url,
            TotalRatings = placeDetails.UserRatingsTotal,
            AverageRating = placeDetails.Rating
        };
    }

    private async Task<PlaceSearchResult?> FindPlaceByAddressAsync(string query)
    {
        var searchUrl = $"{GooglePlacesBaseUrl}/textsearch/json?query={Uri.EscapeDataString(query)}&key={_apiKey}";
        
        var response = await _httpClient.GetAsync(searchUrl);
        var content = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<TextSearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (data?.Status == "OK" && data.Results?.Length > 0)
        {
            return new PlaceSearchResult { PlaceId = data.Results[0].PlaceId };
        }

        return null;
    }

    private async Task<PlaceDetailsResult> GetPlaceDetailsAsync(string placeId)
    {
        var detailsUrl = $"{GooglePlacesBaseUrl}/details/json?place_id={placeId}&fields=name,rating,reviews,url,user_ratings_total&key={_apiKey}";
        
        var response = await _httpClient.GetAsync(detailsUrl);
        var content = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<PlaceDetailsResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (data?.Status == "OK" && data.Result != null)
        {
            return new PlaceDetailsResult
            {
                Url = data.Result.Url ?? string.Empty,
                Rating = data.Result.Rating,
                UserRatingsTotal = data.Result.UserRatingsTotal,
                Reviews = data.Result.Reviews?.Select(r => new GoogleReview
                {
                    AuthorName = r.AuthorName ?? string.Empty,
                    Rating = r.Rating,
                    Text = r.Text ?? string.Empty,
                    Time = r.Time,
                    RelativeTimeDescription = r.RelativeTimeDescription ?? string.Empty,
                    ProfilePhotoUrl = r.ProfilePhotoUrl ?? string.Empty
                }).ToList() ?? new List<GoogleReview>()
            };
        }

        throw new Exception($"Google Places API error: {data?.Status}");
    }

    private class PlaceSearchResult
    {
        public string PlaceId { get; set; } = string.Empty;
    }

    private class PlaceDetailsResult
    {
        public string Url { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int UserRatingsTotal { get; set; }
        public List<GoogleReview> Reviews { get; set; } = new();
    }

    private class TextSearchResponse
    {
        public string Status { get; set; } = string.Empty;
        public TextSearchPlace[]? Results { get; set; }
    }

    private class TextSearchPlace
    {
        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; } = string.Empty;
    }

    private class PlaceDetailsResponse
    {
        public string Status { get; set; } = string.Empty;
        public PlaceResult? Result { get; set; }
    }

    private class PlaceResult
    {
        public string? Url { get; set; }
        public double Rating { get; set; }
        
        [JsonPropertyName("user_ratings_total")]
        public int UserRatingsTotal { get; set; }
        public ReviewResult[]? Reviews { get; set; }
    }

    private class ReviewResult
    {
        [JsonPropertyName("author_name")]
        public string? AuthorName { get; set; }
        public int Rating { get; set; }
        public string? Text { get; set; }
        public long Time { get; set; }
        
        [JsonPropertyName("relative_time_description")]
        public string? RelativeTimeDescription { get; set; }
        
        [JsonPropertyName("profile_photo_url")]
        public string? ProfilePhotoUrl { get; set; }
    }
}
