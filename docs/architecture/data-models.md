# 4. Data Models

Three core entities support all dealership website and CMS functionality: **Dealership** (tenant configuration), **Vehicle** (inventory), and **Lead** (customer enquiries). All tables enforce multi-tenancy via `dealership_id` foreign keys.

## Dealership

**Purpose:** Stores dealership profile information displayed on public website (name, logo, contact info) and editable via admin CMS. Each dealership is an independent tenant.

**Key Attributes:**
- `id`: Serial primary key - unique dealership identifier
- `name`: VARCHAR(255) NOT NULL - dealership business name (e.g., "Acme Auto Sales")
- `logo_url`: TEXT - Cloudinary URL for logo image (nullable - not required for MVP)
- `hero_background_image`: TEXT - Cloudinary URL for hero section background image on public home page (nullable - falls back to default blue gradient if not set)
- `address`: TEXT NOT NULL - full street address for contact page
- `phone`: VARCHAR(20) NOT NULL - contact phone number
- `email`: VARCHAR(255) NOT NULL - contact email address
- `hours`: TEXT - business hours (e.g., "Mon-Fri 9am-6pm, Sat 10am-4pm")
- `about`: TEXT - about us description for public website
- `finance_policy`: TEXT - financing options and policy content for public Finance page
- `warranty_policy`: TEXT - warranty coverage and terms for public Warranty page
- `created_at`: TIMESTAMP DEFAULT NOW() - record creation timestamp

### TypeScript Interface (documentation only - implement in plain JavaScript)

```typescript
interface Dealership {
  id: number;
  name: string;
  logo_url: string | null;
  hero_background_image: string | null;
  address: string;
  phone: string;
  email: string;
  hours: string | null;
  about: string | null;
  finance_policy: string | null;
  warranty_policy: string | null;
  created_at: Date;
}
```

### PostgreSQL Table Definition

```sql
CREATE TABLE dealership (
  id SERIAL PRIMARY KEY,
  name VARCHAR(255) NOT NULL,
  logo_url TEXT,
  hero_background_image TEXT,
  address TEXT NOT NULL,
  phone VARCHAR(20) NOT NULL,
  email VARCHAR(255) NOT NULL,
  hours TEXT,
  about TEXT,
  finance_policy TEXT,
  warranty_policy TEXT,
  created_at TIMESTAMP DEFAULT NOW()
);

-- Seed data for 2 demo dealerships
INSERT INTO dealership (name, address, phone, email, hours, about, finance_policy, warranty_policy, hero_background_image) VALUES
  ('Acme Auto Sales', '123 Main St, Springfield, IL 62701', '(555) 123-4567', 'sales@acmeauto.com', 'Mon-Fri 9am-6pm, Sat 10am-4pm, Sun Closed', 'Family-owned dealership serving Springfield for 20 years. Quality used cars at affordable prices.', 'We offer flexible financing options to fit your budget. Contact us for a personalized quote.', 'All vehicles include 30-day limited warranty. Extended warranties available.', NULL),
  ('Premier Motors', '456 Oak Ave, Springfield, IL 62702', '(555) 987-6543', 'info@premiermotors.com', 'Mon-Sat 9am-7pm, Sun 11am-5pm', 'Premier Motors offers a wide selection of certified pre-owned vehicles with extended warranties.', 'Premier financing with rates as low as 2.9% APR. Pre-qualification available online.', 'Industry-leading warranty: 24 months/24,000 miles bumper-to-bumper. Powertrain: 5 years/60,000 miles.', NULL);
```

### Relationships
- **One-to-Many with Vehicle:** `dealership.id → vehicle.dealership_id`
- **One-to-Many with Lead:** `dealership.id → lead.dealership_id`

## Vehicle

**Purpose:** Represents individual vehicles in dealership inventory. Displayed on public website listing/detail pages. Managed via admin CMS create/edit forms.

**Key Attributes:**
- `id`: Serial primary key - unique vehicle identifier
- `dealership_id`: INTEGER NOT NULL FK - owner dealership (multi-tenancy key)
- `make`: VARCHAR(100) NOT NULL - manufacturer (e.g., "Toyota")
- `model`: VARCHAR(100) NOT NULL - model name (e.g., "Camry")
- `year`: INTEGER NOT NULL - model year (e.g., 2015)
- `price`: DECIMAL(10,2) NOT NULL - sale price in dollars (e.g., 15999.99)
- `mileage`: INTEGER NOT NULL - odometer reading (e.g., 75000)
- `condition`: VARCHAR(10) NOT NULL - "new" or "used"
- `status`: VARCHAR(10) NOT NULL - "active", "sold", "pending", "draft"
- `title`: VARCHAR(255) NOT NULL - display title (e.g., "2015 Toyota Camry SE")
- `description`: TEXT - detailed vehicle description
- `images`: JSONB - array of Cloudinary URLs (e.g., `["https://res.cloudinary.com/...jpg", "..."]`)
- `created_at`: TIMESTAMP DEFAULT NOW() - listing creation timestamp

