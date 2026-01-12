# Location Map Feature

## Overview
Added a "Location Map" section to all dealership websites that displays the dealership's physical location with an embedded Google Map and contact information.

**Status**: ✅ Implemented and Verified (December 2025)

## Important: Google Maps Address Requirements

### Critical Implementation Details
When implementing or modifying the Location Map feature, ensure the following:

1. **Address Cleaning**: Always clean the address string to remove hidden Unicode characters:
   ```javascript
   const cleanAddress = address.trim().replace(/[\u200B-\u200D\uFEFF]/g, '');
   ```
   - Removes zero-width spaces, joiners, and no-break spaces
   - Prevents geocoding errors from invisible characters

2. **URL Format**: Use the complete Google Maps embed URL with all parameters:
   ```javascript
   https://maps.google.com/maps?q=${encodedAddress}&t=&z=16&ie=UTF8&iwloc=&output=embed
   ```
   - `z=16`: Street-level zoom for precise location display
   - `ie=UTF8`: Ensures proper character encoding
   - `output=embed`: Required for iframe embedding

3. **Address Encoding**: Always use `encodeURIComponent()` to properly encode the address for URLs

4. **External Interference**: Be aware that browser extensions, antivirus software, or network proxies may modify URLs or inject prefixes. This is external to the application and not a bug in the code.

## Features Implemented

### 1. New Location Page (`/location`)
- **File**: `frontend/src/pages/public/Location.jsx`
- Displays dealership address, phone, email, and hours
- Embedded Google Maps iframe showing dealership location
- "Get Directions" button linking to Google Maps with directions
- Fully responsive design for mobile and desktop
- Uses dealership data from the existing API

### 2. Navigation Integration
- Added "Location" link to the header navigation
- Uses `FaMapMarkerAlt` icon from react-icons
- Appears between "Warranty" and "Log In" in the navigation menu
- Available on both desktop and mobile views

### 3. Updated Files

#### Frontend:
- **Created**: `frontend/src/pages/public/Location.jsx` - Main location page component
- **Updated**: `frontend/src/App.jsx` - Added `/location` route
- **Updated**: `frontend/src/utils/defaultNavigation.js` - Added Location to default navigation (order: 6)

#### Backend:
- **Updated**: `backend/config/defaultNavigation.js` - Added Location to backend default navigation config

## Google Maps Integration

The page uses Google Maps Embed API with two features:

1. **Embedded Map Frame**: 
   - URL: `https://www.google.com/maps?q={encodedAddress}&output=embed`
   - Shows the dealership location based on the address from database
   - Fully interactive (zoom, pan, street view)

2. **Get Directions Button**:
   - URL: `https://www.google.com/maps/dir/?api=1&destination={encodedAddress}`
   - Opens Google Maps in new tab with directions to dealership
   - Works on both desktop and mobile devices

## Page Layout

The Location page includes:

- **Left Panel** (Desktop) / Top Section (Mobile):
  - Dealership name
  - Full address
  - Clickable phone number (tel: link)
  - Clickable email (mailto: link)
  - Hours of operation
  - "Get Directions" button

- **Right Panel** (Desktop) / Bottom Section (Mobile):
  - Embedded Google Maps iframe
  - 4:3 aspect ratio for optimal viewing

- **Bottom Section**:
  - "Plan Your Visit" information card

## Dependencies

No new dependencies required. Uses existing packages:
- `react-router-dom` - For routing
- `react-icons/fa` - For icons (FaMapMarkerAlt, FaPhone, FaEnvelope)

## Related Documentation

For comprehensive information about this feature:

- **Quick Start**: `../LOCATION_MAP_SUMMARY.md` - Executive summary and verification
- **Technical Details**: `LOCATION_MAP_TECHNICAL_REFERENCE.md` - In-depth technical reference for developers
- **Testing Guide**: `../HOW_TO_TEST_LOCATION_MAP.md` - Step-by-step testing procedures
- **Issue History**: `../LOCATION_MAP_FIX.md` - Problem resolution history
- **Test Script**: `../test_location_url.js` - Automated URL generation verification

## Usage

Users can access the Location page by:
1. Clicking "Location" in the header navigation
2. Navigating directly to `/location` URL

## Data Requirements

The Location page requires the following dealership data fields (all already exist in the database):
- `address` - Required for map location
- `name` - Dealership name
- `phone` - Contact phone number
- `email` - Contact email
- `hours` - Operating hours (optional, graceful fallback if missing)

## Testing

To test the feature:

1. Start the backend server: `cd backend && node server.js`
2. Start the frontend dev server: `cd frontend && npm run dev`
3. Visit `http://localhost:3000/location`
4. Verify:
   - Address displays correctly
   - Map loads showing the correct location (Springfield, IL area)
   - Contact information is clickable (phone, email)
   - "Get Directions" opens Google Maps in new tab with correct destination
   - Page is responsive on mobile devices

### Troubleshooting Map Display Issues

If the map doesn't show the exact location or displays incorrect results:

1. **Check Database Address**: Verify the address in the database is complete and accurate:
   ```bash
   docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT id, name, address FROM dealership;"
   ```

2. **Verify URL Generation**: The map URL should look like:
   ```
   https://maps.google.com/maps?q=123%20Main%20St%2C%20Springfield%2C%20IL%2062701&t=&z=16&ie=UTF8&iwloc=&output=embed
   ```

3. **Test URL Directly**: Copy the generated map URL and test it directly in a browser (remove `&output=embed` for testing)

4. **Check for External Interference**: 
   - Test in browser incognito/private mode
   - Check if browser extensions are modifying URLs
   - Some antivirus or network proxies may inject content
   - If you see unexpected prefixes in the address, it's likely external interference

5. **Address Format**: Ensure addresses follow the format: `Street Address, City, State ZIP`
   - Example: `123 Main St, Springfield, IL 62701`
   - Include all components for accurate geocoding

## Known Considerations

### External URL Modifications
During development and testing, be aware that external factors may modify URLs:
- **Browser Extensions**: Security/privacy extensions may inject prefixes or modify URLs
- **Antivirus Software**: May intercept and modify web requests
- **Network Proxies**: Corporate or institutional networks may inject content
- **ISP Modifications**: Some internet providers modify traffic

These modifications are external to the application and should not be treated as application bugs.

### Sample Data Limitation
The seed data includes sample addresses (e.g., "123 Main St, Springfield, IL 62701") that may not correspond to real businesses. When deploying to production:
- Replace with actual dealership addresses
- Verify each address geocodes correctly
- Consider adding latitude/longitude coordinates for maximum accuracy

## Future Enhancements

Possible improvements:
- Add custom map markers with dealership logo
- Show multiple locations if dealership has branches
- Add photos of the dealership exterior/interior
- Show directions from user's current location
- Embed virtual tour or street view
- Store latitude/longitude in database for more precise location
- Add Google Maps API key for enhanced features and reliability
