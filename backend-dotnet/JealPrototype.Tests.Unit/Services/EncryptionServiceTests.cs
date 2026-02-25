using FluentAssertions;
using JealPrototype.Application.Exceptions;
using JealPrototype.Application.Interfaces;
using JealPrototype.Infrastructure.Configuration;
using JealPrototype.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Cryptography;

namespace JealPrototype.Tests.Unit.Services;

public class EncryptionServiceTests
{
    private readonly IEncryptionService _encryptionService;
    private readonly Mock<ILogger<EncryptionService>> _mockLogger;
    private readonly EncryptionSettings _settings;

    public EncryptionServiceTests()
    {
        // Generate a valid 256-bit encryption key for testing
        byte[] key = new byte[32]; // 256 bits
        RandomNumberGenerator.Fill(key);
        var base64Key = Convert.ToBase64String(key);

        _settings = new EncryptionSettings
        {
            EncryptionKey = base64Key,
            KeyVersion = 1
        };

        _mockLogger = new Mock<ILogger<EncryptionService>>();
        var options = Options.Create(_settings);

        _encryptionService = new EncryptionService(options, _mockLogger.Object);
    }

    [Fact]
    public async Task EncryptAsync_WithValidPlaintext_ReturnsBase64String()
    {
        // Arrange
        var plaintext = "Test credential data";

        // Act
        var result = await _encryptionService.EncryptAsync(plaintext);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().MatchRegex("^[A-Za-z0-9+/]+=*$"); // Valid Base64
    }

    [Fact]
    public async Task EncryptAsync_CalledTwice_GeneratesDifferentCiphertext()
    {
        // Arrange
        var plaintext = "Same plaintext";

        // Act
        var ciphertext1 = await _encryptionService.EncryptAsync(plaintext);
        var ciphertext2 = await _encryptionService.EncryptAsync(plaintext);

        // Assert
        ciphertext1.Should().NotBe(ciphertext2); // Different IVs produce different ciphertext
    }

    [Fact]
    public async Task EncryptAsync_ThenDecryptAsync_ReturnsOriginalPlaintext()
    {
        // Arrange
        var originalPlaintext = "EasyCars Account Number: 12345";

        // Act
        var ciphertext = await _encryptionService.EncryptAsync(originalPlaintext);
        var decryptedPlaintext = await _encryptionService.DecryptAsync(ciphertext);

        // Assert
        decryptedPlaintext.Should().Be(originalPlaintext);
    }

