# Secondary Theme Color - Quick Start Guide

## Step 1: Run Database Migration

First, add the `secondary_theme_color` column to your database:

### Option A: Using Docker (Recommended)
```bash
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend/db/migrations/add_secondary_theme_color.sql
```

### Option B: Using psql directly
```bash
psql $DATABASE_URL < backend/db/migrations/add_secondary_theme_color.sql
```

### Verify Migration
```bash
docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT column_name, column_default FROM information_schema.columns WHERE table_name = 'dealership' AND column_name = 'secondary_theme_color';"
```

Expected output:
```
     column_name      | column_default
----------------------+------------------
 secondary_theme_color | '#FFFFFF'::character varying
```

## Step 2: Start the Application

### Start Backend
```bash
cd backend
npm install  # If not already done
npm run dev
```

Backend should start on http://localhost:3000

### Start Frontend (in a new terminal)
```bash
cd frontend
npm install  # If not already done
npm run dev
```

Frontend should start on http://localhost:5173

## Step 3: Test the Feature

### Option A: Manual Testing via Admin UI

1. **Open the admin panel**: http://localhost:5173/admin
2. **Login** with your admin credentials
3. **Navigate to Dealership Settings**
4. **Scroll down** to find the "Secondary Theme Color" section
5. **Change the color** using:
   - The color picker widget (click the colored square)
   - Or type a hex code directly (e.g., #FF5733)
6. **Preview** the color in the button and accent samples
7. **Click "Save Settings"**
8. **Verify** the success message appears
9. **Reload the page** and verify the color persists
10. **Check the public site** to see button text, header text, and footer text using the secondary color

### Option B: Automated Testing via Test Script

Run the test script to verify API functionality:

```bash
# Make sure backend is running first
node test_secondary_theme_color.js
```

The test script will:
- ✅ Test GET endpoint (retrieve secondary color)
- ✅ Test PUT endpoint (update secondary color)
- ✅ Test color validation (reject invalid formats)
- ✅ Test color validation (accept valid hex codes)

## Step 4: Use the Secondary Color in Your Code

### For Text on Primary Backgrounds (Recommended Use)
```jsx
// Button with primary background and secondary text
<button style={{ 
  backgroundColor: 'var(--theme-color)', 
  color: 'var(--secondary-theme-color)' 
}}>
  Click Me
</button>

// Header text
<h1 style={{ color: 'var(--secondary-theme-color)' }}>
  Dealership Name
</h1>
```

### In CSS Files
```css
/* Button with primary background and secondary text */
.my-button {
  background-color: var(--theme-color);
  color: var(--secondary-theme-color);
}

.my-button:hover {
  background-color: var(--theme-color-dark);
}

/* Header/Footer text */
.header-text {
  color: var(--secondary-theme-color);
}

/* Accent borders */
.my-accent {
  color: var(--secondary-theme-color);
  border-left: 3px solid var(--secondary-theme-color);
}
```

### In Tailwind CSS
You can extend your Tailwind config to use the CSS variables:

```javascript
// tailwind.config.js
module.exports = {
  theme: {
    extend: {
      colors: {
        'theme-primary': 'var(--theme-color)',
        'theme-secondary': 'var(--secondary-theme-color)',
      },
    },
  },
}
```

Then use in your components:
```jsx
<button className="bg-theme-primary text-theme-secondary px-4 py-2 rounded">
  Click Me
</button>
```

## Available CSS Variables

After the feature is active, these CSS variables are available:

### Primary Theme Color (Existing - for backgrounds)
- `--theme-color` → Primary brand color (default: #3B82F6)
- `--theme-color-dark` → 15% darker for hover states
- `--theme-color-light` → 90% lighter for backgrounds

### Secondary Theme Color (New - for text on primary backgrounds)
- `--secondary-theme-color` → Secondary brand color (default: #FFFFFF)
- `--secondary-theme-color-dark` → 15% darker for hover states
- `--secondary-theme-color-light` → 90% lighter for backgrounds

## Where Secondary Color is Applied

The secondary theme color is automatically applied to:
- ✅ **Button text** (all buttons with primary background)
- ✅ **Header text** (dealership name and navigation links)
- ✅ **Footer text** (all footer content)
- ✅ **Navigation button text**
- ✅ **Mobile menu text and icons**

## Troubleshooting

### Migration Fails
**Error**: Column already exists
**Solution**: The migration has already been run. You can skip this step.

**Error**: Cannot connect to database
**Solution**: Ensure Docker container is running:
```bash
docker ps | grep jeal-prototype-db
```

### Color Not Updating
**Issue**: Color changes don't persist after page reload
**Checks**:
1. Check browser console for errors
2. Verify backend is running and accepting requests
3. Check that the PUT request succeeded (Status 200)
4. Verify database has been updated:
```bash
docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT id, name, secondary_theme_color FROM dealership;"
```

### CSS Variables Not Working
**Issue**: `var(--secondary-theme-color)` not applying color
**Checks**:
1. Inspect element in browser DevTools
2. Check the `:root` element for CSS custom properties
3. Verify `Layout.jsx` or `AdminContext.jsx` is setting the variables
4. Hard refresh the page (Ctrl+F5 or Cmd+Shift+R)

### Test Script Fails
**Error**: Connection refused
**Solution**: Ensure backend server is running on http://localhost:3000

**Error**: 404 Not Found
**Solution**: Verify the API endpoint path is correct and routes are registered

## Example Test Workflow

```bash
# 1. Run migration
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend/db/migrations/add_secondary_theme_color.sql

# 2. Start backend (in terminal 1)
cd backend
npm run dev

# 3. Start frontend (in terminal 2)
cd frontend
npm run dev

# 4. Run tests (in terminal 3)
node test_secondary_theme_color.js

# 5. Manual testing
# Open browser to http://localhost:5173/admin
# Login and navigate to Dealership Settings
# Change secondary color and save
# Visit public site to see text colors update
```

## Default Colors

If you want to reset to defaults:
- **Primary Theme Color**: #3B82F6 (blue) - used for backgrounds
- **Secondary Theme Color**: #FFFFFF (white) - used for text on primary backgrounds

## Next Steps

1. ✅ **Customize your dealership's colors** via the admin panel
2. ✅ **Use the CSS variables** in your components for consistent branding
3. ✅ **Test on different pages** to see the colors in action
4. ✅ **Verify text contrast** on buttons, headers, and footers
5. ✅ **Experiment with color combinations** to find the perfect match

## Additional Resources

- **Full Documentation**: `SECONDARY_THEME_COLOR_FEATURE.md`
- **Implementation Details**: `SECONDARY_THEME_COLOR_IMPLEMENTATION_SUMMARY.md`
- **UI Guide**: `SECONDARY_THEME_COLOR_UI_GUIDE.md`
- **Test Script**: `test_secondary_theme_color.js`

## Support

If you encounter any issues:
1. Check the browser console for errors
2. Check the backend server logs
3. Verify database migration completed successfully
4. Review the documentation files above
