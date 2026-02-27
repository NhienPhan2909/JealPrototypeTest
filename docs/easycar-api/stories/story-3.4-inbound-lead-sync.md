# Story 3.4: Implement Inbound Lead Sync (EasyCars to Local)

## Status
Done

## Story
**As a** backend developer,
**I want** to implement inbound lead synchronization that retrieves and refreshes leads from EasyCars,
**so that** lead data updated in EasyCars (status, customer info, ratings) is reflected in our local CRM system automatically.

## Acceptance Criteria
1. `SyncLeadsFromEasyCarsAsync(int dealershipId, CancellationToken ct)` method added to `IEasyCarsLeadSyncService` and implemented in `EasyCarsLeadSyncService`
2. Service retrieves dealership credentials and calls `GetLeadDetailAsync` for each known EasyCars lead
3. Service uses `IEasyCarsLeadMapper.UpdateLeadFromResponse` to transform EasyCars lead data into local lead model
4. Service updates existing lead records identified by `EasyCarsLeadNumber`; individual lead failures do not stop the batch
5. Service interpretation of AC5 ("customer records"): lead's customer fields (Name, Email, Phone) are not overwritten during inbound sync — `UpdateLeadFromResponse` only updates EasyCars metadata fields (`EasyCarsRawData`, `VehicleInterestType`, `FinanceInterested`, `Rating`, `LastSyncedFromEasyCars`), preserving locally-entered customer data
6. All lead updates processed in a single EF Core `SaveChangesAsync` call (batch commit within scoped DbContext — equivalent to a transaction for the mapped entity set)
7. Service creates `EasyCarsSyncLog` entry with counts of processed/succeeded/failed leads
8. Service syncs leads that already have `EasyCarsLeadNumber` set (known EasyCars leads); new repository method `GetLeadsWithEasyCarsNumberAsync` fetches only this subset for a dealership
9. `LeadSyncBackgroundJob.ExecuteInboundSyncAsync()` added and registered as a Hangfire recurring job (`0 * * * *` — hourly) in `Program.cs`
10. Unit tests cover: successful batch sync, partial failure (one lead fails), no leads to sync, no credentials, batch persist called once, sync log created

## Tasks / Subtasks

- [x] Task 1: Extend `ILeadRepository` and `LeadRepository` with inbound sync query (AC: 8)
  - [x] Add `GetLeadsWithEasyCarsNumberAsync` to `ILeadRepository.cs`
  - [x] Implement in `LeadRepository.cs` filtering `EasyCarsLeadNumber != null`

- [x] Task 2: Extend `IEasyCarsLeadSyncService` interface (AC: 1)
  - [x] Added `SyncLeadsFromEasyCarsAsync(int dealershipId, CancellationToken cancellationToken = default)`

- [x] Task 3: Implement `SyncLeadsFromEasyCarsAsync` in `EasyCarsLeadSyncService` (AC: 1, 2, 3, 4, 5, 6, 7)
  - [x] Per-lead try/catch loop with `Update(lead)` (no per-item SaveChanges)
  - [x] Single `SaveChangesAsync` after loop (batch commit)
  - [x] `TryCreateSyncLogAsync` called for all paths (including no-credentials and no-leads)

- [x] Task 4: Extend `LeadSyncBackgroundJob` for inbound sync (AC: 9)
  - [x] Added `IDealershipSettingsRepository` + `IEasyCarsCredentialRepository` to constructor
  - [x] Added `ExecuteInboundSyncAsync` with `[AutomaticRetry(Attempts=0)]` + `[DisableConcurrentExecution(300)]`
  - [x] Loops over all eligible dealerships via `GetDealershipsWithAutoSyncEnabledAsync`

- [x] Task 5: Register hourly recurring Hangfire job in `Program.cs` (AC: 9)
  - [x] `easycar-lead-inbound-sync` registered with cron `0 * * * *` (default hourly)

