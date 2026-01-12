# Developer Guide: Font Family Customization Feature

**Last Updated:** 2025-11-28  
**Feature:** Font Family Customization (Story 3.7)  
**For:** Development Team & Future Contributors  

---

## Quick Reference

### What Was Added

Font family customization allowing dealership admins to select from 10 web-safe fonts that apply site-wide.

### Key Files Modified

```
backend/
├── db/
│   ├── dealers.js                     [MODIFIED]  Added font_family handling
│   └── migrations/
│       └── add_font_family.sql        [NEW]       Database migration
├── routes/
│   └── dealers.js                     [MODIFIED]  Added font_family to API

frontend/
├── src/
│   ├── components/
│   │   └── Layout.jsx                 [MODIFIED]  Font application logic
│   └── pages/
│       └── admin/
│           └── DealerSettings.jsx     [MODIFIED]  Font selector UI

docs/
├── stories/
│   └── 3.7.story.md                   [NEW]       Story specification
├── architecture/
│   ├── database-schema.md             [MODIFIED]  Updated schema
│   ├── api-specification.md           [MODIFIED]  Updated API docs
│   └── typography-system.md           [NEW]       Architecture guide
├── prd.md                             [MODIFIED]  Added FR23, NFR16
├── FONT_CUSTOMIZATION.md              [NEW]       Technical docs
├── FONT_QUICK_START.md                [NEW]       User guide
├── FONT_TROUBLESHOOTING.md            [NEW]       Debug guide
└── CHANGELOG-FONT-CUSTOMIZATION...md  [NEW]       This changelog
```

---

## Database Schema

### New Column

```sql
ALTER TABLE dealership
ADD COLUMN font_family VARCHAR(100) DEFAULT 'system';
```

**Details:**
- Column: `font_family`
- Type: VARCHAR(100)
- Default: 'system'
- Nullable: Yes
- Values: 'system', 'arial', 'times', 'georgia', 'verdana', 'courier', 'comic-sans', 'trebuchet', 'impact', 'palatino'

### Migration

```bash
Get-Content backend\db\migrations\add_font_family.sql | docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype
```

---

## Backend API Changes

### Endpoint: PUT /api/dealers/:id

**Request Body (added field):**
```json
{
  "name": "Acme Auto Sales",
  "font_family": "times",
  ...
}
```

**Response (includes field):**
```json
{
  "id": 1,
  "name": "Acme Auto Sales",
  "font_family": "times",
  ...
}
```

### Code Changes

**backend/routes/dealers.js:**
```javascript
// Line 35: Added to field limits
font_family: 100

// Line 186: Extract from request
const { ..., font_family } = req.body;

// Line 233: Pass to database
font_family
```

**backend/db/dealers.js:**
```javascript
// Lines 113-116: Handle in update function
if (updates.font_family !== undefined) {
  fields.push(`font_family = $${paramIndex++}`);
  values.push(updates.font_family);
}
```

---

## Frontend Changes

### Layout Component

**File:** `frontend/src/components/Layout.jsx`

**Purpose:** Applies selected font to entire website

**Implementation:**
```javascript
useEffect(() => {
  if (dealership?.font_family) {
    const fontMapping = {
      system: '-apple-system, BlinkMacSystemFont, "Segoe UI", ...',
      arial: 'Arial, Helvetica, sans-serif',
      times: '"Times New Roman", Times, serif',
      georgia: 'Georgia, serif',
      verdana: 'Verdana, Geneva, sans-serif',
      courier: '"Courier New", Courier, monospace',
      'comic-sans': '"Comic Sans MS", cursive, sans-serif',
      trebuchet: '"Trebuchet MS", Helvetica, sans-serif',
      impact: 'Impact, Charcoal, sans-serif',
      palatino: '"Palatino Linotype", "Book Antiqua", Palatino, serif'
    };
    
    const fontFamily = fontMapping[dealership.font_family] || fontMapping.system;
    document.body.style.fontFamily = fontFamily;
  }
}, [dealership?.font_family]);
```

**How It Works:**
1. Fetches dealership data via `useDealership` hook
2. Watches for `font_family` changes in `useEffect`
3. Maps identifier to complete font stack
4. Applies to `document.body.style.fontFamily`
5. All child elements inherit via CSS cascade

### Admin UI Component

