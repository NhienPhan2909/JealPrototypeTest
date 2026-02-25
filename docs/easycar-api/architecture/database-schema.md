# EasyCars Integration - Database Schema Documentation

## Overview

This document provides comprehensive documentation for the database schema supporting the EasyCars API Integration feature. The schema is implemented using Entity Framework Core 8.0 with PostgreSQL 15+ as the database engine.

**Migration Name:** `AddEasyCarsIntegration`  
**Created:** 2026-02-24  
**Version:** 1.0

---

## Table of Contents

1. [New Tables](#new-tables)
   - [dealership_easycars_credentials](#dealership_easycars_credentials)
   - [easycars_sync_logs](#easycars_sync_logs)
2. [Extended Tables](#extended-tables)
   - [vehicle (Extensions)](#vehicle-extensions)
   - [lead (Extensions)](#lead-extensions)
3. [Indexes](#indexes)
4. [Relationships](#relationships)
5. [Data Types and Constraints](#data-types-and-constraints)
6. [Sample Queries](#sample-queries)
7. [Performance Considerations](#performance-considerations)
8. [Data Retention](#data-retention)

---

## New Tables

### dealership_easycars_credentials

**Purpose:** Stores encrypted EasyCars API credentials for each dealership, enabling secure multi-tenant credential management.

**Business Context:** Each dealership has exactly one set of EasyCars credentials (enforced by UNIQUE constraint). Credentials are encrypted using AES-256-GCM encryption before storage.

#### Schema

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `id` | SERIAL | PRIMARY KEY | Auto-incrementing unique identifier |
| `dealership_id` | INTEGER | NOT NULL, UNIQUE, FK → dealerships.id | Reference to dealership (one credential per dealership) |
| `client_id_encrypted` | TEXT | NOT NULL | Encrypted EasyCars Client ID (UUID from API portal) |
| `client_secret_encrypted` | TEXT | NOT NULL | Encrypted EasyCars Client Secret (UUID from API portal) |
| `account_number_encrypted` | TEXT | NOT NULL | Encrypted EasyCars account number (EC-prefix, e.g. EC114575) |
| `account_secret_encrypted` | TEXT | NOT NULL | Encrypted EasyCars account secret (dealer account UUID) |
| `encryption_iv` | TEXT | NOT NULL | Initialization vector for AES-256-GCM encryption |
| `environment` | VARCHAR(20) | NOT NULL, CHECK IN ('Test', 'Production') | EasyCars environment to connect to |
| `is_active` | BOOLEAN | NOT NULL, DEFAULT true | Whether credentials are currently active |
| `yard_code` | VARCHAR(50) | NULLABLE | Optional yard code for multi-location dealerships |
| `created_at` | TIMESTAMP | NOT NULL, DEFAULT NOW() | Record creation timestamp (UTC) |
| `updated_at` | TIMESTAMP | NOT NULL, DEFAULT NOW() | Record last updated timestamp (UTC) |
| `last_synced_at` | TIMESTAMP | NULLABLE | Timestamp of last successful sync using these credentials |

#### Constraints

- **Primary Key:** `id`
- **Foreign Key:** `dealership_id` → `dealerships.id` ON DELETE CASCADE
- **Unique Constraint:** `dealership_id` (one credential per dealership)
- **Check Constraint:** `environment IN ('Test', 'Production')`

#### Indexes

- `idx_easycars_credentials_dealership` - UNIQUE index on `dealership_id`
- `idx_easycars_credentials_active` - Partial index on `is_active` WHERE `is_active = true`

#### Notes

- Credentials are never stored in plain text
- **Two separate credential sets are stored:** `ClientId`/`ClientSecret` (for API token acquisition) and `AccountNumber`/`AccountSecret` (for stock data access)
- The `encryption_iv` must be stored alongside encrypted data for decryption
- Deleting a dealership automatically deletes its credentials (CASCADE)
- The partial index on `is_active` optimizes queries for active credentials
- Added via EF migration `AddClientIdClientSecretToEasyCarsCredentials` (2026-02-25)

---

### easycars_sync_logs

**Purpose:** Comprehensive audit log for all EasyCars API synchronization operations, tracking success, failures, and performance metrics.

**Business Context:** Critical for debugging integration issues, monitoring sync health, and providing audit trail for compliance. Logs both inbound (Stock/Lead from EasyCars) and outbound (Lead to EasyCars) operations.

#### Schema

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `id` | SERIAL | PRIMARY KEY | Auto-incrementing unique identifier |
| `dealership_id` | INTEGER | NOT NULL, FK → dealerships.id | Dealership this sync belongs to |
| `credential_id` | INTEGER | NULLABLE, FK → dealership_easycars_credentials.id | Credentials used (NULL if deleted) |
| `sync_type` | VARCHAR(20) | NOT NULL, CHECK IN ('Stock', 'Lead') | Type of data being synchronized |
| `sync_direction` | VARCHAR(20) | NOT NULL, CHECK IN ('Inbound', 'Outbound') | Direction of sync |
| `status` | VARCHAR(20) | NOT NULL, CHECK IN ('Success', 'Failed', 'Warning', 'InProgress') | Sync outcome |
| `started_at` | TIMESTAMP | NOT NULL, DEFAULT NOW() | When sync started (UTC) |
| `completed_at` | TIMESTAMP | NULLABLE, CHECK >= started_at | When sync completed (UTC) |
| `records_processed` | INTEGER | NOT NULL, DEFAULT 0 | Total records processed |
| `records_created` | INTEGER | NOT NULL, DEFAULT 0 | New records created |
| `records_updated` | INTEGER | NOT NULL, DEFAULT 0 | Existing records updated |
| `records_failed` | INTEGER | NOT NULL, DEFAULT 0 | Records that failed to process |
| `error_message` | TEXT | NULLABLE | High-level error message if sync failed |
| `error_details` | TEXT | NULLABLE | Detailed error information (stack trace, etc.) |
| `request_payload` | JSONB | NULLABLE | JSON of API request sent to EasyCars |
| `response_summary` | JSONB | NULLABLE | JSON summary of API response received |
| `created_at` | TIMESTAMP | NOT NULL, DEFAULT NOW() | Record creation timestamp (UTC) |

#### Constraints

- **Primary Key:** `id`
- **Foreign Key:** `dealership_id` → `dealerships.id` ON DELETE CASCADE
- **Foreign Key:** `credential_id` → `dealership_easycars_credentials.id` ON DELETE SET NULL
- **Check Constraint:** `sync_type IN ('Stock', 'Lead')`
- **Check Constraint:** `sync_direction IN ('Inbound', 'Outbound')`
- **Check Constraint:** `status IN ('Success', 'Failed', 'Warning', 'InProgress')`
- **Check Constraint:** `completed_at IS NULL OR completed_at >= started_at`

#### Indexes

- `idx_sync_logs_dealership` - Index on `dealership_id` for filtering by dealership
- `idx_sync_logs_status` - Index on `status` for filtering by outcome
- `idx_sync_logs_started_at` - Descending index on `started_at` for recent logs first
- `idx_sync_logs_sync_type` - Index on `sync_type` for filtering by type

#### Notes

- `credential_id` uses SET NULL on delete to preserve log history even after credentials deleted
- JSONB columns enable flexible storage of API payloads without schema changes
- The `completed_at >= started_at` constraint prevents data integrity issues
- Status 'Warning' indicates partial success (some records failed but sync continued)

---

## Extended Tables

### vehicle (Extensions)

**Purpose:** Extends the existing `vehicle` table with EasyCars-specific fields to support bi-directional data synchronization and comprehensive vehicle attribute storage.

**Business Context:** Vehicles can originate from three sources: Manual entry, EasyCars sync, or bulk Import. The `easycars_raw_data` JSONB column preserves the complete Stock API response (70+ fields) for audit and future feature enhancement.

#### New Columns

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `easycars_stock_number` | VARCHAR(100) | NULLABLE | EasyCars unique stock number (primary identifier in their system) |
| `easycars_yard_code` | VARCHAR(50) | NULLABLE | Yard code if vehicle at specific dealership location |
| `easycars_vin` | VARCHAR(17) | NULLABLE | Vehicle Identification Number from EasyCars |
| `easycars_raw_data` | JSONB | NULLABLE | Complete JSON response from EasyCars Stock API (70+ fields) |
| `data_source` | VARCHAR(20) | NOT NULL, DEFAULT 'Manual', CHECK IN ('Manual', 'EasyCars', 'Import') | Origin of vehicle data |
| `last_synced_from_easycars` | TIMESTAMP | NULLABLE | Last successful sync from EasyCars (UTC) |
| `exterior_color` | VARCHAR(50) | NULLABLE | Vehicle exterior color |
| `interior_color` | VARCHAR(50) | NULLABLE | Vehicle interior color |
| `body` | VARCHAR(50) | NULLABLE | Body type (Sedan, SUV, etc.) |
| `fuel_type` | VARCHAR(50) | NULLABLE | Fuel type (Petrol, Diesel, Electric, etc.) |
| `gear_type` | VARCHAR(50) | NULLABLE | Transmission type (Automatic, Manual) |
| `engine_capacity` | VARCHAR(50) | NULLABLE | Engine size/capacity |
| `door_count` | INTEGER | NULLABLE | Number of doors |
| `features` | JSONB | NULLABLE | JSON array of StandardFeatures and OptionalFeatures |

#### New Indexes

- `idx_vehicles_easycars_stock` - Partial index on `easycars_stock_number` WHERE NOT NULL
- `idx_vehicles_data_source` - Index on `data_source` for filtering by origin
- `idx_vehicles_easycars_vin` - Partial index on `easycars_vin` WHERE NOT NULL

#### Data Source Values

- **Manual**: Vehicle entered manually by dealership staff via web interface
- **EasyCars**: Vehicle synchronized from EasyCars Stock API
- **Import**: Vehicle imported from CSV or other bulk import mechanism

---

### lead (Extensions)

**Purpose:** Extends the existing `lead` table with EasyCars-specific fields to support bi-directional lead synchronization and enriched lead qualification data.

**Business Context:** Leads can originate from Manual entry (staff input), EasyCars (imported from their system), or WebForm (customer submission). The system supports both pushing leads to EasyCars and pulling leads from EasyCars.

#### New Columns

| Column Name | Data Type | Constraints | Description |
|------------|-----------|-------------|-------------|
| `easycars_lead_number` | VARCHAR(100) | NULLABLE | EasyCars unique lead number (primary identifier in their system) |
| `easycars_customer_no` | VARCHAR(100) | NULLABLE | EasyCars customer number (if lead associated with existing customer) |
| `easycars_raw_data` | JSONB | NULLABLE | Complete JSON response from EasyCars Lead API (20+ fields) |
| `data_source` | VARCHAR(20) | NOT NULL, DEFAULT 'Manual', CHECK IN ('Manual', 'EasyCars', 'WebForm') | Origin of lead data |
| `last_synced_to_easycars` | TIMESTAMP | NULLABLE | Last successful push to EasyCars (UTC) |
| `last_synced_from_easycars` | TIMESTAMP | NULLABLE | Last successful pull from EasyCars (UTC) |
| `vehicle_interest_type` | VARCHAR(50) | NULLABLE | Type of vehicle interest (New, Used, Trade-In) |
| `finance_interested` | BOOLEAN | NOT NULL, DEFAULT false | Whether customer interested in financing |
| `rating` | VARCHAR(20) | NULLABLE | Lead quality rating (Hot, Warm, Cold) |

#### New Indexes

- `idx_leads_easycars_lead` - Partial index on `easycars_lead_number` WHERE NOT NULL
- `idx_leads_data_source` - Index on `data_source` for filtering by origin

#### Data Source Values

- **Manual**: Lead entered manually by dealership staff
- **EasyCars**: Lead synchronized from EasyCars Lead API
- **WebForm**: Lead submitted by customer via dealership website

---

## Indexes

### Index Strategy Rationale

The indexing strategy is designed to optimize the most common query patterns while minimizing index maintenance overhead.

#### EasyCars Credentials Indexes

1. **idx_easycars_credentials_dealership** (UNIQUE)
   - **Purpose:** Enforce one credential per dealership, fast lookups by dealership
   - **Query Pattern:** `SELECT * FROM dealership_easycars_credentials WHERE dealership_id = ?`

2. **idx_easycars_credentials_active** (PARTIAL)
   - **Purpose:** Optimize queries for active credentials only
   - **Query Pattern:** `SELECT * FROM dealership_easycars_credentials WHERE is_active = true`
   - **Note:** Partial index reduces size by excluding inactive credentials

#### Sync Logs Indexes

3. **idx_sync_logs_dealership**
   - **Purpose:** Fast filtering of sync logs by dealership
   - **Query Pattern:** `SELECT * FROM easycars_sync_logs WHERE dealership_id = ? ORDER BY started_at DESC`

4. **idx_sync_logs_status**
   - **Purpose:** Quick queries for failed syncs, monitoring dashboards
   - **Query Pattern:** `SELECT * FROM easycars_sync_logs WHERE status = 'Failed'`

5. **idx_sync_logs_started_at** (DESCENDING)
   - **Purpose:** Efficiently retrieve recent sync logs (most common use case)
   - **Query Pattern:** `SELECT * FROM easycars_sync_logs ORDER BY started_at DESC LIMIT 50`

6. **idx_sync_logs_sync_type**
   - **Purpose:** Filter logs by Stock vs Lead syncs
   - **Query Pattern:** `SELECT * FROM easycars_sync_logs WHERE sync_type = 'Stock'`

#### Vehicle Indexes

7. **idx_vehicles_easycars_stock** (PARTIAL)
   - **Purpose:** Fast lookup by EasyCars stock number, correlation with their system
   - **Query Pattern:** `SELECT * FROM vehicle WHERE easycars_stock_number = ?`
   - **Note:** Partial index excludes manually entered vehicles (NULL stock numbers)

8. **idx_vehicles_data_source**
   - **Purpose:** Filter vehicles by origin (Manual, EasyCars, Import)
   - **Query Pattern:** `SELECT * FROM vehicle WHERE data_source = 'EasyCars'`

9. **idx_vehicles_easycars_vin** (PARTIAL)
   - **Purpose:** Fast VIN lookups, duplicate detection
   - **Query Pattern:** `SELECT * FROM vehicle WHERE easycars_vin = ?`

#### Lead Indexes

10. **idx_leads_easycars_lead** (PARTIAL)
    - **Purpose:** Fast lookup by EasyCars lead number, correlation with their system
    - **Query Pattern:** `SELECT * FROM lead WHERE easycars_lead_number = ?`

11. **idx_leads_data_source**
    - **Purpose:** Filter leads by origin (Manual, EasyCars, WebForm)
    - **Query Pattern:** `SELECT * FROM lead WHERE data_source = 'EasyCars'`

---

## Relationships

### Entity Relationship Diagram (ERD)

```
┌─────────────────────────┐
│      dealerships        │
│  (Existing Table)       │
├─────────────────────────┤
│ • id (PK)              │
│ • name                  │
│ • ...                   │
└────────┬────────────────┘
         │
         │ 1:1
         │ ON DELETE CASCADE
         ▼
┌─────────────────────────────────────────┐
│  dealership_easycars_credentials        │
├─────────────────────────────────────────┤
│ • id (PK)                              │
│ • dealership_id (FK, UNIQUE)           │◄──┐
│ • account_number_encrypted              │   │
│ • account_secret_encrypted              │   │
│ • encryption_iv                         │   │
│ • environment                           │   │
│ • is_active                             │   │
│ • yard_code                             │   │
│ • created_at, updated_at                │   │
│ • last_synced_at                        │   │
└────────┬────────────────────────────────┘   │
         │                                     │ N:1
         │ 1:N                                 │ ON DELETE SET NULL
         │ ON DELETE SET NULL                  │
         ▼                                     │
┌─────────────────────────────────────────┐   │
│       easycars_sync_logs                │   │
├─────────────────────────────────────────┤   │
│ • id (PK)                              │   │
│ • dealership_id (FK)                   │───┤
│ • credential_id (FK, NULLABLE)         │───┘
│ • sync_type                             │
│ • sync_direction                        │
│ • status                                │
│ • started_at, completed_at              │
│ • records_*                             │
│ • error_*                               │
│ • request_payload (JSONB)               │
│ • response_summary (JSONB)              │
│ • created_at                            │
└─────────────────────────────────────────┘

┌─────────────────────────┐
│        vehicle          │
│  (Extended Table)       │
├─────────────────────────┤
│ • id (PK)              │
│ • dealership_id (FK)   │───────┐
│ • make, model, year     │       │
│ • price, mileage        │       │
│                         │       │ N:1
│ EasyCars Extensions:    │       │ ON DELETE CASCADE
│ • easycars_stock_number │       │
│ • easycars_yard_code    │       │
│ • easycars_vin          │       ▼
│ • easycars_raw_data     │   ┌──────────────┐
│ • data_source           │   │ dealerships  │
│ • last_synced_from_*    │   └──────────────┘
│ • exterior_color        │       ▲
│ • interior_color        │       │
│ • body, fuel_type       │       │
│ • gear_type, engine_*   │       │
│ • door_count            │       │
│ • features (JSONB)      │       │ N:1
└─────────────────────────┘       │ ON DELETE CASCADE
                                  │
┌─────────────────────────┐       │
│         lead            │       │
│   (Extended Table)      │       │
├─────────────────────────┤       │
│ • id (PK)              │       │
│ • dealership_id (FK)   │───────┘
│ • vehicle_id (FK)       │
│ • name, email, phone    │
│ • message, status       │
│                         │
│ EasyCars Extensions:    │
│ • easycars_lead_number  │
│ • easycars_customer_no  │
│ • easycars_raw_data     │
│ • data_source           │
│ • last_synced_to_*      │
│ • last_synced_from_*    │
│ • vehicle_interest_type │
│ • finance_interested    │
│ • rating                │
└─────────────────────────┘
```

### Cascade Delete Behavior

1. **dealerships → dealership_easycars_credentials**: CASCADE
   - Deleting a dealership automatically deletes its EasyCars credentials
   - Rationale: Credentials belong to dealership, no orphaned credentials

2. **dealerships → easycars_sync_logs**: CASCADE
   - Deleting a dealership automatically deletes all its sync logs
   - Rationale: Logs belong to dealership, audit trail follows dealership lifecycle

3. **dealership_easycars_credentials → easycars_sync_logs**: SET NULL
   - Deleting credentials sets `credential_id` to NULL in sync logs
   - Rationale: Preserve historical sync logs even after credential rotation/deletion

4. **dealerships → vehicle**: CASCADE (existing)
   - Deleting a dealership deletes all its vehicles
   - Extended with EasyCars fields but cascade behavior unchanged

5. **dealerships → lead**: CASCADE (existing)
   - Deleting a dealership deletes all its leads
   - Extended with EasyCars fields but cascade behavior unchanged

---

## Data Types and Constraints

### JSONB Columns

The schema uses PostgreSQL's JSONB data type for storing flexible, queryable JSON data:

1. **easycars_sync_logs.request_payload** - Stores API request JSON
2. **easycars_sync_logs.response_summary** - Stores API response JSON
3. **vehicle.easycars_raw_data** - Stores complete Stock API response (70+ fields)
4. **vehicle.features** - Stores array of StandardFeatures and OptionalFeatures
5. **lead.easycars_raw_data** - Stores complete Lead API response (20+ fields)

#### JSONB Benefits

- **Flexible Schema**: Store variable API responses without schema migrations
- **Queryable**: Use PostgreSQL's JSON operators to query nested data
- **Indexed**: Can create GIN indexes on JSONB columns for performance
- **Space Efficient**: Binary format compresses well
- **Audit Trail**: Preserve complete API responses for debugging

#### Example JSONB Queries

```sql
-- Find vehicles with specific feature
SELECT * FROM vehicle 
WHERE easycars_raw_data @> '{"Make": "Toyota"}';

-- Extract specific field from raw data
SELECT id, easycars_raw_data->>'Make' as make, 
       easycars_raw_data->>'Model' as model
FROM vehicle 
WHERE data_source = 'EasyCars';

-- Query features array
SELECT * FROM vehicle 
WHERE features @> '["Air Conditioning"]';
```

### CHECK Constraints

CHECK constraints enforce data integrity at the database level:

1. **CK_easycars_credentials_environment**: `environment IN ('Test', 'Production')`
   - Ensures only valid environment values
   - Prevents typos and invalid configurations

2. **CK_sync_logs_sync_type**: `sync_type IN ('Stock', 'Lead')`
   - Enforces valid sync types
   - Aligns with application enum values

3. **CK_sync_logs_sync_direction**: `sync_direction IN ('Inbound', 'Outbound')`
   - Enforces valid sync directions
   - Clarifies data flow

4. **CK_sync_logs_status**: `status IN ('Success', 'Failed', 'Warning', 'InProgress')`
   - Enforces valid status values
   - Prevents application bugs from storing invalid states

5. **CK_sync_logs_completed_at**: `completed_at IS NULL OR completed_at >= started_at`
   - Ensures logical timestamp ordering
   - Prevents data integrity issues

6. **CK_vehicle_data_source**: `data_source IN ('Manual', 'EasyCars', 'Import')`
   - Enforces valid vehicle origins
   - Critical for filtering and reporting

7. **CK_lead_data_source**: `data_source IN ('Manual', 'EasyCars', 'WebForm')`
   - Enforces valid lead origins
   - Critical for lead source analysis

---

## Sample Queries

### Common Query Patterns

#### 1. Get Active EasyCars Credentials for Dealership

```sql
SELECT * 
FROM dealership_easycars_credentials 
WHERE dealership_id = 123 
  AND is_active = true;
```

**Performance:** Uses `idx_easycars_credentials_dealership` unique index

---

#### 2. Get Recent Failed Syncs for Monitoring Dashboard

```sql
SELECT 
    sync_type,
    sync_direction,
    started_at,
    completed_at,
    error_message,
    records_processed,
    records_failed
FROM easycars_sync_logs 
WHERE status = 'Failed' 
  AND started_at >= NOW() - INTERVAL '24 hours'
ORDER BY started_at DESC 
LIMIT 50;
```

**Performance:** Uses `idx_sync_logs_status` and `idx_sync_logs_started_at` indexes

---

#### 3. Find Vehicles by EasyCars Stock Number

```sql
SELECT 
    id,
    make,
    model,
    year,
    price,
    easycars_stock_number,
    last_synced_from_easycars
FROM vehicle 
WHERE easycars_stock_number = 'STK-12345';
```

**Performance:** Uses `idx_vehicles_easycars_stock` partial index

---

#### 4. Get All EasyCars-Sourced Vehicles for Dealership

```sql
SELECT 
    id,
    make,
    model,
    year,
    price,
    easycars_stock_number,
    easycars_raw_data->>'Colour' as colour,
    last_synced_from_easycars
FROM vehicle 
WHERE dealership_id = 123 
  AND data_source = 'EasyCars'
ORDER BY last_synced_from_easycars DESC;
```

**Performance:** Uses `idx_vehicle_dealership_id` and `idx_vehicles_data_source`

---

#### 5. Sync Performance Report (Last 7 Days)

```sql
SELECT 
    DATE(started_at) as sync_date,
    sync_type,
    sync_direction,
    COUNT(*) as total_syncs,
    SUM(CASE WHEN status = 'Success' THEN 1 ELSE 0 END) as successful,
    SUM(CASE WHEN status = 'Failed' THEN 1 ELSE 0 END) as failed,
    SUM(records_processed) as total_records,
    AVG(EXTRACT(EPOCH FROM (completed_at - started_at))) as avg_duration_seconds
FROM easycars_sync_logs 
WHERE started_at >= NOW() - INTERVAL '7 days'
  AND completed_at IS NOT NULL
GROUP BY DATE(started_at), sync_type, sync_direction
ORDER BY sync_date DESC, sync_type, sync_direction;
```

**Performance:** Uses `idx_sync_logs_started_at` index

---

#### 6. Find Leads Synced to EasyCars in Last Hour

```sql
SELECT 
    id,
    name,
    email,
    phone,
    easycars_lead_number,
    last_synced_to_easycars
FROM lead 
WHERE last_synced_to_easycars >= NOW() - INTERVAL '1 hour'
ORDER BY last_synced_to_easycars DESC;
```

**Performance:** Full table scan (consider adding index if this query is frequent)

---

#### 7. Check for Duplicate Vehicles by VIN

```sql
SELECT 
    easycars_vin,
    COUNT(*) as duplicate_count,
    STRING_AGG(CAST(id AS VARCHAR), ', ') as vehicle_ids
FROM vehicle 
WHERE easycars_vin IS NOT NULL
GROUP BY easycars_vin
HAVING COUNT(*) > 1;
```

**Performance:** Uses `idx_vehicles_easycars_vin` partial index

---

#### 8. Get Sync Log with Credential Details

```sql
SELECT 
    sl.id,
    sl.sync_type,
    sl.status,
    sl.started_at,
    sl.records_processed,
    c.environment,
    c.yard_code,
    d.name as dealership_name
FROM easycars_sync_logs sl
LEFT JOIN dealership_easycars_credentials c ON sl.credential_id = c.id
INNER JOIN dealership d ON sl.dealership_id = d.id
WHERE sl.dealership_id = 123
ORDER BY sl.started_at DESC
LIMIT 20;
```

**Performance:** Uses indexes on all join columns

---

## Performance Considerations

### Index Selection Strategy

1. **Partial Indexes**: Used for columns with many NULL values (e.g., `easycars_stock_number`)
   - Reduces index size by 50-90% in typical use cases
   - Improves write performance (fewer index updates)
   - Maintains fast lookup for non-NULL values

2. **Composite Indexes**: Currently minimal, may need to add for specific query patterns
   - Example: `(dealership_id, data_source)` for common filtering combinations
   - Monitor slow query log to identify needs

3. **JSONB Indexes**: Not created by default, add if needed
   - GIN indexes on `easycars_raw_data` for frequent JSON queries
   - Example: `CREATE INDEX idx_vehicle_raw_data_gin ON vehicle USING GIN (easycars_raw_data);`

### Query Optimization Tips

1. **Always filter by `dealership_id` first** for tenant isolation and performance
2. **Use partial indexes** when querying EasyCars-sourced data
3. **Avoid SELECT *** on JSONB columns (can be large), be explicit
4. **Use EXPLAIN ANALYZE** to verify index usage on critical queries
5. **Consider materialized views** for complex sync reports if needed

### Scalability Considerations

1. **Sync Logs Table Growth**
   - Expected 100-1000 logs per dealership per day
   - 365,000 - 3,650,000 rows per year (for 1000 dealerships)
   - See [Data Retention](#data-retention) section for archival strategy

2. **JSONB Column Size**
   - Stock API response: ~10-30 KB per vehicle
   - Lead API response: ~2-5 KB per lead
   - Acceptable overhead for audit trail and flexibility

3. **Connection Pooling**
   - PostgreSQL handles JSONB efficiently with proper pooling
   - Recommended: 20-50 connections per application instance

---

## Data Retention

### Sync Logs Archival Strategy

**Problem:** The `easycars_sync_logs` table will grow rapidly with potentially millions of records per year.

**Recommended Strategy:**

1. **Hot Data (Main Table)**: Keep recent 90 days in `easycars_sync_logs`
   - Fast queries for monitoring, debugging, recent reports
   - Estimated size: 9-90 million rows (for 1000 dealerships)

2. **Warm Data (Archive Table)**: Create `easycars_sync_logs_archive`
   - Move logs older than 90 days using monthly batch job
   - Retain for 2 years for compliance and analytics
   - Can be stored on slower/cheaper storage

3. **Cold Data (Object Storage)**: Export logs older than 2 years
   - Compress and store in S3/Azure Blob for long-term retention
   - Delete from database to free space
   - Retrieve only if needed for legal/audit purposes

**Implementation Steps (Future Story):**

```sql
-- Create archive table (identical schema)
CREATE TABLE easycars_sync_logs_archive (LIKE easycars_sync_logs INCLUDING ALL);

-- Monthly archival job
INSERT INTO easycars_sync_logs_archive 
SELECT * FROM easycars_sync_logs 
WHERE started_at < NOW() - INTERVAL '90 days';

DELETE FROM easycars_sync_logs 
WHERE started_at < NOW() - INTERVAL '90 days';

-- Vacuum to reclaim space
VACUUM ANALYZE easycars_sync_logs;
```

### Raw API Data Retention

**vehicle.easycars_raw_data** and **lead.easycars_raw_data**:
- Retained indefinitely with the entity
- Provides audit trail and enables future feature development
- No archival needed (data volume manageable)

---

## Migration Rollback

### Rolling Back the Migration

If you need to rollback this migration, run:

```bash
dotnet ef database update <PreviousMigrationName> -p JealPrototype.Infrastructure -s JealPrototype.API
```

Or directly in PostgreSQL:

```sql
-- Drop indexes
DROP INDEX IF EXISTS idx_easycars_credentials_dealership;
DROP INDEX IF EXISTS idx_easycars_credentials_active;
DROP INDEX IF EXISTS idx_sync_logs_dealership;
DROP INDEX IF EXISTS idx_sync_logs_status;
DROP INDEX IF EXISTS idx_sync_logs_started_at;
DROP INDEX IF EXISTS idx_sync_logs_sync_type;
DROP INDEX IF EXISTS idx_vehicles_easycars_stock;
DROP INDEX IF EXISTS idx_vehicles_data_source;
DROP INDEX IF EXISTS idx_vehicles_easycars_vin;
DROP INDEX IF EXISTS idx_leads_easycars_lead;
DROP INDEX IF EXISTS idx_leads_data_source;

-- Drop new tables
DROP TABLE IF EXISTS easycars_sync_logs;
DROP TABLE IF EXISTS dealership_easycars_credentials;

-- Remove columns from vehicle table
ALTER TABLE vehicle 
DROP COLUMN IF EXISTS easycars_stock_number,
DROP COLUMN IF EXISTS easycars_yard_code,
DROP COLUMN IF EXISTS easycars_vin,
DROP COLUMN IF EXISTS easycars_raw_data,
DROP COLUMN IF EXISTS data_source,
DROP COLUMN IF EXISTS last_synced_from_easycars,
DROP COLUMN IF EXISTS exterior_color,
DROP COLUMN IF EXISTS interior_color,
DROP COLUMN IF EXISTS body,
DROP COLUMN IF EXISTS fuel_type,
DROP COLUMN IF EXISTS gear_type,
DROP COLUMN IF EXISTS engine_capacity,
DROP COLUMN IF EXISTS door_count,
DROP COLUMN IF EXISTS features;

-- Remove columns from lead table
ALTER TABLE lead 
DROP COLUMN IF EXISTS easycars_lead_number,
DROP COLUMN IF EXISTS easycars_customer_no,
DROP COLUMN IF EXISTS easycars_raw_data,
DROP COLUMN IF EXISTS data_source,
DROP COLUMN IF EXISTS last_synced_to_easycars,
DROP COLUMN IF EXISTS last_synced_from_easycars,
DROP COLUMN IF EXISTS vehicle_interest_type,
DROP COLUMN IF EXISTS finance_interested,
DROP COLUMN IF EXISTS rating;
```

**Note:** The EF Core migration's `Down()` method implements proper rollback automatically.

---

## Verification Checklist

After applying this migration, verify:

- [ ] All 2 new tables created (`dealership_easycars_credentials`, `easycars_sync_logs`)
- [ ] All 11 indexes created (2 for credentials, 4 for sync logs, 3 for vehicles, 2 for leads)
- [ ] All CHECK constraints enforced (test inserting invalid enum values)
- [ ] Foreign key relationships cascade correctly (test deleting a dealership)
- [ ] UNIQUE constraint on `dealership_id` in credentials table
- [ ] SET NULL behavior on credential deletion (test deleting credentials with sync logs)
- [ ] JSONB columns accept valid JSON
- [ ] Partial indexes only include non-NULL rows
- [ ] Existing vehicle and lead records unaffected
- [ ] No breaking changes to existing queries

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-24 | BMad Dev Agent | Initial documentation for AddEasyCarsIntegration migration |

---

**End of Database Schema Documentation**
