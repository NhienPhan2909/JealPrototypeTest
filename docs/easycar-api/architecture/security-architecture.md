# Security Architecture

### Credential Security

**Encryption Strategy:**
- **Algorithm:** AES-256-GCM (Galois/Counter Mode)
- **Key Size:** 256 bits (32 bytes)
- **IV:** Unique per encryption operation, 96 bits (12 bytes)
- **Authentication:** GCM provides built-in authentication tag preventing tampering

**Key Management:**
1. **Master Key Storage:**
   - Stored in Azure Key Vault (Production) or AWS Secrets Manager
   - Retrieved once at application startup via Managed Identity
   - Never logged or exposed in configuration files
   - Environment variable: `EasyCarsEncryption:MasterKey`

2. **Key Rotation Strategy:**
   - Support multiple encryption keys with version prefix
   - New credentials encrypted with latest key version
   - Old credentials re-encrypted on access (lazy re-encryption)
   - Document key rotation procedure in operations manual

3. **IV Management:**
   - Generated using cryptographically secure random number generator
   - Stored alongside ciphertext in database (not sensitive)
   - Never reused for same key

**Encryption Flow:**
```
Plaintext (Account Secret) 
  → AES-256-GCM Encrypt(plaintext, masterKey, randomIV) 
  → Ciphertext + AuthTag
  → Base64 encode
  → Store in database with IV
```

**Decryption Flow:**
```
Database (Ciphertext + IV)
  → Base64 decode
  → AES-256-GCM Decrypt(ciphertext, masterKey, IV)
  → Verify AuthTag
  → Plaintext (Account Secret)
  → Use for API call
  → Clear from memory
```

---

### API Security

**Authentication:**
- All EasyCars management endpoints require valid JWT bearer token
- JWT issued by existing authentication system (JwtAuthService)
- Token contains dealershipId claim for tenant isolation
- Token expiration: 24 hours (configurable)

**Authorization:**
- Only admin users can access EasyCars management endpoints
- Authorization policy: `[Authorize(Policy = "AdminOnly")]`
- Dealership isolation enforced at data access layer

**HTTPS/TLS:**
- All EasyCars API communication over HTTPS/TLS 1.2+
- Certificate validation enabled
- No insecure HTTP fallback

**Rate Limiting:**
- Implement rate limiting on sync endpoints to prevent abuse
- Default: 5 manual sync requests per hour per dealership
- Background jobs not subject to rate limiting

**Input Validation:**
- Validate all API request payloads using FluentValidation
- Sanitize input to prevent injection attacks
- Enforce maximum lengths for string fields

---

### Data Security

**Data at Rest:**
- Database encrypted using PostgreSQL native encryption (transparent data encryption)
- EasyCars credentials additionally encrypted at application layer
- Backup encryption enabled

**Data in Transit:**
- All API communication over HTTPS
- Internal service communication over encrypted channels

**Audit Logging:**
- Log all credential management operations (create, update, delete)
- Log all sync operations with timestamps and results
- Never log plaintext credentials or encryption keys
- Log retention: 90 days for compliance

**Tenant Isolation:**
- All database queries automatically filtered by dealershipId
- No cross-tenant data access possible
- Repository pattern enforces isolation at data access layer

---

### Secrets Management

**Environment Variables:**
```bash
# Encryption
EasyCarsEncryption:MasterKey=<32-byte-base64-key-from-key-vault>

# API Configuration
EasyCarsApi:TestBaseUrl=https://testmy.easycars.com.au
EasyCarsApi:ProductionBaseUrl=https://my.easycars.net.au
EasyCarsApi:RequestTimeout=30

# Sync Configuration
EasyCarsSync:StockSyncSchedule=0 2 * * *
EasyCarsSync:LeadSyncSchedule=0 * * * *
```

**Key Vault Integration:**
```csharp
// Startup.cs or Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential()
);
```

**Deployment Checklist:**
- [ ] Generate master encryption key (32 random bytes)
- [ ] Store in Azure Key Vault or AWS Secrets Manager
- [ ] Configure Managed Identity for application
- [ ] Grant Key Vault read permissions to Managed Identity
- [ ] Test key retrieval in staging environment
- [ ] Document key rotation procedure

---