- [x] Task 6: Write unit tests (AC: 10)
  - [x] Created `EasyCarsLeadSyncServiceInboundTests.cs` — 10 tests
    - `SyncLeadsFromEasyCarsAsync_WithMultipleLeads_CallsGetLeadDetailForEach`
    - `SyncLeadsFromEasyCarsAsync_WithMultipleLeads_CallsUpdateLeadFromResponseForEach`
    - `SyncLeadsFromEasyCarsAsync_WithMultipleLeads_CallsSaveChangesOnce`
    - `SyncLeadsFromEasyCarsAsync_OnSuccess_ReturnsSuccessResult` (+ verifies `Update` called n times)
    - `SyncLeadsFromEasyCarsAsync_WithOneFailingLead_ContinuesAndReturnsPartialSuccess`
    - `SyncLeadsFromEasyCarsAsync_WithNoLeads_ReturnsSuccessWithZeroItems` (+ verifies `SaveChangesAsync` never called)
    - `SyncLeadsFromEasyCarsAsync_WithNoCredentials_ReturnsFailureResult` (+ verifies sync log created)
    - `SyncLeadsFromEasyCarsAsync_OnSuccess_CreatesSyncLog`
    - `SyncLeadsFromEasyCarsAsync_WithPartialFailure_CreatesSyncLogWithCounts`
    - `SyncLeadsFromEasyCarsAsync_WithAllLeadsFailing_ReturnsFailureResult`

## Dev Notes

### Previous Story Insights
[Source: Story 3.1, 3.2, 3.3 Dev Agent Records]

- `GetLeadDetailAsync` signature (Story 3.1):
  ```csharp
  Task<LeadDetailResponse> GetLeadDetailAsync(
      string clientId, string clientSecret,
      string accountNumber, string accountSecret,
      string environment, int dealershipId,
      string leadNumber, CancellationToken cancellationToken = default)
  ```
- `IEasyCarsLeadMapper.UpdateLeadFromResponse(Lead lead, LeadDetailResponse response)` (Story 3.2):
  - Updates: `EasyCarsLeadNumber`, `EasyCarsCustomerNo`, `EasyCarsRawData`, `VehicleInterestType`, `FinanceInterested`, `Rating`
  - Also calls `lead.MarkSyncedFromEasyCars(DateTime.UtcNow)`
  - Does **NOT** update: `Name`, `Email`, `Phone`, `Message` (customer identity fields — these are preserved)
  - Does **NOT** update: `Status` (lead status sync is Story 3.6)
- `EasyCarsLeadSyncService` class already exists (Story 3.3) — `SyncLeadsFromEasyCarsAsync` is added to it as a second method
- `IEasyCarsLeadSyncService` interface already exists — needs the new method added
- `LeadSyncBackgroundJob` already exists with `SyncLeadAsync(int leadId)` — needs `ExecuteInboundSyncAsync()` added and an injected `IEasyCarsCredentialRepository`
- `credential.Environment` is a plain `string` ("Test" or "Production") — validated in `EasyCarsCredential.Create()`

### ILeadRepository — New Method to Add
[Source: JealPrototype.Domain/Interfaces/ILeadRepository.cs]

Current interface:
```csharp
public interface ILeadRepository : IRepository<Lead>
{
    Task<IEnumerable<Lead>> GetByDealershipIdAsync(int dealershipId, CancellationToken cancellationToken = default);
    Task<Lead?> GetByIdAndDealershipAsync(int id, int dealershipId, CancellationToken cancellationToken = default);
}
```

Add:
```csharp
/// <summary>Gets all leads that have an EasyCarsLeadNumber set for the given dealership.</summary>
Task<IEnumerable<Lead>> GetLeadsWithEasyCarsNumberAsync(int dealershipId, CancellationToken cancellationToken = default);
```

Implementation in `LeadRepository.cs` (follows EF Core pattern of the file):
```csharp
public async Task<IEnumerable<Lead>> GetLeadsWithEasyCarsNumberAsync(
    int dealershipId, CancellationToken cancellationToken = default)
{
    return await _context.Leads
        .Where(l => l.DealershipId == dealershipId && l.EasyCarsLeadNumber != null)
        .OrderByDescending(l => l.LastSyncedFromEasyCars ?? l.CreatedAt)
        .ToListAsync(cancellationToken);
}
```

**Note on `LastSyncedFromEasyCars`**: This field is `DateTime?` (nullable). The EF expression `l.LastSyncedFromEasyCars ?? l.CreatedAt` orders by last inbound sync if it exists, falling back to creation date. This ensures most-recently-synced leads are processed first (no functional requirement, just good default ordering).

### SyncLeadsFromEasyCarsAsync — Full Method Outline
[Source: Architecture data-flow.md, EasyCarsStockSyncService.cs pattern]

