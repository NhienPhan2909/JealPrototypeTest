# Epic 6: Vehicle Sales Request Feature (Sell Your Car)

**Status:** ✅ COMPLETED  
**Implementation Date:** 2025-12-17  
**Epic Owner:** Product Manager  
**Development Team:** Full-stack development team

---

## Epic Overview

Enable customers to submit vehicle sales requests through a "Sell Your Car" form on the public website. Dealership staff can view, manage, and respond to these sales inquiries through a dedicated admin panel interface.

This feature creates a new customer acquisition channel by allowing vehicle owners to proactively express interest in selling their vehicles to the dealership, complementing the existing lead capture system.

---

## Business Context

### Problem Statement
Dealerships need a way to acquire inventory from customers who want to sell their vehicles. Currently, the platform only supports vehicle sales (outbound) through the inventory listing, but lacks a mechanism for inbound vehicle acquisition requests.

### Business Value
- **Revenue Opportunity**: New inventory acquisition channel
- **Customer Engagement**: Additional touchpoint for customer relationship building
- **Market Intelligence**: Gather data on vehicle types customers want to sell
- **Competitive Advantage**: Streamlined digital process vs. traditional phone-only inquiries

### Success Metrics
- Number of sales requests submitted per dealership per month
- Conversion rate: Sales requests → Actual purchases
- Response time: Time from submission to first dealership contact
- Customer satisfaction: Quality of form experience (via feedback)

---

## User Stories

### Public-Facing Stories

#### Story 6.1: Navigation Access
**As a** vehicle owner  
**I want to** easily find a "Sell Your Car" option in the website navigation  
**So that** I can quickly access the sales request form

**Acceptance Criteria:**
- ✅ "Sell Your Car" link appears in header navigation (order 7, between Location and Log In)
- ✅ Link uses FaDollarSign icon
- ✅ Link is visible on desktop and mobile views
- ✅ Clicking link navigates to `/sell-your-car` route

#### Story 6.2: Sales Request Form Submission
**As a** vehicle owner  
**I want to** submit my vehicle information through an easy-to-use form  
**So that** the dealership can contact me with an offer

**Acceptance Criteria:**
- ✅ Form displays in clean, responsive layout
- ✅ Form collects required fields: name, email, phone, make, model, year, kilometers
- ✅ Form collects optional field: additional_message (for extra details)
- ✅ Form validates email format
- ✅ Form validates year (1900 to current year + 1)
- ✅ Form validates kilometers (positive number)
- ✅ Success message displays after successful submission
- ✅ Form resets after successful submission
- ✅ Error messages display if submission fails
- ✅ Submit button shows loading state during submission
- ✅ Form respects dealership theme colors

### Admin-Facing Stories

#### Story 6.3: Sales Request Management Interface
**As a** dealership staff member  
**I want to** view all sales requests in a dedicated admin section  
**So that** I can review and respond to customer inquiries

**Acceptance Criteria:**
- ✅ "Sales Requests" link appears in admin navigation (between Lead Inbox and View Website)
- ✅ Admin page displays all sales requests in table format
- ✅ Table shows: name, email, phone, vehicle details (year/make/model), kilometers, additional info, date submitted, status
- ✅ Requests sorted by newest first
- ✅ Only requests for selected dealership are visible (multi-tenant isolation)

#### Story 6.4: Sales Request Status Management
**As a** dealership staff member  
**I want to** track the status of each sales request  
**So that** I can manage my workflow and follow up appropriately

**Acceptance Criteria:**
- ✅ Status dropdown displays for each request with options: Received, In Progress, Done
- ✅ Status updates persist to database
- ✅ Status displayed with color-coded badges (blue=received, yellow=in progress, green=done)
- ✅ Default status is "received" for new submissions

#### Story 6.5: Sales Request Contact Actions
**As a** dealership staff member  
**I want to** quickly contact customers who submitted sales requests  
**So that** I can respond efficiently

**Acceptance Criteria:**
- ✅ "Call" button opens tel: link with customer phone number
- ✅ "Email" button opens mailto: link with pre-filled subject (vehicle details)
- ✅ Email subject includes vehicle year, make, and model

#### Story 6.6: Sales Request Deletion
**As a** dealership staff member  
**I want to** delete sales requests that are spam or no longer relevant  
**So that** I can keep my inbox clean

**Acceptance Criteria:**
- ✅ "Delete" button appears for each request
- ✅ Confirmation modal appears before deletion
- ✅ Modal displays customer name for verification
- ✅ Request is permanently deleted from database after confirmation
- ✅ Only requests belonging to selected dealership can be deleted (security)

#### Story 6.7: Sales Request Filtering
**As a** dealership staff member  
**I want to** filter sales requests by date  
**So that** I can focus on recent or historical inquiries

**Acceptance Criteria:**
- ✅ Date filter dropdown with options: All time, Last 7 days, Last 30 days
- ✅ Filtered count displays (e.g., "Showing 5 sales requests")
- ✅ Filter persists during session