    [Fact]
    public async Task EncryptAsync_WithNullPlaintext_ThrowsArgumentNullException()
    {
        // Arrange
        string? plaintext = null;

        // Act
        Func<Task> act = async () => await _encryptionService.EncryptAsync(plaintext!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("plaintext");
    }

    [Fact]
    public async Task EncryptAsync_WithEmptyString_ThrowsArgumentNullException()
    {
        // Arrange
        var plaintext = string.Empty;

        // Act
        Func<Task> act = async () => await _encryptionService.EncryptAsync(plaintext);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("plaintext");
    }

    [Fact]
    public async Task DecryptAsync_WithNullCiphertext_ThrowsArgumentNullException()
    {
        // Arrange
        string? ciphertext = null;

        // Act
        Func<Task> act = async () => await _encryptionService.DecryptAsync(ciphertext!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("ciphertext");
    }

    [Fact]
    public async Task DecryptAsync_WithEmptyString_ThrowsArgumentNullException()
    {
        // Arrange
        var ciphertext = string.Empty;

        // Act
        Func<Task> act = async () => await _encryptionService.DecryptAsync(ciphertext);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("ciphertext");
    }

    [Fact]
    public async Task DecryptAsync_WithInvalidBase64_ThrowsDecryptionException()
    {
        // Arrange
        var invalidCiphertext = "This is not valid base64!!!";

        // Act
        Func<Task> act = async () => await _encryptionService.DecryptAsync(invalidCiphertext);

        // Assert
        await act.Should().ThrowAsync<DecryptionException>()
            .WithMessage("*Invalid ciphertext format*");
    }

    [Fact]
    public async Task DecryptAsync_WithTamperedCiphertext_ThrowsDecryptionException()
    {
        // Arrange
        var plaintext = "Original data";
        var ciphertext = await _encryptionService.EncryptAsync(plaintext);
        
        // Tamper with the ciphertext by changing a character
        var tamperedCiphertext = ciphertext.Substring(0, ciphertext.Length - 5) + "XXXX" + ciphertext.Substring(ciphertext.Length - 1);

        // Act
        Func<Task> act = async () => await _encryptionService.DecryptAsync(tamperedCiphertext);

        // Assert
        await act.Should().ThrowAsync<DecryptionException>()
            .WithMessage("*corrupted or tampered*");
    }

    [Fact]
    public async Task DecryptAsync_WithTooShortCiphertext_ThrowsDecryptionException()
    {
        // Arrange - Create a base64 string that's too short (less than 29 bytes: 12 nonce + 16 tag + 1 data)
        var tooShortCiphertext = Convert.ToBase64String(new byte[20]);

        // Act
        Func<Task> act = async () => await _encryptionService.DecryptAsync(tooShortCiphertext);

        // Assert
        await act.Should().ThrowAsync<DecryptionException>()
            .WithMessage("*too short*");
    }

    [Fact]
    public async Task EncryptAsync_WithLongPlaintext_SuccessfullyEncryptsAndDecrypts()
    {
        // Arrange - Test with 1KB of data
        var longPlaintext = new string('A', 1024);

        // Act
        var ciphertext = await _encryptionService.EncryptAsync(longPlaintext);
        var decryptedPlaintext = await _encryptionService.DecryptAsync(ciphertext);

        // Assert
        decryptedPlaintext.Should().Be(longPlaintext);
        decryptedPlaintext.Length.Should().Be(1024);
    }

    [Fact]
    public async Task EncryptAsync_WithSpecialCharacters_PreservesData()
    {
        // Arrange
        var plaintextWithSpecialChars = "Test@123!#$%^&*()_+-=[]{}|;':\",./<>?éñç你好";

        // Act
        var ciphertext = await _encryptionService.EncryptAsync(plaintextWithSpecialChars);
        var decryptedPlaintext = await _encryptionService.DecryptAsync(ciphertext);

        // Assert
        decryptedPlaintext.Should().Be(plaintextWithSpecialChars);
    }

    [Fact]
    public async Task EncryptAsync_WithUnicodeText_PreservesData()
    {
        // Arrange
        var unicodeText = "Hello 世界 🌍 Привет";

        // Act
        var ciphertext = await _encryptionService.EncryptAsync(unicodeText);
        var decryptedPlaintext = await _encryptionService.DecryptAsync(ciphertext);

        // Assert
        decryptedPlaintext.Should().Be(unicodeText);
    }

    [Fact]
    public async Task EncryptAsync_WithVeryLongPlaintext_SuccessfullyEncryptsAndDecrypts()
    {
        // Arrange - Test with 10KB of data
        var veryLongPlaintext = new string('B', 10240);

        // Act
        var ciphertext = await _encryptionService.EncryptAsync(veryLongPlaintext);
        var decryptedPlaintext = await _encryptionService.DecryptAsync(ciphertext);

        // Assert
        decryptedPlaintext.Should().Be(veryLongPlaintext);
        decryptedPlaintext.Length.Should().Be(10240);
    }

    [Fact]
    public void Constructor_WithNullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        IOptions<EncryptionSettings>? nullSettings = null;
        var logger = new Mock<ILogger<EncryptionService>>().Object;

        // Act
        Action act = () => new EncryptionService(nullSettings!, logger);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var settings = Options.Create(_settings);
        ILogger<EncryptionService>? nullLogger = null;

        // Act
        Action act = () => new EncryptionService(settings, nullLogger!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithEmptyEncryptionKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var invalidSettings = new EncryptionSettings { EncryptionKey = string.Empty };
        var options = Options.Create(invalidSettings);
        var logger = new Mock<ILogger<EncryptionService>>().Object;

        // Act
        Action act = () => new EncryptionService(options, logger);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Encryption key is not configured*");
    }

    [Fact]
    public void Constructor_WithInvalidKeyLength_ThrowsInvalidOperationException()
    {
        // Arrange - Create a key that's not 256 bits (using 128 bits instead)
        var shortKey = Convert.ToBase64String(new byte[16]); // 128 bits
        var invalidSettings = new EncryptionSettings { EncryptionKey = shortKey };
        var options = Options.Create(invalidSettings);
        var logger = new Mock<ILogger<EncryptionService>>().Object;

        // Act
        Action act = () => new EncryptionService(options, logger);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be 256 bits*");
    }

    [Fact]
    public void Constructor_WithInvalidBase64Key_ThrowsInvalidOperationException()
    {
        // Arrange
        var invalidSettings = new EncryptionSettings { EncryptionKey = "Not-Valid-Base64!!!" };
        var options = Options.Create(invalidSettings);
        var logger = new Mock<ILogger<EncryptionService>>().Object;

        // Act
        Action act = () => new EncryptionService(options, logger);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be a valid Base64-encoded string*");
    }

    [Fact]
    public async Task DecryptAsync_WithWrongKey_ThrowsDecryptionException()
    {
        // Arrange - Encrypt with one key
        var plaintext = "Secret data";
        var ciphertext = await _encryptionService.EncryptAsync(plaintext);

        // Create a new service with a different key
        var differentKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var differentSettings = new EncryptionSettings { EncryptionKey = differentKey };
        var differentOptions = Options.Create(differentSettings);
        var differentService = new EncryptionService(differentOptions, _mockLogger.Object);

        // Act
        Func<Task> act = async () => await differentService.DecryptAsync(ciphertext);

        // Assert
        await act.Should().ThrowAsync<DecryptionException>()
            .WithMessage("*corrupted or tampered*");
    }
}
