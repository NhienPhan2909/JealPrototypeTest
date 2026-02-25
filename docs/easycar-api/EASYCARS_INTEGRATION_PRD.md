# EasyCars API Integration Product Requirements Document (PRD)

## Goals and Background Context

### Goals

- Enable seamless bi-directional integration between the dealership management system and EasyCars platform for vehicle inventory and customer leads
- Provide dealership administrators with secure credential management capabilities through the CMS admin interface
- Implement automated synchronization mechanisms to maintain data consistency between systems with minimal manual intervention
- Store complete EasyCars API data to maintain system consistency and enable future analytics and reporting capabilities
- Deliver a reliable and maintainable integration that scales with multiple dealerships and their respective EasyCars accounts

### Background Context

The dealership management system currently operates independently from EasyCars, a platform used by many dealerships for inventory management and lead generation. This creates significant operational inefficiencies as staff must manually enter vehicle stock information and customer leads into multiple systems, leading to data inconsistencies, increased labor costs, and potential loss of sales opportunities due to outdated information.

By integrating with both the EasyCars Stock API and Lead API, the system will automatically synchronize vehicle inventory and customer lead data, eliminating duplicate data entry and ensuring real-time accuracy across platforms. Each dealership maintains its own EasyCars account with unique credentials (Account Number/PublicID and Account Secret/SecretKey), requiring a multi-tenant approach to credential management and API interactions.

### Change Log

| Date | Version | Description | Author |
|------|---------|-------------|--------|
| 2025-01-15 | 1.0 | Initial PRD creation for EasyCars API Integration | John (BMad PM) |

## Requirements

### Functional

**FR1:** The system shall store EasyCars credentials (Account Number/PublicID and Account Secret/SecretKey) securely for each dealership in the database with encryption at rest.

**FR2:** The CMS admin interface shall provide a "Dealership Settings" section where authorized administrators can view, create, update, and delete EasyCars credentials for their dealership.

**FR3:** The system shall implement JWT token-based authentication for all EasyCars API requests following the RequestToken endpoint specification (10-minute token validity).

**FR4:** The system shall integrate with the EasyCars Stock API to retrieve vehicle inventory data including all fields specified in the GetAdvertisementStocks endpoint response.

**FR5:** The system shall store all EasyCars Stock API fields in the database to maintain consistency with the source system, including: YardCode, YardName, StockNumber, Make, Model, Badge, RegoNum, VIN, Price, YearGroup, Odometer, Body, Colour, EngineCapacity, GearType, FuelType, AdvDescription, Series, AdvSpecialPrice, IsDemo, IsSpecial, IsPrestiged, StockType, IsUsed, RegoExpiry, VideoLink, DriveTrain, DoorNum, Cylinder, ShortDescription, EngineTypeDescription, EngineSize, StockStatus, GCM, GVM, Tare, SleepingCapacity, Toilet, Shower, AirConditioning, Fridge, Stereo, SeatCapacity, RegoState, AdditionalDescription, IsDriveAway, InteriorColor, BuiltDate, ComplianceDate, EngineNumber, GPS, Wheelsize, TowBallWeight, Warranty, Wheels, AxleConfiguration, Location, StandardFeatures, OptionalFeatures, RedbookCode, NVIC, IsMiles, GearCount, EnginePower, PowerkW, Powerhp, EngineMake, SerialNumber, Length, IsDepositTaken, ImageCount, and ImageURLs.

**FR6:** The system shall integrate with the EasyCars Lead API to synchronize customer leads including CreateLead, UpdateLead, and GetLeadDetail operations.

**FR7:** The system shall store all EasyCars Lead API fields in the database including: LeadNumber, StockNumber, VehicleMake, VehicleModel, VehiclePrice, VehicleType, VehicleYear, IsVehicleNew, VehicleInterest, Notes, VehicleSourcePageURL, FinanceStatus, Rating, ExternalID, CreatedDateTime, CustomerNo, CustomerName, CustomerEmail, CustomerAddress, CustomerCity, CustomerState, CustomerPostCode, CustomerFax, CustomerPhone, and CustomerMobile.

