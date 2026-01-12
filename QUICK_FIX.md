# QUICK FIX REFERENCE

## Problem
Font selection in admin panel doesn't save - reverts to "system" default.

## Root Cause
Backend route `backend/routes/dealers.js` was not handling `font_family` field.

## Fix Applied ✅

### File: `backend/routes/dealers.js`

**Line 35:** Added to FIELD_LIMITS
```javascript
font_family: 100 // Font identifier (e.g., 'times', 'arial', 'system')
```

**Line 186:** Extract from request body
```javascript
const { name, address, phone, email, logo_url, hours, finance_policy, warranty_policy, about, hero_background_image, theme_color, font_family } = req.body;
```

**Line 233:** Pass to database
```javascript
font_family // Font identifier, no sanitization needed (predefined values)
```

## Action Required

### 1. Restart Backend
```bash
cd backend
# Press Ctrl+C if already running
npm start
```

### 2. Test in Browser
1. Open admin panel
2. Go to Dealership Settings  
3. Change font to "Times New Roman"
4. Click "Save Settings"
5. Should see success message
6. Refresh page - font should persist
7. Visit public site - text should use Times New Roman

### 3. Verify Database (Optional)
```bash
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT id, name, font_family FROM dealership;"
```

Should show the selected font, not 'system'.

## Test Script (Optional)
```bash
node test_font_api.js
```

## Troubleshooting
If still not working after restart:
- Check backend console for errors
- Check browser console (F12) for errors
- Clear browser cache (Ctrl+Shift+Delete)
- See `docs/FONT_TROUBLESHOOTING.md` for detailed help

## Verification
✅ Backend route extracts font_family  
✅ Backend route passes font_family to database  
✅ Database has font_family column  
✅ Frontend sends font_family in request  
✅ Frontend applies font_family to page  

**All code is in place - just needs backend restart!**