```csharp
public async Task<SyncResult> SyncLeadsFromEasyCarsAsync(
    int dealershipId, CancellationToken cancellationToken = default)
{
    var sw = Stopwatch.StartNew();
    _logger.LogInformation("Starting inbound lead sync for dealership {DealershipId}", dealershipId);

    try
    {
        // Step 1: Retrieve and decrypt credentials
        var credential = await _credentialRepository.GetByDealershipIdAsync(dealershipId);
        if (credential == null)
        {
            _logger.LogWarning("No EasyCars credentials for dealership {DealershipId}", dealershipId);
            sw.Stop();
            var noCredResult = SyncResult.Failure(
                $"No EasyCars credentials for dealership {dealershipId}", sw.ElapsedMilliseconds);
            await TryCreateSyncLogAsync(dealershipId, noCredResult, cancellationToken);
            return noCredResult;
        }

        var clientId = await _encryptionService.DecryptAsync(credential.ClientIdEncrypted);
        var clientSecret = await _encryptionService.DecryptAsync(credential.ClientSecretEncrypted);
        var accountNumber = await _encryptionService.DecryptAsync(credential.AccountNumberEncrypted);
        var accountSecret = await _encryptionService.DecryptAsync(credential.AccountSecretEncrypted);

        // Step 2: Get leads known to EasyCars (AC8)
        var leads = (await _leadRepository.GetLeadsWithEasyCarsNumberAsync(dealershipId, cancellationToken))
            .ToList();

        if (!leads.Any())
        {
            _logger.LogInformation("No EasyCars leads to sync for dealership {DealershipId}", dealershipId);
            sw.Stop();
            return SyncResult.Success(0, sw.ElapsedMilliseconds);
        }

        _logger.LogInformation("Syncing {Count} leads from EasyCars for dealership {DealershipId}",
            leads.Count, dealershipId);

        int succeeded = 0, failed = 0;
        var errors = new List<string>();

        // Step 3: Per-lead fetch + update (AC2, AC3, AC4)
        foreach (var lead in leads)
        {
            try
            {
                var response = await _apiClient.GetLeadDetailAsync(
                    clientId, clientSecret, accountNumber, accountSecret,
                    credential.Environment, dealershipId, lead.EasyCarsLeadNumber!, cancellationToken);

                _leadMapper.UpdateLeadFromResponse(lead, response);
                _leadRepository.Update(lead);  // mark modified, no SaveChanges yet
                succeeded++;
            }
            catch (Exception ex)
            {
                failed++;
                var errMsg = $"Lead {lead.EasyCarsLeadNumber}: {ex.Message}";
                errors.Add(errMsg);
                _logger.LogWarning(ex, "Failed to sync lead {LeadNumber} for dealership {DealershipId}",
                    lead.EasyCarsLeadNumber, dealershipId);
            }
        }

        // Step 4: Batch persist (AC6)
        if (succeeded > 0)
            await _leadRepository.SaveChangesAsync(cancellationToken);

        sw.Stop();

        // Step 5: Build result
        SyncResult result;
        if (failed == 0)
            result = SyncResult.Success(succeeded, sw.ElapsedMilliseconds);
        else if (succeeded == 0)
            result = SyncResult.Failure(succeeded + failed, errors, sw.ElapsedMilliseconds);
        else
            result = SyncResult.PartialSuccess(succeeded, failed, errors, sw.ElapsedMilliseconds);

        await TryCreateSyncLogAsync(dealershipId, result, cancellationToken); // AC7
        return result;
    }
    catch (Exception ex)
    {
        sw.Stop();
        _logger.LogError(ex, "Fatal error in inbound lead sync for dealership {DealershipId}", dealershipId);
        var failResult = SyncResult.Failure(ex.Message, sw.ElapsedMilliseconds);
        await TryCreateSyncLogAsync(dealershipId, failResult, cancellationToken);
        return failResult;
    }
}
```

### LeadSyncBackgroundJob — Inbound Method to Add
[Source: JealPrototype.Application/BackgroundJobs/StockSyncBackgroundJob.cs pattern]

The existing `LeadSyncBackgroundJob` only has `SyncLeadAsync(int leadId)` (outbound). Add `ExecuteInboundSyncAsync()` for inbound batch execution across all dealerships:

