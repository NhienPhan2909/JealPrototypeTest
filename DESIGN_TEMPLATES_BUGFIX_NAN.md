# Design Templates - Bug Fix: Template Deletion NaN Error

## Issue Description
When an admin user tries to delete a custom template, the deletion fails with the error:
```
Error deleting design template: error: invalid input syntax for type integer: "NaN"
DELETE /api/design-templates/9 500
```

**Root Cause:** The `dealership_id` query parameter was not being passed from the frontend when making DELETE (and POST) requests.

## Date
2026-01-09

## Affected Users
- Admin users attempting to delete or create custom templates

## Solution

### Frontend Changes
**File:** `frontend/src/components/admin/TemplateSelector.jsx`

#### 1. Fixed DELETE Request
**Before:**
```javascript
const response = await fetch(`/api/design-templates/${templateId}`, {
  method: 'DELETE',
  credentials: 'include'
});
```

**After:**
```javascript
// Include dealership_id for admin users
const url = `/api/design-templates/${templateId}?dealership_id=${selectedDealership.id}`;

const response = await fetch(url, {
  method: 'DELETE',
  credentials: 'include'
});
```

#### 2. Fixed POST Request
**Before:**
```javascript
body: JSON.stringify({
  name: newTemplateName.trim(),
  description: newTemplateDescription.trim() || undefined,
  theme_color: currentSettings.theme_color,
  secondary_theme_color: currentSettings.secondary_theme_color,
  body_background_color: currentSettings.body_background_color,
  font_family: currentSettings.font_family
})
```

**After:**
```javascript
body: JSON.stringify({
  dealership_id: selectedDealership.id,  // Added this line
  name: newTemplateName.trim(),
  description: newTemplateDescription.trim() || undefined,
  theme_color: currentSettings.theme_color,
  secondary_theme_color: currentSettings.secondary_theme_color,
  body_background_color: currentSettings.body_background_color,
  font_family: currentSettings.font_family
})
```

### Backend Changes
**File:** `backend/routes/designTemplates.js`

#### 1. Enhanced DELETE Route Validation
**Added:**
```javascript
if (!dealershipId || isNaN(dealershipId)) {
  return res.status(400).json({ error: 'Dealership ID is required' });
}
```

#### 2. Enhanced POST Route Validation
**Added:**
```javascript
// Validate dealership_id
if (!dealershipId || isNaN(dealershipId)) {
  return res.status(400).json({ error: 'Dealership ID is required' });
}
```

## Testing

### Verification Steps

1. **Log in as Admin**
2. **Select a dealership** from the dropdown
3. **Go to Settings**
4. **Create a custom template**
   - Expected: Template created successfully
   - Verify: Check database for new template with correct dealership_id

5. **Delete the custom template**
   - Expected: Template deleted successfully
   - Verify: Template removed from UI and database

6. **Log in as Dealership Owner**
7. **Create a custom template**
   - Expected: Works without selecting dealership
   - Verify: Uses session dealership_id

8. **Delete the template**
   - Expected: Works correctly
   - Verify: Template removed

### Expected Behavior After Fix

**Admin Users:**
- ✅ Can create templates for selected dealership
- ✅ Can delete templates for selected dealership
- ✅ Proper error message if no dealership selected
- ✅ No NaN errors

**Dealership Users:**
- ✅ Can create templates (uses session dealership_id)
- ✅ Can delete templates (uses session dealership_id)
- ✅ No changes in behavior

## Database Verification

```bash
# Check templates
docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT id, name, dealership_id, is_preset FROM design_templates ORDER BY dealership_id, name;"
```

## Error Handling Improvements

### Before
- NaN passed to database causing crash
- Generic "Failed to delete" message
- No validation on dealership_id

### After
- Dealership ID validated before database call
- Clear error message: "Dealership ID is required"
- Early return prevents NaN database errors

## Impact

**Severity:** High (feature broken for admin users)  
**Affected Routes:** POST and DELETE  
**User Impact:** Admin users could not create or delete templates  

## Status
✅ **RESOLVED**

## Files Modified

1. `frontend/src/components/admin/TemplateSelector.jsx`
   - Added `dealership_id` to DELETE URL query parameter
   - Added `dealership_id` to POST request body

2. `backend/routes/designTemplates.js`
   - Added validation for `dealership_id` in DELETE route
   - Added validation for `dealership_id` in POST route

## Prevention

To prevent similar issues in the future:
1. Always pass `dealership_id` for admin user operations
2. Validate all ID parameters before database operations
3. Test with both admin and dealership user accounts
4. Check for NaN before parseInt operations

## Related Issues
- Initial auth fix: Using `req.session.user` instead of `req.user`
- Multi-tenant support: Query parameter implementation

---

**Date:** 2026-01-09 03:40 UTC  
**Issue:** Template deletion/creation failing with NaN error  
**Fix:** Added dealership_id to frontend requests and backend validation  
**Status:** RESOLVED ✓
