# EasyCars API Integration - Quick Start Guide

**Version:** 1.0  
**Last Updated:** 2026-02-24  
**Related Documents:**
- [PRD](./EASYCARS_INTEGRATION_PRD.md)
- [Architecture](./EASYCARS_INTEGRATION_ARCHITECTURE.md)

---

## 🚀 Overview

This guide helps developers quickly understand and implement the EasyCars API integration feature. The integration syncs vehicle inventory and customer leads from EasyCars to our database for each dealership.

---

## 📋 Prerequisites

- [ ] Review PRD and Architecture documents
- [ ] Access to EasyCars API documentation PDFs
- [ ] Test credentials:
  - **PublicID:** `AA20EE61-5CFA-458D-9AFB-C4E929EA18E6`
  - **SecretKey:** `7326AF23-714A-41A5-A74F-EC77B4E4F2F2`
- [ ] .NET 8 SDK installed
- [ ] PostgreSQL database setup
- [ ] Node.js and npm for frontend development

---

## 🏗️ Implementation Phases

### Phase 1: Domain & Database Schema (Days 1-2)

**Goal:** Update domain entities and create database migration

**Tasks:**
1. **Update Dealership Entity** (`JealPrototype.Domain/Entities/Dealership.cs`)
   ```csharp
   public string? EasyCarsPublicId { get; private set; }
   public string? EasyCarsSecretKey { get; private set; }  // Encrypted
   public DateTime? LastEasyCarsSyncAt { get; private set; }
   ```

2. **Extend Vehicle Entity** with EasyCars fields:
   - `EasyCarsId` (external reference)
   - `VIN`, `StockNumber`, `ExteriorColor`, `InteriorColor`
   - `Transmission`, `FuelType`, `BodyStyle`, `Engine`, `Drivetrain`
   - `Features` (JSON array), additional fields per Stock API docs

3. **Extend Lead Entity** with EasyCars fields:
   - `EasyCarsId` (external reference)
   - `LeadNumber`, `LeadType`, `Source`
   - `PreferredContactMethod`, `Notes`, additional fields per Lead API docs

4. **Create Migration:**
   ```bash
   cd backend-dotnet/JealPrototype.API
   dotnet ef migrations add EasyCarsIntegration
   dotnet ef database update
   ```

**Acceptance Criteria:**
- ✅ All entities have EasyCarsId for tracking
- ✅ Migration runs without errors
- ✅ Dealership has credential fields
- ✅ LastEasyCarsSyncAt timestamp added

---

### Phase 2: Infrastructure Layer (Days 3-5)

**Goal:** Implement EasyCars API client services

**Tasks:**
1. **Create DTOs** (`JealPrototype.Application/DTOs/EasyCars/`)
   - `EasyCarsVehicleDto.cs` - Match Stock API response
   - `EasyCarsLeadDto.cs` - Match Lead API response
   - `EasyCarsAuthDto.cs` - Authentication request/response

2. **Create Service Interfaces** (`JealPrototype.Application/Interfaces/`)
   ```csharp
   public interface IEasyCarsStockApiService
   {
       Task<List<EasyCarsVehicleDto>> GetVehiclesAsync(string publicId, string secretKey);
       Task<EasyCarsVehicleDto?> GetVehicleByIdAsync(string publicId, string secretKey, string vehicleId);
   }
   
   public interface IEasyCarsLeadApiService
   {
       Task<List<EasyCarsLeadDto>> GetLeadsAsync(string publicId, string secretKey);
       Task<bool> CreateLeadAsync(string publicId, string secretKey, EasyCarsLeadDto lead);
       Task<bool> UpdateLeadAsync(string publicId, string secretKey, EasyCarsLeadDto lead);
   }
   ```

3. **Implement Services** (`JealPrototype.Infrastructure/Services/`)
   - `EasyCarsStockApiService.cs`
   - `EasyCarsLeadApiService.cs`
   - Use `HttpClient` with proper authentication headers
   - Add retry logic using Polly
   - Log all API calls (without exposing credentials)

4. **Add Configuration** (`appsettings.json`)
   ```json
   {
     "EasyCars": {
       "StockApiBaseUrl": "https://api.easycars.com/stock/v1",
       "LeadApiBaseUrl": "https://api.easycars.com/lead/v1",
       "TimeoutSeconds": 30,
       "MaxRetries": 3
     }
   }
   ```