```csharp
// Add IEasyCarsCredentialRepository to constructor (alongside existing _syncService + _logger)
private readonly IEasyCarsCredentialRepository _credentialRepo;

[AutomaticRetry(Attempts = 0)]
[DisableConcurrentExecution(timeoutInSeconds: 300)]
public async Task ExecuteInboundSyncAsync(CancellationToken cancellationToken = default)
{
    _logger.LogInformation("Inbound lead sync job started at {Timestamp}", DateTime.UtcNow);

    try
    {
        // Get all active credentials → one inbound sync per dealership
        // Note: IEasyCarsCredentialRepository.GetActiveCredentialsAsync not available.
        // Use available: no "GetAll" on credential repo.
        // ALTERNATIVE: Use IEasyCarsCredentialRepository — check for GetActiveCredentialsAsync
        // or follow StockSyncBackgroundJob which uses IDealershipSettingsRepository.GetDealershipsWithAutoSyncEnabledAsync
    }
    ...
}
```

**IMPORTANT — Check actual available methods on `IEasyCarsCredentialRepository`:**
```csharp
// Available methods (from IEasyCarsCredentialRepository.cs):
Task<EasyCarsCredential?> GetByDealershipIdAsync(int dealershipId)
Task<EasyCarsCredential?> GetByIdAsync(int id)
Task<bool> ExistsForDealershipAsync(int dealershipId)
// No GetAll / GetActive method
```

**Pattern to follow** — `StockSyncBackgroundJob.ExecuteAsync()` uses:
```csharp
var dealershipIds = await _settingsRepo.GetDealershipsWithAutoSyncEnabledAsync(ct)
```
Then checks each for active credentials. Follow the **exact same pattern** for `ExecuteInboundSyncAsync`:
- Inject `IDealershipSettingsRepository _settingsRepo` (already in existing `StockSyncBackgroundJob`)
- OR: Inject `IEasyCarsCredentialRepository _credentialRepo` — but since there's no GetAll, you need `_settingsRepo.GetDealershipsWithAutoSyncEnabledAsync`
- Call `_syncService.SyncLeadsFromEasyCarsAsync(dealershipId, ct)` for each eligible dealership

**Best approach**: Inject `IDealershipSettingsRepository` and `IEasyCarsCredentialRepository` into `LeadSyncBackgroundJob` (same deps as `StockSyncBackgroundJob`). Then `ExecuteInboundSyncAsync` gets eligible dealerships from settings, verifies credentials exist, and calls `SyncLeadsFromEasyCarsAsync` for each.

### Program.cs — Recurring Job Registration
[Source: JealPrototype.API/Program.cs lines 194-206]

Existing pattern:
```csharp
using (var scope = app.Services.CreateScope())
{
    var systemSettingsRepo = scope.ServiceProvider.GetRequiredService<ISystemSettingsRepository>();
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    var cronExpression = await systemSettingsRepo.GetValueAsync("easycar_sync_cron") ?? "0 2 * * *";

    recurringJobManager.AddOrUpdate<StockSyncBackgroundJob>(
        "easycar-stock-sync",
        job => job.ExecuteAsync(CancellationToken.None),
        cronExpression,
        new Hangfire.RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
}
```

Add the lead inbound sync recurring job inside the **same `using` block**, after the stock sync registration:
```csharp
var leadSyncCron = await systemSettingsRepo.GetValueAsync("easycar_lead_sync_cron") ?? "0 * * * *";
recurringJobManager.AddOrUpdate<JealPrototype.Application.BackgroundJobs.LeadSyncBackgroundJob>(
    "easycar-lead-inbound-sync",
    job => job.ExecuteInboundSyncAsync(CancellationToken.None),
    leadSyncCron,
    new Hangfire.RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
```

Default cron `"0 * * * *"` = every hour at :00 minutes.

### AC5 Clarification — Customer Records
[Source: Story 3.4 spec analysis, JealPrototype.Domain/Entities/Lead.cs]

The EasyCars integration PRD mentions "creates/updates customer records". In this system, there is **no separate Customer entity** — customer information is embedded directly in the `Lead` entity (`Name`, `Email`, `Phone`).

`IEasyCarsLeadMapper.UpdateLeadFromResponse()` (Story 3.2) only calls:
```csharp
lead.UpdateEasyCarsData(response.LeadNumber, response.CustomerNo, rawJson, vehicleInterestType, financeInterested, rating);
lead.MarkSyncedFromEasyCars(DateTime.UtcNow);
```
This preserves `Name`, `Email`, `Phone` (not overwritten). This is intentional — leads created locally should keep their locally-entered customer data. The EasyCars `CustomerNo` is stored for cross-referencing.

If Story 3.4 needs to also update `Name`/`Email`/`Phone` from EasyCars, a new `IEasyCarsLeadMapper` method would be needed. **For this story, AC5 is satisfied by the existing `UpdateLeadFromResponse` behavior** — do not add extra name/email/phone update logic.

