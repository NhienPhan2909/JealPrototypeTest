namespace JealPrototype.Application.Interfaces;

/// <summary>
/// Service for encrypting and decrypting sensitive data using AES-256-GCM.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts plaintext using AES-256-GCM encryption.
    /// </summary>
    /// <param name="plaintext">The plaintext string to encrypt.</param>
    /// <returns>Base64-encoded string containing IV, ciphertext, and authentication tag.</returns>
    /// <exception cref="ArgumentNullException">Thrown when plaintext is null or empty.</exception>
    /// <exception cref="Exceptions.EncryptionException">Thrown when encryption fails.</exception>
    Task<string> EncryptAsync(string plaintext);

    /// <summary>
    /// Decrypts ciphertext that was encrypted using AES-256-GCM.
    /// </summary>
    /// <param name="ciphertext">Base64-encoded string containing IV, ciphertext, and authentication tag.</param>
    /// <returns>The decrypted plaintext string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when ciphertext is null or empty.</exception>
    /// <exception cref="Exceptions.DecryptionException">Thrown when decryption fails or authentication fails.</exception>
    Task<string> DecryptAsync(string ciphertext);
}
