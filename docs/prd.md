# Multi-Dealership Car Website + CMS Platform Product Requirements Document (PRD)

**Version:** 1.4
**Date:** 2025-12-12
**Status:** Draft
**Owner:** Product Management

---

## Goals and Background Context

### Goals

- Complete functional 2-dealership prototype with public site + admin CMS deployed to live URL within 2 days (48 hours)
- Demonstrate multi-tenant architecture works seamlessly for 2+ dealerships from single codebase/database instance
- Validate that entire platform can run on free tiers (Cloudinary, PostgreSQL, hosting) without functional limitations for small dealership use case
- Provide affordable, scalable dealership web platform that can be deployed rapidly for real-world dealership customers
- Enable dealership staff to manage vehicle inventory and view customer leads efficiently from integrated admin CMS
- Capture qualified leads from website visitors browsing dealership inventory
- Establish foundation for long-term SaaS platform supporting multiple independent dealerships

### Background Context

Car dealerships of all sizes face significant barriers to establishing effective online presence. Commercial dealership website platforms charge $200-$1000+/month with rigid templates and vendor lock-in, making them prohibitive for small independent lots. Website builders lack inventory management capabilities, and custom development is expensive with long timelines. This creates a competitive disadvantage for smaller dealerships in an increasingly digital automotive retail market.

This PRD addresses the problem by defining a unified, multi-tenant platform combining public-facing dealership websites (vehicle browsing, search, detail pages, lead capture) with an integrated CMS for inventory and dealership management. Built entirely on free, open-source technologies (Node.js/Express, React, PostgreSQL, Cloudinary), the solution targets car dealerships from small independent lots to large multi-site franchises. The 2-day prototype timeline validates rapid deployment feasibility while establishing architecture that scales from single dealership to multi-tenant SaaS platform.

### Change Log

| Date | Version | Description | Author |
|------|---------|-------------|--------|
| 2025-11-19 | 1.0 | Initial PRD draft based on Project Brief | PM |
| 2025-11-28 | 1.1 | Added Dynamic Theme Color Management feature (FR22, NFR15) - moved from post-MVP to MVP scope. Updated Story 3.6 with detailed acceptance criteria for theme color picker, validation, and live preview functionality. Updated database schema and API endpoints to include theme_color field. | PM |
| 2025-11-28 | 1.2 | Added Font Family Customization feature (FR23, NFR16). Enables dealership administrators to customize website typography from CMS admin panel with 10 web-safe font options, live preview, and site-wide application. Updated database schema to include font_family field. Created Story 3.7 documentation. | PM |
| 2025-12-09 | 1.3 | Added General Enquiry Form feature (FR24). Enables website visitors to submit general enquiries from homepage without selecting a specific vehicle. Form positioned side-by-side with vehicle search widget using responsive grid layout. Enquiries appear in admin Lead Inbox with "General Enquiry" label. No backend or database changes required - uses existing leads table with vehicle_id = null. Created Story 6.1 documentation. | PM |
| 2025-12-12 | 1.4 | Added Email Notification Service feature (FR25). When customers submit leads, dealerships automatically receive email notifications with customer details (name, email, phone, vehicle info, message). Implemented using nodemailer with configurable SMTP support for Gmail, SendGrid, and other providers. Email failures handled gracefully - lead creation always succeeds. Created Story 1.8 and EMAIL_SETUP.md documentation. | PM |

---

## Requirements

### Functional Requirements

1. **FR1:** The public website displays a home page with dealership name, tagline, and "Browse Inventory" call-to-action button
2. **FR2:** The vehicle listing page displays all active vehicles for the selected dealership in grid/list view with thumbnail, make/model/year, and price
3. **FR3:** The vehicle listing page provides case-insensitive text search with partial matching across make, model, year, and description fields, plus simple filtering (condition: new/used, sorting by price/year/mileage)
4. **FR4:** The vehicle detail page displays full vehicle information including image gallery, specifications (make, model, year, price, mileage, condition), description, and enquiry form
5. **FR5:** The enquiry form captures customer information (name, email, phone, message) and associates it with the related vehicle and dealership
6. **FR6:** The about/contact page displays dealership information including name, logo, address, phone, email, opening hours, and about text
7. **FR7:** The admin CMS provides a dealership selector (dropdown or URL parameter) allowing admin to switch between dealerships
8. **FR8:** The vehicle manager displays all vehicles for selected dealership with edit/delete actions and create/edit form for all vehicle attributes
9. **FR9:** The vehicle manager integrates image upload (Cloudinary upload widget on desktop, fallback file input on mobile) with mobile-friendly interface for uploading vehicle photos (primary image + gallery)
10. **FR10:** The dealership settings form allows editing of core dealership profile information (name, logo, address, phone, email, about text, opening hours, theme color)
11. **FR11:** Changes to dealership settings are immediately reflected on the public website
12. **FR12:** The lead inbox displays customer enquiries (name, email, phone, message, related vehicle, timestamp) filtered by selected dealership
13. **FR13:** The admin panel is protected by basic authentication (login form with credentials)
14. **FR14:** All database queries filter data by `dealershipId` to ensure multi-tenant data isolation
15. **FR15:** Public site routes use dealership identifier for routing (e.g., `/dealer/:dealershipId/inventory`)
16. **FR16:** The system must validate uploaded images (max file size 5MB, accepted formats: JPG/PNG/WebP) before upload
17. **FR17:** Vehicle status must support multiple states: Active (public-visible), Sold (public-hidden, admin-visible), Pending (public-visible with badge), Draft (admin-only)
18. **FR18:** The vehicle listing page must filter vehicles by status (default: Active + Pending only)
19. **FR19:** The admin vehicle manager must display vehicle status prominently and allow status filtering/sorting
20. **FR20:** When no vehicles exist for a dealership, the vehicle listing page displays helpful empty state: "No vehicles available yet. Check back soon!" with dealership contact info
21. **FR21:** When no vehicles exist in admin vehicle manager, display empty state with prominent "Add Your First Vehicle" call-to-action button
22. **FR22:** The dealership settings form provides a theme color picker allowing administrators to customize their website's primary brand color, with validation for hex color format (#RRGGBB), live preview of header appearance, reset to default functionality, and applies the selected color across all UI elements (buttons, links, headers, badges, focus states) on both public website and admin panel
23. **FR23:** The dealership settings form provides a font family selector allowing administrators to customize the typography for their entire website, with 10 web-safe font options (System Default, Arial, Times New Roman, Georgia, Verdana, Courier New, Comic Sans MS, Trebuchet MS, Impact, Palatino), live preview of selected font, and applies the selected font to all text elements (headers, body text, navigation, buttons, forms) site-wide
24. **FR24:** The homepage displays a general enquiry form positioned to the right of the vehicle search widget, allowing visitors to submit enquiries (name, email, phone, message) without selecting a specific vehicle. The form uses responsive grid layout (side-by-side on desktop ≥1024px, stacked on mobile), validates all fields with inline error messages, displays success confirmation after submission, and stores enquiries in the database with vehicle_id = null. General enquiries appear in the admin Lead Inbox with a "General Enquiry" label in the Vehicle column.
25. **FR25:** When a customer submits a new lead (enquiry) through the website, the system automatically sends an email notification to the dealership's email address. The notification email includes customer details (name, email, phone), vehicle information (year, make, model) if the lead is for a specific vehicle or "General Enquiry" label, and the customer's message. Email delivery is handled via configurable SMTP service (Gmail, SendGrid, or other providers) with graceful degradation (lead creation succeeds even if email fails).

