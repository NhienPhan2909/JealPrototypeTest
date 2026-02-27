# Components

### Backend Components

#### 1. EasyCarsApiClient Service

**Responsibility:** Manages all HTTP communication with EasyCars API endpoints including authentication, request/response handling, and error mapping.

**Key Interfaces:**
```csharp
public interface IEasyCarsApiClient
{
    Task<string> RequestTokenAsync(string clientId, string clientSecret, string environment, int dealershipId, CancellationToken ct);
    Task<StockResponse> GetAdvertisementStocksAsync(string accountNumber, string accountSecret, string environment, int dealershipId, string? yardCode, CancellationToken ct);
    Task<bool> TestConnectionAsync(string clientId, string clientSecret, string accountNumber, string accountSecret, string environment, CancellationToken ct);
}
```

**EasyCars API Endpoints (per official documentation v1.01):**

| Endpoint | Method | Purpose | Auth |
|----------|--------|---------|------|
| `/StockService/RequestToken?ClientID=X&ClientSecret=Y` | POST | Obtain JWT token | Query params (no body) |
| `/StockService/GetAdvertisementStocks` | POST | Retrieve vehicle stock | Bearer JWT + JSON body `{AccountNumber, AccountSecret, YardCode}` |

**Base URLs:**
- Test: `https://testmy.easycars.com.au/TestECService`
- Production: `https://my.easycars.net.au/ECService`

**Dependencies:**
- `IHttpClientFactory` - Create configured HttpClient instances
- `ILogger<EasyCarsApiClient>` - Structured logging
- `IConfiguration` - API base URLs and timeouts

**Implementation Details:**
- Token request uses query parameters (`?ClientID=X&ClientSecret=Y`), empty POST body
- Stock request sends `{AccountNumber, AccountSecret, YardCode}` as JSON body with Bearer token header
- Caches JWT tokens until 1 minute before expiry; keyed by `{ClientId}:{Environment}:{DealershipId}`
- EasyCars responses use `"Code"` field (not `"ResponseCode"`) — success when `Code == 0`
- Logs all requests at Debug level, errors at Error level
- Configurable base URLs: `EasyCars:TestApiUrl` and `EasyCars:ProductionApiUrl`

---

#### 2. CredentialEncryptionService

**Responsibility:** Encrypts and decrypts dealership EasyCars credentials using AES-256-GCM with secure key management.

**Key Interfaces:**
```csharp
public interface ICredentialEncryptionService
{
    EncryptedCredential Encrypt(string plaintext);
    string Decrypt(string ciphertext, string iv);
}

public class EncryptedCredential
{
    public string Ciphertext { get; set; }
    public string InitializationVector { get; set; }
}
```

**Dependencies:**
- `IConfiguration` - Encryption key from environment variables

**Implementation Details:**
- Retrieves encryption key from `EasyCarsEncryption:MasterKey` environment variable
- Generates unique IV for each encryption operation
- Uses AES-256-GCM for authenticated encryption (prevents tampering)
- Key rotation: supports multiple keys with version prefix (future enhancement)
- Throws `CryptographicException` on decryption failures

**Security Considerations:**
- Master key must be 32 bytes (256 bits), stored in secure key vault (Azure Key Vault, AWS KMS)
- Never log plaintext credentials or encryption keys
- IVs stored alongside ciphertext in database, not sensitive
- Key rotation strategy documented in deployment guide

---

#### 3. EasyCarsStockMapper Service

**Responsibility:** Maps between EasyCars Stock API response models and Vehicle domain entities, handling data transformations and validation.

**Key Interfaces:**
```csharp
public interface IEasyCarsStockMapper
{
    Vehicle MapToVehicle(StockItem stockItem, int dealershipId);
    void UpdateVehicleFromStock(Vehicle vehicle, StockItem stockItem);
}
```

**Dependencies:**
- None (pure mapping logic)

**Implementation Details:**
- Maps Make, Model, YearGroup → Year, Price, Odometer → Mileage
- Converts EasyCars VIN, RegoNum, StockNumber to entity fields
- Maps Body, Colour → ExteriorColor, InteriorColor, FuelType, GearType
- Stores complete StockItem as JSON in `EasyCarsRawData`
- Sets `DataSource = DataSourceType.EasyCars`
- Handles null/empty fields gracefully with sensible defaults
- Uses `StockNumber` as unique identifier for duplicate detection
- Creates vehicle title from Make + Model + Badge + YearGroup
- Maps ImageURLs (comma-separated string) to vehicle Images list by splitting on ','

