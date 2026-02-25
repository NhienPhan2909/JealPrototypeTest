using System.Collections.Concurrent;
using System.Security.Cryptography;
using JealPrototype.Application.Interfaces;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JealPrototype.Application.Services.EasyCars;

/// <summary>
/// Downloads images from EasyCars URLs and stores them in Cloudinary.
/// Implements Story 2.6: Image Synchronization for Stock Items.
/// </summary>
public class ImageDownloadService : IImageDownloadService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IImageUploadService _imageUploadService;
    private readonly ISystemSettingsRepository _systemSettingsRepository;
    private readonly ILogger<ImageDownloadService> _logger;
    private static readonly SemaphoreSlim _semaphore = new(5, 5);

    public ImageDownloadService(
        IHttpClientFactory httpClientFactory,
        IImageUploadService imageUploadService,
        ISystemSettingsRepository systemSettingsRepository,
        ILogger<ImageDownloadService> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _imageUploadService = imageUploadService ?? throw new ArgumentNullException(nameof(imageUploadService));
        _systemSettingsRepository = systemSettingsRepository ?? throw new ArgumentNullException(nameof(systemSettingsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<string>> DownloadAndStoreImagesAsync(
        List<string> imageUrls,
        int vehicleId,
        CancellationToken cancellationToken = default)
    {
        // AC10: configuration toggle
        var imageSyncEnabled = await _systemSettingsRepository
            .GetBoolValueAsync("easycar_image_sync_enabled", true, cancellationToken);

        if (!imageSyncEnabled)
        {
            _logger.LogInformation("Image sync disabled via easycar_image_sync_enabled. Skipping for vehicle {VehicleId}.", vehicleId);
            return new List<string>();
        }

        if (imageUrls == null || imageUrls.Count == 0)
            return new List<string>();

        var seenHashes = new HashSet<string>(); // AC6: dedup within this sync call
        var results = new ConcurrentBag<string>();

        // AC7: rate-limited parallel downloads
        var tasks = imageUrls.Select((url, index) =>
            DownloadSingleImageAsync(url, vehicleId, index, seenHashes, results, cancellationToken));

        await Task.WhenAll(tasks);

        var cloudinaryUrls = results.ToList();
        _logger.LogInformation(
            "Image sync complete for vehicle {VehicleId}: {Count}/{Total} images stored.",
            vehicleId, cloudinaryUrls.Count, imageUrls.Count);

        return cloudinaryUrls;
    }

    private async Task DownloadSingleImageAsync(
        string url,
        int vehicleId,
        int index,
        HashSet<string> seenHashes,
        ConcurrentBag<string> results,
        CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var httpClient = _httpClientFactory.CreateClient("ImageDownload");
            byte[] imageBytes;

            try
            {
                var response = await httpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    // AC5: graceful failure – log warning and continue
                    _logger.LogWarning(
                        "Failed to download image {Index} for vehicle {VehicleId}: HTTP {StatusCode} from {Url}",
                        index, vehicleId, (int)response.StatusCode, url);
                    return;
                }
                imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // AC5: graceful failure – log warning and continue
                _logger.LogWarning(ex, "Exception downloading image {Index} for vehicle {VehicleId} from {Url}", index, vehicleId, url);
                return;
            }

            // AC6: MD5 dedup within this sync call
            string hash;
            using (var md5 = MD5.Create())
                hash = Convert.ToHexString(md5.ComputeHash(imageBytes));

            lock (seenHashes)
            {
                if (!seenHashes.Add(hash))
                {
                    _logger.LogInformation(
                        "Skipping duplicate image {Index} for vehicle {VehicleId} (hash {Hash})",
                        index, vehicleId, hash);
                    return;
                }
            }

            // AC3: upload to Cloudinary
            var fileName = $"vehicle_{vehicleId}_{index}_{hash[..8]}.jpg";
            using var stream = new MemoryStream(imageBytes);
            string cloudinaryUrl;
            try
            {
                cloudinaryUrl = await _imageUploadService.UploadImageAsync(stream, fileName, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to upload image {Index} to Cloudinary for vehicle {VehicleId}", index, vehicleId);
                return;
            }

            results.Add(cloudinaryUrl);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