### Non-Functional Requirements

1. **NFR1:** The platform must run entirely on free tiers (Cloudinary < 25GB storage/bandwidth, PostgreSQL < 1GB data, Railway/Render free tier)
2. **NFR2:** Page load time must be < 3 seconds on standard broadband (target: < 2s for listing pages)
3. **NFR3:** Images must be optimized via Cloudinary CDN with lazy loading and responsive images
4. **NFR4:** The platform must support 100+ vehicle listings per dealership without significant performance degradation
5. **NFR5:** The platform must be compatible with modern browsers (Chrome, Firefox, Safari, Edge—latest 2 versions) on desktop, tablet, and mobile
6. **NFR6:** The platform must be deployed to a live URL (Railway or Render) and remain accessible during demo period with 99%+ uptime
7. **NFR7:** Database schema must use parameterized queries to prevent SQL injection
8. **NFR8:** The platform must complete backend + frontend + deployment within 48-hour window (2-day timeline)
9. **NFR9:** The repository must use monorepo structure with `/backend` and `/frontend` folders
10. **NFR10:** Multi-tenant data must have zero cross-contamination between dealerships (enforced by `dealershipId` filtering and foreign key constraints)
11. **NFR11:** Images must be automatically resized/compressed by Cloudinary to standard dimensions (1920px max width for gallery, 400px for thumbnails)
12. **NFR12:** The admin CMS must be mobile-responsive and fully functional on tablets (iPad) and large phones (iPhone 14 Pro+)
13. **NFR13:** Core admin workflows (add vehicle, view leads, edit dealership settings) must be testable on mobile Safari and Chrome during development
14. **NFR14:** Search must return results with partial matches (e.g., "camry" matches "Toyota Camry 2015")
15. **NFR15:** Theme color changes must apply instantly without page refresh using CSS custom properties, with automatic calculation of hover and background shade variants for consistent visual design
16. **NFR16:** Font family changes must apply site-wide to all text elements with no component refactoring required, using web-safe fonts with universal browser and device compatibility
17. **NFR17:** Email notifications must be sent asynchronously without blocking HTTP responses - lead creation must return within 2 seconds regardless of email delivery time
18. **NFR18:** Email delivery failures must not impact customer experience - customers always receive success response when lead is created, regardless of email outcome (graceful degradation)
19. **NFR19:** Email credentials and configuration must never be logged, exposed in error messages, or committed to source control

---

## User Interface Design Goals

### Overall UX Vision

Clean, professional, and trustworthy design that builds confidence with car buyers while remaining simple enough for dealership staff to manage without technical expertise. Public-facing website prioritizes fast, friction-free vehicle discovery with clear call-to-actions for lead capture. Admin CMS emphasizes efficiency and clarity—dealership staff should be able to add a vehicle listing in under 5 minutes with minimal cognitive load.

**Design Philosophy:** "Get out of the way"—the platform should showcase vehicles and facilitate transactions without drawing attention to itself through overly complex UI or unnecessary features.

### Key Interaction Paradigms

- **Public Website:** Browse-first navigation (immediate access to inventory from home page), progressive disclosure (vehicle cards → detail page → enquiry form), mobile-optimized touch targets for filtering and CTAs
- **Admin CMS:** Dashboard-style layout with clear section navigation (Vehicles, Dealership Settings, Leads), inline editing where possible (click to edit), immediate feedback on actions (success/error toasts), single-page workflows for common tasks (add vehicle without navigating away)
- **Multi-Tenancy UX:** Prominent dealership selector in admin header with clear indication of "currently managing: [Dealership Name]" to prevent accidental cross-dealership edits

### Core Screens and Views

**Public Website:**
1. **Home Page** - Hero section with dealership branding, tagline, "Browse Inventory" CTA
2. **Vehicle Listing Page** - Grid view of vehicles with search/filter controls, sorting options
3. **Vehicle Detail Page** - Large image gallery, specs table, description, prominent enquiry form
4. **About/Contact Page** - Dealership info, map/address, hours, contact methods

**Admin CMS:**
5. **Login Screen** - Simple username/password form
6. **Admin Dashboard** - Overview with dealership selector, quick stats (total vehicles, recent leads), navigation to main sections
7. **Vehicle Manager** - Table/list of all vehicles with inline actions, "Add Vehicle" button, status indicators
8. **Vehicle Create/Edit Form** - Multi-field form with image upload, save/cancel actions
9. **Dealership Settings** - Edit form for dealership profile (name, logo, contact info, hours)
10. **Lead Inbox** - Table of enquiries with sorting/filtering, view lead details

### Accessibility: WCAG AA

Target WCAG 2.1 Level AA compliance to ensure platform is usable by dealership staff and car buyers with disabilities. Key considerations:
- Sufficient color contrast (4.5:1 for text)
- Keyboard navigation for all interactive elements
- Screen reader compatibility (semantic HTML, ARIA labels where needed)
- Form labels and error messages clearly associated with inputs
- Focus indicators visible on all focusable elements

**Rationale:** WCAG AA balances accessibility with implementation effort for 2-day timeline; AAA deferred to Phase 2.

### Branding

Minimal, neutral default theme that doesn't compete with dealership branding. Customizable color palette with dealership-specific theme color for primary UI elements (default: #3B82F6 blue for CTAs, buttons, links) with dealership logo and name prominently displayed. Platform should feel like "the dealership's website" not "a platform hosting dealerships."

**Customization Scope (MVP):** Logo upload, dealership name/tagline customization, and dynamic theme color management. Administrators can select their brand color via color picker in dealership settings, which applies instantly across the entire public website and admin panel. Advanced theming (custom fonts, layouts, typography) deferred to post-MVP.

### Target Device and Platforms: Web Responsive

- **Primary:** Desktop browsers (Chrome, Firefox, Safari, Edge) for admin CMS and public browsing
- **Secondary:** Mobile phones (iOS Safari, Android Chrome) for public website vehicle browsing and enquiry submission
- **Tertiary:** Tablets (iPad, Android tablets) for admin CMS mobile management

**Responsive Breakpoints:**
- Mobile: < 768px (stacked layouts, simplified navigation, touch-optimized)
- Tablet: 768px - 1024px (hybrid layouts, accessible admin panel)
- Desktop: > 1024px (full multi-column layouts, optimized for productivity)

---

## Technical Assumptions

### Repository Structure: Monorepo

**Decision:** Monorepo with `/backend` and `/frontend` folders, shared root `package.json` for dev scripts.

**Rationale:**
- Simplifies development workflow (single `git clone`, unified dependency management)
- Enables running backend + frontend in parallel via `concurrently` for local dev
- Reduces deployment complexity (single Git repository for Railway/Render auto-deploy)
- Appropriate for small team/solo developer and 2-day timeline
- Avoids overhead of polyrepo coordination (multiple repos, separate deployments, version alignment)

**Implementation Details:**
- Root `package.json` with scripts: `npm run dev` (runs both backend and frontend), `npm run build` (builds frontend)
- Backend: Express server in `/backend` with API routes, database models, Cloudinary integration
- Frontend: React app in `/frontend` (public site + admin panel in single codebase, separated by routes)

### Service Architecture: Monolith (Unified Full-Stack Deployment)

**Decision:** Single service deployment combining backend API and frontend static build.