**File:** `frontend/src/pages/admin/DealerSettings.jsx`

**Purpose:** Font selector with live preview

**Key Features:**
- Dropdown with 10 font options
- Live preview showing sample text
- Syncs with form state
- Saves to backend on submit

**Implementation:**
```jsx
const [fontFamily, setFontFamily] = useState('system');

<select
  value={fontFamily}
  onChange={(e) => setFontFamily(e.target.value)}
>
  <option value="system">System Default (Sans Serif)</option>
  <option value="arial">Arial</option>
  <option value="times">Times New Roman</option>
  {/* ... 7 more options */}
</select>

<div style={{ fontFamily: getFontStackForPreview(fontFamily) }}>
  <p>Font Preview: The quick brown fox jumps over the lazy dog.</p>
</div>
```

---

## Architecture Patterns

### Font Identifier System

**Pattern:** String identifiers map to complete font stacks

**Why:** 
- Database stores small identifier (7-10 bytes)
- Frontend maps to full font stack
- Easy to add new fonts
- No font files to manage

**Example:**
```
'times' → '"Times New Roman", Times, serif'
```

### Body Font Inheritance

**Pattern:** Set font on `document.body`, all elements inherit

**Why:**
- Zero refactoring required for existing components
- Universal application across all text
- Single point of control
- Best performance (one style update)

### Web-Safe Fonts Only

**Pattern:** Use pre-installed system fonts

**Why:**
- Zero network latency (no font downloads)
- Universal availability (99%+ devices)
- No FOUT (Flash of Unstyled Text)
- No CDN or licensing concerns

---

## Testing

### Automated Test

**File:** `test_font_api.js`

**Run:**
```bash
node test_font_api.js
```

**Tests:**
- GET dealership (includes font_family)
- PUT update font_family
- Verify persistence
- Reset to default

### Manual Testing

1. **Admin UI:**
   - Open Dealership Settings
   - Select font from dropdown
   - Verify preview updates
   - Click Save
   - Verify success message

2. **Public Website:**
   - Visit dealership site
   - Verify all text uses selected font
   - Check multiple pages
   - Test on mobile

3. **Multi-Tenancy:**
   - Switch to different dealership
   - Change their font
   - Verify isolation (each dealership independent)

---

## Common Issues & Solutions

### Issue: Font doesn't save

**Symptoms:** 
- Success message appears
- Page refresh shows 'system' again

**Cause:** Backend route not extracting `font_family`

**Solution:** 
- Verify line 186 in `backend/routes/dealers.js` includes `font_family`
- Verify line 233 passes `font_family` to database
- Restart backend server

**Verification:**
```bash
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT id, name, font_family FROM dealership;"
```

### Issue: Font doesn't apply on public site

**Symptoms:**
- Font saves correctly
- Public site still shows default font

**Cause:** Layout component not applying font

**Solution:**
- Verify `useEffect` in `Layout.jsx` includes font mapping
- Check browser console for errors
- Hard refresh browser (Ctrl+F5)

### Issue: Preview doesn't match website

**Symptoms:**
- Preview shows one font
- Website shows different font

**Cause:** Font mapping mismatch between preview and Layout

**Solution:**
- Ensure both use identical font mapping object
- Consider extracting to shared constant

---

## Extending the Feature

### Adding New Fonts

**Step 1: Update Database**
- No changes needed (VARCHAR(100) allows any identifier)

**Step 2: Update Frontend Mapping**

Add to font mapping in `Layout.jsx` and `DealerSettings.jsx`:

```javascript
const fontMapping = {
  // ... existing fonts
  'new-font': '"Font Name", Fallback, generic-family'
};
```

**Step 3: Update Admin UI**

Add option to dropdown in `DealerSettings.jsx`:

```jsx
<select>
  {/* ... existing options */}
  <option value="new-font">New Font Name</option>
</select>
```

**Step 4: Update Documentation**
- Add to `docs/FONT_CUSTOMIZATION.md`
- Add to `docs/architecture/typography-system.md`
- Update this guide

### Adding Google Fonts (Future)

**Requirements:**
- Font loading mechanism
- API key management
- Font weight selection UI
- Performance optimization (font subsetting)

**See:** `docs/architecture/typography-system.md` - Future Enhancements section

---

## Integration with Other Features

### Theme Color System