**Acceptance Criteria:**
- ✅ API services successfully call EasyCars test endpoints
- ✅ Authentication works with test credentials
- ✅ Error responses are handled gracefully
- ✅ Retry logic works for transient failures

**Test Command:**
```bash
cd backend-dotnet/JealPrototype.Tests.Unit
dotnet test --filter "FullyQualifiedName~EasyCarsApiService"
```

---

### Phase 3: Application Layer (Days 6-8)

**Goal:** Create sync use cases with data mapping

**Tasks:**
1. **Create Use Cases** (`JealPrototype.Application/UseCases/EasyCars/`)
   - `SyncVehiclesFromEasyCarsUseCase.cs`
   - `SyncLeadsFromEasyCarsUseCase.cs`

2. **Implement Vehicle Sync Logic:**
   ```csharp
   public class SyncVehiclesFromEasyCarsUseCase
   {
       private readonly IEasyCarsStockApiService _stockApi;
       private readonly IVehicleRepository _vehicleRepo;
       private readonly IDealershipRepository _dealershipRepo;
       
       public async Task<SyncResult> ExecuteAsync(int dealershipId)
       {
           // 1. Get dealership credentials
           // 2. Call EasyCars Stock API
           // 3. Map DTOs to Vehicle entities
           // 4. Upsert by EasyCarsId (idempotent)
           // 5. Update LastEasyCarsSyncAt
           // 6. Return sync statistics
       }
   }
   ```

3. **Implement Lead Sync Logic:**
   - Fetch leads from EasyCars
   - Map to Lead entities
   - Upsert by EasyCarsId
   - Handle duplicate prevention

4. **Create Mapping Extensions:**
   ```csharp
   public static class EasyCarsMappingExtensions
   {
       public static Vehicle ToVehicle(this EasyCarsVehicleDto dto, int dealershipId)
       {
           // Map all fields from DTO to entity
       }
       
       public static Lead ToLead(this EasyCarsLeadDto dto, int dealershipId)
       {
           // Map all fields from DTO to entity
       }
   }
   ```

**Acceptance Criteria:**
- ✅ Use cases successfully sync test data
- ✅ Upsert logic works (no duplicates on re-run)
- ✅ All EasyCars fields are mapped correctly
- ✅ LastEasyCarsSyncAt timestamp is updated

---

### Phase 4: Background Sync Service (Days 9-10)

**Goal:** Implement periodic automatic sync

**Tasks:**
1. **Create Background Service** (`JealPrototype.Infrastructure/BackgroundServices/`)
   ```csharp
   public class EasyCarsSyncBackgroundService : BackgroundService
   {
       protected override async Task ExecuteAsync(CancellationToken stoppingToken)
       {
           while (!stoppingToken.IsCancellationRequested)
           {
               // 1. Get all dealerships with EasyCars credentials
               // 2. For each dealership, run vehicle and lead sync
               // 3. Log results
               // 4. Wait for configured interval
               await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
           }
       }
   }
   ```

2. **Register Service** (`Program.cs`)
   ```csharp
   builder.Services.AddHostedService<EasyCarsSyncBackgroundService>();
   ```

3. **Add Configuration:**
   ```json
   {
     "EasyCarsSyncOptions": {
       "SyncIntervalMinutes": 30,
       "EnableAutoSync": true,
       "MaxConcurrentSyncs": 5
     }
   }
   ```

**Acceptance Criteria:**
- ✅ Background service starts on application startup
- ✅ Periodic sync runs at configured intervals
- ✅ No sync conflicts (concurrent prevention)
- ✅ Errors are logged but don't crash service

---

### Phase 5: API Endpoints (Days 11-13)

**Goal:** Create REST endpoints for credential management and manual sync

**Tasks:**
1. **Create Controller** (`JealPrototype.API/Controllers/EasyCarsController.cs`)

