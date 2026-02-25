using System.Net;
using System.Reflection;
using JealPrototype.Application.Interfaces;
using JealPrototype.Application.Services.EasyCars;
using JealPrototype.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace JealPrototype.Tests.Unit.Services.EasyCars;

/// <summary>
/// Unit tests for Story 2.6: ImageDownloadService
/// Covers AC1, AC2, AC5, AC6, AC7, AC9, AC10
/// </summary>
public class ImageDownloadServiceTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IImageUploadService> _mockImageUploadService;
    private readonly Mock<ISystemSettingsRepository> _mockSettingsRepo;
    private readonly Mock<ILogger<ImageDownloadService>> _mockLogger;

    public ImageDownloadServiceTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockImageUploadService = new Mock<IImageUploadService>();
        _mockSettingsRepo = new Mock<ISystemSettingsRepository>();
        _mockLogger = new Mock<ILogger<ImageDownloadService>>();

        // Default: image sync enabled
        _mockSettingsRepo
            .Setup(r => r.GetBoolValueAsync("easycar_image_sync_enabled", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    // ---- Helpers ----

    private ImageDownloadService CreateSut() =>
        new(_mockHttpClientFactory.Object, _mockImageUploadService.Object,
            _mockSettingsRepo.Object, _mockLogger.Object);

    private static byte[] FakeImageBytes(int seed = 1) =>
        Enumerable.Range(0, 100).Select(i => (byte)((i + seed) % 256)).ToArray();

    private HttpClient CreateMockHttpClient(HttpResponseMessage response)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        return new HttpClient(handler.Object);
    }

    private HttpClient CreateMockHttpClientSequential(params HttpResponseMessage[] responses)
    {
        var queue = new Queue<HttpResponseMessage>(responses);
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => queue.Dequeue());
        return new HttpClient(handler.Object);
    }

    // ---- Tests ----

    [Fact]
    public async Task DownloadAndStoreImagesAsync_WithValidUrls_ReturnsCloudinaryUrls()
    {
        // Arrange
        var imageBytes = FakeImageBytes();
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(imageBytes)
        });
        _mockHttpClientFactory.Setup(f => f.CreateClient("ImageDownload")).Returns(httpClient);
        _mockImageUploadService
            .Setup(s => s.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://res.cloudinary.com/test/image.jpg");

        var sut = CreateSut();

        // Act
        var result = await sut.DownloadAndStoreImagesAsync(
            new List<string> { "https://easycars.com/img1.jpg" }, vehicleId: 1);

        // Assert
        Assert.Single(result);
        Assert.Equal("https://res.cloudinary.com/test/image.jpg", result[0]);
    }

    [Fact]
    public async Task DownloadAndStoreImagesAsync_With404Url_LogsWarningAndContinuesWithOthers()
    {
        // Arrange
        var goodBytes = FakeImageBytes(1);
        var httpClient = CreateMockHttpClientSequential(
            new HttpResponseMessage(HttpStatusCode.NotFound),
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(goodBytes) }
        );
        _mockHttpClientFactory.Setup(f => f.CreateClient("ImageDownload")).Returns(httpClient);
        _mockImageUploadService
            .Setup(s => s.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://res.cloudinary.com/test/image_good.jpg");

        var sut = CreateSut();

        // Act
        var result = await sut.DownloadAndStoreImagesAsync(
            new List<string> { "https://easycars.com/missing.jpg", "https://easycars.com/good.jpg" },
            vehicleId: 2);

        // Assert – only the successful URL returned, no exception
        Assert.Single(result);
        Assert.Equal("https://res.cloudinary.com/test/image_good.jpg", result[0]);
    }

    [Fact]
    public async Task DownloadAndStoreImagesAsync_WithDuplicateImageBytes_UploadsOnce()
    {
        // Arrange – both URLs return identical bytes → same MD5
        var sharedBytes = FakeImageBytes(42);
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(sharedBytes)
        });
        // The factory must return a new client per call (or the same; bytes will be identical)
        _mockHttpClientFactory.Setup(f => f.CreateClient("ImageDownload")).Returns(httpClient);
        _mockImageUploadService
            .Setup(s => s.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://res.cloudinary.com/test/single.jpg");

        var sut = CreateSut();

        // Act
        var result = await sut.DownloadAndStoreImagesAsync(
            new List<string> { "https://easycars.com/img_a.jpg", "https://easycars.com/img_b.jpg" },
            vehicleId: 3);

        // Assert – upload called only once due to duplicate detection
        _mockImageUploadService.Verify(
            s => s.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Single(result);
    }

    [Fact]
    public async Task DownloadAndStoreImagesAsync_WithImageSyncDisabled_ReturnsEmptyList()
    {
        // Arrange
        _mockSettingsRepo
            .Setup(r => r.GetBoolValueAsync("easycar_image_sync_enabled", true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = CreateSut();

        // Act
        var result = await sut.DownloadAndStoreImagesAsync(
            new List<string> { "https://easycars.com/img1.jpg" }, vehicleId: 4);

        // Assert – empty list, no HTTP calls
        Assert.Empty(result);
        _mockHttpClientFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DownloadAndStoreImagesAsync_WithEmptyUrls_ReturnsEmptyList()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = await sut.DownloadAndStoreImagesAsync(new List<string>(), vehicleId: 5);

        // Assert
        Assert.Empty(result);
        _mockHttpClientFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DownloadAndStoreImagesAsync_WithDownloadException_LogsWarningAndContinues()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(handler.Object);
        _mockHttpClientFactory.Setup(f => f.CreateClient("ImageDownload")).Returns(httpClient);

        var sut = CreateSut();

        // Act – should not throw
        var result = await sut.DownloadAndStoreImagesAsync(
            new List<string> { "https://easycars.com/img1.jpg" }, vehicleId: 6);

        // Assert – empty result, no exception thrown
        Assert.Empty(result);
        _mockImageUploadService.Verify(
            s => s.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DownloadAndStoreImagesAsync_WithNullUrls_ReturnsEmptyList()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = await sut.DownloadAndStoreImagesAsync(null!, vehicleId: 7);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task DownloadAndStoreImagesAsync_WithCloudinaryUploadFailure_LogsWarningAndContinues()
    {
        // Arrange
        var imageBytes = FakeImageBytes();
        var httpClient = CreateMockHttpClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(imageBytes)
        });
        _mockHttpClientFactory.Setup(f => f.CreateClient("ImageDownload")).Returns(httpClient);
        _mockImageUploadService
            .Setup(s => s.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cloudinary error"));

        var sut = CreateSut();

        // Act – should not throw
        var result = await sut.DownloadAndStoreImagesAsync(
            new List<string> { "https://easycars.com/img1.jpg" }, vehicleId: 8);

        // Assert – empty result, no exception thrown
        Assert.Empty(result);
    }
}