**FR8:** The system shall implement periodic automatic synchronization at configurable intervals (hourly, daily, or custom) to keep the local database synchronized with EasyCars data.

**FR9:** The CMS admin interface shall provide manual sync trigger buttons for both Stock and Lead data, allowing administrators to initiate immediate synchronization on demand.

**FR10:** The system shall display sync status and history in the admin interface, including last sync timestamp, sync duration, records processed, and any errors encountered.

**FR11:** The system shall implement intelligent sync logic to detect changes and only update modified records, reducing API calls and database operations.

**FR12:** The system shall map EasyCars Stock data to the existing vehicle inventory schema, creating or updating vehicle records as appropriate.

**FR13:** The system shall map EasyCars Lead data to the existing customer lead management system, creating new lead records and updating customer information as appropriate.

**FR14:** The system shall support test and production EasyCars API environments with separate base URLs (testmy.easycars.com.au for test, my.easycars.net.au for production).

**FR15:** The system shall handle EasyCars API response codes appropriately: Success (0), AuthenticationFail (1), Warning (5), Failed (7), and SystemError (9), with proper error logging and user notification.

**FR16:** The system shall support filtering stock synchronization by YardCode when configured, allowing dealerships to sync only specific yard inventories.

**FR17:** The system shall handle image synchronization from EasyCars, downloading and storing vehicle images referenced in the ImageURLs field.

**FR18:** The system shall track the source of each vehicle and lead record (e.g., "EasyCars", "Manual Entry") to support data lineage and conflict resolution.

**FR19:** The admin interface shall display clear indicators showing which vehicles and leads originated from EasyCars synchronization.

**FR20:** The system shall provide data reconciliation reports showing discrepancies between local data and EasyCars data, with options to resolve conflicts.

### Non Functional

**NFR1:** All EasyCars API credentials must be encrypted using AES-256 encryption at rest and transmitted over HTTPS/TLS 1.2 or higher.

**NFR2:** The synchronization process shall be designed as a background job that does not impact front-end user experience or response times.

**NFR3:** The system shall implement exponential backoff retry logic for failed API calls, with a maximum of 3 retry attempts before marking the sync as failed.

**NFR4:** API integration code shall be modular and loosely coupled to facilitate maintenance, testing, and potential future integration with other third-party systems.

**NFR5:** All API interactions shall be logged with appropriate detail levels (info, warning, error) to support troubleshooting and audit requirements.

**NFR6:** The synchronization process shall handle API rate limits gracefully, implementing throttling if necessary to stay within EasyCars API usage limits.

**NFR7:** The system shall maintain backward compatibility with existing vehicle and lead data structures, ensuring the integration does not break existing functionality.

**NFR8:** The integration shall support horizontal scaling to handle multiple dealerships synchronizing simultaneously without performance degradation.

**NFR9:** Database schema changes for EasyCars integration shall use migrations to ensure safe deployment and rollback capabilities.

**NFR10:** The system shall implement comprehensive error handling to prevent partial data corruption during synchronization failures, using database transactions where appropriate.

**NFR11:** The admin interface for credential management and sync operations shall be responsive and work on desktop, tablet, and mobile devices.

**NFR12:** API integration performance shall be monitored with metrics including sync duration, API response times, success/failure rates, and data throughput.

## User Interface Design Goals

### Overall UX Vision

The EasyCars integration features will be seamlessly integrated into the existing CMS admin interface with minimal learning curve for dealership administrators. The design prioritizes clarity and confidence, showing administrators exactly what data is being synchronized, when it was last synced, and providing transparent status reporting. The interface should make administrators feel in control, with clear manual override options and visual indicators of sync health.

### Key Interaction Paradigms

- **Progressive Disclosure:** Advanced configuration options (sync intervals, yard filtering) hidden behind expandable sections or secondary screens to avoid overwhelming users with the simple credential setup process
- **Status-First Display:** Sync status and last successful sync time prominently displayed before action buttons to provide context before actions
- **Confirmation for Destructive Actions:** Manual sync operations that could overwrite local changes require clear confirmation dialogs explaining the implications
- **Real-time Feedback:** Sync operations show progress indicators and provide streaming status updates rather than blocking the interface

