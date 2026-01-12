# Hierarchical User Management System

## Overview

This system implements a complete 4-tier user hierarchy with role-based access control (RBAC), multi-tenancy support, and granular permission management. The system uses a **read-only by default** approach where all authenticated users can view data, but permissions control who can edit.

## 🎯 User Types

### 1. **Admin** (Super Administrator)
**Capabilities:**
- ✅ Full access to all dealerships
- ✅ Can create/manage dealership owner accounts
- ✅ Can view and edit all data across all dealerships
- ✅ Can delete any user account
- ✅ Switch between dealerships via dropdown selector
- ❌ No dealership restriction

**Database:**
```json
{
  "user_type": "admin",
  "dealership_id": null,
  "permissions": []
}
```

### 2. **Dealership Owner**
**Capabilities:**
- ✅ Full access to their specific dealership
- ✅ Can create/manage/delete staff accounts for their dealership
- ✅ Can assign and modify permissions for staff
- ✅ Can view and edit all dealership data
- ✅ Can delete staff from their dealership
- ❌ Cannot access other dealerships
- ❌ Cannot delete themselves

**Database:**
```json
{
  "user_type": "dealership_owner",
  "dealership_id": 1,
  "permissions": []
}
```

### 3. **Dealership Staff**
**Capabilities:**
- ✅ Can view **all** sections and data for their dealership (read-only by default)
- ✅ Can see all navigation links (Dashboard, Vehicle Manager, Blog Manager, Settings, Lead Inbox, Sales Requests)
- ✅ Can view dashboard statistics (full access)
- ✅ Can edit only sections they have permission for
- ✅ See read-only banners on sections without edit permission
- ✅ Can call/email customers from lead inbox (read-only actions)
- ❌ Cannot create or delete user accounts
- ❌ Cannot access other dealerships
- ❌ Cannot edit data without specific permission

**Database:**
```json
{
  "user_type": "dealership_staff",
  "dealership_id": 1,
  "permissions": ["leads", "vehicles"]
}
```

**Available Permissions:**
- `leads` - Can edit lead status and delete leads
- `sales_requests` - Can edit sales request status and delete requests
- `vehicles` - Can create, edit, and delete vehicles
- `blogs` - Can create, edit, and delete blog posts
- `settings` - Can edit dealership settings

**Permission Model:**
- **Viewing:** All staff can VIEW all sections (read-only)
- **Editing:** Only staff with specific permissions can EDIT those sections
- **Example:** Staff without "leads" permission can:
  - ✅ View all leads in the inbox
  - ✅ Call or email customers
  - ❌ Change lead status
  - ❌ Delete leads

### 4. **End User** (Public Visitor)
**Capabilities:**
- ✅ Visit dealership websites
- ✅ Submit enquiries and sales requests
- ❌ No authentication required
- ❌ No database record

## 🚀 Quick Start

### Prerequisites
- Docker running PostgreSQL database
- Node.js backend
- React frontend

### Installation (5 Steps)

#### Step 1: Run Database Migration
```bash
Get-Content backend\db\migrations\009_add_user_hierarchy.sql | docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype
```

#### Step 2: Install bcrypt
```bash
cd backend
npm install bcrypt
```

#### Step 3: Seed Admin User
```bash
cd backend
node db\migrations\seed_admin.js
```

This creates:
- **Username:** `admin`
- **Password:** `admin123`
- **Email:** `admin@example.com`

#### Step 4: Restart Backend
```bash
cd backend
npm run dev
```

#### Step 5: Test Login
1. Navigate to `http://localhost:3000/admin/login`
2. Login with `admin` / `admin123`
3. ✅ You should see the admin dashboard

⚠️ **IMPORTANT:** Change the admin password immediately after first login!

## 📚 Usage Guide

### As Admin

