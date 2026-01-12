# 5. API Specification

REST API with 13 endpoints covering all public website and admin CMS functionality. All endpoints return JSON.

**CRITICAL MULTI-TENANCY SECURITY (SEC-001):** All vehicle endpoints enforce multi-tenant data isolation by requiring `dealershipId` query parameter. This prevents cross-dealership data leaks, modifications, and deletions. Vehicle operations (GET/PUT/DELETE) filter by BOTH `id` AND `dealership_id` at the database layer.

## Base URL

- **Development:** `http://localhost:5000/api`
- **Production:** `https://your-app.up.railway.app/api`

## Security Requirements for All Endpoints

**⚠️ MANDATORY: All API endpoints MUST implement security measures defined in [security-guidelines.md](./security-guidelines.md)**

### Critical Security Requirements

**1. Multi-Tenancy (SEC-001) - CRITICAL**
- All tenant-scoped endpoints MUST require `dealershipId` parameter
- All database queries MUST filter by `dealership_id`
- Return 400 if dealershipId missing or invalid

**2. XSS Prevention - CRITICAL**
- Sanitize ALL user text inputs (name, message, description, etc.)
- Use `sanitizeInput()` function from security-guidelines.md
- DO NOT sanitize: validated emails, numeric values, enum fields

**3. SQL Injection Prevention - CRITICAL**
- ALWAYS use parameterized queries ($1, $2, ...)
- NEVER concatenate user input into SQL strings

**4. Input Validation - REQUIRED**
- Validate required fields (return 400 if missing)
- Validate field lengths (prevent DoS/database errors)
- Validate formats (email, numeric IDs, enums)
- Validate positive numbers for IDs

### Implementation Checklist

Before implementing any endpoint, review:
- ✅ [Security Guidelines](./security-guidelines.md) - Complete patterns and examples
- ✅ [Story 1.5](../stories/1.5.story.md) - Reference implementation (Lead API)
- ✅ Security checklist in security-guidelines.md for endpoint type (GET/POST/PUT/DELETE)

## API Endpoints Overview

| Method | Endpoint | Auth Required | Purpose |
|--------|----------|---------------|---------|
| **Authentication** |
| POST | `/auth/login` | No | Admin login |
| POST | `/auth/logout` | Yes | Admin logout |
| GET | `/auth/me` | Yes | Check auth status |
| **Dealerships** |
| GET | `/dealers` | No | List all dealerships |
| GET | `/dealers/:id` | No | Get single dealership |
| PUT | `/dealers/:id` | Yes | Update dealership settings |
| **Vehicles** |
| GET | `/vehicles?dealershipId=<id>` | No | List vehicles (with filters) |
| GET | `/vehicles/:id?dealershipId=<id>` | No | Get single vehicle (SEC-001: requires dealershipId) |
| POST | `/vehicles` | Yes | Create vehicle |
| PUT | `/vehicles/:id?dealershipId=<id>` | Yes | Update vehicle (SEC-001: requires dealershipId) |
| DELETE | `/vehicles/:id?dealershipId=<id>` | Yes | Delete vehicle (SEC-001: requires dealershipId) |
| **Leads** |
| GET | `/leads` | Yes | List leads (admin) |
| POST | `/leads` | No | Submit enquiry (public) |
| **Google Reviews** |
| GET | `/google-reviews/:dealershipId` | No | Get Google reviews for dealership |
| **Uploads** |
| POST | `/upload` | Yes | Upload image to Cloudinary |

## Authentication Endpoints

### POST /api/auth/login

Admin login with hard-coded credentials. Creates session cookie.

**Request Body:**
```json
{
  "username": "admin",
  "password": "your-admin-password"
}
```

**Success Response (200):**
```json
{
  "success": true,
  "message": "Logged in successfully"
}
```

**Error Response (401):**
```json
{
  "error": "Invalid credentials"
}
```

