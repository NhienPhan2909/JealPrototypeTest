using System.Security.Cryptography;
using System.Text;
using JealPrototype.Application.Exceptions;
using JealPrototype.Application.Interfaces.Security;
using JealPrototype.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JealPrototype.Infrastructure.Security;

/// <summary>
/// Implementation of ICredentialEncryptionService using AES-256-GCM encryption.
/// Provides secure encryption/decryption with authentication for credential storage.
/// </summary>
public class CredentialEncryptionService : ICredentialEncryptionService
{
    private readonly EncryptionSettings _settings;
    private readonly ILogger<CredentialEncryptionService> _logger;
    private readonly byte[] _encryptionKey;

    // AES-GCM standard sizes
    private const int NonceSize = 12; // 96 bits (optimal for GCM)
    private const int TagSize = 16;   // 128 bits
    private const int KeySize = 32;   // 256 bits

    public CredentialEncryptionService(
        IOptions<EncryptionSettings> settings,
        ILogger<CredentialEncryptionService> logger)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Validate and cache encryption key
        ValidateAndCacheKey();
        _encryptionKey = Convert.FromBase64String(GetEncryptionKey());
    }

    /// <inheritdoc />
    public async Task<string> EncryptAsync(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
        {
            throw new ArgumentNullException(nameof(plaintext), "Plaintext cannot be null or empty");
        }

        try
        {
            // Generate unique nonce (IV) for this encryption operation
            byte[] nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(nonce);

            // Convert plaintext to bytes
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            // Prepare buffers for ciphertext and authentication tag
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TagSize];

            // Encrypt with authentication using AES-256-GCM
            using (var aesGcm = new AesGcm(_encryptionKey, TagSize))
            {
                aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);
            }

            // Combine nonce + tag + ciphertext into single byte array
            byte[] combined = new byte[NonceSize + TagSize + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, combined, 0, NonceSize);
            Buffer.BlockCopy(tag, 0, combined, NonceSize, TagSize);
            Buffer.BlockCopy(ciphertext, 0, combined, NonceSize + TagSize, ciphertext.Length);

            // Return as base64 string for storage
            string result = Convert.ToBase64String(combined);
            
            _logger.LogDebug("Successfully encrypted data of length {PlaintextLength}", plaintext.Length);
            
            return await Task.FromResult(result);
        }
        catch (Exception ex) when (ex is not EncryptionException)
        {
            _logger.LogError(ex, "Encryption operation failed");
            throw new EncryptionException("Failed to encrypt data", ex);
        }
    }

    /// <inheritdoc />
    public async Task<string> DecryptAsync(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
        {
            throw new ArgumentNullException(nameof(ciphertext), "Ciphertext cannot be null or empty");
        }

        try
        {
            // Decode base64 to byte array
            byte[] combined = Convert.FromBase64String(ciphertext);

            // Validate combined data length
            if (combined.Length < NonceSize + TagSize)
            {
                throw new DecryptionException($"Invalid ciphertext format: insufficient length");
            }

            // Extract nonce, tag, and encrypted data
            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            byte[] encryptedData = new byte[combined.Length - NonceSize - TagSize];

            Buffer.BlockCopy(combined, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(combined, NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(combined, NonceSize + TagSize, encryptedData, 0, encryptedData.Length);

            // Decrypt with authentication verification using AES-256-GCM
            byte[] plaintextBytes = new byte[encryptedData.Length];
            
            using (var aesGcm = new AesGcm(_encryptionKey, TagSize))
            {
                aesGcm.Decrypt(nonce, encryptedData, tag, plaintextBytes);
            }

            // Convert bytes back to string
            string result = Encoding.UTF8.GetString(plaintextBytes);
            
            _logger.LogDebug("Successfully decrypted data of length {CiphertextLength}", ciphertext.Length);
            
            return await Task.FromResult(result);
        }
        catch (CryptographicException ex)
        {
            _logger.LogError(ex, "Decryption failed - possible data tampering or wrong key");
            throw new DecryptionException("Failed to decrypt data - authentication failed", ex);
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Decryption failed - invalid base64 format");
            throw new DecryptionException("Failed to decrypt data - invalid ciphertext format", ex);
        }
        catch (Exception ex) when (ex is not DecryptionException)
        {
            _logger.LogError(ex, "Decryption operation failed");
            throw new DecryptionException("Failed to decrypt data", ex);
        }
    }

    /// <summary>
    /// Validates encryption configuration and throws if invalid.
    /// </summary>
    private void ValidateAndCacheKey()
    {
        string key = GetEncryptionKey();

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "Encryption key not configured. Set EASYCARS_ENCRYPTION_KEY environment variable or configure in settings.");
        }

        try
        {
            byte[] keyBytes = Convert.FromBase64String(key);
            
            if (keyBytes.Length != KeySize)
            {
                throw new InvalidOperationException(
                    $"Encryption key must be {KeySize} bytes (256 bits). Current key is {keyBytes.Length} bytes.");
            }
        }
        catch (FormatException)
        {
            throw new InvalidOperationException(
                "Encryption key must be a valid base64-encoded string.");
        }
    }

    /// <summary>
    /// Gets encryption key from environment variable or configuration.
    /// </summary>
    private string GetEncryptionKey()
    {
        // Priority: Environment variable > Configuration
        string? envKey = Environment.GetEnvironmentVariable("EASYCARS_ENCRYPTION_KEY");
        
        if (!string.IsNullOrWhiteSpace(envKey))
        {
            return envKey;
        }

        return _settings.EncryptionKey;
    }
}