#### Create Dealership Owners
1. Login at `/admin/login`
2. Click "User Management" in the header
3. Click "Create User"
4. Fill in:
   - Username: `owner_dealership1`
   - Password: Secure password
   - Email: Owner's email
   - Full Name: Owner's name
   - User Type: **Dealership Owner**
   - Dealership: Select from dropdown
5. Click "Create User"

### As Dealership Owner

#### Create Staff Members
1. Login with your owner account
2. Go to "User Management"
3. Click "Create User"
4. Fill in staff details
5. **Assign Permissions** (check boxes):
   - ☐ Manage Leads
   - ☐ Manage Sales Requests
   - ☐ Manage Vehicles
   - ☐ Manage Blogs
   - ☐ Edit Dealership Settings
6. Click "Create User"

#### Manage Staff
- ✅ View all staff for your dealership
- ✅ Edit staff details and permissions
- ✅ Deactivate staff accounts
- ❌ Cannot see staff from other dealerships

### As Dealership Staff

When you login:
- ✅ Can VIEW everything for your dealership
- ✅ Can EDIT only sections you have permission for
- 📋 If you have NO permissions = read-only access

**Example Permissions:**

**Sales Manager** - `["leads", "sales_requests"]`
- ✅ Respond to customer inquiries
- ✅ Manage lead inbox
- ❌ Cannot edit vehicles

**Inventory Manager** - `["vehicles"]`
- ✅ Add/edit vehicle listings
- ❌ Cannot access leads

**Full Access Staff** - `["leads", "sales_requests", "vehicles", "blogs", "settings"]`
- ✅ Can do everything an owner can do

**Read-Only** - `[]` (empty array)
- ✅ View everything
- ❌ Cannot edit anything

## 🔐 API Reference

### Authentication Endpoints

#### POST /api/auth/login
Login with username and password.

**Request:**
```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Response:**
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

#### POST /api/auth/logout
Logout and destroy session.

#### GET /api/auth/me
Get current authenticated user.

### User Management Endpoints

#### GET /api/users
List users (filtered by role).
- **Admin:** All users
- **Owner:** Users from their dealership

#### GET /api/users/:id
Get specific user by ID.

#### POST /api/users
Create new user.

**Request (Owner creating staff):**
```json
{
  "username": "john_staff",
  "password": "password123",
  "email": "john@example.com",
  "full_name": "John Doe",
  "user_type": "dealership_staff",
  "dealership_id": 1,
  "permissions": ["leads", "vehicles"]
}
```

#### PUT /api/users/:id
Update user (email, full_name, permissions).

#### PUT /api/users/:id/password
Update password.

**Request:**
```json
{
  "password": "newpassword123"
}
```

#### DELETE /api/users/:id
**Delete user (hard delete - permanently removes from database).**

**Authorization:**
- Admin: Can delete any user
- Owner: Can delete staff from their dealership
- Staff: Cannot delete users
- No one can delete themselves

**Result:**
- User completely removed from database
- Username becomes available for reuse
- Users created by deleted user have `created_by` set to NULL

## 🛡️ Security Features

### Password Security
- ✅ **Bcrypt hashing** (10 salt rounds)
- ✅ Minimum 6 characters enforced
- ✅ Passwords never stored in plain text

### Session Security
- ✅ Server-side session storage
- ✅ HTTP-only cookies (prevents XSS)
- ✅ 24-hour session expiry

### Access Control
- ✅ **Role-based access control (RBAC)**
- ✅ **Multi-tenant isolation** - Users can only access their dealership
- ✅ **Permission-based authorization** for granular control
- ✅ **Read-only by default** - All users can view, permissions control editing
- ✅ Middleware enforces all security rules

### Data Management
- ✅ **Hard delete** - Users permanently removed from database
- ✅ Username reuse allowed after deletion
- ✅ Tracks who created each user (`created_by`)
- ✅ Timestamps for creation and updates
- ✅ Foreign key constraints prevent orphaned records

## 📊 Database Schema

```sql
CREATE TABLE app_user (
  id SERIAL PRIMARY KEY,
  username VARCHAR(255) NOT NULL UNIQUE,
  password_hash VARCHAR(255) NOT NULL,
  email VARCHAR(255) NOT NULL,
  full_name VARCHAR(255) NOT NULL,
  user_type VARCHAR(20) NOT NULL CHECK (user_type IN ('admin', 'dealership_owner', 'dealership_staff')),
  dealership_id INTEGER REFERENCES dealership(id) ON DELETE CASCADE,
  created_by INTEGER REFERENCES app_user(id) ON DELETE SET NULL,
  permissions JSONB DEFAULT '[]'::jsonb,
  is_active BOOLEAN DEFAULT true,
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW(),
  
  CONSTRAINT check_admin_no_dealership CHECK (
    (user_type = 'admin' AND dealership_id IS NULL) OR
    (user_type IN ('dealership_owner', 'dealership_staff') AND dealership_id IS NOT NULL)
  )
);
```

**Indexes:**
- `idx_user_username` (UNIQUE) - Fast login lookups
- `idx_user_email` - Email searches
- `idx_user_dealership_id` - Multi-tenant queries
- `idx_user_type` - Role filtering
- `idx_user_is_active` - Active user queries

## 🧪 Testing

### Automated Test
```bash
node test_user_hierarchy.js
```

This validates:
- ✅ Database table exists
- ✅ Admin user created
- ✅ Password hashing works
- ✅ Indexes created
- ✅ Constraints enforced

### Manual Test (API)

#### Test Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

#### Test User Creation
```bash
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -H "Cookie: connect.sid=<your-session-cookie>" \
  -d '{
    "username": "test_owner",
    "password": "password123",
    "email": "test@example.com",
    "full_name": "Test Owner",
    "user_type": "dealership_owner",
    "dealership_id": 1
  }'