**Status Field Values:**
- `active`: Public-visible, available for sale
- `pending`: Public-visible with "Pending Sale" badge
- `sold`: Hidden from public, visible in admin (marked as sold)
- `draft`: Admin-only, not published to public site

### TypeScript Interface (documentation only)

```typescript
interface Vehicle {
  id: number;
  dealership_id: number;
  make: string;
  model: string;
  year: number;
  price: number;
  mileage: number;
  condition: 'new' | 'used';
  status: 'active' | 'sold' | 'pending' | 'draft';
  title: string;
  description: string | null;
  images: string[]; // Array of Cloudinary URLs
  created_at: Date;
}
```

### PostgreSQL Table Definition

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

-- Index for multi-tenant queries (critical for performance)
CREATE INDEX idx_vehicle_dealership_id ON vehicle(dealership_id);

-- Index for status filtering (public site shows active and pending vehicles)
-- NOTE: Public inventory pages should filter client-side for (status='active' OR status='pending')
-- This displays both available vehicles and vehicles pending sale, excluding sold/draft
CREATE INDEX idx_vehicle_status ON vehicle(status);

-- Sample seed data (run for each dealership_id)
INSERT INTO vehicle (dealership_id, make, model, year, price, mileage, condition, status, title, description, images) VALUES
  (1, 'Toyota', 'Camry', 2015, 15999.99, 75000, 'used', 'active', '2015 Toyota Camry SE', 'Well-maintained sedan with excellent fuel economy. Clean title, one owner.', '["https://res.cloudinary.com/demo/image/upload/sample.jpg"]'),
  (1, 'Honda', 'Civic', 2018, 18500.00, 45000, 'used', 'active', '2018 Honda Civic LX', 'Low mileage, excellent condition. Bluetooth, backup camera, automatic transmission.', '[]');
```

### Relationships
- **Many-to-One with Dealership:** `vehicle.dealership_id → dealership.id` (required, cascade delete)
- **One-to-Many with Lead:** `vehicle.id → lead.vehicle_id` (optional reference)

## Lead

**Purpose:** Captures customer enquiries submitted via public website enquiry forms. Displayed in admin CMS lead inbox for dealership follow-up.

**Key Attributes:**
- `id`: Serial primary key - unique lead identifier
- `dealership_id`: INTEGER NOT NULL FK - which dealership received this lead
- `vehicle_id`: INTEGER FK - related vehicle (nullable - general enquiries don't reference specific vehicle)
- `name`: VARCHAR(255) NOT NULL - customer name
- `email`: VARCHAR(255) NOT NULL - customer email
- `phone`: VARCHAR(20) NOT NULL - customer phone number
- `message`: TEXT NOT NULL - customer enquiry message
- `created_at`: TIMESTAMP DEFAULT NOW() - submission timestamp (used for sorting)

### TypeScript Interface (documentation only)

```typescript
interface Lead {
  id: number;
  dealership_id: number;
  vehicle_id: number | null;
  name: string;
  email: string;
  phone: string;
  message: string;
  created_at: Date;
}
```

### PostgreSQL Table Definition

```sql
CREATE TABLE lead (
  id SERIAL PRIMARY KEY,
  dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE,
  vehicle_id INTEGER REFERENCES vehicle(id) ON DELETE SET NULL,
  name VARCHAR(255) NOT NULL,
  email VARCHAR(255) NOT NULL,
  phone VARCHAR(20) NOT NULL,
  message TEXT NOT NULL,
  created_at TIMESTAMP DEFAULT NOW()
);

-- Index for multi-tenant queries (admin lead inbox filters by dealership_id)
CREATE INDEX idx_lead_dealership_id ON lead(dealership_id);

-- Index for sorting by submission date (newest first)
CREATE INDEX idx_lead_created_at ON lead(created_at DESC);

-- Sample seed data
INSERT INTO lead (dealership_id, vehicle_id, name, email, phone, message) VALUES
  (1, 1, 'John Doe', 'john@example.com', '(555) 111-2222', 'I''m interested in the 2015 Toyota Camry. Is it still available?'),
  (1, NULL, 'Jane Smith', 'jane@example.com', '(555) 333-4444', 'Do you offer financing options?');
```

### Relationships
- **Many-to-One with Dealership:** `lead.dealership_id → dealership.id` (required, cascade delete)
- **Many-to-One with Vehicle:** `lead.vehicle_id → vehicle.id` (optional, set null on vehicle delete)

## Entity Relationship Diagram

```
┌─────────────┐
│ dealership  │
├─────────────┤
│ id (PK)     │──┐
│ name        │  │
│ logo_url    │  │ One-to-Many
│ address     │  │
│ phone       │  ├──> vehicle.dealership_id (FK)
│ email       │  │
│ hours       │  │
│ about       │  │
└─────────────┘  │
                 │
                 ├──> lead.dealership_id (FK)

┌─────────────────┐
│ vehicle         │
├─────────────────┤
│ id (PK)         │──┐
│ dealership_id FK│  │
│ make            │  │ One-to-Many (optional)
│ model           │  │
│ year            │  ├──> lead.vehicle_id (FK, nullable)
│ price           │  │
│ mileage         │  │
│ condition       │  │
│ status          │  │
│ title           │  │
│ description     │  │
│ images (JSONB)  │  │
└─────────────────┘  │
                     │
