# User Hierarchy System - Implementation Guide

## Overview

This implementation adds a hierarchical user system with 4 user types to replace the simple admin/end-user model:

1. **Admin** - Super admin with full access to all dealerships
2. **Dealership Owner** - Full access to their specific dealership, can manage staff
3. **Dealership Staff** - Limited access based on assigned permissions
4. **End User** - Public website visitors (no authentication needed)

## Database Schema

### New Table: `app_user`

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
  updated_at TIMESTAMP DEFAULT NOW()
);
```

### Permissions Structure

For `dealership_staff` users, the `permissions` field is a JSONB array of strings:

```json
["leads", "vehicles", "settings"]
```

Available permissions:
- `leads` - Manage lead inbox
- `sales_requests` - Manage sales requests  
- `vehicles` - Manage vehicle inventory
- `blogs` - Manage blog posts
- `settings` - Edit dealership settings

## User Hierarchy & Permissions

### Admin
- Can access and edit **all dealerships**
- Can create/manage dealership owner accounts
- Can view all users across all dealerships
- No `dealership_id` (must be NULL)

### Dealership Owner
- Full access to **their specific dealership**
- Can create/manage staff accounts for their dealership
- Can view and edit all settings for their dealership
- Must have a `dealership_id`

### Dealership Staff
- Can view everything in **their dealership**
- Can only edit sections based on assigned `permissions`
- Cannot create other user accounts
- Must have a `dealership_id`

### End User
- Public website visitors
- No database record needed
- No authentication required

## API Endpoints

### Authentication Routes (`/api/auth`)

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
Get current authenticated user info.

**Response:**
```json
{
  "authenticated": true,
  "user": { /* user object */ }
}
```

### User Management Routes (`/api/users`)

#### GET /api/users
Get all users (filtered by role).
- **Admin**: Gets all users
- **Owner**: Gets users from their dealership only

#### GET /api/users/:id
Get specific user by ID.

#### POST /api/users
Create new user.
- **Admin**: Can create `dealership_owner` or `admin` accounts
- **Owner**: Can create `dealership_staff` accounts for their dealership

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
Update user information (email, full_name, permissions).

#### PUT /api/users/:id/password
Update user password.

**Request:**
```json
{
  "password": "newpassword123"
}
```

#### DELETE /api/users/:id
Deactivate user (soft delete).

## Middleware

### Authentication Middleware (`backend/middleware/auth.js`)

#### `requireAuth(req, res, next)`
Requires any authenticated user.

#### `requireAdmin(req, res, next)`
Requires admin user type.

#### `requireOwner(req, res, next)`
Requires dealership owner user type.

#### `requireAdminOrOwner(req, res, next)`
Requires admin OR dealership owner.

#### `enforceDealershipScope(req, res, next)`
Ensures users can only access their own dealership data.
- Admin users bypass this check
- Owner/Staff can only access their `dealership_id`

#### `requirePermission(permission)`
Checks if user has specific permission.
- Admin and Owner always have permission
- Staff must have permission in their `permissions` array

**Usage:**
```javascript
router.get('/api/leads', 
  requireAuth, 
  enforceDealershipScope, 
  requirePermission('leads'),
  getLeads
);
```

## Frontend Components

### AdminContext Updates

Now includes:
- `user` - Full user object from session
- `setUser` - Update user state
- Auto-selects dealership for non-admin users

### User Management Page

Located at: `/admin/users`

Features:
- List all users (filtered by role)
- Create new user accounts
- Edit user details and permissions
- Deactivate users
- Permission checkboxes for staff users

Access:
- **Admin**: Can manage all dealership owners
- **Dealership Owner**: Can manage their staff only

## Installation & Setup

### 1. Run Database Migrations

```bash
# Run the user hierarchy migration
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend/db/migrations/009_add_user_hierarchy.sql
```

### 2. Install Dependencies

```bash
cd backend
npm install bcrypt
```

### 3. Seed Default Admin User

```bash
# Create initial admin account
node backend/db/migrations/seed_admin.js
```

This creates:
- **Username:** admin
- **Password:** admin123
- **Email:** admin@example.com

⚠️ **Change the admin password immediately after first login!**

### 4. Restart Backend Server

```bash
cd backend
npm run dev
```

## Usage Examples

### Creating a Dealership Owner (as Admin)

1. Login as admin at `/admin/login`
2. Navigate to `/admin/users`
3. Click "Create User"
4. Fill in:
   - Username: `john_owner`
   - Password: `password123`
   - Email: `john@dealership1.com`
   - Full Name: `John Smith`
   - User Type: `Dealership Owner`
   - Dealership: Select from dropdown
5. Click "Create User"

### Creating Staff (as Dealership Owner)

1. Login as dealership owner
2. Navigate to `/admin/users`
3. Click "Create User"
4. Fill in user details
5. Select permissions:
   - ✅ Manage Leads
   - ✅ Manage Vehicles
   - ⬜ Edit Dealership Settings
6. Click "Create User"

### Staff Permissions Example

A staff member with `["leads"]` permission:
- ✅ Can view lead inbox
- ✅ Can update lead status
- ✅ Can view vehicles
- ❌ Cannot create/edit vehicles
- ❌ Cannot edit dealership settings

## Security Considerations

1. **Password Hashing**: All passwords are hashed with bcrypt (10 salt rounds)
2. **Session-based Auth**: User data stored in server-side session
3. **Role-based Access Control**: Middleware enforces permissions
4. **Dealership Isolation**: Non-admin users can only access their dealership
5. **Soft Deletes**: Users are deactivated, not deleted (audit trail)

## Migration from Old System

The old authentication system used:
- Environment variables (`ADMIN_USERNAME`, `ADMIN_PASSWORD`)
- Simple `req.session.isAuthenticated` flag

The new system:
- Uses database-backed user accounts
- Stores full user object in `req.session.user`
- Provides granular role-based permissions

**Breaking Changes:**
- Old middleware `requireAuth` now checks `req.session.user` instead of `req.session.isAuthenticated`
- Login response now includes `user` object
- `/api/auth/me` returns `{ authenticated, user }` instead of just `{ authenticated }`

## Testing

### Test Admin Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

### Test User Creation (as admin)
```bash
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -b "sessionid=..." \
  -d '{
    "username": "test_owner",
    "password": "password123",
    "email": "test@example.com",
    "full_name": "Test Owner",
    "user_type": "dealership_owner",
    "dealership_id": 1
  }'