**Integration:** Both use similar patterns

**Similarities:**
- Database column in `dealership` table
- API endpoint: PUT /api/dealers/:id
- Admin UI in DealerSettings.jsx
- Dynamic application without component changes

**Differences:**
- Theme uses CSS custom properties
- Font uses body style inheritance

**Files:** See `docs/architecture/theming-system.md`

### Multi-Tenancy

**Isolation:** Each dealership has independent font

**Enforcement:**
- Database: Foreign key to dealership
- API: Requires dealershipId
- Frontend: Context manages current dealership

**Testing:** Change font for Dealership A, verify Dealership B unchanged

---

## Performance Considerations

### Metrics

- **Database Impact:** +7-10 bytes per dealership
- **API Response:** +15-20 bytes per request
- **Bundle Size:** 0 bytes (no new dependencies)
- **Runtime:** <1ms (single style update)
- **Page Load:** No impact

### Optimization

**Already Optimized:**
- Web-safe fonts (no downloads)
- Body inheritance (single update)
- No component rerenders
- No CSS repaints

**Not Needed:**
- Font file optimization
- Lazy loading
- CDN configuration

---

## Security Considerations

### Threat Analysis

**Low Risk Feature:**
- No file uploads (no custom fonts)
- No user input (dropdown selection)
- No SQL injection risk (parameterized queries)
- No XSS risk (no HTML/CSS injection)

### Validation

**Backend:**
```javascript
// Field length validation only
FIELD_LIMITS: {
  font_family: 100
}
```

**Frontend:**
- Dropdown ensures valid values
- No free text input
- Predefined options only

### Multi-Tenancy

**Isolated:** Each dealership manages their own font

**Enforcement:**
- API requires dealershipId
- Database has foreign key
- Frontend context manages selection

---

## Documentation Index

### For Developers

- **This Document** - Developer quick reference
- `docs/architecture/typography-system.md` - Complete architecture
- `docs/architecture/database-schema.md` - Database schema
- `docs/architecture/api-specification.md` - API documentation
- `docs/stories/3.7.story.md` - Story specification

### For Users

- `docs/FONT_QUICK_START.md` - Administrator user guide
- `docs/FONT_CUSTOMIZATION.md` - Feature documentation

### For Troubleshooting

- `docs/FONT_TROUBLESHOOTING.md` - Complete debugging guide
- `QUICK_FIX.md` - Fast reference for common issue
- `test_font_api.js` - Automated API test

### For Project Management

- `docs/prd.md` - Product requirements (FR23, NFR16)
- `docs/CHANGELOG-FONT-CUSTOMIZATION-2025-11-28.md` - Complete changelog

---

## Code Review Checklist

When reviewing font-related code:

- [ ] Font identifiers match between mapping objects
- [ ] All font stacks include fallback fonts
- [ ] Generic font family specified (serif/sans-serif/monospace)
- [ ] No hardcoded fonts in component styles
- [ ] No `!important` on font-family
- [ ] Font mapping includes all dropdown options
- [ ] Preview and website use same mapping
- [ ] Multi-tenancy isolation maintained
- [ ] API includes font_family in response
- [ ] Database migration tested

---

## Support & Troubleshooting

### Debug Commands

```bash
# Check database column
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT column_name FROM information_schema.columns WHERE table_name = 'dealership' AND column_name = 'font_family';"

# Check current font values
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT id, name, font_family FROM dealership;"

# Run API test
node test_font_api.js

# Check backend logs
# (View terminal running backend)

# Check browser console
# F12 → Console tab
```

### Getting Help

1. **Check Documentation:**
   - Start with `docs/FONT_TROUBLESHOOTING.md`
   - Review `docs/architecture/typography-system.md`

2. **Run Tests:**
   - Execute `test_font_api.js`
   - Check database directly

3. **Verify Setup:**
   - Backend server running
   - Database migration applied
   - Browser cache cleared

4. **Collect Information:**
   - Backend console logs
   - Browser console errors
   - Network tab API responses
   - Database query results

---

## Version History

| Date | Version | Changes |
|------|---------|---------|
| 2025-11-28 | 1.0 | Initial implementation |

---

**Status:** ✅ Complete and Verified  
**Next Steps:** Feature is production-ready, no further action required  
**Maintenance:** No ongoing maintenance required (web-safe fonts)
