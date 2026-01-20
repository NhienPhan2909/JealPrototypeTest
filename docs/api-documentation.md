# API Documentation

## Overview

This document provides complete API specifications for the Multi-Dealership Car Website + CMS Platform backend.

**Base URL:** `http://localhost:5000/api`
**Production URL:** (To be configured on Railway deployment)

**Date:** 2026-01-14
**Version:** 2.0
**Last Updated:** Dealership Management (Create/Delete)

---

## Table of Contents

1. [Authentication Endpoints](#authentication-endpoints)
2. [Dealership Endpoints](#dealership-endpoints)
3. [Vehicle Endpoints](#vehicle-endpoints)
4. [Lead Endpoints](#lead-endpoints)
5. [Upload Endpoints](#upload-endpoints)
6. [Error Codes](#error-codes)
7. [Security Notes](#security-notes)

---

## Authentication Endpoints

### 1. POST /api/auth/login

Login to admin panel with username and password.

**Authentication:** None (public endpoint)

**Request Body:**
```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Success Response (200 OK):**
```json
{
  "message": "Login successful"
}
```

**Error Responses:**
- `400 Bad Request` - Missing username or password
- `401 Unauthorized` - Invalid credentials

**curl Example:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}'
```

---

### 2. POST /api/auth/logout

Logout from admin panel (destroys session).

**Authentication:** Required (session-based)

**Request Body:** None

**Success Response (200 OK):**
```json
{
  "message": "Logout successful"
}
```

**curl Example:**
```bash
curl -X POST http://localhost:5000/api/auth/logout \
  -H "Content-Type: application/json" \
  --cookie "session_cookie_here"
```

---

### 3. GET /api/auth/me

Check current authentication status.

**Authentication:** Required (session-based)

**Success Response (200 OK):**
```json
{
  "isAuthenticated": true
}
```

**Error Responses:**
- `401 Unauthorized` - Not authenticated

**curl Example:**
```bash
curl -X GET http://localhost:5000/api/auth/me \
  -H "Content-Type: application/json" \
  --cookie "session_cookie_here"
```

---

## Dealership Endpoints

### 4. GET /api/dealers

Retrieve list of all dealerships.

**Authentication:** None (public endpoint)

**Query Parameters:** None

**Success Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "Premium Auto Sales",
    "logo_url": "https://...",
    "address": "123 Main St, City, State 12345",
    "phone": "(555) 123-4567",
    "email": "info@premiumauto.com",
    "hours": "Mon-Sat 9am-7pm, Sun 10am-5pm",
    "about": "Serving the community since 1995..."
  }
]
```

**Error Responses:**
- `500 Internal Server Error` - Database error

**curl Example:**
```bash
curl -X GET http://localhost:5000/api/dealers \
  -H "Content-Type: application/json"
```

---

### 5. GET /api/dealers/:id

Retrieve single dealership by ID.

**Authentication:** None (public endpoint)

**URL Parameters:**
- `id` (number, required) - Dealership ID

**Success Response (200 OK):**
```json
{
  "id": 1,
  "name": "Premium Auto Sales",
  "logo_url": "https://...",
  "address": "123 Main St, City, State 12345",
  "phone": "(555) 123-4567",
  "email": "info@premiumauto.com",
  "hours": "Mon-Sat 9am-7pm, Sun 10am-5pm",
  "about": "Serving the community since 1995..."
}
```

**Error Responses:**
- `400 Bad Request` - Invalid dealership ID (non-numeric or negative)
- `404 Not Found` - Dealership not found
- `500 Internal Server Error` - Database error

**curl Example:**
```bash
curl -X GET http://localhost:5000/api/dealers/1 \
  -H "Content-Type: application/json"
```

---

### 6. POST /api/dealers

Create a new dealership (Admin only).

**Authentication:** Required (Admin only)

**Authorization:** `user_type: 'admin'` required

**Request Body:**
```json
{
  "name": "New Auto Sales",
  "address": "123 Main St, City, State 12345",
  "phone": "(555) 123-4567",
  "email": "info@newautosales.com",
  "logo_url": "https://example.com/logo.png",
  "hours": "Mon-Fri: 9am-6pm\nSat: 10am-4pm\nSun: Closed",
  "about": "Family owned dealership serving the community since 1985."
}
```

**Required Fields:**
- `name` (string, max 255 chars)
- `address` (string, max 255 chars)
- `phone` (string, max 20 chars)
- `email` (string, max 255 chars, valid email format)

**Optional Fields:**
- `logo_url` (string)
- `hours` (string, max 500 chars)
- `about` (string, max 2000 chars)

**Success Response (201 Created):**
```json
{
  "id": 3,
  "name": "New Auto Sales",
  "address": "123 Main St, City, State 12345",
  "phone": "(555) 123-4567",
  "email": "info@newautosales.com",
  "logo_url": "https://example.com/logo.png",
  "hours": "Mon-Fri: 9am-6pm\nSat: 10am-4pm\nSun: Closed",
  "about": "Family owned dealership serving the community since 1985.",
  "created_at": "2026-01-14T02:30:00.000Z",
  "theme_color": null,
  "hero_background_image": null
}
```

**Error Responses:**
- `400 Bad Request` - Missing required fields, invalid email format, or field too long
- `403 Forbidden` - User is not admin
- `500 Internal Server Error` - Database error

**Security Notes:**
- Admin authentication required
- All text inputs are sanitized to prevent XSS attacks
- Email format is validated using regex
- Field length limits enforced

**curl Example:**
```bash
curl -X POST http://localhost:5000/api/dealers \
  -H "Content-Type: application/json" \
  --cookie "session_cookie_here" \
  -d '{
    "name": "New Auto Sales",
    "address": "123 Main St, City, State 12345",
    "phone": "(555) 123-4567",
    "email": "info@newautosales.com"
  }'
```

---

### 7. PUT /api/dealers/:id

Update dealership profile information.

**Authentication:** Required (Authenticated users)

**URL Parameters:**
- `id` (number, required) - Dealership ID

**Request Body:**
```json
{
  "name": "Premium Auto Sales Updated",
  "address": "456 New St, City, State 12345",
  "phone": "(555) 999-8888",
  "email": "contact@premiumauto.com",
  "logo_url": "https://res.cloudinary.com/...",
  "hours": "Mon-Sun 8am-8pm",
  "about": "Updated about text"
}
```

**Required Fields:**
- `name` (string, max 255 chars)
- `address` (string, max 255 chars)
- `phone` (string, max 20 chars)
- `email` (string, max 255 chars, valid email format)

**Optional Fields:**
- `logo_url` (string)
- `hours` (string, max 500 chars)
- `about` (string, max 2000 chars)

**Success Response (200 OK):**
```json
{
  "id": 1,
  "name": "Premium Auto Sales Updated",
  "address": "456 New St, City, State 12345",
  "phone": "(555) 999-8888",
  "email": "contact@premiumauto.com",
  "logo_url": "https://res.cloudinary.com/...",
  "hours": "Mon-Sun 8am-8pm",
  "about": "Updated about text"
}
```

**Error Responses:**
- `400 Bad Request` - Missing required fields, invalid email format, or field too long
- `404 Not Found` - Dealership not found
- `500 Internal Server Error` - Database error

**Security Notes:**
- All text inputs are sanitized to prevent XSS attacks
- Email format is validated using regex

**curl Example:**
```bash
curl -X PUT http://localhost:5000/api/dealers/1 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Premium Auto Sales Updated",
    "address": "456 New St, City, State 12345",
    "phone": "(555) 999-8888",
    "email": "contact@premiumauto.com"
  }'
```

---

### 8. DELETE /api/dealers/:id

Delete a dealership and all related data (Admin only).

⚠️ **CRITICAL WARNING:** This endpoint performs a HARD DELETE with CASCADE effects:
- Deletes the dealership record
- Deletes ALL vehicles associated with the dealership
- Deletes ALL leads associated with the dealership
- Deletes ALL sales requests associated with the dealership
- Deletes ALL blog posts associated with the dealership
- Deletes ALL user accounts (owners and staff) associated with the dealership

**This action is IRREVERSIBLE. All data will be permanently lost.**

**Authentication:** Required (Admin only)

**Authorization:** `user_type: 'admin'` required

**URL Parameters:**
- `id` (number, required) - Dealership ID to delete

**Success Response (200 OK):**
```json
{
  "message": "Dealership deleted successfully",
  "dealership": {
    "id": 3,
    "name": "Test Auto Sales",
    "address": "123 Main St",
    "phone": "(555) 123-4567",
    "email": "info@testautosales.com",
    "created_at": "2026-01-14T02:30:00.000Z"
  }
}
```

**Error Responses:**
- `400 Bad Request` - Invalid dealership ID (non-numeric or negative)
- `403 Forbidden` - User is not admin
- `404 Not Found` - Dealership not found
- `500 Internal Server Error` - Database error

**Security Notes:**
- Admin authentication required
- ID validation (must be positive integer)
- Database CASCADE constraints automatically delete related records
- No recovery mechanism - deletion is permanent

**Frontend Safety Measures:**
- User must type exact dealership name to confirm
- Strong warning messages about consequences
- Multi-step confirmation process

**curl Example:**
```bash
curl -X DELETE http://localhost:5000/api/dealers/3 \
  -H "Content-Type: application/json" \
  --cookie "session_cookie_here"
```

---

## Vehicle Endpoints

**SECURITY (SEC-001):** All vehicle endpoints require `dealershipId` query parameter to enforce multi-tenant data isolation and prevent cross-dealership access.

### 9. GET /api/vehicles?dealershipId=<id>

List vehicles for specific dealership with optional status filter.

**Authentication:** None (public endpoint)

**Query Parameters:**
- `dealershipId` (number, **REQUIRED**) - Dealership ID to filter vehicles
- `status` (string, optional) - Filter by status: `active`, `sold`, `pending`, `draft`

**Success Response (200 OK):**
```json
[
  {
    "id": 1,
    "dealership_id": 1,
    "make": "Toyota",
    "model": "Camry",
    "year": 2021,
    "price": "24995.00",
    "mileage": 32000,
    "condition": "used",
    "status": "active",
    "title": "2021 Toyota Camry SE - Low Miles",
    "description": "Well-maintained Toyota Camry SE...",
    "images": ["https://...", "https://..."],
    "created_at": "2025-11-19T17:23:11.859Z"
  }
]
```

**Error Responses:**
- `400 Bad Request` - Missing or invalid `dealershipId` parameter
- `500 Internal Server Error` - Database error

**Security Notes (SEC-001):**
- Query ONLY returns vehicles belonging to specified dealership
- Prevents cross-dealership data leaks

**curl Example:**
```bash
curl -X GET "http://localhost:5000/api/vehicles?dealershipId=1" \
  -H "Content-Type: application/json"

# With status filter
curl -X GET "http://localhost:5000/api/vehicles?dealershipId=1&status=active" \
  -H "Content-Type: application/json"
```

---

### 10. GET /api/vehicles/:id?dealershipId=<id>

Get single vehicle by ID with dealership ownership verification.

**Authentication:** None (public endpoint)

**URL Parameters:**
- `id` (number, required) - Vehicle ID

**Query Parameters:**
- `dealershipId` (number, **REQUIRED**) - Dealership ID to verify ownership

**Success Response (200 OK):**
```json
{
  "id": 1,
  "dealership_id": 1,
  "make": "Toyota",
  "model": "Camry",
  "year": 2021,
  "price": "24995.00",
  "mileage": 32000,
  "condition": "used",
  "status": "active",
  "title": "2021 Toyota Camry SE - Low Miles",
  "description": "Well-maintained Toyota Camry SE...",
  "images": ["https://...", "https://..."],
  "created_at": "2025-11-19T17:23:11.859Z"
}
```

**Error Responses:**
- `400 Bad Request` - Missing or invalid `dealershipId` or vehicle ID
- `404 Not Found` - Vehicle not found OR belongs to different dealership
- `500 Internal Server Error` - Database error

**Security Notes (SEC-001):**
- Returns 404 if vehicle exists but belongs to different dealership
- Prevents cross-dealership access

**curl Example:**
```bash
curl -X GET "http://localhost:5000/api/vehicles/1?dealershipId=1" \
  -H "Content-Type: application/json"
```

---

### 11. POST /api/vehicles

Create new vehicle.

**Authentication:** None (will be added in future story)

**Request Body:**
```json
{
  "dealership_id": 1,
  "make": "Honda",
  "model": "Civic",
  "year": 2023,
  "price": 25000,
  "mileage": 10000,
  "condition": "used",
  "status": "active",
  "title": "2023 Honda Civic EX",
  "description": "Low mileage Honda Civic in excellent condition...",
  "images": [
    "https://res.cloudinary.com/.../image1.jpg",
    "https://res.cloudinary.com/.../image2.jpg"
  ]
}
```

**Required Fields:**
- `dealership_id` (number, positive integer)
- `make` (string, max 100 chars)
- `model` (string, max 100 chars)
- `year` (number, 1900-2100)
- `price` (number, positive)
- `mileage` (number, positive)
- `condition` (enum: `new` or `used`)
- `status` (enum: `active`, `sold`, `pending`, `draft`)
- `title` (string, max 200 chars)

**Optional Fields:**
- `description` (string, max 2000 chars)
- `images` (array of strings, Cloudinary URLs)

**Success Response (201 Created):**
```json
{
  "id": 15,
  "dealership_id": 1,
  "make": "Honda",
  "model": "Civic",
  "year": 2023,
  "price": "25000.00",
  "mileage": 10000,
  "condition": "used",
  "status": "active",
  "title": "2023 Honda Civic EX",
  "description": "Low mileage Honda Civic in excellent condition...",
  "images": ["https://..."],
  "created_at": "2025-11-21T02:00:00.000Z"
}
```

**Error Responses:**
- `400 Bad Request` - Missing required fields, invalid values, field too long, or invalid enum
- `500 Internal Server Error` - Database error

**Security Notes:**
- All text inputs sanitized to prevent XSS attacks
- Numeric fields validated (year, price, mileage)
- Enum fields validated (condition, status)

**curl Example:**
```bash
curl -X POST http://localhost:5000/api/vehicles \
  -H "Content-Type: application/json" \
  -d '{
    "dealership_id": 1,
    "make": "Honda",
    "model": "Civic",
    "year": 2023,
    "price": 25000,
    "mileage": 10000,
    "condition": "used",
    "status": "active",
    "title": "2023 Honda Civic EX",
    "description": "Low mileage Honda Civic...",
    "images": []
  }'
```

---

### 12. PUT /api/vehicles/:id?dealershipId=<id>

Update existing vehicle with dealership ownership verification.

**Authentication:** None (will be added in future story)

**URL Parameters:**
- `id` (number, required) - Vehicle ID

**Query Parameters:**
- `dealershipId` (number, **REQUIRED**) - Dealership ID to verify ownership

**Request Body:**
All fields are optional. Only include fields you want to update.

```json
{
  "price": 23500,
  "status": "pending",
  "description": "Updated description - price reduced!"
}
```

**Success Response (200 OK):**
```json
{
  "id": 1,
  "dealership_id": 1,
  "make": "Toyota",
  "model": "Camry",
  "year": 2021,
  "price": "23500.00",
  "mileage": 32000,
  "condition": "used",
  "status": "pending",
  "title": "2021 Toyota Camry SE - Low Miles",
  "description": "Updated description - price reduced!",
  "images": ["https://..."],
  "created_at": "2025-11-19T17:23:11.859Z"
}
```

**Error Responses:**
- `400 Bad Request` - Missing or invalid `dealershipId`, invalid values, or field too long
- `404 Not Found` - Vehicle not found OR belongs to different dealership
- `500 Internal Server Error` - Database error

**Security Notes (SEC-001):**
- Ownership verification prevents cross-dealership modifications
- All text inputs sanitized to prevent XSS

**curl Example:**
```bash
curl -X PUT "http://localhost:5000/api/vehicles/1?dealershipId=1" \
  -H "Content-Type: application/json" \
  -d '{
    "price": 23500,
    "status": "pending"
  }'
```

---

### 13. DELETE /api/vehicles/:id?dealershipId=<id>

Delete vehicle with dealership ownership verification.

**Authentication:** None (will be added in future story)

**URL Parameters:**
- `id` (number, required) - Vehicle ID

**Query Parameters:**
- `dealershipId` (number, **REQUIRED**) - Dealership ID to verify ownership

**Success Response (204 No Content):**
No response body

**Error Responses:**
- `400 Bad Request` - Missing or invalid `dealershipId` or vehicle ID
- `404 Not Found` - Vehicle not found OR belongs to different dealership
- `500 Internal Server Error` - Database error

**Security Notes (SEC-001):**
- Ownership verification prevents cross-dealership deletions

**curl Example:**
```bash
curl -X DELETE "http://localhost:5000/api/vehicles/1?dealershipId=1" \
  -H "Content-Type: application/json"
```

---

## Lead Endpoints

**SECURITY (SEC-001):** GET endpoint requires `dealershipId` query parameter to enforce multi-tenant data isolation.

### 14. GET /api/leads?dealershipId=<id>

List customer enquiries for specific dealership (admin inbox), sorted newest first.

**Authentication:** None (will be added in future story)

**Query Parameters:**
- `dealershipId` (number, **REQUIRED**) - Dealership ID to filter leads

**Success Response (200 OK):**
```json
[
  {
    "id": 1,
    "dealership_id": 1,
    "vehicle_id": 1,
    "name": "Sarah Johnson",
    "email": "sarah.johnson@email.com",
    "phone": "(555) 234-5678",
    "message": "Hi, I am interested in the 2021 Toyota Camry...",
    "created_at": "2025-11-19T17:23:11.869Z"
  }
]
```

**Error Responses:**
- `400 Bad Request` - Missing or invalid `dealershipId` parameter
- `500 Internal Server Error` - Database error

**Security Notes (SEC-001):**
- Query ONLY returns leads belonging to specified dealership
- Prevents cross-dealership lead access

**curl Example:**
```bash
curl -X GET "http://localhost:5000/api/leads?dealershipId=1" \
  -H "Content-Type: application/json"
```

---

### 15. POST /api/leads

Submit customer enquiry (public endpoint, no auth required).

**Authentication:** None (public endpoint for customer submissions)

**Request Body:**
```json
{
  "dealership_id": 1,
  "vehicle_id": 5,
  "name": "John Smith",
  "email": "john@example.com",
  "phone": "(555) 123-4567",
  "message": "I am interested in this vehicle. Please contact me."
}
```

**Required Fields:**
- `dealership_id` (number, positive integer)
- `name` (string, max 255 chars)
- `email` (string, max 255 chars, valid email format)
- `phone` (string, max 20 chars)
- `message` (string, max 5000 chars)

**Optional Fields:**
- `vehicle_id` (number, nullable) - If provided, must belong to specified dealership

**Success Response (201 Created):**
```json
{
  "id": 15,
  "dealership_id": 1,
  "vehicle_id": 5,
  "name": "John Smith",
  "email": "john@example.com",
  "phone": "(555) 123-4567",
  "message": "I am interested in this vehicle. Please contact me.",
  "created_at": "2025-11-21T02:00:00.000Z"
}
```

**Error Responses:**
- `400 Bad Request` - Missing required fields, invalid email format, field too long, or vehicle ownership mismatch
- `500 Internal Server Error` - Database error

**Security Notes:**
- All text inputs (name, phone, message) sanitized to prevent XSS attacks
- Email format validated using regex
- Vehicle ownership validated to enforce cross-tenant data integrity (SEC-001)

**curl Example:**
```bash
curl -X POST http://localhost:5000/api/leads \
  -H "Content-Type: application/json" \
  -d '{
    "dealership_id": 1,
    "vehicle_id": 5,
    "name": "John Smith",
    "email": "john@example.com",
    "phone": "(555) 123-4567",
    "message": "I am interested in this vehicle. Please contact me."
  }'
