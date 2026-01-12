# Sell Your Car Feature - Implementation Summary

## Overview
Implemented a complete "Sell Your Car" feature that allows customers to submit vehicle sales requests through the dealership website. Dealership staff can manage these requests through a new admin panel section.

## Feature Components

### 1. Database Layer
- **Table Created**: `sales_request`
  - Fields: id, dealership_id, name, email, phone, make, model, year, kilometers, additional_message, status, created_at
  - Status options: 'received', 'in progress', 'done'
  - Indexes created for performance (dealership_id, created_at DESC)
  - Multi-tenant isolation enforced via dealership_id foreign key

- **Migration File**: `backend/db/migrations/006_add_sales_request_table.sql`
- **Database Functions**: `backend/db/salesRequests.js`
  - `getAll(dealershipId)` - Fetch all sales requests for a dealership
  - `create(salesRequestData)` - Create new sales request
  - `updateStatus(id, dealershipId, status)` - Update request status
  - `deleteSalesRequest(id, dealershipId)` - Delete request

### 2. Backend API Routes
- **File**: `backend/routes/salesRequests.js`
- **Endpoints**:
  - `GET /api/sales-requests?dealershipId=<id>` - List sales requests (admin)
  - `POST /api/sales-requests` - Submit new sales request (public)
  - `PATCH /api/sales-requests/:id/status?dealershipId=<id>` - Update status (admin)
  - `DELETE /api/sales-requests/:id?dealershipId=<id>` - Delete request (admin)

- **Security Features**:
  - Input sanitization to prevent XSS attacks
  - Email format validation
  - Field length validation
  - Year and kilometers validation
  - Multi-tenant data isolation

- **Route Registration**: Added to `backend/server.js` as `/api/sales-requests`

### 3. Public Page
- **File**: `frontend/src/pages/public/SellYourCar.jsx`
- **Route**: `/sell-your-car`
- **Features**:
  - Responsive form with two sections (Your Information, Vehicle Details)
  - Required fields: name, email, phone, make, model, year, kilometers
  - Optional field: additional_message
  - Success/error message display
  - Form reset on successful submission
  - Theme color integration

### 4. Admin Management Page
- **File**: `frontend/src/pages/admin/SalesRequests.jsx`
- **Route**: `/admin/sales-requests`
- **Features**:
  - Table view of all sales requests
  - Date filtering (All time, Last 7 days, Last 30 days)
  - Status management dropdown (received, in progress, done)
  - Contact actions (Call, Email buttons)
  - Delete functionality with confirmation modal
  - Message expansion for long additional messages
  - HTML entity decoding for safe display
  - Sorted by newest first

### 5. Navigation Updates

#### Public Navigation
- Added "Sell Your Car" link to header navigation
- Icon: FaDollarSign
- Order: 7 (between Location and Log In)
- Updated files:
  - `frontend/src/utils/defaultNavigation.js`
  - `backend/config/defaultNavigation.js`
  - `frontend/src/utils/iconMapper.js` (added FaDollarSign icon)

#### Admin Navigation
- Added "Sales Requests" link to AdminHeader
- Positioned between "Lead Inbox" and "View Website"
- Updated file: `frontend/src/components/AdminHeader.jsx`

### 6. Routing Configuration
- **File**: `frontend/src/App.jsx`
- **Public Route**: `/sell-your-car` → `<SellYourCar />`
- **Admin Route**: `/admin/sales-requests` → `<SalesRequests />` (protected)

## Files Created
1. `backend/db/salesRequests.js` - Database query functions
2. `backend/routes/salesRequests.js` - API route handlers
3. `backend/db/migrations/006_add_sales_request_table.sql` - Database migration
4. `frontend/src/pages/public/SellYourCar.jsx` - Public form page
5. `frontend/src/pages/admin/SalesRequests.jsx` - Admin management page

## Files Modified
1. `backend/db/schema.sql` - Added sales_request table definition
2. `backend/server.js` - Registered sales requests routes
3. `frontend/src/App.jsx` - Added public and admin routes
4. `frontend/src/components/AdminHeader.jsx` - Added navigation link
5. `frontend/src/utils/defaultNavigation.js` - Added "Sell Your Car" nav item
6. `backend/config/defaultNavigation.js` - Added "Sell Your Car" nav item
7. `frontend/src/utils/iconMapper.js` - Added FaDollarSign icon

## Testing Steps

### 1. Start Backend Server
```bash
cd backend
npm start
```

### 2. Start Frontend Server
```bash
cd frontend
npm run dev
```

### 3. Test Public Form Submission
1. Navigate to any dealership website
2. Click "Sell Your Car" in the header
3. Fill out the form with test data:
   - Name: John Doe
   - Email: john@example.com
   - Phone: (555) 123-4567
   - Make: Toyota
   - Model: Camry
   - Year: 2018
   - Kilometers: 75000
   - Additional Message: Great condition, well maintained
4. Submit and verify success message

### 4. Test Admin Management
1. Log in to admin panel at `/admin/login`
2. Click "Sales Requests" in the navigation
3. Verify submitted request appears in the table
4. Test status change (received → in progress → done)
5. Test contact actions (Call, Email buttons)
6. Test date filtering
7. Test delete functionality

## Security Considerations
- All inputs are sanitized to prevent XSS attacks
- Email addresses are validated
- Field lengths are enforced
- Multi-tenant isolation via dealershipId
- Admin endpoints are protected by authentication
- Status values are restricted via CHECK constraint

## Database Schema
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

## API Examples

### Submit Sales Request (Public)
```bash
POST /api/sales-requests
Content-Type: application/json

{
  "dealership_id": 1,
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "(555) 123-4567",
  "make": "Toyota",
  "model": "Camry",
  "year": 2018,
  "kilometers": 75000,
  "additional_message": "Great condition"
}
```

### Get Sales Requests (Admin)
```bash
GET /api/sales-requests?dealershipId=1
```

### Update Status (Admin)
```bash
PATCH /api/sales-requests/1/status?dealershipId=1
Content-Type: application/json

{
  "status": "in progress"
}
```

### Delete Sales Request (Admin)
```bash
DELETE /api/sales-requests/1?dealershipId=1
```

## Future Enhancements
1. Email notifications to dealership when new sales request is submitted
2. Export sales requests to CSV
3. Advanced filtering (by make/model, year range)
4. Add notes/comments to sales requests
5. Integration with vehicle valuation APIs
6. Photo upload capability for customer's vehicle
7. SMS notifications
8. Response templates for common replies

## Notes
- The implementation follows the same patterns as the Lead Inbox feature
- All security measures from leads (XSS prevention, input validation) are applied
- Multi-tenant isolation is enforced at all levels (database, API, UI)
- The feature is fully responsive and works on mobile devices
- Success and error states are properly handled
- The admin interface matches the styling of existing admin pages
