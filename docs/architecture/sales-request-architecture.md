# Sales Request Feature - Technical Architecture

**Version:** 1.1  
**Date:** 2025-12-31 (Updated)  
**Status:** ✅ IMPLEMENTED + EMAIL NOTIFICATIONS  
**Author:** Architecture Team

**Latest Update:** December 31, 2025 - Added email notification system

---

## Table of Contents
1. [Overview](#overview)
2. [System Architecture](#system-architecture)
3. [Data Model](#data-model)
4. [API Design](#api-design)
5. [Frontend Architecture](#frontend-architecture)
6. [Security Architecture](#security-architecture)
7. [Performance Considerations](#performance-considerations)
8. [Integration Points](#integration-points)
9. [Deployment Architecture](#deployment-architecture)

---

## Overview

### Purpose
The Sales Request feature enables customers to submit vehicle sales inquiries through the public website and allows dealership staff to manage these requests through the admin CMS. This document outlines the technical architecture and design decisions.

### Architecture Principles
- **Multi-Tenancy:** Strict data isolation per dealership
- **Security-First:** Input sanitization, validation, and authorization
- **Consistency:** Follows existing patterns (Lead Inbox, Vehicle Management)
- **Scalability:** Indexed queries, efficient data structures
- **Maintainability:** Clear separation of concerns, documented code

---

## System Architecture

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                      CLIENT LAYER                            │
├──────────────────────┬──────────────────────────────────────┤
│   Public Website     │      Admin CMS                       │
│                      │                                       │
│ ┌──────────────────┐ │ ┌──────────────────────────────────┐│
│ │ Header Nav       │ │ │ AdminHeader                      ││
│ │ - Sell Your Car  │ │ │ - Sales Requests Link            ││
│ └──────────────────┘ │ └──────────────────────────────────┘│
│                      │                                       │
│ ┌──────────────────┐ │ ┌──────────────────────────────────┐│
│ │ SellYourCar.jsx  │ │ │ SalesRequests.jsx                ││
│ │ - Form UI        │ │ │ - Table View                     ││
│ │ - Validation     │ │ │ - Status Management              ││
│ │ - Submit         │ │ │ - Contact Actions                ││
│ └──────────────────┘ │ │ - Delete Functionality           ││
│                      │ │ - Date Filtering                 ││
│                      │ └──────────────────────────────────┘│
└──────────────────────┴──────────────────────────────────────┘
                            │
                            ↓ HTTP/HTTPS (JSON)
┌─────────────────────────────────────────────────────────────┐
│                     API LAYER (Express.js)                   │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Routes: /api/sales-requests                                │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ salesRequests.js                                     │  │
│  │ ┌──────────────────────────────────────────────────┐ │  │
│  │ │ POST   /                    (Public)             │ │  │
│  │ │   ↓ Sends email notification (NEW 2025-12-31)    │ │  │
│  │ │ GET    /?dealershipId=:id   (Admin)              │ │  │
│  │ │ PATCH  /:id/status          (Admin)              │ │  │
│  │ │ DELETE /:id                 (Admin)              │ │  │
│  │ └──────────────────────────────────────────────────┘ │  │
│  │                                                       │  │
│  │ Middleware:                                          │  │
│  │ - Input Validation                                   │  │
│  │ - XSS Sanitization                                   │  │
│  │ - Multi-tenant Filtering                             │  │
│  │ - Authentication Check (admin routes)                │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  Services: /services/emailService.js (NEW 2025-12-31)      │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ sendNewSalesRequestNotification()                    │  │
│  │ - HTML email template                                │  │
│  │ - Plain text fallback                                │  │
│  │ - Non-blocking error handling                        │  │
│  │ - Uses nodemailer + SMTP                             │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                    │                    │
                    │                    └─→ SMTP Server (Email)
                    ↓ SQL Queries (Parameterized)
┌─────────────────────────────────────────────────────────────┐
│                  DATA ACCESS LAYER                           │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Database Functions: db/salesRequests.js                    │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ getAll(dealershipId)                                 │  │
│  │ create(salesRequestData)                             │  │
│  │ updateStatus(id, dealershipId, status)               │  │
│  │ deleteSalesRequest(id, dealershipId)                 │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
                            │
                            ↓ PostgreSQL Connection Pool
┌─────────────────────────────────────────────────────────────┐
│                  DATABASE LAYER (PostgreSQL)                 │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Table: sales_request                                       │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Columns:                                             │  │
│  │ - id (PK)                                            │  │
│  │ - dealership_id (FK → dealership.id)                │  │
│  │ - name, email, phone                                 │  │
│  │ - make, model, year, kilometers                      │  │
│  │ - additional_message                                 │  │
│  │ - status (CHECK constraint)                          │  │
│  │ - created_at                                         │  │
│  │                                                       │  │
│  │ Indexes:                                             │  │
│  │ - idx_sales_request_dealership_id                    │  │
│  │ - idx_sales_request_created_at (DESC)                │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## Data Model

### Entity: sales_request

#### Schema Definition

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

#### Relationships

```
dealership (1) ──────< (M) sales_request
   │                         │
   └─ dealership_id (FK) ────┘
   
ON DELETE CASCADE: When dealership deleted, all sales requests deleted
```

#### Indexes

| Index Name | Columns | Type | Purpose |
|------------|---------|------|---------|
| `sales_request_pkey` | `id` | PRIMARY KEY | Unique identifier |
| `idx_sales_request_dealership_id` | `dealership_id` | BTREE | Multi-tenant filtering |
| `idx_sales_request_created_at` | `created_at DESC` | BTREE | Sorting (newest first) |

#### Constraints

| Constraint | Type | Definition |
|------------|------|------------|
| `sales_request_dealership_id_fkey` | FOREIGN KEY | `dealership_id → dealership(id)` |
| `sales_request_status_check` | CHECK | `status IN ('received', 'in progress', 'done')` |

### Data Flow Diagram

```
Customer Submission Flow:
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   Customer   │────>│   Form UI    │────>│  API POST    │────>│  Database    │
│  (Browser)   │     │ Validation   │     │  Sanitize    │     │   INSERT     │
└──────────────┘     └──────────────┘     └──────────────┘     └──────────────┘
                            │                     │
                            ↓ (On Failure)       ↓ (On Success)
                     ┌──────────────┐     ┌──────────────┐
                     │ Error Msg    │     │ Success Msg  │
                     │ Display      │     │ Form Reset   │
                     └──────────────┘     └──────────────┘

Admin Management Flow:
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  Admin User  │────>│   Admin UI   │────>│  API GET     │────>│  Database    │
│  (Browser)   │     │   Table      │     │ + Filter by  │     │   SELECT     │
└──────────────┘     └──────────────┘     │ dealershipId │     └──────────────┘
       │                    ↓               └──────────────┘           │
       │             ┌──────────────┐                                 │
       │             │  Status      │<────────────────────────────────┘
       │             │  Update UI   │
       │             └──────────────┘
       │                    │
       └──────> (Actions)   │
                │           │
        ┌───────┴───────────┴────────┐
        │                            │
  ┌──────────┐              ┌──────────────┐
  │  DELETE  │              │ PATCH Status │
  │  Request │              │   Request    │
  └──────────┘              └──────────────┘
```

---

## API Design

### REST API Specification

#### 1. Submit Sales Request (Public)

**Endpoint:** `POST /api/sales-requests`  
**Authentication:** None (Public endpoint)  
**Content-Type:** `application/json`

**Request Body:**
```json
{
  "dealership_id": 1,
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "(555) 123-4567",
  "make": "Toyota",
  "model": "Camry",
  "year": 2018,
  "kilometers": 75000,
  "additional_message": "Well maintained, one owner"
}
```

**Validation Rules:**
- `dealership_id`: Required, positive integer
- `name`: Required, max 255 characters
- `email`: Required, max 255 characters, valid email format
- `phone`: Required, max 20 characters
- `make`: Required, max 100 characters
- `model`: Required, max 100 characters
- `year`: Required, integer, 1900 ≤ year ≤ (current_year + 1)
- `kilometers`: Required, integer, ≥ 0
- `additional_message`: Optional, max 5000 characters

**Response (201 Created):**
```json
{
  "id": 42,
  "dealership_id": 1,
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "(555) 123-4567",
  "make": "Toyota",
  "model": "Camry",
  "year": 2018,
  "kilometers": 75000,
  "additional_message": "Well maintained, one owner",
  "status": "received",
  "created_at": "2025-12-17T12:00:00.000Z"
}
```

**Error Responses:**
- `400 Bad Request`: Validation failed
  ```json
  { "error": "Missing required fields: email, phone" }
  ```
- `500 Internal Server Error`: Database error

---

#### 2. List Sales Requests (Admin)

**Endpoint:** `GET /api/sales-requests?dealershipId=1`  
**Authentication:** Required (Session cookie)  
**Query Parameters:**
- `dealershipId` (required): Dealership ID to filter by

**Response (200 OK):**
```json
[
  {
    "id": 42,
    "dealership_id": 1,
    "name": "John Doe",
    "email": "john@example.com",
    "phone": "(555) 123-4567",
    "make": "Toyota",
    "model": "Camry",
    "year": 2018,
    "kilometers": 75000,
    "additional_message": "Well maintained",
    "status": "received",
    "created_at": "2025-12-17T12:00:00.000Z"
  }
]
```

**Sorting:** Results sorted by `created_at DESC` (newest first)

**Error Responses:**
- `400 Bad Request`: Missing dealershipId
  ```json
  { "error": "dealershipId query parameter is required" }
  ```
- `401 Unauthorized`: Not authenticated
- `500 Internal Server Error`: Database error

---

#### 3. Update Sales Request Status (Admin)

**Endpoint:** `PATCH /api/sales-requests/:id/status?dealershipId=1`  
**Authentication:** Required (Session cookie)  
**Content-Type:** `application/json`

**Request Body:**
```json
{
  "status": "in progress"
}
```

**Valid Status Values:** `"received"`, `"in progress"`, `"done"`

**Response (200 OK):**
```json
{
  "id": 42,
  "dealership_id": 1,
  "status": "in progress",
  ...
}
```

**Error Responses:**
- `400 Bad Request`: Invalid status or missing dealershipId
- `404 Not Found`: Sales request not found or doesn't belong to dealership
- `500 Internal Server Error`: Database error

---

#### 4. Delete Sales Request (Admin)

**Endpoint:** `DELETE /api/sales-requests/:id?dealershipId=1`  
**Authentication:** Required (Session cookie)

**Response (200 OK):**
```json
{
  "message": "Sales request deleted successfully"
}
```

**Error Responses:**
- `400 Bad Request`: Missing dealershipId
- `404 Not Found`: Sales request not found or doesn't belong to dealership
- `500 Internal Server Error`: Database error

---

## Frontend Architecture

### Component Hierarchy

```
App
├── Layout (Public)
│   ├── Header
│   │   └── NavigationButton (Sell Your Car)
│   └── SellYourCar
│       ├── Personal Info Section
│       │   ├── Name Input
│       │   ├── Email Input
│       │   └── Phone Input
│       └── Vehicle Details Section
│           ├── Make Input
│           ├── Model Input
│           ├── Year Input
│           ├── Kilometers Input
│           └── Additional Message Textarea
│
└── ProtectedRoute (Admin)
    └── AdminHeader
        ├── Navigation Link (Sales Requests)
        └── SalesRequests
            ├── Date Filter Dropdown
            ├── Sales Requests Table
            │   ├── Table Headers
            │   └── Table Rows
            │       ├── Customer Info
            │       ├── Vehicle Info
            │       ├── Status Dropdown
            │       └── Action Buttons
            │           ├── Call Button
            │           ├── Email Button
            │           └── Delete Button
            └── Delete Confirmation Modal
```

### State Management

#### SellYourCar Component State

```javascript
{
  formData: {
    name: '',
    email: '',
    phone: '',
    make: '',
    model: '',
    year: '',
    kilometers: '',
    additional_message: ''
  },
  loading: false,
  success: false,
  error: null
}
```

#### SalesRequests Component State

```javascript
{
  salesRequests: [],
  loading: false,
  error: null,
  expandedRequestId: null,
  dateFilter: 'All time',
  deleteModal: {
    isOpen: false,
    requestId: null,
    requestName: ''
  }
}
```

### Context Usage

- **DealershipContext:** Provides `currentDealershipId` for public form submissions
- **AdminContext:** Provides `selectedDealership` for admin filtering

---

## Security Architecture

### 1. Input Sanitization (XSS Prevention)

**Threat:** Stored XSS via malicious input in text fields

**Mitigation:**
```javascript
function sanitizeInput(input) {
  if (typeof input !== 'string') return input;
  return input
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#x27;')
    .replace(/\//g, '&#x2F;');
}
```

**Applied to:**
- name
- phone
- make
- model
- additional_message

**Not applied to:**
- email (validated format only, no HTML rendering)
- year, kilometers (integer validation)

---

### 2. Multi-Tenant Isolation

**Threat:** Cross-dealership data access

**Mitigation Strategy:**

**Database Level:**
```sql
-- All queries filter by dealership_id
SELECT * FROM sales_request WHERE dealership_id = $1;

-- Foreign key constraint
dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE
```

**API Level:**
```javascript
// Require dealershipId query parameter
if (!dealershipId) {
  return res.status(400).json({ error: 'dealershipId required' });
}

// Validate ownership before update/delete
const result = await salesRequestsDb.deleteSalesRequest(id, dealershipId);
if (!result) {
  return res.status(404).json({ error: 'Not found or unauthorized' });
}
```

**UI Level:**
```javascript
// AdminContext provides selectedDealership
const { selectedDealership } = useContext(AdminContext);

// All API calls include dealershipId
fetch(`/api/sales-requests?dealershipId=${selectedDealership.id}`)
```

---

### 3. SQL Injection Prevention

**Mitigation:** Parameterized queries using `pg` library

```javascript
// BAD (Vulnerable to SQL injection)
pool.query(`SELECT * FROM sales_request WHERE id = ${id}`);

// GOOD (Parameterized)
pool.query('SELECT * FROM sales_request WHERE id = $1', [id]);
```

---

### 4. Authentication & Authorization

**Public Endpoints:**
- `POST /api/sales-requests` - No auth required (customer-facing)

**Protected Endpoints (Admin only):**
- `GET /api/sales-requests`
- `PATCH /api/sales-requests/:id/status`
- `DELETE /api/sales-requests/:id`

**Enforcement:** Express session middleware + ProtectedRoute component

---

## Performance Considerations

### Database Optimization

#### 1. Indexes

```sql
-- Primary lookup by dealership (most common query)
CREATE INDEX idx_sales_request_dealership_id ON sales_request(dealership_id);

-- Sorting for admin table (newest first)
CREATE INDEX idx_sales_request_created_at ON sales_request(created_at DESC);
```

**Query Performance:**
- Filtered list query: O(log n) due to dealership_id index
- Sorted retrieval: O(1) for index scan

#### 2. Query Patterns

```javascript
// Efficient: Uses both indexes
SELECT * FROM sales_request 
WHERE dealership_id = $1 
ORDER BY created_at DESC;

// Query plan:
// 1. Index scan on idx_sales_request_dealership_id
// 2. Index scan on idx_sales_request_created_at (already sorted)
```

### Frontend Optimization

#### 1. Lazy Loading
- Additional messages truncated to 100 characters
- Expand on demand with "Show more" button

#### 2. Debouncing (Future)
- Search/filter inputs debounced to reduce API calls

#### 3. Pagination (Future Consideration)
- Current implementation loads all requests
- Recommendation: Implement pagination at 100+ records

### API Response Times

| Endpoint | Expected Time | Notes |
|----------|---------------|-------|
| POST /api/sales-requests | < 200ms | Single INSERT query |
| GET /api/sales-requests | < 300ms | Indexed SELECT with ORDER BY |
| PATCH /api/sales-requests/:id/status | < 150ms | Single UPDATE query |
| DELETE /api/sales-requests/:id | < 150ms | Single DELETE query |

---

## Integration Points

### 1. Navigation System Integration

**Modified Files:**
- `frontend/src/utils/defaultNavigation.js`
- `backend/config/defaultNavigation.js`

**Integration:**
```javascript
{
  id: 'sell-your-car',
  label: 'Sell Your Car',
  route: '/sell-your-car',
  icon: 'FaDollarSign',
  order: 7,
  enabled: true,
  showIcon: true
}
```

### 2. Icon System Integration

**Modified File:** `frontend/src/utils/iconMapper.js`

**Added Icon:**
```javascript
import { FaDollarSign } from 'react-icons/fa';

const iconMap = {
  ...
  FaDollarSign,
  ...
};
```

### 3. Routing Integration

**Modified File:** `frontend/src/App.jsx`

```javascript
// Public route
<Route path="sell-your-car" element={<SellYourCar />} />

// Admin route (protected)
<Route path="sales-requests" element={<SalesRequests />} />
```

### 4. Admin Navigation Integration

**Modified File:** `frontend/src/components/AdminHeader.jsx`

```javascript
<Link to="/admin/sales-requests">
  Sales Requests
</Link>
```

### 5. Email Service Integration (Added 2025-12-31)

**Integration Point:** Notification system for new sales requests

**Modified Files:**
- `backend/services/emailService.js` - Email notification function
- `backend/routes/salesRequests.js` - Email trigger in POST endpoint

**Architecture:**
```
Customer Submits Form
        ↓
  POST /api/sales-requests
        ↓
  salesRequestsDb.create()  ← Saves to database
        ↓
  dealersDb.getById()       ← Fetches dealership email
        ↓
  sendNewSalesRequestNotification()  ← Sends email
        ↓
  Return success (even if email fails)
```

**Email Flow:**
```javascript
// 1. Create sales request in database
const salesRequest = await salesRequestsDb.create(data);

// 2. Fetch dealership contact email
const dealership = await dealersDb.getById(dealership_id);

// 3. Send notification email (non-blocking)
try {
  await sendNewSalesRequestNotification(dealership.email, {
    name, email, phone, make, model, year, kilometers, additional_message
  });
} catch (emailError) {
  // Log error but don't fail request
  console.error('Email failed:', emailError);
}

// 4. Return success regardless of email outcome
return res.status(201).json(salesRequest);
```

**Configuration Requirements:**
```env
EMAIL_HOST=smtp.gmail.com
EMAIL_PORT=587
EMAIL_USER=your-email@gmail.com
EMAIL_PASSWORD=your-app-password
EMAIL_FROM=noreply@yourdomain.com
```

**Error Handling:**
- Missing email config: Warning logged, request proceeds
- Email sending fails: Error logged, request proceeds
- Dealership has no email: Warning logged, request proceeds
- Non-blocking: Email failures never prevent sales request creation

---

## Deployment Architecture

### Database Migration

**File:** `backend/db/migrations/006_add_sales_request_table.sql`

**Deployment Steps:**
```bash
# 1. Create table and indexes
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < 006_add_sales_request_table.sql

# 2. Verify table creation
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype -c "\d sales_request"

# 3. Verify indexes
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype -c "\di"
```

**Rollback Plan:**
```sql
DROP TABLE IF EXISTS sales_request CASCADE;
```

### Backend Deployment

**Environment Variables:** None required (uses existing DB connection)

**Dependencies:** No new npm packages required

**Deployment Checklist:**
- ✅ Database migration executed
- ✅ Backend server restarted
- ✅ API endpoints tested (health check)
- ✅ Multi-tenant isolation verified

### Frontend Deployment

**Build Process:**
```bash
cd frontend
npm run build
```

**Static Assets:** No new assets added

**Deployment Checklist:**
- ✅ Frontend build successful
- ✅ Routes accessible
- ✅ Navigation links visible
- ✅ Forms functional

---

## Testing Architecture

### Unit Tests (Backend)

```javascript
// db/salesRequests.test.js
describe('salesRequests database functions', () => {
  test('getAll filters by dealershipId', async () => {
    const requests = await salesRequestsDb.getAll(1);
    requests.forEach(r => expect(r.dealership_id).toBe(1));
  });

  test('create validates required fields', async () => {
    await expect(salesRequestsDb.create({}))
      .rejects.toThrow('Missing required fields');
  });
});

// routes/salesRequests.test.js
describe('POST /api/sales-requests', () => {
  test('sanitizes input', async () => {
    const response = await request(app)
      .post('/api/sales-requests')
      .send({ name: '<script>alert("xss")</script>', ... });
    
    expect(response.body.name).toBe('&lt;script&gt;alert(&quot;xss&quot;)&lt;/script&gt;');
  });
});
```

### Integration Tests

```javascript
describe('Sales Request API Integration', () => {
  test('customer submits request → admin views request', async () => {
    // 1. Submit request
    const submitResponse = await request(app)
      .post('/api/sales-requests')
      .send({ dealership_id: 1, name: 'Test', ... });
    
    const requestId = submitResponse.body.id;

    // 2. Admin retrieves requests
    const listResponse = await authenticatedRequest(app)
      .get('/api/sales-requests?dealershipId=1');
    
    expect(listResponse.body).toContainEqual(
      expect.objectContaining({ id: requestId })
    );
  });
});
```

### E2E Tests (Cypress/Playwright)

```javascript
describe('Sales Request Flow', () => {
  it('customer can submit sales request', () => {
    cy.visit('/');
    cy.contains('Sell Your Car').click();
    cy.url().should('include', '/sell-your-car');
    
    cy.get('input[name="name"]').type('John Doe');
    cy.get('input[name="email"]').type('john@example.com');
    // ... fill other fields
    cy.get('button[type="submit"]').click();
    
    cy.contains('Success!').should('be.visible');
  });

  it('admin can view and manage sales requests', () => {
    cy.login('admin', 'password');
    cy.visit('/admin/sales-requests');
    
    cy.get('table').should('contain', 'John Doe');
    cy.get('select[value="received"]').select('in progress');
    cy.get('table').should('contain', 'in progress');
  });
});
```

---

## Monitoring & Observability

### Metrics to Track

1. **Usage Metrics:**
   - Sales requests submitted per dealership per day
   - Conversion rate: submissions → status "done"
   - Average response time (submission → first admin view)

2. **Performance Metrics:**
   - API response times (P50, P95, P99)
   - Database query times
   - Form submission success rate

3. **Error Metrics:**
   - Validation errors by field
   - API error rate (4xx, 5xx)
   - Failed submissions

### Logging Strategy

```javascript
// Backend logging (Morgan middleware)
app.use(morgan('dev'));

// Custom logging for sales requests
console.log('New sales request:', {
  dealership_id,
  make,
  model,
  year,
  timestamp: new Date().toISOString()
});
```

---

## Future Architecture Considerations

### Scalability Enhancements

1. **Pagination:** 
   - Implement cursor-based pagination for > 100 records
   - Add `limit` and `offset` query parameters

2. **Caching:**
   - Redis cache for frequently accessed dealership data
   - Cache invalidation on status updates

3. **Real-time Updates:**
   - WebSocket connection for live admin dashboard updates
   - Push notifications for new submissions

### Integration Opportunities

1. **Email Service:**
   - SendGrid/Mailgun integration for notifications
   - Template system for automated responses

2. **CRM Integration:**
   - Export API for external CRM systems
   - Webhook support for real-time sync

3. **Analytics:**
   - Google Analytics events for form tracking
   - Custom dashboard for sales metrics

---

## Architecture Decision Records (ADRs)

### ADR-001: Use Same Table Pattern as Lead Inbox

**Context:** Need to store customer sales requests

**Decision:** Create new `sales_request` table following same pattern as `lead` table

**Rationale:**
- Consistency with existing codebase
- Familiarity for developers
- Proven pattern for multi-tenancy

**Consequences:**
- ✅ Faster development (reuse patterns)
- ✅ Easier maintenance
- ❌ Some duplication (could consider unified "inquiries" table in future)

---

### ADR-002: Separate Route `/api/sales-requests`

**Context:** Need API endpoints for sales request management

**Decision:** Create separate route module instead of extending `/api/leads`

**Rationale:**
- Clear separation of concerns
- Different data model and validation rules
- Future extensibility (different features)

**Consequences:**
- ✅ Clean code organization
- ✅ Independent evolution of features
- ❌ Some code duplication (sanitization, validation logic)

---

### ADR-003: Public Submission Endpoint (No Auth)

**Context:** Customers need to submit sales requests without account

**Decision:** POST endpoint is public (no authentication required)

**Rationale:**
- Lower barrier to entry for customers
- Consistent with lead submission pattern
- Standard practice for contact forms

**Consequences:**
- ✅ Better user experience
- ⚠️ Requires spam protection (future consideration)
- ⚠️ Must validate and sanitize all inputs rigorously

---

## Conclusion

The Sales Request feature architecture follows established patterns in the codebase while introducing a new customer acquisition channel. The design prioritizes security (input sanitization, multi-tenant isolation), performance (indexed queries), and maintainability (clear separation of concerns).

**December 31, 2025 Update:** Email notification system added to immediately alert dealerships of new sales requests, improving response times and conversion rates.

All architectural decisions align with the project's technical assumptions and non-functional requirements, ensuring a scalable and secure implementation.

---

## Email Notification Architecture (Added 2025-12-31)

### Overview
Automatic email notifications are sent to dealerships when customers submit sales requests through the "Sell Your Car" form.

### Email Service Design

**Service Location:** `backend/services/emailService.js`

**Function:** `sendNewSalesRequestNotification(dealershipEmail, salesRequestData)`

**Email Template:**
- Subject: "New Sales Request: {year} {make} {model}"
- HTML + plain text versions
- Includes: Customer info, vehicle details, additional message
- Professional formatting matching lead notifications

### Integration Architecture

```
Customer Form Submission
        ↓
POST /api/sales-requests
        ↓
1. Validate & sanitize input
2. Create sales_request record ✅
3. Fetch dealership email
4. Send notification email (async, non-blocking)
5. Return 201 Created (even if email fails)
```

**Key Design Decision:** Non-blocking email delivery
- Sales request saved FIRST
- Email failures logged but don't fail request
- Ensures customer submission always succeeds

### Configuration
```env
EMAIL_HOST, EMAIL_PORT, EMAIL_USER, EMAIL_PASSWORD, EMAIL_FROM
```

See `SALES_REQUEST_EMAIL_NOTIFICATION.md` for details.

---

## Architecture Decision Records (ADRs)

### ADR-004: Email Notifications (December 31, 2025)

**Status:** Accepted

**Context:**
Dealerships need immediate notification when customers submit sales requests for quick response and better conversion.

**Decision:**
Implement automatic email notifications using existing nodemailer infrastructure (same as lead notifications).

**Consequences:**
✅ Immediate dealership notification
✅ Consistent with lead notification pattern  
✅ Reuses existing infrastructure
✅ Non-blocking ensures reliability
⚠️ Requires email configuration

---

**Document Version:** 1.1  
**Last Updated:** 2025-12-31  
**Status:** ✅ APPROVED AND IMPLEMENTED (with email notifications)
