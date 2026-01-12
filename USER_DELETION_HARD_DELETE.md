# User Deletion - Changed from Soft Delete to Hard Delete

## Issue

When a dealership owner deactivated a staff account (e.g., username "Acme Sales"), the user was soft-deleted (marked as inactive in database). When trying to recreate a staff account with the same username, it showed error "Username already exists".

## Root Cause

The system was using **soft delete** (setting `is_active = false`):
- User remained in database
- Username was still marked as UNIQUE
- Could not reuse the username

## Solution

Changed from **soft delete** to **hard delete**:
- User is permanently removed from database
- Username becomes available for reuse
- Simpler and matches user expectations

## Changes Made

### 1. Backend Database Function (`backend/db/users.js`)

**Before (Soft Delete):**
```javascript
async function deactivate(userId) {
  const query = `
    UPDATE app_user
    SET is_active = false, updated_at = NOW()
    WHERE id = $1
  `;
  // ...
}
```

**After (Hard Delete):**
```javascript
async function deactivate(userId) {
  const query = `
    DELETE FROM app_user
    WHERE id = $1
  `;
  // ...
}
```

### 2. Backend Route (`backend/routes/users.js`)

**Updated:**
- Changed comments from "deactivate" to "delete"
- Changed success message to "User deleted successfully"
- Changed variable names from `canDeactivate` to `canDelete`

### 3. Frontend (`frontend/src/pages/admin/UserManagement.jsx`)

**Updated:**
- Button text: "Deactivate" → "Delete"
- Confirmation message: "Are you sure you want to delete this user? This action cannot be undone."
- Error messages: "deactivate" → "delete"

## Impact

### Before
1. Owner deactivates staff "Acme Sales"
2. User still in database with `is_active = false`
3. Try to create new user "Acme Sales"
4. ❌ Error: "Username already exists"

### After
1. Owner deletes staff "Acme Sales"
2. User completely removed from database
3. Try to create new user "Acme Sales"
4. ✅ Success: New user created

## Files Modified

1. ✅ `backend/db/users.js` - Changed deactivate() to DELETE instead of UPDATE
2. ✅ `backend/routes/users.js` - Updated comments and messages
3. ✅ `frontend/src/pages/admin/UserManagement.jsx` - Updated UI text

## Database Constraints

The database schema has `ON DELETE SET NULL` for the `created_by` field:
```sql
created_by INTEGER REFERENCES app_user(id) ON DELETE SET NULL
```

**This means:**
- When a user is deleted, any users they created will have `created_by` set to NULL
- No orphaned records - referential integrity is maintained
- Audit trail partially preserved (you know someone created them, but not who)

## Security & Authorization

**Who can delete users:**
- ✅ **Admin:** Can delete any user
- ✅ **Dealership Owner:** Can delete staff from their dealership only
- ❌ **Dealership Staff:** Cannot delete users
- ❌ **Any user:** Cannot delete themselves

## Testing

### Test Case 1: Delete and Recreate Username
1. Login as dealership owner
2. Create staff user with username "Acme Sales"
3. Delete the staff user
4. Create new staff user with same username "Acme Sales"
5. ✅ **Expected:** Success - new user created

### Test Case 2: Cannot Delete Self
1. Login as any user
2. Try to delete your own account
3. ✅ **Expected:** Error - "Cannot delete your own account"

### Test Case 3: Owner Can Only Delete Their Staff
1. Login as Owner of Dealership A
2. Try to delete staff from Dealership B
3. ✅ **Expected:** Error - "Access denied"

## Status

✅ **FIXED** - Users are now permanently deleted from database, allowing username reuse.

**User experience:**
- Clear warning: "This action cannot be undone"
- Immediate username availability after deletion
- Clean database (no inactive user records)

## Alternative (Not Used)

We could have kept soft delete and used a **partial unique index**:
```sql
CREATE UNIQUE INDEX idx_user_username_active 
ON app_user(username) WHERE is_active = true;
```

This would allow username reuse while keeping historical records.

**Why we chose hard delete:**
- Simpler implementation
- Matches user expectations ("delete" = gone)
- No accumulation of inactive records
- GDPR compliance (right to be forgotten)
- Cleaner database