**Express Implementation:**
```javascript
app.post('/api/auth/login', (req, res) => {
  const { username, password } = req.body;

  if (username === process.env.ADMIN_USERNAME && password === process.env.ADMIN_PASSWORD) {
    req.session.isAuthenticated = true;
    res.json({ success: true, message: 'Logged in successfully' });
  } else {
    res.status(401).json({ error: 'Invalid credentials' });
  }
});
```

### POST /api/auth/logout

Destroys admin session.

**Request Body:** None

**Success Response (200):**
```json
{
  "success": true,
  "message": "Logged out successfully"
}
```

**Express Implementation:**
```javascript
app.post('/api/auth/logout', (req, res) => {
  req.session.destroy();
  res.json({ success: true, message: 'Logged out successfully' });
});
```

### GET /api/auth/me

Check if current session is authenticated (for React app to verify login status).

**Request Body:** None

**Success Response (200):**
```json
{
  "authenticated": true
}
```

**Not Authenticated Response (200):**
```json
{
  "authenticated": false
}
```

**Express Implementation:**
```javascript
app.get('/api/auth/me', (req, res) => {
  res.json({ authenticated: !!req.session.isAuthenticated });
});
```

## Dealership Endpoints

### GET /api/dealers

List all dealerships (used for admin dealership selector dropdown).

**Query Parameters:** None

**Success Response (200):**
```json
[
  {
    "id": 1,
    "name": "Acme Auto Sales",
    "logo_url": "https://res.cloudinary.com/...",
    "hero_background_image": "https://res.cloudinary.com/...",
    "address": "123 Main St, Springfield, IL 62701",
    "phone": "(555) 123-4567",
    "email": "sales@acmeauto.com",
    "hours": "Mon-Fri 9am-6pm, Sat 10am-4pm, Sun Closed",
    "about": "Family-owned dealership...",
    "created_at": "2025-11-19T10:00:00.000Z"
  },
  {
    "id": 2,
    "name": "Premier Motors",
    ...
  }
]
```

### GET /api/dealers/:id

Get single dealership details (for public About page, admin settings form).

**Success Response (200):**
```json
{
  "id": 1,
  "name": "Acme Auto Sales",
  "logo_url": "https://res.cloudinary.com/...",
  "hero_background_image": "https://res.cloudinary.com/...",
  "theme_color": "#3B82F6",
  "font_family": "system",
  "address": "123 Main St, Springfield, IL 62701",
  "phone": "(555) 123-4567",
  "email": "sales@acmeauto.com",
  "hours": "Mon-Fri 9am-6pm, Sat 10am-4pm, Sun Closed",
  "about": "Family-owned dealership serving Springfield for 20 years.",
  "finance_policy": "We offer flexible financing options...",
  "warranty_policy": "All vehicles come with comprehensive warranty...",
  "created_at": "2025-11-19T10:00:00.000Z"
}
```

**Error Response (404):**
```json
{
  "error": "Dealership not found"
}
```

### PUT /api/dealers/:id (Auth Required)

Update dealership settings (admin CMS). **Supports partial updates** - you can send only the fields you want to update.

**Full Request Body (All Fields):**
```json
{
  "name": "Acme Auto Sales",
  "logo_url": "https://res.cloudinary.com/...",
  "hero_background_image": "https://res.cloudinary.com/.../hero-bg.jpg",
  "theme_color": "#3B82F6",
  "font_family": "times",
  "address": "123 Main St, Springfield, IL 62701",
  "phone": "(555) 123-4567",
  "email": "sales@acmeauto.com",
  "hours": "Mon-Fri 9am-6pm, Sat 10am-4pm",
  "finance_policy": "We offer flexible financing options...",
  "warranty_policy": "All vehicles come with comprehensive warranty...",
  "about": "Updated about text...",
  "navigation_config": [
    {
      "id": "home",
      "label": "Home",
      "route": "/",
      "icon": "FaHome",
      "order": 1,
      "enabled": true,
      "showIcon": true
    },
    {
      "id": "inventory",
      "label": "Inventory",
      "route": "/inventory",
      "icon": "FaCar",
      "order": 2,
      "enabled": true,
      "showIcon": false
    }
  ]
}
```