```

## 🔧 Troubleshooting

### Can't Login
**Check:**
```bash
# Database running?
docker ps | grep jeal-prototype-db

# Admin user exists?
docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT * FROM app_user;"

# Backend logs
npm run dev
```

### Migration Failed
```bash
# Check if table exists
docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "\dt"

# Drop and recreate if needed
docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "DROP TABLE app_user CASCADE;"
Get-Content backend\db\migrations\009_add_user_hierarchy.sql | docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype
```

### bcrypt Error
```bash
cd backend
npm install bcrypt
npm rebuild bcrypt
```

## 📁 File Structure

```
backend/
├── db/
│   ├── migrations/
│   │   ├── 009_add_user_hierarchy.sql  # Migration
│   │   └── seed_admin.js                # Seed script
│   └── users.js                          # User DB functions
├── middleware/
│   └── auth.js                           # Auth middleware (updated)
├── routes/
│   ├── auth.js                           # Auth routes (updated)
│   └── users.js                          # User routes (new)
└── server.js                             # Mount user routes

frontend/
├── src/
│   ├── context/
│   │   └── AdminContext.jsx              # User state (updated)
│   ├── pages/
│   │   └── admin/
│   │       ├── Login.jsx                 # Login page (updated)
│   │       └── UserManagement.jsx        # User mgmt (new)
│   ├── components/
│   │   └── AdminHeader.jsx               # Header (updated)
│   └── App.jsx                           # Routes (updated)

