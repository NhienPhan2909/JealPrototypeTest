# Font Customization Troubleshooting Guide

## Issue: Font changes don't save (reverts to system default)

### Root Cause
The backend API route (`backend/routes/dealers.js`) was missing the `font_family` field in the PUT request handler.

### ✅ Solution Applied
Updated `backend/routes/dealers.js` to:
1. Extract `font_family` from request body
2. Pass `font_family` to the database update function
3. Add field length validation for `font_family`

## Steps to Fix (ALREADY DONE)

The following changes have been made to fix the issue:

### 1. Backend Route Updated ✅
File: `backend/routes/dealers.js`

**Added:**
- `font_family` to FIELD_LIMITS (line 35)
- `font_family` extraction from req.body (line 184)
- `font_family` in update call (line 232)
- JSDoc documentation for font_family parameter (line 172)

### 2. Test Script Created ✅
File: `test_font_api.js`

A test script to verify the API is working correctly.

## 🚀 What You Need To Do Now

### Step 1: Restart Backend Server

**Option A: If backend is running in a terminal**
1. Go to the terminal running the backend
2. Press `Ctrl+C` to stop the server
3. Run: `npm start`

**Option B: If backend is not running**
1. Open terminal/command prompt
2. Navigate to backend folder: `cd backend`
3. Run: `npm start`

### Step 2: Verify Backend is Running

Check that you see:
```
Server running on port 3000
Database connected successfully
```

### Step 3: Test the API (Optional)

Run the test script:
```bash
node test_font_api.js
```

Expected output:
```
✅ GET request successful
✅ UPDATE request successful - font changed to "times"
✅ Change persisted successfully!
✅ All tests passed!
```

### Step 4: Test in Admin UI

1. **Open admin panel** in browser
2. **Clear browser cache** (Ctrl+Shift+Delete or Cmd+Shift+Delete)
3. **Go to Dealership Settings**
4. **Change font** to "Times New Roman"
5. **Click "Save Settings"**
6. **Check for success message**: "Dealership settings updated successfully!"
7. **Refresh the page** - font dropdown should show "Times New Roman"
8. **Visit public website** - all text should use Times New Roman

## Verification Checklist

Run through this checklist to confirm everything is working:

- [ ] Backend server restarted successfully
- [ ] No errors in backend console logs
- [ ] Admin panel loads without errors
- [ ] Can access Dealership Settings page
- [ ] Font dropdown shows all 10 font options
- [ ] Preview box updates when selecting different fonts
- [ ] "Save Settings" button works
- [ ] Success message appears after saving
- [ ] Font selection persists after page refresh
- [ ] Public website displays selected font
- [ ] Font applies to all text elements

## Common Issues & Solutions

### Issue: "Save Settings" shows error
**Solution:** Check backend console for error messages. Ensure all required fields (name, address, phone, email) are filled.

### Issue: Success message appears but font doesn't change
**Solution:** 
1. Hard refresh browser (Ctrl+F5 or Cmd+Shift+R)
2. Clear browser cache
3. Check browser console for JavaScript errors

### Issue: Font changes but reverts after refresh
**Solution:** Backend not saving to database. Check:
```bash
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT id, name, font_family FROM dealership;"
```
Should show your selected font, not 'system'.

### Issue: Backend won't start
**Solution:**
```bash
cd backend
rm -rf node_modules
npm install
npm start
```

### Issue: Database connection error
**Solution:**
```bash
docker ps  # Check if database container is running
docker start jeal-prototype-db  # Start if not running
```

## Manual Database Test

If API test fails, test database directly:

```bash
# Update font directly in database
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype -c "UPDATE dealership SET font_family = 'times' WHERE id = 1;"

# Verify change
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT id, name, font_family FROM dealership WHERE id = 1;"
```

Expected output:
```
 id |          name           | font_family 
----+-------------------------+-------------
  1 | Acme Auto Sales         | times
```

## Debugging API Requests

### Using Browser DevTools

1. Open admin panel
2. Open DevTools (F12)
3. Go to "Network" tab
4. Change font and click "Save Settings"
5. Look for PUT request to `/api/dealers/1`
6. Check "Payload" - should include `font_family: "times"`
7. Check "Response" - should return updated dealership with new font_family

### Expected Request Payload
```json
{
  "name": "Acme Auto Sales",
  "address": "123 Main St...",
  "phone": "(555) 123-4567",
  "email": "sales@acmeauto.com",
  "hours": "Mon-Fri...",
  "finance_policy": "...",
  "warranty_policy": "...",
  "about": "...",
  "hero_background_image": "...",
  "theme_color": "#3B82F6",
  "font_family": "times"
}
```

### Expected Response
```json
{
  "id": 1,
  "name": "Acme Auto Sales",
  "font_family": "times",
  ...
}
```

## Still Not Working?

If issue persists after following all steps:

1. **Check backend logs** for any error messages
2. **Check browser console** for JavaScript errors
3. **Run test script** to isolate API vs UI issue
4. **Verify database migration** was run successfully
5. **Check all files** were saved correctly

### Contact Information
Provide these details when requesting help:
- Backend console logs
- Browser console errors (F12 → Console)
- Network tab showing API request/response
- Result of test script
- Database query results

## Files Modified (Summary)

✅ `backend/routes/dealers.js` - Added font_family handling
✅ `backend/db/dealers.js` - Already had font_family support
✅ `frontend/src/pages/admin/DealerSettings.jsx` - Already had font UI
✅ `frontend/src/components/Layout.jsx` - Already had font application

The missing piece was the route not passing font_family to the database.

---

**Status:** Issue identified and fixed. Restart backend to apply changes.