---

#### 4. EasyCarsLeadMapper Service

**Responsibility:** Bi-directional mapping between Lead domain entities and EasyCars Lead API models.

**Key Interfaces:**
```csharp
public interface IEasyCarsLeadMapper
{
    CreateLeadRequest MapToCreateLeadRequest(Lead lead, Vehicle? vehicle);
    UpdateLeadRequest MapToUpdateLeadRequest(Lead lead, string leadNumber, Vehicle? vehicle);
    Lead MapToLead(LeadDetailResponse response, int dealershipId);
    void UpdateLeadFromResponse(Lead lead, LeadDetailResponse response);
}
```

**Dependencies:**
- None (pure mapping logic)

**Implementation Details:**
- Maps Lead → CreateLeadRequest with CustomerName, CustomerEmail, CustomerPhone, CustomerMobile
- Includes vehicle details if VehicleId populated: VehicleMake, VehicleModel, VehicleYear, VehiclePrice
- Maps enums: VehicleType (1-6), VehicleInterest (1-5), FinanceStatus (1-3), Rating (1-3)
- Stores complete LeadDetailResponse as JSON in `EasyCarsRawData`
- Sets `DataSource = DataSourceType.EasyCars` for inbound leads
- Uses `LeadNumber` as unique identifier for duplicate detection

---

#### 5. EasyCarsStockSyncService

**Responsibility:** Orchestrates the complete stock synchronization workflow from API retrieval to database persistence with error handling and logging.

**Key Interfaces:**
```csharp
public interface IEasyCarsStockSyncService
{
    Task<SyncResult> SyncStockAsync(int dealershipId, CancellationToken ct);
}

public class SyncResult
{
    public bool Success { get; set; }
    public int RecordsProcessed { get; set; }
    public int RecordsCreated { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsFailed { get; set; }
    public List<string> Errors { get; set; }
}
```

**Dependencies:**
- `IEasyCarsApiClient` - API communication
- `IEasyCarsCredentialRepository` - Retrieve credentials
- `IVehicleRepository` - Vehicle persistence
- `IEasyCarsSyncLogRepository` - Audit logging
- `ICredentialEncryptionService` - Decrypt credentials
- `IEasyCarsStockMapper` - Data mapping
- `IImageDownloadService` - Vehicle image sync
- `ILogger<EasyCarsStockSyncService>` - Logging

**Implementation Details:**
1. Retrieve and decrypt dealership credentials
2. Request JWT token from EasyCars API
3. Call GetAdvertisementStocks with optional YardCode filter
4. For each StockItem:
   - Check for existing vehicle by EasyCarsStockNumber or VIN
   - Create new or update existing vehicle using mapper
   - Trigger image download for ImageURLs
   - Handle individual item failures without stopping batch
5. Wrap operations in database transaction
6. Create EasyCarsSyncLog entry with results
7. Return SyncResult summary

**Error Handling:**
- Retries transient API failures (network errors, timeouts)
- Logs per-vehicle failures but continues processing
- Rolls back database transaction on critical failures
- Captures exception details in sync log

---

#### 6. EasyCarsLeadSyncService

**Responsibility:** Orchestrates bi-directional lead synchronization including outbound (local → EasyCars) and inbound (EasyCars → local) operations.

**Key Interfaces:**
```csharp
public interface IEasyCarsLeadSyncService
{
    Task<SyncResult> SyncLeadToEasyCarsAsync(int leadId, CancellationToken ct);
    Task<SyncResult> SyncLeadsFromEasyCarsAsync(int dealershipId, CancellationToken ct);
}
```

**Dependencies:**
- `IEasyCarsApiClient` - API communication
- `IEasyCarsCredentialRepository` - Retrieve credentials
- `ILeadRepository` - Lead persistence
- `IVehicleRepository` - Retrieve vehicle details for leads
- `IEasyCarsSyncLogRepository` - Audit logging
- `ICredentialEncryptionService` - Decrypt credentials
- `IEasyCarsLeadMapper` - Data mapping
- `ILogger<EasyCarsLeadSyncService>` - Logging