```

---

## Upload Endpoints

### 16. POST /api/upload

Upload image file to Cloudinary for vehicle photos or dealer logos.

**Authentication:** None (will be added in future story)

**Content-Type:** `multipart/form-data`

**Form Fields:**
- `image` (file, required) - Image file to upload

**File Constraints:**
- Maximum size: 5MB
- Allowed types: JPG, PNG, WebP only

**Success Response (200 OK):**
```json
{
  "url": "https://res.cloudinary.com/demo/image/upload/v1/dealership-vehicles/abc123.jpg"
}
```

**Error Responses:**
- `400 Bad Request` - No file uploaded, file too large, or invalid file type
- `500 Internal Server Error` - Cloudinary upload failure

**Security Notes:**
- File size limited to 5MB via multer configuration
- File type restricted to JPG/PNG/WebP via file filter
- Cloudinary API Secret kept server-side (never exposed to frontend)

**curl Example:**
```bash
curl -X POST http://localhost:5000/api/upload \
  -F "image=@/path/to/vehicle-photo.jpg"
```

---

## Error Codes

All endpoints follow consistent error response format:

```json
{
  "error": "Descriptive error message"
}
```

### HTTP Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| `200 OK` | Success | Successful GET or PUT operation |
| `201 Created` | Created | Successful POST operation (resource created) |
| `204 No Content` | Success | Successful DELETE operation (no response body) |
| `400 Bad Request` | Client Error | Validation failed, missing required fields, invalid format, invalid parameters |
| `401 Unauthorized` | Authentication Error | Authentication required or invalid credentials |
| `404 Not Found` | Not Found | Resource not found or cross-tenant access denied (SEC-001) |
| `500 Internal Server Error` | Server Error | Database errors, unexpected failures |

### Common Error Messages

**Validation Errors (400):**
- `"dealershipId query parameter is required"`
- `"dealershipId must be a valid positive number"`
- `"Missing required fields: <field list>"`
- `"Invalid email format"`
- `"<field> must be <limit> characters or less"`
- `"Condition must be one of: new, used"`
- `"Status must be one of: active, sold, pending, draft"`
- `"Year must be a valid number between 1900 and 2100"`
- `"Vehicle does not belong to specified dealership"`

**Not Found Errors (404):**
- `"Vehicle not found"` (includes cross-dealership access attempts per SEC-001)
- `"Dealership not found"`

**Server Errors (500):**
- `"Failed to fetch vehicles"`
- `"Failed to create vehicle"`
- `"Failed to update dealership"`

---

## Security Notes

### SEC-001: Multi-Tenancy Data Isolation

**CRITICAL SECURITY REQUIREMENT:** All tenant-scoped endpoints enforce multi-tenancy filtering.

**Vehicle Endpoints:**
- ALL vehicle endpoints require `dealershipId` query parameter
- Database queries filter by BOTH `id` AND `dealership_id`
- Cross-dealership access attempts return 404 (not 403, to avoid info leakage)

**Lead Endpoints:**
- GET /api/leads requires `dealershipId` query parameter
- POST /api/leads validates vehicle ownership if `vehicle_id` provided

**Why This Matters:**
Without dual filtering (id AND dealership_id), an attacker could guess vehicle IDs and access/modify/delete vehicles from other dealerships.

### XSS Prevention

**Protected Endpoints:**
- PUT /api/dealers/:id - Text inputs (name, address, phone, hours, about)
- POST /api/vehicles - Text inputs (make, model, title, description)
- PUT /api/vehicles/:id - Text inputs (make, model, title, description)
- POST /api/leads - Text inputs (name, phone, message)

**Implementation:**
All user-provided text is sanitized by escaping HTML special characters: `< > & " ' /`

