using System.Security.Cryptography;
using FluentAssertions;
using JealPrototype.Application.Exceptions;
using JealPrototype.Application.Interfaces.Security;
using JealPrototype.Infrastructure.Configuration;
using JealPrototype.Infrastructure.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace JealPrototype.Tests.Unit.Security;

public class CredentialEncryptionServiceTests
{
    private readonly ICredentialEncryptionService _sut;
    private readonly Mock<ILogger<CredentialEncryptionService>> _loggerMock;
    private readonly EncryptionSettings _settings;

    public CredentialEncryptionServiceTests()
    {
        // Generate a test encryption key
        byte[] testKey = RandomNumberGenerator.GetBytes(32); // 256 bits
        _settings = new EncryptionSettings
        {
            EncryptionKey = Convert.ToBase64String(testKey),
            KeyVersion = 1
        };

        _loggerMock = new Mock<ILogger<CredentialEncryptionService>>();
        _sut = new CredentialEncryptionService(
            Options.Create(_settings),
            _loggerMock.Object
        );
    }

    #region Happy Path Tests

    [Fact]
    public async Task EncryptAsync_ValidPlaintext_ReturnsBase64String()
    {
        // Arrange
        var plaintext = "test-secret-key-123";

        // Act
        var encrypted = await _sut.EncryptAsync(plaintext);

        // Assert
        encrypted.Should().NotBeNullOrEmpty();
        encrypted.Should().NotBe(plaintext);
        
        // Should be valid base64
        var act = () => Convert.FromBase64String(encrypted);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task EncryptDecrypt_RoundTrip_ReturnsOriginalPlaintext()
    {
        // Arrange
        var plaintext = "my-secret-password-!@#$%";

        // Act
        var encrypted = await _sut.EncryptAsync(plaintext);
        var decrypted = await _sut.DecryptAsync(encrypted);

        // Assert
        decrypted.Should().Be(plaintext);
    }

    [Fact]
    public async Task DecryptAsync_ValidCiphertext_ReturnsPlaintext()
    {
        // Arrange
        var plaintext = "test123";
        var encrypted = await _sut.EncryptAsync(plaintext);

        // Act
        var decrypted = await _sut.DecryptAsync(encrypted);

        // Assert
        decrypted.Should().Be(plaintext);
    }

    #endregion

    #region Unique IV Tests

    [Fact]
    public async Task EncryptAsync_SamePlaintext_GeneratesDifferentCiphertext()
    {
        // Arrange
        var plaintext = "same-plaintext";

        // Act
        var encrypted1 = await _sut.EncryptAsync(plaintext);
        var encrypted2 = await _sut.EncryptAsync(plaintext);

        // Assert
        encrypted1.Should().NotBe(encrypted2, "because IVs must be unique for each encryption");
    }

    [Fact]
    public async Task EncryptAsync_MultipleCalls_GeneratesUniqueIVs()
    {
        // Arrange
        var plaintext = "test-data";
        var encryptedValues = new List<string>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            encryptedValues.Add(await _sut.EncryptAsync(plaintext));
        }

        // Assert
        encryptedValues.Should().OnlyHaveUniqueItems("because each encryption should use a unique IV");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task EncryptAsync_NullPlaintext_ThrowsArgumentNullException()
    {
        // Act & Assert
        await FluentActions.Invoking(() => _sut.EncryptAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("plaintext");
    }

    [Fact]
    public async Task EncryptAsync_EmptyString_ThrowsArgumentNullException()
    {
        // Act & Assert
        await FluentActions.Invoking(() => _sut.EncryptAsync(string.Empty))
            .Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("plaintext");
    }

    [Fact]
    public async Task DecryptAsync_NullCiphertext_ThrowsArgumentNullException()
    {
        // Act & Assert
        await FluentActions.Invoking(() => _sut.DecryptAsync(null!))
            .Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("ciphertext");
    }

    [Fact]
    public async Task DecryptAsync_EmptyString_ThrowsArgumentNullException()
    {
        // Act & Assert
        await FluentActions.Invoking(() => _sut.DecryptAsync(string.Empty))
            .Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("ciphertext");
    }

    [Theory]
    [InlineData("short")]
    [InlineData("medium-length-credential-value-with-special-chars-!@#$%")]
    [InlineData("Very long string that contains a lot of characters to test encryption with larger payloads and ensure that the implementation can handle various string lengths without issues")]
    public async Task EncryptDecrypt_VariousStringLengths_Success(string plaintext)
    {
        // Act
        var encrypted = await _sut.EncryptAsync(plaintext);
        var decrypted = await _sut.DecryptAsync(encrypted);

        // Assert
        decrypted.Should().Be(plaintext);
    }

    [Fact]
    public async Task EncryptDecrypt_VeryLongString_Success()
    {
        // Arrange - Create a 100KB string
        var plaintext = new string('A', 100 * 1024);

        // Act
        var encrypted = await _sut.EncryptAsync(plaintext);
        var decrypted = await _sut.DecryptAsync(encrypted);

        // Assert
        decrypted.Should().Be(plaintext);
        decrypted.Length.Should().Be(100 * 1024);
    }

    [Fact]
    public async Task EncryptDecrypt_SpecialCharacters_Success()
    {
        // Arrange
        var plaintext = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~©®™€£¥¢₹";

        // Act
        var encrypted = await _sut.EncryptAsync(plaintext);
        var decrypted = await _sut.DecryptAsync(encrypted);

        // Assert
        decrypted.Should().Be(plaintext);
    }

    [Fact]
    public async Task EncryptDecrypt_UnicodeCharacters_Success()
    {
        // Arrange
        var plaintext = "Hello世界🌍Привет😀émojis";

        // Act
        var encrypted = await _sut.EncryptAsync(plaintext);
        var decrypted = await _sut.DecryptAsync(encrypted);

        // Assert
        decrypted.Should().Be(plaintext);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task DecryptAsync_InvalidBase64_ThrowsDecryptionException()
    {
        // Arrange
        var invalidCiphertext = "not-valid-base64!@#";

        // Act & Assert
        await FluentActions.Invoking(() => _sut.DecryptAsync(invalidCiphertext))
            .Should().ThrowAsync<DecryptionException>()
            .WithMessage("*decrypt*");
    }

    [Fact]
    public async Task DecryptAsync_TamperedCiphertext_ThrowsDecryptionException()
    {
        // Arrange
        var plaintext = "original-data";
        var encrypted = await _sut.EncryptAsync(plaintext);
        
        // Tamper with the ciphertext by changing a character
        var tamperedCiphertext = encrypted.Substring(0, encrypted.Length - 5) + "XXXX";

        // Act & Assert
        await FluentActions.Invoking(() => _sut.DecryptAsync(tamperedCiphertext))
            .Should().ThrowAsync<DecryptionException>();
    }

    [Fact]
    public async Task DecryptAsync_TooShortCiphertext_ThrowsDecryptionException()
    {
        // Arrange - Create a base64 string that's too short (less than nonce + tag size)
        var tooShortCiphertext = Convert.ToBase64String(new byte[10]);

        // Act & Assert
        await FluentActions.Invoking(() => _sut.DecryptAsync(tooShortCiphertext))
            .Should().ThrowAsync<DecryptionException>()
            .WithMessage("*Invalid ciphertext format*");
    }

    [Fact]
    public async Task DecryptAsync_WrongKey_ThrowsDecryptionException()
    {
        // Arrange
        var plaintext = "test-data";
        var encrypted = await _sut.EncryptAsync(plaintext);

        // Create new service with different key
        var differentKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var differentSettings = new EncryptionSettings { EncryptionKey = differentKey };
        var differentService = new CredentialEncryptionService(
            Options.Create(differentSettings),
            _loggerMock.Object
        );

        // Act & Assert
        await FluentActions.Invoking(() => differentService.DecryptAsync(encrypted))
            .Should().ThrowAsync<DecryptionException>();
    }

    [Fact]
    public void Constructor_NullSettings_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new CredentialEncryptionService(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new CredentialEncryptionService(Options.Create(_settings), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_EmptyEncryptionKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var emptySettings = new EncryptionSettings { EncryptionKey = string.Empty };

        // Act & Assert
        var act = () => new CredentialEncryptionService(
            Options.Create(emptySettings),
            _loggerMock.Object
        );
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Encryption key not configured*");
    }

    [Fact]
    public void Constructor_InvalidKeyLength_ThrowsInvalidOperationException()
    {
        // Arrange - Create a 128-bit key instead of 256-bit
        var shortKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        var invalidSettings = new EncryptionSettings { EncryptionKey = shortKey };

        // Act & Assert
        var act = () => new CredentialEncryptionService(
            Options.Create(invalidSettings),
            _loggerMock.Object
        );
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be*32 bytes*");
    }

    [Fact]
    public void Constructor_InvalidBase64Key_ThrowsInvalidOperationException()
    {
        // Arrange
        var invalidSettings = new EncryptionSettings { EncryptionKey = "not-valid-base64!@#" };

        // Act & Assert
        var act = () => new CredentialEncryptionService(
            Options.Create(invalidSettings),
            _loggerMock.Object
        );
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*valid base64*");
    }

    #endregion

    #region Security Tests

    [Fact]
    public async Task EncryptAsync_SameInput_DifferentOutputEachTime()
    {
        // Arrange
        var plaintext = "sensitive-data";
        var results = new HashSet<string>();

        // Act - Encrypt 100 times
        for (int i = 0; i < 100; i++)
        {
            results.Add(await _sut.EncryptAsync(plaintext));
        }

        // Assert - All should be unique
        results.Count.Should().Be(100, "because each encryption should produce unique output");
    }

    [Fact]
    public async Task EncryptedValue_DoesNotContainPlaintext()
    {
        // Arrange
        var plaintext = "SECRET_API_KEY";

        // Act
        var encrypted = await _sut.EncryptAsync(plaintext);

        // Assert
        encrypted.Should().NotContain(plaintext, "encrypted value should not contain plaintext");
    }

    #endregion
}
