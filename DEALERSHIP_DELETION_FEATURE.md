# Dealership Deletion Feature

## Overview
System Administrators can now delete existing dealerships from the CMS admin page. This feature includes strong safety measures due to the destructive nature of the operation.

## ⚠️ CRITICAL WARNING

**Deleting a dealership is IRREVERSIBLE and will CASCADE DELETE:**
- The dealership record itself
- ALL vehicles associated with the dealership
- ALL leads (customer enquiries) associated with the dealership
- ALL sales requests associated with the dealership
- ALL blog posts associated with the dealership
- ALL user accounts (owners and staff) associated with the dealership

**This data CANNOT be recovered once deleted.**

---

## What Was Added

### Backend Changes

#### 1. Database Layer (`backend/db/dealers.js`)
- **New Function**: `deleteDealership(dealershipId)`
  - Deletes a dealership record from the database
  - Uses `DELETE ... RETURNING *` to return deleted record
  - Relies on database CASCADE constraints for related data
  - Returns: Deleted dealership object or null if not found

#### 2. API Routes (`backend/routes/dealers.js`)
- **New Endpoint**: `DELETE /api/dealers/:id`
  - **Access**: Admin only (requires authentication and admin role)
  - **Purpose**: Delete dealership and all related data
  - **Security Measures**:
    - Admin-only access via `requireAuth` and `requireAdmin` middleware
    - ID validation (must be positive integer)
  - **Response**: Success message with deleted dealership details (HTTP 200)
  - **Errors**:
    - 400: Invalid dealership ID
    - 403: User is not admin
    - 404: Dealership not found
    - 500: Database error

### Frontend Changes

#### 1. Updated Page: `DealershipManagement.jsx`
- **New Function**: `handleDeleteDealership(dealershipId, dealershipName)`
  - Shows confirmation prompt with strong warning
  - Requires user to type exact dealership name to confirm
  - Calls DELETE API endpoint
  - Updates list and shows success/error message
  
- **New UI Element**: "Delete" button in Actions column
  - Red text to indicate destructive action
  - Appears for each dealership in the table
  - Triggers confirmation flow

- **Safety Features**:
  - Multi-step confirmation process
  - User must type exact dealership name
  - Clear warning about cascade deletion
  - Visual indicators (red color, warning icon)

---

## User Flow

### For System Administrators:

1. **Navigate to Dealership Management**
   - Log in to admin panel
   - Click "Dealership Management"

2. **Locate Dealership to Delete**
   - View table of all dealerships
   - Find the dealership you want to delete

3. **Click Delete Button**
   - Click red "Delete" button in Actions column
   - A prompt appears with strong warning

4. **Confirm Deletion**
   - Read the warning carefully
   - Type the exact dealership name (case-sensitive)
   - Click OK to proceed, or Cancel to abort

5. **Verification**
   - If name matches: Dealership is deleted
   - If name doesn't match: Operation is cancelled
   - Success/error message displays
   - Table updates to reflect changes

### Confirmation Dialog

```
⚠️ WARNING: This action is IRREVERSIBLE!

Deleting "Acme Auto Sales" will permanently delete:
• The dealership record
• ALL vehicles
• ALL leads and sales requests
• ALL blog posts
• ALL user accounts (owners and staff)

This data CANNOT be recovered.

Type the dealership name to confirm deletion:
```

User must type: `Acme Auto Sales` (exact match required)

---

## Security

### Backend Security
1. **Authentication**: Requires valid admin session
2. **Authorization**: Checks `user_type === 'admin'`
3. **ID Validation**: Ensures valid positive integer
4. **Database Constraints**: Relies on ON DELETE CASCADE

### Frontend Security
1. **Confirmation Required**: User must type exact name
2. **Warning Messages**: Clear indication of consequences
3. **Access Control**: Only admins see delete buttons
4. **Error Handling**: Graceful failure with user feedback

---

## Database Cascade Behavior

The dealership table has `ON DELETE CASCADE` constraints that automatically delete related records:

```sql
-- From schema.sql
CREATE TABLE vehicle (
  ...
  dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE
);

CREATE TABLE lead (
  ...
  dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE
);

CREATE TABLE sales_request (
  ...
  dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE
);

CREATE TABLE blog (
  ...
  dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE
);

CREATE TABLE app_user (
  ...
  dealership_id INTEGER REFERENCES dealership(id) ON DELETE CASCADE
);
```

When a dealership is deleted, PostgreSQL automatically:
1. Deletes all vehicles with that `dealership_id`
2. Deletes all leads with that `dealership_id`
3. Deletes all sales_requests with that `dealership_id`
4. Deletes all blog posts with that `dealership_id`
5. Deletes all users (owners/staff) with that `dealership_id`

---

## Testing

