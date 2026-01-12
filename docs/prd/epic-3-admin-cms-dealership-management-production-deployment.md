# Epic 3: Admin CMS, Dealership Management & Production Deployment

**Epic Goal:** Enable dealership staff to independently manage their business without developer intervention by providing a complete admin CMS for inventory management, dealership configuration, and lead tracking. Implement multi-tenancy UI with dealership selector, integrate Cloudinary upload widget for photo management, and deploy the complete platform (backend + public site + admin CMS) to live hosting with seeded demo data demonstrating two fully functional dealerships. This epic delivers operational readiness and validates the platform's viability as a production system.

## Story 3.1: Admin Login & Authentication

**As a** dealership staff member,
**I want** to log in to the admin panel with secure credentials,
**so that** I can access inventory management tools while preventing unauthorized public access.

### Acceptance Criteria

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

## Story 3.2: Admin Dashboard & Dealership Selector

**As a** dealership staff member or platform admin,
**I want** to select which dealership I'm managing from a dropdown and see an overview dashboard,
**so that** I can switch between dealerships and navigate to management tools efficiently.

### Acceptance Criteria

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

## Story 3.2.1: Admin Panel "View Website" Navigation (Enhancement)

**As a** dealership staff member using the admin panel,
**I want** a quick way to view my dealership's public website from the admin interface,
**so that** I can preview my changes and see how customers will experience my dealership.

**Note:** This story was implemented as a usability enhancement to the admin panel. See `docs/stories/3.2.1.story.md` for complete implementation details.

### Acceptance Criteria

1. "View Website" button appears in AdminHeader navigation menu
2. Button positioned between "Lead Inbox" and "Log Out" links
3. Clicking button updates public site DealershipContext to match admin's selected dealership
4. Clicking button navigates to public home page (`/`)
5. Button displays external link icon for visual clarity
6. Button shows hover effect (blue-400 → blue-300)
7. Button disabled when no dealership is selected
8. Button shows tooltip on hover: "View public website for this dealership"
9. Button is mobile-responsive and wraps appropriately
10. No backend changes required (uses existing context infrastructure)

## Story 3.3: Vehicle Manager List View

**As a** dealership staff member,
**I want** to view all vehicles for my dealership in a table with actions to edit or delete,
**so that** I can manage inventory efficiently and see status at a glance.

### Acceptance Criteria

1. Vehicle Manager page route `/admin/vehicles` created
2. Page fetches vehicles from API (`GET /api/vehicles?dealershipId=<selectedDealershipId>`) on component mount
3. Vehicles displayed in table or list view with columns: Thumbnail Image, Title (year make model), Price, Mileage, Condition, Status, Actions (Edit/Delete buttons)
4. Status displayed with visual indicators (badges/colors): Active (green), Sold (gray), Pending (yellow), Draft (blue)
5. "Add Vehicle" button prominently displayed above table, links to vehicle create form
6. Edit button for each vehicle navigates to vehicle edit form (`/admin/vehicles/edit/:id`)
7. Delete button triggers confirmation modal: "Are you sure you want to delete this vehicle?" with Cancel/Confirm options
8. On delete confirmation, DELETE request sent to API (`DELETE /api/vehicles/:id?dealershipId=<id>`) with dealershipId query parameter (REQUIRED for multi-tenancy security - API v1.1+), vehicle removed from table, success message displayed
9. Empty state when no vehicles exist: "No vehicles yet. Click 'Add Vehicle' to get started." with prominent CTA button
10. Table supports filtering by status (dropdown: All, Active, Sold, Pending, Draft)
11. Vehicle count displayed: "Showing X vehicles"
12. Loading and error states handled (similar to public site)

## Story 3.4: Vehicle Create/Edit Form with Image Upload

**As a** dealership staff member,
**I want** to create new vehicle listings and edit existing ones with photo uploads,
**so that** I can maintain accurate, up-to-date inventory with professional images.

### Acceptance Criteria

1. Vehicle create form route `/admin/vehicles/new` created
2. Vehicle edit form route `/admin/vehicles/edit/:id` created (pre-populates with existing vehicle data from API)
3. Form includes fields: Make (text), Model (text), Year (number), Price (number), Mileage (number), Condition (dropdown: New/Used), Status (dropdown: Active/Sold/Pending/Draft), Title (text, e.g., "2015 Toyota Camry"), Description (textarea)
4. Image upload section integrates Cloudinary upload widget (frontend widget or backend endpoint fallback for mobile)
5. User can upload multiple images (primary image + additional gallery images), maximum 10 images per vehicle
6. Uploaded images displayed as thumbnails with "Remove" button to delete image from list
7. Image URLs stored in `images` array field (JSON format) when form submitted
8. Client-side validation: Make, Model, Year, Price, Condition, Status, Title are required fields
9. On valid create form submission, POST request sent to `/api/vehicles` with all fields including `dealershipId` (from selected dealership context)
10. On valid edit form submission, PUT request sent to `/api/vehicles/:id?dealershipId=<id>` with dealershipId query parameter (REQUIRED for multi-tenancy security - API v1.1+) and updated fields
11. Success message displayed after save: "Vehicle saved successfully!" and redirect to Vehicle Manager list
12. Error handling: display API error messages if save fails
13. "Cancel" button returns to Vehicle Manager without saving
14. Form is mobile-responsive (tablet-friendly for iPad use)
15. Image upload fallback: if Cloudinary widget fails on mobile, use simple file input with backend `/api/upload` endpoint

## Story 3.5: Lead Inbox & Viewing

**As a** dealership staff member,
**I want** to view all customer enquiries submitted through the public website,
**so that** I can follow up with potential buyers and convert leads into sales.

