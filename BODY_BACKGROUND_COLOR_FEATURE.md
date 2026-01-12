# Body Background Color Feature

## Overview
The body background color feature allows dealerships to customize the background color of their website's main body content area through the CMS admin page. This feature complements the existing primary and secondary theme color customization.

## Feature Details

### Default Value
- **Default Color**: `#FFFFFF` (White)
- The background color defaults to white if not set by the dealership

### Admin Interface
Located in: **Admin Dashboard → Settings → Body Background Color**

The color picker interface includes:
1. **Visual Color Picker**: Click to select a color from the color wheel
2. **Hex Input Field**: Manually enter a hex color code (e.g., #FFFFFF)
3. **Reset Button**: Quickly reset to default white color
4. **Live Preview**: See how the background will look with content

### Technical Implementation

#### Database Schema
```sql
-- Added column to dealership table
ALTER TABLE dealership
ADD COLUMN body_background_color VARCHAR(7) DEFAULT '#FFFFFF';
```

#### Backend Changes

1. **Database Layer** (`backend/db/dealers.js`)
   - Added `body_background_color` field handling in the `update()` function
   - Supports hex color format validation

2. **API Routes** (`backend/routes/dealers.js`)
   - Added `body_background_color` to field limits (7 characters max)
   - Added hex color format validation
   - Included in PUT `/api/dealers/:id` endpoint

#### Frontend Changes

1. **Admin Settings Page** (`frontend/src/pages/admin/DealerSettings.jsx`)
   - Added `bodyBackgroundColor` state management
   - Added color picker UI component with:
     - Color input (type="color")
     - Text input for hex code
     - Reset to default button
     - Live preview area
   - Included in form submission to API

2. **Layout Component** (`frontend/src/components/Layout.jsx`)
   - Added CSS custom property: `--body-background-color`
   - Applies background color to `document.body`
   - Falls back to white (`#FFFFFF`) if not set
   - Updates dynamically when dealership data changes

### Migration
**File**: `backend/db/migrations/add_body_background_color.sql`

To apply the migration manually:
```bash
# Using Get-Content (PowerShell)
Get-Content backend\db\migrations\add_body_background_color.sql | docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype

# Using cat (Linux/Mac)
cat backend/db/migrations/add_body_background_color.sql | docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype
```

### Usage Instructions

#### For Dealership Staff
1. Log in to the admin dashboard
2. Navigate to **Settings**
3. Scroll to **Body Background Color** section
4. Choose a color using one of three methods:
   - Click the color square to open the color picker
   - Type a hex code directly (e.g., #F0F0F0 for light gray)
   - Click "Reset to Default" for white background
5. Preview the color in the preview box
6. Click **Save Settings** to apply changes
7. Visit the public website to see the new background color

#### Color Recommendations
- **Light backgrounds** (#FFFFFF to #F5F5F5) work best for readability
- Ensure sufficient contrast between background and text
- Test on multiple pages to verify the color works throughout the site
- Consider your brand colors when selecting a background

### API Examples

#### Get Dealership with Body Background Color
```javascript
GET /api/dealers/1
Response:
{
  "id": 1,
  "name": "Acme Auto Sales",
  "theme_color": "#3B82F6",
  "secondary_theme_color": "#FFFFFF",
  "body_background_color": "#F9FAFB",
  ...
}
```

#### Update Body Background Color
```javascript
PUT /api/dealers/1
Headers: { "Content-Type": "application/json" }
Body:
{
  "body_background_color": "#F0F0F0"
}
```

### Validation Rules
- **Format**: Must be a valid hex color code (#RGB or #RRGGBB)
- **Length**: Exactly 7 characters (including #)
- **Examples**: 
  - ✅ `#FFFFFF` (white)
  - ✅ `#F5F5F5` (light gray)
  - ✅ `#FFF` (short format white)
  - ❌ `FFFFFF` (missing #)
  - ❌ `#GGGGGG` (invalid hex)

### CSS Custom Property
The body background color is available throughout the frontend as:
```css
var(--body-background-color)
```

This can be used in custom components for consistent theming.

### Related Features
- **Primary Theme Color**: Header and footer background
- **Secondary Theme Color**: Buttons, text, and accents
- **Font Family**: Website typography customization

### Files Modified
```
backend/
  db/
    dealers.js                           # Added body_background_color handling
    migrations/
      add_body_background_color.sql      # New migration file
  routes/
    dealers.js                           # Added validation and API support

frontend/
  src/
    components/
      Layout.jsx                         # Added background color application
    pages/
      admin/
        DealerSettings.jsx               # Added UI for color picker
```

### Testing Checklist
- [ ] Migration runs successfully
- [ ] Default value (#FFFFFF) is applied to new and existing dealerships
- [ ] Color picker displays current value correctly
- [ ] Manual hex input accepts valid colors
- [ ] Invalid colors are rejected with error message
- [ ] Reset button returns to #FFFFFF
- [ ] Preview shows accurate color representation
- [ ] Save button persists the color to database
- [ ] Public website displays the selected background color
- [ ] Color updates immediately without page reload
- [ ] Works across all pages (Home, Inventory, About, etc.)

### Browser Compatibility
- Chrome/Edge: ✅ Full support
- Firefox: ✅ Full support
- Safari: ✅ Full support
- Mobile browsers: ✅ Full support

### Troubleshooting

**Problem**: Background color doesn't update after saving
- **Solution**: Clear browser cache and hard reload (Ctrl+Shift+R)

**Problem**: Color picker shows wrong color
- **Solution**: Check that the hex code is valid format

**Problem**: Migration fails
- **Solution**: Ensure database container is running and accessible

**Problem**: Background too dark, text not readable
- **Solution**: Choose a lighter background color or adjust text colors

## Future Enhancements
Possible future improvements:
- Pre-defined color palette suggestions
- Accessibility contrast checker
- Background image support alongside color
- Gradient background options
- Per-page background customization
