# Encryption Service Guide

## Overview

The **EncryptionService** provides secure encryption and decryption of sensitive data using AES-256-GCM (Galois/Counter Mode) encryption. This service is critical for protecting EasyCars API credentials and other sensitive information stored in the database.

## Features

- **AES-256-GCM Encryption**: Industry-standard authenticated encryption
- **Unique IV per Encryption**: Each encryption operation generates a unique initialization vector
- **Authentication Tag**: Ensures data integrity and detects tampering
- **Clean Architecture**: Interface in Application layer, implementation in Infrastructure
- **Dependency Injection**: Registered as a scoped service
- **Configuration-based Key Management**: Keys stored in environment variables or configuration
- **Comprehensive Error Handling**: Custom exceptions with clear error messages

## Architecture

### Components

```
Application Layer:
├── IEncryptionService (Interface)
├── EncryptionException
└── DecryptionException

Infrastructure Layer:
├── EncryptionService (Implementation)
└── EncryptionSettings (Configuration)
```

### Encryption Format

The service produces Base64-encoded strings with the following structure:

```
Base64(Nonce || Tag || Ciphertext)

- Nonce: 12 bytes (96 bits) - Random IV
- Tag: 16 bytes (128 bits) - Authentication tag
- Ciphertext: Variable length - Encrypted data
```

## Configuration

### Environment Variable (Recommended for Production)

Set the encryption key as an environment variable:

```bash
# PowerShell
$env:EASYCARS_ENCRYPTION_KEY = "your-base64-encoded-256-bit-key-here"

# Linux/Mac
export EASYCARS_ENCRYPTION_KEY="your-base64-encoded-256-bit-key-here"
```

### appsettings.json (Development/Testing)

```json
{
  "EncryptionSettings": {
    "EncryptionKey": "base64-encoded-key-here",
    "KeyVersion": 1
  }
}
```

### Generating an Encryption Key

Use the following PowerShell script to generate a secure 256-bit encryption key:

```powershell
# Generate 256-bit (32 bytes) random key
$key = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Fill($key)
$base64Key = [Convert]::ToBase64String($key)
Write-Host "Generated Encryption Key: $base64Key"

# Set as environment variable
$env:EASYCARS_ENCRYPTION_KEY = $base64Key
```

Or using .NET code:

```csharp
using System.Security.Cryptography;

var key = RandomNumberGenerator.GetBytes(32); // 256 bits
var base64Key = Convert.ToBase64String(key);
Console.WriteLine($"Generated Key: {base64Key}");
```

## Usage

### Basic Encryption/Decryption

```csharp
public class MyService
{
    private readonly IEncryptionService _encryptionService;

    public MyService(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public async Task<string> SaveCredential(string accountNumber)
    {
        // Encrypt before saving to database
        var encrypted = await _encryptionService.EncryptAsync(accountNumber);
        
        // Save encrypted value to database
        await _repository.SaveAsync(encrypted);
        
        return encrypted;
    }

    public async Task<string> RetrieveCredential(string encryptedData)
    {
        // Retrieve encrypted data from database
        var encrypted = await _repository.GetAsync();
        
        // Decrypt before use
        var decrypted = await _encryptionService.DecryptAsync(encrypted);
        
        return decrypted;
    }
}
```

### Error Handling

```csharp
try
{
    var encrypted = await _encryptionService.EncryptAsync(plaintext);
}
catch (ArgumentNullException ex)
{
    // Plaintext was null or empty
    _logger.LogError(ex, "Plaintext cannot be null");
}
catch (EncryptionException ex)
{
    // Encryption failed (config issue, etc.)
    _logger.LogError(ex, "Encryption failed");
}

try
{
    var decrypted = await _encryptionService.DecryptAsync(ciphertext);
}
catch (ArgumentNullException ex)
{
    // Ciphertext was null or empty
    _logger.LogError(ex, "Ciphertext cannot be null");
}
catch (DecryptionException ex)
{
    // Decryption failed (wrong key, tampered data, invalid format)
    _logger.LogError(ex, "Decryption failed");
}
```