### Core Screens and Views

- **Dealership Settings - EasyCars Credentials:** Form for entering and managing Account Number/PublicID and Account Secret/SecretKey with validation and test connection capability
- **EasyCars Sync Dashboard:** Overview showing sync status for both Stock and Lead APIs, last sync times, record counts, and quick access to manual sync triggers
- **Sync History Log:** Table view of past synchronization operations with filtering by date, type (Stock/Lead), and status (Success/Failed/Warning)
- **Sync Configuration:** Settings page for configuring automatic sync intervals, yard filtering, and conflict resolution preferences
- **Vehicle Inventory List (Enhanced):** Existing vehicle list view with additional column/indicator showing EasyCars sync status and last sync timestamp
- **Lead Management List (Enhanced):** Existing lead list view with additional indicator showing which leads originated from EasyCars

### Accessibility

WCAG AA compliance following the existing system accessibility standards, ensuring all sync status indicators have text alternatives and color is not the only means of conveying information.

### Branding

Integration features will follow the existing CMS admin design system and branding guidelines, maintaining visual consistency with current dealership settings and management interfaces. EasyCars branding (logo, colors) may be incorporated subtly in the credentials section to provide visual recognition.

### Target Device and Platforms

Web Responsive - The admin interface must work seamlessly on desktop (primary use case for administrators), tablets, and mobile devices for on-the-go monitoring and manual sync triggering.

## Technical Assumptions

### Repository Structure

Monorepo - The EasyCars integration will be implemented within the existing monorepo structure with backend integration code in the backend/backend-dotnet directories and frontend admin interface enhancements in the frontend directory.

### Service Architecture

The integration will follow the existing Monolith architecture pattern with:
- RESTful API endpoints in the backend for credential management and sync operations
- Background job scheduler (using existing job infrastructure or implementing one) for periodic synchronization
- Service layer pattern isolating EasyCars API client logic from business logic
- Database models for storing EasyCars credentials and sync audit logs

### Testing Requirements

- Unit tests for EasyCars API client classes, credential encryption/decryption, and data mapping logic
- Integration tests using mock EasyCars API responses to test sync workflows end-to-end
- Manual testing convenience method to trigger sync operations with provided test credentials (PublicID: AA20EE61-5CFA-458D-9AFB-C4E929EA18E6, SecretKey: 7326AF23-714A-41A5-A74F-EC77B4E4F2F2)
- E2E tests covering admin interface credential management and manual sync trigger workflows

### Additional Technical Assumptions and Requests

- **API Client Library:** Implement a dedicated EasyCars API client class/module handling authentication token management, request/response serialization, and error handling
- **Environment Variable Support:** EasyCars API base URLs should be configurable via environment variables to support test/production environments and potential future staging environment
- **Database Schema:** New tables required: `dealership_easycar_credentials` (encrypted credentials), `easycar_stock_data` (complete Stock API response storage), `easycar_lead_data` (complete Lead API response storage), `easycar_sync_log` (audit trail of sync operations)
- **Background Job Framework:** Use existing job scheduler if available (e.g., Hangfire for .NET, node-cron for Node.js), or implement a simple scheduler for periodic sync execution
- **Idempotency:** Sync operations must be idempotent to handle duplicate execution scenarios safely
- **Data Retention:** Consider policy for EasyCars audit log retention (e.g., keep last 90 days of sync logs)
- **Observability:** Integrate with existing logging framework and consider adding metrics/monitoring for sync health dashboard
- **Credential Validation:** Implement "Test Connection" functionality allowing administrators to verify credentials before saving
- **Migration Path:** Provide migration scripts for existing dealerships to onboard to EasyCars integration without disrupting current operations

## Epic List

**Epic 1: Foundation & Credential Management**
Establish database schema, credential encryption infrastructure, and admin interface for secure EasyCars credential management with test connection capability.

