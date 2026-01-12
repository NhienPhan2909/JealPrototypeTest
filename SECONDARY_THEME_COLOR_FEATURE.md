# Secondary Theme Color Feature

## Overview
This feature adds support for a secondary theme color to dealership websites, complementing the existing primary theme color. The secondary color is used for text elements on buttons, headers, footers, and navigation to provide contrast against the primary theme color backgrounds.

## Feature Details

### Database Changes
- **New Column**: `secondary_theme_color` added to the `dealership` table
- **Type**: VARCHAR(7) - stores hex color codes (e.g., #FFFFFF)
- **Default Value**: #FFFFFF (white)
- **Migration File**: `backend/db/migrations/add_secondary_theme_color.sql`

### Backend Changes

#### Files Modified:
1. **`backend/db/dealers.js`**
   - Added `secondary_theme_color` parameter to `update()` function documentation
   - Added handling for `secondary_theme_color` in the update query builder

2. **`backend/routes/dealers.js`**
   - Added `secondary_theme_color` to field limits (7 characters max)
   - Added validation for secondary theme color hex format
   - Added `secondary_theme_color` to the PUT endpoint request body handling
   - Validation ensures proper hex format (#RRGGBB or #RGB)

### Frontend Changes

#### Files Modified:
1. **`frontend/src/pages/admin/DealerSettings.jsx`**
   - Added `secondaryThemeColor` state variable (default: #FFFFFF)
   - Added secondary theme color picker UI with:
     - Color input widget
     - Text input for hex code
     - Reset to default button
     - Live preview showing button and accent text styles
   - Added secondary color to form submission
   - Added secondary color loading from API response

2. **`frontend/src/components/Layout.jsx`**
   - Added CSS custom property setting for `--secondary-theme-color`
   - Automatically calculates and sets:
     - `--secondary-theme-color-dark` (15% darker for hover states)
     - `--secondary-theme-color-light` (90% lighter for backgrounds)
   - Falls back to default white (#FFFFFF) if not specified

3. **`frontend/src/context/AdminContext.jsx`**
   - Added CSS custom property setting for secondary theme color in admin panel
   - Same shade calculations as Layout.jsx for consistency
   - Falls back to default white if not specified

4. **`frontend/src/components/Header.jsx`**
   - Dealership name uses secondary theme color
   - Navigation links use secondary theme color
   - Mobile hamburger menu icon uses secondary theme color
   - Mobile menu border uses secondary theme color

5. **`frontend/src/components/Footer.jsx`**
   - All footer text (headings, contact info, links) uses secondary theme color
   - Social media icons use secondary theme color
   - Copyright text uses secondary theme color
   - Border dividers use secondary theme color

6. **`frontend/src/components/NavigationButton.jsx`**
   - Navigation button text uses secondary theme color instead of white

7. **`frontend/src/index.css`**
   - Updated `.btn-primary` class to use secondary theme color for text
   - Button backgrounds use primary theme color
   - Button text uses secondary theme color

8. **Button Components Updated:**
   - `frontend/src/components/EnquiryForm.jsx`
   - `frontend/src/components/GeneralEnquiryForm.jsx`
   - `frontend/src/components/SearchWidget.jsx`
   - `frontend/src/components/VehicleCard.jsx`
   - `frontend/src/pages/public/Home.jsx`
   - All buttons now use primary theme color for background and secondary theme color for text

## Usage

### For Dealership Administrators:
1. Navigate to **Admin Panel → Dealership Settings**
2. Scroll to the **Secondary Theme Color** section
3. Use the color picker or enter a hex code directly
4. Preview the color with the button and accent text samples
5. Click **Save Settings** to apply changes

### For Developers:
Use the CSS custom properties in your stylesheets or inline styles:

```css
/* Primary theme color (backgrounds) */
background-color: var(--theme-color);
background-color: var(--theme-color-dark);  /* Hover state */
background-color: var(--theme-color-light); /* Light backgrounds */

/* Secondary theme color (text on primary backgrounds) */
color: var(--secondary-theme-color);
border-color: var(--secondary-theme-color);
background-color: var(--secondary-theme-color-dark);  /* Hover state */
background-color: var(--secondary-theme-color-light); /* Light backgrounds */
```

## Example Use Cases

### Primary Theme Color
- Header background
- Footer background  
- Button backgrounds
- Main navigation background
- Primary brand color for backgrounds

### Secondary Theme Color
- Button text
- Header text (dealership name, navigation links)
- Footer text (all content)
- Text on primary colored backgrounds
- Accent text and borders for contrast

## Default Values
- **Primary Theme Color**: `#3B82F6` (blue)
- **Secondary Theme Color**: `#FFFFFF` (white)

## API Endpoints

### Get Dealership
```
GET /api/dealers/:id
Response includes: secondary_theme_color
```

### Update Dealership
```
PUT /api/dealers/:id
Body: {
  ...other fields,
  secondary_theme_color: "#FFFFFF"  // Optional
}
```

## Migration Instructions

### Running the Migration
To add the `secondary_theme_color` column to existing databases:

```bash
# Using Docker (local development)
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend/db/migrations/add_secondary_theme_color.sql

# Or using psql directly
psql $DATABASE_URL < backend/db/migrations/add_secondary_theme_color.sql
```

### Verification
After running the migration, verify the column exists:

```sql
SELECT column_name, data_type, column_default
FROM information_schema.columns
WHERE table_name = 'dealership' AND column_name = 'secondary_theme_color';
```

Expected output:
```
column_name            | data_type       | column_default
-----------------------+-----------------+----------------
secondary_theme_color  | character varying | '#FFFFFF'::character varying
```

## Testing Checklist
- [ ] Migration runs successfully without errors
- [ ] Secondary color can be set/updated via admin panel
- [ ] Secondary color persists after page reload
- [ ] CSS variables are correctly set in DOM
- [ ] Color picker shows current secondary color
- [ ] Hex text input validates format
- [ ] Reset button restores default color (#FFFFFF)
- [ ] Preview elements display the selected color
- [ ] Existing dealerships get default value (#FFFFFF)
- [ ] Button text uses secondary theme color
- [ ] Header text uses secondary theme color
- [ ] Footer text uses secondary theme color
- [ ] Navigation links use secondary theme color

## Browser Compatibility
- CSS custom properties supported in all modern browsers (Chrome, Firefox, Safari, Edge)
- Color input type supported in all modern browsers
- Hex color validation works consistently across browsers

## Future Enhancements
- Add more theme color slots (tertiary, quaternary)
- Add preset color palettes/themes
- Add color accessibility contrast checker
- Add color harmony suggestions based on primary color
