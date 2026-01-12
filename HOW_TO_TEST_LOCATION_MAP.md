# How to Test the Location Map Feature

**Feature Status**: ✅ Implemented and Verified (December 2025)

## Quick Start

1. **Start the backend server**:
   ```bash
   cd backend
   node server.js
   ```

2. **Start the frontend dev server** (in a new terminal):
   ```bash
   cd frontend
   npm run dev
   ```

3. **Open browser**: Navigate to `http://localhost:3000/location`

## What to Check

### ✅ Visual Elements
- [ ] Page title "Our Location" displays
- [ ] Dealership name shows: "Acme Auto Sales"
- [ ] Address shows: "123 Main St, Springfield, IL 62701"
- [ ] Phone number is clickable: `(555) 123-4567`
- [ ] Email is clickable: `sales@acmeauto.com`
- [ ] Hours display correctly
- [ ] "Get Directions" button is visible
- [ ] Map iframe loads on the right side (desktop) or bottom (mobile)

### ✅ Functionality Tests

#### Test 1: Map Display
1. Open `http://localhost:3000/location`
2. Wait for map to load (may take a few seconds)
3. Verify map shows Springfield, IL area
4. Map should be interactive (can zoom, pan, click)

#### Test 2: Get Directions Button
1. Click the "Get Directions" button
2. Should open Google Maps in a new tab
3. **Check the destination field carefully**
4. It should show: `123 Main St, Springfield, IL 62701`
5. **Look for "Jtsecurity" prefix** - if you see it, it's NOT from our code

#### Test 3: Responsive Design
1. Desktop view (wide screen):
   - Two-column layout
   - Contact info on left
   - Map on right
2. Mobile view (narrow screen):
   - Stacked layout
   - Contact info on top
   - Map below
3. Test by resizing browser window or using DevTools device emulation

### ✅ Navigation Tests

#### Test 4: Header Navigation
1. Go to homepage: `http://localhost:3000/`
2. Look at header navigation menu
3. Find "Location" link (should have a map pin icon 📍)
4. Click "Location" link
5. Should navigate to `/location` page

#### Test 5: Mobile Menu
1. Resize browser to mobile width (< 768px)
2. Click hamburger menu icon
3. Verify "Location" appears in mobile menu
4. Click it and verify navigation works

## Troubleshooting "Jtsecurity" Issue

If you see "Jtsecurity" in the address, it's **NOT** from our application. Here's how to verify:

### Test in Incognito/Private Mode
1. Open browser in incognito/private mode
2. Navigate to `http://localhost:3000/location`
3. Test the "Get Directions" button
4. If "Jtsecurity" is gone → It's a browser extension

### Check Browser Extensions
1. Open browser extensions page:
   - Chrome: `chrome://extensions`
   - Firefox: `about:addons`
   - Edge: `edge://extensions`
2. Look for security/privacy extensions
3. Temporarily disable them
4. Test again

### Check Antivirus Software
Some antivirus software modifies web traffic:
1. Temporarily disable antivirus
2. Test the Location page
3. If "Jtsecurity" disappears → It's your antivirus

### Test on Different Network
1. Try on a different WiFi network
2. Or use mobile hotspot
3. Corporate/school networks often modify traffic

### Verify Database Content
Run this command to see the raw database address:
```bash
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT address FROM dealership WHERE id = 1;"
```

Expected output (should NOT contain "Jtsecurity"):
```
              address
------------------------------------
 123 Main St, Springfield, IL 62701
```

## Manual URL Testing

### Test Map URL Directly
Copy and paste this URL in your browser:
```
https://maps.google.com/maps?q=123%20Main%20St%2C%20Springfield%2C%20IL%2062701&t=&z=16&ie=UTF8&iwloc=
```

This should open Google Maps showing the location in Springfield, IL.

### Test Directions URL Directly
Copy and paste this URL in your browser:
```
https://www.google.com/maps/dir/?api=1&destination=123%20Main%20St%2C%20Springfield%2C%20IL%2062701
```

This should open Google Maps directions with the destination set correctly.

**Important**: If "Jtsecurity" appears when you paste these URLs, it confirms the issue is with your browser/network, not our code.

## Expected Results

### ✅ Map Should Show
- Location: Springfield, Illinois
- Approximate area: Near downtown Springfield
- Zoom level: Street level (can see individual buildings)
- Note: "123 Main St" is a sample address - it may not be a real business

### ✅ Map Display
The map should display:
- Interactive Google Maps iframe
- Springfield, Illinois area
- Street-level zoom (can see individual buildings)
- Clickable map with zoom/pan controls

## Testing with Different Dealerships

The system supports multiple dealerships. To test with Dealership #2:

1. Update the dealership context or URL parameter (if implemented)
2. Or query directly: `http://localhost:5000/api/dealers/2`
3. Expected address: `456 Oak Ave, Springfield, IL 62702`

## Automated Testing

Run the URL generation test script:
```bash
node test_location_url.js
```

This verifies:
- ✓ Address is clean (no "Jtsecurity")
- ✓ URLs are properly formatted
- ✓ Encoding is correct

## Common Issues and Solutions

### Issue: Map doesn't load
**Solution**: 
- Check internet connection
- Check browser console for errors
- Try refreshing the page
- Clear browser cache

### Issue: Map shows wrong location
**Solution**:
- Verify the address in database is correct
- Check console logs for the actual address being used
- The sample address "123 Main St" might not be precise

### Issue: "Get Directions" button doesn't work
**Solution**:
- Check if pop-up blocker is active
- Try right-click → Open in new tab
- Check browser console for errors

### Issue: Responsive layout broken
**Solution**:
- Clear browser cache
- Hard refresh (Ctrl+Shift+R or Cmd+Shift+R)
- Check CSS is loading correctly

## Production Checklist

Before deploying to production:
- [x] Remove console.log statements
- [x] Remove debug display text
- [ ] Replace sample addresses with real dealership addresses
- [ ] Test with real dealership addresses for accurate geocoding
- [ ] Test on multiple browsers (Chrome, Firefox, Edge, Safari)
- [ ] Test on mobile devices (iOS, Android)
- [ ] Verify map loads on slow connections
- [ ] Consider adding Google Maps API key for enhanced features
- [ ] Add error handling for map loading failures
- [ ] Verify addresses are in complete format (Street, City, State ZIP)

## Need Help?

If issues persist:
1. Check `LOCATION_MAP_FIX.md` for detailed technical information
2. Review browser console for error messages
3. Verify all changes were applied correctly
4. Test in a clean browser profile (no extensions)
