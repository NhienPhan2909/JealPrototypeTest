namespace JealPrototype.Application.Interfaces;

public interface IImageUploadService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<List<string>> UploadMultipleImagesAsync(IEnumerable<(Stream stream, string fileName)> files, CancellationToken cancellationToken = default);
    Task DeleteImageAsync(string publicId, CancellationToken cancellationToken = default);
}

