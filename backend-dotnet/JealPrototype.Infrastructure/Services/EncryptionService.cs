using System.Security.Cryptography;
using System.Text;
using JealPrototype.Application.Exceptions;
using JealPrototype.Application.Interfaces;
using JealPrototype.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JealPrototype.Infrastructure.Services;

/// <summary>
/// Implementation of IEncryptionService using AES-256-GCM encryption.
/// Provides secure encryption and decryption of sensitive data.
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly EncryptionSettings _settings;
    private readonly ILogger<EncryptionService> _logger;
    private readonly byte[] _encryptionKey;

    private const int NonceSize = 12; // 96 bits - standard for AES-GCM
    private const int TagSize = 16;   // 128 bits - authentication tag size

    public EncryptionService(IOptions<EncryptionSettings> settings, ILogger<EncryptionService> logger)
    {
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Validate and load encryption key
        if (string.IsNullOrWhiteSpace(_settings.EncryptionKey))
        {
            throw new InvalidOperationException(
                "Encryption key is not configured. Set EASYCARS_ENCRYPTION_KEY environment variable.");
        }

        try
        {
            _encryptionKey = Convert.FromBase64String(_settings.EncryptionKey);
            
            if (_encryptionKey.Length != 32) // 256 bits
            {
                throw new InvalidOperationException(
                    $"Encryption key must be 256 bits (32 bytes). Current key is {_encryptionKey.Length * 8} bits.");
            }
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "Encryption key must be a valid Base64-encoded string.", ex);
        }

        _logger.LogInformation("EncryptionService initialized with key version {KeyVersion}", 
            _settings.KeyVersion);
    }

    /// <summary>
    /// Encrypts plaintext using AES-256-GCM encryption.
    /// Each encryption generates a unique nonce (IV) for security.
    /// </summary>
    /// <param name="plaintext">The plaintext string to encrypt.</param>
    /// <returns>Base64-encoded string containing nonce + tag + ciphertext.</returns>
    public async Task<string> EncryptAsync(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            throw new ArgumentNullException(nameof(plaintext), "Plaintext cannot be null or empty.");
        }

        try
        {
            return await Task.Run(() => Encrypt(plaintext));
        }
        catch (Exception ex) when (ex is not ArgumentNullException && ex is not EncryptionException)
        {
            _logger.LogError(ex, "Encryption operation failed");
            throw new EncryptionException("Failed to encrypt data. See inner exception for details.", ex);
        }
    }

    private string Encrypt(string plaintext)
    {
        // Generate random nonce (IV) - must be unique for each encryption
        byte[] nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);

        // Convert plaintext to bytes
        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

        // Prepare buffers for ciphertext and authentication tag
        byte[] ciphertext = new byte[plaintextBytes.Length];
        byte[] tag = new byte[TagSize];

        // Perform AES-GCM encryption
        using (var aesGcm = new AesGcm(_encryptionKey, TagSize))
        {
            aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);
        }

        // Combine: nonce + tag + ciphertext
        // This format allows easy parsing during decryption
        byte[] combined = new byte[nonce.Length + tag.Length + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, combined, nonce.Length, tag.Length);
        Buffer.BlockCopy(ciphertext, 0, combined, nonce.Length + tag.Length, ciphertext.Length);

        // Return as base64 for storage in database
        return Convert.ToBase64String(combined);
    }

    /// <summary>
    /// Decrypts ciphertext that was encrypted using AES-256-GCM.
    /// Verifies authentication tag to ensure data integrity.
    /// </summary>
    /// <param name="ciphertext">Base64-encoded string containing nonce + tag + ciphertext.</param>
    /// <returns>The decrypted plaintext string.</returns>
    public async Task<string> DecryptAsync(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
        {
            throw new ArgumentNullException(nameof(ciphertext), "Ciphertext cannot be null or empty.");
        }

        try
        {
            return await Task.Run(() => Decrypt(ciphertext));
        }
        catch (Exception ex) when (ex is not ArgumentNullException && ex is not DecryptionException)
        {
            _logger.LogError(ex, "Decryption operation failed");
            throw new DecryptionException("Failed to decrypt data. See inner exception for details.", ex);
        }
    }

    private string Decrypt(string ciphertext)
    {
        byte[] combined;
        
        try
        {
            combined = Convert.FromBase64String(ciphertext);
        }
        catch (FormatException ex)
        {
            throw new DecryptionException("Invalid ciphertext format. Expected Base64-encoded string.", ex);
        }

        // Validate minimum length: nonce + tag + at least 1 byte of ciphertext
        int minLength = NonceSize + TagSize + 1;
        if (combined.Length < minLength)
        {
            throw new DecryptionException(
                $"Ciphertext is too short. Expected at least {minLength} bytes, got {combined.Length} bytes.");
        }

        // Extract components
        byte[] nonce = new byte[NonceSize];
        byte[] tag = new byte[TagSize];
        byte[] ciphertextBytes = new byte[combined.Length - NonceSize - TagSize];

        Buffer.BlockCopy(combined, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(combined, NonceSize, tag, 0, TagSize);
        Buffer.BlockCopy(combined, NonceSize + TagSize, ciphertextBytes, 0, ciphertextBytes.Length);

        // Prepare buffer for plaintext
        byte[] plaintextBytes = new byte[ciphertextBytes.Length];

        try
        {
            // Perform AES-GCM decryption with authentication
            using (var aesGcm = new AesGcm(_encryptionKey, TagSize))
            {
                aesGcm.Decrypt(nonce, ciphertextBytes, tag, plaintextBytes);
            }
        }
        catch (CryptographicException ex)
        {
            throw new DecryptionException(
                "Decryption failed. The data may be corrupted or tampered with, or the wrong key is being used.", ex);
        }

        // Convert decrypted bytes back to string
        return Encoding.UTF8.GetString(plaintextBytes);
    }
}
