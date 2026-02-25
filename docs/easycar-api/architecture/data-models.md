# Data Models

### Core Domain Entities

The EasyCars integration introduces new domain entities while extending existing ones to support synchronization tracking and data lineage.

#### EasyCarsCredential Entity

**Purpose:** Securely stores dealership-specific EasyCars API credentials with encryption for Account Number (PublicID) and Account Secret (SecretKey).

**Key Attributes:**
- `Id` (int, PK): Unique identifier for credential record
- `DealershipId` (int, FK): Reference to owning Dealership
- `AccountNumberEncrypted` (string): AES-256-GCM encrypted Account Number/PublicID
- `AccountSecretEncrypted` (string): AES-256-GCM encrypted Account Secret/SecretKey
- `Environment` (enum: Test, Production): Target API environment
- `IsActive` (bool): Whether credentials are currently active
- `YardCode` (string, nullable): Optional filter for specific yard inventory
- `CreatedAt` (DateTime): Credential creation timestamp
- `UpdatedAt` (DateTime): Last modification timestamp
- `LastSyncedAt` (DateTime, nullable): Last successful sync operation

**TypeScript Interface (Shared Model):**
```typescript
export interface EasyCarsCredential {
  id: number;
  dealershipId: number;
  accountNumber?: string; // Only exposed in write operations, never returned in GET
  environment: 'Test' | 'Production';
  isActive: boolean;
  yardCode?: string;
  createdAt: string; // ISO 8601
  updatedAt: string; // ISO 8601
  lastSyncedAt?: string; // ISO 8601
}

export interface EasyCarsCredentialRequest {
  accountNumber: string;
  accountSecret: string;
  environment: 'Test' | 'Production';
  yardCode?: string;
}
```

**Relationships:**
- Belongs to one Dealership (Many-to-One)
- Has many EasyCarsSyncLog entries (One-to-Many)

---

#### EasyCarsSyncLog Entity

**Purpose:** Audit trail for all synchronization operations tracking success, failures, performance metrics, and detailed error information.

**Key Attributes:**
- `Id` (int, PK): Unique log entry identifier
- `DealershipId` (int, FK): Dealership that triggered sync
- `CredentialId` (int, FK, nullable): Credential used for sync
- `SyncType` (enum: Stock, Lead): Type of synchronization
- `SyncDirection` (enum: Inbound, Outbound): Data flow direction
- `Status` (enum: Success, Failed, Warning, InProgress): Sync result
- `StartedAt` (DateTime): Sync operation start time
- `CompletedAt` (DateTime, nullable): Sync completion time
- `RecordsProcessed` (int): Number of records handled
- `RecordsCreated` (int): New records created
- `RecordsUpdated` (int): Existing records updated
- `RecordsFailed` (int): Records that failed to process
- `ErrorMessage` (string, nullable): Summary error message if failed
- `ErrorDetails` (string, nullable): Detailed error information and stack trace
- `RequestPayload` (JSON, nullable): API request payload for debugging
- `ResponseSummary` (JSON, nullable): API response metadata

**TypeScript Interface:**
```typescript
export interface EasyCarsSyncLog {
  id: number;
  dealershipId: number;
  syncType: 'Stock' | 'Lead';
  syncDirection: 'Inbound' | 'Outbound';
  status: 'Success' | 'Failed' | 'Warning' | 'InProgress';
  startedAt: string; // ISO 8601
  completedAt?: string; // ISO 8601
  recordsProcessed: number;
  recordsCreated: number;
  recordsUpdated: number;
  recordsFailed: number;
  errorMessage?: string;
  durationMs?: number; // Calculated: completedAt - startedAt
}

export interface SyncSummary {
  lastSync?: EasyCarsSyncLog;
  recentLogs: EasyCarsSyncLog[];
  isConfigured: boolean;
  isSyncing: boolean;
}
```

**Relationships:**
- Belongs to one Dealership (Many-to-One)
- Belongs to one EasyCarsCredential (Many-to-One, nullable)

---

#### Vehicle Entity Extensions

**Purpose:** Track EasyCars synchronization metadata and store complete API response data for vehicles.

**Extended Attributes:**
- `EasyCarsStockNumber` (string, nullable): EasyCars unique stock identifier
- `EasyCarsYardCode` (string, nullable): Yard location code from EasyCars
- `EasyCarsVIN` (string, nullable): VIN from EasyCars API
- `EasyCarsRawData` (JSON, nullable): Complete EasyCars Stock API response
- `DataSource` (enum: Manual, EasyCars, Import): Origin of vehicle data
- `LastSyncedFromEasyCars` (DateTime, nullable): Last sync timestamp
- `ExteriorColor` (string, nullable): Added from EasyCars Colour field
- `InteriorColor` (string, nullable): Added from EasyCars InteriorColor field
- `Body` (string, nullable): Body type (Sedan, SUV, etc.) from EasyCars
- `FuelType` (string, nullable): Fuel type from EasyCars
- `GearType` (string, nullable): Transmission type from EasyCars
- `EngineCapacity` (string, nullable): Engine size from EasyCars
- `DoorCount` (int, nullable): Number of doors from EasyCars
- `Features` (string[], nullable): Array of features from StandardFeatures/OptionalFeatures

**Note:** Vehicle entity already has Make, Model, Year, Price, Mileage, Condition, Status, Title, Description, Images which map naturally to EasyCars fields.

---

#### Lead Entity Extensions

**Purpose:** Track EasyCars synchronization metadata and support bi-directional lead sync.

**Extended Attributes:**
- `EasyCarsLeadNumber` (string, nullable): EasyCars unique lead identifier
- `EasyCarsCustomerNo` (string, nullable): Customer number from EasyCars
- `EasyCarsRawData` (JSON, nullable): Complete EasyCars Lead API response
- `DataSource` (enum: Manual, EasyCars, WebForm): Origin of lead data
- `LastSyncedToEasyCars` (DateTime, nullable): Last outbound sync timestamp
- `LastSyncedFromEasyCars` (DateTime, nullable): Last inbound sync timestamp
- `VehicleInterestType` (enum, nullable): Type of interest (Purchase, TradeIn, Finance, etc.)
- `FinanceInterested` (bool): Whether customer interested in financing
- `Rating` (enum, nullable): Lead quality rating (Hot, Warm, Cold)

**Note:** Lead entity already has DealershipId, VehicleId, Name, Email, Phone, Message, Status which map to EasyCars customer and lead data.

---