**Epic 2: Stock API Integration & Synchronization**
Implement EasyCars Stock API client, data mapping, periodic sync scheduler, and admin interface for stock synchronization with manual triggers and status monitoring.

**Epic 3: Lead API Integration & Synchronization**
Implement EasyCars Lead API client with CreateLead, UpdateLead, and GetLeadDetail operations, data mapping to customer lead system, and admin interface for lead synchronization.

**Epic 4: Sync Monitoring & Data Reconciliation**
Build comprehensive sync monitoring dashboard, historical sync logs, data reconciliation reports, and conflict resolution tools to provide administrators full visibility and control over integration health.

## Epic 1: Foundation & Credential Management

Establish the foundational infrastructure for EasyCars integration including database schema for storing encrypted credentials and sync audit logs, credential encryption/decryption services, and admin interface components for secure credential management. This epic delivers immediate value by allowing dealerships to configure their EasyCars connection and verify it works before any automatic synchronization occurs.

### Story 1.1: Design and Implement Database Schema for EasyCars Integration

As a system architect,
I want to design and implement the database schema for EasyCars integration,
so that we have a solid foundation to store credentials, sync data, and audit logs.

#### Acceptance Criteria

1. Database migration created with table `dealership_easycar_credentials` containing fields: id (PK), dealership_id (FK), account_number (encrypted), account_secret (encrypted), environment (enum: test/production), is_active (boolean), created_at, updated_at
2. Database migration created with table `easycar_sync_log` containing fields: id (PK), dealership_id (FK), sync_type (enum: stock/lead), sync_status (enum: success/failed/warning), started_at, completed_at, records_processed, error_message (nullable), request_payload (JSON), response_summary (JSON)
3. Database migration created with table `easycar_stock_data` containing all 70+ fields from EasyCars Stock API response mapped to appropriate data types
4. Database migration created with table `easycar_lead_data` containing all 20+ fields from EasyCars Lead API response mapped to appropriate data types
5. Foreign key relationships established correctly with cascade delete rules where appropriate
6. Indexes created on frequently queried fields (dealership_id, sync_status, created_at, StockNumber, LeadNumber)
7. Migration includes rollback capability
8. Database schema documentation added to architecture docs with field descriptions

### Story 1.2: Implement Credential Encryption Service

As a backend developer,
I want to implement a secure credential encryption/decryption service,
so that EasyCars credentials are protected at rest using industry-standard encryption.

#### Acceptance Criteria

1. Service class created (e.g., `CredentialEncryptionService`) with methods `encrypt(plaintext)` and `decrypt(ciphertext)`
2. AES-256-GCM encryption algorithm implemented with secure key management
3. Encryption keys stored securely using environment variables or secure key management system (not hardcoded)
4. Service generates unique initialization vector (IV) for each encryption operation
5. Unit tests cover encryption/decryption round-trip, handling of empty strings, and error cases
6. Service throws meaningful exceptions for encryption/decryption failures
7. Documentation added explaining key rotation strategy and recovery procedures

### Story 1.3: Create Backend API Endpoints for Credential Management

As a backend developer,
I want to create RESTful API endpoints for managing EasyCars credentials,
so that the admin frontend can perform CRUD operations securely.

#### Acceptance Criteria

1. POST `/api/admin/easycars/credentials` endpoint created to save dealership credentials with authentication/authorization middleware
2. GET `/api/admin/easycars/credentials` endpoint created to retrieve credentials for authenticated dealership (returns account number only, never returns decrypted secret in GET response)
3. PUT `/api/admin/easycars/credentials/:id` endpoint created to update existing credentials
4. DELETE `/api/admin/easycars/credentials/:id` endpoint created to remove credentials
5. All endpoints validate user has admin permissions for the dealership
6. Request validation implemented ensuring account_number and account_secret meet format requirements
7. Responses return appropriate HTTP status codes (200, 201, 400, 401, 403, 404, 500)
8. API endpoint integration tests created covering success and error scenarios
9. Endpoints log all credential management operations for audit purposes

### Story 1.4: Implement Test Connection Functionality

As a backend developer,
I want to implement a "test connection" function that validates EasyCars credentials,
so that administrators can verify their credentials work before saving them.