**Rationale:**
- **Speed:** Fastest path to deployment within 2-day timeline (no microservices orchestration, no separate frontend hosting)
- **Cost:** Fits within single free-tier hosting instance (Railway/Render)
- **Simplicity:** Backend serves built React app via `express.static('frontend/build')` in production—no CORS complexity, no separate domain coordination
- **Scalability:** Monolith sufficient for 2-5 dealerships with <100 vehicles each; can refactor to microservices post-MVP if needed

**Architecture Components:**
- **RESTful API Endpoints:** `/api/vehicles`, `/api/dealers`, `/api/leads`, `/api/upload` (Cloudinary integration), `/api/auth` (basic login)
- **Multi-Tenancy:** All vehicle/lead queries scoped by `dealershipId` filtering in API layer
- **Static Asset Serving:** Backend serves frontend build in production; React dev server proxy to backend in development

**Deferred for Post-MVP:**
- Microservices architecture (separate inventory service, lead service, etc.)
- Serverless functions (AWS Lambda, Vercel Functions)
- Separate frontend hosting (Vercel, Netlify)

### Testing Requirements: Manual Testing Only (Prototype)

**Decision:** Developer-led manual testing during 2-day build; no automated test suites for MVP.

**Rationale:**
- **Timeline Constraint:** Writing unit/integration/e2e tests would consume 30-50% of 48-hour budget—unacceptable for prototype
- **Risk Mitigation:** Manual testing sufficient to validate core user journeys (visitor → enquiry submission; admin → add vehicle → view on public site)
- **Post-MVP Plan:** Add automated tests in Phase 2 (Jest for backend API, React Testing Library for components, Playwright for e2e)

**Manual Testing Checklist (Day 2):**
- Multi-tenancy validation: Create/edit/delete vehicles for Dealership A and B, verify no cross-contamination
- End-to-end public journey: Browse listing → view detail → submit enquiry → verify in admin lead inbox
- End-to-end admin journey: Add vehicle with photos → verify on public site, edit dealership settings → verify updates live
- Mobile testing: Test public site and admin panel on iPhone/iPad, verify responsive layouts and image upload
- Browser testing: Chrome, Firefox, Safari (desktop and mobile)

**Test Data Seeding:**
- Seed 2 dealerships with 5-10 sample vehicles each (diverse makes/models/prices)
- Include edge cases: vehicle with 1 photo vs. 10 photos, very long descriptions, no description, sold vehicles

### Additional Technical Assumptions and Requests

**Frontend Stack:**
- **React 18+** with functional components and hooks (no class components)
- **React Router v6** for client-side routing (public routes: `/`, `/inventory`, `/inventory/:id`, `/about`; admin routes: `/admin`, `/admin/vehicles`, `/admin/settings`, `/admin/leads`)
- **Styling:** Tailwind CSS (utility-first framework for rapid UI development) OR plain CSS modules (if Tailwind setup time is concern)
- **Forms:** Controlled components with basic validation; React Hook Form if it accelerates development without complexity
- **HTTP Client:** `fetch` API (built-in) or `axios` for API calls
- **State Management:** React Context + useState/useReducer for global state (dealership selector, auth token); avoid Redux/Zustand for MVP simplicity

**Backend Stack:**
- **Node.js v18+ LTS** (latest stable with long-term support)
- **Express.js** for REST API server
- **pg (node-postgres)** for PostgreSQL database client with parameterized queries (SQL injection protection)
- **Cloudinary Node.js SDK** for image upload handling (or frontend-only Cloudinary upload widget)
- **dotenv** for environment variable management (`.env` file for local dev, platform env vars for production)
- **nodemon** for development hot-reload (restart server on file changes)
- **bcrypt** for password hashing (if implementing proper auth beyond hard-coded credentials)

**Database:**
- **PostgreSQL 14+** (relational database for structured data, multi-tenancy, data integrity)
- **Schema Entities:** `Dealership` (id, name, logo_url, address, phone, email, hours, about, theme_color), `Vehicle` (id, dealership_id FK, make, model, year, price, mileage, condition, status, title, description, images JSON/array), `Lead` (id, dealership_id FK, vehicle_id FK nullable, name, email, phone, message, created_at)
- **Indexes:** `dealership_id` on Vehicle and Lead tables for query performance
- **Hosting:** Supabase free tier, Railway Postgres add-on, or Render Postgres (whichever provides easiest setup + free tier)

**Image Storage & CDN:**
- **Cloudinary Free Tier** (25GB storage, 25GB bandwidth/month)
- **Upload Method:** Cloudinary upload widget (frontend integration) with server-side signature generation for security
- **Image Transformations:** Automatic resizing (1920px max width for gallery, 400px for thumbnails), format optimization (WebP with JPG fallback), lazy loading
- **Fallback Plan:** If Cloudinary widget integration stalls, use simple file upload to local backend storage temporarily (migrate to Cloudinary post-demo)

**Hosting & Deployment:**
- **Primary Choice:** Railway free tier (unified backend + frontend + Postgres, Git-based auto-deploy, generous free tier limits)
- **Alternative:** Render free tier (similar capabilities, slightly different UX)
- **SSL/HTTPS:** Provided automatically by Railway/Render
- **Environment Variables:** Database URL, Cloudinary API credentials, admin auth secret stored as platform env vars
- **Build Process:** `npm run build` in frontend generates production build → backend serves static files

**Security Practices:**
- Parameterized SQL queries (prevent SQL injection)
- Input validation on API endpoints (sanitize user input)
- CORS configuration (allow frontend origin in dev, not needed for unified production deployment)
- Environment variables for secrets (never commit `.env` to Git)
- HTTPS enforcement (platform-provided)
- **Deferred for Post-MVP:** Rate limiting, CSRF tokens, advanced auth (OAuth, SSO), audit logging

**Development Workflow:**
- **Version Control:** Git with GitHub/GitLab repository
- **Branching Strategy:** Simple `main` branch for prototype (no feature branches for 2-day sprint)
- **Deployment:** Git push to `main` triggers auto-deploy on Railway/Render
- **Local Development:** `npm run dev` starts both backend (port 5000) and frontend (port 3000), frontend proxies API requests to backend

**Key Technical Constraints for Architect:**

1. **MUST use free/open-source technologies only** (no licensed software, paid APIs, or commercial platforms)
2. **MUST complete within 48-hour timeline** (any tech choice that adds >2 hours setup time should be reconsidered)
3. **MUST support multi-tenancy via `dealershipId` filtering** (all data access layers must enforce this)
4. **MUST be deployable to Railway or Render free tier** (architecture must fit within free tier CPU/memory/storage limits)
5. **MUST be production-viable** (prototype architecture should not require complete rewrite for real dealership deployment)

---

## Epic List

### Epic 1: Backend Infrastructure & Multi-Tenant API
Establish foundational project setup (monorepo structure, Git repository, database schema), implement backend REST API with multi-tenancy architecture, and deliver core CRUD operations for vehicles, dealerships, and leads with Cloudinary image upload integration.

### Epic 2: Public Dealership Website & Lead Capture
Deliver complete visitor-facing experience including home page, vehicle inventory listing with search/filtering, vehicle detail pages with image galleries, about/contact page, and functional enquiry form that saves leads to database.

### Epic 3: Admin CMS, Dealership Management & Production Deployment
Enable dealership staff to manage vehicle inventory (create/edit/delete with photo uploads), configure dealership settings (name, logo, contact info, hours, brand theme color), view customer leads, switch between dealerships (multi-tenancy UI), and deploy the complete platform to live hosting (Railway/Render) with seeded demo data for 2 dealerships.

---

## Epic 1: Backend Infrastructure & Multi-Tenant API

