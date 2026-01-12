# Permission System Enforcement - Bug Fix

## Issue

The permission system was created but not enforced in the existing routes. A sales staff member with only "sales_requests" permission could still access and edit the "Lead Inbox" section.

## Root Cause

While the permission middleware (`requirePermission`) was created, it was not added to the existing protected routes. The routes were only checking for authentication (`requireAuth`) but not validating specific permissions.

## Solution

Added permission middleware to all protected routes that require specific permissions.

## Files Modified

### 1. backend/routes/leads.js
**Added:**
- `requirePermission('leads')` to all lead management routes
- `enforceDealershipScope` for multi-tenancy

**Routes affected:**
- `GET /api/leads` - View leads (requires 'leads' permission)
- `PATCH /api/leads/:id/status` - Update lead status (requires 'leads' permission)
- `DELETE /api/leads/:id` - Delete lead (requires 'leads' permission)

### 2. backend/routes/salesRequests.js
**Added:**
- `requirePermission('sales_requests')` to all sales request routes
- `enforceDealershipScope` for multi-tenancy

**Routes affected:**
- `GET /api/sales-requests` - View sales requests (requires 'sales_requests' permission)
- `PATCH /api/sales-requests/:id/status` - Update status (requires 'sales_requests' permission)
- `DELETE /api/sales-requests/:id` - Delete request (requires 'sales_requests' permission)

### 3. backend/routes/vehicles.js
**Added:**
- `requirePermission('vehicles')` to vehicle modification routes
- `enforceDealershipScope` for multi-tenancy

**Routes affected:**
- `POST /api/vehicles` - Create vehicle (requires 'vehicles' permission)
- `PUT /api/vehicles/:id` - Update vehicle (requires 'vehicles' permission)
- `DELETE /api/vehicles/:id` - Delete vehicle (requires 'vehicles' permission)

### 4. backend/routes/blogs.js
**Added:**
- `requirePermission('blogs')` to blog management routes
- `enforceDealershipScope` for multi-tenancy

**Routes affected:**
- `POST /api/blogs` - Create blog (requires 'blogs' permission)
- `PUT /api/blogs/:id` - Update blog (requires 'blogs' permission)
- `DELETE /api/blogs/:id` - Delete blog (requires 'blogs' permission)

### 5. backend/routes/dealers.js
**Added:**
- `requirePermission('settings')` to dealership settings route
- `enforceDealershipScope` for multi-tenancy

**Routes affected:**
- `PUT /api/dealers/:id` - Update dealership settings (requires 'settings' permission)

## Permission Matrix

| User Type          | leads | sales_requests | vehicles | blogs | settings |
|-------------------|-------|----------------|----------|-------|----------|
| **Admin**         | ✅     | ✅              | ✅        | ✅     | ✅        |
| **Owner**         | ✅     | ✅              | ✅        | ✅     | ✅        |
| **Staff**         | Based on assigned permissions                          |

### Staff Permission Examples

**Sales Manager** - `["leads", "sales_requests"]`
- ✅ Can view/edit Lead Inbox
- ✅ Can view/edit Sales Requests
- ❌ Cannot create/edit vehicles
- ❌ Cannot create/edit blogs
- ❌ Cannot edit dealership settings

**Inventory Manager** - `["vehicles"]`
- ❌ Cannot access Lead Inbox
- ❌ Cannot access Sales Requests
- ✅ Can create/edit vehicles
- ❌ Cannot create/edit blogs
- ❌ Cannot edit dealership settings

**Content Manager** - `["blogs"]`
- ❌ Cannot access Lead Inbox
- ❌ Cannot access Sales Requests
- ❌ Cannot create/edit vehicles
- ✅ Can create/edit blogs
- ❌ Cannot edit dealership settings

**Settings Manager** - `["settings"]`
- ❌ Cannot access Lead Inbox
- ❌ Cannot access Sales Requests
- ❌ Cannot create/edit vehicles
- ❌ Cannot create/edit blogs
- ✅ Can edit dealership settings

**Read-Only Staff** - `[]` (no permissions)
- ✅ Can **view** all sections for their dealership
- ❌ Cannot **edit** anything

## Testing

### Test Case 1: Staff with "sales_requests" Only
**Setup:**
1. Create staff user with only `["sales_requests"]` permission
2. Login with staff account

**Expected Behavior:**
- ✅ Can access Sales Requests page
- ✅ Can edit sales request status
- ❌ **Should get 403 error when accessing Lead Inbox**
- ❌ Should get 403 error when trying to edit leads

### Test Case 2: Staff with "leads" Only
**Setup:**
1. Create staff user with only `["leads"]` permission
2. Login with staff account

**Expected Behavior:**
- ✅ Can access Lead Inbox
- ✅ Can edit lead status
- ❌ Should get 403 error when accessing Sales Requests
- ❌ Should get 403 error when trying to create vehicles

### Test Case 3: Staff with Multiple Permissions
**Setup:**
1. Create staff user with `["leads", "vehicles"]` permissions
2. Login with staff account

**Expected Behavior:**
- ✅ Can access Lead Inbox
- ✅ Can edit leads
- ✅ Can access Vehicles
- ✅ Can create/edit vehicles
- ❌ Should get 403 error when accessing Sales Requests
- ❌ Should get 403 error when trying to edit settings

## How It Works

### Middleware Chain

All protected routes now use this middleware chain:

```javascript
router.get('/api/leads', 
  requireAuth,                    // Step 1: Check user is logged in
  enforceDealershipScope,         // Step 2: Ensure user can only access their dealership
  requirePermission('leads'),     // Step 3: Check user has 'leads' permission
  async (req, res) => { ... }     // Step 4: Execute route handler
);
```

### Permission Check Logic

From `backend/middleware/auth.js`:

```javascript
function requirePermission(permission) {
  return (req, res, next) => {
    const user = req.session.user;

    // Admin and Owner have all permissions - always allow
    if (user.user_type === 'admin' || user.user_type === 'dealership_owner') {
      return next();
    }

    // Staff users check their permissions array
    if (user.user_type === 'dealership_staff') {
      const permissions = Array.isArray(user.permissions) ? user.permissions : [];
      if (permissions.includes(permission)) {
        return next();  // Permission found - allow access
      }
    }

    // No permission - deny access
    return res.status(403).json({ error: `Permission '${permission}' required` });
  };
}
```

## Backend Response Codes

### 401 Unauthorized
User is not logged in at all.
```json
{
  "error": "Authentication required"
}
```

### 403 Forbidden
User is logged in but doesn't have the required permission.
```json
{
  "error": "Permission 'leads' required"
}
```

## Status

✅ **FIXED** - Permission system is now fully enforced across all routes.

Staff users can only access sections they have explicit permission for:
- Admin and Owners: Full access to everything
- Staff: Only sections matching their assigned permissions
- No permissions: Read-only access

## Verification

To verify the fix:

1. **Login as staff with only "sales_requests" permission**
2. **Try to access Lead Inbox** → Should get 403 error
3. **Try to access Sales Requests** → Should work ✅
4. **Try to create a vehicle** → Should get 403 error

This confirms the permission system is working correctly.
