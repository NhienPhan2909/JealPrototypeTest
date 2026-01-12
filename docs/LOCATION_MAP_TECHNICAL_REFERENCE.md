# Location Map Feature - Technical Reference for AI Agents

**Document Purpose**: Provide comprehensive technical context for AI development agents working on the dealership website platform.

**Last Updated**: December 2025  
**Status**: ✅ Production-Ready

---

## Feature Overview

The Location Map feature displays an interactive Google Maps embed showing each dealership's physical location, along with contact information and a "Get Directions" button.

**Access**: `/location` route on each dealership website  
**Navigation**: Accessible via header menu (between "Warranty" and "Log In")

---

## Critical Implementation Details

### 1. Address Data Source

**Database**: PostgreSQL table `dealership`  
**Column**: `address` (TEXT, NOT NULL)  
**Format**: `Street Address, City, State ZIP`  
**Example**: `123 Main St, Springfield, IL 62701`

**API Endpoint**: `GET /api/dealers/:id`  
Returns dealership object including address field.

### 2. Address Cleaning (CRITICAL)

**Location**: `frontend/src/pages/public/Location.jsx`

```javascript
const cleanAddress = (dealership?.address || '').trim().replace(/[\u200B-\u200D\uFEFF]/g, '');
```

**Why This Matters**:
- Removes hidden Unicode characters (zero-width spaces, joiners, no-break spaces)
- These invisible characters can break Google Maps geocoding
- Must be applied before URL encoding

**Unicode Characters Removed**:
- `\u200B` - Zero Width Space
- `\u200C` - Zero Width Non-Joiner
- `\u200D` - Zero Width Joiner
- `\uFEFF` - Zero Width No-Break Space (BOM)

### 3. Google Maps URL Format (CRITICAL)

#### Map Embed URL
```javascript
const mapUrl = `https://maps.google.com/maps?q=${encodedAddress}&t=&z=16&ie=UTF8&iwloc=&output=embed`;
```

**Parameter Breakdown**:
- `q=` - Query parameter (encoded address)
- `t=` - Map type (empty = default/roadmap)
- `z=16` - Zoom level (16 = street level, shows buildings)
- `ie=UTF8` - Input encoding (ensures proper character handling)
- `iwloc=` - Info window location parameter
- `output=embed` - Required for iframe embedding

**Why `z=16`**:
- Zoom levels range from 0 (world) to 21 (building detail)
- Level 16 shows individual buildings and streets clearly
- Appropriate for locating a business address
- Lower values (e.g., z=12) show too wide an area
- Higher values (e.g., z=18) may be too zoomed in

#### Directions URL
```javascript
const directionsUrl = `https://www.google.com/maps/dir/?api=1&destination=${encodedAddress}`;
```

**Parameter Breakdown**:
- `api=1` - Uses Google Maps URLs API format
- `destination=` - Encoded destination address

**Why Separate URL**:
- Embed URL uses different parameters than directions URL
- Directions API is more modern and reliable
- Opens in new tab with user's current location as starting point

### 4. URL Encoding

**Always use**: `encodeURIComponent(cleanAddress)`

**What it encodes**:
- Spaces → `%20`
- Commas → `%2C`
- Special characters → percent-encoded equivalents

**Example**:
```
Input:  "123 Main St, Springfield, IL 62701"
Output: "123%20Main%20St%2C%20Springfield%2C%20IL%2062701"
```

---

## Component Architecture

### File Structure
```
frontend/src/pages/public/Location.jsx       # Main location page component
frontend/src/App.jsx                         # Route definition: /location
frontend/src/utils/defaultNavigation.js      # Navigation config (order: 6)
backend/config/defaultNavigation.js          # Backend nav config sync
```

### Component Responsibilities

**Location.jsx**:
1. Fetches dealership data using `useDealership` hook
2. Cleans address and generates URLs
3. Renders two-column layout (desktop) or stacked (mobile)
4. Left/Top: Contact information with clickable phone/email
5. Right/Bottom: Google Maps iframe embed
6. Bottom: "Plan Your Visit" information section

### Dependencies
- `react-router-dom` - Routing
- `react-icons/fa` - Icons (FaMapMarkerAlt, FaPhone, FaEnvelope)
- `useDealership` hook - Fetches dealership data
- `useDealershipContext` - Gets current dealership ID

---

## Common Issues and Solutions

### Issue 1: Map Not Showing Correct Location

**Symptoms**: Map loads but shows wrong area or generic location

**Root Causes**:
1. Incomplete address in database (missing city, state, or ZIP)
2. Address format not recognized by Google's geocoder
3. Hidden Unicode characters in address string

**Solutions**:
1. Verify address format: `Street, City, State ZIP`
2. Check database for complete address:
   ```sql
   SELECT id, name, address FROM dealership WHERE id = X;
   ```
3. Ensure address cleaning is applied (see section 2)
4. Test URL directly by copying map URL and removing `&output=embed`

**Prevention**:
- Validate address format on admin input
- Store addresses in consistent format
- Consider adding latitude/longitude fields for precise control

### Issue 2: External URL Modifications

**Symptoms**: Unexpected prefixes or modifications appear in URLs (e.g., "Jtsecurity")

**Root Cause**: External to application
- Browser extensions (security, privacy, ad blockers)
- Antivirus software URL scanning
- Network proxies (corporate, institutional)
- ISP traffic modification

**Verification**:
1. Check browser console - application logs clean address
2. Test in incognito/private mode (disables extensions)
3. Verify database has clean data
4. Test on different network

**Important**: This is NOT a bug in the application code. The application correctly generates clean URLs.

### Issue 3: Map Iframe Not Loading

**Symptoms**: Empty space where map should be, or error message

**Root Causes**:
1. Network/firewall blocking Google Maps
2. Browser blocking third-party iframes
3. CSP (Content Security Policy) restrictions
4. AdBlocker blocking Google services

**Solutions**:
1. Check browser console for CSP or CORS errors
2. Verify Google Maps is accessible: Visit `maps.google.com`
3. Whitelist Google Maps in content blockers
4. Check iframe attributes (allowFullScreen, referrerPolicy)

---

## Data Flow

```
1. User navigates to /location
   ↓
