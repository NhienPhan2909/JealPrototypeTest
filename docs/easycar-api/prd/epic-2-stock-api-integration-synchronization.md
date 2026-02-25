# Epic 2: Stock API Integration & Synchronization

Implement complete integration with EasyCars Stock API including data retrieval, mapping to local vehicle inventory schema, periodic synchronization scheduler, and admin interface for monitoring and manually triggering stock sync operations. This epic delivers immediate business value by automating vehicle inventory synchronization, eliminating manual data entry, and ensuring dealership websites always display accurate, up-to-date stock information.

### Story 2.1: Implement Stock API Data Retrieval

As a backend developer,
I want to implement the GetAdvertisementStocks API call in the EasyCars client,
so that the system can retrieve vehicle inventory data from EasyCars.

#### Acceptance Criteria

1. Method `getAdvertisementStocks(accountNumber, accountSecret, yardCode?)` added to EasyCarsApiClient
2. Method constructs POST request to GetAdvertisementStocks endpoint with proper headers and JSON body
3. Method authenticates using RequestToken before making the stock request
4. Optional yardCode parameter filters results when provided
5. Response parsed into strongly-typed StockItem objects with all 70+ fields
6. Method handles pagination if EasyCars API supports it (review documentation)
7. Returns array of stock items on success, throws typed exception on failure
8. Unit tests with mocked API responses cover successful retrieval, empty results, authentication failure, and API errors
9. Integration test using provided test credentials validates end-to-end retrieval

### Story 2.2: Create Data Mapping Service for Stock Data

As a backend developer,
I want to create a service that maps EasyCars stock data to our vehicle inventory schema,
so that synchronized data integrates correctly with existing vehicle management features.

#### Acceptance Criteria

1. Service class created (e.g., `EasyCarsStockMapper`) with method `mapToVehicle(stockItem)`
2. Mapping handles all critical fields: Make, Model, Badge, Year, Price, VIN, RegoNum, Body, Colour, Odometer, etc.
3. Mapping creates or updates vehicle record with source indicator "EasyCars"
4. Mapping stores complete EasyCars raw data in `easycar_stock_data` table for auditability
5. Mapping handles data type conversions (e.g., string to enum for StockType, string to boolean)
6. Mapping handles nullable fields appropriately with sensible defaults where needed
7. Duplicate detection logic identifies existing vehicles by VIN or StockNumber
8. Unit tests cover field mapping accuracy, null handling, and duplicate detection scenarios
9. Mapper logs any data transformation warnings or issues

### Story 2.3: Implement Stock Synchronization Service

As a backend developer,
I want to implement a stock synchronization service that orchestrates the sync process,
so that vehicle inventory can be synchronized from EasyCars to our local database reliably.

#### Acceptance Criteria

1. Service class created (e.g., `EasyCarsStockSyncService`) with method `syncStock(dealershipId)`
2. Service retrieves dealership credentials from database
3. Service calls EasyCars Stock API using credentials
4. Service iterates through stock items and uses mapper to create/update vehicle records
5. Service wraps sync operation in database transaction for atomicity
6. Service creates audit log entry in `easycar_sync_log` with summary (records processed, errors)
7. Service implements idempotent sync logic (safe to run multiple times)
8. Service handles partial failures gracefully (logs errors but processes remaining items)
9. Service returns sync summary object (success status, counts, errors)
10. Unit tests with mocked dependencies cover successful sync, partial failures, and complete failures
11. Integration test performs full sync operation with test database

### Story 2.4: Implement Background Job Scheduler for Periodic Sync

As a backend developer,
I want to implement a background job scheduler for periodic stock synchronization,
so that vehicle inventory automatically stays synchronized without manual intervention.

#### Acceptance Criteria

1. Background job framework integrated (use existing scheduler or implement simple cron-like scheduler)
2. Configurable sync interval via environment variable or database setting (default: daily at 2 AM)
3. Job iterates through all dealerships with active EasyCars credentials
4. Job executes stock sync for each dealership sequentially or with concurrency control
5. Job logs start/completion for each dealership sync operation
6. Job handles failures for individual dealerships without stopping overall job
7. Job includes mechanism to prevent overlapping executions
8. System administrator can disable automatic sync globally or per-dealership
9. Job execution status visible in system logs for monitoring purposes

### Story 2.5: Create Stock Sync Admin Interface with Manual Trigger

As a dealership administrator,
I want an admin interface to view stock sync status and manually trigger synchronization,
so that I can ensure my vehicle inventory is up-to-date and initiate immediate syncs when needed.

#### Acceptance Criteria

1. "EasyCars Stock Sync" section added to CMS admin with navigation menu item
2. Dashboard displays: last sync timestamp, last sync status (Success/Failed/Warning), records processed in last sync
3. "Sync Now" button triggers manual stock synchronization via API call
4. Button disabled during sync operation with spinner/loading indicator
5. Real-time or polling-based status updates shown during sync operation
6. Success message displayed with summary when sync completes successfully
7. Error message displayed with details when sync fails
8. Sync history table shows last 10 sync operations with timestamps, status, and record counts
9. "View Details" link for each sync log entry opens modal/page with full sync log details
10. Interface indicates if no credentials configured with link to credential management
11. Interface responsive on desktop, tablet, and mobile devices
12. Frontend tests cover UI interactions and API integration

### Story 2.6: Implement Image Synchronization for Stock Items

As a backend developer,
I want to implement image synchronization for stock items,
so that vehicle photos from EasyCars are available in our system for display on dealership websites.

#### Acceptance Criteria

1. Image download service created to retrieve images from URLs in ImageURLs field
2. Service downloads images asynchronously during or after stock sync operation
3. Images stored in system's media storage (file system, cloud storage, etc.)
4. Image records linked to vehicle records in database
5. Service handles missing images gracefully (logs warning, continues processing)
6. Service implements duplicate image detection to avoid re-downloading unchanged images
7. Service respects reasonable rate limits to avoid overwhelming EasyCars servers
8. Image sync operation included in overall sync status and logging
9. Unit tests cover image download success, failure, and duplicate detection
10. Configuration option to enable/disable image sync (enabled by default)
