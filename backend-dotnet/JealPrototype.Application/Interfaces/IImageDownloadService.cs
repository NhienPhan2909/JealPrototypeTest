namespace JealPrototype.Application.Interfaces;

public interface IImageDownloadService
{
    Task<List<string>> DownloadAndStoreImagesAsync(
        List<string> imageUrls,
        int vehicleId,
        CancellationToken cancellationToken = default);
}