**Partial Update Example (Navigation Only):**
```json
{
  "navigation_config": [
    {
      "id": "home",
      "label": "Home",
      "route": "/",
      "icon": "FaHome",
      "order": 1,
      "enabled": true,
      "showIcon": true
    }
  ]
}
```

**Field Details:**
- `name`, `address`, `phone`, `email` (required ONLY if updating basic info): Core dealership information
- `hero_background_image` (optional, TEXT): Cloudinary URL for hero section background image on public home page. If null or omitted, home page displays default blue gradient. Uploaded via `/api/upload` endpoint (see Upload Endpoint section).
- `theme_color` (optional, VARCHAR(7), default: '#3B82F6'): Hex color code for dealership theme. Must be in #RRGGBB or #RGB format. Used for header background, buttons, links, and all branding elements across the site. See Story 3.6 for details.
- `font_family` (optional, VARCHAR(100), default: 'system'): Font family identifier for website typography. Options: system, arial, times, georgia, verdana, courier, comic-sans, trebuchet, impact, palatino. Applied to all text elements site-wide. See Story 3.7 for details.
- `navigation_config` (optional, JSONB, default: null): Navigation menu configuration array. When null, uses default navigation. Each item includes optional `showIcon` boolean field (default: true) to control icon visibility while keeping text visible. See Story 5.1 for details.

**Success Response (200):**
```json
{
  "id": 1,
  "name": "Acme Auto Sales",
  "navigation_config": [...],
  ...
}
```

**Validation Error (400):**
```json
{
  "error": "Missing required fields: name, address, phone, email"
}
```

**Note:** Required fields (name, address, phone, email) are only validated when updating basic dealership info. You can update optional fields like `navigation_config`, `theme_color`, or `font_family` independently without providing required fields.

## Vehicle Endpoints

### GET /api/vehicles

List vehicles with filtering (public inventory page, admin vehicle manager).

**Query Parameters:**
- `dealershipId` (required, integer) - Filter by dealership
- `status` (optional, string) - Filter by status ("active", "sold", "pending", "draft")
  - Public site default: `status=active` (or omit to get active + pending)
  - Admin default: all statuses

**Success Response (200):**
```json
[
  {
    "id": 1,
    "dealership_id": 1,
    "make": "Toyota",
    "model": "Camry",
    "year": 2015,
    "price": "15999.99",
    "mileage": 75000,
    "condition": "used",
    "status": "active",
    "title": "2015 Toyota Camry SE",
    "description": "Well-maintained sedan...",
    "images": ["https://res.cloudinary.com/...", "..."],
    "created_at": "2025-11-19T10:00:00.000Z"
  },
  ...
]
```

**Validation Error (400):**
```json
{
  "error": "dealershipId query parameter is required"
}
```

### GET /api/vehicles/:id

Get single vehicle details (public vehicle detail page, admin edit form).

**Query Parameters:**
- `dealershipId` (required, integer) - Filter by dealership for multi-tenancy security (SEC-001)

**Success Response (200):** (Same as vehicle object above)

**Validation Error (400):**
```json
{
  "error": "dealershipId query parameter is required"
}
```

**Error Response (404):**
```json
{
  "error": "Vehicle not found"
}
```

**Security Note:** Returns 404 if vehicle exists but belongs to different dealership. This prevents information disclosure and enforces multi-tenant data isolation (SEC-001 requirement from Story 1.2).

### POST /api/vehicles (Auth Required)

Create new vehicle (admin CMS).

**Request Body:**
```json
{
  "dealership_id": 1,
  "make": "Honda",
  "model": "Civic",
  "year": 2018,
  "price": 18500.00,
  "mileage": 45000,
  "condition": "used",
  "status": "active",
  "title": "2018 Honda Civic LX",
  "description": "Low mileage, excellent condition.",
  "images": ["https://res.cloudinary.com/...", "..."]
}
```

**Success Response (201):** (Returns created vehicle with ID)