#### Acceptance Criteria

1. POST `/api/admin/easycars/test-connection` endpoint created accepting account_number, account_secret, and environment (test/production)
2. Endpoint calls EasyCars RequestToken API with provided credentials
3. Returns success response if token is obtained successfully
4. Returns detailed error message if authentication fails (AuthenticationFail response code 1)
5. Returns error message if API is unreachable or times out
6. Test connection does not save credentials to database
7. Operation completes within 10 seconds with timeout handling
8. Endpoint logs test connection attempts with success/failure status
9. Unit tests cover successful connection, authentication failure, and timeout scenarios

### Story 1.5: Build Admin Interface for EasyCars Credential Management

As a dealership administrator,
I want a user interface to manage my EasyCars credentials,
so that I can configure the integration between my dealership and EasyCars.

#### Acceptance Criteria

1. New "EasyCars Settings" section added to Dealership Settings page in CMS admin
2. Form displays with fields: Account Number/PublicID (text input), Account Secret/SecretKey (password input), Environment (dropdown: Test/Production)
3. "Test Connection" button triggers test connection API call and displays success/failure message
4. "Save Credentials" button disabled until successful test connection (optional: can be overridden with checkbox)
5. Form displays current credentials status (configured/not configured) without showing the secret
6. "Update Credentials" flow allows changing credentials with re-testing before save
7. "Delete Credentials" button with confirmation dialog ("Are you sure? This will stop all EasyCars synchronization.")
8. Form validation provides helpful error messages for invalid inputs
9. Loading states shown during API calls (test connection, save, delete)
10. Success/error toast notifications displayed after operations complete
11. Interface is responsive and works on desktop, tablet, and mobile viewports
12. Frontend component tests cover form interactions and API integration

### Story 1.6: Create EasyCars API Client Base Infrastructure

As a backend developer,
I want to create the foundational EasyCars API client infrastructure,
so that subsequent stories can build on a consistent, well-architected API integration pattern.

#### Acceptance Criteria

1. Base API client class created (e.g., `EasyCarsApiClient`) with configuration for test/production environments
2. Token management implemented with automatic request/refresh of JWT tokens (10-minute expiry)
3. HTTP client configured with appropriate timeouts, retry logic (3 attempts with exponential backoff), and error handling
4. Request method created for authenticated API calls with "Bearer {token}" header formatting
5. Response parsing handles all EasyCars response codes (0, 1, 5, 7, 9) with appropriate exception types
6. Client logs all API requests/responses at appropriate log levels (debug for request details, error for failures)
7. Environment-specific base URLs configurable via environment variables
8. Unit tests using mocked HTTP responses cover token acquisition, authenticated requests, token expiry/refresh, and error handling
9. Client handles network errors gracefully with meaningful error messages

## Epic 2: Stock API Integration & Synchronization

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

## Epic 3: Lead API Integration & Synchronization

Implement complete integration with EasyCars Lead API including CreateLead, UpdateLead, and GetLeadDetail operations to synchronize customer leads bi-directionally between systems. This epic enables dealerships to automatically capture leads generated through EasyCars into their CRM, reducing response time to potential customers and ensuring no leads fall through the cracks due to manual data entry delays.

### Story 3.1: Implement Lead API Operations in Client

As a backend developer,
I want to implement CreateLead, UpdateLead, and GetLeadDetail methods in the EasyCars client,
so that the system can manage leads through the EasyCars API.

#### Acceptance Criteria

1. Method `createLead(request)` added to EasyCarsApiClient accepting CreateLeadRequest model
2. Method `updateLead(request)` added accepting UpdateLeadRequest model with LeadNumber
3. Method `getLeadDetail(accountNumber, accountSecret, leadNumber)` added to retrieve lead details
4. All methods construct proper POST requests with authentication and JSON payloads
5. Request models include all required fields per API specification (CustomerNo or Name-Email pair, vehicle details, etc.)
6. Response parsing extracts LeadNumber and CustomerNo from successful responses
7. Methods handle all EasyCars response codes with appropriate exceptions
8. Unit tests with mocked responses cover successful operations and error cases for all three methods
9. Integration test using test credentials validates creating, updating, and retrieving a lead