## Key Rotation

### Overview

Key rotation is the process of replacing the current encryption key with a new one to enhance security. The service supports key versioning through the `KeyVersion` configuration property.

### Step-by-Step Key Rotation Procedure

#### 1. Generate New Key

```powershell
# Generate new 256-bit key
$newKey = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Fill($newKey)
$newBase64Key = [Convert]::ToBase64String($newKey)
Write-Host "New Key (v2): $newBase64Key"
```

#### 2. Update Configuration

Add the new key with version 2:

```json
{
  "EncryptionSettings": {
    "EncryptionKey": "new-base64-key-here",
    "KeyVersion": 2
  }
}
```

#### 3. Re-encrypt Existing Data (Migration Script)

Create a migration script to re-encrypt all existing data:

```csharp
public class EncryptionKeyRotationService
{
    private readonly IEncryptionService _oldKeyService;
    private readonly IEncryptionService _newKeyService;
    private readonly ApplicationDbContext _context;

    public async Task RotateKeysAsync()
    {
        // Get all encrypted credentials
        var credentials = await _context.DealershipEasyCarsCredentials.ToListAsync();

        foreach (var credential in credentials)
        {
            // Decrypt with old key
            var decryptedAccount = await _oldKeyService.DecryptAsync(credential.EncryptedAccountNumber);
            var decryptedSecret = await _oldKeyService.DecryptAsync(credential.EncryptedSecretKey);

            // Re-encrypt with new key
            credential.EncryptedAccountNumber = await _newKeyService.EncryptAsync(decryptedAccount);
            credential.EncryptedSecretKey = await _newKeyService.EncryptAsync(decryptedSecret);
        }

        await _context.SaveChangesAsync();
    }
}
```

#### 4. Deploy New Key

1. **Backup database** before rotation
2. Run migration script to re-encrypt data
3. Update environment variable: `EASYCARS_ENCRYPTION_KEY`
4. Restart application
5. Verify all credentials can be decrypted

### Key Rotation Best Practices

- **Rotate keys annually** or after suspected compromise
- **Backup database** before key rotation
- **Test in staging** environment first
- **Monitor errors** during and after rotation
- **Keep old key** temporarily for rollback capability
- **Document rotation** in security logs

## Security Best Practices

### 1. Key Storage

✅ **DO:**
- Store keys in environment variables
- Use Azure Key Vault or AWS Secrets Manager in production
- Restrict access to key storage systems
- Use different keys for dev/staging/production

❌ **DON'T:**
- Hardcode keys in source code
- Commit keys to version control
- Share keys via email or chat
- Reuse keys across environments

### 2. Key Management

- **Key Length**: Always use 256-bit (32 bytes) keys
- **Key Generation**: Use cryptographically secure random number generators
- **Key Rotation**: Implement regular key rotation schedule
- **Key Backup**: Securely backup keys with restricted access

### 3. Logging

- **Never log plaintext** sensitive data
- **Never log encryption keys**
- **Log encryption/decryption events** (without data)
- **Log errors** with context but no sensitive info

Example of safe logging:

```csharp
// ✅ GOOD - No sensitive data
_logger.LogInformation("Encrypted credential for dealership {DealershipId}", dealershipId);

// ❌ BAD - Logs sensitive data
_logger.LogInformation("Encrypting: {Plaintext}", accountNumber);
```

### 4. Error Messages

- Don't expose cryptographic details in user-facing errors
- Log detailed errors server-side only
- Return generic error messages to clients

## Troubleshooting

### Error: "Encryption key is not configured"

**Cause**: Environment variable `EASYCARS_ENCRYPTION_KEY` is not set, and no key in appsettings.json

**Solution**:
```powershell
$env:EASYCARS_ENCRYPTION_KEY = "your-base64-key"
```

### Error: "Encryption key must be 256 bits"

**Cause**: Provided key is not 32 bytes (256 bits)

**Solution**: Generate a valid 256-bit key using the script in "Generating an Encryption Key" section

### Error: "Encryption key must be a valid Base64-encoded string"

**Cause**: Key is not valid Base64 format