**Validation Error (400):**
```json
{
  "error": "Missing required fields: make, model, year, price, mileage, condition, status, title"
}
```

### PUT /api/vehicles/:id (Auth Required)

Update existing vehicle (admin CMS).

**Query Parameters:**
- `dealershipId` (required, integer) - Filter by dealership for multi-tenancy security (SEC-001)

**Request Body:** (Same as POST, all fields optional)

**Success Response (200):** (Returns updated vehicle)

**Validation Error (400):**
```json
{
  "error": "dealershipId query parameter is required"
}
```

**Error Response (404):**
```json
{
  "error": "Vehicle not found"
}
```

**Security Note:** Returns 404 if vehicle exists but belongs to different dealership. This prevents cross-dealership modifications and enforces multi-tenant data isolation (SEC-001 requirement from Story 1.2).

### DELETE /api/vehicles/:id (Auth Required)

Delete vehicle (admin CMS).

**Query Parameters:**
- `dealershipId` (required, integer) - Filter by dealership for multi-tenancy security (SEC-001)

**Success Response (204):** (No content)

**Validation Error (400):**
```json
{
  "error": "dealershipId query parameter is required"
}
```

**Error Response (404):**
```json
{
  "error": "Vehicle not found"
}
```

**Security Note:** Returns 404 if vehicle exists but belongs to different dealership. This prevents cross-dealership deletions and enforces multi-tenant data isolation (SEC-001 requirement from Story 1.2).

## Lead Endpoints

### GET /api/leads (Auth Required)

List leads for dealership (admin CMS lead inbox).

**Query Parameters:**
- `dealershipId` (required, integer) - Filter by dealership

**Success Response (200):**
```json
[
  {
    "id": 1,
    "dealership_id": 1,
    "vehicle_id": 1,
    "name": "John Doe",
    "email": "john@example.com",
    "phone": "(555) 111-2222",
    "message": "I'm interested in the 2015 Toyota Camry. Is it still available?",
    "status": "received",
    "created_at": "2025-11-19T14:20:00.000Z"
  },
  ...
]
```

**Note:** `status` field added in v1.2 with default value "received". Possible values: "received", "in progress", "done".

### POST /api/leads

Submit customer enquiry (public vehicle detail page enquiry form). **Automatically sends email notification to dealership upon successful lead creation (v1.4+).**

**Request Body:**
```json
{
  "dealership_id": 1,
  "vehicle_id": 1,
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "(555) 111-2222",
  "message": "I'm interested in this vehicle. Is it still available?"
}
```

**Email Notification Behavior (v1.4+):**

After lead is successfully created in database, the system automatically sends an email notification to the dealership's email address (from `dealerships.email` field). The notification includes:
- Customer name, email, phone
- Vehicle information (year, make, model) if `vehicle_id` provided, otherwise "General Enquiry"
- Customer message

**Email Notification Features:**
- **Non-blocking**: Email is sent asynchronously. Lead creation returns 201 success even if email fails.
- **Graceful degradation**: If email configuration is missing (development mode), email is skipped with console warning but lead is still created.
- **Error handling**: Email delivery failures are logged but don't impact customer experience.

**Required Environment Variables (for email notifications):**
- `EMAIL_HOST` - SMTP server hostname (e.g., smtp.gmail.com)
- `EMAIL_PORT` - SMTP port (587 for TLS, 465 for SSL)
- `EMAIL_USER` - SMTP authentication username
- `EMAIL_PASSWORD` - SMTP authentication password/app password
- `EMAIL_FROM` - Sender email address (optional, defaults to EMAIL_USER)

**Email Configuration Examples:**
- Gmail: `EMAIL_HOST=smtp.gmail.com`, `EMAIL_PORT=587`, use app-specific password
- SendGrid: `EMAIL_HOST=smtp.sendgrid.net`, `EMAIL_PORT=587`, `EMAIL_USER=apikey`, `EMAIL_PASSWORD=YOUR_API_KEY`

**Success Response (201):** (Returns created lead with ID and default status "received")