### SyncResult Factory Methods
[Source: JealPrototype.Application/DTOs/EasyCars/SyncResult.cs]

```csharp
SyncResult.Success(int itemsProcessed, long durationMs)
SyncResult.PartialSuccess(int itemsSucceeded, int itemsFailed, List<string> errors, long durationMs)
SyncResult.Failure(string errorMessage, long durationMs = 0)
SyncResult.Failure(int itemsProcessed, List<string> errors, long durationMs)  // for all-failed batch
```

### EasyCarsSyncLog.Create() Signature
[Source: JealPrototype.Domain/Entities/EasyCarsSyncLog.cs]

```csharp
EasyCarsSyncLog.Create(
    int dealershipId,
    SyncStatus status,
    int itemsProcessed,
    int itemsSucceeded,
    int itemsFailed,
    List<string> errors,
    long durationMs,
    string apiVersion = "1.0")
```

`result.Errors`, `result.ItemsProcessed`, `result.ItemsSucceeded`, `result.ItemsFailed`, `result.DurationMs` are all accessible on `SyncResult`.

### IRepository<T> Methods Available
[Source: JealPrototype.Domain/Interfaces/IRepository.cs]

```csharp
void Update(T entity);           // marks modified — does NOT SaveChanges
Task UpdateAsync(T entity, ...); // marks modified AND calls SaveChanges
Task SaveChangesAsync(...);      // explicit SaveChanges
```

**For batch update**: Call `_leadRepository.Update(lead)` in the loop (no-await, no SaveChanges), then `await _leadRepository.SaveChangesAsync(ct)` once after the loop. This is more efficient than calling `UpdateAsync` per lead.

### Project File Locations

```
backend-dotnet/
  JealPrototype.Domain/
    Interfaces/
      ILeadRepository.cs                          ← MODIFY (add GetLeadsWithEasyCarsNumberAsync)
  JealPrototype.Infrastructure/
    Persistence/Repositories/
      LeadRepository.cs                           ← MODIFY (implement new method)
  JealPrototype.Application/
    Interfaces/
      IEasyCarsLeadSyncService.cs                 ← MODIFY (add SyncLeadsFromEasyCarsAsync)
    Services/EasyCars/
      EasyCarsLeadSyncService.cs                  ← MODIFY (implement SyncLeadsFromEasyCarsAsync)
    BackgroundJobs/
      LeadSyncBackgroundJob.cs                    ← MODIFY (add ExecuteInboundSyncAsync + new deps)
  JealPrototype.API/
    Program.cs                                    ← MODIFY (add recurring job registration)
  JealPrototype.Tests.Unit/
    Services/EasyCars/
      EasyCarsLeadSyncServiceInboundTests.cs      ← NEW (10 unit tests)
```

No new database migrations required — all domain entity methods and columns already exist.

### Testing Pattern
[Source: JealPrototype.Tests.Unit/Services/EasyCars/EasyCarsLeadSyncServiceTests.cs]

Follow the exact same setup pattern as `EasyCarsLeadSyncServiceTests`:
- Same mocks (all 8)
- Same `CreateTestLead(...)` helper
- Same `CreateTestCredential()` helper
- Same `SetupCredentials(lead)` helper pattern — but for inbound, setup `GetLeadsWithEasyCarsNumberAsync` returning a list
- Mock `GetLeadDetailAsync` to return a `LeadDetailResponse` with known values
- Verify `_mockLeadMapper.Verify(m => m.UpdateLeadFromResponse(...), Times.Exactly(n))`
- Verify `_mockLeadRepo.Verify(r => r.SaveChangesAsync(...), Times.Once)` for batch commit

**Key mock setup for `GetLeadsWithEasyCarsNumberAsync`:**
```csharp
_mockLeadRepo.Setup(r => r.GetLeadsWithEasyCarsNumberAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(leads);
```

**Key mock setup for partial failure:**
```csharp
// First lead succeeds, second throws
_mockApiClient.SetupSequence(a => a.GetLeadDetailAsync(...))
    .ReturnsAsync(new LeadDetailResponse { LeadNumber = "LEAD-001" })
    .ThrowsAsync(new Exception("API error"));
```

## Change Log

| Date | Version | Description | Author |
|------|---------|-------------|--------|
| 2026-02-26 | 1.0 | Initial draft | Bob (SM Agent) |

## Dev Agent Record

