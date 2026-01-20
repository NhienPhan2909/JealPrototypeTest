# Dealership Management Feature

## Overview
System Administrators ("Admin" accounts) can now create new dealership websites from the CMS admin page. This feature provides a dedicated interface for managing all dealerships in the platform.

## What Was Added

### Backend Changes

#### 1. Database Layer (`backend/db/dealers.js`)
- **New Function**: `create(dealershipData)`
  - Creates a new dealership record in the database
  - Accepts: name, address, phone, email, logo_url (optional), hours (optional), about (optional)
  - Returns: Created dealership object with generated ID

#### 2. API Routes (`backend/routes/dealers.js`)
- **New Endpoint**: `POST /api/dealers`
  - **Access**: Admin only (requires authentication and admin role)
  - **Purpose**: Create new dealership
  - **Security Measures**:
    - Admin-only access via `requireAuth` and `requireAdmin` middleware
    - Input sanitization to prevent XSS attacks
    - Email format validation
    - Field length validation
  - **Request Body**:
    ```json
    {
      "name": "Dealership Name",
      "address": "123 Main St, City, State ZIP",
      "phone": "(555) 123-4567",
      "email": "contact@dealership.com",
      "logo_url": "https://example.com/logo.png",  // optional
      "hours": "Mon-Fri: 9am-6pm",                 // optional
      "about": "About the dealership"              // optional
    }
    ```
  - **Response**: Created dealership object (HTTP 201)
  - **Errors**:
    - 400: Missing required fields or validation errors
    - 403: User is not admin
    - 500: Database error

### Frontend Changes

#### 1. New Page: `DealershipManagement.jsx`
- **Location**: `frontend/src/pages/admin/DealershipManagement.jsx`
- **Route**: `/admin/dealerships`
- **Access**: Admin only (shows access denied message for non-admin users)
- **Features**:
  - **View all dealerships** in a table format
    - Shows: ID, Name, Website URL, Email, Phone, Created Date
    - **Sortable columns**: Click headers to sort by ID, Name, or Created Date
    - **Sort indicators**: Visual arrows (↑/↓) show current sort direction
  - **Create new dealership** via modal form
    - Required fields: Name, Address, Phone, Email
    - Optional fields: Logo URL, Business Hours, About, Website URL
  - **Delete dealership** via Actions column (admin only)
    - Confirmation required with name verification
  - **Success/error messaging**
  - **Form validation** (client-side)

#### 2. Navigation (`AdminHeader.jsx`)
- **New Link**: "Dealership Management"
  - Only visible to admin users (`user_type === 'admin'`)
  - Located in the admin header navigation menu

#### 3. Routing (`App.jsx`)
- **New Route**: `/admin/dealerships` → `<DealershipManagement />`
  - Protected route (requires authentication)

## User Flow

### For System Administrators:
1. Log in to the admin panel
2. Click **"Dealership Management"** in the navigation menu
3. View list of all existing dealerships
4. Click **"+ Create New Dealership"** button
5. Fill in the form:
   - **Required**: Name, Address, Phone, Email
   - **Optional**: Logo URL, Business Hours, About
6. Click **"Create Dealership"**
7. New dealership appears in the list
8. Success message confirms creation

### For Non-Admin Users:
- Navigation link is hidden
- Direct URL access shows "Access Denied" message

## Security

### Backend Security
1. **Authentication**: Requires valid session
2. **Authorization**: Checks `user_type === 'admin'`
3. **Input Sanitization**: Escapes HTML characters to prevent XSS
4. **Email Validation**: Uses regex to validate email format
5. **Field Length Limits**: Enforces maximum character counts
6. **SQL Injection Protection**: Uses parameterized queries

### Frontend Security
1. **Access Control**: Page checks user role and displays access denied for non-admins
2. **Form Validation**: Client-side validation before submission
3. **Credential Inclusion**: Uses `credentials: 'include'` for session cookies

## Testing

### Manual Testing
1. Start the backend and frontend servers
2. Log in as admin (username: `admin`, password: `admin123`)
3. Navigate to `/admin/dealerships`
4. Test creating a new dealership
5. Verify the dealership appears in the list

### Automated Testing
Run the test script:
```bash
node test_dealership_creation.js
```

This script:
1. Logs in as admin
2. Fetches current dealerships
3. Creates a test dealership
4. Verifies the dealership was created

## Database Schema

The `dealership` table already exists with these fields:
- `id` (SERIAL PRIMARY KEY)
- `name` (VARCHAR(255) NOT NULL)
- `address` (TEXT NOT NULL)
- `phone` (VARCHAR(20) NOT NULL)
- `email` (VARCHAR(255) NOT NULL)
- `logo_url` (TEXT)
- `hours` (TEXT)
- `about` (TEXT)
- `created_at` (TIMESTAMP DEFAULT NOW())

Additional fields (populated via Dealership Settings page):
- `finance_policy`, `warranty_policy`
- `hero_background_image`, `hero_type`, `hero_video_url`, `hero_carousel_images`
- `theme_color`, `secondary_theme_color`, `body_background_color`
- `font_family`, `navigation_config`
- `facebook_url`, `instagram_url`
- `finance_promo_image`, `finance_promo_text`
- `warranty_promo_image`, `warranty_promo_text`

## Next Steps

After creating a dealership, administrators can:
1. **Create a dealership owner account** via User Management
2. **Configure dealership settings** via Dealership Settings
3. **Add vehicles** via Vehicle Manager
4. **Customize the website** (theme, hero, navigation, etc.)

## Files Modified

### Backend
- `backend/db/dealers.js` - Added `create()` function
- `backend/routes/dealers.js` - Added POST endpoint, imported `requireAdmin`

### Frontend
- `frontend/src/pages/admin/DealershipManagement.jsx` - **NEW FILE**
- `frontend/src/App.jsx` - Added route and import
- `frontend/src/components/AdminHeader.jsx` - Added navigation link (admin only)

## API Reference

### POST /api/dealers
Create a new dealership (admin only).

**Headers:**
```
Content-Type: application/json
Cookie: connect.sid=... (session cookie)
```

**Request Body:**
```json
{
  "name": "Acme Auto Sales",
  "address": "123 Main Street, City, State 12345",
  "phone": "(555) 123-4567",
  "email": "contact@acmeauto.com",
  "logo_url": "https://example.com/logo.png",
  "hours": "Mon-Fri: 9am-6pm\nSat: 10am-4pm\nSun: Closed",
  "about": "Family owned dealership serving the community since 1985."
}
```

**Success Response (201 Created):**
```json
{
  "id": 3,
  "name": "Acme Auto Sales",
  "address": "123 Main Street, City, State 12345",
  "phone": "(555) 123-4567",
  "email": "contact@acmeauto.com",
  "logo_url": "https://example.com/logo.png",
  "hours": "Mon-Fri: 9am-6pm\nSat: 10am-4pm\nSun: Closed",
  "about": "Family owned dealership serving the community since 1985.",
  "created_at": "2026-01-14T02:30:00.000Z",
  "theme_color": null,
  "hero_background_image": null,
  ...
}
```

**Error Responses:**
- **400 Bad Request**: Missing required fields or validation errors
- **403 Forbidden**: User is not admin
- **500 Internal Server Error**: Database error

## Summary

✅ System Administrators can now create new dealership websites
✅ Dedicated "Dealership Management" page with list and create form
✅ Admin-only access with proper security controls
✅ Input validation and sanitization
✅ Success/error messaging
✅ Navigation link only visible to admins
