# Sell Your Car Feature - Quick Start Guide

## ✅ Feature Status: IMPLEMENTED & READY

The "Sell Your Car" feature has been successfully implemented and is ready for testing!

## 🎯 What Was Built

### Public Side (Customer-Facing)
- **Navigation Link**: "Sell Your Car" appears in the header menu (between Location and Log In)
- **Form Page**: `/sell-your-car` - Customers can submit their vehicle details
- **Form Fields**:
  - Personal: Name, Email, Phone
  - Vehicle: Make, Model, Year, Kilometers, Additional Message (optional)

### Admin Side (Dealership Staff)
- **Management Page**: `/admin/sales-requests` - View and manage all sales requests
- **Navigation Link**: "Sales Requests" in admin header (between Lead Inbox and View Website)
- **Features**:
  - View all requests in a table
  - Filter by date (All time, Last 7 days, Last 30 days)
  - Update status (Received → In Progress → Done)
  - Contact customers (Call/Email buttons)
  - Delete requests with confirmation

## 🚀 How to Test

### 1. Database Migration (COMPLETED ✓)
The database table has already been created and verified:
```bash
# Already executed - sales_request table exists
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype -c "\dt"
```

### 2. Start the Backend Server
```bash
cd backend
npm start
```
Backend will run on: http://localhost:5000

### 3. Start the Frontend Server
```bash
cd frontend
npm run dev
```
Frontend will run on: http://localhost:3000

### 4. Test Customer Flow
1. Open browser to http://localhost:3000
2. Click **"Sell Your Car"** in the header navigation
3. Fill out the form:
   - Name: Test Customer
   - Email: test@example.com
   - Phone: (555) 123-4567
   - Make: Honda
   - Model: Civic
   - Year: 2020
   - Kilometers: 50000
   - Additional Message: Excellent condition, one owner
4. Click **"Submit Request"**
5. Verify success message appears

### 5. Test Admin Flow
1. Navigate to http://localhost:3000/admin/login
2. Login with admin credentials
3. Click **"Sales Requests"** in the admin navigation
4. Verify your submitted request appears in the table
5. Test the features:
   - Change status from "Received" to "In Progress"
   - Click "Call" button (should open tel: link)
   - Click "Email" button (should open mailto: link)
   - Try the date filter dropdown
   - Click "Delete" and confirm in the modal

## 📁 New Files Created

### Backend
- `backend/db/salesRequests.js` - Database query functions
- `backend/routes/salesRequests.js` - API endpoints
- `backend/db/migrations/006_add_sales_request_table.sql` - Database migration

### Frontend
- `frontend/src/pages/public/SellYourCar.jsx` - Customer form page
- `frontend/src/pages/admin/SalesRequests.jsx` - Admin management page

## 📝 Files Modified

### Backend
- `backend/server.js` - Added sales requests routes
- `backend/db/schema.sql` - Added sales_request table
- `backend/config/defaultNavigation.js` - Added nav link

### Frontend
- `frontend/src/App.jsx` - Added routes
- `frontend/src/components/AdminHeader.jsx` - Added admin nav link
- `frontend/src/utils/defaultNavigation.js` - Added public nav link
- `frontend/src/utils/iconMapper.js` - Added FaDollarSign icon

## 🔒 Security Features

✅ XSS Prevention (input sanitization)
✅ Email validation
✅ Field length limits
✅ Year and kilometers validation
✅ Multi-tenant data isolation
✅ Admin authentication required
✅ SQL injection prevention (parameterized queries)

## 📊 Database Schema

```
sales_request
├── id (PRIMARY KEY)
├── dealership_id (FOREIGN KEY → dealership.id)
├── name (VARCHAR 255)
├── email (VARCHAR 255)
├── phone (VARCHAR 20)
├── make (VARCHAR 100)
├── model (VARCHAR 100)
├── year (INTEGER)
├── kilometers (INTEGER)
├── additional_message (TEXT, nullable)
├── status (VARCHAR 20, default: 'received')
└── created_at (TIMESTAMP, default: NOW())

Indexes:
- idx_sales_request_dealership_id
- idx_sales_request_created_at
```

## 🌐 API Endpoints

### Public Endpoint
```
POST /api/sales-requests
- Submit new sales request (no auth required)
```

### Admin Endpoints (require dealershipId parameter)
```
GET    /api/sales-requests?dealershipId=1
       - List all sales requests for dealership

PATCH  /api/sales-requests/:id/status?dealershipId=1
       - Update request status

DELETE /api/sales-requests/:id?dealershipId=1
       - Delete request
```

## 🎨 UI/UX Features

### Public Form
- Clean, responsive design
- Two-section layout (Personal Info / Vehicle Details)
- Real-time validation
- Success/error messaging
- Theme color integration
- Form reset on success

### Admin Page
- Table view with all details
- Sortable by date (newest first)
- Color-coded status badges
- Expandable additional messages (truncated at 100 chars)
- Quick action buttons (Call, Email, Delete)
- Date filtering
- Responsive design

## ✨ Next Steps (Optional Enhancements)

1. **Email Notifications**: Send email to dealership when new request submitted
2. **Export to CSV**: Export sales requests for external analysis
3. **Photo Upload**: Allow customers to upload vehicle photos
4. **Valuation Integration**: Auto-estimate vehicle value
5. **SMS Notifications**: Text alerts for new requests
6. **Notes/Comments**: Add internal notes to requests
7. **Advanced Filters**: Filter by make, model, year range, etc.

## 🐛 Troubleshooting

### "Table doesn't exist" error
```bash
# Re-run migration
Get-Content backend\db\migrations\006_add_sales_request_table.sql | docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype
```

### "Module not found" error
```bash
# Restart backend server
cd backend
npm start
```

### Navigation link not appearing
- Clear browser cache
- Restart frontend server
- Check browser console for errors

## 📞 Support

For issues or questions about this feature:
1. Check the detailed documentation: `SELL_YOUR_CAR_FEATURE.md`
2. Review backend logs in the terminal
3. Check browser console for frontend errors
4. Verify database connection is working

---

**Status**: ✅ COMPLETE - Ready for testing and deployment!