2. Location component mounts
   ↓
3. useDealership hook fetches from API
   ↓
4. GET /api/dealers/:id returns dealership data
   ↓
5. Component receives dealership.address
   ↓
6. Clean address (remove Unicode)
   ↓
7. Encode address (encodeURIComponent)
   ↓
8. Generate map URL with parameters
   ↓
9. Render iframe with generated URL
   ↓
10. Google Maps geocodes address and displays map
```

---

## Testing Requirements

### Manual Testing Checklist
- [ ] Map loads within 3 seconds
- [ ] Map displays correct city/state
- [ ] Map is interactive (zoom, pan work)
- [ ] Address displays correctly in contact card
- [ ] Phone link opens dialer (mobile) or prompts (desktop)
- [ ] Email link opens mail client
- [ ] "Get Directions" opens Google Maps in new tab
- [ ] Destination in Google Maps is correct
- [ ] Layout is responsive (test at 320px, 768px, 1024px, 1920px)
- [ ] Mobile menu includes "Location" link

### Automated Testing (Future)
- Component renders without errors
- API call successful
- Address cleaning removes Unicode characters
- URL encoding produces valid URLs
- Iframe src attribute is correctly set

### Browser Compatibility
Tested and working in:
- Chrome/Chromium (latest)
- Firefox (latest)
- Edge (latest)
- Safari (latest)
- Mobile browsers (iOS Safari, Chrome Android)

---

## Database Schema Context

```sql
CREATE TABLE dealership (
  id SERIAL PRIMARY KEY,
  name VARCHAR(255) NOT NULL,
  address TEXT NOT NULL,        -- Used by Location Map feature
  phone VARCHAR(20) NOT NULL,   -- Displayed with tel: link
  email VARCHAR(255) NOT NULL,  -- Displayed with mailto: link
  hours TEXT,                   -- Displayed in contact card
  -- ... other fields
);
```

**Important**: `address` field must contain complete address for accurate geocoding.

---

## Navigation Configuration

**Default Order**:
1. Home
2. Inventory
3. About
4. Finance
5. Warranty
6. **Location** ← This feature
7. Log In

**Icon**: `FaMapMarkerAlt` (map pin/marker icon)

**Customization**: Admins can reorder, enable/disable, or change the icon via Navigation Manager in admin settings.

---

## Future Enhancement Considerations

### Recommended Improvements
1. **Add Latitude/Longitude Fields**:
   - More accurate than address geocoding
   - Example schema addition:
     ```sql
     ALTER TABLE dealership 
     ADD COLUMN latitude DECIMAL(10, 8),
     ADD COLUMN longitude DECIMAL(11, 8);
     ```
   - URL format: `https://maps.google.com/maps?q=${lat},${lng}&z=16`

