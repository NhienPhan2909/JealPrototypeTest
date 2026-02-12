# Secret Management Guide

## Overview

This project uses a **multi-layer secret management strategy** to ensure credentials are never committed to version control while maintaining developer productivity.

## ⚠️ CRITICAL SECURITY RULES

1. **NEVER commit secrets to Git** - All sensitive data must be in:
   - User Secrets (development)
   - Environment variables (all environments)
   - External secret managers (production)

2. **ALWAYS use template files** - `.env.example` provides documentation without exposing secrets

3. **ROTATE immediately if exposed** - See "Secret Rotation" section below

---

## Configuration Priority (High → Low)

1. **Environment Variables** - Runtime override (production/CI/CD)
2. **User Secrets** - Development only (`.NET User Secrets`)
3. **appsettings.{Environment}.json** - Non-sensitive environment config
4. **appsettings.json** - Public defaults only (NO SECRETS)

---

## Local Development Setup

### First-Time Setup

1. **Clone the repository**
   ```bash
   git clone <repo-url>
   cd JealPrototypeTest
   ```

2. **Copy environment template**
   ```bash
   # For Docker Compose
   cp .env.example .env
   
   # Edit .env with your local values
   notepad .env
   ```

3. **Initialize .NET User Secrets**
   ```bash
   cd backend-dotnet/JealPrototype.API
   dotnet user-secrets init
   ```

4. **Set required secrets**
   ```bash
   # Database
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=jeal_prototype;Username=postgres;Password=YOUR_PASSWORD"
   
   # JWT Authentication
   dotnet user-secrets set "JwtSettings:Secret" "YOUR-SECURE-RANDOM-SECRET-MIN-32-CHARS"
   
   # Cloudinary (Media Storage)
   dotnet user-secrets set "CloudinarySettings:CloudName" "your-cloud-name"
   dotnet user-secrets set "CloudinarySettings:ApiKey" "your-api-key"
   dotnet user-secrets set "CloudinarySettings:ApiSecret" "your-api-secret"
   
   # SMTP Email
   dotnet user-secrets set "EmailSettings:Username" "your-email@gmail.com"
   dotnet user-secrets set "EmailSettings:Password" "your-app-password"
   
   # Google Places API
   dotnet user-secrets set "GooglePlacesSettings:ApiKey" "your-google-api-key"
   ```

5. **Verify secrets are set**
   ```bash
   dotnet user-secrets list
   ```

### User Secrets Storage Location

**Windows:** `%APPDATA%\Microsoft\UserSecrets\<user-secrets-id>\secrets.json`

**macOS/Linux:** `~/.microsoft/usersecrets/<user-secrets-id>/secrets.json`

The `<user-secrets-id>` is stored in `JealPrototype.API.csproj` and is: `76eef1c7-c303-4867-ab44-fdce8913ab76`

---

## Production Deployment

### Environment Variables (Minimum)

For production, set these environment variables in your hosting platform:

```bash
# Database
ConnectionStrings__DefaultConnection="Host=prod-db;Database=jeal;Username=app;Password=STRONG_PASSWORD"

# JWT
JwtSettings__Secret="PRODUCTION-STRONG-RANDOM-SECRET-MIN-64-CHARS"

# Cloudinary
CloudinarySettings__CloudName="prod-cloud-name"
CloudinarySettings__ApiKey="prod-api-key"
CloudinarySettings__ApiSecret="PROD-API-SECRET"

# Email
EmailSettings__Username="production@yourcompany.com"
EmailSettings__Password="PROD-SMTP-APP-PASSWORD"

# Google Places
GooglePlacesSettings__ApiKey="PROD-GOOGLE-API-KEY"
```

**Note:** Use double underscores `__` for nested configuration in environment variables.

### Recommended: External Secret Managers

For enterprise production deployments, use:

#### Azure Key Vault (Azure deployments)
```csharp
// In Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

#### AWS Secrets Manager (AWS deployments)
```bash
# Install: dotnet add package Amazon.Extensions.Configuration.SystemsManager
```

#### HashiCorp Vault (Platform-agnostic)
```bash
# Install: dotnet add package VaultSharp
```

---

## Required Secrets Reference

| Secret | Purpose | Where to Get It |
|--------|---------|----------------|
| `ConnectionStrings:DefaultConnection` | PostgreSQL database connection | Your database server |
| `JwtSettings:Secret` | JWT token signing | Generate random 256-bit string |
| `CloudinarySettings:CloudName` | Media storage | [Cloudinary Dashboard](https://cloudinary.com/console) |
| `CloudinarySettings:ApiKey` | Media storage auth | Cloudinary Dashboard |
| `CloudinarySettings:ApiSecret` | Media storage auth | Cloudinary Dashboard |
| `EmailSettings:Username` | SMTP email sender | Your email provider |
| `EmailSettings:Password` | SMTP authentication | Gmail: [App Passwords](https://myaccount.google.com/apppasswords) |
| `GooglePlacesSettings:ApiKey` | Google Reviews feature | [Google Cloud Console](https://console.cloud.google.com/apis/credentials) |

---

## Secret Rotation

### If secrets are exposed in Git history:

1. **Immediate Actions:**
   - [ ] Rotate ALL exposed credentials immediately
   - [ ] Generate new API keys from service dashboards
   - [ ] Update all environments with new secrets
   - [ ] Monitor for unauthorized access

2. **Clean Git History (Optional):**
   ```bash
   # Option A: BFG Repo-Cleaner (Recommended)
   bfg --replace-text passwords.txt
   
   # Option B: git-filter-repo
   git filter-repo --path backend-dotnet/JealPrototype.API/appsettings.json --invert-paths
   
   # Force push (coordinate with team first!)
   git push --force --all
   ```

3. **Notify team members to re-clone**

### Rotating Individual Secrets

**JWT Secret:**
```bash
# Generate new secret (PowerShell)
$bytes = New-Object byte[] 32
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)

# Update user secret
dotnet user-secrets set "JwtSettings:Secret" "<new-secret>"
```

**Cloudinary:**
- Login to [Cloudinary Dashboard](https://cloudinary.com/console/settings/security)
- Navigate to Security → API Keys → Regenerate API Secret
- Update user secrets with new value

**Google API Key:**
- Go to [Google Cloud Console](https://console.cloud.google.com/apis/credentials)
- Click on your API key → Regenerate Key
- Update restrictions if needed
- Update user secrets

**Gmail SMTP:**
- Visit [App Passwords](https://myaccount.google.com/apppasswords)
- Revoke old password
- Generate new app-specific password
- Update user secrets

---

## Troubleshooting

### "Configuration value not found" errors

**Cause:** Missing required secret

**Solution:**
1. Check which secret is missing from error message
2. Set it with `dotnet user-secrets set "Key:Name" "value"`
3. Restart the application

### User secrets not loading

**Cause:** Not running in Development environment

**Solution:**
```bash
# Set environment
$env:ASPNETCORE_ENVIRONMENT="Development"

# Or in launchSettings.json
"environmentVariables": {
  "ASPNETCORE_ENVIRONMENT": "Development"
}
```

### Docker Compose database connection fails

**Cause:** `.env` file not created

**Solution:**
```bash
cp .env.example .env
# Edit .env with your database password
```

---

## CI/CD Integration

### GitHub Actions Example
```yaml
env:
  JwtSettings__Secret: ${{ secrets.JWT_SECRET }}
  CloudinarySettings__ApiSecret: ${{ secrets.CLOUDINARY_SECRET }}
  # ... other secrets from GitHub Secrets
```

### Azure DevOps Example
```yaml
variables:
  - group: 'production-secrets'  # Variable group with secrets
```

---

## Security Best Practices

✅ **DO:**
- Use User Secrets for local development
- Keep `.env.example` updated as documentation
- Use strong, randomly generated secrets (min 32 chars)
- Rotate secrets regularly (every 90 days)
- Use different secrets for each environment
- Monitor secret access logs

❌ **DON'T:**
- Commit `.env` files to Git
- Share secrets via email/Slack
- Use weak or predictable secrets
- Reuse secrets across environments
- Store secrets in code comments
- Log sensitive configuration values

---

## Support

For questions or issues:
1. Check this guide first
2. Verify `.env.example` matches your setup
3. Check User Secrets with `dotnet user-secrets list`
4. Contact the DevOps team

---

**Document Version:** 1.0  
**Last Updated:** 2026-02-12  
**Maintained By:** Development Team