2. **Implement Endpoints:**
   ```csharp
   // Update dealership credentials
   PUT /api/dealers/{id}/easycars/credentials
   Body: { "publicId": "...", "secretKey": "..." }
   
   // Manual sync vehicles
   POST /api/dealers/{id}/easycars/sync-vehicles
   Response: { "syncedCount": 150, "errors": [] }
   
   // Manual sync leads
   POST /api/dealers/{id}/easycars/sync-leads
   Response: { "syncedCount": 25, "errors": [] }
   
   // Get sync status
   GET /api/dealers/{id}/easycars/sync-status
   Response: { "lastSyncAt": "2026-02-24T09:30:00Z", "status": "success" }
   ```

3. **Add Authorization:**
   - Validate JWT token
   - Check dealership access (tenant isolation)
   - Require appropriate permissions

**Acceptance Criteria:**
- ✅ All endpoints require authentication
- ✅ Tenant isolation enforced (can only access own dealership)
- ✅ Credentials are encrypted before storage
- ✅ Manual sync returns detailed results

**Test with cURL:**
```bash
# Get auth token first
TOKEN=$(curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@test.com","password":"Test123!"}' \
  | jq -r '.token')

# Test sync
curl -X POST http://localhost:5000/api/dealers/1/easycars/sync-vehicles \
  -H "Authorization: Bearer $TOKEN"
```

---

### Phase 6: Frontend Integration (Days 14-16)

**Goal:** Add UI for credential management and sync controls

**Tasks:**
1. **Update DealerSettings Component** (`frontend/src/pages/admin/DealerSettings.jsx`)

2. **Add EasyCars Credentials Section:**
   ```jsx
   <div className="settings-section">
     <h3>EasyCars Integration</h3>
     
     <div className="form-group">
       <label>Account Number (Public ID)</label>
       <input 
         type="text" 
         value={easyCarsPublicId}
         onChange={(e) => setEasyCarsPublicId(e.target.value)}
         placeholder="AA20EE61-5CFA-458D-9AFB-C4E929EA18E6"
       />
     </div>
     
     <div className="form-group">
       <label>Account Secret</label>
       <input 
         type="password" 
         value={easyCarsSecretKey}
         onChange={(e) => setEasyCarsSecretKey(e.target.value)}
         placeholder="Enter secret key..."
       />
       <small>Secret key is encrypted and never displayed</small>
     </div>
     
     <div className="sync-controls">
       <button onClick={handleSyncVehicles}>
         Sync Vehicles Now
       </button>
       <button onClick={handleSyncLeads}>
         Sync Leads Now
       </button>
     </div>
     
     {lastSyncAt && (
       <div className="sync-status">
         Last synced: {formatDate(lastSyncAt)}
       </div>
     )}
   </div>
   ```

3. **Add API Integration:**
   ```javascript
   const handleSyncVehicles = async () => {
     setLoading(true);
     try {
       const response = await apiRequest(
         `/api/dealers/${selectedDealership.id}/easycars/sync-vehicles`,
         { method: 'POST' }
       );
       const result = await response.json();
       setSuccessMessage(`Synced ${result.syncedCount} vehicles`);
     } catch (error) {
       setError('Sync failed: ' + error.message);
     } finally {
       setLoading(false);
     }
   };
   ```

4. **Add Validation:**
   - Validate PublicID format (GUID)
   - Validate SecretKey not empty
   - Show loading spinner during sync
   - Display success/error messages

**Acceptance Criteria:**
- ✅ Credentials can be saved from admin panel
- ✅ Manual sync buttons trigger API calls
- ✅ Loading states displayed during operations
- ✅ Success/error messages shown to user
- ✅ Last sync timestamp displayed

---

### Phase 7: Testing & Documentation (Days 17-18)

**Goal:** Comprehensive testing and documentation

**Tasks:**
1. **Integration Tests** (`JealPrototype.Tests.Integration/`)
   - Test full sync flow with test credentials
   - Verify data persistence
   - Test error scenarios

2. **Unit Tests** (`JealPrototype.Tests.Unit/`)
   - Test use case logic
   - Test mapping functions
   - Test validation

3. **Manual Testing Checklist:**
   - [ ] Save EasyCars credentials in admin panel
   - [ ] Trigger manual vehicle sync
   - [ ] Trigger manual lead sync
   - [ ] Verify vehicles appear in inventory
   - [ ] Verify leads appear in CRM
   - [ ] Test with invalid credentials (should show error)
   - [ ] Verify background sync runs automatically
   - [ ] Check logs for errors