### Story 3.2: Create Data Mapping Service for Lead Data

As a backend developer,
I want to create a service that maps between our lead data model and EasyCars lead format,
so that leads can be synchronized bi-directionally with proper data transformation.

#### Acceptance Criteria

1. Service class created (e.g., `EasyCarsLeadMapper`) with methods `mapToEasyCarsLead(localLead)` and `mapFromEasyCarsLead(easyCarsLead)`
2. `mapToEasyCarsLead` transforms local lead data into CreateLeadRequest or UpdateLeadRequest format
3. `mapFromEasyCarsLead` transforms GetLeadDetail response into local lead data model
4. Mapping handles customer information (name, email, phone, address) with proper field alignment
5. Mapping handles vehicle information (make, model, year, price, type, interest type)
6. Mapping handles enums correctly (VehicleType: 1-6, VehicleInterest: 1-5, FinanceStatus: 1-3, Rating: 1-3)
7. Mapping stores complete EasyCars raw lead data in `easycar_lead_data` table
8. Duplicate detection logic identifies existing leads by LeadNumber or ExternalID
9. Unit tests cover field mapping accuracy, enum conversions, and null handling

### Story 3.3: Implement Outbound Lead Sync (Local to EasyCars)

As a backend developer,
I want to implement outbound lead synchronization that sends new leads to EasyCars,
so that leads created in our system are automatically pushed to EasyCars for dealership processing.

#### Acceptance Criteria

1. Service method `syncLeadToEasyCars(leadId)` created that sends local lead to EasyCars
2. Service retrieves lead data and dealership credentials from database
3. Service uses mapper to transform lead into CreateLeadRequest format
4. Service calls EasyCars CreateLead API and stores returned LeadNumber
5. Service updates local lead record with EasyCars LeadNumber for tracking
6. Service creates audit log entry for the sync operation
7. Service handles scenarios where lead already exists in EasyCars (use UpdateLead)
8. Event hook or trigger automatically calls sync service when new lead created locally
9. Service implements retry logic for transient failures
10. Unit tests cover successful sync, duplicate detection, and error scenarios

### Story 3.4: Implement Inbound Lead Sync (EasyCars to Local)

As a backend developer,
I want to implement inbound lead synchronization that retrieves leads from EasyCars,
so that leads generated through EasyCars are automatically captured in our CRM system.

#### Acceptance Criteria

1. Service class created (e.g., `EasyCarsLeadSyncService`) with method `syncLeadsFromEasyCars(dealershipId)`
2. Service retrieves dealership credentials and calls GetLeadDetail for new/updated leads
3. Service uses mapper to transform EasyCars lead data into local lead model
4. Service creates new lead records or updates existing ones based on LeadNumber
5. Service creates/updates customer records with information from lead
6. Service wraps sync in database transaction for atomicity
7. Service creates audit log entry with sync summary
8. Service determines which leads to sync (new leads since last sync, updated leads)
9. Periodic job added to scheduler for regular lead synchronization (e.g., hourly)
10. Unit tests cover successful sync, duplicate handling, and error scenarios

### Story 3.5: Build Lead Sync Admin Interface

As a dealership administrator,
I want an admin interface to view lead sync status and manually trigger synchronization,
so that I can monitor lead integration and initiate immediate syncs when needed.

#### Acceptance Criteria

1. "EasyCars Lead Sync" section added to CMS admin with navigation menu item
2. Dashboard displays: last sync timestamp, last sync status, leads processed in last sync, sync direction indicator
3. "Sync Leads from EasyCars" button triggers inbound lead sync via API call
4. Button disabled during sync with loading indicator
5. Success/error messages displayed with sync summary
6. Sync history table shows last 10 lead sync operations with details
7. Lead management list enhanced with indicator showing EasyCars-sourced leads
8. "View Details" for sync log entries shows processed leads with outcomes
9. Interface indicates if no credentials configured with link to credential setup
10. Interface responsive on all device sizes
11. Frontend tests cover UI interactions and sync operations