docs/
├── USER_HIERARCHY_IMPLEMENTATION.md      # Full documentation
├── USER_HIERARCHY_QUICK_START.md         # Quick start guide
├── USER_HIERARCHY_SUMMARY.md             # Implementation summary
└── USER_HIERARCHY_README.md              # This file
```

## 🎓 Best Practices

### Password Management
1. ✅ Change admin password after first login
2. ✅ Use strong passwords (12+ characters)
3. ✅ Never share credentials
4. ✅ Rotate passwords regularly

### Permission Assignment
1. ✅ **Principle of least privilege** - Give only needed permissions
2. ✅ Review permissions quarterly
3. ✅ Remove permissions when staff changes roles
4. ✅ Delete users when they leave (username can be reused)

### User Management
1. ✅ Use descriptive usernames (e.g., `john_sales`, `mary_inventory`)
2. ✅ Keep email addresses up to date
3. ✅ Delete users when they leave the company
4. ✅ Usernames can be reused after deletion
5. ✅ Document special permissions

### Dashboard & Viewing Access
1. ✅ All staff can view dashboard statistics (read-only)
2. ✅ All staff can view all sections (leads, vehicles, etc.)
3. ✅ Permissions only control **editing** capabilities
4. ✅ Staff without permissions see read-only banners

## 🚦 Migration from Old System

### What Changed

**Before:**
- Environment variable authentication
- Single admin user
- `req.session.isAuthenticated = true`

**After:**
- Database-backed users
- Multiple user types with roles
- `req.session.user = { ...user object }`
- Hierarchical permissions

### Breaking Changes
1. Session structure changed (`isAuthenticated` → `user`)
2. Login response includes `user` object
3. `/api/auth/me` returns `{ authenticated, user }`
4. Middleware checks `req.session.user` instead of `req.session.isAuthenticated`

### Backward Compatibility
✅ End users (public website visitors) unaffected

## 📖 Additional Documentation

- **Full Documentation:** [USER_HIERARCHY_IMPLEMENTATION.md](USER_HIERARCHY_IMPLEMENTATION.md)
- **Quick Start:** [USER_HIERARCHY_QUICK_START.md](USER_HIERARCHY_QUICK_START.md)
- **Summary:** [USER_HIERARCHY_SUMMARY.md](USER_HIERARCHY_SUMMARY.md)
- **Permission Fix:** [FRONTEND_PERMISSION_FIX.md](FRONTEND_PERMISSION_FIX.md) - Read-only access implementation
- **Dashboard Fix:** [DASHBOARD_READONLY_FIX.md](DASHBOARD_READONLY_FIX.md) - Dashboard visibility for all users
- **User Deletion:** [USER_DELETION_HARD_DELETE.md](USER_DELETION_HARD_DELETE.md) - Hard delete implementation

## ✅ Success Checklist

- [x] Database migration completed
- [x] Admin user seeded
- [x] Backend routes implemented
- [x] Frontend UI created
- [x] Authentication working
- [x] RBAC functional
- [x] Multi-tenancy enforced
- [x] Permissions operational
- [x] Frontend permission enforcement added
- [x] Dashboard visible to all users
- [x] Hard delete implemented
- [x] Read-only access for viewing data
- [x] Edit permissions for modifying data
- [x] Tests passing
- [x] Documentation complete

## 🔄 Recent Updates (January 2026)

### Permission Model Enhancement
- ✅ Changed from "view requires permission" to "view is free, edit requires permission"
- ✅ All staff can now view all sections (read-only)
- ✅ Dashboard statistics visible to all authenticated users
- ✅ Edit buttons/actions hidden if user lacks permission

### User Deletion Update
- ✅ Changed from soft delete (deactivate) to hard delete
- ✅ Deleted users completely removed from database
- ✅ Usernames can be reused immediately after deletion
- ✅ Confirmation warns: "This action cannot be undone"

### Frontend Improvements
- ✅ Permission utilities (`hasPermission`, `canManageUsers`)
- ✅ Unauthorized component for blocked access
- ✅ Read-only banners in sections without edit permission
- ✅ Navigation links only show for editable sections
- ✅ Status dropdowns disabled without permission
- ✅ Delete buttons hidden without permission

## 🎉 Conclusion

The hierarchical user management system is **fully implemented and ready to use**!

**Next Steps:**
1. Start backend: `cd backend && npm run dev`
2. Login as admin
3. Change admin password
4. Create dealership owners
5. Let owners create staff
6. Assign appropriate permissions

**Default Login:**
- Username: `admin`
- Password: `admin123`

🔐 **Remember to change the password!**