**Validation Error (400):**
```json
{
  "error": "Missing required fields: dealership_id, name, email, phone, message"
}
```

### PATCH /api/leads/:id/status (Auth Required) [v1.2+]

Update lead status for progress tracking.

**Query Parameters:**
- `dealershipId` (required, integer) - Validates lead ownership (SEC-001)

**Request Body:**
```json
{
  "status": "in progress"
}
```

**Allowed Status Values:** "received", "in progress", "done"

**Success Response (200):**
```json
{
  "id": 1,
  "dealership_id": 1,
  "vehicle_id": 1,
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "(555) 111-2222",
  "message": "I'm interested in the 2015 Toyota Camry.",
  "status": "in progress",
  "created_at": "2025-11-19T14:20:00.000Z"
}
```

**Validation Error (400):**
```json
{
  "error": "Status must be one of: received, in progress, done"
}
```

**Not Found (404):** Lead not found or doesn't belong to specified dealership

### DELETE /api/leads/:id (Auth Required) [v1.2+]

Delete a lead permanently.

**Query Parameters:**
- `dealershipId` (required, integer) - Validates lead ownership (SEC-001)

**Success Response (200):**
```json
{
  "message": "Lead deleted successfully"
}
```

**Not Found (404):** Lead not found or doesn't belong to specified dealership

**Security Note (SEC-001):** Both PATCH and DELETE endpoints require dealershipId parameter and validate that the lead belongs to the specified dealership before performing operations. This prevents cross-dealership data manipulation.

## Google Reviews Endpoint

### GET /api/google-reviews/:dealershipId

Fetch Google reviews for a specific dealership using Google Places API. Returns top-rated reviews (4+ stars) sorted by rating with Google Maps URL.

**Path Parameters:**
- `dealershipId` (required, integer) - Dealership ID

**Process:**
1. Fetches dealership address from database
2. Searches Google Places API using dealership name + address
3. Retrieves place details and reviews from Google
4. Filters for 4+ star reviews only
5. Sorts by rating (highest first)
6. Returns top 10 reviews + Google Maps URL

**Success Response (200):**
```json
{
  "reviews": [
    {
      "author_name": "John Doe",
      "rating": 5,
      "text": "Great service and excellent vehicles! Highly recommend this dealership.",
      "time": 1640995200,
      "relative_time_description": "2 months ago",
      "profile_photo_url": "https://lh3.googleusercontent.com/a-/..."
    },
    {
      "author_name": "Jane Smith",
      "rating": 5,
      "text": "Excellent experience! The staff was professional and helpful.",
      "time": 1638316800,
      "relative_time_description": "3 months ago",
      "profile_photo_url": "https://lh3.googleusercontent.com/a-/..."
    }
  ],
  "googleMapsUrl": "https://www.google.com/maps/place/?q=place_id:ChIJ...",
  "totalRatings": 156,
  "averageRating": 4.8
}
```

**Empty Response (200):** When Google API key not configured, location not found, or no reviews available:
```json
{
  "reviews": [],
  "googleMapsUrl": "",
  "message": "Google Reviews not configured"
}
```

**Error Responses:**

**400 Bad Request:** Invalid dealership ID
```json
{
  "error": "Invalid dealership ID"
}
```

**404 Not Found:** Dealership not found
```json
{
  "error": "Dealership not found"
}
```

**Configuration Required:**

Environment variable in `.env`:
```env
GOOGLE_PLACES_API_KEY=your_google_api_key_here
```

**Google Places API Setup:**
1. Enable Places API in Google Cloud Console
2. Create API Key
3. Restrict API key to server IP (recommended)
4. Enable billing (free tier: 28,000+ requests/month)
5. Set up usage alerts

**Usage Notes:**
- Returns empty array if API key not configured (graceful degradation)
- Returns empty array if dealership location not found on Google Maps
- Returns empty array if dealership has no 4+ star reviews
- One API call per request (no caching in Phase 1)
- Cost: $17 per 1,000 requests after free tier