```

## Future Enhancements

Potential improvements:
- Password reset functionality
- Email verification
- Two-factor authentication
- Audit logging for user actions
- Role-based UI visibility (hide sections user can't access)
- Bulk permission management
- User activity tracking

## Support

For issues or questions:
1. Check the database logs: `docker logs jeal-prototype-db`
2. Check backend logs for auth errors
3. Verify migrations ran successfully: `docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "\dt"`

## Files Modified/Created

### Backend
- ✅ `backend/db/migrations/009_add_user_hierarchy.sql` - New user table schema
- ✅ `backend/db/migrations/seed_admin.js` - Seed script for admin user
- ✅ `backend/db/users.js` - User database functions
- ✅ `backend/middleware/auth.js` - Updated auth middleware
- ✅ `backend/routes/auth.js` - Updated auth routes
- ✅ `backend/routes/users.js` - New user management routes
- ✅ `backend/server.js` - Added user routes
- ✅ `backend/package.json` - Added bcrypt dependency

### Frontend
- ✅ `frontend/src/context/AdminContext.jsx` - Updated for user object
- ✅ `frontend/src/pages/admin/Login.jsx` - Updated to set user
- ✅ `frontend/src/pages/admin/UserManagement.jsx` - New user management UI
- ✅ `frontend/src/App.jsx` - Added user management route

## Conclusion

This hierarchical user system provides:
- ✅ Flexible role-based access control
- ✅ Multi-tenancy support with dealership isolation
- ✅ Granular permission management
- ✅ Secure authentication with bcrypt
- ✅ Scalable architecture for future enhancements