### Story 3.6: Implement Lead Status Synchronization

As a backend developer,
I want to implement bi-directional lead status synchronization,
so that lead status updates in either system are reflected in the other system.

#### Acceptance Criteria

1. Service monitors local lead status changes and pushes updates to EasyCars using UpdateLead
2. Service maps local lead statuses to EasyCars LeadStatus enum (10-New, 30-InProgress, 50-Won, 60-Lost, 90-Deleted)
3. Inbound sync checks for lead status changes in EasyCars and updates local leads
4. Status sync respects business rules (e.g., cannot undelete a lead marked as deleted)
5. Conflict resolution strategy implemented when both systems have status changes (configurable: local wins, remote wins, manual review)
6. Audit trail captures all status synchronization events with timestamps
7. Admin interface displays sync conflicts requiring manual resolution
8. Unit tests cover status mapping, conflict detection, and resolution scenarios

## Epic 4: Sync Monitoring & Data Reconciliation

Build comprehensive monitoring, reporting, and reconciliation tools to provide dealership administrators complete visibility into integration health, historical sync operations, data discrepancies, and tools for resolving conflicts. This epic ensures the integration is maintainable, trustworthy, and provides administrators the confidence and control needed to rely on automated synchronization.

### Story 4.1: Create Comprehensive Sync Dashboard

As a dealership administrator,
I want a unified dashboard showing all EasyCars integration activity,
so that I can quickly understand integration health and identify any issues at a glance.

#### Acceptance Criteria

1. Unified "EasyCars Integration Dashboard" page created in CMS admin
2. Dashboard displays sync status cards for both Stock and Lead syncs with color-coded status indicators
3. Each card shows: last sync time, next scheduled sync time, success/failure status, records processed
4. Dashboard displays sync trend chart showing success rate over time (last 30 days)
5. Dashboard shows quick stats: total synced vehicles, total synced leads, active credentials status
6. Alert banner displayed for failed syncs or integration errors requiring attention
7. Quick action buttons: "Sync Stock Now", "Sync Leads Now", "View Full History"
8. Dashboard auto-refreshes every 60 seconds or provides refresh button
9. Dashboard responsive on all device sizes
10. Loading states displayed during data fetching

### Story 4.2: Implement Detailed Sync History and Logs

As a dealership administrator,
I want to view detailed history of all synchronization operations,
so that I can troubleshoot issues and understand what data was synchronized when.

#### Acceptance Criteria

1. "Sync History" page created showing paginated table of all sync operations
2. Table columns: Timestamp, Sync Type (Stock/Lead), Direction (Inbound/Outbound), Status, Records Processed, Duration, Actions
3. Filtering options: Date range, sync type, status, search by record identifiers
4. "View Details" action opens detailed view showing: full request/response logs, processed items, error messages, stack traces for failures
5. Detailed view displays list of affected records (vehicles or leads) with before/after states
6. Export functionality to download sync logs as CSV or JSON for external analysis
7. Retention policy implemented: logs older than 90 days auto-archived or deleted per configuration
8. Page includes helpful context: "No syncs found" message with link to run first sync
9. Performance optimized for dealerships with thousands of sync log entries
10. Responsive design for mobile/tablet viewing

### Story 4.3: Build Data Reconciliation Report

As a dealership administrator,
I want a reconciliation report comparing local data with EasyCars data,
so that I can identify and resolve discrepancies between the two systems.

#### Acceptance Criteria

1. "Data Reconciliation" feature added to EasyCars Integration section
2. "Run Reconciliation" button triggers comparison between local and EasyCars data
3. Report identifies: vehicles in EasyCars but not locally, vehicles locally but not in EasyCars, vehicles with differing field values
4. Report identifies: leads in EasyCars but not locally, leads locally but not in EasyCars, leads with status mismatches
5. Report displays discrepancies in categorized tables with severity indicators (missing, outdated, conflicting)
6. Each discrepancy row provides actions: "Sync from EasyCars", "Push to EasyCars", "Ignore", "View Details"
7. Batch actions allow resolving multiple discrepancies at once
8. Reconciliation can be scheduled to run automatically (e.g., weekly) with email notification if discrepancies found
9. Report exportable as PDF or CSV for record-keeping
10. Service implements efficient comparison logic to handle large datasets without performance issues