**Example:**
Input: `<script>alert('xss')</script>`
Stored: `&lt;script&gt;alert(&#x27;xss&#x27;)&lt;/script&gt;`

### SQL Injection Prevention

**Status:** SECURE - All database queries use parameterized syntax ($1, $2, etc.)

The `pg` client automatically escapes parameters, preventing SQL injection attacks.

### Input Validation

**All endpoints validate:**
- Required fields (return 400 if missing)
- Field lengths (return 400 if too long)
- Email format (regex validation)
- Numeric IDs (return 400 if non-numeric or negative)
- Enum values (return 400 if invalid)

---

## Testing Examples

### Multi-Tenancy Isolation Test

```bash
# Create vehicle for Dealership 1
curl -X POST http://localhost:5000/api/vehicles \
  -H "Content-Type: application/json" \
  -d '{"dealership_id": 1, "make": "Toyota", "model": "Camry", "year": 2021, "price": 25000, "mileage": 30000, "condition": "used", "status": "active", "title": "Test Vehicle"}'

# Try to access it using Dealership 2's ID (should return 404)
curl -X GET "http://localhost:5000/api/vehicles/1?dealershipId=2"
# Returns: {"error":"Vehicle not found"}
```

### XSS Attack Prevention Test

```bash
# Attempt XSS attack in lead submission
curl -X POST http://localhost:5000/api/leads \
  -H "Content-Type: application/json" \
  -d '{
    "dealership_id": 1,
    "name": "<script>alert(\"xss\")</script>",
    "email": "test@example.com",
    "phone": "555-0000",
    "message": "Test message"
  }'

# Retrieve lead to verify sanitization
curl -X GET "http://localhost:5000/api/leads?dealershipId=1"
# name field will show: "&lt;script&gt;alert(&quot;xss&quot;)&lt;/script&gt;"
```

### Validation Error Test

```bash
# Missing required fields
curl -X POST http://localhost:5000/api/vehicles \
  -H "Content-Type: application/json" \
  -d '{"dealership_id": 1, "make": "Toyota"}'
# Returns: {"error":"Missing required fields: dealership_id, make, model, year, price, mileage, condition, status, title"}

# Invalid email format
curl -X POST http://localhost:5000/api/leads \
  -H "Content-Type: application/json" \
  -d '{
    "dealership_id": 1,
    "name": "Test",
    "email": "not-an-email",
    "phone": "555-0000",
    "message": "Test"
  }'
# Returns: {"error":"Invalid email format"}
```

---

## Changelog

| Date | Version | Description | Author |
|------|---------|-------------|--------|
| 2025-11-21 | 1.0 | Initial API documentation with comprehensive security notes and curl examples | James (Dev Agent) |

---

**For Questions:** Contact development team or refer to security-guidelines.md for detailed security implementation patterns.
