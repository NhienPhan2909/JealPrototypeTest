using JealPrototype.Application.DTOs.Common;
using JealPrototype.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JealPrototype.API.Controllers;

[ApiController]
[Route("api/upload")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IImageUploadService _imageUploadService;

    public UploadController(IImageUploadService imageUploadService)
    {
        _imageUploadService = imageUploadService;
    }

    [HttpPost]
    public async Task<ActionResult<object>> UploadImage(IFormFile image)
    {
        if (image == null || image.Length == 0)
            return BadRequest(new { error = "No file provided" });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(image.FileName).ToLower();
        
        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { error = "Invalid file type. Only images are allowed." });

        if (image.Length > 10 * 1024 * 1024) // 10MB
            return BadRequest(new { error = "File size exceeds 10MB limit" });

        using var stream = image.OpenReadStream();
        var imageUrl = await _imageUploadService.UploadImageAsync(stream, image.FileName);

        return Ok(new { url = imageUrl, message = "Image uploaded successfully" });
    }

    [HttpPost("image")]
    public async Task<ActionResult<ApiResponse<string>>> UploadImageWithApiResponse(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.ErrorResponse("No file provided"));

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        
        if (!allowedExtensions.Contains(extension))
            return BadRequest(ApiResponse<string>.ErrorResponse("Invalid file type. Only images are allowed."));

        if (file.Length > 10 * 1024 * 1024) // 10MB
            return BadRequest(ApiResponse<string>.ErrorResponse("File size exceeds 10MB limit"));

        using var stream = file.OpenReadStream();
        var imageUrl = await _imageUploadService.UploadImageAsync(stream, file.FileName);

        return Ok(ApiResponse<string>.SuccessResponse(imageUrl, "Image uploaded successfully"));
    }

    [HttpPost("images")]
    public async Task<ActionResult<ApiResponse<List<string>>>> UploadMultipleImages(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            return BadRequest(ApiResponse<List<string>>.ErrorResponse("No files provided"));

        if (files.Count > 20)
            return BadRequest(ApiResponse<List<string>>.ErrorResponse("Maximum 20 images allowed"));

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var imageUrls = new List<string>();

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiResponse<List<string>>.ErrorResponse($"Invalid file type: {file.FileName}"));

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(ApiResponse<List<string>>.ErrorResponse($"File too large: {file.FileName}"));

            using var stream = file.OpenReadStream();
            var imageUrl = await _imageUploadService.UploadImageAsync(stream, file.FileName);
            imageUrls.Add(imageUrl);
        }

        return Ok(ApiResponse<List<string>>.SuccessResponse(imageUrls, $"{imageUrls.Count} images uploaded successfully"));
    }
}
