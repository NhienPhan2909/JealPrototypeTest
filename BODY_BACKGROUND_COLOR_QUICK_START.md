# Body Background Color - Quick Start Guide

## What Is This?
The body background color feature allows you to customize the background color of your dealership website's main content area from the CMS admin dashboard.

## Quick Setup (5 Minutes)

### Step 1: Access Settings
1. Log in to your admin dashboard
2. Click **Settings** in the navigation menu

### Step 2: Find Body Background Color
Scroll down to the **Body Background Color** section (located between Secondary Theme Color and Website Font)

### Step 3: Choose Your Color
Pick a color using one of these methods:

**Option A - Color Picker**
- Click the colored square on the left
- Select your desired color from the color picker

**Option B - Type Hex Code**
- Enter a hex code in the text field (e.g., `#F5F5F5`)
- Common colors:
  - `#FFFFFF` - White (default)
  - `#F9FAFB` - Very light gray
  - `#F3F4F6` - Light gray
  - `#E5E7EB` - Medium light gray

**Option C - Reset to Default**
- Click "Reset to Default" button for white background

### Step 4: Preview
Check the preview box below the color picker to see how it will look

### Step 5: Save
Click **Save Settings** at the bottom of the page

### Step 6: Verify
Visit your public website to see the new background color applied

## Tips
✅ **Do:**
- Use light colors for better readability
- Preview before saving
- Test on multiple pages

❌ **Don't:**
- Use very dark backgrounds (makes text hard to read)
- Forget to save your changes
- Use colors that clash with your brand

## Color Suggestions by Brand Type

### Professional/Corporate
- `#FFFFFF` (White)
- `#F9FAFB` (Very light gray)
- `#F0F4F8` (Light blue-gray)

### Modern/Tech
- `#F5F5F5` (Light gray)
- `#FAFAFA` (Off-white)
- `#F8F9FA` (Cool white)

### Warm/Friendly
- `#FFFBF0` (Cream)
- `#FFF9F0` (Warm white)
- `#FAF9F6` (Linen)

### Eco/Green
- `#F0FDF4` (Light mint)
- `#F7FEF9` (Very light green)

## Troubleshooting

**Q: The color didn't change after saving**
A: Clear your browser cache (Ctrl+Shift+R on most browsers)

**Q: Text is hard to read**
A: Choose a lighter background color with better contrast

**Q: Color looks different on mobile**
A: Some displays show colors differently - preview on multiple devices

## Database Migration

If you're setting up the feature for the first time, run:

```bash
# PowerShell
Get-Content backend\db\migrations\add_body_background_color.sql | docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype

# Linux/Mac
cat backend/db/migrations/add_body_background_color.sql | docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype
```

## Technical Details
- **Default Value**: `#FFFFFF` (White)
- **Format**: Hex color codes (#RRGGBB or #RGB)
- **Validation**: 7 characters max, must be valid hex
- **CSS Variable**: `--body-background-color`

## Related Settings
- **Primary Theme Color**: Header/footer background
- **Secondary Theme Color**: Buttons and accents
- **Website Font**: Typography settings

## Need Help?
Refer to the full documentation: `BODY_BACKGROUND_COLOR_FEATURE.md`