4. **Create User Documentation:**
   - How to obtain EasyCars credentials
   - Step-by-step setup guide
   - Troubleshooting common issues

**Test Commands:**
```bash
# Run all tests
cd backend-dotnet
dotnet test

# Run specific test suite
dotnet test --filter "FullyQualifiedName~EasyCars"

# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

**Acceptance Criteria:**
- ✅ All integration tests pass with test credentials
- ✅ Unit test coverage > 80%
- ✅ Manual testing checklist completed
- ✅ User documentation created

---

## 🔧 Configuration Reference

### Backend Configuration (`appsettings.json`)
```json
{
  "EasyCars": {
    "StockApiBaseUrl": "https://api.easycars.com/stock/v1",
    "LeadApiBaseUrl": "https://api.easycars.com/lead/v1",
    "TimeoutSeconds": 30,
    "MaxRetries": 3
  },
  "EasyCarsSyncOptions": {
    "SyncIntervalMinutes": 30,
    "EnableAutoSync": true,
    "MaxConcurrentSyncs": 5
  },
  "Encryption": {
    "KeyVaultUrl": "https://your-keyvault.vault.azure.net/",
    "EncryptionKeyName": "easycars-credentials-key"
  }
}
```

### Environment Variables
```bash
# Development
ASPNETCORE_ENVIRONMENT=Development
EASYCARS_TEST_PUBLIC_ID=AA20EE61-5CFA-458D-9AFB-C4E929EA18E6
EASYCARS_TEST_SECRET_KEY=7326AF23-714A-41A5-A74F-EC77B4E4F2F2

# Production
ASPNETCORE_ENVIRONMENT=Production
AZURE_KEY_VAULT_URL=https://prod-keyvault.vault.azure.net/
```

---

## 🐛 Troubleshooting

### Common Issues

**Issue:** "401 Unauthorized" from EasyCars API
- **Solution:** Verify PublicID and SecretKey are correct
- Check authentication header format
- Ensure credentials are for correct environment (test vs prod)

**Issue:** Sync completes but no vehicles appear
- **Solution:** Check logs for mapping errors
- Verify dealershipId is correct
- Check database for records with EasyCarsId set

**Issue:** Background service not running
- **Solution:** Check application logs on startup
- Verify `AddHostedService` is registered in Program.cs
- Check EnableAutoSync configuration

**Issue:** Duplicate vehicles after sync
- **Solution:** Verify EasyCarsId is being set correctly
- Check upsert logic uses EasyCarsId for matching
- Review database unique constraints

---

## 📊 Success Metrics

Track these metrics to verify successful implementation:

- **Sync Success Rate:** > 95% of sync operations complete successfully
- **Sync Duration:** < 5 minutes for 1000 vehicles
- **Data Accuracy:** 100% of EasyCars fields mapped correctly
- **Error Recovery:** Failed syncs retry successfully within 1 hour
- **User Adoption:** > 80% of dealerships configure credentials within 30 days

---

## 🔗 Related Resources

- [EasyCars Stock API Documentation](./EasyCars%20Stock%20API%20Documentation%20v101%201.pdf)
- [EasyCars Lead API Documentation](./EasyCars%20Lead%20API%20Documentation_v105.pdf)
- [PRD - Product Requirements](./EASYCARS_INTEGRATION_PRD.md)
- [Architecture - Technical Design](./EASYCARS_INTEGRATION_ARCHITECTURE.md)

---

## 🚦 Getting Started

Ready to implement? Follow this sequence:

1. **Day 1:** Review all documentation (PRD, Architecture, this guide)
2. **Day 2:** Set up development environment and test credentials
3. **Days 3-10:** Implement Phases 1-4 (backend foundation)
4. **Days 11-16:** Implement Phases 5-6 (APIs and frontend)
5. **Days 17-18:** Testing and documentation
6. **Day 19:** Demo and stakeholder review
7. **Day 20:** Deploy to staging and production

**Estimated Total:** 4 weeks (20 business days)

---

**Questions?** Contact the product team or review the detailed PRD and Architecture documents.

**Last Updated:** 2026-02-24 by BMad Orchestrator