**Implementation Details - Outbound (SyncLeadToEasyCarsAsync):**
1. Retrieve lead and associated vehicle
2. Retrieve and decrypt dealership credentials
3. Request JWT token from EasyCars API
4. Map lead to CreateLeadRequest or UpdateLeadRequest
5. Call EasyCars CreateLead or UpdateLead API
6. Store returned LeadNumber and CustomerNo in lead entity
7. Update `LastSyncedToEasyCars` timestamp
8. Create sync log entry

**Implementation Details - Inbound (SyncLeadsFromEasyCarsAsync):**
1. Retrieve and decrypt dealership credentials
2. Request JWT token from EasyCars API
3. Query EasyCars for recent leads (implementation depends on API capability)
4. For each lead response:
   - Check for existing lead by EasyCarsLeadNumber
   - Create new or update existing lead using mapper
   - Match to vehicle by StockNumber if provided
5. Create sync log entry with results

**Trigger Mechanisms:**
- **Outbound:** Triggered automatically when new lead created via event handler
- **Inbound:** Triggered by scheduled background job (hourly)

---

#### 7. ImageDownloadService

**Responsibility:** Downloads vehicle images from EasyCars ImageURLs and stores them in the system's media storage.

**Key Interfaces:**
```csharp
public interface IImageDownloadService
{
    Task<List<string>> DownloadAndStoreImagesAsync(List<string> imageUrls, int vehicleId, CancellationToken ct);
}
```

**Dependencies:**
- `IHttpClientFactory` - HTTP client for image downloads
- `ICloudinaryImageUploadService` - Upload images to Cloudinary (existing service)
- `ILogger<ImageDownloadService>` - Logging

**Implementation Details:**
- Downloads images asynchronously from EasyCars URLs
- Uploads to Cloudinary with vehicleId tag for organization
- Returns list of Cloudinary URLs to store in Vehicle.Images
- Implements duplicate detection using image hash comparison
- Gracefully handles missing images (404) without failing entire sync
- Rate limits downloads to avoid overwhelming EasyCars servers (max 5 concurrent)
- Logs warnings for failed downloads but continues processing

---

### Background Job Components

#### 8. StockSyncBackgroundJob (Hangfire)

**Responsibility:** Scheduled background job that triggers periodic stock synchronization for all active dealerships.

**Configuration:**
- Schedule: Cron expression from `EasyCarsSync:StockSyncSchedule` (default: `0 2 * * *` - daily at 2 AM)
- Concurrency: Sequential execution per dealership with max 3 concurrent dealerships
- Timeout: 10 minutes per dealership

**Implementation:**
```csharp
public class StockSyncBackgroundJob
{
    private readonly IEasyCarsCredentialRepository _credentialRepo;
    private readonly IEasyCarsStockSyncService _syncService;
    private readonly ILogger<StockSyncBackgroundJob> _logger;

    [DisableConcurrentExecution(timeoutInSeconds: 600)]
    public async Task ExecuteAsync()
    {
        var activeCredentials = await _credentialRepo.GetActiveCredentialsAsync();
        
        foreach (var credential in activeCredentials)
        {
            try
            {
                await _syncService.SyncStockAsync(credential.DealershipId, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stock sync failed for dealership {DealershipId}", credential.DealershipId);
                // Continue to next dealership
            }
        }
    }
}
```

**Hangfire Registration:**
```csharp
RecurringJob.AddOrUpdate<StockSyncBackgroundJob>(
    "easycars-stock-sync",
    job => job.ExecuteAsync(),
    Configuration["EasyCarsSync:StockSyncSchedule"] ?? "0 2 * * *"
);
```

---

#### 9. LeadSyncBackgroundJob (Hangfire)

**Responsibility:** Scheduled background job for inbound lead synchronization from EasyCars to local system.

**Configuration:**
- Schedule: Cron expression from `EasyCarsSync:LeadSyncSchedule` (default: `0 * * * *` - hourly)
- Concurrency: Sequential execution per dealership
- Timeout: 5 minutes per dealership