**Solution**: Ensure key is properly Base64 encoded. Use `Convert.ToBase64String()` in .NET

### Error: "Decryption failed. The data may be corrupted or tampered with"

**Possible Causes**:
1. Wrong encryption key being used
2. Data was tampered with (authentication tag verification failed)
3. Data was corrupted in storage/transmission
4. Key was rotated but data wasn't re-encrypted

**Solution**:
- Verify correct encryption key is configured
- Check if key rotation is needed
- Restore from backup if data is corrupted
- Ensure database stores full ciphertext (no truncation)

### Error: "Ciphertext is too short"

**Cause**: Ciphertext is incomplete (less than 29 bytes)

**Solution**:
- Check database field length (should be TEXT or VARCHAR(MAX))
- Ensure no truncation during storage/retrieval
- Verify complete Base64 string is being stored

## Performance Considerations

### Benchmarks

Typical performance on modern hardware:
- **Encryption**: ~0.5-2ms per operation
- **Decryption**: ~0.5-2ms per operation
- **Throughput**: ~500-2000 operations/second

### Optimization Tips

1. **Batch Operations**: Encrypt/decrypt in parallel when possible
```csharp
var tasks = plainTextList.Select(p => _encryptionService.EncryptAsync(p));
var encrypted = await Task.WhenAll(tasks);
```

2. **Caching**: Don't repeatedly decrypt the same value
```csharp
private readonly IMemoryCache _cache;

public async Task<string> GetDecryptedCredential(string encrypted)
{
    return await _cache.GetOrCreateAsync($"decrypted_{encrypted}", async entry =>
    {
        entry.SlidingExpiration = TimeSpan.FromMinutes(5);
        return await _encryptionService.DecryptAsync(encrypted);
    });
}
```

3. **Database Indexing**: Don't index encrypted fields (they're random data)

## Testing

### Unit Tests

Run unit tests:
```bash
dotnet test backend-dotnet/JealPrototype.Tests.Unit/JealPrototype.Tests.Unit.csproj --filter "ClassName~EncryptionServiceTests"
```

Coverage includes:
- ✅ Round-trip encryption/decryption
- ✅ Unique IV generation
- ✅ Error handling (null, empty, invalid)
- ✅ Large data handling
- ✅ Unicode and special characters
- ✅ Configuration validation
- ✅ Wrong key detection
- ✅ Tampered data detection

### Integration Tests

Run integration tests:
```bash
dotnet test backend-dotnet/JealPrototype.Tests.Integration/JealPrototype.Tests.Integration.csproj --filter "ClassName~EncryptionServiceIntegrationTests"
```

Coverage includes:
- ✅ DI registration
- ✅ Configuration loading
- ✅ End-to-end scenarios
- ✅ Concurrent operations
- ✅ Performance benchmarks

### Manual Testing

Generate test key and encrypt sample data:

```powershell
# 1. Generate test key
$key = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Fill($key)
$base64Key = [Convert]::ToBase64String($key)
$env:EASYCARS_ENCRYPTION_KEY = $base64Key

# 2. Run application
cd backend-dotnet
dotnet run --project JealPrototype.API

# 3. Test via API or directly in code
```

## References

### Related Documentation
- [Story 1.2: Credential Encryption Service](../stories/story-1.2-credential-encryption-service.md)
- [Story 1.1: Database Schema](../stories/story-1.1-database-schema-easycars-credentials.md)
- [AES-GCM Standard](https://csrc.nist.gov/publications/detail/sp/800-38d/final)

### Microsoft Documentation
- [AesGcm Class](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aesgcm)
- [Data Protection in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/)

### Security Resources
- [OWASP Cryptographic Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cryptographic_Storage_Cheat_Sheet.html)
- [NIST Key Management Guidelines](https://csrc.nist.gov/projects/key-management)

## Support

For issues or questions:
1. Check this guide's troubleshooting section
2. Review unit/integration test examples
3. Consult security team for key management issues
4. Create issue in project repository

---

**Last Updated**: 2026-02-24  
**Version**: 1.0  
**Author**: BMad Dev Agent