### Acceptance Criteria

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

## Story 3.5.1: Lead Status Tracking & Delete Functionality (Enhancement)

**As a** dealership staff member,
**I want** to track the progress of each lead through status stages and delete leads when necessary,
**so that** I can manage my lead pipeline effectively and keep my inbox organized.

**Note:** This enhancement was implemented to bring Lead Inbox feature parity with Vehicle Manager. See `docs/stories/3.5.1.story.md` for complete implementation details.

### Acceptance Criteria

**Status Tracking:**
1. Status column added to Lead Inbox table with dropdown: "Received", "In Progress", "Done"
2. Default status for new leads is "Received"
3. Status updates trigger PATCH to `/api/leads/:id/status?dealershipId=<id>`
4. Color-coded status badges: Received (blue), In Progress (yellow), Done (green)
5. Status changes update immediately without page refresh

**Delete Functionality:**
6. Delete button added to Actions column (red button)
7. Clicking Delete opens confirmation modal showing lead name
8. Confirm button sends DELETE to `/api/leads/:id?dealershipId=<id>`
9. On success, lead removed from table

**Security:**
10. Status and delete endpoints validate dealershipId (SEC-001)
11. Cannot update/delete leads from other dealerships

**Database:**
12. `status` column added to `lead` table with CHECK constraint
13. Migration script provided for existing databases

## Story 3.6: Dealership Settings Management

**As a** dealership staff member,
**I want** to edit my dealership's profile information including logo, contact details, and hours,
**so that** the public website displays accurate, up-to-date information about my business.

### Acceptance Criteria

1. Dealership Settings page route `/admin/settings` created
2. Page fetches dealership data from API (`GET /api/dealers/:id`) using selected dealership ID from context
3. Settings form includes fields: Dealership Name (text), Logo (Cloudinary upload widget or file input), Address (textarea), Phone (text), Email (text), Hours (textarea, e.g., "Mon-Fri 9am-6pm, Sat 10am-4pm"), About Us (textarea)
4. Logo upload uses Cloudinary widget (or backend `/api/upload` endpoint), displays current logo if exists
5. Current logo displayed with "Remove" button to clear logo (sets `logo_url` to null)
6. Client-side validation: Name, Address, Phone, Email are required fields
7. Email validation ensures valid email format
8. On valid form submission, PUT request sent to API (`PUT /api/dealers/:id`) with updated fields (uses endpoint from Epic 1, Story 1.3)
9. Success message displayed: "Dealership settings updated successfully!"
10. Changes immediately reflected on public website (test by navigating to public site and verifying updated info appears)
11. Error handling: display API error messages if save fails
12. Form is mobile-responsive (tablet-friendly)

## Story 3.6.1: Public Website Dealership Selector (Enhancement)

**As a** platform user visiting the public website,
**I want** to choose which dealership's website to view from a dropdown selector,
**so that** I can browse multiple dealerships without changing URLs or configurations.

**Note:** This story was implemented as an enhancement to enable dynamic dealership selection on the public website. See `docs/stories/3.6.1.story.md` for complete implementation details.

### Acceptance Criteria

1. Global dealership context created to manage currently selected dealership ID across all public pages
2. Dealership selector component fetches all dealerships from API and displays dropdown
3. Selected dealership ID stored in localStorage and persists across sessions
4. Selector only appears when 2+ dealerships exist (hidden for single dealership)
5. Selector integrated into Header component (both desktop and mobile navigation)
6. All public pages (Home, Inventory, About, VehicleDetail) use context instead of hardcoded dealership ID
7. Changing dealership immediately updates all page content without page reload
8. Default dealership is ID 1 on first visit
9. Mobile-responsive layout with appropriate styling
10. No backend changes required (uses existing `GET /api/dealers` endpoint)

## Story 3.7: Production Deployment & Multi-Tenancy Validation

**As a** platform stakeholder,
**I want** the complete platform deployed to live hosting with demo data for 2 dealerships,
**so that** I can demonstrate the working prototype to potential users and validate multi-tenant functionality in production.

### Acceptance Criteria

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

## Story 3.8: Manage Finance & Warranty Content in Admin CMS

**As a** dealership staff member,
**I want** to edit my dealership's financing and warranty policy content in the admin CMS,
**so that** the Finance and Warranty pages on the public website display accurate, customized information for my dealership.

### Acceptance Criteria

1. Dealership Settings form (`/admin/settings` - Story 3.6) updated to include two new fields: "Finance Policy" and "Warranty Policy"
2. Finance Policy field is a textarea input with label "Financing Options & Policy" and placeholder text "Describe your financing options, terms, and application process..."
3. Warranty Policy field is a textarea input with label "Warranty Information & Coverage" and placeholder text "Describe your warranty coverage, terms, and conditions..."
4. Both fields support multi-line text input (minimum 5 rows visible, expandable)
5. Both fields are optional (dealership can leave blank to show default contact message on public pages)
6. Character count displayed below each textarea: "X / 2000 characters" (soft limit for guidance, not enforced)
7. Fields positioned in Settings form after "Hours" field and before "About Us" field for logical grouping
8. On form load, current `finance_policy` and `warranty_policy` values fetched from API and populated in form
9. On form submission, PUT request to `/api/dealers/:id` includes updated `finance_policy` and `warranty_policy` fields
10. Success message confirms save: "Dealership settings updated successfully!"
11. Changes immediately reflected on public Finance (`/finance`) and Warranty (`/warranty`) pages when navigating to public site
12. Form validation allows empty values (optional fields)
13. Text formatting preserved (line breaks maintained when displaying on public pages)
14. Mobile-responsive form layout (tablet-friendly)

---