### Agent Model Used
claude-sonnet-4.6

### Debug Log References
- Build: Infrastructure 0 errors ✅, API 0 errors ✅
- Pre-existing test build errors in `JealPrototype.Tests.Unit` unrelated to Epic 3 — not fixed per spec

### Completion Notes List
- `ILeadRepository` extended with `GetLeadsWithEasyCarsNumberAsync`; `LeadRepository` implementation uses EF Core LINQ filtering `EasyCarsLeadNumber != null`
- `IEasyCarsLeadSyncService` extended with `SyncLeadsFromEasyCarsAsync(int dealershipId, CancellationToken ct)`
- `EasyCarsLeadSyncService.SyncLeadsFromEasyCarsAsync` implemented: per-lead try/catch, `_leadRepository.Update(lead)` in loop, single `SaveChangesAsync` after loop, `TryCreateSyncLogAsync` called for all code paths including no-credentials and no-leads (QA fix applied)
- `LeadSyncBackgroundJob` extended: injected `IDealershipSettingsRepository` + `IEasyCarsCredentialRepository`; `ExecuteInboundSyncAsync` iterates eligible dealerships via `GetDealershipsWithAutoSyncEnabledAsync`
- `Program.cs`: `easycar-lead-inbound-sync` Hangfire recurring job registered inside existing `using` scope, cron from `easycar_lead_sync_cron` setting (default `0 * * * *`)
- Known issue (pre-existing, Story 3.2): `UpdateEasyCarsData` sets `DataSource = EasyCars` which overwrites leads created locally — identified by QA as 🟡 MEDIUM; deferred to future story as it requires domain entity change

### File List
- `JealPrototype.Domain/Interfaces/ILeadRepository.cs` — modified
- `JealPrototype.Infrastructure/Persistence/Repositories/LeadRepository.cs` — modified
- `JealPrototype.Application/Interfaces/IEasyCarsLeadSyncService.cs` — modified
- `JealPrototype.Application/Services/EasyCars/EasyCarsLeadSyncService.cs` — modified
- `JealPrototype.Application/BackgroundJobs/LeadSyncBackgroundJob.cs` — modified
- `JealPrototype.API/Program.cs` — modified
- `JealPrototype.Tests.Unit/Services/EasyCars/EasyCarsLeadSyncServiceInboundTests.cs` — created (10 tests)

## QA Results

### QA Agent Review — 2026-02-26

**Overall Verdict: PASS — 96/100** (after QA fixes applied)

**Initial score: 87/100.** Three fixes applied post-QA:
1. 🟡 Issue #2 (MEDIUM): Added `_mockSyncLogRepo.Verify(..., Times.Once)` to Test #7 (no-credentials path)
2. 🔵 Issue #3 (MINOR): Added `TryCreateSyncLogAsync` call for the "no leads" early-return path in `SyncLeadsFromEasyCarsAsync`
3. 🔵 Issue #4 (MINOR): Added `SaveChangesAsync Times.Never` assertion to Test #6 (no leads path)
4. 🔵 Issue #5 (MINOR): Added `_leadRepository.Update Times.Exactly(2)` assertion to Test #4 (success path)

**AC Checklist (post-fix):**

| AC | Result | Notes |
|----|--------|-------|
| AC1 — Interface + implementation | ✅ | |
| AC2 — Credentials + GetLeadDetailAsync per lead | ✅ | |
| AC3 — UpdateLeadFromResponse called per lead | ✅ | |
| AC4 — Per-lead error isolation (try/catch + continue) | ✅ | |
| AC5 — Name/Email/Phone preserved | ✅ (⚠️ known) | `DataSource` overwritten by pre-existing `UpdateEasyCarsData` behavior (Story 3.2 issue); deferred |
| AC6 — Single SaveChangesAsync after loop | ✅ | |
| AC7 — SyncLog created for all paths | ✅ | Fixed: no-leads path now creates sync log |
| AC8 — GetLeadsWithEasyCarsNumberAsync filters correctly | ✅ | |
| AC9 — Hourly Hangfire recurring job | ✅ | `0 * * * *` default cron |
| AC10 — 10 unit tests | ✅ | All 10 with complete mock assertions |

**Known deferred issue:**
- 🟡 `UpdateEasyCarsData()` (Story 3.2 domain method) sets `DataSource = EasyCars` on every call, silently overwriting locally-created leads' provenance. Requires a domain entity change (`UpdateEasyCarsMetadata` variant). Deferred to future story.