**Epic Goal:** Establish the foundational backend infrastructure including project setup, database schema, and REST API with multi-tenancy support. Deliver core CRUD operations for vehicles, dealerships, and leads with proper data isolation via `dealershipId` filtering. Enable Cloudinary integration for image upload handling, providing a testable API layer that both public website and admin CMS will consume.

### Story 1.1: Project Setup & Basic Server

**As a** developer,
**I want** to initialize the monorepo structure and create a basic Express server with health check endpoint,
**so that** I have a working foundation to build the API upon and can verify the server runs successfully.

#### Acceptance Criteria

1. Monorepo structure created with `/backend` and `/frontend` folders
2. Root `package.json` includes scripts for running backend (`npm run server`) and development mode
3. Backend dependencies installed: Express, pg, dotenv, nodemon, Cloudinary SDK
4. Express server runs on port 5000 (configurable via environment variable)
5. Health check endpoint `GET /api/health` returns `{ status: 'ok', timestamp: <ISO date> }`
6. Server can be started with `npm run server` and restarts automatically on file changes (nodemon)
7. `.env` file template created with placeholders for DATABASE_URL, CLOUDINARY credentials
8. `.gitignore` includes `node_modules`, `.env`, and other appropriate exclusions
9. Git repository initialized with initial commit

### Story 1.2: Database Schema & Seed Data

**As a** developer,
**I want** to create the PostgreSQL database schema and seed initial dealership data,
**so that** I have the multi-tenant data model ready for API development and testing.

#### Acceptance Criteria

1. PostgreSQL database connection established using `pg` client with `DATABASE_URL` from environment variables
2. `Dealership` table created with columns: `id` (serial primary key), `name` (varchar), `logo_url` (varchar nullable), `address` (text), `phone` (varchar), `email` (varchar), `hours` (text), `about` (text), `theme_color` (varchar nullable, default '#3B82F6'), `created_at` (timestamp)
3. `Vehicle` table created with columns: `id` (serial primary key), `dealership_id` (integer FK to Dealership), `make` (varchar), `model` (varchar), `year` (integer), `price` (decimal), `mileage` (integer), `condition` (varchar: new/used), `status` (varchar: active/sold/pending/draft), `title` (varchar), `description` (text), `images` (jsonb array of URLs), `created_at` (timestamp)
4. `Lead` table created with columns: `id` (serial primary key), `dealership_id` (integer FK to Dealership), `vehicle_id` (integer FK to Vehicle, nullable), `name` (varchar), `email` (varchar), `phone` (varchar), `message` (text), `created_at` (timestamp)
5. Foreign key constraints enforce referential integrity (`dealership_id` in Vehicle and Lead must reference valid Dealership)
6. Indexes created on `dealership_id` columns in Vehicle and Lead tables for query performance
7. Database seeded with 2 sample dealerships: "Acme Auto Sales" and "Premier Motors" with complete profile information
8. Database connection can be tested with simple query (e.g., `SELECT * FROM Dealership`)

### Story 1.3: Dealership API Endpoints (CRUD)

**As a** frontend developer,
**I want** to retrieve and update dealership information via API endpoints,
**so that** I can display dealership details on the public website and enable dealership staff to update their profile in the admin CMS.

#### Acceptance Criteria