2. **Google Maps API Key**:
   - Better control and customization
   - Custom markers with dealership logo
   - Places API for verified addresses
   - Requires billing account setup

3. **Multiple Locations**:
   - Some dealerships have multiple branches
   - Create `dealership_locations` table
   - Display list of locations or map with multiple pins

4. **Directions From Current Location**:
   - Use Geolocation API to get user's position
   - Pre-populate starting point in directions URL

5. **Hours Integration**:
   - Parse hours field
   - Show "Open now" / "Closed" status
   - Display next opening time

---

## Security Considerations

### XSS Prevention
- Address data is from trusted database (admin input)
- `encodeURIComponent()` prevents injection in URLs
- React escapes content in JSX automatically

### Data Validation
- Address should be validated on admin input
- Prevent SQL injection via parameterized queries (already implemented)
- Limit address field length (255 characters recommended)

### Privacy
- No personal user data collected
- Dealership addresses are public business information
- Google Maps iframe may set cookies (standard practice)

---

## Performance Considerations

### Loading Time
- Map iframe loads asynchronously
- Use `loading="lazy"` attribute (already implemented)
- Doesn't block page render

### Bundle Size
- No additional JavaScript libraries required
- Google Maps loads from CDN (external)
- Minimal impact on bundle size

### Caching
- Dealership data cached by React Query (if implemented)
- Google Maps tiles cached by browser
- Consider service worker for offline support (future)

---

## Accessibility

### Current Implementation
- Semantic HTML (dl/dt/dd for contact information)
- Alt text on map iframe: `title={Map showing location of ${dealership?.name}}`
- Clickable phone/email links with proper protocols (tel:, mailto:)
- Keyboard navigable (all interactive elements)

### Recommendations
- Add aria-label to map iframe
- Provide text alternative to map (address is already shown)
- Ensure sufficient color contrast
- Test with screen readers

---

## Related Documentation

- `docs/LOCATION_MAP_FEATURE.md` - Feature overview and implementation details
- `LOCATION_MAP_FIX.md` - Historical issue resolution
- `HOW_TO_TEST_LOCATION_MAP.md` - Testing procedures
- `test_location_url.js` - URL generation verification script

---

## AI Agent Guidelines

When modifying or debugging the Location Map feature:

1. **Always maintain address cleaning logic** - Don't remove Unicode character handling
2. **Preserve URL format exactly** - All parameters are necessary for proper display
3. **Test with multiple addresses** - Different formats may behave differently
4. **Check database first** - Many issues stem from incorrect data
5. **Remember external factors** - Browser extensions and network proxies can interfere
6. **Document changes** - Update this file if implementation changes

### Quick Diagnostic Commands

```bash
# Check database address
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT id, name, address FROM dealership;"

# Test URL generation
node test_location_url.js

# Start development environment
cd backend && node server.js &
cd frontend && npm run dev

# Check API response
curl http://localhost:5000/api/dealers/1 | jq '.address'
```

---

**Document Maintenance**: Update this file when making changes to the Location Map feature implementation, URL formats, or discovering new issues/solutions.