### Story 4.4: Implement Conflict Resolution Tools

As a dealership administrator,
I want tools to resolve data conflicts when both systems have been modified,
so that I can maintain data integrity and make informed decisions about which data to keep.

#### Acceptance Criteria

1. Conflict resolution interface displays side-by-side comparison of conflicting records
2. Interface shows: local version, EasyCars version, last modified timestamps, field-by-field differences highlighted
3. Resolution options provided: "Use Local", "Use EasyCars", "Merge (select fields)", "Manual Edit"
4. "Merge" option allows field-by-field selection with preview before applying
5. Conflict resolution actions logged in audit trail with user ID and timestamp
6. Bulk conflict resolution workflow for handling multiple conflicts efficiently
7. Conflicts automatically detected and flagged during sync operations
8. Email/notification sent to administrators when conflicts are detected requiring manual review
9. Configuration option for default conflict resolution strategy (local priority vs. remote priority)
10. Unit tests cover conflict detection logic and resolution application

### Story 4.5: Implement Sync Performance Monitoring and Alerts

As a system administrator,
I want monitoring and alerting for EasyCars integration performance,
so that I can proactively identify and resolve issues before they impact dealership operations.

#### Acceptance Criteria

1. Metrics collection implemented for: sync duration, API response times, success/failure rates, records per minute throughput
2. Metrics stored in time-series format for trend analysis
3. Dashboard displays performance metrics with charts: average sync time, API latency trends, success rate over time
4. Alert rules configurable: sync failures exceeding threshold, sync duration exceeding threshold, API errors rate
5. Alerts sent via email or integrated notification system when rules triggered
6. Health check endpoint created: `/api/admin/easycars/health` returning integration status
7. Monitoring dashboard shows: current sync operations in progress, queue depth if applicable, system resource usage
8. Historical performance data retained for at least 90 days for trend analysis
9. Performance metrics help identify API throttling or rate limiting issues
10. Admin documentation includes troubleshooting guide referencing monitoring metrics

### Story 4.6: Create Integration Documentation and Admin Guide

As a dealership administrator and system administrator,
I want comprehensive documentation for EasyCars integration,
so that I can configure, monitor, and troubleshoot the integration effectively.

#### Acceptance Criteria

1. User guide document created covering: credential setup, initial sync, monitoring sync status, manual sync triggers
2. Administrator guide created covering: configuration options, troubleshooting common issues, understanding sync logs
3. API documentation updated with EasyCars integration endpoints
4. Database schema documentation includes EasyCars-related tables with field descriptions
5. Architecture documentation includes integration architecture diagram showing data flow
6. Troubleshooting guide includes: common error messages and resolutions, how to handle sync failures, how to re-sync data
7. Documentation includes screenshots of admin interface sections
8. FAQ section addresses: credential security, sync frequency recommendations, handling large datasets, data ownership
9. Documentation published in system's documentation site or included in repository
10. Documentation includes contact information for EasyCars support and internal technical support

## Checklist Results Report

*This section will be populated after executing the PM checklist to validate the PRD completeness and quality.*

## Next Steps

### UX Expert Prompt

Please review this EasyCars API Integration PRD and create detailed UX specifications for the admin interface components, including wireframes for the credential management section, sync dashboard, and data reconciliation interface. Focus on providing dealership administrators intuitive, confidence-inspiring controls for managing this critical integration.

### Architect Prompt

Please review this EasyCars API Integration PRD and create a comprehensive technical architecture document. Design the system architecture covering: database schema details with indexes and constraints, API client architecture with token management and retry logic, background job scheduling strategy, data mapping and transformation layer, error handling and logging strategy, security implementation for credential encryption, and deployment considerations for test/production environments. Consider scalability for multiple dealerships and maintainability for future third-party integrations.
