using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using JealPrototype.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace JealPrototype.Infrastructure.Services;

public class CloudinaryImageUploadService : IImageUploadService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryImageUploadService(IConfiguration configuration)
    {
        var cloudName = configuration["CloudinarySettings:CloudName"] 
            ?? throw new InvalidOperationException("Cloudinary CloudName not configured");
        var apiKey = configuration["CloudinarySettings:ApiKey"] 
            ?? throw new InvalidOperationException("Cloudinary ApiKey not configured");
        var apiSecret = configuration["CloudinarySettings:ApiSecret"] 
            ?? throw new InvalidOperationException("Cloudinary ApiSecret not configured");

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        if (fileStream.Length == 0)
            throw new ArgumentException("Stream is empty", nameof(fileStream));

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = "jeal-prototype",
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (uploadResult.Error != null)
            throw new InvalidOperationException($"Image upload failed: {uploadResult.Error.Message}");

        return uploadResult.SecureUrl.ToString();
    }

    public async Task<List<string>> UploadMultipleImagesAsync(IEnumerable<(Stream stream, string fileName)> files, CancellationToken cancellationToken = default)
    {
        var uploadTasks = files.Select(file => UploadImageAsync(file.stream, file.fileName, cancellationToken));
        var results = await Task.WhenAll(uploadTasks);
        return results.ToList();
    }

    public async Task DeleteImageAsync(string publicId, CancellationToken cancellationToken = default)
    {
        var deletionParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deletionParams);

        if (result.Error != null)
            throw new InvalidOperationException($"Image deletion failed: {result.Error.Message}");
    }
}

