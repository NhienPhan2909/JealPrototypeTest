# Epic 3: Lead API Integration & Synchronization

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