---

## Functional Requirements

### Public Form (FR-SALES-001 to FR-SALES-008)

**FR-SALES-001:** Public website shall display "Sell Your Car" link in header navigation  
**Priority:** P0 (Must Have)

**FR-SALES-002:** Sales request form shall collect customer information (name, email, phone)  
**Priority:** P0 (Must Have)

**FR-SALES-003:** Sales request form shall collect vehicle information (make, model, year, kilometers)  
**Priority:** P0 (Must Have)

**FR-SALES-004:** Sales request form shall validate email format and required fields  
**Priority:** P0 (Must Have)

**FR-SALES-005:** Sales request form shall validate year range (1900 to current year + 1)  
**Priority:** P0 (Must Have)

**FR-SALES-006:** Sales request form shall validate kilometers as positive integer  
**Priority:** P0 (Must Have)

**FR-SALES-007:** Sales request form shall display success/error messages  
**Priority:** P0 (Must Have)

**FR-SALES-008:** Sales request form shall optionally collect additional message (max 5000 characters)  
**Priority:** P1 (Should Have)

### Admin Management (FR-SALES-009 to FR-SALES-017)

**FR-SALES-009:** Admin panel shall display "Sales Requests" navigation link  
**Priority:** P0 (Must Have)

**FR-SALES-010:** Sales Requests page shall display all requests in table format  
**Priority:** P0 (Must Have)

**FR-SALES-011:** Sales Requests table shall sort requests by newest first  
**Priority:** P0 (Must Have)

**FR-SALES-012:** Each sales request shall have editable status (received, in progress, done)  
**Priority:** P0 (Must Have)

**FR-SALES-013:** Admin shall be able to contact customers via Call/Email buttons  
**Priority:** P0 (Must Have)

**FR-SALES-014:** Admin shall be able to delete sales requests with confirmation  
**Priority:** P0 (Must Have)

**FR-SALES-015:** Admin shall be able to filter sales requests by date range  
**Priority:** P1 (Should Have)

**FR-SALES-016:** Long additional messages shall be truncated with "Show more" expansion  
**Priority:** P2 (Nice to Have)

**FR-SALES-017:** Admin panel shall enforce multi-tenant data isolation (dealershipId filtering)  
**Priority:** P0 (Must Have) - Security Critical

---

## Non-Functional Requirements

**NFR-SALES-001:** Form submission shall complete within 3 seconds under normal network conditions  
**NFR-SALES-002:** All user inputs shall be sanitized to prevent XSS attacks  
**NFR-SALES-003:** Email validation shall prevent common typos (missing @, .com, etc.)  
**NFR-SALES-004:** Form shall be fully responsive on mobile, tablet, and desktop  
**NFR-SALES-005:** Admin page shall support pagination for > 100 sales requests (future)  
**NFR-SALES-006:** Database queries shall use indexes for optimal performance  
**NFR-SALES-007:** API endpoints shall enforce multi-tenant isolation via dealershipId  
**NFR-SALES-008:** HTML entities in user input shall be decoded for safe display  
**NFR-SALES-009:** Email notifications shall be sent to dealership within 5 seconds of form submission (Added 2025-12-31)  
**NFR-SALES-010:** Email sending failures shall not prevent successful sales request creation (Added 2025-12-31)  

---

## Technical Specifications

### Database Schema

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