**Implementation:**
```csharp
public class LeadSyncBackgroundJob
{
    private readonly IEasyCarsCredentialRepository _credentialRepo;
    private readonly IEasyCarsLeadSyncService _syncService;
    private readonly ILogger<LeadSyncBackgroundJob> _logger;

    [DisableConcurrentExecution(timeoutInSeconds: 300)]
    public async Task ExecuteAsync()
    {
        var activeCredentials = await _credentialRepo.GetActiveCredentialsAsync();
        
        foreach (var credential in activeCredentials)
        {
            try
            {
                await _syncService.SyncLeadsFromEasyCarsAsync(credential.DealershipId, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lead sync failed for dealership {DealershipId}", credential.DealershipId);
                // Continue to next dealership
            }
        }
    }
}
```

---

### Application Layer Components

#### 10. Use Cases

**ManageEasyCarsCredentialsUseCase:**
- `CreateCredentialsAsync(CreateCredentialsCommand)` - Save encrypted credentials
- `UpdateCredentialsAsync(UpdateCredentialsCommand)` - Update existing credentials
- `DeleteCredentialsAsync(DeleteCredentialsCommand)` - Remove credentials
- `GetCredentialsAsync(GetCredentialsQuery)` - Retrieve metadata (no secrets)

**TestEasyCarsConnectionUseCase:**
- `ExecuteAsync(TestConnectionCommand)` - Validate credentials without saving

**TriggerStockSyncUseCase:**
- `ExecuteAsync(TriggerStockSyncCommand)` - Manually initiate stock sync

**TriggerLeadSyncUseCase:**
- `ExecuteAsync(TriggerLeadSyncCommand)` - Manually initiate lead sync

**GetSyncStatusUseCase:**
- `ExecuteAsync(GetSyncStatusQuery)` - Retrieve current sync state and history

**GetSyncLogsUseCase:**
- `ExecuteAsync(GetSyncLogsQuery)` - Paginated sync history retrieval

---

### Frontend Components

#### 11. EasyCarsSettings Component

**Responsibility:** React component for managing EasyCars credentials with test connection and save functionality.

**Key Features:**
- Form inputs: Account Number, Account Secret, Environment dropdown, Yard Code (optional)
- "Test Connection" button with loading state and success/error feedback
- "Save Credentials" button (enabled after successful test or with override checkbox)
- Displays current configuration status
- Delete credentials with confirmation modal
- Form validation with helpful error messages

**State Management:**
```typescript
interface EasyCarsSettingsState {
  credentials: EasyCarsCredential | null;
  formData: EasyCarsCredentialRequest;
  isTestingConnection: boolean;
  isSaving: boolean;
  isDeleting: boolean;
  testResult: { success: boolean; message: string } | null;
}
```

**API Integration:**
- GET `/api/admin/easycars/credentials` on mount
- POST `/api/admin/easycars/credentials/test-connection` on test button
- POST `/api/admin/easycars/credentials` on save button
- DELETE `/api/admin/easycars/credentials/{id}` on delete confirmation

---

#### 12. EasyCarsSyncDashboard Component

**Responsibility:** Real-time sync status monitoring and manual sync trigger controls.

**Key Features:**
- Status cards showing last stock sync and last lead sync with timestamps
- Visual indicators (green checkmark, red X, yellow warning) for sync status
- "Sync Stock Now" and "Sync Leads Now" buttons with loading states
- Progress indicators during active sync operations
- Recent sync logs table (last 10 operations)
- Link to full sync history page
- Auto-refresh status every 30 seconds when not syncing

**State Management:**
```typescript
interface SyncDashboardState {
  syncStatus: SyncSummary;
  isStockSyncing: boolean;
  isLeadSyncing: boolean;
  autoRefresh: boolean;
}
```

**API Integration:**
- GET `/api/admin/easycars/sync/status` on mount and every 30 seconds
- POST `/api/admin/easycars/sync/stock` on manual stock sync
- POST `/api/admin/easycars/sync/leads` on manual lead sync
- Poll sync status during active sync operations

---

#### 13. SyncHistoryLog Component

**Responsibility:** Paginated table view of historical sync operations with filtering and detail view.

**Key Features:**
- Table columns: Sync Type, Direction, Status, Started At, Duration, Records Processed, Actions
- Filters: Sync Type (Stock/Lead), Status (Success/Failed/Warning), Date range
- Pagination controls with configurable page size
- "View Details" button opens modal with full log details including errors
- Export functionality (future enhancement)

**API Integration:**
- GET `/api/admin/easycars/sync/logs?page={page}&pageSize={size}&syncType={type}&status={status}`
- GET `/api/admin/easycars/sync/logs/{id}` for detail modal

---
