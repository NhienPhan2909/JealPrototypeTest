# User Hierarchy System - Quick Start Guide

## What Was Added?

A complete hierarchical user management system with role-based access control:

- **Admin** (Super Admin) - Full access to all dealerships, can delete any user
- **Dealership Owner** - Full access to their dealership, can manage staff, can delete staff
- **Dealership Staff** - Can view all data (read-only), can edit only sections with permission
- **End User** - Public website visitors (unchanged)

**Key Features:**
- ✅ Read-only by default (all users can view data)
- ✅ Permissions control editing capabilities
- ✅ Dashboard visible to all authenticated users
- ✅ Hard delete (users permanently removed, usernames can be reused)

## Quick Setup (5 Steps)

### 1. Database Migration ✅ DONE
```bash
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend/db/migrations/009_add_user_hierarchy.sql
```

### 2. Install Dependencies ✅ DONE
```bash
cd backend
npm install bcrypt
```

### 3. Seed Admin User ✅ DONE
```bash
node backend/db/migrations/seed_admin.js
```

**Default Admin Credentials:**
- Username: `admin`
- Password: `admin123`
- Email: `admin@example.com`

⚠️ **IMPORTANT:** Change this password after first login!

### 4. Restart Backend
```bash
cd backend
npm run dev
```

### 5. Test Login
1. Navigate to `http://localhost:3000/admin/login`
2. Login with username `admin` and password `admin123`
3. You should see the admin dashboard

## Using the System

### As Admin

#### Create a Dealership Owner
1. Go to `/admin/users`
2. Click "Create User"
3. Fill in:
   - Username: e.g., `owner_dealership1`
   - Password: Set a secure password
   - Email: Owner's email
   - Full Name: Owner's name
   - User Type: **Dealership Owner**
   - Dealership: Select from dropdown
4. Click "Create User"

#### Manage All Users
- View all users across all dealerships
- Edit any user's details
- Delete any user account (except yourself)

### As Dealership Owner

When you log in as a dealership owner:
- You'll automatically be scoped to your dealership
- You can only see/edit your dealership's data
- You can manage your staff members

#### Create Staff Members
1. Go to `/admin/users`
2. Click "Create User"
3. Fill in staff details
4. Select permissions (what staff can EDIT):
   - ✅ **Manage Leads** - Can edit lead status and delete leads
   - ✅ **Manage Sales Requests** - Can edit request status and delete requests
   - ✅ **Manage Vehicles** - Can create, edit, and delete vehicles
   - ✅ **Manage Blogs** - Can create, edit, and delete blog posts
   - ✅ **Edit Dealership Settings** - Can edit dealership settings
5. Click "Create User"

**Note:** Staff can VIEW all sections regardless of permissions. Permissions only control what they can EDIT.

#### Manage Your Staff
- View all staff for your dealership
- Edit staff permissions
- Delete staff accounts (username becomes available for reuse)

### As Dealership Staff

When you log in as staff:
- ✅ You can VIEW **everything** for your dealership (dashboard, leads, vehicles, blogs, etc.)
- ✅ You can EDIT only sections you have permission for
- ✅ Sections you can't edit show a "View Only" banner
- ✅ Edit buttons are hidden for sections without permission
- ❌ If you have NO permissions, everything is read-only

## Permission Examples

### Full Access Staff
Permissions: `["leads", "sales_requests", "vehicles", "blogs", "settings"]`
- ✅ Can view everything
- ✅ Can edit everything (like an owner)
- ✅ Navigation shows all sections
- ✅ No read-only banners

### Sales Manager
Permissions: `["leads", "sales_requests"]`
- ✅ Can view: Dashboard, all leads, all sales requests, all vehicles, all blogs, all settings
- ✅ Can edit: Leads and Sales Requests
- ✅ Navigation shows: All sections (Dashboard, Lead Inbox, Vehicle Manager, Blog Manager, Settings, Sales Requests)
- ⚠️ Read-only banners shown in: Vehicle Manager, Blog Manager, Settings

### Inventory Manager
Permissions: `["vehicles"]`
- ✅ Can view: Dashboard, all data
- ✅ Can edit: Vehicle inventory only
- ✅ Navigation shows: All sections
- ⚠️ Read-only banners shown in: Lead Inbox, Blog Manager, Settings, Sales Requests

### Marketing Staff
Permissions: `["blogs"]`
- ✅ Can view: Dashboard, all data
- ✅ Can edit: Blog posts only
- ✅ Navigation shows: All sections
- ⚠️ Read-only banners shown in: Lead Inbox, Vehicle Manager, Settings, Sales Requests