-- Indexes for performance
CREATE INDEX idx_sales_request_dealership_id ON sales_request(dealership_id);
CREATE INDEX idx_sales_request_created_at ON sales_request(created_at DESC);
```

### API Endpoints

| Method | Endpoint | Auth Required | Description |
|--------|----------|---------------|-------------|
| POST | `/api/sales-requests` | No (Public) | Submit new sales request |
| GET | `/api/sales-requests?dealershipId=:id` | Yes (Admin) | List all sales requests for dealership |
| PATCH | `/api/sales-requests/:id/status?dealershipId=:id` | Yes (Admin) | Update sales request status |
| DELETE | `/api/sales-requests/:id?dealershipId=:id` | Yes (Admin) | Delete sales request |

### Frontend Routes

| Route | Component | Access |
|-------|-----------|--------|
| `/sell-your-car` | `SellYourCar.jsx` | Public |
| `/admin/sales-requests` | `SalesRequests.jsx` | Protected (Admin) |

---

## Security Considerations

### Input Sanitization (SEC-SALES-001)
All text inputs (name, phone, make, model, additional_message) shall be sanitized using HTML entity encoding to prevent stored XSS attacks when displayed in admin panel.

**Implementation:**
```javascript
function sanitizeInput(input) {
  return input
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#x27;')
    .replace(/\//g, '&#x2F;');
}
```

### Multi-Tenant Isolation (SEC-SALES-002)
All API endpoints requiring admin access shall enforce `dealershipId` filtering to prevent cross-dealership data access.

**Example:**
```javascript
// GET endpoint
const salesRequests = await salesRequestsDb.getAll(dealershipIdNum);

// DELETE endpoint
const deleted = await salesRequestsDb.deleteSalesRequest(idNum, dealershipIdNum);
```

### Validation Rules (SEC-SALES-003)
- Name: Max 255 characters
- Email: Max 255 characters, valid format
- Phone: Max 20 characters
- Make/Model: Max 100 characters each
- Year: Integer, 1900 ≤ year ≤ (current year + 1)
- Kilometers: Integer, ≥ 0
- Additional Message: Max 5000 characters

---

## Implementation Summary

### Files Created
1. **Backend:**
   - `backend/db/salesRequests.js` - Database query functions
   - `backend/routes/salesRequests.js` - API route handlers
   - `backend/db/migrations/006_add_sales_request_table.sql` - Database migration

2. **Frontend:**
   - `frontend/src/pages/public/SellYourCar.jsx` - Public form page
   - `frontend/src/pages/admin/SalesRequests.jsx` - Admin management page

### Files Modified
1. **Backend:**
   - `backend/db/schema.sql` - Added sales_request table
   - `backend/server.js` - Registered sales request routes
   - `backend/config/defaultNavigation.js` - Added navigation item
   - `backend/services/emailService.js` - Added email notification function (Updated 2025-12-31)
   - `backend/routes/salesRequests.js` - Added email notification logic (Updated 2025-12-31)

2. **Frontend:**
   - `frontend/src/App.jsx` - Added routes
   - `frontend/src/components/AdminHeader.jsx` - Added admin navigation link
   - `frontend/src/utils/defaultNavigation.js` - Added public navigation item
   - `frontend/src/utils/iconMapper.js` - Added FaDollarSign icon

---

## Testing Requirements

### Unit Tests
- [ ] Database query functions (CRUD operations)
- [ ] Input sanitization function
- [ ] Validation functions (email, year, kilometers)

### Integration Tests
- [ ] POST /api/sales-requests endpoint
- [ ] GET /api/sales-requests endpoint
- [ ] PATCH /api/sales-requests/:id/status endpoint
- [ ] DELETE /api/sales-requests/:id endpoint

### End-to-End Tests
- [ ] Customer submits sales request successfully
- [ ] Admin views sales request in table
- [ ] Admin updates sales request status
- [ ] Admin deletes sales request
- [ ] Multi-tenant isolation is enforced

### Manual Testing Checklist
- ✅ Navigate to /sell-your-car from header
- ✅ Submit form with valid data
- ✅ Submit form with invalid email (should error)
- ✅ Submit form with invalid year (should error)
- ✅ View sales request in admin panel
- ✅ Update status to "in progress"
- ✅ Click Call button (should open tel: link)
- ✅ Click Email button (should open mailto: link)
- ✅ Delete sales request with confirmation
- ✅ Filter by "Last 7 days"
- ✅ Test responsive design on mobile

---

## Recent Updates

### December 31, 2025 - Email Notification Feature Added ✅
**Status:** IMPLEMENTED AND DEPLOYED

Added automatic email notifications to dealerships when new sales requests are submitted.

**Implementation Details:**
- Dealership receives email immediately when customer submits "Sell Your Car" form
- Email includes all customer information and vehicle details
- Professional HTML template with plain text fallback
- Non-blocking: Email failures don't prevent request submission
- Uses same email infrastructure as lead notifications

**Files Modified:**
- `backend/services/emailService.js` - Added `sendNewSalesRequestNotification()` function
- `backend/routes/salesRequests.js` - Added email sending logic to POST endpoint

**Documentation:**
- See `SALES_REQUEST_EMAIL_NOTIFICATION.md` for full details
- See `test_sales_request_email.js` for testing

---

## Future Enhancements

### Phase 2 Considerations
1. ✅ **Email Notifications:** Auto-email dealership when new sales request submitted (COMPLETED 2025-12-31)
2. **Photo Upload:** Allow customers to upload vehicle photos
3. **Valuation API:** Integrate with KBB/Edmunds for instant vehicle valuation
4. **SMS Notifications:** Text alerts for new sales requests
5. **Advanced Filtering:** Filter by make, model, year range, price range
6. **Export to CSV:** Export sales requests for external CRM systems
7. **Notes/Comments:** Add internal notes to sales requests
8. **Response Templates:** Pre-written response templates for common scenarios
9. **Integration with Lead Inbox:** Unified view of all customer inquiries
10. **Analytics Dashboard:** Track sales request metrics and trends

---

## Stakeholder Sign-Off

**Product Manager:** ✅ Approved  
**Tech Lead:** ✅ Approved  
**QA Lead:** ✅ Approved  
**Business Owner:** ✅ Approved  

**Implementation Date:** December 17, 2025  
**Status:** COMPLETED AND DEPLOYED ✅
