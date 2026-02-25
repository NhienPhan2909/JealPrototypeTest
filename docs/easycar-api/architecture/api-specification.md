# API Specification

### REST API Endpoints

#### Credential Management Endpoints

**POST /api/admin/easycars/credentials/test-connection**

Test EasyCars credentials without saving them.

Request:
```json
{
  "clientId": "uuid-from-easycars-portal",
  "clientSecret": "uuid-from-easycars-portal",
  "accountNumber": "EC114575",
  "accountSecret": "11b6d61f-6e67-4c16-8511-8647ecb881d6",
  "environment": "Test"
}
```

> **Note:** `clientId` and `clientSecret` are the API-level credentials (UUIDs from the EasyCars API portal, used to obtain a JWT token). `accountNumber` and `accountSecret` are the dealer account credentials (EC-prefixed number and UUID) sent in stock request bodies.

Response (200 OK):
```json
{
  "success": true,
  "message": "Successfully connected to EasyCars Test API",
  "tokenExpiresAt": "2025-01-15T10:15:00Z"
}
```

Response (401 Unauthorized - from EasyCars):
```json
{
  "success": false,
  "message": "Authentication failed. Please verify your Account Number and Secret.",
  "errorCode": "AUTHENTICATION_FAIL"
}
```

---

**POST /api/admin/easycars/credentials**

Create or update EasyCars credentials for authenticated dealership.

Request:
```json
{
  "clientId": "uuid-from-easycars-portal",
  "clientSecret": "uuid-from-easycars-portal",
  "accountNumber": "EC114575",
  "accountSecret": "11b6d61f-6e67-4c16-8511-8647ecb881d6",
  "environment": "Production",
  "yardCode": "MAIN" // Optional
}
```

Response (201 Created):
```json
{
  "id": 1,
  "dealershipId": 123,
  "environment": "Production",
  "isActive": true,
  "yardCode": "MAIN",
  "createdAt": "2025-01-15T10:00:00Z",
  "updatedAt": "2025-01-15T10:00:00Z"
}
```

---

**GET /api/admin/easycars/credentials**

Retrieve EasyCars credentials metadata for authenticated dealership (does not return decrypted secrets).

Response (200 OK):
```json
{
  "id": 1,
  "dealershipId": 123,
  "environment": "Production",
  "isActive": true,
  "yardCode": "MAIN",
  "createdAt": "2025-01-15T10:00:00Z",
  "updatedAt": "2025-01-15T10:00:00Z",
  "lastSyncedAt": "2025-01-15T12:30:00Z"
}
```

Response (404 Not Found):
```json
{
  "message": "No EasyCars credentials configured for this dealership"
}
```

---

**DELETE /api/admin/easycars/credentials/{id}**

Delete EasyCars credentials for authenticated dealership.

Response (204 No Content)

---

#### Synchronization Endpoints

**POST /api/admin/easycars/sync/stock**

Manually trigger stock synchronization for authenticated dealership.

Response (202 Accepted):
```json
{
  "jobId": "hangfire-job-123",
  "message": "Stock synchronization started",
  "estimatedDurationSeconds": 60
}
```

---

**POST /api/admin/easycars/sync/leads**

Manually trigger lead synchronization for authenticated dealership.

Response (202 Accepted):
```json
{
  "jobId": "hangfire-job-124",
  "message": "Lead synchronization started",
  "estimatedDurationSeconds": 30
}
```

---

**GET /api/admin/easycars/sync/status**

Get current sync status and recent history for authenticated dealership.

Response (200 OK):
```json
{
  "isConfigured": true,
  "isSyncing": false,
  "lastStockSync": {
    "id": 45,
    "syncType": "Stock",
    "status": "Success",
    "startedAt": "2025-01-15T12:30:00Z",
    "completedAt": "2025-01-15T12:31:15Z",
    "recordsProcessed": 150,
    "recordsCreated": 5,
    "recordsUpdated": 145,
    "recordsFailed": 0,
    "durationMs": 75000
  },
  "lastLeadSync": {
    "id": 46,
    "syncType": "Lead",
    "status": "Success",
    "startedAt": "2025-01-15T12:35:00Z",
    "completedAt": "2025-01-15T12:35:20Z",
    "recordsProcessed": 25,
    "recordsCreated": 3,
    "recordsUpdated": 22,
    "recordsFailed": 0,
    "durationMs": 20000
  },
  "recentLogs": [
    /* Last 10 sync logs */
  ]
}
```

---

**GET /api/admin/easycars/sync/logs**

Get paginated sync history for authenticated dealership.

Query Parameters:
- `page` (int, default: 1): Page number
- `pageSize` (int, default: 20, max: 100): Records per page
- `syncType` (string, optional): Filter by "Stock" or "Lead"
- `status` (string, optional): Filter by status

Response (200 OK):
```json
{
  "logs": [
    {
      "id": 45,
      "syncType": "Stock",
      "syncDirection": "Inbound",
      "status": "Success",
      "startedAt": "2025-01-15T12:30:00Z",
      "completedAt": "2025-01-15T12:31:15Z",
      "recordsProcessed": 150,
      "recordsCreated": 5,
      "recordsUpdated": 145,
      "recordsFailed": 0
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalRecords": 150,
    "totalPages": 8
  }
}
```

---

**GET /api/admin/easycars/sync/logs/{id}**

Get detailed sync log including error details and payloads.

Response (200 OK):
```json
{
  "id": 47,
  "dealershipId": 123,
  "syncType": "Stock",
  "syncDirection": "Inbound",
  "status": "Failed",
  "startedAt": "2025-01-15T13:00:00Z",
  "completedAt": "2025-01-15T13:00:45Z",
  "recordsProcessed": 50,
  "recordsFailed": 50,
  "errorMessage": "API rate limit exceeded",
  "errorDetails": "System.Net.Http.HttpRequestException: 429 Too Many Requests...",
  "requestPayload": { /* Full API request */ },
  "responseSummary": { /* API response metadata */ }
}
```

---

### Authentication & Authorization

All endpoints require:
- **Authentication:** Valid JWT bearer token in `Authorization` header
- **Authorization:** User must have admin permissions for their dealership
- **Tenant Isolation:** All operations scoped to the authenticated user's dealership

**Admin Users:** Platform-level admins do not have a `dealershipid` JWT claim. They must pass `?dealershipId=` as a query parameter on all requests. The backend resolves the effective dealership using `ResolveEffectiveDealershipId(int? requestedDealershipId)` in each controller — admins use the query param, regular users use their JWT claim.

**JWT Claim Notes:**
- All JWT claims are lowercase: `"sub"`, `"username"`, `"email"`, `"usertype"`, `"fullname"`, `"dealershipid"`
- The `usertype` value is `"Admin"` (PascalCase) — comparison must be case-insensitive
- Non-admin users always have a `dealershipid` claim

Middleware flow:
1. JWT validation (signature, expiration)
2. Extract `dealershipid` claim from token (or `dealershipId` query param for admins)
3. Verify user has admin permission
4. Inject dealership context into request pipeline
5. Repository queries automatically filter by dealership

---
