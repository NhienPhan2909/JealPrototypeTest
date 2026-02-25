namespace JealPrototype.Domain.Entities;

public class EasyCarsCredential : BaseEntity
{
    public int DealershipId { get; private set; }
    public string ClientIdEncrypted { get; private set; } = null!;
    public string ClientSecretEncrypted { get; private set; } = null!;
    public string AccountNumberEncrypted { get; private set; } = null!;
    public string AccountSecretEncrypted { get; private set; } = null!;
    public string EncryptionIV { get; private set; } = null!;
    public string Environment { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public string? YardCode { get; private set; }
    public DateTime? LastSyncedAt { get; private set; }

    public Dealership Dealership { get; private set; } = null!;

    private EasyCarsCredential() { }

    public static EasyCarsCredential Create(
        int dealershipId,
        string clientIdEncrypted,
        string clientSecretEncrypted,
        string accountNumberEncrypted,
        string accountSecretEncrypted,
        string encryptionIV,
        string environment,
        bool isActive = true,
        string? yardCode = null)
    {
        if (dealershipId <= 0)
            throw new ArgumentException("Invalid dealership ID", nameof(dealershipId));

        if (string.IsNullOrWhiteSpace(clientIdEncrypted))
            throw new ArgumentException("Client ID encrypted is required", nameof(clientIdEncrypted));

        if (string.IsNullOrWhiteSpace(clientSecretEncrypted))
            throw new ArgumentException("Client secret encrypted is required", nameof(clientSecretEncrypted));

        if (string.IsNullOrWhiteSpace(accountNumberEncrypted))
            throw new ArgumentException("Account number encrypted is required", nameof(accountNumberEncrypted));

        if (string.IsNullOrWhiteSpace(accountSecretEncrypted))
            throw new ArgumentException("Account secret encrypted is required", nameof(accountSecretEncrypted));

        if (string.IsNullOrWhiteSpace(encryptionIV))
            throw new ArgumentException("Encryption IV is required", nameof(encryptionIV));

        if (string.IsNullOrWhiteSpace(environment))
            throw new ArgumentException("Environment is required", nameof(environment));

        if (environment != "Test" && environment != "Production")
            throw new ArgumentException("Environment must be 'Test' or 'Production'", nameof(environment));

        return new EasyCarsCredential
        {
            DealershipId = dealershipId,
            ClientIdEncrypted = clientIdEncrypted,
            ClientSecretEncrypted = clientSecretEncrypted,
            AccountNumberEncrypted = accountNumberEncrypted,
            AccountSecretEncrypted = accountSecretEncrypted,
            EncryptionIV = encryptionIV,
            Environment = environment,
            IsActive = isActive,
            YardCode = yardCode
        };
    }

    public void UpdateCredentials(
        string clientIdEncrypted,
        string clientSecretEncrypted,
        string accountNumberEncrypted,
        string accountSecretEncrypted,
        string encryptionIV,
        string environment,
        string? yardCode = null)
    {
        if (environment != "Test" && environment != "Production")
            throw new ArgumentException("Environment must be 'Test' or 'Production'", nameof(environment));

        ClientIdEncrypted = clientIdEncrypted;
        ClientSecretEncrypted = clientSecretEncrypted;
        AccountNumberEncrypted = accountNumberEncrypted;
        AccountSecretEncrypted = accountSecretEncrypted;
        EncryptionIV = encryptionIV;
        Environment = environment;
        YardCode = yardCode;
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    public void UpdateLastSyncedAt(DateTime syncTime) => LastSyncedAt = syncTime;
}