### Read-Only Staff (New Hire / Trainee)
Permissions: `[]` (empty array)
- ✅ Can view: Dashboard (full statistics), all leads, all vehicles, all blogs, all settings, all data
- ✅ Navigation shows: All sections (except User Management)
- ❌ Cannot edit anything
- ⚠️ Shows "View Only" banners in all sections

## API Changes

### Old Auth (DEPRECATED)
```javascript
// Old way - environment variables
if (username === process.env.ADMIN_USERNAME && password === process.env.ADMIN_PASSWORD) {
  req.session.isAuthenticated = true;
}
```

### New Auth (CURRENT)
```javascript
// New way - database users with roles
const user = await userDb.findByUsername(username);
const isValid = await userDb.verifyPassword(password, user.password_hash);
req.session.user = userWithoutPassword;
```

### Session Structure

**Old:**
```javascript
req.session.isAuthenticated = true
```

**New:**
```javascript
req.session.user = {
  id: 1,
  username: "admin",
  email: "admin@example.com",
  full_name: "System Administrator",
  user_type: "admin",
  dealership_id: null,
  permissions: []
}
```

## Testing

### Test Admin Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

Expected response:
```json
{
  "success": true,
  "message": "Logged in successfully",
  "user": {
    "id": 1,
    "username": "admin",
    "user_type": "admin",
    ...
  }
}
```

### Test User Creation
```bash
# First login to get session cookie, then:
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -b "your-session-cookie" \
  -d '{
    "username": "test_owner",
    "password": "password123",
    "email": "test@example.com",
    "full_name": "Test Owner",
    "user_type": "dealership_owner",
    "dealership_id": 1
  }'
```

## Troubleshooting

### Can't Login
- Check database is running: `docker ps | grep jeal-prototype-db`
- Verify admin user exists: `docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT * FROM app_user;"`
- Check backend logs for errors

### "Username already exists" Error
- Old inactive users may still be in database
- Delete inactive users: `docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "DELETE FROM app_user WHERE is_active = false;"`
- After cleanup, usernames can be reused

### Migration Fails
- Ensure database is running
- Check if table already exists: `docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "\dt"`
- Drop and recreate if needed: `DROP TABLE app_user CASCADE;`

### "bcrypt not found" Error
```bash
cd backend
npm install bcrypt
```

### Session Not Persisting
- Check `SESSION_SECRET` is set in `.env`
- Verify cookies are enabled in browser
- Check for CORS issues in browser console

### Staff Can't See Dashboard
- **Fixed!** Dashboard is now visible to all authenticated users
- All staff can view all data (read-only)
- Permissions only control editing

### Staff Can Edit Everything
- **Fixed!** Permissions now properly enforced on frontend
- Edit buttons hidden without permission
- Status dropdowns disabled without permission
- Read-only banners shown in restricted sections

## Security Best Practices

1. **Change Default Password**
   - Login as admin immediately
   - Go to user management
   - Update admin password

2. **Use Strong Passwords**
   - Minimum 6 characters (enforced)
   - Recommend 12+ characters with mixed case, numbers, symbols

3. **Limit Permissions**
   - Give staff ONLY the permissions they need
   - Review permissions regularly
   - Remember: all staff can VIEW, permissions control EDIT

4. **Delete Unused Accounts**
   - Delete users when they leave the company
   - Username becomes available immediately
   - Permanent deletion (no soft delete)
   - Confirmation warns: "This action cannot be undone"

## Next Steps

1. ✅ Complete setup steps above
2. Login as admin and change password
3. Create dealership owner accounts for each dealership
4. Let owners create their own staff
5. Assign appropriate permissions to staff
6. Test each user type to verify permissions
7. Remember: Staff can VIEW everything, permissions control EDIT

## Recent Updates (January 2026)

### Permission Model Change
- ✅ All authenticated users can now VIEW all data (read-only)
- ✅ Permissions only control EDIT capabilities
- ✅ Dashboard visible to all users
- ✅ Navigation shows only editable sections

### User Deletion Change
- ✅ Changed from soft delete to hard delete
- ✅ Users permanently removed from database
- ✅ Usernames can be reused immediately
- ✅ Cleaner database with no inactive records

## Support

See full documentation:
- **Complete Guide:** `USER_HIERARCHY_README.md`
- **Implementation Details:** `USER_HIERARCHY_IMPLEMENTATION.md`
- **Permission Fix:** `FRONTEND_PERMISSION_FIX.md`
- **Dashboard Fix:** `DASHBOARD_READONLY_FIX.md`
- **Deletion Info:** `USER_DELETION_HARD_DELETE.md`

Key files:
- Backend: `backend/routes/users.js`, `backend/db/users.js`
- Frontend: `frontend/src/pages/admin/UserManagement.jsx`
- Utilities: `frontend/src/utils/permissions.js`
- Middleware: `backend/middleware/auth.js`
- Database: `backend/db/users.js`