**Implementation Example:**
```javascript
// Frontend usage
const response = await fetch(`/api/google-reviews/${dealershipId}`);
const data = await response.json();

if (data.reviews && data.reviews.length > 0) {
  // Display reviews in carousel
  displayReviews(data.reviews, data.googleMapsUrl);
} else {
  // No reviews available - hide carousel
  hideReviewsCarousel();
}
```

**Backend Implementation Reference:**
- Route file: `backend/routes/googleReviews.js`
- Server mounting: `backend/server.js` (line 48)
- Documentation: `GOOGLE_REVIEWS_FEATURE.md`

**Future Enhancements:**
- Database caching to reduce API calls
- Admin controls to enable/disable per dealership
- Manual refresh button
- Review moderation

## Upload Endpoint

### POST /api/upload (Auth Required)

Upload image to Cloudinary for vehicle images, dealership logos, and hero backgrounds.

**Request:** `multipart/form-data` with `image` field

**Supported Use Cases:**
- Vehicle images: Multiple uploads via Cloudinary widget
- Dealership logos: Single upload via Cloudinary widget with cropping
- Hero background images: Single upload via file input (uses this endpoint due to Cloudinary widget DOM compatibility issues)

**Validation:**
- Accepted formats: JPG, PNG, WebP
- Maximum file size: 5MB
- Cloudinary folder: `dealership-hero-backgrounds` (for hero images)

**Success Response (200):**
```json
{
  "url": "https://res.cloudinary.com/your-cloud/image/upload/v123456789/dealership-hero-backgrounds/sample.jpg"
}
```

**Error Response (400):**
```json
{
  "error": "File too large (max 5MB)"
}
```

**Usage Example (Hero Background):**
```javascript
const formData = new FormData();
formData.append('image', fileInputElement.files[0]);

const response = await fetch('/api/upload', {
  method: 'POST',
  credentials: 'include',
  body: formData
});

const data = await response.json();
// Use data.url for hero_background_image field
```

## Auth Middleware

Protect admin endpoints with simple session check:

```javascript
function requireAuth(req, res, next) {
  if (!req.session.isAuthenticated) {
    return res.status(401).json({ error: 'Authentication required' });
  }
  next();
}
```

## Error Handling

Wrap all async route handlers with error catcher or use try-catch as shown. Return consistent error format:
```json
{
  "error": "Descriptive error message"
}
```

## React Frontend API Calls

Example using `fetch`:

```javascript
// Get vehicles for public site (list)
const response = await fetch(`/api/vehicles?dealershipId=${dealershipId}&status=active`);
const vehicles = await response.json();

// Get single vehicle (detail page) - REQUIRES dealershipId for multi-tenancy security
const response = await fetch(`/api/vehicles/${vehicleId}?dealershipId=${dealershipId}`);
const vehicle = await response.json();

// Update vehicle (admin CMS) - REQUIRES dealershipId for multi-tenancy security
const response = await fetch(`/api/vehicles/${vehicleId}?dealershipId=${dealershipId}`, {
  method: 'PUT',
  headers: { 'Content-Type': 'application/json' },
  credentials: 'include',
  body: JSON.stringify({ price, status, description })
});

// Delete vehicle (admin CMS) - REQUIRES dealershipId for multi-tenancy security
const response = await fetch(`/api/vehicles/${vehicleId}?dealershipId=${dealershipId}`, {
  method: 'DELETE',
  credentials: 'include'
});

// Submit enquiry
const response = await fetch('/api/leads', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ dealership_id: 1, vehicle_id: 5, name, email, phone, message })
});

// Admin login
const response = await fetch('/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  credentials: 'include', // Important for session cookies
  body: JSON.stringify({ username, password })
});
```

**CRITICAL SECURITY NOTE (SEC-001):** All vehicle single-record operations (GET /:id, PUT /:id, DELETE /:id) MUST include the `dealershipId` query parameter. This enforces multi-tenant data isolation at the API layer, preventing cross-dealership data leaks, modifications, and deletions.

---
