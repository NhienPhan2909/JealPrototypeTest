# Epic 1: Backend Infrastructure & Multi-Tenant API

**Epic Goal:** Establish the foundational backend infrastructure including project setup, database schema, and REST API with multi-tenancy support. Deliver core CRUD operations for vehicles, dealerships, and leads with proper data isolation via `dealershipId` filtering. Enable Cloudinary integration for image upload handling, providing a testable API layer that both public website and admin CMS will consume.

## Story 1.1: Project Setup & Basic Server

**As a** developer,
**I want** to initialize the monorepo structure and create a basic Express server with health check endpoint,
**so that** I have a working foundation to build the API upon and can verify the server runs successfully.

### Acceptance Criteria

1. Monorepo structure created with `/backend` and `/frontend` folders
2. Root `package.json` includes scripts for running backend (`npm run server`) and development mode
3. Backend dependencies installed: Express, pg, dotenv, nodemon, Cloudinary SDK
4. Express server runs on port 5000 (configurable via environment variable)
5. Health check endpoint `GET /api/health` returns `{ status: 'ok', timestamp: <ISO date> }`
6. Server can be started with `npm run server` and restarts automatically on file changes (nodemon)
7. `.env` file template created with placeholders for DATABASE_URL, CLOUDINARY credentials
8. `.gitignore` includes `node_modules`, `.env`, and other appropriate exclusions
9. Git repository initialized with initial commit

## Story 1.2: Database Schema & Seed Data

**As a** developer,
**I want** to create the PostgreSQL database schema and seed initial dealership data,
**so that** I have the multi-tenant data model ready for API development and testing.

### Acceptance Criteria

1. PostgreSQL database connection established using `pg` client with `DATABASE_URL` from environment variables
2. `Dealership` table created with columns: `id` (serial primary key), `name` (varchar), `logo_url` (varchar nullable), `address` (text), `phone` (varchar), `email` (varchar), `hours` (text), `about` (text), `created_at` (timestamp)
3. `Vehicle` table created with columns: `id` (serial primary key), `dealership_id` (integer FK to Dealership), `make` (varchar), `model` (varchar), `year` (integer), `price` (decimal), `mileage` (integer), `condition` (varchar: new/used), `status` (varchar: active/sold/pending/draft), `title` (varchar), `description` (text), `images` (jsonb array of URLs), `created_at` (timestamp)
4. `Lead` table created with columns: `id` (serial primary key), `dealership_id` (integer FK to Dealership), `vehicle_id` (integer FK to Vehicle, nullable), `name` (varchar), `email` (varchar), `phone` (varchar), `message` (text), `created_at` (timestamp)
5. Foreign key constraints enforce referential integrity (`dealership_id` in Vehicle and Lead must reference valid Dealership)
6. Indexes created on `dealership_id` columns in Vehicle and Lead tables for query performance
7. Database seeded with 2 sample dealerships: "Acme Auto Sales" and "Premier Motors" with complete profile information
8. Database connection can be tested with simple query (e.g., `SELECT * FROM Dealership`)

## Story 1.3: Dealership API Endpoints (CRUD)

**As a** frontend developer,
**I want** to retrieve and update dealership information via API endpoints,
**so that** I can display dealership details on the public website and enable dealership staff to update their profile in the admin CMS.

### Acceptance Criteria

1. `GET /api/dealers` endpoint returns array of all dealerships with all fields (id, name, logo_url, address, phone, email, hours, about)
2. `GET /api/dealers/:id` endpoint returns single dealership by ID with all fields
3. If dealership ID does not exist, `GET /api/dealers/:id` returns 404 status with error message `{ error: 'Dealership not found' }`
4. API responses include proper HTTP status codes (200 for success, 404 for not found, 500 for server errors)
5. API endpoints tested with Postman or curl, returning seeded dealership data correctly
6. Error handling implemented: database errors return 500 status with generic error message (no sensitive info leaked)
7. `PUT /api/dealers/:id` endpoint updates dealership fields (name, logo_url, address, phone, email, hours, about) and returns updated dealership
8. Input validation ensures required fields present for PUT operation (name, address, phone, email required - return 400 Bad Request if missing)
9. PUT endpoint tested: update Dealership A name and about text, verify change persists in database and GET request returns updated data

## Story 1.4: Vehicle CRUD API

**As a** frontend developer,
**I want** to create, read, update, and delete vehicles via API with proper multi-tenant filtering,
**so that** the public website can display vehicles and the admin CMS can manage inventory.

### Acceptance Criteria

