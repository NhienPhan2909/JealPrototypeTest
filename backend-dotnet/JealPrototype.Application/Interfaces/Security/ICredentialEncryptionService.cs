namespace JealPrototype.Application.Interfaces.Security;

/// <summary>
/// Service for encrypting and decrypting sensitive credentials using AES-256-GCM.
/// NOTE: This is an alias for IEncryptionService for Story 1.2 requirements.
/// The actual implementation was completed in Story 1.1.
/// </summary>
public interface ICredentialEncryptionService
{
    /// <summary>
    /// Encrypts plaintext using AES-256-GCM with a unique IV per operation.
    /// </summary>
    /// <param name="plaintext">The plaintext string to encrypt</param>
    /// <returns>Base64-encoded string containing IV + Tag + Ciphertext</returns>
    /// <exception cref="ArgumentNullException">Thrown when plaintext is null or empty</exception>
    /// <exception cref="Exceptions.EncryptionException">Thrown when encryption fails</exception>
    Task<string> EncryptAsync(string plaintext);

    /// <summary>
    /// Decrypts ciphertext using AES-256-GCM with authentication tag verification.
    /// </summary>
    /// <param name="ciphertext">Base64-encoded string containing IV + Tag + Ciphertext</param>
    /// <returns>The decrypted plaintext string</returns>
    /// <exception cref="ArgumentNullException">Thrown when ciphertext is null or empty</exception>
    /// <exception cref="Exceptions.DecryptionException">Thrown when decryption or authentication fails</exception>
    Task<string> DecryptAsync(string ciphertext);
}