┌─────────────────┐  │
│ lead            │<─┘
├─────────────────┤
│ id (PK)         │
│ dealership_id FK│
│ vehicle_id FK   │ (nullable)
│ name            │
│ email           │
│ phone           │
│ message         │
│ created_at      │
└─────────────────┘
```

## Multi-Tenancy Data Isolation

**⚠️ CRITICAL SECURITY REQUIREMENT (SEC-001)**

**ALL tenant-scoped queries MUST filter by `dealership_id`** to prevent cross-tenant data leaks, modifications, and deletions.

**📖 Complete Security Implementation Guide:** [security-guidelines.md](./security-guidelines.md#1-multi-tenancy-data-isolation-sec-001)

This section provides database-level patterns. For complete API endpoint security including:
- XSS prevention
- Input validation
- SQL injection prevention
- Testing requirements

**→ See [Security Guidelines](./security-guidelines.md) for comprehensive patterns and reusable functions.**

---

**Critical Implementation Rule:** ALL queries for vehicles, leads, and other tenant-scoped resources MUST filter by `dealership_id`.

### List Queries (e.g., GET /api/vehicles)
```javascript
// ✅ CORRECT - filters by dealership_id
const vehicles = await db.query(
  'SELECT * FROM vehicle WHERE dealership_id = $1 AND status = $2',
  [dealershipId, 'active']
);

// ❌ WRONG - no dealership filter (SECURITY VULNERABILITY!)
const vehicles = await db.query('SELECT * FROM vehicle WHERE status = $1', ['active']);
```

### Single-Record Queries (e.g., GET /api/vehicles/:id, PUT, DELETE)
```javascript
// ✅ CORRECT - filters by BOTH id AND dealership_id (defense-in-depth)
const vehicle = await db.query(
  'SELECT * FROM vehicle WHERE id = $1 AND dealership_id = $2',
  [vehicleId, dealershipId]
);

// ❌ WRONG - only filters by id (allows cross-dealership access by ID guessing!)
const vehicle = await db.query('SELECT * FROM vehicle WHERE id = $1', [vehicleId]);
```

### Update/Delete Queries
```javascript
// ✅ CORRECT - filters by BOTH id AND dealership_id in WHERE clause
await db.query(
  'UPDATE vehicle SET price = $1 WHERE id = $2 AND dealership_id = $3',
  [newPrice, vehicleId, dealershipId]
);

await db.query(
  'DELETE FROM vehicle WHERE id = $1 AND dealership_id = $2',
  [vehicleId, dealershipId]
);

// ❌ WRONG - only filters by id (allows cross-dealership modifications/deletions!)
await db.query('UPDATE vehicle SET price = $1 WHERE id = $2', [newPrice, vehicleId]);
await db.query('DELETE FROM vehicle WHERE id = $1', [vehicleId]);
```

**Why Both Filters Are Required:**
- **List queries:** Filter by `dealership_id` only (no ID involved)
- **Single-record queries:** Filter by BOTH `id` AND `dealership_id` to ensure the requested vehicle belongs to the requesting dealership
- **Without dual filtering:** An attacker could guess vehicle IDs and access/modify/delete vehicles from other dealerships
- **API Contract (v1.1+):** All vehicle endpoints (GET/:id, PUT/:id, DELETE/:id) require `dealershipId` query parameter and return 404 if vehicle belongs to different dealership

**Database Constraints Enforce Integrity:**
- Foreign key constraints prevent orphaned records (e.g., can't create vehicle with non-existent `dealership_id`)
- `ON DELETE CASCADE` ensures deleting dealership removes all vehicles/leads
- `ON DELETE SET NULL` for `lead.vehicle_id` preserves lead if vehicle deleted (customer enquiry history retained)
- `CHECK` constraints on `condition` and `status` prevent invalid values

## Implementation Notes

**Database Setup Script:** Create `backend/db/schema.sql` with all CREATE TABLE statements above. Run once on Railway Postgres:
```bash
railway run psql $DATABASE_URL < backend/db/schema.sql
```

**Seed Data:** Create `backend/db/seed.sql` with sample INSERT statements. Run after schema to populate 2 demo dealerships with 5-10 vehicles each.

**JSONB for Images Array:**
Store Cloudinary URLs as JSON array: `["url1.jpg", "url2.jpg"]`. PostgreSQL JSONB supports array operations. In JavaScript:
```javascript
// Insert vehicle with images
const images = JSON.stringify(['https://res.cloudinary.com/...', '...']);
await db.query('INSERT INTO vehicle (..., images) VALUES (..., $1)', [images]);

// Query returns JSONB as parsed JavaScript array automatically
const result = await db.query('SELECT images FROM vehicle WHERE id = $1', [id]);
console.log(result.rows[0].images); // ['url1', 'url2']
```

**Estimated Schema Setup Time:** 30 minutes (write SQL, test locally, deploy to Railway)

---