1. `POST /api/vehicles` endpoint creates new vehicle with all required fields (dealership_id, make, model, year, price, mileage, condition, status, title, description, images) and returns created vehicle with generated ID
2. `GET /api/vehicles?dealershipId=<id>` endpoint returns array of vehicles filtered by `dealershipId` query parameter
3. `GET /api/vehicles?dealershipId=<id>&status=active` endpoint supports optional `status` filter (active/sold/pending/draft)
4. `GET /api/vehicles/:id?dealershipId=<id>` endpoint returns single vehicle by ID with all fields, ONLY if vehicle belongs to specified dealership (returns 404 if vehicle exists but belongs to different dealership)
5. `PUT /api/vehicles/:id?dealershipId=<id>` endpoint updates existing vehicle (requires request body with updated fields and dealershipId query parameter), ONLY if vehicle belongs to specified dealership (returns 404 if vehicle exists but belongs to different dealership), and returns updated vehicle
6. `DELETE /api/vehicles/:id?dealershipId=<id>` endpoint deletes vehicle by ID ONLY if vehicle belongs to specified dealership (returns 404 if vehicle exists but belongs to different dealership), and returns 204 No Content status
7. If vehicle ID does not exist for update/delete operations, API returns 404 status with error message
8. Input validation ensures required fields are present for POST/PUT operations (return 400 Bad Request if missing)
9. Multi-tenancy enforced: `dealershipId` parameter is required for ALL endpoints (GET list, GET single, PUT, DELETE) and returns 400 if missing; ensures vehicles can only be accessed/modified by their owning dealership
10. API endpoints tested with Postman/curl: create vehicle for Dealership A, verify it appears in GET list for Dealership A but NOT in list for Dealership B; test cross-dealership access protection (attempt to GET/PUT/DELETE Dealership A vehicle using Dealership B's dealershipId should return 404)

**Version History:**
- **v1.0 (2025-11-20):** Initial story definition
- **v1.1 (2025-11-20):** CRITICAL SEC-001 SECURITY ENHANCEMENT - Extended dealership_id filtering to ALL operations (not just list queries). Updated ACs 4-6, 9-10 to require dealershipId query parameter for GET/:id, PUT/:id, DELETE/:id endpoints. This prevents cross-dealership data leaks, modifications, and deletions via ID guessing attacks.

## Story 1.5: Lead API Endpoints

**As a** car buyer or dealership staff member,
**I want** to submit enquiries via the public website and retrieve them in the admin CMS,
**so that** dealerships can receive and respond to customer leads.

### Acceptance Criteria

1. `POST /api/leads` endpoint creates new lead with required fields (dealership_id, name, email, phone, message) and optional `vehicle_id` field
2. `POST /api/leads` returns created lead with generated ID and `created_at` timestamp
3. Input validation ensures required fields are present (return 400 Bad Request if missing: dealership_id, name, email, message)
4. Email validation ensures `email` field contains valid email format (basic regex check)
5. `GET /api/leads?dealershipId=<id>` endpoint returns array of leads filtered by `dealershipId` query parameter, sorted by `created_at` descending (newest first)
6. Multi-tenancy enforced: `dealershipId` parameter is required for GET endpoint (returns 400 if missing)
7. API endpoints tested: submit lead for Dealership A with vehicle reference, verify it appears in GET list for Dealership A but NOT in list for Dealership B
8. If `vehicle_id` is provided, endpoint validates that vehicle exists and belongs to the specified dealership (return 400 if mismatch)

## Story 1.6: Cloudinary Image Upload Integration

**As a** dealership staff member,
**I want** to upload vehicle photos that are stored and optimized via Cloudinary,
**so that** vehicle listings display high-quality images without consuming local server storage.

### Acceptance Criteria

1. Cloudinary account created (free tier) and API credentials (cloud_name, api_key, api_secret) stored in `.env` file
2. Cloudinary Node.js SDK configured in backend with credentials from environment variables
3. `POST /api/upload` endpoint accepts image file upload (multipart/form-data) and uploads to Cloudinary
4. `POST /api/upload` returns JSON response with Cloudinary image URL: `{ url: 'https://res.cloudinary.com/...' }`
5. Uploaded images automatically optimized by Cloudinary (format conversion to WebP where supported, quality optimization)
6. File size validation: reject uploads larger than 5MB (return 400 Bad Request with error message)
7. File type validation: only accept JPG, PNG, WebP formats (return 400 for other formats)
8. Error handling: if Cloudinary upload fails, return 500 status with error message
9. Upload endpoint tested with Postman or curl: upload sample vehicle image, verify Cloudinary URL returned and image accessible at URL

## Story 1.7: Multi-Tenancy Validation & API Testing

**As a** developer and QA tester,
**I want** to validate that multi-tenant data isolation works correctly and all API endpoints are properly documented,
**so that** the API is ready for frontend integration with confidence in data security.

### Acceptance Criteria

1. Input validation middleware added to all API endpoints: sanitize user input, validate required fields, return 400 Bad Request for invalid requests
2. All SQL queries use parameterized queries (no string concatenation) to prevent SQL injection
3. Multi-tenancy validation test: Create vehicles for Dealership A and B, verify `GET /api/vehicles?dealershipId=A` returns ONLY Dealership A vehicles
4. Multi-tenancy validation test: Create leads for Dealership A and B, verify `GET /api/leads?dealershipId=A` returns ONLY Dealership A leads
5. Cross-contamination test: Attempt to update Dealership A vehicle via `PUT /api/vehicles/:id` (no additional checks needed for MVP, but noted for Phase 2: verify requester has permission for that dealership)
6. Error handling tested: Invalid dealership ID, missing required fields, malformed requests all return appropriate error responses (400/404/500) with helpful messages
7. API documentation created (simple README or Postman collection) listing all endpoints, required parameters, expected responses, and example requests
8. Database seeded with 5-10 sample vehicles for each dealership to support frontend development and testing

---
