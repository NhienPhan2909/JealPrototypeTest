# Font Customization Feature

## Overview
Dealership administrators can now customize the font family used across their entire website from the CMS Admin page. This allows each dealership to match their branding preferences.

## How to Use

### For Administrators:

1. **Access Dealer Settings**
   - Log in to the admin panel at `/admin/login`
   - Navigate to "Dealership Settings" from the admin menu

2. **Select a Font**
   - Scroll to the "Website Font" section
   - Choose from 10 available font options:
     - System Default (Sans Serif)
     - Arial
     - Times New Roman
     - Georgia
     - Verdana
     - Courier New
     - Comic Sans MS
     - Trebuchet MS
     - Impact
     - Palatino

3. **Preview and Save**
   - A live preview shows how the font looks
   - Click "Save Settings" to apply the changes
   - All text on your dealership website will immediately update to use the selected font

## Available Fonts

| Font Name | Style | Best For |
|-----------|-------|----------|
| System Default | Clean, modern sans-serif | General use (default) |
| Arial | Classic sans-serif | Professional, easy to read |
| Times New Roman | Traditional serif | Formal, traditional feel |
| Georgia | Web-optimized serif | Elegant, readable |
| Verdana | Wide sans-serif | High readability |
| Courier New | Monospace | Technical, retro look |
| Comic Sans MS | Casual, friendly | Playful branding |
| Trebuchet MS | Modern sans-serif | Contemporary feel |
| Impact | Bold, condensed | Strong headlines |
| Palatino | Classic serif | Sophisticated, book-like |

## Technical Details

### Database Changes
- New column: `dealership.font_family` (VARCHAR(100))
- Default value: `'system'`
- Migration file: `backend/db/migrations/add_font_family.sql`

### Backend Changes
- Updated `backend/db/dealers.js` to handle `font_family` field
- Font family is stored as a string identifier (e.g., 'times', 'arial')

### Frontend Changes
1. **DealerSettings Component** (`frontend/src/pages/admin/DealerSettings.jsx`)
   - Added font selector dropdown
   - Added live font preview
   - Sends font_family in update requests

2. **Layout Component** (`frontend/src/components/Layout.jsx`)
   - Applies font family to `document.body.style.fontFamily`
   - Maps font identifiers to CSS font-family values
   - Updates dynamically when dealership data changes

### Font Mapping
```javascript
{
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
}
```

## Migration Instructions

To apply the database changes to an existing installation:

```bash
# Using Docker (recommended)
Get-Content backend\db\migrations\add_font_family.sql | docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype

# Using psql directly
psql -U postgres -d jeal_prototype < backend/db/migrations/add_font_family.sql
```

## Testing

1. **Verify Database Column:**
   ```bash
   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT column_name, data_type FROM information_schema.columns WHERE table_name = 'dealership' AND column_name = 'font_family';"
   ```

2. **Test Font Changes:**
   - Log in to admin panel
   - Go to Dealership Settings
   - Change font to "Times New Roman"
   - Save settings
   - Visit the public website
   - Verify all text uses Times New Roman

3. **Test Multiple Dealerships:**
   - Each dealership should have independent font settings
   - Changing one dealership's font should not affect others

## Browser Compatibility

All selected fonts are web-safe fonts supported by all major browsers:
- Chrome, Firefox, Safari, Edge
- Windows, macOS, Linux
- Mobile browsers (iOS Safari, Chrome Mobile)

## Future Enhancements

Possible future improvements:
- Custom Google Fonts integration
- Font weight customization (light, regular, bold)
- Separate fonts for headings vs body text
- Font size controls
- Import custom fonts via file upload