### Manual Testing
1. Start backend and frontend servers
2. Log in as admin
3. Create a test dealership
4. Click "Delete" button
5. Cancel first attempt (wrong name)
6. Try again with correct name
7. Verify dealership is removed

### Automated Testing
Run the test script:
```bash
node test_dealership_deletion.js
```

This script:
1. Logs in as admin
2. Creates a test dealership
3. Verifies it exists
4. Deletes the dealership
5. Verifies it no longer exists

---

## UI Components

### Delete Button
- **Location**: Actions column in dealerships table
- **Appearance**: Red text, "Delete" label
- **Behavior**: Triggers confirmation dialog

### Confirmation Dialog
- **Type**: Browser `prompt()` dialog
- **Purpose**: Strong warning and name verification
- **Input**: User must type exact dealership name
- **Cancel**: Click Cancel or type wrong name

### Success Message
```
✅ Successfully deleted dealership: [Name]
```

### Error Messages
```
❌ Deletion cancelled: Dealership name did not match
❌ Failed to delete dealership
❌ Dealership not found
```

---

## Files Modified

### Backend
- `backend/db/dealers.js` - Added `deleteDealership()` function
- `backend/routes/dealers.js` - Added DELETE endpoint

### Frontend
- `frontend/src/pages/admin/DealershipManagement.jsx` - Added delete button and handler

### Testing
- `test_dealership_deletion.js` - **NEW FILE**

---

## API Reference

### DELETE /api/dealers/:id
Delete a dealership and all related data (admin only).

**Headers:**
```
Cookie: connect.sid=... (session cookie)
```

**URL Parameters:**
- `id` (integer): Dealership ID to delete

**Success Response (200 OK):**
```json
{
  "message": "Dealership deleted successfully",
  "dealership": {
    "id": 3,
    "name": "Test Auto Sales",
    "address": "123 Main St",
    "phone": "(555) 123-4567",
    "email": "info@testautosales.com",
    ...
  }
}
```

**Error Responses:**
- **400 Bad Request**: Invalid dealership ID
  ```json
  { "error": "Dealership ID must be a valid positive number" }
  ```

- **403 Forbidden**: User is not admin
  ```json
  { "error": "Admin access required" }
  ```

- **404 Not Found**: Dealership doesn't exist
  ```json
  { "error": "Dealership not found" }
  ```

- **500 Internal Server Error**: Database error
  ```json
  { "error": "Failed to delete dealership" }
  ```

---

## Best Practices

### Before Deleting
1. **Export Important Data**: Save any data you might need later
2. **Notify Users**: Inform dealership owner and staff
3. **Check Dependencies**: Understand what will be deleted
4. **Consider Alternatives**: Could you deactivate instead?

### When Deleting
1. **Double-Check**: Verify you're deleting the correct dealership
2. **Read Warning**: Understand the consequences
3. **Type Carefully**: Exact name match required
4. **Confirm Action**: Only proceed if you're certain

### After Deleting
1. **Verify Removal**: Check that dealership is gone
2. **Update Documentation**: Note the deletion
3. **Inform Stakeholders**: Notify relevant parties

---

## Frequently Asked Questions

### Q: Can I recover a deleted dealership?
**A:** No. Deletion is permanent and irreversible. All data is lost.

### Q: What if I delete by accident?
**A:** There is no undo. The confirmation dialog requires typing the exact name to prevent accidents, but once confirmed, the data is gone forever.

### Q: Can I soft-delete instead?
**A:** Not currently. This feature performs hard deletion. Consider adding a soft-delete feature if you need to preserve data.

### Q: Will it delete dealerships with active content?
**A:** Yes. The delete operation doesn't check for active vehicles, leads, or blog posts. Everything is deleted.

### Q: What happens to dealership owner accounts?
**A:** They are deleted due to CASCADE constraints. Users will no longer be able to log in.

### Q: Can dealership owners delete their own dealership?
**A:** No. Only System Administrators (`user_type: 'admin'`) can delete dealerships.

---

## Troubleshooting

### Delete button not showing?
- Ensure you're logged in as admin
- Check that the Actions column is visible
- Verify page permissions

### Confirmation dialog not appearing?
- Check browser popup blockers
- Try a different browser
- Review browser console for errors

### Deletion fails with 403 error?
- Verify you're logged in as admin
- Session may have expired - log out and log in again

### Deletion succeeds but dealership still visible?
- Refresh the page
- Clear browser cache
- Check that you're looking at the correct page

---

## Summary

✅ System Administrators can delete dealerships
✅ Strong confirmation process prevents accidents
✅ Cascade deletion removes all related data
✅ Clear warnings about consequences
✅ Admin-only access with proper security
✅ Success/error messaging
✅ Automated testing provided

**Remember**: Deletion is permanent. Use with extreme caution.
