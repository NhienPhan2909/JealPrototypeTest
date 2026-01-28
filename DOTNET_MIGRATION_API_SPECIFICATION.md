# .NET Migration - Complete API Specification Document

**Created:** 2026-01-27  
**Purpose:** Complete API contract specification for migrating Node.js/Express backend to .NET/Clean Architecture  
**Status:** Ready for implementation

---

## Table of Contents

1. [Overview](#overview)
2. [Database Schema](#database-schema)
3. [Authentication & Authorization](#authentication--authorization)
4. [API Endpoints](#api-endpoints)
   - [Authentication](#authentication-endpoints)
   - [Dealerships](#dealership-endpoints)
   - [Vehicles](#vehicle-endpoints)
   - [Leads](#lead-endpoints)
   - [Users](#user-endpoints)
   - [Upload](#upload-endpoints)
   - [Sales Requests](#sales-request-endpoints)
   - [Blogs](#blog-endpoints)
   - [Google Reviews](#google-reviews-endpoints)
   - [Design Templates](#design-template-endpoints)
5. [External Services](#external-services)
6. [Middleware & Cross-Cutting Concerns](#middleware--cross-cutting-concerns)
7. [Environment Configuration](#environment-configuration)

---

## Overview

### Technology Stack

**Current (Node.js):**
- Framework: Express 4.18.2
- Database: PostgreSQL 14+ (pg driver 8.11.3)
- Authentication: Express Session (cookie-based)
- File Upload: Multer 2.0.2 + Cloudinary 2.8.0
- Email: Nodemailer 7.0.11
- Password Hashing: bcrypt 6.0.0
- Session Storage: express-session 1.17.3

**Target (.NET):**
- Framework: ASP.NET Core 8.0 Web API
- ORM: Entity Framework Core 8.0 + Npgsql
- Authentication: ASP.NET Core Identity or JWT
- File Upload: IFormFile + Cloudinary SDK
- Email: MailKit or SMTP client
- Password Hashing: BCrypt.Net or Identity's default
- Session: Distributed cache (Redis) or JWT tokens

### Architecture Principles

**Clean Architecture Layers:**
```
JealPrototype.Domain       - Entities, Value Objects, Aggregates, Domain Events
JealPrototype.Application  - Use Cases, DTOs, Interfaces, Business Logic
JealPrototype.Infrastructure - EF Core, Repositories, External Services
JealPrototype.API          - Controllers, Middleware, Filters
```

**Domain-Driven Design Patterns:**
- Aggregates: Dealership, Vehicle, User, BlogPost
- Entities: Lead, SalesRequest
- Value Objects: Email, PhoneNumber, Color (hex)
- Repositories: Per-aggregate repository pattern
- Domain Services: For cross-aggregate operations

---

## Database Schema

### Tables (PostgreSQL)

#### 1. `dealership`
```sql
CREATE TABLE dealership (
  id SERIAL PRIMARY KEY,
  name VARCHAR(255) NOT NULL,
  logo_url TEXT,
  address TEXT NOT NULL,
  phone VARCHAR(20) NOT NULL,
  email VARCHAR(255) NOT NULL,
  hours TEXT,
  about TEXT,
  website_url VARCHAR(255) UNIQUE,
  finance_policy TEXT,
  warranty_policy TEXT,
  hero_background_image TEXT,
  hero_type VARCHAR(20) DEFAULT 'image',
  hero_video_url TEXT,
  hero_carousel_images JSONB DEFAULT '[]'::jsonb,
  theme_color VARCHAR(7) DEFAULT '#3B82F6',
  secondary_theme_color VARCHAR(7) DEFAULT '#FFFFFF',
  body_background_color VARCHAR(7) DEFAULT '#FFFFFF',
  font_family VARCHAR(100) DEFAULT 'system',
  navigation_config JSONB,
  facebook_url TEXT,
  instagram_url TEXT,
  finance_promo_image TEXT,
  finance_promo_text VARCHAR(500),
  warranty_promo_image TEXT,
  warranty_promo_text VARCHAR(500),
  created_at TIMESTAMP DEFAULT NOW()
);
```

#### 2. `vehicle`
```sql
CREATE TABLE vehicle (
  id SERIAL PRIMARY KEY,
  dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE,
  make VARCHAR(100) NOT NULL,
  model VARCHAR(100) NOT NULL,
  year INTEGER NOT NULL,
  price DECIMAL(10,2) NOT NULL,
  mileage INTEGER NOT NULL,
  condition VARCHAR(10) NOT NULL CHECK (condition IN ('new', 'used')),
  status VARCHAR(10) NOT NULL DEFAULT 'draft' CHECK (status IN ('active', 'sold', 'pending', 'draft')),
  title VARCHAR(255) NOT NULL,
  description TEXT,
  images JSONB DEFAULT '[]'::jsonb,
  created_at TIMESTAMP DEFAULT NOW()
);
```

#### 3. `lead`
```sql
CREATE TABLE lead (
  id SERIAL PRIMARY KEY,
  dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE,
  vehicle_id INTEGER REFERENCES vehicle(id) ON DELETE SET NULL,
  name VARCHAR(255) NOT NULL,
  email VARCHAR(255) NOT NULL,
  phone VARCHAR(20) NOT NULL,
  message TEXT NOT NULL,
  status VARCHAR(20) DEFAULT 'received' CHECK (status IN ('received', 'in progress', 'done')),
  created_at TIMESTAMP DEFAULT NOW()
);
```

#### 4. `sales_request`
```sql
CREATE TABLE sales_request (
  id SERIAL PRIMARY KEY,
  dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE,
  name VARCHAR(255) NOT NULL,
  email VARCHAR(255) NOT NULL,
  phone VARCHAR(20) NOT NULL,
  make VARCHAR(100) NOT NULL,
  model VARCHAR(100) NOT NULL,
  year INTEGER NOT NULL,
  kilometers INTEGER NOT NULL,
  additional_message TEXT,
  status VARCHAR(20) DEFAULT 'received' CHECK (status IN ('received', 'in progress', 'done')),
  created_at TIMESTAMP DEFAULT NOW()
);
```

#### 5. `app_user`
```sql
CREATE TABLE app_user (
  id SERIAL PRIMARY KEY,
  username VARCHAR(100) NOT NULL UNIQUE,
  password_hash TEXT NOT NULL,
  email VARCHAR(255) NOT NULL,
  full_name VARCHAR(255) NOT NULL,
  user_type VARCHAR(50) NOT NULL CHECK (user_type IN ('admin', 'dealership_owner', 'dealership_staff')),
  dealership_id INTEGER REFERENCES dealership(id) ON DELETE CASCADE,
  permissions JSONB DEFAULT '[]'::jsonb,
  created_by INTEGER REFERENCES app_user(id),
  created_at TIMESTAMP DEFAULT NOW()
);
```

#### 6. `blog_post`
```sql
CREATE TABLE blog_post (
  id SERIAL PRIMARY KEY,
  dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE,
  title VARCHAR(255) NOT NULL,
  slug VARCHAR(255) NOT NULL,
  content TEXT NOT NULL,
  excerpt TEXT,
  featured_image_url TEXT,
  author_name VARCHAR(255) NOT NULL,
  status VARCHAR(20) DEFAULT 'draft' CHECK (status IN ('draft', 'published', 'archived')),
  published_at TIMESTAMP,
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW(),
  UNIQUE(dealership_id, slug)
);
```

#### 7. `design_templates`
```sql
CREATE TABLE design_templates (
  id SERIAL PRIMARY KEY,
  dealership_id INTEGER REFERENCES dealership(id) ON DELETE CASCADE,
  name VARCHAR(100) NOT NULL,
  description VARCHAR(500),
  theme_color VARCHAR(7) NOT NULL,
  secondary_theme_color VARCHAR(7) NOT NULL,
  body_background_color VARCHAR(7) NOT NULL,
  font_family VARCHAR(100) NOT NULL,
  is_preset BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMP DEFAULT NOW()
);
```

### Indexes
```sql
CREATE INDEX idx_vehicle_dealership_id ON vehicle(dealership_id);
CREATE INDEX idx_lead_dealership_id ON lead(dealership_id);
CREATE INDEX idx_sales_request_dealership_id ON sales_request(dealership_id);
CREATE INDEX idx_vehicle_status ON vehicle(status);
CREATE INDEX idx_lead_created_at ON lead(created_at DESC);
CREATE INDEX idx_sales_request_created_at ON sales_request(created_at DESC);
CREATE INDEX idx_vehicle_dealership_status ON vehicle(dealership_id, status);
CREATE INDEX idx_dealership_website_url ON dealership(website_url);
CREATE INDEX idx_blog_post_dealership_id ON blog_post(dealership_id);
CREATE INDEX idx_blog_post_slug ON blog_post(slug);
CREATE INDEX idx_app_user_username ON app_user(username);
CREATE INDEX idx_app_user_dealership_id ON app_user(dealership_id);
```

---

## Authentication & Authorization

### Session-Based Authentication

**Current Implementation:**
- Uses `express-session` with in-memory store (development)
- Session cookie: 24-hour expiration, httpOnly, secure flag for production
- Session stores full user object (excluding password_hash)

**Session Structure:**
```json
{
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@example.com",
    "full_name": "System Administrator",
    "user_type": "admin",
    "dealership_id": null,
    "permissions": []
  }
}
```

**Recommended .NET Implementation:**
- Option 1: Cookie-based authentication with ASP.NET Core Identity
- Option 2: JWT tokens with refresh tokens
- **Preferred:** JWT for API scalability + distributed cache for sessions

### User Types & Hierarchy

1. **System Administrator (`admin`)**
   - Full system access
   - Can manage all dealerships
   - Can create dealership owners
   - No dealership_id association

2. **Dealership Owner (`dealership_owner`)**
   - Full access to their dealership
   - Can create/manage staff users
   - Has dealership_id association
   - All permissions enabled by default

3. **Dealership Staff (`dealership_staff`)**
   - Limited access based on permissions array
   - Has dealership_id association
   - Permissions: `['vehicles', 'leads', 'sales_requests', 'blogs', 'settings']`

### Authorization Rules

**Middleware Chain:**
1. `requireAuth` - Validates session exists
2. `enforceDealershipScope` - Validates dealership_id matches user's dealership (except admin)
3. `requirePermission(permission)` - Validates staff has specific permission

**Permission Matrix:**

| Endpoint | Admin | Owner | Staff (with permission) |
|----------|-------|-------|------------------------|
| POST /api/dealers | ✓ | ✗ | ✗ |
| DELETE /api/dealers/:id | ✓ | ✗ | ✗ |
| PUT /api/dealers/:id | ✓ | ✓ (own) | ✓ (settings permission) |
| POST /api/vehicles | ✓ | ✓ (own) | ✓ (vehicles permission) |
| POST /api/users | ✓ | ✓ (staff only) | ✗ |
| DELETE /api/users/:id | ✓ | ✓ (staff only) | ✗ |

---

## API Endpoints

### Base URL
- Development: `http://localhost:5000/api`
- Production: `https://your-domain.com/api`

### Common Headers
```
Content-Type: application/json
Cookie: connect.sid=<session-id>  (for authenticated requests)
```

### Common Response Formats

**Success:**
```json
{
  "data": { ... },
  "success": true
}
```

**Error:**
```json
{
  "error": "Error message here",
  "success": false
}
```

---

### Authentication Endpoints

#### 1. POST /api/auth/login
**Description:** Authenticate user and create session

**Request:**
```json
{
  "username": "admin",
  "password": "password123"
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Logged in successfully",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@example.com",
    "full_name": "System Administrator",
    "user_type": "admin",
    "dealership_id": null,
    "permissions": []
  }
}
```

**Response (401):**
```json
{
  "error": "Invalid username or password"
}
```

**Response (400):**
```json
{
  "error": "Username and password are required"
}
```

---

#### 2. POST /api/auth/logout
**Description:** Destroy session and log out user

**Request:** Empty body

**Response (200):**
```json
{
  "success": true,
  "message": "Logged out successfully"
}
```

---

#### 3. GET /api/auth/me
**Description:** Get current authenticated user information

**Response (200) - Authenticated:**
```json
{
  "authenticated": true,
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@example.com",
    "full_name": "System Administrator",
    "user_type": "admin",
    "dealership_id": null,
    "permissions": []
  }
}
```

**Response (200) - Not Authenticated:**
```json
{
  "authenticated": false
}
```

---

### Dealership Endpoints

#### 1. GET /api/dealers
**Description:** List all dealerships

**Auth:** None required (public)

**Response (200):**
```json
[
  {
    "id": 1,
    "name": "Acme Auto Sales",
    "logo_url": "https://res.cloudinary.com/...",
    "address": "123 Main St, City, State",
    "phone": "(555) 123-4567",
    "email": "contact@acme.com",
    "hours": "Mon-Fri 9AM-6PM",
    "about": "Your trusted car dealer...",
    "website_url": "acme-autos",
    "theme_color": "#3B82F6",
    "secondary_theme_color": "#FFFFFF",
    "body_background_color": "#F9FAFB",
    "font_family": "system",
    "created_at": "2026-01-01T00:00:00.000Z"
  }
]
```

---

#### 2. GET /api/dealers/:id
**Description:** Get single dealership by ID

**Auth:** None required (public)

**Path Parameters:**
- `id` (integer): Dealership ID

**Response (200):**
```json
{
  "id": 1,
  "name": "Acme Auto Sales",
  "logo_url": "https://res.cloudinary.com/...",
  "address": "123 Main St, City, State",
  "phone": "(555) 123-4567",
  "email": "contact@acme.com",
  "hours": "Mon-Fri 9AM-6PM",
  "finance_policy": "We offer financing...",
  "warranty_policy": "All vehicles come with...",
  "about": "Your trusted car dealer...",
  "hero_background_image": "https://res.cloudinary.com/...",
  "hero_type": "image",
  "hero_video_url": null,
  "hero_carousel_images": ["url1", "url2"],
  "theme_color": "#3B82F6",
  "secondary_theme_color": "#FFFFFF",
  "body_background_color": "#F9FAFB",
  "font_family": "system",
  "navigation_config": [...],
  "facebook_url": "https://facebook.com/acme",
  "instagram_url": "https://instagram.com/acme",
  "finance_promo_image": "https://...",
  "finance_promo_text": "0% APR for 60 months",
  "warranty_promo_image": "https://...",
  "warranty_promo_text": "5 year warranty included",
  "website_url": "acme-autos",
  "created_at": "2026-01-01T00:00:00.000Z"
}
```

**Response (404):**
```json
{
  "error": "Dealership not found"
}
```

**Response (400):**
```json
{
  "error": "Dealership ID must be a valid positive number"
}
```

---

#### 3. POST /api/dealers
**Description:** Create new dealership

**Auth:** Required (Admin only)

**Request:**
```json
{
  "name": "New Motors",
  "address": "456 Oak Ave, City, State",
  "phone": "(555) 987-6543",
  "email": "info@newmotors.com",
  "logo_url": "https://res.cloudinary.com/...",
  "hours": "Mon-Sat 8AM-8PM",
  "about": "Family owned since 1990...",
  "website_url": "new-motors"
}
```

**Response (201):**
```json
{
  "id": 3,
  "name": "New Motors",
  "address": "456 Oak Ave, City, State",
  "phone": "(555) 987-6543",
  "email": "info@newmotors.com",
  "logo_url": "https://res.cloudinary.com/...",
  "hours": "Mon-Sat 8AM-8PM",
  "about": "Family owned since 1990...",
  "website_url": "new-motors",
  "theme_color": "#3B82F6",
  "secondary_theme_color": "#FFFFFF",
  "body_background_color": "#FFFFFF",
  "font_family": "system",
  "created_at": "2026-01-27T12:00:00.000Z"
}
```

**Response (400):**
```json
{
  "error": "Missing required fields: name, address, phone, email"
}
```

**Response (403):**
```json
{
  "error": "Admin access required"
}
```

---

#### 4. PUT /api/dealers/:id
**Description:** Update dealership profile

**Auth:** Required (Admin, Owner with settings permission, Staff with settings permission)

**Path Parameters:**
- `id` (integer): Dealership ID

**Request (partial update allowed):**
```json
{
  "name": "Acme Auto Sales - Updated",
  "theme_color": "#FF5733",
  "navigation_config": [
    { "label": "Home", "path": "/", "visible": true },
    { "label": "Inventory", "path": "/inventory", "visible": true }
  ]
}
```

**Response (200):**
```json
{
  "id": 1,
  "name": "Acme Auto Sales - Updated",
  "theme_color": "#FF5733",
  ...
}
```

**Response (404):**
```json
{
  "error": "Dealership not found"
}
```

**Response (403):**
```json
{
  "error": "Access denied to this dealership"
}
```

---

#### 5. DELETE /api/dealers/:id
**Description:** Delete dealership (CASCADE deletes vehicles, leads, users, sales requests, blogs)

**Auth:** Required (Admin only)

**Path Parameters:**
- `id` (integer): Dealership ID

**Response (200):**
```json
{
  "message": "Dealership deleted successfully",
  "dealership": {
    "id": 1,
    "name": "Acme Auto Sales"
  }
}
```

**Response (404):**
```json
{
  "error": "Dealership not found"
}
```

---

### Vehicle Endpoints

#### 1. GET /api/vehicles?dealershipId={id}
**Description:** List vehicles for a dealership with optional filters

**Auth:** None required (public)

**Query Parameters:**
- `dealershipId` (required, integer): Dealership ID
- `status` (optional, string): Filter by status (active, sold, pending, draft)
- `brand` (optional, string): Filter by make (case-insensitive)
- `minYear` (optional, integer): Minimum year (1900-2100)
- `maxYear` (optional, integer): Maximum year (1900-2100)
- `minPrice` (optional, decimal): Minimum price
- `maxPrice` (optional, decimal): Maximum price

**Response (200):**
```json
[
  {
    "id": 1,
    "dealership_id": 1,
    "make": "Toyota",
    "model": "Camry",
    "year": 2022,
    "price": 25999.99,
    "mileage": 15000,
    "condition": "used",
    "status": "active",
    "title": "2022 Toyota Camry SE",
    "description": "Excellent condition, one owner...",
    "images": [
      "https://res.cloudinary.com/image1.jpg",
      "https://res.cloudinary.com/image2.jpg"
    ],
    "created_at": "2026-01-15T10:00:00.000Z"
  }
]
```

**Response (400):**
```json
{
  "error": "dealershipId query parameter is required"
}
```

---

#### 2. GET /api/vehicles/:id?dealershipId={id}
**Description:** Get single vehicle by ID

**Auth:** None required (public)

**Path Parameters:**
- `id` (integer): Vehicle ID

**Query Parameters:**
- `dealershipId` (required, integer): Dealership ID for ownership verification

**Response (200):**
```json
{
  "id": 1,
  "dealership_id": 1,
  "make": "Toyota",
  "model": "Camry",
  "year": 2022,
  "price": 25999.99,
  "mileage": 15000,
  "condition": "used",
  "status": "active",
  "title": "2022 Toyota Camry SE",
  "description": "Excellent condition...",
  "images": ["url1", "url2"],
  "created_at": "2026-01-15T10:00:00.000Z"
}
```

**Response (404):**
```json
{
  "error": "Vehicle not found"
}
```

---

#### 3. POST /api/vehicles
**Description:** Create new vehicle

**Auth:** Required (Admin, Owner, Staff with vehicles permission)

**Request:**
```json
{
  "dealership_id": 1,
  "make": "Honda",
  "model": "Accord",
  "year": 2023,
  "price": 28500.00,
  "mileage": 5000,
  "condition": "used",
  "status": "active",
  "title": "2023 Honda Accord Sport",
  "description": "Like new condition...",
  "images": [
    "https://res.cloudinary.com/image1.jpg"
  ]
}
```

**Response (201):**
```json
{
  "id": 15,
  "dealership_id": 1,
  "make": "Honda",
  "model": "Accord",
  "year": 2023,
  "price": 28500.00,
  "mileage": 5000,
  "condition": "used",
  "status": "active",
  "title": "2023 Honda Accord Sport",
  "description": "Like new condition...",
  "images": ["https://res.cloudinary.com/image1.jpg"],
  "created_at": "2026-01-27T12:30:00.000Z"
}
```

**Response (400):**
```json
{
  "error": "Missing required fields: dealership_id, make, model, year, price, mileage, condition, status, title"
}
```

---

#### 4. PUT /api/vehicles/:id?dealershipId={id}
**Description:** Update existing vehicle

**Auth:** Required (Admin, Owner, Staff with vehicles permission)

**Path Parameters:**
- `id` (integer): Vehicle ID

**Query Parameters:**
- `dealershipId` (required, integer): Dealership ID for ownership verification

**Request (partial update allowed):**
```json
{
  "price": 26999.00,
  "status": "sold",
  "description": "SOLD - Updated description..."
}
```

**Response (200):**
```json
{
  "id": 1,
  "price": 26999.00,
  "status": "sold",
  "description": "SOLD - Updated description...",
  ...
}
```

**Response (404):**
```json
{
  "error": "Vehicle not found"
}
```

---

#### 5. DELETE /api/vehicles/:id?dealershipId={id}
**Description:** Delete vehicle

**Auth:** Required (Admin, Owner, Staff with vehicles permission)

**Path Parameters:**
- `id` (integer): Vehicle ID

**Query Parameters:**
- `dealershipId` (required, integer): Dealership ID for ownership verification

**Response (204):** No content

**Response (404):**
```json
{
  "error": "Vehicle not found"
}
```

---

### Lead Endpoints

#### 1. GET /api/leads?dealershipId={id}
**Description:** Get all leads for a dealership (admin inbox)

**Auth:** Required

**Query Parameters:**
- `dealershipId` (required, integer): Dealership ID

**Response (200):**
```json
[
  {
    "id": 1,
    "dealership_id": 1,
    "vehicle_id": 5,
    "name": "John Doe",
    "email": "john@example.com",
    "phone": "(555) 123-4567",
    "message": "Interested in the 2022 Camry...",
    "status": "received",
    "created_at": "2026-01-27T10:00:00.000Z"
  }
]
```

---

#### 2. POST /api/leads
**Description:** Submit customer enquiry (public form submission)

**Auth:** None required (public endpoint with CAPTCHA)

**Headers:**
```
x-captcha-token: <captcha-token>
```

**Request:**
```json
{
  "dealership_id": 1,
  "vehicle_id": 5,
  "name": "Jane Smith",
  "email": "jane@example.com",
  "phone": "(555) 987-6543",
  "message": "Is this vehicle still available?"
}
```

**Response (201):**
```json
{
  "id": 25,
  "dealership_id": 1,
  "vehicle_id": 5,
  "name": "Jane Smith",
  "email": "jane@example.com",
  "phone": "(555) 987-6543",
  "message": "Is this vehicle still available?",
  "status": "received",
  "created_at": "2026-01-27T12:45:00.000Z"
}
```

**Response (400):**
```json
{
  "error": "CAPTCHA verification failed"
}
```

---

#### 3. PATCH /api/leads/:id/status?dealershipId={id}
**Description:** Update lead status

**Auth:** Required (Admin, Owner, Staff with leads permission)

**Path Parameters:**
- `id` (integer): Lead ID

**Query Parameters:**
- `dealershipId` (required, integer): Dealership ID

**Request:**
```json
{
  "status": "in progress"
}
```

**Response (200):**
```json
{
  "id": 1,
  "status": "in progress",
  ...
}
```

**Valid Status Values:** `received`, `in progress`, `done`

---

#### 4. DELETE /api/leads/:id?dealershipId={id}
**Description:** Delete lead

**Auth:** Required (Admin, Owner, Staff with leads permission)

**Response (200):**
```json
{
  "message": "Lead deleted successfully"
}
```

---

### User Endpoints

#### 1. GET /api/users
**Description:** Get all users (admin: all users, owner: dealership users)

**Auth:** Required (Admin or Owner)

**Response (200):**
```json
[
  {
    "id": 1,
    "username": "admin",
    "email": "admin@example.com",
    "full_name": "System Administrator",
    "user_type": "admin",
    "dealership_id": null,
    "permissions": [],
    "created_at": "2026-01-01T00:00:00.000Z"
  },
  {
    "id": 2,
    "username": "owner1",
    "email": "owner@acme.com",
    "full_name": "John Owner",
    "user_type": "dealership_owner",
    "dealership_id": 1,
    "permissions": [],
    "created_at": "2026-01-02T00:00:00.000Z"
  }
]
```

---

#### 2. GET /api/users/:id
**Description:** Get specific user by ID

**Auth:** Required

**Path Parameters:**
- `id` (integer): User ID

**Response (200):**
```json
{
  "id": 2,
  "username": "owner1",
  "email": "owner@acme.com",
  "full_name": "John Owner",
  "user_type": "dealership_owner",
  "dealership_id": 1,
  "permissions": [],
  "created_at": "2026-01-02T00:00:00.000Z"
}
```

**Response (403):**
```json
{
  "error": "Access denied"
}
```

---

#### 3. POST /api/users
**Description:** Create new user

**Auth:** Required (Admin creates owners, Owners create staff)

**Request:**
```json
{
  "username": "staff1",
  "password": "password123",
  "email": "staff@acme.com",
  "full_name": "Jane Staff",
  "user_type": "dealership_staff",
  "dealership_id": 1,
  "permissions": ["vehicles", "leads"]
}
```

**Response (201):**
```json
{
  "id": 5,
  "username": "staff1",
  "email": "staff@acme.com",
  "full_name": "Jane Staff",
  "user_type": "dealership_staff",
  "dealership_id": 1,
  "permissions": ["vehicles", "leads"],
  "created_at": "2026-01-27T13:00:00.000Z"
}
```

---

#### 4. PUT /api/users/:id
**Description:** Update user information

**Auth:** Required

**Request:**
```json
{
  "email": "newemail@acme.com",
  "full_name": "Jane Updated Staff",
  "permissions": ["vehicles", "leads", "blogs"]
}
```

**Response (200):**
```json
{
  "id": 5,
  "email": "newemail@acme.com",
  "full_name": "Jane Updated Staff",
  "permissions": ["vehicles", "leads", "blogs"],
  ...
}
```

---

#### 5. PUT /api/users/:id/password
**Description:** Update user password

**Auth:** Required

**Request:**
```json
{
  "password": "newpassword123"
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Password updated successfully"
}
```

**Response (400):**
```json
{
  "error": "Password must be at least 6 characters"
}
```

---

#### 6. DELETE /api/users/:id
**Description:** Delete user (hard delete)

**Auth:** Required (Admin deletes any, Owner deletes staff)

**Response (200):**
```json
{
  "success": true,
  "message": "User deleted successfully"
}
```

**Response (400):**
```json
{
  "error": "Cannot delete your own account"
}
```

---

### Upload Endpoints

#### 1. POST /api/upload
**Description:** Upload image file to Cloudinary

**Auth:** None required (will be added in future)

**Content-Type:** `multipart/form-data`

**Form Data:**
- `image` (file): Image file (JPG, PNG, WebP)

**File Constraints:**
- Max size: 5MB
- Allowed types: `image/jpeg`, `image/png`, `image/webp`

**Response (200):**
```json
{
  "url": "https://res.cloudinary.com/dxyz/image/upload/v123/dealership-vehicles/abc123.jpg"
}
```

**Response (400):**
```json
{
  "error": "No file uploaded"
}
```

**Response (400):**
```json
{
  "error": "File too large. Maximum size is 5MB."
}
```

**Response (400):**
```json
{
  "error": "Invalid file type. Only JPG, PNG, and WebP are allowed."
}
```

---

### Sales Request Endpoints

#### 1. GET /api/sales-requests?dealershipId={id}
**Description:** Get all sales requests for dealership

**Auth:** Required

**Query Parameters:**
- `dealershipId` (required, integer): Dealership ID

**Response (200):**
```json
[
  {
    "id": 1,
    "dealership_id": 1,
    "name": "Bob Johnson",
    "email": "bob@example.com",
    "phone": "(555) 111-2222",
    "make": "Ford",
    "model": "F-150",
    "year": 2019,
    "kilometers": 75000,
    "additional_message": "Truck is in good condition...",
    "status": "received",
    "created_at": "2026-01-25T14:30:00.000Z"
  }
]
```

---

#### 2. POST /api/sales-requests
**Description:** Submit sales request (sell your car form)

**Auth:** None required (public endpoint with CAPTCHA)

**Headers:**
```
x-captcha-token: <captcha-token>
```

**Request:**
```json
{
  "dealership_id": 1,
  "name": "Alice Brown",
  "email": "alice@example.com",
  "phone": "(555) 333-4444",
  "make": "Honda",
  "model": "Civic",
  "year": 2020,
  "kilometers": 45000,
  "additional_message": "Looking to trade in for SUV"
}
```

**Response (201):**
```json
{
  "id": 8,
  "dealership_id": 1,
  "name": "Alice Brown",
  "email": "alice@example.com",
  "phone": "(555) 333-4444",
  "make": "Honda",
  "model": "Civic",
  "year": 2020,
  "kilometers": 45000,
  "additional_message": "Looking to trade in for SUV",
  "status": "received",
  "created_at": "2026-01-27T15:00:00.000Z"
}
```

---

#### 3. PATCH /api/sales-requests/:id/status?dealershipId={id}
**Description:** Update sales request status

**Auth:** Required (Admin, Owner, Staff with sales_requests permission)

**Request:**
```json
{
  "status": "in progress"
}
```

**Valid Status Values:** `received`, `in progress`, `done`

---

#### 4. DELETE /api/sales-requests/:id?dealershipId={id}
**Description:** Delete sales request

**Auth:** Required (Admin, Owner, Staff with sales_requests permission)

**Response (200):**
```json
{
  "message": "Sales request deleted successfully"
}
```

---

### Blog Endpoints

#### 1. GET /api/blogs?dealershipId={id}
**Description:** List all blog posts for dealership (admin view)

**Auth:** None required

**Query Parameters:**
- `dealershipId` (required, integer): Dealership ID

**Response (200):**
```json
[
  {
    "id": 1,
    "dealership_id": 1,
    "title": "5 Tips for First-Time Car Buyers",
    "slug": "5-tips-first-time-car-buyers",
    "content": "Full blog content here...",
    "excerpt": "Short excerpt...",
    "featured_image_url": "https://res.cloudinary.com/...",
    "author_name": "John Doe",
    "status": "published",
    "published_at": "2026-01-20T10:00:00.000Z",
    "created_at": "2026-01-19T14:00:00.000Z",
    "updated_at": "2026-01-19T14:00:00.000Z"
  }
]
```

---

#### 2. GET /api/blogs/published?dealershipId={id}
**Description:** List published blog posts only (public view)

**Auth:** None required

**Query Parameters:**
- `dealershipId` (required, integer): Dealership ID

**Response (200):**
```json
[
  {
    "id": 1,
    "title": "5 Tips for First-Time Car Buyers",
    "slug": "5-tips-first-time-car-buyers",
    ...
  }
]
```

---

#### 3. GET /api/blogs/slug/:slug?dealershipId={id}
**Description:** Get blog post by slug (for public URLs)

**Auth:** None required

**Path Parameters:**
- `slug` (string): URL-friendly slug

**Query Parameters:**
- `dealershipId` (required, integer): Dealership ID

**Response (200):**
```json
{
  "id": 1,
  "title": "5 Tips for First-Time Car Buyers",
  "slug": "5-tips-first-time-car-buyers",
  "content": "Full content...",
  ...
}
```

---

#### 4. GET /api/blogs/:id?dealershipId={id}
**Description:** Get blog post by ID

**Auth:** None required

**Response (200):**
```json
{
  "id": 1,
  "title": "5 Tips for First-Time Car Buyers",
  ...
}
```

---

#### 5. POST /api/blogs
**Description:** Create new blog post

**Auth:** Required (Admin, Owner, Staff with blogs permission)

**Request:**
```json
{
  "dealership_id": 1,
  "title": "Winter Maintenance Tips",
  "slug": "winter-maintenance-tips",
  "content": "Full blog content here...",
  "excerpt": "Keep your car running smoothly...",
  "featured_image_url": "https://res.cloudinary.com/...",
  "author_name": "Jane Doe",
  "status": "draft"
}
```

**Response (201):**
```json
{
  "id": 5,
  "dealership_id": 1,
  "title": "Winter Maintenance Tips",
  "slug": "winter-maintenance-tips",
  ...
}
```

**Valid Status Values:** `draft`, `published`, `archived`

---

#### 6. PUT /api/blogs/:id?dealershipId={id}
**Description:** Update blog post

**Auth:** Required (Admin, Owner, Staff with blogs permission)

**Request:**
```json
{
  "title": "Updated Title",
  "status": "published",
  "published_at": "2026-01-27T16:00:00.000Z"
}
```

---

#### 7. DELETE /api/blogs/:id?dealershipId={id}
**Description:** Delete blog post

**Auth:** Required (Admin, Owner, Staff with blogs permission)

**Response (200):**
```json
{
  "message": "Blog post deleted successfully"
}
```

---

### Google Reviews Endpoints

#### 1. GET /api/google-reviews/:dealershipId
**Description:** Fetch Google reviews for dealership using Google Places API

**Auth:** None required

**Path Parameters:**
- `dealershipId` (integer): Dealership ID

**Response (200):**
```json
{
  "reviews": [
    {
      "author_name": "John Smith",
      "rating": 5,
      "text": "Excellent service! Highly recommend...",
      "time": 1706356800,
      "relative_time_description": "2 weeks ago",
      "profile_photo_url": "https://lh3.googleusercontent.com/..."
    }
  ],
  "googleMapsUrl": "https://www.google.com/maps/place/...",
  "totalRatings": 127,
  "averageRating": 4.8
}
```

**Response (200) - No API Key:**
```json
{
  "reviews": [],
  "googleMapsUrl": "",
  "message": "Google Reviews not configured"
}
```

**Response (200) - Not Found:**
```json
{
  "reviews": [],
  "googleMapsUrl": "",
  "message": "Location not found on Google Maps"
}
```

---

### Design Template Endpoints

#### 1. GET /api/design-templates
**Description:** List all design templates available to dealership

**Auth:** Required

**Query Parameters (optional):**
- `dealership_id` (integer): For admin users to view specific dealership templates

**Response (200):**
```json
[
  {
    "id": 1,
    "dealership_id": null,
    "name": "Modern Blue",
    "description": "Clean and professional blue theme",
    "theme_color": "#3B82F6",
    "secondary_theme_color": "#FFFFFF",
    "body_background_color": "#F9FAFB",
    "font_family": "system",
    "is_preset": true,
    "created_at": "2026-01-01T00:00:00.000Z"
  },
  {
    "id": 15,
    "dealership_id": 1,
    "name": "Acme Custom Red",
    "description": "Custom red theme for Acme",
    "theme_color": "#DC2626",
    "secondary_theme_color": "#FECACA",
    "body_background_color": "#FFFFFF",
    "font_family": "arial",
    "is_preset": false,
    "created_at": "2026-01-15T10:00:00.000Z"
  }
]
```

---

#### 2. POST /api/design-templates
**Description:** Create new custom design template

**Auth:** Required (Admin, Owner, Staff with settings permission)

**Request:**
```json
{
  "name": "My Custom Theme",
  "description": "Custom theme for special events",
  "theme_color": "#10B981",
  "secondary_theme_color": "#D1FAE5",
  "body_background_color": "#ECFDF5",
  "font_family": "times"
}
```

**Response (201):**
```json
{
  "id": 20,
  "dealership_id": 1,
  "name": "My Custom Theme",
  "description": "Custom theme for special events",
  "theme_color": "#10B981",
  "secondary_theme_color": "#D1FAE5",
  "body_background_color": "#ECFDF5",
  "font_family": "times",
  "is_preset": false,
  "created_at": "2026-01-27T16:30:00.000Z"
}
```

**Response (400):**
```json
{
  "error": "Invalid theme color format. Use hex format: #RRGGBB or #RGB"
}
```

---

#### 3. DELETE /api/design-templates/:id
**Description:** Delete custom design template (presets cannot be deleted)

**Auth:** Required (Admin, Owner, Staff with settings permission)

**Path Parameters:**
- `id` (integer): Template ID

**Query Parameters:**
- `dealership_id` (optional, integer): Required for staff users

**Response (200):**
```json
{
  "message": "Template deleted successfully"
}
```

**Response (404):**
```json
{
  "error": "Template not found or cannot be deleted"
}
```

---

### Health Check Endpoint

#### GET /api/health
**Description:** Server health check

**Auth:** None required

**Response (200):**
```json
{
  "status": "ok",
  "timestamp": "2026-01-27T16:45:00.000Z"
}
```

---

## External Services

### 1. Cloudinary (Image Upload Service)

**Purpose:** Store vehicle photos, dealership logos, blog images

**Configuration:**
```env
CLOUDINARY_CLOUD_NAME=your-cloud-name
CLOUDINARY_API_KEY=your-api-key
CLOUDINARY_API_SECRET=your-api-secret
```

**Upload Folder:** `dealership-vehicles`

**Supported Formats:** JPG, PNG, WebP

**.NET Implementation:**
- Use `CloudinaryDotNet` NuGet package
- Configure in `appsettings.json` or user secrets
- Create service: `IImageUploadService`

---

### 2. Google Places API

**Purpose:** Fetch dealership reviews from Google Maps

**Configuration:**
```env
GOOGLE_PLACES_API_KEY=your-api-key
```

**Endpoints Used:**
- Text Search API: Find place by name + address
- Place Details API: Get reviews and rating

**.NET Implementation:**
- Use `HttpClient` to call Google APIs
- Create service: `IGoogleReviewsService`
- Cache results to reduce API calls

---

### 3. Email Service (Nodemailer)

**Purpose:** Send email notifications for new leads and sales requests

**Configuration:**
```env
EMAIL_USER=your-email@gmail.com
EMAIL_PASSWORD=your-app-password
EMAIL_HOST=smtp.gmail.com
EMAIL_PORT=587
EMAIL_SECURE=false
```

**Email Templates:**

1. **New Lead Notification:**
   - To: Dealership email
   - Subject: "New Customer Enquiry - [Vehicle Info]"
   - Body: Customer name, email, phone, message, vehicle details

2. **New Sales Request Notification:**
   - To: Dealership email
   - Subject: "New Vehicle Sales Request"
   - Body: Customer info, vehicle details (make, model, year, km)

**.NET Implementation:**
- Use `MailKit` NuGet package
- Create service: `IEmailService`
- Template engine for HTML emails

---

## Middleware & Cross-Cutting Concerns

### 1. Authentication Middleware

**Node.js:**
```javascript
function requireAuth(req, res, next) {
  if (!req.session.user) {
    return res.status(401).json({ error: 'Authentication required' });
  }
  next();
}
```

**.NET Equivalent:**
```csharp
[Authorize]
public class SecureController : ControllerBase { }
```

---

### 2. Authorization Middleware

**Permission-Based:**
```javascript
function requirePermission(permission) {
  return (req, res, next) => {
    const user = req.session.user;
    if (user.user_type === 'dealership_staff' && 
        !user.permissions.includes(permission)) {
      return res.status(403).json({ error: 'Insufficient permissions' });
    }
    next();
  };
}
```

**.NET Equivalent:**
```csharp
[Authorize(Policy = "RequireVehiclesPermission")]
public async Task<IActionResult> CreateVehicle() { }
```

---

### 3. Dealership Scope Enforcement

**Node.js:**
```javascript
function enforceDealershipScope(req, res, next) {
  if (user.user_type === 'admin') return next();
  
  const requestedDealershipId = parseInt(req.body?.dealership_id || req.query?.dealershipId);
  
  if (requestedDealershipId !== user.dealership_id) {
    return res.status(403).json({ error: 'Access denied to this dealership' });
  }
  next();
}
```

**.NET Equivalent:**
- Custom authorization handler: `DealershipScopeHandler`
- Policy: `RequireDealershipScope`

---

### 4. Input Validation & Sanitization

**XSS Prevention:**
```javascript
function sanitizeInput(input) {
  return input
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#x27;');
}
```

**.NET Equivalent:**
- Use `System.Net.WebUtility.HtmlEncode()`
- Or FluentValidation + custom sanitizer

---

### 5. CAPTCHA Verification

**Node.js:**
```javascript
async function captchaVerification(req, res, next) {
  const token = req.headers['x-captcha-token'];
  // Verify token with CAPTCHA service
  if (!isValid) {
    return res.status(400).json({ error: 'CAPTCHA verification failed' });
  }
  next();
}
```

**.NET Equivalent:**
- Create `CaptchaVerificationFilter` action filter
- Use Google reCAPTCHA v3 or similar

---

### 6. Navigation Config Validation

**Purpose:** Validate JSONB navigation_config structure

**Node.js:**
```javascript
function validateNavigationConfig(req, res, next) {
  const { navigation_config } = req.body;
  if (!navigation_config) return next();
  
  if (!Array.isArray(navigation_config)) {
    return res.status(400).json({ error: 'navigation_config must be an array' });
  }
  
  // Validate each item has required fields
  next();
}
```

**.NET Equivalent:**
- Use FluentValidation with custom validator
- Validate JSON structure before deserialization

---

## Environment Configuration

### Required Environment Variables

```env
# Database
DATABASE_URL=postgresql://postgres:password@localhost:5432/jeal_prototype

# Session Secret
SESSION_SECRET=your-random-secret-key-here

# Cloudinary
CLOUDINARY_CLOUD_NAME=your-cloud-name
CLOUDINARY_API_KEY=your-api-key
CLOUDINARY_API_SECRET=your-api-secret

# Google Places API
GOOGLE_PLACES_API_KEY=your-google-api-key

# Email (SMTP)
EMAIL_USER=your-email@gmail.com
EMAIL_PASSWORD=your-app-password
EMAIL_HOST=smtp.gmail.com
EMAIL_PORT=587
EMAIL_SECURE=false

# Server
PORT=5000
NODE_ENV=development

# CORS
CORS_ORIGIN=http://localhost:3000
```

### .NET appsettings.json Equivalent

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=jeal_prototype;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "SecretKey": "your-jwt-secret-key",
    "Issuer": "JealPrototype",
    "Audience": "JealPrototypeClient",
    "ExpirationMinutes": 1440
  },
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  },
  "GooglePlaces": {
    "ApiKey": "your-google-api-key"
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@yourapp.com",
    "FromName": "Jeal Prototype"
  },
  "CorsSettings": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

---

## Validation Rules Summary

### Field Constraints

| Field | Type | Max Length | Constraints |
|-------|------|------------|-------------|
| dealership.name | string | 255 | Required |
| dealership.address | string | 255 | Required |
| dealership.phone | string | 20 | Required |
| dealership.email | string | 255 | Required, valid email format |
| dealership.theme_color | string | 7 | Hex color #RRGGBB or #RGB |
| vehicle.make | string | 100 | Required |
| vehicle.model | string | 100 | Required |
| vehicle.year | integer | - | Required, 1900-2100 |
| vehicle.price | decimal | - | Required, >= 0 |
| vehicle.condition | string | 10 | Required, enum: 'new', 'used' |
| vehicle.status | string | 10 | Required, enum: 'active', 'sold', 'pending', 'draft' |
| lead.name | string | 255 | Required |
| lead.email | string | 255 | Required, valid email format |
| lead.phone | string | 20 | Required |
| lead.message | string | 5000 | Required |
| user.username | string | 100 | Required, unique |
| user.password | string | - | Required, min 6 characters |
| blog_post.title | string | 255 | Required |
| blog_post.content | string | 50000 | Required |

---

## Error Response Codes

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Successful GET, PUT, PATCH |
| 201 | Created | Successful POST |
| 204 | No Content | Successful DELETE |
| 400 | Bad Request | Validation errors, missing fields |
| 401 | Unauthorized | Authentication required |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource not found |
| 500 | Internal Server Error | Server/database errors |
| 503 | Service Unavailable | External service failure |

---

## Implementation Checklist

### Phase 1: Foundation ✅
- [ ] .NET Solution structure (Clean Architecture)
- [ ] EF Core DbContext with entities
- [ ] Database migrations matching existing schema
- [ ] Connection to existing PostgreSQL database
- [ ] JWT authentication setup

### Phase 2: Core Features
- [ ] Dealership CRUD endpoints
- [ ] Vehicle CRUD endpoints with filters
- [ ] Lead submission + management
- [ ] User management + hierarchy
- [ ] Sales request endpoints

### Phase 3: Advanced Features
- [ ] Cloudinary image upload service
- [ ] Email notification service
- [ ] Google Reviews integration
- [ ] Blog post management
- [ ] Design templates

### Phase 4: Testing & Validation
- [ ] Integration tests for all endpoints
- [ ] Validate same database works with both backends
- [ ] Frontend compatibility testing
- [ ] Performance benchmarking

---

## Notes for Implementation

1. **Database Compatibility:** The .NET backend MUST work with the existing PostgreSQL database without schema changes.

2. **API Contract:** ALL endpoints must maintain exact same request/response formats for frontend compatibility.

3. **Authentication:** Consider JWT over session-based for better scalability and stateless API.

4. **Multi-Tenancy:** Enforce dealership_id filtering at repository level using EF Core query filters.

5. **Validation:** Use FluentValidation for cleaner validation logic separate from controllers.

6. **Mapping:** Use AutoMapper or Mapster to map between entities and DTOs.

7. **Error Handling:** Implement global exception handler middleware for consistent error responses.

8. **Logging:** Use Serilog for structured logging with context enrichment.

---

**End of API Specification Document**
