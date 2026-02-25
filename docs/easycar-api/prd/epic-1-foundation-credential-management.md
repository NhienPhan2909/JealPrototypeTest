# Epic 1: Foundation & Credential Management

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