1. `GET /api/dealers` endpoint returns array of all dealerships with all fields (id, name, logo_url, address, phone, email, hours, about, theme_color)
2. `GET /api/dealers/:id` endpoint returns single dealership by ID with all fields
3. If dealership ID does not exist, `GET /api/dealers/:id` returns 404 status with error message `{ error: 'Dealership not found' }`
4. API responses include proper HTTP status codes (200 for success, 404 for not found, 500 for server errors)
5. API endpoints tested with Postman or curl, returning seeded dealership data correctly
6. Error handling implemented: database errors return 500 status with generic error message (no sensitive info leaked)
7. `PUT /api/dealers/:id` endpoint updates dealership fields (name, logo_url, address, phone, email, hours, about, theme_color) and returns updated dealership
8. Input validation ensures required fields present for PUT operation (name, address, phone, email required - return 400 Bad Request if missing)
9. theme_color field validates hex color format (#RRGGBB) if provided, returns 400 Bad Request if invalid format
10. PUT endpoint tested: update Dealership A name, about text, and theme_color, verify change persists in database and GET request returns updated data

### Story 1.4: Vehicle CRUD API

**As a** frontend developer,
**I want** to create, read, update, and delete vehicles via API with proper multi-tenant filtering,
**so that** the public website can display vehicles and the admin CMS can manage inventory.

#### Acceptance Criteria

1. `POST /api/vehicles` endpoint creates new vehicle with all required fields (dealership_id, make, model, year, price, mileage, condition, status, title, description, images) and returns created vehicle with generated ID
2. `GET /api/vehicles?dealershipId=<id>` endpoint returns array of vehicles filtered by `dealershipId` query parameter
3. `GET /api/vehicles?dealershipId=<id>&status=active` endpoint supports optional `status` filter (active/sold/pending/draft)
4. `GET /api/vehicles/:id` endpoint returns single vehicle by ID with all fields
5. `PUT /api/vehicles/:id` endpoint updates existing vehicle (requires request body with updated fields) and returns updated vehicle
6. `DELETE /api/vehicles/:id` endpoint deletes vehicle by ID and returns 204 No Content status
7. If vehicle ID does not exist for update/delete operations, API returns 404 status with error message
8. Input validation ensures required fields are present for POST/PUT operations (return 400 Bad Request if missing)
9. Multi-tenancy enforced: `dealershipId` parameter is required for GET list endpoint (returns 400 if missing)
10. API endpoints tested with Postman/curl: create vehicle for Dealership A, verify it appears in GET list for Dealership A but NOT in list for Dealership B

### Story 1.5: Lead API Endpoints

**As a** car buyer or dealership staff member,
**I want** to submit enquiries via the public website and retrieve them in the admin CMS,
**so that** dealerships can receive and respond to customer leads.

#### Acceptance Criteria

1. `POST /api/leads` endpoint creates new lead with required fields (dealership_id, name, email, phone, message) and optional `vehicle_id` field
2. `POST /api/leads` returns created lead with generated ID and `created_at` timestamp
3. Input validation ensures required fields are present (return 400 Bad Request if missing: dealership_id, name, email, message)
4. Email validation ensures `email` field contains valid email format (basic regex check)
5. `GET /api/leads?dealershipId=<id>` endpoint returns array of leads filtered by `dealershipId` query parameter, sorted by `created_at` descending (newest first)
6. Multi-tenancy enforced: `dealershipId` parameter is required for GET endpoint (returns 400 if missing)
7. API endpoints tested: submit lead for Dealership A with vehicle reference, verify it appears in GET list for Dealership A but NOT in list for Dealership B
8. If `vehicle_id` is provided, endpoint validates that vehicle exists and belongs to the specified dealership (return 400 if mismatch)

### Story 1.6: Cloudinary Image Upload Integration

**As a** dealership staff member,
**I want** to upload vehicle photos that are stored and optimized via Cloudinary,
**so that** vehicle listings display high-quality images without consuming local server storage.

#### Acceptance Criteria

1. Cloudinary account created (free tier) and API credentials (cloud_name, api_key, api_secret) stored in `.env` file
2. Cloudinary Node.js SDK configured in backend with credentials from environment variables
3. `POST /api/upload` endpoint accepts image file upload (multipart/form-data) and uploads to Cloudinary
4. `POST /api/upload` returns JSON response with Cloudinary image URL: `{ url: 'https://res.cloudinary.com/...' }`
5. Uploaded images automatically optimized by Cloudinary (format conversion to WebP where supported, quality optimization)
6. File size validation: reject uploads larger than 5MB (return 400 Bad Request with error message)
7. File type validation: only accept JPG, PNG, WebP formats (return 400 for other formats)
8. Error handling: if Cloudinary upload fails, return 500 status with error message
9. Upload endpoint tested with Postman or curl: upload sample vehicle image, verify Cloudinary URL returned and image accessible at URL

### Story 1.7: Multi-Tenancy Validation & API Testing

**As a** developer and QA tester,
**I want** to validate that multi-tenant data isolation works correctly and all API endpoints are properly documented,
**so that** the API is ready for frontend integration with confidence in data security.

#### Acceptance Criteria

1. Input validation middleware added to all API endpoints: sanitize user input, validate required fields, return 400 Bad Request for invalid requests
2. All SQL queries use parameterized queries (no string concatenation) to prevent SQL injection
3. Multi-tenancy validation test: Create vehicles for Dealership A and B, verify `GET /api/vehicles?dealershipId=A` returns ONLY Dealership A vehicles
4. Multi-tenancy validation test: Create leads for Dealership A and B, verify `GET /api/leads?dealershipId=A` returns ONLY Dealership A leads
5. Cross-contamination test: Attempt to update Dealership A vehicle via `PUT /api/vehicles/:id` (no additional checks needed for MVP, but noted for Phase 2: verify requester has permission for that dealership)
6. Error handling tested: Invalid dealership ID, missing required fields, malformed requests all return appropriate error responses (400/404/500) with helpful messages
7. API documentation created (simple README or Postman collection) listing all endpoints, required parameters, expected responses, and example requests
8. Database seeded with 5-10 sample vehicles for each dealership to support frontend development and testing

---

## Epic 2: Public Dealership Website & Lead Capture

**Epic Goal:** Deliver complete visitor-facing experience that enables car buyers to discover vehicles, evaluate options, and submit enquiries. Build responsive React application with home page, vehicle inventory listing with search/filtering capabilities, detailed vehicle pages with image galleries, about/contact page, and functional lead capture forms that integrate with the backend API. This epic delivers the core business value: generating qualified leads for dealerships.

### Story 2.1: Public Site Layout & Home Page

**As a** car buyer,
**I want** to land on a welcoming home page with clear navigation to browse vehicles,
**so that** I can quickly understand what the dealership offers and start shopping.

#### Acceptance Criteria

1. React application initialized in `/frontend` folder with Create React App or Vite
2. React Router v6 installed and configured with basic routing structure
3. App layout component created with header (dealership name/logo, navigation links) and footer (basic copyright text)
4. Home page route `/` displays hero section with dealership name, tagline, and prominent "Browse Inventory" call-to-action button
5. Navigation header includes links: "Home", "Inventory", "About"
6. "Browse Inventory" button links to `/inventory` route
7. Dealership name and logo fetched from API (`GET /api/dealers/:id`) and displayed dynamically in header
8. Basic CSS styling applied (Tailwind CSS or plain CSS) for clean, professional appearance
9. App runs locally with `npm start` and displays home page without errors
10. Dealership ID is configurable (e.g., via URL parameter, environment variable, or hardcoded for prototype) so public site knows which dealership to display

### Story 2.2: Vehicle Listing Page with Grid View

**As a** car buyer,
**I want** to view all available vehicles in an organized grid layout,
**so that** I can quickly browse the dealership's inventory and compare options.

#### Acceptance Criteria

1. Vehicle listing page route `/inventory` created and accessible from navigation
2. Page fetches vehicles from API (`GET /api/vehicles?dealershipId=<id>&status=active`) on component mount
3. Vehicles displayed in responsive grid layout (3 columns on desktop, 2 on tablet, 1 on mobile)
4. Each vehicle card displays: thumbnail image (first image from `images` array or placeholder if no images), make/model/year, price (formatted as currency), condition badge ("New" or "Used")
5. Vehicle cards are clickable and link to vehicle detail page `/inventory/:vehicleId`
6. Empty state displayed when no vehicles exist: "No vehicles available yet. Check back soon!" with dealership contact info
7. Loading state displayed while fetching vehicles from API ("Loading inventory...")
8. Error state displayed if API request fails: "Unable to load inventory. Please try again later."
9. Images use lazy loading (React library or native `loading="lazy"` attribute)
10. Vehicle status "Pending" displays badge on card ("Pending Sale")

### Story 2.3: Search & Filter Controls

**As a** car buyer,
**I want** to search and filter vehicles by keywords and condition,
**so that** I can quickly find vehicles that match my needs.

#### Acceptance Criteria

1. Search input field added above vehicle grid with placeholder text "Search by make, model, or year..."
2. Search executes on text input change (debounced to avoid excessive API calls) or on "Search" button click
3. Search filters vehicles client-side (case-insensitive) across make, model, year, and title fields
4. Condition filter dropdown added with options: "All", "New", "Used"
5. Condition filter filters vehicles client-side based on `condition` field
6. Sort dropdown added with options: "Price: Low to High", "Price: High to Low", "Year: Newest", "Year: Oldest"
7. Sort applies to filtered vehicle list client-side
8. Vehicle count displayed: "Showing X vehicles" (updates based on search/filter results)
9. If search/filter returns no results, display message: "No vehicles match your search. Try different filters."
10. Clear/reset button resets all filters and search to show full inventory

### Story 2.4: Vehicle Detail Page with Image Gallery

**As a** car buyer,
**I want** to view detailed information about a specific vehicle including photos and specifications,
**so that** I can evaluate whether it meets my needs before contacting the dealership.

#### Acceptance Criteria

1. Vehicle detail page route `/inventory/:vehicleId` created
2. Page fetches single vehicle from API (`GET /api/vehicles/:id`) using route parameter on component mount
3. Page displays vehicle title (e.g., "2015 Toyota Camry") as H1 heading
4. Image gallery displays all vehicle images from `images` array with primary image shown large and thumbnails below (clickable to change primary image)
5. If no images exist, display placeholder image ("No photos available")
6. Image gallery supports navigation: click thumbnail to view full size, optional: previous/next arrows
7. Specifications section displays: Make, Model, Year, Price (formatted), Mileage (formatted with commas), Condition ("New" or "Used"), Status (with badge if "Pending")
8. Vehicle description displayed in full below specifications
9. "Back to Inventory" link/button returns to `/inventory` page
10. Loading state displayed while fetching vehicle ("Loading vehicle details...")
11. Error state if vehicle not found (404 from API): "Vehicle not found" with link back to inventory
12. Images optimized via Cloudinary transformations (responsive sizes, WebP format with fallback)

### Story 2.5: Enquiry Form & Lead Submission

**As a** car buyer,
**I want** to submit an enquiry about a vehicle directly from the detail page,
**so that** I can express interest and the dealership can follow up with me.

#### Acceptance Criteria

1. Enquiry form component added to vehicle detail page below vehicle information
2. Form includes fields: Name (required), Email (required), Phone (required), Message (textarea, required)
3. Form has clear heading: "Interested in this vehicle? Contact us!"
4. Client-side validation: all fields required, email must be valid format, display error messages below fields if validation fails
5. "Submit Enquiry" button triggers form validation and API submission
6. On valid submission, POST request sent to `/api/leads` with `dealershipId`, `vehicle_id`, name, email, phone, message
7. Success state: form cleared, success message displayed: "Thank you! We'll contact you soon." with confirmation that enquiry was submitted
8. Error state: if API submission fails, display error message: "Unable to submit enquiry. Please try again or call us at [dealership phone]."
9. Submit button disabled during API request (prevent duplicate submissions)
10. Form auto-populates message with vehicle information: "I'm interested in the [year make model]."
11. After successful submission, option to "Submit Another Enquiry" (resets form) or "Back to Inventory"

### Story 2.6: About/Contact Page

**As a** car buyer,
**I want** to view dealership information including location, hours, and contact details,
**so that** I can visit the dealership or contact them outside of vehicle-specific enquiries.

#### Acceptance Criteria

1. About/Contact page route `/about` created and accessible from navigation
2. Page fetches dealership information from API (`GET /api/dealers/:id`)
3. Page displays dealership name as H1 heading
4. "About Us" section displays dealership `about` text (2-3 paragraphs)
5. Contact information section displays: Address (formatted), Phone (clickable `tel:` link on mobile), Email (clickable `mailto:` link)
6. Hours section displays dealership `hours` (formatted as table or list)
7. Logo displayed prominently if `logo_url` exists
8. Optional: embed Google Maps iframe or static map image showing dealership address
9. "Browse Our Inventory" call-to-action button links to `/inventory`
10. Page is mobile-responsive with readable text and properly formatted contact info

### Story 2.7: Mobile Responsiveness & Browser Testing

**As a** car buyer using a mobile device,
**I want** the website to work seamlessly on my phone or tablet,
**so that** I can browse vehicles and submit enquiries from any device.

#### Acceptance Criteria

1. All public website pages (home, inventory listing, vehicle detail, about) tested on mobile viewports (iPhone, Android phone sizes: 375px, 414px width)
2. Navigation menu collapses to mobile hamburger menu or simplified layout on small screens
3. Vehicle grid layout adjusts to single column on mobile (<768px width)
4. Vehicle detail page image gallery is touch-friendly (swipe to change images or tap thumbnails)
5. Enquiry form fields are full-width on mobile with adequate touch target sizes (min 44px height)
6. Text is readable without zooming (minimum 16px font size for body text)
7. Images responsive and properly sized (no horizontal scrolling, images scale to container width)
8. Call-to-action buttons have adequate size and spacing for touch interaction
9. Public website tested on mobile Safari (iOS) and Chrome (Android): all features functional
10. Public website tested on desktop browsers (Chrome, Firefox, Safari): all features functional
11. Page load performance acceptable on mobile (< 3 seconds on simulated 3G connection)
12. No JavaScript errors in browser console for any page or user interaction

---

## Epic 3: Admin CMS, Dealership Management & Production Deployment

**Epic Goal:** Enable dealership staff to independently manage their business without developer intervention by providing a complete admin CMS for inventory management, dealership configuration (including brand theme color customization), and lead tracking. Implement multi-tenancy UI with dealership selector, integrate Cloudinary upload widget for photo management, and deploy the complete platform (backend + public site + admin CMS) to live hosting with seeded demo data demonstrating two fully functional dealerships. This epic delivers operational readiness and validates the platform's viability as a production system.

### Story 3.1: Admin Login & Authentication

**As a** dealership staff member,
**I want** to log in to the admin panel with secure credentials,
**so that** I can access inventory management tools while preventing unauthorized public access.

#### Acceptance Criteria

1. Admin login page route `/admin/login` created with simple login form
2. Login form includes fields: Username (or Email), Password, "Log In" button
3. Hard-coded admin credentials stored in backend environment variables (`ADMIN_USERNAME`, `ADMIN_PASSWORD`) for prototype
4. `POST /api/auth/login` endpoint validates credentials and returns success/failure response
5. On successful login, user redirected to admin dashboard (`/admin`)
6. Authentication state managed in React (e.g., Context API or localStorage) to persist login across page refreshes
7. Protected route logic: if user accesses `/admin/*` routes without authentication, redirect to `/admin/login`
8. "Log Out" button in admin header clears authentication state and redirects to login page
9. Error message displayed for invalid credentials: "Invalid username or password"
10. For MVP: no JWT tokens required (session-based auth acceptable); password hashing deferred to Phase 2 (acceptable to use plain text comparison for prototype with environment variable storage)

### Story 3.2: Admin Dashboard & Dealership Selector

**As a** dealership staff member or platform admin,
**I want** to select which dealership I'm managing from a dropdown and see an overview dashboard,
**so that** I can switch between dealerships and navigate to management tools efficiently.

#### Acceptance Criteria

1. Admin dashboard route `/admin` created (accessible only after login)
2. Admin layout component includes header with dealership selector dropdown and navigation menu
3. Dealership selector fetches all dealerships from API (`GET /api/dealers`) and populates dropdown with dealership names
4. Selected dealership stored in React state (Context or state management) and persists across admin pages
5. Header displays currently selected dealership: "Managing: [Dealership Name]"
6. Dashboard displays summary statistics for selected dealership: Total Vehicles (count), Active Listings (status=active count), Recent Leads (count from last 7 days)
7. Navigation menu includes links: "Vehicle Manager", "Dealership Settings", "Lead Inbox", "Log Out"
8. Clicking navigation links routes to respective admin pages while maintaining selected dealership context
9. If no dealership is selected on initial load, auto-select first dealership from API response
10. Mobile-responsive layout: dealership selector and navigation accessible on tablet/large phone screens

### Story 3.3: Vehicle Manager List View

**As a** dealership staff member,
**I want** to view all vehicles for my dealership in a table with actions to edit or delete,
**so that** I can manage inventory efficiently and see status at a glance.

#### Acceptance Criteria

1. Vehicle Manager page route `/admin/vehicles` created
2. Page fetches vehicles from API (`GET /api/vehicles?dealershipId=<selectedDealershipId>`) on component mount
3. Vehicles displayed in table or list view with columns: Thumbnail Image, Title (year make model), Price, Mileage, Condition, Status, Actions (Edit/Delete buttons)
4. Status displayed with visual indicators (badges/colors): Active (green), Sold (gray), Pending (yellow), Draft (blue)
5. "Add Vehicle" button prominently displayed above table, links to vehicle create form
6. Edit button for each vehicle navigates to vehicle edit form (`/admin/vehicles/edit/:id`)
7. Delete button triggers confirmation modal: "Are you sure you want to delete this vehicle?" with Cancel/Confirm options
8. On delete confirmation, DELETE request sent to API (`DELETE /api/vehicles/:id`), vehicle removed from table, success message displayed
9. Empty state when no vehicles exist: "No vehicles yet. Click 'Add Vehicle' to get started." with prominent CTA button
10. Table supports filtering by status (dropdown: All, Active, Sold, Pending, Draft)
11. Vehicle count displayed: "Showing X vehicles"
12. Loading and error states handled (similar to public site)

### Story 3.4: Vehicle Create/Edit Form with Image Upload

**As a** dealership staff member,
**I want** to create new vehicle listings and edit existing ones with photo uploads,
**so that** I can maintain accurate, up-to-date inventory with professional images.

#### Acceptance Criteria

1. Vehicle create form route `/admin/vehicles/new` created
2. Vehicle edit form route `/admin/vehicles/edit/:id` created (pre-populates with existing vehicle data from API)
3. Form includes fields: Make (text), Model (text), Year (number), Price (number), Mileage (number), Condition (dropdown: New/Used), Status (dropdown: Active/Sold/Pending/Draft), Title (text, e.g., "2015 Toyota Camry"), Description (textarea)
4. Image upload section integrates Cloudinary upload widget (frontend widget or backend endpoint fallback for mobile)
5. User can upload multiple images (primary image + additional gallery images), maximum 10 images per vehicle
6. Uploaded images displayed as thumbnails with "Remove" button to delete image from list
7. Image URLs stored in `images` array field (JSON format) when form submitted
8. Client-side validation: Make, Model, Year, Price, Condition, Status, Title are required fields
9. On valid create form submission, POST request sent to `/api/vehicles` with all fields including `dealershipId` (from selected dealership context)
10. On valid edit form submission, PUT request sent to `/api/vehicles/:id` with updated fields
11. Success message displayed after save: "Vehicle saved successfully!" and redirect to Vehicle Manager list
12. Error handling: display API error messages if save fails
13. "Cancel" button returns to Vehicle Manager without saving
14. Form is mobile-responsive (tablet-friendly for iPad use)
15. Image upload fallback: if Cloudinary widget fails on mobile, use simple file input with backend `/api/upload` endpoint

### Story 3.5: Lead Inbox & Viewing

**As a** dealership staff member,
**I want** to view all customer enquiries submitted through the public website,
**so that** I can follow up with potential buyers and convert leads into sales.

#### Acceptance Criteria

1. Lead Inbox page route `/admin/leads` created
2. Page fetches leads from API (`GET /api/leads?dealershipId=<selectedDealershipId>`) on component mount
3. Leads displayed in table or list view with columns: Name, Email, Phone, Vehicle (linked to vehicle if `vehicle_id` exists), Message (truncated with "View more" option), Date Submitted (formatted as "MM/DD/YYYY HH:MM AM/PM")
4. Leads sorted by `created_at` descending (newest first)
5. If `vehicle_id` exists, vehicle column displays vehicle title (e.g., "2015 Toyota Camry") as clickable link to public vehicle detail page (opens in new tab)
6. Click on lead row expands to show full message or opens modal with complete lead details
7. Each lead row includes contact action buttons: "Call" (tel: link), "Email" (mailto: link with pre-filled subject "Re: Your enquiry about [vehicle]")
8. Empty state when no leads exist: "No leads yet. Leads submitted through the website will appear here."
9. Lead count displayed: "Showing X leads"
10. Optional: filter by date range (e.g., "Last 7 days", "Last 30 days", "All time")
11. Loading and error states handled
12. Mobile-responsive layout (tablet-friendly)

### Story 3.6: Dealership Settings Management

**As a** dealership staff member,
**I want** to edit my dealership's profile information including logo, contact details, hours, and brand theme color,
**so that** the public website displays accurate, up-to-date information about my business and matches my dealership's brand identity.

#### Acceptance Criteria

1. Dealership Settings page route `/admin/settings` created
2. Page fetches dealership data from API (`GET /api/dealers/:id`) using selected dealership ID from context
3. Settings form includes fields: Dealership Name (text), Logo (Cloudinary upload widget or file input), Address (textarea), Phone (text), Email (text), Hours (textarea, e.g., "Mon-Fri 9am-6pm, Sat 10am-4pm"), About Us (textarea), Theme Color (color picker with hex input)
4. Logo upload uses Cloudinary widget (or backend `/api/upload` endpoint), displays current logo if exists
5. Current logo displayed with "Remove" button to clear logo (sets `logo_url` to null)
6. Theme Color section includes: color picker input (HTML5 type="color"), text input for manual hex entry (#RRGGBB format), "Reset to Default" button (resets to #3B82F6), live preview showing header with selected color
7. Color picker and hex input are synchronized (changing one updates the other)
8. Hex color input validates format on blur/submit (must match #RRGGBB pattern), displays error message if invalid
9. Live preview section displays sample header with dealership name using the selected theme color as background, updating in real-time as color changes
10. Client-side validation: Name, Address, Phone, Email are required fields; theme_color must be valid hex format if provided
11. Email validation ensures valid email format
12. On valid form submission, PUT request sent to API (`PUT /api/dealers/:id`) with updated fields including theme_color (uses endpoint from Epic 1, Story 1.3)
13. Success message displayed: "Dealership settings updated successfully!"
14. Theme color changes immediately reflected on both public website AND admin panel (all primary buttons, links, headers, badges update to new color)
15. System applies theme color via CSS custom properties: --theme-primary (main color), --theme-primary-dark (hover states, 10% darker), --theme-primary-light (backgrounds, 90% lighter)
16. Changes immediately reflected on public website (test by navigating to public site and verifying updated info and theme color appears)
17. Error handling: display API error messages if save fails, specific validation error for invalid hex color format
18. Form is mobile-responsive (tablet-friendly)
19. All UI elements respect theme color: header background, primary buttons (Browse Inventory, Submit Enquiry, etc.), vehicle prices, navigation links, phone/email links, filter badges, selected states, input focus borders
20. If no theme_color is set or null, system defaults to #3B82F6 (blue)

### Story 3.7: Production Deployment & Multi-Tenancy Validation

**As a** platform stakeholder,
**I want** the complete platform deployed to live hosting with demo data for 2 dealerships,
**so that** I can demonstrate the working prototype to potential users and validate multi-tenant functionality in production.

#### Acceptance Criteria

1. Railway or Render account created with free tier hosting plan
2. PostgreSQL database provisioned via platform add-on (Railway Postgres or Render PostgreSQL)
3. Environment variables configured on platform: `DATABASE_URL`, `CLOUDINARY_CLOUD_NAME`, `CLOUDINARY_API_KEY`, `CLOUDINARY_API_SECRET`, `ADMIN_USERNAME`, `ADMIN_PASSWORD`, `NODE_ENV=production`, `PORT=<platform-assigned-port>`
4. Backend configured to serve built React frontend in production mode (`app.use(express.static('frontend/build'))`)
5. Frontend build process configured (`npm run build` in `/frontend` generates production build)
6. Git repository connected to Railway/Render with auto-deploy enabled (push to `main` branch triggers deployment)
7. Backend and frontend successfully deployed, platform assigned public URL accessible (e.g., `https://multi-dealership-platform.up.railway.app`)
8. Database seeded with 2 demo dealerships: "Acme Auto Sales" (5-10 sample vehicles) and "Premier Motors" (5-10 sample vehicles)
9. Demo vehicles include diverse makes/models/prices, at least 3 images per vehicle (uploaded to Cloudinary), varied statuses (active, pending, sold)
10. Demo leads seeded for both dealerships (2-3 leads each) to populate lead inbox
11. Multi-tenancy validation: Access public site for Dealership A (`/?dealershipId=1` or via routing), verify ONLY Dealership A vehicles shown; repeat for Dealership B
12. End-to-end production test: Submit enquiry via public site → verify appears in admin lead inbox for correct dealership
13. Admin CMS production test: Log in, switch between dealerships, add/edit/delete vehicle → verify changes appear on public site
14. Platform uptime verified: site accessible and stable, no 500 errors or deployment failures
15. Production URL documented and shared for demo purposes

---

## Next Steps

### Checklist Results Report

#### Executive Summary

**Overall PRD Completeness:** 99% ✅

**MVP Scope Appropriateness:** Just Right ✅
The 2-day timeline and ruthless prioritization have produced a truly minimal yet viable scope. The 21 user stories across 3 epics are well-sized and sequenced.

**Readiness for Architecture Phase:** ✅ READY
The PRD provides comprehensive technical guidance, clear constraints, and well-defined requirements. The Architect can proceed with confidence.

**Most Critical Strengths:**
- Excellent story sequencing with zero forward dependencies
- Comprehensive acceptance criteria with testability built in
- Strong technical constraints and decision framework
- Clear multi-tenancy architecture guidance
- Realistic MVP scope for 48-hour timeline

**Minor Gaps (Non-blocking):**
- Explicit conflict resolution process for stakeholder disagreements (acceptable for prototype phase)

---

#### Category Analysis

| Category                         | Status | Critical Issues |
| -------------------------------- | ------ | --------------- |
| 1. Problem Definition & Context  | PASS ✅  | None |
| 2. MVP Scope Definition          | PASS ✅  | None |
| 3. User Experience Requirements  | PASS ✅  | None |
| 4. Functional Requirements       | PASS ✅  | None |
| 5. Non-Functional Requirements   | PASS ✅  | None |
| 6. Epic & Story Structure        | PASS ✅  | None |
| 7. Technical Guidance            | PASS ✅  | None |
| 8. Cross-Functional Requirements | PASS ✅  | None |
| 9. Clarity & Communication       | PASS ✅  | Minor: stakeholder conflict resolution (non-blocking) |

**Legend:** PASS (90%+ complete), PARTIAL (60-89%), FAIL (<60%)

---

#### Top Issues by Priority

**🚫 BLOCKERS (Must fix before architect can proceed)**
None identified ✅

**🔴 HIGH (Should fix for quality)**
None identified ✅

**🟡 MEDIUM (Would improve clarity)**
1. **Stakeholder Conflict Resolution Process**: While key stakeholders are identified and communication plan exists, there's no explicit process for handling disagreements during implementation.
   - **Impact:** Low - prototype phase makes this acceptable
   - **Recommendation:** Document escalation path if needed during pilot deployment

**🟢 LOW (Nice to have)**
1. **Visual Diagrams**: PRD is text-heavy; could benefit from architecture diagram or user flow diagrams.
   - **Impact:** Minimal - text is comprehensive and clear
   - **Recommendation:** Architect can create diagrams during architecture phase

---

#### MVP Scope Assessment

**✅ Scope is Appropriately Minimal**

**Features correctly included:**
- Multi-tenancy from Day 1 (validates core differentiator)
- Basic auth (sufficient for prototype, upgradeable later)
- Manual testing (pragmatic for 48-hour timeline)
- Cloudinary integration (offloads complexity, enables rapid development)

**Features correctly excluded (deferred to Phase 2):**
- Advanced filtering (price sliders, multi-select)
- Role-based access control
- Automated testing
- Email notifications
- Advanced analytics

**⚠️ Potential Complexity Concerns:**

1. **Story 3.4 (Vehicle Create/Edit Form with Image Upload):** 4-5 hours (upper limit)
   - **Mitigation:** Well-scoped acceptance criteria, Cloudinary widget, mobile fallback planned
   - **Assessment:** Acceptable - delivers complete vertical slice

2. **Cloudinary Upload Widget Integration:** External dependency risk
   - **Mitigation:** Fallback to simple file input documented
   - **Assessment:** Risk acknowledged and mitigated

3. **Multi-Tenancy Data Isolation:** Critical security requirement
   - **Mitigation:** Story 1.7 includes comprehensive validation testing
   - **Assessment:** Well-planned, testable

**📅 Timeline Realism: REALISTIC** ✅

**Epic 1 (Day 1):** 7 stories, ~18-20 hours → Tight but achievable
**Epic 2 (Day 2 morning):** 7 stories, ~16-18 hours → Reasonable
**Epic 3 (Day 2 afternoon):** 7 stories, ~18-20 hours → Deployment in final hours

**Total:** ~52-58 hours compressed into 48-hour sprint
**Assessment:** Aggressive but realistic with experienced developer and no blockers

---

#### Technical Readiness

**✅ Clarity of Technical Constraints**

Excellent guidance provided:
- 5 non-negotiable constraints clearly documented
- Technology stack fully specified
- Architecture decisions with rationale
- Trade-offs articulated

**✅ Identified Technical Risks**

Risks documented with mitigation:
1. **Cloudinary widget on mobile** → Fallback to file input
2. **Free tier limits** → Monitoring plan, upgrade path
3. **Multi-tenancy data isolation** → Systematic testing
4. **Timeline risk** → Ruthless prioritization, deferral plan

**✅ Areas Needing Architect Investigation**

Open questions documented:
1. Database migration strategy (raw SQL vs migration tool)
2. API middleware pattern for multi-tenancy enforcement
3. Frontend environment variables for API base URL

**Assessment:** Appropriate architecture-level decisions, not PRD deficiencies.

---

#### Recommendations

**🎯 No Critical Actions Required**

The PRD is production-ready for handoff to Architect.

**💡 Optional Improvements (Low Priority)**

1. Add stakeholder conflict resolution note if needed during pilot
2. Consider adding architecture diagram placeholder for Architect
3. Validate Cloudinary free tier immediately before Day 1 development

**✅ Next Steps**

1. **Handoff to Architect:** Provide PRD and Project Brief, schedule kickoff
2. **Pre-Development Prep (Day 0):** Create accounts (Cloudinary, Railway/Render), set up Git repo
3. **During Development:** PM available for clarifications, daily check-ins, prepared to cut scope if needed

---

#### Final Decision

**✅ READY FOR ARCHITECT**

The PRD and epic definitions are comprehensive, properly structured, well-sequenced, and ready for architectural design. No refinement required before architecture phase.

**Checklist Completion Score: 99/100** ✅

### UX Expert Prompt

```
I need you to create the UX/UI architecture for a multi-dealership car website + CMS platform based on the attached PRD (docs/prd.md).

Key focus areas:
- Design system and component library (public website + admin CMS)
- Responsive layouts for mobile, tablet, desktop
- User flows with wireframes for critical paths (vehicle browsing → enquiry submission; admin → manage inventory)
- Accessibility implementation strategy (WCAG AA compliance)
- Cloudinary image gallery component design
- Multi-tenancy UX patterns (dealership selector, branding customization)

Timeline: 2-day development sprint starting immediately after architecture phase
Tech stack: React + Tailwind CSS (or plain CSS)
Deliverable: UX architecture document with wireframes, component specs, and implementation guidance for developer

Please review the PRD at docs/prd.md and let me know when you're ready to begin.
```

### Architect Prompt

```
I need you to create the technical architecture for a multi-dealership car website + CMS platform based on the attached PRD (docs/prd.md) and Project Brief (docs/brief.md).

Critical requirements:
- Multi-tenant architecture with dealershipId-based data isolation
- Monorepo structure (backend + frontend)
- Monolith deployment (Express backend serving React frontend)
- PostgreSQL schema design (Dealership, Vehicle, Lead entities with relationships)
- RESTful API design (/api/vehicles, /api/dealers, /api/leads, /api/upload)
- Cloudinary integration strategy
- Railway/Render deployment configuration
- 48-hour development timeline constraint

Key decisions needed:
1. Database migration strategy (raw SQL vs migration tool)
2. Multi-tenancy enforcement pattern (middleware vs manual filtering)
3. Environment variable configuration approach
4. Frontend-backend integration during development (proxy setup)

Deliverable: Architecture document with:
- System architecture diagram
- Database schema with relationships and indexes
- API endpoint specifications
- Deployment architecture
- Development environment setup guide
- Technical risk mitigation strategies

Please review the PRD (docs/prd.md) and Brief (docs/brief.md), then provide your architecture design ready for developer handoff.
```

---

**End of Product Requirements Document**
