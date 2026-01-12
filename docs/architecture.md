# JealPrototypeTest Fullstack Architecture Document

**Version:** 1.1
**Date:** 2025-12-09
**Status:** Final
**Author:** Winston (Architect Agent)

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [High Level Architecture](#2-high-level-architecture)
3. [Tech Stack](#3-tech-stack)
4. [Data Models](#4-data-models)
5. [API Specification](#5-api-specification)
6. [Components](#6-components)
7. [External APIs](#7-external-apis---cloudinary-integration)
8. [Database Schema](#8-database-schema)
9. [Source Tree](#9-source-tree)
10. [Development Workflow](#10-development-workflow)
11. [Coding Standards](#11-coding-standards)
11.1. [Dynamic Theme Color Management System](#111-dynamic-theme-color-management-system)
12. [2-Day Implementation Checklist](#12-2-day-implementation-checklist)

---

## 1. Introduction

### Starter Template or Existing Project

**N/A - Greenfield Project**

This is a greenfield project with no existing starter template or codebase. The PRD explicitly defines a **custom monorepo structure** (`/backend` and `/frontend` folders) with Node.js/Express backend and React frontend, deployed as a unified full-stack application.

**Rationale for Not Using a Starter:**
- The PRD specifies custom requirements (multi-tenancy via `dealershipId`, Cloudinary integration, specific monorepo structure) that don't align well with opinionated starters
- The 48-hour timeline benefits from minimal boilerplate - using Create React App/Vite for frontend and vanilla Express for backend is faster than learning/configuring a fullstack starter
- Free tier constraints (Railway/Render) work best with simple, standard deployments rather than framework-specific configurations

**Alternative Starters Considered (but not recommended):**
- **T3 Stack** (Next.js + tRPC + Tailwind + Prisma): Great for type-safety but adds learning curve for tRPC/Prisma; PRD already specifies REST API
- **MERN Starters**: Most assume separate frontend/backend hosting; PRD requires unified deployment
- **Turborepo/Nx Monorepo Templates**: Overkill for 2-package monorepo; adds complexity without benefit

### Introduction Content

This document outlines the complete fullstack architecture for **JealPrototypeTest - Multi-Dealership Car Website + CMS Platform**, including backend systems, frontend implementation, and their integration. It serves as the single source of truth for AI-driven development, ensuring consistency across the entire technology stack.

This unified approach combines what would traditionally be separate backend and frontend architecture documents, streamlining the development process for a modern fullstack application where these concerns are tightly integrated. The architecture is optimized for a **48-hour development timeline** while maintaining production viability for small-to-medium dealership deployments.

**Key Architectural Principles:**
- **Multi-tenancy from Day 1:** `dealershipId`-based data isolation enabling multiple independent dealerships
- **Zero-cost deployment:** Entire stack runs on free tiers (Railway/Render + PostgreSQL + Cloudinary)
- **Monolith simplicity:** Single codebase, single deployment, unified backend serving frontend
- **Rapid iteration:** Technology choices prioritize speed of implementation and proven reliability

### Change Log

| Date | Version | Description | Author |
|------|---------|-------------|--------|
| 2025-11-19 | 1.0 | Initial architecture document | Winston (Architect Agent) |
| 2025-12-09 | 1.1 | Added General Enquiry Form component documentation. Updated Components section to include GeneralEnquiryForm.jsx with technical specifications for homepage general enquiry submission. No API or database schema changes required - feature uses existing /api/leads endpoint with optional vehicle_id. | Winston (Architect Agent) |

---

## 2. High Level Architecture

### Technical Summary

**JealPrototypeTest** is a monolithic full-stack application with multi-tenant data architecture. The backend is a Node.js/Express REST API serving both API endpoints and the built React frontend as static files. PostgreSQL provides relational storage with `dealershipId` foreign keys enforcing tenant isolation. Cloudinary handles all image uploads via its upload widget (frontend) and transformation API (CDN delivery). The application deploys as a single service to Railway or Render, with database and environment variables configured via platform add-ons. This architecture achieves the PRD goal of rapid 2-day development by eliminating microservices complexity, separate frontend hosting, and custom image infrastructure—while maintaining production viability for 2-5 small dealerships (~50 vehicles each).

### Platform and Infrastructure Choice

**Platform:** Railway (primary) or Render (fallback)

**Key Services:**
- **Compute:** Railway web service (free tier: 500 hours/month, $5 credit) running Node.js app
- **Database:** Railway PostgreSQL plugin (free tier: 1GB storage, 1GB RAM)
- **CDN/Storage:** Cloudinary (free tier: 25GB storage, 25GB bandwidth/month)
- **SSL/Domain:** Railway-provided HTTPS domain (`*.up.railway.app`)

**Deployment Host and Regions:** Railway US region (auto-assigned), Cloudinary US datacenter

**Decision Rationale:**
- **Railway over Render:** Simpler Git-based deploys, PostgreSQL plugin auto-wires `DATABASE_URL`, generous free tier
- **Railway over Vercel/Netlify:** Need unified backend+frontend hosting; Vercel/Netlify serverless functions add complexity
- **PostgreSQL over MongoDB:** Relational model enforces referential integrity for multi-tenancy (foreign keys prevent orphaned vehicles/leads)
- **Cloudinary over S3/local storage:** Upload widget eliminates file handling code, auto-optimization (WebP, resizing) is free, CDN included

**Fallback Plan:** If Railway free tier exhausted, migrate to Render (identical architecture, different platform config)

### Repository Structure

**Structure:** Monorepo with `/backend` and `/frontend` workspaces

**Monorepo Tool:** npm workspaces (lightweight, no additional dependencies)

**Package Organization:**
```
/
├── backend/              # Express API server
│   ├── package.json
│   ├── server.js         # Entry point
│   ├── routes/           # API route handlers
│   ├── db/              # Database queries
│   └── middleware/       # Auth, error handling
├── frontend/             # React app
│   ├── package.json
│   ├── src/
│   │   ├── public/      # Public site components
│   │   ├── admin/       # Admin CMS components
│   │   └── shared/      # Shared utilities
│   └── dist/            # Production build output
├── package.json          # Root workspace config
└── .env                 # Local dev environment vars
```

**Rationale:** Simple two-package structure matches PRD requirements. Root `package.json` scripts run both servers concurrently in dev (`npm run dev`). Backend serves `frontend/dist` in production via `express.static()`. No shared code packages needed—API is the contract between frontend and backend.

### High Level Architecture Diagram

```mermaid
graph TB
    subgraph "User Access"
        A[Car Buyer - Browser]
        B[Dealership Staff - Browser]
    end

    subgraph "Railway Deployment"
        C[Express Server :5000]
        D[React App - Static Build]
        E[PostgreSQL Database]
    end

    F[Cloudinary CDN]

    A -->|Browse /inventory| D
    A -->|API calls /api/*| C
    B -->|Admin /admin/*| D
    B -->|API calls /api/*| C

    C -->|Serves static files| D
    C -->|SQL queries with dealershipId filter| E
    C -->|Upload images| F
    D -->|Fetch images| F

    E -.->|Tables: Dealership, Vehicle, Lead| E
```

**Key Data Flows:**
1. **Public visitor:** Browser → React app (static) → API calls → Express → PostgreSQL → JSON response → React renders
2. **Image upload (admin):** Cloudinary widget → direct upload to Cloudinary → URL returned → React → API → save URL to PostgreSQL
3. **Multi-tenancy:** All API queries include `WHERE dealership_id = ?` filter

### Architectural Patterns

- **Monolithic Architecture:** Single deployment containing API and frontend - _Rationale:_ Fastest path to production; sufficient for 2-5 dealerships with <100 vehicles each; avoids CORS, service orchestration, and deployment coordination overhead

- **REST API:** JSON-based HTTP endpoints following RESTful conventions - _Rationale:_ Universally understood, simple to test (Postman/curl), no additional transport layer (vs GraphQL/tRPC); PRD explicitly specifies REST

- **Repository Pattern (Backend):** Abstract database queries into `db/` modules (e.g., `db/vehicles.js` exports `getAllVehicles(dealershipId)`) - _Rationale:_ Centralizes `dealershipId` filtering logic, enables unit testing of business logic separate from SQL, simplifies migration to ORM later if needed

- **Multi-Tenant Data Isolation:** All tables include `dealership_id` foreign key; all queries filter by dealership context - _Rationale:_ Shared database with row-level filtering is simplest multi-tenancy model for small scale; avoids schema-per-tenant or database-per-tenant complexity

- **Client-Side Routing (React Router):** React Router handles `/`, `/inventory`, `/admin/*` routes - _Rationale:_ SPA navigation without page reloads; backend serves `index.html` for all non-API routes (catch-all)

- **Stateless API:** No server-side sessions; admin auth uses simple JWT or session cookie - _Rationale:_ Enables horizontal scaling later (though not needed for MVP); simplifies backend logic

- **Cloudinary Direct Upload:** Frontend uploads images directly to Cloudinary (signed upload widget) - _Rationale:_ Offloads image processing from backend; reduces backend bandwidth; Cloudinary widget handles progress, retries, validation

---

## 3. Tech Stack

**This is the DEFINITIVE technology selection for the entire project. All development must use these exact versions.**

### Technology Stack Table

| Category | Technology | Version | Purpose | Rationale |
|----------|-----------|---------|---------|-----------|
| **Frontend Language** | JavaScript | ES2022 | React development | No TypeScript - saves setup time, type complexity; plain JS sufficient for 2-day MVP; add TS in Phase 2 if needed |
| **Frontend Framework** | React | 18.2+ | UI rendering, SPA | Industry standard, huge documentation, fast to iterate, CRA/Vite scaffolding gets you running in 5 minutes |
| **UI Component Library** | None | - | N/A | No component library (Material-UI, Ant Design) - they're too heavyweight; build custom components with Tailwind |
| **State Management** | React Context + useState | Built-in | Global state (auth, dealership selector) | Built-in, zero dependencies, sufficient for small app; avoid Redux/Zustand overhead |
| **CSS Framework** | Tailwind CSS | 3.4+ | Rapid styling | Utility-first = fast iteration, no context switching to CSS files, well-documented, zero runtime JS; alternative: plain CSS modules (viable but slower) |
| **Backend Language** | Node.js | 18 LTS | Server runtime | Long-term support, Railway/Render default, stable, widely documented |
| **Backend Framework** | Express | 4.18+ | REST API server | Minimal, unopinionated, battle-tested, dead simple to deploy; no NestJS/Fastify complexity |
| **API Style** | REST | - | Client-server communication | JSON over HTTP, testable with curl/Postman, universally understood; no GraphQL/tRPC learning curve |
| **Database** | PostgreSQL | 14+ | Relational data storage | Foreign keys enforce multi-tenancy integrity, ACID guarantees, free tier on Railway/Render, SQL is standard |
| **Database Client** | pg (node-postgres) | 8.11+ | PostgreSQL driver | Official Postgres client, parameterized queries (SQL injection protection), no ORM overhead (Prisma/Sequelize adds complexity) |
| **Cache** | None | - | N/A | Not needed for MVP; small data volumes, free tier DB is fast enough; defer Redis to Phase 2 |
| **File Storage** | Cloudinary | Free tier | Image uploads, CDN | Upload widget = no backend file handling, auto-optimization (WebP, resize), 25GB free, CDN included |
| **Authentication** | express-session + cookie | express-session 1.17+ | Admin login | Simple session-based auth, sufficient for prototype; no JWT complexity; upgrade to Passport.js/bcrypt in Phase 2 |
| **Frontend Testing** | None (manual) | - | N/A | Manual testing only for 2-day MVP; Jest + React Testing Library deferred to Phase 2 |
| **Backend Testing** | None (manual) | - | N/A | Manual Postman/curl testing; Jest/Supertest deferred to Phase 2 |
| **E2E Testing** | None (manual) | - | N/A | Manual browser testing; Playwright/Cypress deferred to Phase 2 |
| **Build Tool** | Vite | 5.0+ | Frontend build/dev server | Faster than CRA, minimal config, HMR out of box; alternative: CRA (more stable but slower) |
| **Package Manager** | npm | 9+ | Dependency management | Built-in with Node, workspaces support for monorepo, universally documented |
| **IaC Tool** | None | - | N/A | Railway/Render use platform UI for config; no Terraform/Pulumi needed for free tier single-service deployment |
| **CI/CD** | Railway/Render Auto-Deploy | Platform feature | Git-based deployment | Push to main = auto-deploy; zero config needed; GitHub Actions deferred to Phase 2 |
| **Monitoring** | Railway Logs | Platform feature | Error visibility | Built-in log viewer; Sentry/Datadog deferred to Phase 2 |
| **Logging** | console.log + Morgan | morgan 1.10+ | HTTP request logging | Morgan logs all requests (method, path, status, time); console.log for errors; structured logging (Winston) deferred |
| **Routing** | React Router | 6.20+ | Client-side navigation | SPA routing, declarative, nested routes for admin panel, excellent docs |
| **HTTP Client** | fetch API | Built-in | API calls from React | Native browser API, zero dependencies, adequate for simple GET/POST/PUT/DELETE; axios unnecessary |
| **Forms** | React Hook Form | 7.49+ | Form state management | Reduces boilerplate for 10+ field vehicle form, built-in validation, uncontrolled inputs (better performance); faster than manual controlled components |
| **Environment Config** | dotenv | 16.3+ | Local .env file parsing | Standard for local dev; Railway/Render use platform env vars in production |
| **Dev Server** | Nodemon | 3.0+ | Backend hot reload | Auto-restart on file changes; essential for rapid iteration |
| **CORS** | cors (Express middleware) | 2.8+ | Dev mode API access | Allow React dev server (localhost:3000) to call Express (localhost:5000); not needed in production (same origin) |

### Key Technology Decisions

**JavaScript over TypeScript:**
TypeScript adds 2-4 hours setup (tsconfig, type definitions, build pipeline). Plain JS with JSDoc comments for critical functions is faster. Add TS in Phase 2 when codebase stabilizes.

**Tailwind over CSS Modules:**
Tailwind's utility-first approach eliminates CSS file creation and naming decisions. `className="flex items-center gap-4"` is faster than creating `.container { display: flex; ... }` and importing stylesheets. Tradeoff: class clutter in JSX (acceptable for 2-day sprint).

**React Hook Form over Controlled Components:**
The vehicle create/edit form has 10+ fields (make, model, year, price, mileage, condition, status, title, description, images). Manual controlled components require `useState` + `onChange` for each field = 20+ lines of boilerplate. RHF reduces this to `register("make")` = 1 line per field. Saves ~2 hours over 48-hour timeline.

**pg over Prisma/Sequelize:**
Raw SQL with parameterized queries (`db.query('SELECT * FROM vehicles WHERE dealership_id = $1', [id])`) is explicit and debuggable. Prisma requires schema definition, migration setup, and code generation. Sequelize has learning curve for associations. Raw SQL is faster to write when you know SQL.

**Express Session over JWT:**
Session cookies are simpler than JWT (no token expiry logic, refresh tokens, local storage security concerns). For single-server deployment, session store in memory is fine (stateful but acceptable for prototype).

**Vite over Create React App:**
Vite's dev server starts in <1 second vs CRA's 10-30 seconds. HMR is instant. Build times are faster. Minimal config (`npm create vite@latest`). CRA is deprecated in favor of Next.js, but Next.js is overkill for SPA.

**No ORM, No GraphQL, No Microservices:**
Every abstraction layer adds time. Direct SQL, REST, and monolith are the fastest path to working software.

### Dependencies Installation Commands

**Backend (`backend/package.json`):**
```bash
npm install express pg dotenv cloudinary morgan cors express-session cookie-parser
npm install --save-dev nodemon
```

**Frontend (`frontend/package.json`):**
```bash
npm install react react-dom react-router-dom react-hook-form
npm install --save-dev vite @vitejs/plugin-react tailwindcss postcss autoprefixer
```

**Root (`package.json`):**
```bash
npm install --save-dev concurrently
```

---

## 4. Data Models

Three core entities support all dealership website and CMS functionality: **Dealership** (tenant configuration), **Vehicle** (inventory), and **Lead** (customer enquiries). All tables enforce multi-tenancy via `dealership_id` foreign keys.

### Dealership

**Purpose:** Stores dealership profile information displayed on public website (name, logo, contact info) and editable via admin CMS. Each dealership is an independent tenant.

**Key Attributes:**
- `id`: Serial primary key - unique dealership identifier
- `name`: VARCHAR(255) NOT NULL - dealership business name (e.g., "Acme Auto Sales")
- `logo_url`: TEXT - Cloudinary URL for logo image (nullable - not required for MVP)
- `theme_color`: VARCHAR(7) DEFAULT '#3B82F6' - hex color code for dealership theme (e.g., #3B82F6)
- `address`: TEXT NOT NULL - full street address for contact page
- `phone`: VARCHAR(20) NOT NULL - contact phone number
- `email`: VARCHAR(255) NOT NULL - contact email address
- `hours`: TEXT - business hours (e.g., "Mon-Fri 9am-6pm, Sat 10am-4pm")
- `about`: TEXT - about us description for public website
- `created_at`: TIMESTAMP DEFAULT NOW() - record creation timestamp

#### TypeScript Interface (documentation only - implement in plain JavaScript)

```typescript
interface Dealership {
  id: number;
  name: string;
  logo_url: string | null;
  theme_color: string; // Hex color code (#RRGGBB)
  address: string;
  phone: string;
  email: string;
  hours: string | null;
  about: string | null;
  created_at: Date;
}
```

#### PostgreSQL Table Definition

```sql
CREATE TABLE dealership (
  id SERIAL PRIMARY KEY,
  name VARCHAR(255) NOT NULL,
  logo_url TEXT,
  theme_color VARCHAR(7) DEFAULT '#3B82F6',
  address TEXT NOT NULL,
  phone VARCHAR(20) NOT NULL,
  email VARCHAR(255) NOT NULL,
  hours TEXT,
  about TEXT,
  created_at TIMESTAMP DEFAULT NOW()
);

-- Seed data for 2 demo dealerships
INSERT INTO dealership (name, address, phone, email, hours, about) VALUES
  ('Acme Auto Sales', '123 Main St, Springfield, IL 62701', '(555) 123-4567', 'sales@acmeauto.com', 'Mon-Fri 9am-6pm, Sat 10am-4pm, Sun Closed', 'Family-owned dealership serving Springfield for 20 years. Quality used cars at affordable prices.'),
  ('Premier Motors', '456 Oak Ave, Springfield, IL 62702', '(555) 987-6543', 'info@premiermotors.com', 'Mon-Sat 9am-7pm, Sun 11am-5pm', 'Premier Motors offers a wide selection of certified pre-owned vehicles with extended warranties.');
```

#### Relationships
- **One-to-Many with Vehicle:** `dealership.id → vehicle.dealership_id`
- **One-to-Many with Lead:** `dealership.id → lead.dealership_id`

### Vehicle

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

#### TypeScript Interface (documentation only)

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

#### PostgreSQL Table Definition

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

-- Index for status filtering (public site shows only active/pending)
CREATE INDEX idx_vehicle_status ON vehicle(status);

-- Sample seed data (run for each dealership_id)
INSERT INTO vehicle (dealership_id, make, model, year, price, mileage, condition, status, title, description, images) VALUES
  (1, 'Toyota', 'Camry', 2015, 15999.99, 75000, 'used', 'active', '2015 Toyota Camry SE', 'Well-maintained sedan with excellent fuel economy. Clean title, one owner.', '["https://res.cloudinary.com/demo/image/upload/sample.jpg"]'),
  (1, 'Honda', 'Civic', 2018, 18500.00, 45000, 'used', 'active', '2018 Honda Civic LX', 'Low mileage, excellent condition. Bluetooth, backup camera, automatic transmission.', '[]');
```

#### Relationships
- **Many-to-One with Dealership:** `vehicle.dealership_id → dealership.id` (required, cascade delete)
- **One-to-Many with Lead:** `vehicle.id → lead.vehicle_id` (optional reference)

### Lead

**Purpose:** Captures customer enquiries submitted via public website enquiry forms. Displayed in admin CMS lead inbox for dealership follow-up.

**Display Behavior (LeadInbox Component):**
- Messages truncated to 100 characters initially with "..." indicator
- "Show more" link appears for messages > 100 characters
- Click expands to show full message, changes to "Show less"
- Only one message expanded at a time for cleaner layout
- HTML entities properly decoded for display (XSS prevention)
- Updated: 2025-12-10 - Enhanced message display with show more/less toggle

**Key Attributes:**
- `id`: Serial primary key - unique lead identifier
- `dealership_id`: INTEGER NOT NULL FK - which dealership received this lead
- `vehicle_id`: INTEGER FK - related vehicle (nullable - general enquiries don't reference specific vehicle)
- `name`: VARCHAR(255) NOT NULL - customer name
- `email`: VARCHAR(255) NOT NULL - customer email
- `phone`: VARCHAR(20) NOT NULL - customer phone number
- `message`: TEXT NOT NULL - customer enquiry message
- `created_at`: TIMESTAMP DEFAULT NOW() - submission timestamp (used for sorting)

#### TypeScript Interface (documentation only)

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

#### PostgreSQL Table Definition

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

#### Relationships
- **Many-to-One with Dealership:** `lead.dealership_id → dealership.id` (required, cascade delete)
- **Many-to-One with Vehicle:** `lead.vehicle_id → vehicle.id` (optional, set null on vehicle delete)

### Entity Relationship Diagram

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

### Multi-Tenancy Data Isolation

**Critical Implementation Rule:** Every API query MUST filter by `dealership_id`:

```javascript
// ✅ CORRECT - filters by dealership
const vehicles = await db.query(
  'SELECT * FROM vehicle WHERE dealership_id = $1 AND status = $2',
  [dealershipId, 'active']
);

// ❌ WRONG - no dealership filter (data leak!)
const vehicles = await db.query('SELECT * FROM vehicle WHERE status = $1', ['active']);
```

**Database Constraints Enforce Integrity:**
- Foreign key constraints prevent orphaned records (e.g., can't create vehicle with non-existent `dealership_id`)
- `ON DELETE CASCADE` ensures deleting dealership removes all vehicles/leads
- `ON DELETE SET NULL` for `lead.vehicle_id` preserves lead if vehicle deleted (customer enquiry history retained)
- `CHECK` constraints on `condition` and `status` prevent invalid values

### Implementation Notes

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

## 5. API Specification

REST API with 13 endpoints covering all public website and admin CMS functionality. All endpoints return JSON. Multi-tenancy enforced via `dealership_id` query parameter or path parameter.

### Base URL

- **Development:** `http://localhost:5000/api`
- **Production:** `https://your-app.up.railway.app/api`

### API Endpoints Overview

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
| GET | `/vehicles` | No | List vehicles (with filters) |
| GET | `/vehicles/:id` | No | Get single vehicle |
| POST | `/vehicles` | Yes | Create vehicle |
| PUT | `/vehicles/:id` | Yes | Update vehicle |
| DELETE | `/vehicles/:id` | Yes | Delete vehicle |
| **Leads** |
| GET | `/leads` | Yes | List leads (admin) |
| POST | `/leads` | No | Submit enquiry (public) |
| **Uploads** |
| POST | `/upload` | Yes | Upload image to Cloudinary |

### Authentication Endpoints

#### POST /api/auth/login

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

#### POST /api/auth/logout

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

#### GET /api/auth/me

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

### Dealership Endpoints

#### GET /api/dealers

List all dealerships (used for admin dealership selector dropdown).

**Query Parameters:** None

**Success Response (200):**
```json
[
  {
    "id": 1,
    "name": "Acme Auto Sales",
    "logo_url": "https://res.cloudinary.com/...",
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

#### GET /api/dealers/:id

Get single dealership details (for public About page, admin settings form).

**Success Response (200):**
```json
{
  "id": 1,
  "name": "Acme Auto Sales",
  "logo_url": "https://res.cloudinary.com/...",
  "address": "123 Main St, Springfield, IL 62701",
  "phone": "(555) 123-4567",
  "email": "sales@acmeauto.com",
  "hours": "Mon-Fri 9am-6pm, Sat 10am-4pm, Sun Closed",
  "about": "Family-owned dealership serving Springfield for 20 years.",
  "created_at": "2025-11-19T10:00:00.000Z"
}
```

**Error Response (404):**
```json
{
  "error": "Dealership not found"
}
```

#### PUT /api/dealers/:id (Auth Required)

Update dealership settings (admin CMS).

**Request Body:**
```json
{
  "name": "Acme Auto Sales",
  "logo_url": "https://res.cloudinary.com/...",
  "address": "123 Main St, Springfield, IL 62701",
  "phone": "(555) 123-4567",
  "email": "sales@acmeauto.com",
  "hours": "Mon-Fri 9am-6pm, Sat 10am-4pm",
  "about": "Updated about text..."
}
```

**Success Response (200):**
```json
{
  "id": 1,
  "name": "Acme Auto Sales",
  ...
}
```

**Validation Error (400):**
```json
{
  "error": "Missing required fields: name, address, phone, email"
}
```

### Vehicle Endpoints

#### GET /api/vehicles

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

#### GET /api/vehicles/:id

Get single vehicle details (public vehicle detail page, admin edit form).

**Success Response (200):** (Same as vehicle object above)

**Error Response (404):**
```json
{
  "error": "Vehicle not found"
}
```

#### POST /api/vehicles (Auth Required)

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

#### PUT /api/vehicles/:id (Auth Required)

Update existing vehicle (admin CMS).

**Request Body:** (Same as POST, all fields optional)

**Success Response (200):** (Returns updated vehicle)

#### DELETE /api/vehicles/:id (Auth Required)

Delete vehicle (admin CMS).

**Success Response (204):** (No content)

**Error Response (404):**
```json
{
  "error": "Vehicle not found"
}
```

### Lead Endpoints

#### GET /api/leads (Auth Required)

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
    "created_at": "2025-11-19T14:20:00.000Z"
  },
  ...
]
```

#### POST /api/leads

Submit customer enquiry (public vehicle detail page enquiry form).

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

**Success Response (201):** (Returns created lead with ID)

**Validation Error (400):**
```json
{
  "error": "Missing required fields: dealership_id, name, email, phone, message"
}
```

### Upload Endpoint

#### POST /api/upload (Auth Required)

Upload image to Cloudinary (fallback if upload widget doesn't work).

**Request:** `multipart/form-data` with file field

**Success Response (200):**
```json
{
  "url": "https://res.cloudinary.com/your-cloud/image/upload/v123456789/sample.jpg"
}
```

**Error Response (400):**
```json
{
  "error": "File too large (max 5MB)"
}
```

### Auth Middleware

Protect admin endpoints with simple session check:

```javascript
function requireAuth(req, res, next) {
  if (!req.session.isAuthenticated) {
    return res.status(401).json({ error: 'Authentication required' });
  }
  next();
}
```

### Error Handling

Wrap all async route handlers with error catcher or use try-catch as shown. Return consistent error format:
```json
{
  "error": "Descriptive error message"
}
```

### React Frontend API Calls

Example using `fetch`:

```javascript
// Get vehicles for public site
const response = await fetch(`/api/vehicles?dealershipId=${dealershipId}&status=active`);
const vehicles = await response.json();

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

---

## 6. Components

Flat, domain-organized component structure for rapid 2-day implementation. Backend uses simple route handlers + database modules. Frontend uses page-level components for routes + minimal shared components.

### Backend Components

#### File Structure

```
backend/
├── server.js                 # Express app setup, middleware, route mounting
├── routes/
│   ├── auth.js              # POST /auth/login, /auth/logout, GET /auth/me
│   ├── dealers.js           # GET/PUT /dealers
│   ├── vehicles.js          # GET/POST/PUT/DELETE /vehicles
│   ├── leads.js             # GET/POST /leads
│   └── upload.js            # POST /upload (Cloudinary)
├── db/
│   ├── index.js             # PostgreSQL connection pool setup
│   ├── dealers.js           # Dealership queries (getAll, getById, update)
│   ├── vehicles.js          # Vehicle queries (getAll, getById, create, update, delete)
│   └── leads.js             # Lead queries (getByDealership, create)
├── middleware/
│   └── auth.js              # requireAuth middleware
└── package.json
```

#### server.js

**Responsibility:** Main Express app setup, middleware configuration, route mounting, static file serving.

**Key Functions:**
- Initialize Express app
- Configure middleware (JSON parser, CORS, session, Morgan logging)
- Mount API routes (`/api/auth`, `/api/dealers`, `/api/vehicles`, `/api/leads`, `/api/upload`)
- Serve React build in production (`express.static('frontend/dist')`)
- Catch-all route for React Router (return `index.html` for non-API routes)
- Start server on port 5000

**Implementation Sketch:**
```javascript
const express = require('express');
const session = require('express-session');
const cors = require('cors');
const morgan = require('morgan');
const path = require('path');

const app = express();

// Middleware
app.use(express.json());
app.use(cors({ origin: 'http://localhost:3000', credentials: true })); // Dev only
app.use(morgan('dev'));
app.use(session({
  secret: process.env.SESSION_SECRET,
  resave: false,
  saveUninitialized: false,
  cookie: { secure: false } // Set true in production with HTTPS
}));

// API Routes
app.use('/api/auth', require('./routes/auth'));
app.use('/api/dealers', require('./routes/dealers'));
app.use('/api/vehicles', require('./routes/vehicles'));
app.use('/api/leads', require('./routes/leads'));
app.use('/api/upload', require('./routes/upload'));

// Serve React app in production
if (process.env.NODE_ENV === 'production') {
  app.use(express.static(path.join(__dirname, '../frontend/dist')));
  app.get('*', (req, res) => {
    res.sendFile(path.join(__dirname, '../frontend/dist', 'index.html'));
  });
}

const PORT = process.env.PORT || 5000;
app.listen(PORT, () => console.log(`Server running on port ${PORT}`));
```

#### routes/*.js

**Responsibility:** Handle HTTP requests for specific domain (auth, dealers, vehicles, leads).

**Dependencies:** Database modules (`db/*.js`), auth middleware

**Pattern:** Express Router with route handlers that call database functions.

#### db/*.js

**Responsibility:** Encapsulate all database queries for a domain.

**Functions:** getAll, getById, create, update, delete (as needed)

**Pattern:** Async functions that use parameterized queries with pg pool.

#### middleware/auth.js

**Responsibility:** Protect admin routes.

**Function:**
```javascript
function requireAuth(req, res, next) {
  if (!req.session.isAuthenticated) {
    return res.status(401).json({ error: 'Authentication required' });
  }
  next();
}

module.exports = { requireAuth };
```

### Frontend Components

#### File Structure

```
frontend/src/
├── main.jsx                       # App entry point, React Router setup
├── App.jsx                        # Root component with routes
├── index.css                      # Tailwind directives + global styles
├── pages/
│   ├── public/
│   │   ├── Home.jsx               # Public home page
│   │   ├── Inventory.jsx          # Vehicle listing page with search/filter
│   │   ├── VehicleDetail.jsx      # Single vehicle detail + enquiry form
│   │   └── About.jsx              # Dealership about/contact page
│   └── admin/
│       ├── Login.jsx              # Admin login page
│       ├── Dashboard.jsx          # Admin dashboard with dealership selector
│       ├── VehicleList.jsx        # Vehicle manager (list + edit/delete)
│       ├── VehicleForm.jsx        # Create/edit vehicle form
│       ├── DealerSettings.jsx     # Dealership settings form
│       └── LeadInbox.jsx          # Customer enquiries list
├── components/
│   ├── Layout.jsx                 # Shared layout wrapper (header, footer, children)
│   ├── Header.jsx                 # Public site header with nav
│   ├── AdminHeader.jsx            # Admin header with dealership selector + logout
│   ├── VehicleCard.jsx            # Vehicle grid item (thumbnail, title, price)
│   ├── SearchWidget.jsx           # Homepage vehicle search form with filters
│   ├── EnquiryForm.jsx            # Reusable enquiry form component
│   ├── GeneralEnquiryForm.jsx     # Homepage general enquiry form (no vehicle)
│   ├── ImageGallery.jsx           # Vehicle detail image gallery
│   └── ProtectedRoute.jsx         # Auth wrapper for admin routes
├── context/
│   └── AdminContext.jsx           # Global state for admin (selected dealership, auth status)
└── utils/
    └── api.js                     # Fetch wrappers for API calls (optional)
```

#### Public Pages

**pages/public/Home.jsx**
- Fetch dealership info from `/api/dealers/:id`
- Display hero section with name, logo, tagline
- "Browse Inventory" CTA → link to `/inventory`
- Responsive grid layout with SearchWidget and GeneralEnquiryForm side-by-side
- Grid: `lg:grid-cols-2` (desktop) / `grid-cols-1` (mobile)

**pages/public/Inventory.jsx**
- Fetch vehicles from `/api/vehicles?dealershipId=X&status=active`
- Display grid of VehicleCard components
- Client-side search, filter, sort

**pages/public/VehicleDetail.jsx**
- Fetch vehicle from `/api/vehicles/:id`
- Display ImageGallery, specs, description
- Render EnquiryForm component

**pages/public/About.jsx**
- Fetch dealership from `/api/dealers/:id`
- Display contact info, hours, about text

#### Admin Pages

**pages/admin/Login.jsx**
- Username/password form
- POST to `/api/auth/login`
- On success: update AdminContext, redirect to `/admin`

**pages/admin/Dashboard.jsx**
- Fetch dealerships (`/api/dealers`)
- Dealership selector dropdown (updates AdminContext)
- Display stats: vehicle count, lead count
- Navigation to other admin pages

**pages/admin/VehicleList.jsx**
- Fetch vehicles (`/api/vehicles?dealershipId=X`)
- Table with edit/delete actions
- "Add Vehicle" button → navigate to `/admin/vehicles/new`

**pages/admin/VehicleForm.jsx**
- React Hook Form for all fields
- Cloudinary upload widget for images
- Submit → POST or PUT `/api/vehicles`

**pages/admin/DealerSettings.jsx**
- Fetch and edit dealership settings
- Cloudinary upload for logo
- Submit → PUT `/api/dealers/:id`

**pages/admin/LeadInbox.jsx**
- Fetch leads (`/api/leads?dealershipId=X`)
- Display table with contact action buttons

#### Shared Components

**components/Layout.jsx**
- Wrapper with Header and footer

**components/Header.jsx**
- Public site navigation

**components/AdminHeader.jsx**
- Dealership selector + nav + logout

**components/VehicleCard.jsx**
- Props: vehicle object
- Displays thumbnail, title, price, condition badge

**components/SearchWidget.jsx**
- Props: None
- Homepage vehicle search form with advanced filters
- Filter inputs: brand dropdown, min/max year, min/max price
- Client-side validation: year range, price range
- Submit → Navigate to `/inventory` with URL query parameters
- Styled for side-by-side grid layout (h-full for equal height)

**components/EnquiryForm.jsx**
- Props: vehicleId, dealershipId
- Form with name, email, phone, message
- Submit → POST `/api/leads`

**components/GeneralEnquiryForm.jsx**
- Props: None (uses DealershipContext for dealershipId)
- General enquiry form for homepage (no specific vehicle)
- Form fields: name, email, phone, message (all required)
- Client-side validation:
  - Email format validation (regex)
  - Phone validation (minimum 10 digits)
  - Message character limit (5000 chars with counter)
- Real-time error clearing on input
- Success/error message display
- Form reset after successful submission
- Submit → POST `/api/leads` with vehicle_id omitted (stored as NULL)
- Styled for side-by-side grid layout with SearchWidget

**components/ImageGallery.jsx**
- Props: images array
- Main image + thumbnail navigation

**components/ProtectedRoute.jsx**
- Checks AdminContext.isAuthenticated
- Redirects to `/admin/login` if not authenticated

#### context/AdminContext.jsx

**Provides:**
- `isAuthenticated` (boolean)
- `setIsAuthenticated` (function)
- `selectedDealership` (object or null)
- `setSelectedDealership` (function)

**Pattern:** React Context API with useState and useEffect hooks.

---

## 7. External APIs - Cloudinary Integration

Single external dependency: **Cloudinary** for image uploads, storage, and CDN delivery. Use unsigned upload preset for fastest setup (no server-side signature generation). Frontend upload widget handles all uploads directly to Cloudinary. Backend only stores returned URLs in PostgreSQL.

### Cloudinary Account Setup (5 minutes)

**Step 1: Create Free Account**

1. Go to https://cloudinary.com/users/register/free
2. Sign up with email (no credit card required)
3. Note your **Cloud Name** from dashboard (e.g., `dxyz123abc`)

**Step 2: Create Upload Preset**

1. Navigate to **Settings → Upload** in Cloudinary dashboard
2. Scroll to **Upload presets**
3. Click **Add upload preset**
4. Configure:
   - **Preset name:** `vehicle-images`
   - **Signing mode:** **Unsigned** (simplest - no backend signature required)
   - **Folder:** `dealership-vehicles` (optional - organizes uploads)
   - **Format:** Leave as Auto (Cloudinary auto-converts to WebP with fallback)
   - **Transformation:** Add optional transformation:
     - **Quality:** Auto (Cloudinary optimizes automatically)
     - **Max width:** 1920 (prevents huge uploads)
5. Click **Save**
6. Note the **Preset name**: `vehicle-images`

**Result:** You now have an unsigned upload preset that allows direct browser uploads without server involvement.

### Environment Variables

**Frontend (.env or Vite config)**

```bash
VITE_CLOUDINARY_CLOUD_NAME=your-cloud-name
VITE_CLOUDINARY_UPLOAD_PRESET=vehicle-images
```

**Note:** Vite requires `VITE_` prefix for env vars accessible in browser. In production, set these on Railway/Render.

**Backend (.env) - Optional for fallback upload endpoint**

```bash
CLOUDINARY_CLOUD_NAME=your-cloud-name
CLOUDINARY_API_KEY=123456789012345
CLOUDINARY_API_SECRET=abcdefghijklmnopqrstuvwxyz
```

**When needed:** Only if implementing `/api/upload` fallback endpoint.

### Frontend Integration - Upload Widget

**Add Cloudinary Widget Script to index.html**

Add before closing `</body>` tag in `frontend/index.html`:

```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Dealership Platform</title>
  </head>
  <body>
    <div id="root"></div>
    <script type="module" src="/src/main.jsx"></script>

    <!-- Cloudinary Upload Widget -->
    <script src="https://upload-widget.cloudinary.com/global/all.js" type="text/javascript"></script>
  </body>
</html>
```

**Why:** Loads Cloudinary's upload widget globally. Widget is ~100KB, loaded once per session.

### Usage in VehicleForm.jsx (Multiple Images)

**Scenario:** Admin uploads 1-10 vehicle photos for a listing.

**Implementation:**

```javascript
import { useState } from 'react';

function VehicleForm() {
  const [images, setImages] = useState([]); // Array of Cloudinary URLs

  const handleUploadClick = () => {
    // Open Cloudinary upload widget
    window.cloudinary.openUploadWidget(
      {
        cloudName: import.meta.env.VITE_CLOUDINARY_CLOUD_NAME,
        uploadPreset: import.meta.env.VITE_CLOUDINARY_UPLOAD_PRESET,
        sources: ['local', 'camera'], // Allow file upload and camera (mobile)
        multiple: true, // Allow selecting multiple files
        maxFiles: 10, // Limit to 10 images per vehicle
        maxFileSize: 5000000, // 5MB max per file
        clientAllowedFormats: ['jpg', 'png', 'jpeg', 'webp'],
        cropping: false, // Disable cropping UI (not needed for MVP)
        folder: 'dealership-vehicles', // Optional: organize in Cloudinary
        resourceType: 'image',
      },
      (error, result) => {
        if (error) {
          console.error('Upload error:', error);
          alert('Upload failed. Please try again.');
          return;
        }

        if (result.event === 'success') {
          // Add uploaded image URL to array
          const newImageUrl = result.info.secure_url;
          setImages((prev) => [...prev, newImageUrl]);
        }

        // Widget auto-closes after upload or user closes it
      }
    );
  };

  const handleRemoveImage = (indexToRemove) => {
    setImages((prev) => prev.filter((_, index) => index !== indexToRemove));
  };

  return (
    <div className="vehicle-form">
      <h2>Add Vehicle</h2>

      {/* Other form fields (make, model, year, etc.) */}

      {/* Image Upload Section */}
      <div className="mb-4">
        <label className="block font-semibold mb-2">Vehicle Photos</label>

        <button
          type="button"
          onClick={handleUploadClick}
          className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
        >
          Upload Photos
        </button>

        {/* Display uploaded images */}
        <div className="grid grid-cols-4 gap-4 mt-4">
          {images.map((url, index) => (
            <div key={index} className="relative">
              <img
                src={url}
                alt={`Vehicle ${index + 1}`}
                className="w-full h-32 object-cover rounded"
              />
              <button
                type="button"
                onClick={() => handleRemoveImage(index)}
                className="absolute top-1 right-1 bg-red-500 text-white rounded-full w-6 h-6 text-xs"
              >
                ✕
              </button>
            </div>
          ))}
        </div>

        <p className="text-sm text-gray-600 mt-2">
          {images.length} / 10 photos uploaded
        </p>
      </div>

      {/* Form submit */}
      <button
        type="submit"
        onClick={handleSubmit}
        className="bg-green-500 text-white px-6 py-2 rounded"
      >
        Save Vehicle
      </button>
    </div>
  );

  async function handleSubmit(e) {
    e.preventDefault();

    // Collect form data (use React Hook Form in real implementation)
    const vehicleData = {
      dealership_id: 1, // From context
      make: 'Toyota',
      model: 'Camry',
      year: 2015,
      price: 15999.99,
      mileage: 75000,
      condition: 'used',
      status: 'active',
      title: '2015 Toyota Camry SE',
      description: 'Well-maintained sedan...',
      images: images, // Array of Cloudinary URLs
    };

    // POST to API
    const response = await fetch('/api/vehicles', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(vehicleData),
    });

    if (response.ok) {
      alert('Vehicle saved!');
      // Navigate back to vehicle list
    }
  }
}
```

**What Gets Stored in Database:**

```json
{
  "images": [
    "https://res.cloudinary.com/dxyz123abc/image/upload/v1732012345/dealership-vehicles/abc123.jpg",
    "https://res.cloudinary.com/dxyz123abc/image/upload/v1732012346/dealership-vehicles/def456.jpg",
    "https://res.cloudinary.com/dxyz123abc/image/upload/v1732012347/dealership-vehicles/ghi789.jpg"
  ]
}
```

Stored as **JSONB array** in PostgreSQL `vehicle.images` column.

### Usage in DealerSettings.jsx (Single Logo)

**Scenario:** Admin uploads dealership logo.

**Implementation:**

```javascript
import { useState } from 'react';

function DealerSettings() {
  const [logoUrl, setLogoUrl] = useState(''); // Single Cloudinary URL

  const handleLogoUpload = () => {
    window.cloudinary.openUploadWidget(
      {
        cloudName: import.meta.env.VITE_CLOUDINARY_CLOUD_NAME,
        uploadPreset: import.meta.env.VITE_CLOUDINARY_UPLOAD_PRESET,
        sources: ['local'],
        multiple: false, // Only one logo
        maxFileSize: 2000000, // 2MB max for logos
        clientAllowedFormats: ['jpg', 'png', 'svg'],
        cropping: true, // Enable cropping for logos (square crop recommended)
        croppingAspectRatio: 1, // Force square crop
        folder: 'dealership-logos',
      },
      (error, result) => {
        if (error) {
          console.error('Logo upload error:', error);
          return;
        }

        if (result.event === 'success') {
          setLogoUrl(result.info.secure_url);
        }
      }
    );
  };

  const handleRemoveLogo = () => {
    setLogoUrl('');
  };

  return (
    <div className="dealer-settings">
      <h2>Dealership Settings</h2>

      {/* Logo Upload */}
      <div className="mb-4">
        <label className="block font-semibold mb-2">Logo</label>

        {logoUrl ? (
          <div className="flex items-center gap-4">
            <img src={logoUrl} alt="Logo" className="w-24 h-24 object-contain" />
            <button
              type="button"
              onClick={handleRemoveLogo}
              className="text-red-500 underline"
            >
              Remove Logo
            </button>
          </div>
        ) : (
          <button
            type="button"
            onClick={handleLogoUpload}
            className="bg-blue-500 text-white px-4 py-2 rounded"
          >
            Upload Logo
          </button>
        )}
      </div>

      {/* Other dealership fields (name, address, etc.) */}

      <button
        type="submit"
        onClick={handleSave}
        className="bg-green-500 text-white px-6 py-2 rounded"
      >
        Save Settings
      </button>
    </div>
  );

  async function handleSave(e) {
    e.preventDefault();

    const dealershipData = {
      name: 'Acme Auto Sales',
      logo_url: logoUrl, // Cloudinary URL or empty string
      address: '123 Main St...',
      phone: '(555) 123-4567',
      email: 'sales@acmeauto.com',
      hours: 'Mon-Fri 9am-6pm',
      about: 'Family-owned dealership...',
    };

    const response = await fetch('/api/dealers/1', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(dealershipData),
    });

    if (response.ok) {
      alert('Settings saved!');
    }
  }
}
```

**What Gets Stored in Database:**

```json
{
  "logo_url": "https://res.cloudinary.com/dxyz123abc/image/upload/v1732012400/dealership-logos/logo_abc.png"
}
```

Stored as **TEXT** in PostgreSQL `dealership.logo_url` column.

### Displaying Images on Public Site

**Vehicle Listing - Thumbnails**

Use Cloudinary's URL transformation for automatic thumbnails:

```javascript
function VehicleCard({ vehicle }) {
  // Get first image or use placeholder
  const primaryImage = vehicle.images?.[0] || 'https://via.placeholder.com/400x300?text=No+Image';

  // Transform URL to thumbnail (400px width, auto quality, WebP)
  const thumbnailUrl = primaryImage.replace('/upload/', '/upload/w_400,f_auto,q_auto/');

  return (
    <div className="vehicle-card">
      <img src={thumbnailUrl} alt={vehicle.title} className="w-full h-48 object-cover" />
      <h3>{vehicle.title}</h3>
      <p>${vehicle.price.toLocaleString()}</p>
    </div>
  );
}
```

**URL Transformation Examples:**

| Original URL | Transformed URL | Effect |
|--------------|-----------------|--------|
| `.../upload/v123/image.jpg` | `.../upload/w_400,f_auto,q_auto/v123/image.jpg` | 400px width, auto format (WebP if supported), auto quality |
| `.../upload/v123/image.jpg` | `.../upload/w_1920,h_1080,c_limit/v123/image.jpg` | Max 1920x1080, maintain aspect ratio |
| `.../upload/v123/image.jpg` | `.../upload/c_thumb,g_auto,w_200,h_200/v123/image.jpg` | 200x200 square thumbnail, auto-crop to focus |

**For MVP, use simple transformations:**
- Thumbnails: `w_400,f_auto,q_auto`
- Gallery: `w_1920,f_auto,q_auto`
- Logo: `w_200,h_200,c_fit,f_auto`

**No Backend Code Required:** Transformations happen via URL syntax. Cloudinary CDN serves optimized images.

### Vehicle Detail - Image Gallery

```javascript
function ImageGallery({ images }) {
  const [selectedIndex, setSelectedIndex] = useState(0);

  if (!images || images.length === 0) {
    return <img src="https://via.placeholder.com/800x600?text=No+Images" alt="Placeholder" />;
  }

  const currentImage = images[selectedIndex];
  const galleryUrl = currentImage.replace('/upload/', '/upload/w_1920,f_auto,q_auto/');

  return (
    <div className="image-gallery">
      {/* Main Image */}
      <img
        src={galleryUrl}
        alt={`Vehicle image ${selectedIndex + 1}`}
        className="w-full h-96 object-contain bg-gray-100"
      />

      {/* Thumbnail Navigation */}
      <div className="flex gap-2 mt-4">
        {images.map((url, index) => {
          const thumbUrl = url.replace('/upload/', '/upload/w_200,h_150,c_fill,f_auto,q_auto/');
          return (
            <button
              key={index}
              onClick={() => setSelectedIndex(index)}
              className={`border-2 ${index === selectedIndex ? 'border-blue-500' : 'border-gray-300'}`}
            >
              <img src={thumbUrl} alt={`Thumbnail ${index + 1}`} className="w-20 h-16 object-cover" />
            </button>
          );
        })}
      </div>
    </div>
  );
}
```

### What URLs to Store in Database

**Always store the full Cloudinary URL:**

```
https://res.cloudinary.com/your-cloud-name/image/upload/v1732012345/dealership-vehicles/abc123.jpg
```

**Do NOT store:**
- Relative paths (`/dealership-vehicles/abc123.jpg`) ❌
- Transformed URLs (`...w_400.../abc123.jpg`) ❌

**Why:** Store original URL, apply transformations on-the-fly in frontend via URL manipulation. This allows changing transformation logic without migrating database.

**Database Storage:**
- `vehicle.images`: JSONB array of full URLs
- `dealership.logo_url`: TEXT field with single full URL (or NULL)

### Free Tier Limits

**Cloudinary Free Tier:**
- **Storage:** 25GB
- **Bandwidth:** 25GB/month
- **Transformations:** Unlimited

**Estimation for 2-5 Dealerships:**
- Average 10 vehicles per dealership with 5 photos each = 250 images
- Average 2MB per image = 500MB storage
- Monthly traffic (~1000 views, 3 images/view) = ~6GB bandwidth

**Result:** Well within free tier. Upgrade to paid tier ($89/month for 85GB bandwidth) only if traffic grows significantly.

### Testing Checklist

- [ ] Create Cloudinary account and upload preset
- [ ] Add script tag to `index.html`
- [ ] Test widget opens in VehicleForm (desktop Chrome)
- [ ] Upload 3-5 sample images, verify URLs returned
- [ ] Save vehicle with images, verify stored in database
- [ ] View vehicle detail page, verify images display
- [ ] Test logo upload in DealerSettings
- [ ] Test on mobile Safari (iOS) - widget should work
- [ ] Test image removal (delete from state, not from Cloudinary)
- [ ] Verify transformed URLs (thumbnails) load correctly

**Estimated Integration Time:** 1 hour (setup + testing)

### Quick Setup Summary

1. **Create Cloudinary account** → Note Cloud Name
2. **Create upload preset** `vehicle-images` (unsigned)
3. **Add env vars** to `.env`: `VITE_CLOUDINARY_CLOUD_NAME`, `VITE_CLOUDINARY_UPLOAD_PRESET`
4. **Add script tag** to `frontend/index.html`
5. **Copy-paste widget code** into VehicleForm and DealerSettings
6. **Store returned URLs** in database (images array, logo_url)
7. **Display with transformations** (add `w_400,f_auto,q_auto` to URL)

**No backend code needed for MVP.** Widget handles everything. Backend `/api/upload` only if issues arise.

---

## 8. Database Schema

Complete PostgreSQL schema with copy-pasteable SQL scripts. Run once on Railway to set up database.

### Complete Schema Script

**File:** `backend/db/schema.sql`

```sql
-- ============================================
-- JealPrototypeTest Database Schema
-- Multi-Dealership Car Website + CMS Platform
-- ============================================

-- Drop existing tables (for clean setup)
DROP TABLE IF EXISTS lead CASCADE;
DROP TABLE IF EXISTS vehicle CASCADE;
DROP TABLE IF EXISTS dealership CASCADE;

-- ============================================
-- Dealership Table
-- ============================================
CREATE TABLE dealership (
  id SERIAL PRIMARY KEY,
  name VARCHAR(255) NOT NULL,
  logo_url TEXT,
  address TEXT NOT NULL,
  phone VARCHAR(20) NOT NULL,
  email VARCHAR(255) NOT NULL,
  hours TEXT,
  about TEXT,
  created_at TIMESTAMP DEFAULT NOW()
);

-- ============================================
-- Vehicle Table
-- ============================================
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

-- ============================================
-- Lead Table
-- ============================================
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

-- ============================================
-- Indexes for Performance
-- ============================================

-- Multi-tenancy queries (most critical)
CREATE INDEX idx_vehicle_dealership_id ON vehicle(dealership_id);
CREATE INDEX idx_lead_dealership_id ON lead(dealership_id);

-- Status filtering (public site shows only active/pending)
CREATE INDEX idx_vehicle_status ON vehicle(status);

-- Lead sorting (admin inbox sorts by created_at DESC)
CREATE INDEX idx_lead_created_at ON lead(created_at DESC);

-- Composite index for common query: vehicles by dealership + status
CREATE INDEX idx_vehicle_dealership_status ON vehicle(dealership_id, status);
```

### Seed Data Script

**File:** `backend/db/seed.sql`

```sql
-- ============================================
-- Seed Data for 2 Demo Dealerships
-- ============================================

-- Insert 2 Dealerships
INSERT INTO dealership (name, address, phone, email, hours, about) VALUES
  (
    'Acme Auto Sales',
    '123 Main St, Springfield, IL 62701',
    '(555) 123-4567',
    'sales@acmeauto.com',
    'Mon-Fri: 9:00 AM - 6:00 PM
Sat: 10:00 AM - 4:00 PM
Sun: Closed',
    'Family-owned dealership serving Springfield for over 20 years. We specialize in quality used cars at affordable prices with transparent pricing and no-pressure sales. Come visit us today!'
  ),
  (
    'Premier Motors',
    '456 Oak Ave, Springfield, IL 62702',
    '(555) 987-6543',
    'info@premiermotors.com',
    'Mon-Sat: 9:00 AM - 7:00 PM
Sun: 11:00 AM - 5:00 PM',
    'Premier Motors offers a wide selection of certified pre-owned vehicles with extended warranties. Our experienced sales team is committed to helping you find the perfect vehicle for your needs and budget.'
  );

-- Insert Vehicles for Dealership 1 (Acme Auto Sales)
INSERT INTO vehicle (dealership_id, make, model, year, price, mileage, condition, status, title, description, images) VALUES
  (
    1,
    'Toyota',
    'Camry',
    2015,
    15999.99,
    75000,
    'used',
    'active',
    '2015 Toyota Camry SE',
    'Well-maintained sedan with excellent fuel economy. Features include backup camera, Bluetooth connectivity, and premium sound system. Clean title, one owner. Service records available.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  ),
  (
    1,
    'Honda',
    'Civic',
    2018,
    18500.00,
    45000,
    'used',
    'active',
    '2018 Honda Civic LX',
    'Low mileage, excellent condition. Automatic transmission, lane departure warning, adaptive cruise control. Non-smoker vehicle, garage kept. Still under manufacturer warranty.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  ),
  (
    1,
    'Ford',
    'F-150',
    2017,
    28999.00,
    68000,
    'used',
    'active',
    '2017 Ford F-150 XLT SuperCrew',
    'Powerful and reliable pickup truck. 4WD, towing package, bed liner. Perfect for work or weekend adventures. Recently serviced with new tires.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  ),
  (
    1,
    'Chevrolet',
    'Malibu',
    2016,
    13500.00,
    82000,
    'used',
    'pending',
    '2016 Chevrolet Malibu LT',
    'Spacious sedan with modern features. Touchscreen display, remote start, heated seats. Great commuter car with excellent safety ratings.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  ),
  (
    1,
    'Nissan',
    'Altima',
    2014,
    11999.00,
    95000,
    'used',
    'sold',
    '2014 Nissan Altima 2.5 S',
    'Affordable and reliable sedan. Bluetooth, backup camera, fuel-efficient 4-cylinder engine. Great first car or daily driver.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  );

-- Insert Vehicles for Dealership 2 (Premier Motors)
INSERT INTO vehicle (dealership_id, make, model, year, price, mileage, condition, status, title, description, images) VALUES
  (
    2,
    'BMW',
    '3 Series',
    2019,
    32500.00,
    28000,
    'used',
    'active',
    '2019 BMW 3 Series 330i',
    'Luxury sedan with premium features. Leather seats, navigation, sunroof, premium audio. Certified pre-owned with extended warranty. Meticulously maintained.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  ),
  (
    2,
    'Mercedes-Benz',
    'C-Class',
    2018,
    29999.00,
    35000,
    'used',
    'active',
    '2018 Mercedes-Benz C-Class C300',
    'Sophisticated and stylish. Turbocharged engine, advanced safety features, smartphone integration. Service history available. Like-new condition.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  ),
  (
    2,
    'Audi',
    'A4',
    2020,
    35999.00,
    22000,
    'used',
    'active',
    '2020 Audi A4 Premium',
    'Low mileage luxury sedan. All-wheel drive, virtual cockpit, premium sound system. Remaining factory warranty. Pristine condition inside and out.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  ),
  (
    2,
    'Lexus',
    'RX 350',
    2017,
    31500.00,
    48000,
    'used',
    'active',
    '2017 Lexus RX 350 Luxury',
    'Premium SUV with exceptional reliability. Heated and ventilated seats, panoramic sunroof, navigation. Perfect for families. No accidents.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  ),
  (
    2,
    'Acura',
    'TLX',
    2019,
    27500.00,
    32000,
    'used',
    'pending',
    '2019 Acura TLX Technology',
    'Sport luxury sedan with advanced tech. Adaptive suspension, collision mitigation, lane keeping assist. Certified pre-owned with comprehensive warranty.',
    '["https://res.cloudinary.com/demo/image/upload/v1/sample.jpg"]'::jsonb
  );

-- Insert Sample Leads for Dealership 1
INSERT INTO lead (dealership_id, vehicle_id, name, email, phone, message) VALUES
  (
    1,
    1,
    'John Doe',
    'john.doe@example.com',
    '(555) 111-2222',
    'I''m interested in the 2015 Toyota Camry. Is it still available? Can I schedule a test drive this weekend?'
  ),
  (
    1,
    2,
    'Jane Smith',
    'jane.smith@example.com',
    '(555) 333-4444',
    'The 2018 Honda Civic looks great! Do you offer financing options? What''s the APR?'
  ),
  (
    1,
    NULL,
    'Bob Johnson',
    'bob.j@example.com',
    '(555) 555-6666',
    'I''m looking for a reliable truck under $30k. Do you have any recommendations based on your current inventory?'
  );

-- Insert Sample Leads for Dealership 2
INSERT INTO lead (dealership_id, vehicle_id, name, email, phone, message) VALUES
  (
    2,
    6,
    'Sarah Williams',
    'sarah.w@example.com',
    '(555) 777-8888',
    'Very interested in the 2019 BMW 3 Series. Can you provide the vehicle history report? What does the certified pre-owned warranty cover?'
  ),
  (
    2,
    9,
    'Michael Brown',
    'michael.brown@example.com',
    '(555) 999-0000',
    'I''d like more information about the Lexus RX 350. Has it been in any accidents? Can I bring my mechanic for an inspection?'
  );
```

### Running Scripts on Railway

**Method 1: Railway CLI (Recommended)**

```bash
# Install Railway CLI (one-time setup)
npm install -g @railway/cli

# Login to Railway
railway login

# Link to your project
railway link

# Run schema script
railway run psql $DATABASE_URL < backend/db/schema.sql

# Run seed script
railway run psql $DATABASE_URL < backend/db/seed.sql
```

**Method 2: Railway Dashboard**

1. Railway project → **PostgreSQL plugin** → **Data** tab → **Query**
2. Copy contents of `schema.sql`, paste, execute
3. Copy contents of `seed.sql`, paste, execute

**Verification:**

```bash
railway run psql $DATABASE_URL -c "SELECT COUNT(*) FROM dealership;"
# Expected: 2

railway run psql $DATABASE_URL -c "SELECT COUNT(*) FROM vehicle;"
# Expected: 10

railway run psql $DATABASE_URL -c "SELECT COUNT(*) FROM lead;"
# Expected: 5
```

---

## 9. Source Tree

Complete file tree for 2-day MVP. Every file listed is needed. Use as implementation checklist.

### Root Directory

```
JealPrototypeTest/
├── backend/                      # Express API server
├── frontend/                     # React app (Vite)
├── .gitignore                    # Git ignore rules
├── package.json                  # Root workspace config
├── README.md                     # Setup instructions
└── .env.example                  # Environment variable template
```

### Root Files

**package.json** (Root Workspace)

```json
{
  "name": "jeal-prototype-test",
  "version": "1.0.0",
  "private": true,
  "workspaces": [
    "backend",
    "frontend"
  ],
  "scripts": {
    "dev": "concurrently \"npm run server\" \"npm run client\"",
    "server": "npm run dev --workspace=backend",
    "client": "npm run dev --workspace=frontend",
    "build": "npm run build --workspace=frontend",
    "start": "npm start --workspace=backend"
  },
  "devDependencies": {
    "concurrently": "^8.2.2"
  }
}
```

**.gitignore**

```
# Dependencies
node_modules/
package-lock.json

# Environment
.env
.env.local

# Build outputs
frontend/dist/
frontend/build/

# Logs
*.log
npm-debug.log*

# OS files
.DS_Store
Thumbs.db

# IDE
.vscode/
.idea/
*.swp
*.swo
```

**.env.example**

```bash
# Backend - Railway PostgreSQL (same for dev and production)
DATABASE_URL=postgresql://postgres:ABC123@containers-us-west-123.railway.app:6789/railway

# Session secret (generate random string)
SESSION_SECRET=your-random-secret-here-change-this
ADMIN_USERNAME=admin
ADMIN_PASSWORD=admin123
NODE_ENV=development
PORT=5000

# Cloudinary
CLOUDINARY_CLOUD_NAME=your-cloud-name
CLOUDINARY_API_KEY=your-api-key
CLOUDINARY_API_SECRET=your-api-secret

# Frontend (Vite requires VITE_ prefix)
VITE_CLOUDINARY_CLOUD_NAME=your-cloud-name
VITE_CLOUDINARY_UPLOAD_PRESET=vehicle-images
```

**README.md**

```markdown
# Multi-Dealership Car Website + CMS Platform

2-day MVP prototype for multi-tenant dealership platform.

## Quick Start

1. **Clone and install:**
   ```bash
   git clone <repo-url>
   cd JealPrototypeTest
   npm install
   ```

2. **Set up Railway database:**
   - Create Railway account and PostgreSQL instance
   - Copy DATABASE_URL from Railway dashboard
   - Paste into .env file

3. **Set up environment:**
   ```bash
   cp .env.example .env
   # Edit .env with Railway DATABASE_URL, Cloudinary credentials, etc.
   ```

4. **Run database migrations:**
   ```bash
   railway login
   railway link
   railway run psql $DATABASE_URL < backend/db/schema.sql
   railway run psql $DATABASE_URL < backend/db/seed.sql
   ```

5. **Run development servers:**
   ```bash
   npm run dev
   ```
   - Backend: http://localhost:5000
   - Frontend: http://localhost:3000

6. **Login to admin:**
   - URL: http://localhost:3000/admin/login
   - Username: admin
   - Password: (from .env ADMIN_PASSWORD)

## Tech Stack

- Backend: Node.js 18, Express, node-postgres
- Frontend: React 18, Vite, Tailwind CSS, React Router
- Database: PostgreSQL 14+ (Railway)
- Image Storage: Cloudinary
- Deployment: Railway

## Project Structure

- `/backend` - Express API server
- `/frontend` - React SPA (public site + admin CMS)

## Deployment

Deploy to Railway:
1. Push to GitHub
2. Import project in Railway
3. Add PostgreSQL plugin
4. Set environment variables
5. Deploy
```

### Backend Structure

```
backend/
├── server.js                     # Express app setup, entry point
├── package.json                  # Backend dependencies
├── .env                          # Environment variables (not committed)
├── routes/
│   ├── auth.js                   # POST /auth/login, /logout, GET /auth/me
│   ├── dealers.js                # GET/PUT /dealers
│   ├── vehicles.js               # CRUD /vehicles
│   ├── leads.js                  # GET/POST /leads
│   └── upload.js                 # POST /upload (Cloudinary fallback)
├── db/
│   ├── index.js                  # PostgreSQL connection pool
│   ├── dealers.js                # Dealership queries
│   ├── vehicles.js               # Vehicle queries
│   ├── leads.js                  # Lead queries
│   ├── schema.sql                # Database schema (CREATE TABLE)
│   └── seed.sql                  # Seed data (2 dealerships + vehicles + leads)
└── middleware/
    └── auth.js                   # requireAuth middleware
```

**backend/package.json**

```json
{
  "name": "backend",
  "version": "1.0.0",
  "main": "server.js",
  "scripts": {
    "dev": "nodemon server.js",
    "start": "node server.js"
  },
  "dependencies": {
    "express": "^4.18.2",
    "pg": "^8.11.3",
    "dotenv": "^16.3.1",
    "express-session": "^1.17.3",
    "cookie-parser": "^1.4.6",
    "cors": "^2.8.5",
    "morgan": "^1.10.0"
  },
  "devDependencies": {
    "nodemon": "^3.0.2"
  }
}
```

**backend/server.js**

See Section 2 (High Level Architecture) for complete implementation.

**backend/db/index.js**

```javascript
const { Pool } = require('pg');

const pool = new Pool({
  connectionString: process.env.DATABASE_URL,
  ssl: process.env.NODE_ENV === 'production'
    ? { rejectUnauthorized: false }
    : false
});

// Test connection
pool.query('SELECT NOW()', (err, res) => {
  if (err) {
    console.error('Database connection error:', err);
  } else {
    console.log('Database connected successfully');
  }
});

module.exports = pool;
```

**backend/middleware/auth.js**

```javascript
function requireAuth(req, res, next) {
  if (!req.session.isAuthenticated) {
    return res.status(401).json({ error: 'Authentication required' });
  }
  next();
}

module.exports = { requireAuth };
```

### Frontend Structure

```
frontend/
├── index.html                    # HTML entry point (with Cloudinary script)
├── package.json                  # Frontend dependencies
├── vite.config.js                # Vite configuration (proxy to backend)
├── tailwind.config.js            # Tailwind CSS configuration
├── postcss.config.js             # PostCSS configuration
├── .env                          # Frontend env vars (VITE_* prefix)
├── src/
│   ├── main.jsx                  # React entry point
│   ├── App.jsx                   # Root component with routes
│   ├── index.css                 # Tailwind directives + global styles
│   ├── pages/
│   │   ├── public/
│   │   │   ├── Home.jsx          # Public home page
│   │   │   ├── Inventory.jsx     # Vehicle listing with search/filter
│   │   │   ├── VehicleDetail.jsx # Vehicle detail + enquiry form
│   │   │   └── About.jsx         # Dealership about/contact page
│   │   └── admin/
│   │       ├── Login.jsx         # Admin login page
│   │       ├── Dashboard.jsx     # Admin dashboard with dealership selector
│   │       ├── VehicleList.jsx   # Vehicle manager (list + actions)
│   │       ├── VehicleForm.jsx   # Create/edit vehicle form
│   │       ├── DealerSettings.jsx # Dealership settings form
│   │       └── LeadInbox.jsx     # Customer enquiries list
│   ├── components/
│   │   ├── Layout.jsx            # Public site layout wrapper
│   │   ├── Header.jsx            # Public site header/nav
│   │   ├── AdminHeader.jsx       # Admin header with dealership selector
│   │   ├── VehicleCard.jsx       # Vehicle grid item component
│   │   ├── EnquiryForm.jsx       # Reusable enquiry form
│   │   ├── ImageGallery.jsx      # Vehicle detail image gallery
│   │   └── ProtectedRoute.jsx    # Auth wrapper for admin routes
│   ├── context/
│   │   └── AdminContext.jsx      # Global admin state (auth, selected dealership)
│   └── utils/
│       └── api.js                # Fetch wrappers for API calls (optional)
└── public/
    └── (static assets like favicon.ico)
```

**frontend/package.json**

```json
{
  "name": "frontend",
  "version": "1.0.0",
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "vite build",
    "preview": "vite preview"
  },
  "dependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "react-router-dom": "^6.20.0",
    "react-hook-form": "^7.49.0"
  },
  "devDependencies": {
    "@vitejs/plugin-react": "^4.2.1",
    "vite": "^5.0.8",
    "tailwindcss": "^3.4.0",
    "postcss": "^8.4.32",
    "autoprefixer": "^10.4.16"
  }
}
```

**frontend/index.html**

```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Dealership Platform</title>
  </head>
  <body>
    <div id="root"></div>
    <script type="module" src="/src/main.jsx"></script>

    <!-- Cloudinary Upload Widget -->
    <script src="https://upload-widget.cloudinary.com/global/all.js" type="text/javascript"></script>
  </body>
</html>
```

**frontend/vite.config.js**

```javascript
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true
      }
    }
  },
  build: {
    outDir: 'dist'
  }
});
```

**frontend/tailwind.config.js**

```javascript
/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,jsx}"
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}
```

**frontend/postcss.config.js**

```javascript
export default {
  plugins: {
    tailwindcss: {},
    autoprefixer: {},
  },
}
```

**frontend/src/index.css**

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

/* Global styles */
body {
  margin: 0;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen',
    'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue',
    sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

/* Custom utility classes */
.btn-primary {
  @apply bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition;
}

.btn-secondary {
  @apply bg-gray-600 text-white px-4 py-2 rounded hover:bg-gray-700 transition;
}

.btn-danger {
  @apply bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700 transition;
}

.input-field {
  @apply border border-gray-300 rounded px-3 py-2 w-full focus:outline-none focus:ring-2 focus:ring-blue-500;
}

.card {
  @apply bg-white rounded-lg shadow-md p-4;
}
```

**frontend/src/main.jsx**

```javascript
import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './index.css';

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
```

**frontend/src/App.jsx**

See Section 6 (Components) for complete implementation.

### Total File Count

**Backend:** 14 files
- 1 server.js
- 1 package.json
- 5 route files
- 4 db files
- 2 SQL scripts
- 1 middleware file

**Frontend:** 27 files
- 1 index.html
- 1 package.json
- 3 config files (vite, tailwind, postcss)
- 3 root files (main.jsx, App.jsx, index.css)
- 10 page components
- 7 shared components
- 1 context file
- 1 utils file (optional)

**Root:** 4 files
- package.json, .gitignore, README.md, .env.example

**Grand Total:** ~45 files

---

## 10. Development Workflow

### Part 1: Local Development Setup (30-45 minutes)

**Step 1: Prerequisites Check**

```bash
node --version    # Should be v18 or higher
npm --version     # Should be v9 or higher
git --version     # Any recent version
```

**Note:** No local PostgreSQL required. Railway provides the database.

**Step 2-7:** Create project structure, install dependencies (see Section 9 for commands)

**Step 8: Set Up Railway PostgreSQL Database**

1. Create Railway account: https://railway.app/ (free, sign up with GitHub)
2. Create new project → **"Provision PostgreSQL"**
3. Copy DATABASE_URL from PostgreSQL service → Connect tab
4. Paste into `.env` file

**Step 9: Run Database Migrations**

```bash
# Install Railway CLI
npm install -g @railway/cli
railway login
railway link

# Run migrations
railway run psql $DATABASE_URL < backend/db/schema.sql
railway run psql $DATABASE_URL < backend/db/seed.sql

# Verify
railway run psql $DATABASE_URL -c "SELECT COUNT(*) FROM dealership;"
```

**Step 10: Start Development Servers**

```bash
npm run dev
```

**Expected:** Backend on port 5000, Frontend on port 3000

### Part 2: Testing Core Flows

See Section 10 (Development Workflow) in full document for detailed testing checklists.

### Part 3: Railway Production Deployment

See Section 10 (Development Workflow) in full document for step-by-step deployment guide.

---

## 11. Coding Standards

### JSDoc Documentation Requirements

All functions, classes, and complex logic must include JSDoc comments for human readability and AI agent understanding.

**Backend Functions:**

```javascript
/**
 * Retrieves all vehicles for a specific dealership with optional status filter.
 *
 * @param {number} dealershipId - The dealership ID to filter vehicles
 * @param {string} [status] - Optional status filter ('active', 'sold', 'pending', 'draft')
 * @returns {Promise<Array<Object>>} Array of vehicle objects
 * @throws {Error} If database query fails
 *
 * @example
 * const vehicles = await getAll(1, 'active');
 */
async function getAll(dealershipId, status) {
  // Implementation
}
```

**Frontend Components:**

```javascript
/**
 * VehicleCard - Displays a vehicle summary in grid/list view.
 *
 * @component
 * @param {Object} props
 * @param {Object} props.vehicle - Vehicle object from API
 * @param {number} props.vehicle.id - Vehicle ID
 * @param {string} props.vehicle.title - Vehicle display title
 * @param {number} props.vehicle.price - Vehicle price in dollars
 *
 * @example
 * <VehicleCard vehicle={vehicleData} />
 */
function VehicleCard({ vehicle }) {
  // Implementation
}
```

**File Header Comments:**

```javascript
/**
 * @fileoverview Vehicle CRUD API routes.
 * Handles all vehicle-related operations with multi-tenant filtering.
 *
 * Routes:
 * - GET    /api/vehicles       - List vehicles for dealership
 * - POST   /api/vehicles       - Create vehicle (auth required)
 * - PUT    /api/vehicles/:id   - Update vehicle (auth required)
 * - DELETE /api/vehicles/:id   - Delete vehicle (auth required)
 */
```

**Guidelines:**
- Document WHY, not just WHAT
- Explain business logic and constraints
- Note multi-tenancy requirements
- Include examples for complex functions
- Comment SQL queries with their purpose
- Note security considerations

---

## 11.1. Dynamic Theme Color Management System

**Overview:**

The platform features a comprehensive dynamic theming system allowing each dealership to customize their brand color from the CMS admin panel. The theme color is applied consistently across the entire dealership website including header, buttons, links, prices, and all UI elements.

**Key Architecture Components:**

1. **Database Layer:** `dealership.theme_color` VARCHAR(7) field stores hex color codes (default: #3B82F6)
2. **Backend API:** PUT /api/dealers/:id endpoint with hex color validation
3. **CSS Variables:** Three CSS custom properties for runtime theming:
   - `--theme-color`: Main brand color
   - `--theme-color-dark`: Darker shade for hover states (15% darker)
   - `--theme-color-light`: Lighter shade for backgrounds (90% lighter)
4. **Frontend Setup:** Both Layout.jsx (public) and AdminContext.jsx (admin) set CSS variables dynamically
5. **Admin UI:** DealerSettings.jsx provides color picker with live preview

**Why CSS Custom Properties?**

This approach was chosen over Tailwind's JIT compilation or other theming methods because:
- **Runtime theming:** Dealerships can change colors and see results immediately without rebuild
- **Zero build overhead:** No CSS regeneration or app rebuild required
- **Native performance:** Pure CSS with no JavaScript-in-CSS overhead
- **Automatic propagation:** Changing one variable updates all elements using it

**Usage in Components:**

```javascript
// ✅ PREFERRED: Use utility classes
<button className="bg-theme text-white">Click Me</button>
<a className="text-theme">Learn More</a>

// ✅ ACCEPTABLE: Direct CSS variable for complex styling
<div style={{ backgroundColor: 'var(--theme-color)' }}>Content</div>

// ❌ WRONG: Hardcoded colors
<button className="bg-blue-600">Wrong</button>
```

**Automatic Shade Calculation:**

The system automatically calculates lighter and darker shades using RGB manipulation:

```javascript
// Darker (85% multiplier for hover states)
darkerR = Math.round(r * 0.85);

// Lighter (90% toward white for backgrounds)
lighterR = Math.round(r + (255 - r) * 0.9);
```

**Components Using Theme Colors:**
- Header.jsx: Background color (`.bg-theme`)
- VehicleCard.jsx: Price display (`.text-theme`)
- Inventory.jsx: Phone numbers, filter badges
- All public pages: Contact links, CTA buttons

**Complete Documentation:** See [docs/architecture/theming-system.md](./architecture/theming-system.md) for comprehensive implementation details, data flows, and usage guidelines.

---

## 12. 2-Day Implementation Checklist

See Section 11 (2-Day Implementation Checklist) in this document for complete hour-by-hour breakdown covering:

**Day 1 (12 hours):**
- Hour 0-1: Environment setup (Railway, dependencies)
- Hour 1-2: Database schema + Express foundation
- Hour 2-4: Backend database modules + auth routes
- Hour 4-6: Backend API routes (dealers, vehicles, leads)
- Hour 7-9: Frontend foundation + routing
- Hour 9-11: Shared components + admin layout
- Hour 11-12: Admin login page

**Day 2 (12 hours):**
- Hour 0-2: Public site pages (Home, Inventory, Detail, About)
- Hour 2-4: Admin dashboard + dealership selector
- Hour 4-6: Admin vehicle manager (CRUD)
- Hour 6-7: Cloudinary integration
- Hour 8-9: Admin settings + lead inbox
- Hour 9-10: Final testing + bug fixes
- Hour 10-11: Railway production deployment
- Hour 11-12: Production testing + demo prep

---

**End of Architecture Document**
