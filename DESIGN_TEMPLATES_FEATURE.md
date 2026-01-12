# Design Templates Feature

## Overview
The Design Templates feature allows dealership administrators to quickly apply pre-set design themes or create and save custom templates. Each template is a combination of primary theme color, secondary theme color, body background color, and website font.

## Features

### 1. Pre-set Templates
- **8 Professional Templates**: Pre-configured design combinations ready to use
  - Modern Blue
  - Classic Black
  - Luxury Gold
  - Sporty Red
  - Elegant Silver
  - Eco Green
  - Premium Navy
  - Sunset Orange
- **One-Click Application**: Apply any pre-set template instantly
- **Read-Only**: Pre-set templates cannot be modified or deleted

### 2. Custom Templates
- **Save Current Settings**: Save your current design configuration as a custom template
- **Template Management**: Create, apply, and delete custom templates
- **Dealership-Specific**: Custom templates are only visible to your dealership
- **Template Metadata**: Add name and description to templates for easy identification

### 3. Template Components
Each template includes:
- **Primary Theme Color**: Header background and primary elements
- **Secondary Theme Color**: Buttons, accents, and complementary elements
- **Body Background Color**: Website background color
- **Font Family**: Website text font

## Quick Start Guide

### Applying a Pre-set Template

1. **Navigate to Settings**
   - Log in to the admin panel
   - Click "Settings" in the sidebar
   - Scroll to the "Design Templates" section at the top

2. **Browse Templates**
   - View pre-set templates in the "Pre-set Templates" section
   - Each card shows:
     - Template name and description
     - Color swatches (primary, secondary, background)
     - Font family name

3. **Apply a Template**
   - Click the "Apply Template" button on your chosen template
   - The theme colors and font will update immediately in the form below
   - Click "Save Changes" at the bottom to persist your selection

### Saving a Custom Template

1. **Configure Your Design**
   - Adjust the Primary Theme Color
   - Adjust the Secondary Theme Color
   - Adjust the Body Background Color
   - Select your preferred Website Font

2. **Save as Template**
   - Click "Save Current as Template" button (top right)
   - Enter a template name (required, max 100 characters)
   - Optionally add a description (max 500 characters)
   - Click "Save Template"

3. **Your Template is Now Available**
   - Find it in the "Your Custom Templates" section
   - Apply it anytime with one click
   - Delete it if you no longer need it

### Managing Custom Templates

**To Delete a Custom Template:**
1. Find the template in "Your Custom Templates"
2. Click the "Delete" button (red)
3. Confirm the deletion
4. Template is permanently removed

**Note:** You can only delete your own custom templates, not pre-set templates.

## Technical Implementation

### Database Schema

**Table: `design_templates`**
```sql
CREATE TABLE design_templates (
  id SERIAL PRIMARY KEY,
  name VARCHAR(100) NOT NULL,
  description TEXT,
  dealership_id INTEGER REFERENCES dealership(id) ON DELETE CASCADE,
  is_preset BOOLEAN DEFAULT false,
  theme_color VARCHAR(7) NOT NULL,
  secondary_theme_color VARCHAR(7) NOT NULL,
  body_background_color VARCHAR(7) NOT NULL,
  font_family VARCHAR(100) NOT NULL,
  created_at TIMESTAMP DEFAULT NOW()
);
```

**Key Constraints:**
- Preset templates have `is_preset = true` and `dealership_id = NULL`
- Custom templates have `is_preset = false` and a valid `dealership_id`
- Template names must be unique per dealership

### API Endpoints

#### GET /api/design-templates
Lists all templates available to the authenticated dealership.

**Authentication:** Required  
**Query Parameters:**
- `dealership_id` (optional) - For admin users to view a specific dealership's templates

**Behavior:**
- **Dealership Users:** Returns presets + their dealership's custom templates
- **Admin Users (with dealership_id):** Returns presets + that dealership's custom templates  
- **Admin Users (without dealership_id):** Returns only preset templates

**Returns:** Array of template objects (presets + custom templates)

**Example Response:**
```json
[
  {
    "id": 1,
    "name": "Modern Blue",
    "description": "Clean and professional blue theme with modern sans-serif font",
    "dealership_id": null,
    "is_preset": true,
    "theme_color": "#3B82F6",
    "secondary_theme_color": "#FFFFFF",
    "body_background_color": "#FFFFFF",
    "font_family": "inter",
    "created_at": "2026-01-09T00:00:00.000Z"
  }
]
```

#### POST /api/design-templates
Creates a new custom design template.

**Authentication:** Required  
**Permission:** `settings`  
**Query/Body Parameters:**
- `dealership_id` (optional) - For admin users to create templates for a specific dealership

**Request Body:**
```json
{
  "name": "My Custom Theme",
  "description": "Optional description",
  "theme_color": "#FF5733",
  "secondary_theme_color": "#FFFFFF",
  "body_background_color": "#F5F5F5",
  "font_family": "roboto"
}
```

**Returns:** Created template object  
**Status Codes:**
- 201: Template created successfully
- 400: Validation error or duplicate name
- 401: Not authenticated
- 403: Insufficient permissions

#### DELETE /api/design-templates/:id
Deletes a custom design template.

**Authentication:** Required  
**Permission:** `settings`  
**Parameters:** 
- `id` - Template ID to delete (URL parameter)
- `dealership_id` (optional) - For admin users (query parameter)

**Returns:** Success message  
**Status Codes:**
- 200: Template deleted successfully
- 404: Template not found or cannot be deleted (preset)
- 401: Not authenticated
- 403: Insufficient permissions

### Frontend Components

**TemplateSelector.jsx**
- Location: `frontend/src/components/admin/TemplateSelector.jsx`
- Props:
  - `currentSettings`: Object with current theme settings
  - `onApplyTemplate`: Callback function to apply template
- Features:
  - Fetches templates on mount
  - Displays templates in grid layout
  - Handles template creation and deletion
  - Shows success/error messages

**Integration in DealerSettings.jsx**
- Added to `frontend/src/pages/admin/DealerSettings.jsx`
- Positioned at the top of the settings page
- Only visible to users with `settings` permission
- Applies templates to form state, not directly to database
- Passes `selectedDealership.id` as query parameter for multi-tenant support

## User Permissions

**Required Permission:** `settings`

Only users with the `settings` permission can:
- Apply templates
- Create custom templates
- Delete custom templates

Users without this permission cannot see the template selector.

### User Type Behavior

**Dealership Owner/Staff:**
- Automatically scoped to their dealership
- See 8 preset templates + their dealership's custom templates
- Can create/delete custom templates for their dealership

**Admin Users:**
- Can view any dealership's templates by selecting that dealership
- When dealership selected: See presets + that dealership's custom templates
- When no dealership selected: See only preset templates
- Can create/delete templates for any dealership

## Validation & Security

### Input Validation
- **Template Name**: Required, max 100 characters
- **Description**: Optional, max 500 characters
- **Color Format**: Must be valid hex color (#RGB or #RRGGBB)
- **Font Family**: Must be a valid font identifier
- **Dealership ID**: Must be a valid integer (validated on POST and DELETE)

### Security Measures
- **XSS Prevention**: All text inputs are sanitized
- **Multi-tenancy**: Custom templates are scoped to dealership
- **Permission Enforcement**: Settings permission required
- **SQL Injection**: Parameterized queries used throughout
- **Admin Flexibility**: Admin users can manage templates for any dealership by selecting it
- **Session-based Auth**: Uses `req.session.user` for authentication
- **Parameter Validation**: All IDs validated to prevent NaN errors

## Font Options

The following fonts are available in templates:

| Font ID | Display Name | Font Stack |
|---------|-------------|------------|
| `system` | System Default | -apple-system, BlinkMacSystemFont, Segoe UI, Roboto |
| `inter` | Inter | Inter, sans-serif |
| `roboto` | Roboto | Roboto, sans-serif |
| `opensans` | Open Sans | Open Sans, sans-serif |
| `lato` | Lato | Lato, sans-serif |
| `montserrat` | Montserrat | Montserrat, sans-serif |
| `poppins` | Poppins | Poppins, sans-serif |
| `playfair` | Playfair Display | Playfair Display, serif |
| `nunito` | Nunito | Nunito, sans-serif |
| `times` | Times New Roman | Times New Roman, Times, serif |
| `arial` | Arial | Arial, Helvetica, sans-serif |
| `georgia` | Georgia | Georgia, serif |
| `verdana` | Verdana | Verdana, Geneva, sans-serif |
| `courier` | Courier New | Courier New, Courier, monospace |

## Pre-set Template Catalog

### Modern Blue
- **Primary Color:** #3B82F6 (Bright Blue)
- **Secondary Color:** #FFFFFF (White)
- **Background:** #FFFFFF (White)
- **Font:** Inter
- **Best For:** Professional dealerships, clean corporate look

### Classic Black
- **Primary Color:** #000000 (Black)
- **Secondary Color:** #FFFFFF (White)
- **Background:** #F9FAFB (Light Gray)
- **Font:** Playfair Display
- **Best For:** Luxury brands, elegant presentation

### Luxury Gold
- **Primary Color:** #D4AF37 (Gold)
- **Secondary Color:** #1F2937 (Dark Gray)
- **Background:** #FFFFFF (White)
- **Font:** Montserrat
- **Best For:** Premium dealerships, high-end vehicles

### Sporty Red
- **Primary Color:** #DC2626 (Red)
- **Secondary Color:** #FFFFFF (White)
- **Background:** #FAFAFA (Off White)
- **Font:** Poppins
- **Best For:** Performance vehicles, energetic brands

### Elegant Silver
- **Primary Color:** #71717A (Gray)
- **Secondary Color:** #FFFFFF (White)
- **Background:** #F4F4F5 (Light Gray)
- **Font:** Roboto
- **Best For:** Modern minimalist design

### Eco Green
- **Primary Color:** #10B981 (Green)
- **Secondary Color:** #FFFFFF (White)
- **Background:** #F0FDF4 (Light Green)
- **Font:** Lato
- **Best For:** Electric vehicles, eco-friendly focus

### Premium Navy
- **Primary Color:** #1E3A8A (Navy Blue)
- **Secondary Color:** #FFFFFF (White)
- **Background:** #EFF6FF (Light Blue)
- **Font:** Open Sans
- **Best For:** Professional corporate identity

### Sunset Orange
- **Primary Color:** #F97316 (Orange)
- **Secondary Color:** #FFFFFF (White)
- **Background:** #FFF7ED (Light Orange)
- **Font:** Nunito
- **Best For:** Welcoming, friendly atmosphere

## Testing

### Manual Testing Steps

1. **Test Template Application**
   - Log in as admin with settings permission
   - Navigate to Settings page
   - Apply each pre-set template
   - Verify colors and font update correctly
   - Save changes and verify persistence

2. **Test Custom Template Creation**
   - Configure custom colors and font
   - Click "Save Current as Template"
   - Enter name and description
   - Verify template appears in "Your Custom Templates"
   - Apply the custom template
   - Verify settings apply correctly

3. **Test Template Deletion**
   - Create a custom template
   - Delete the custom template
   - Confirm deletion
   - Verify template is removed from list
   - Try to delete a pre-set template (should not be possible)

4. **Test Validation**
   - Try to create template with empty name (should fail)
   - Try to create template with duplicate name (should fail)
   - Try to create template with invalid color format (should fail)
   - Verify error messages display correctly

5. **Test Permissions**
   - Log in as user without settings permission
   - Verify template selector is hidden
   - Verify API endpoints return 403 for unauthorized users

## Troubleshooting

### Templates Not Loading
**Problem:** Template selector shows "Loading templates..." indefinitely  
**Solution:**
- Check browser console for errors
- Verify user is authenticated
- Ensure database migration ran successfully
- Check backend server logs
- For admin users: Ensure a dealership is selected in the dropdown

### Admin User Not Seeing Custom Templates
**Problem:** Admin can only see preset templates  
**Solution:**
- Select a dealership from the dropdown (top left of admin panel)
- Navigate to Settings page
- You should now see that dealership's custom templates

### Cannot Save Custom Template
**Problem:** Error when trying to save template  
**Solution:**
- Verify template name is unique
- Check that all required fields are filled
- Ensure colors are in valid hex format
- Verify user has settings permission
- For admin users: Ensure a dealership is selected

### Template Deletion Fails (NaN Error)
**Problem:** Error "invalid input syntax for type integer: NaN"  
**Solution:**
- This was fixed in version 1.1.1
- Ensure you're running the latest version
- For admin users: Ensure a dealership is selected
- Refresh browser (Ctrl+F5) to clear cache

### Template Deleted But Still Visible
**Problem:** Deleted template still appears in list  
**Solution:**
- Refresh the page
- Check browser console for errors
- Verify deletion API call succeeded

### Colors Not Applying
**Problem:** Template applies but colors don't change  
**Solution:**
- Remember to click "Save Changes" after applying template
- Check that form values updated correctly
- Verify no browser extensions interfering with color picker

## Future Enhancements

Potential improvements for future versions:
- Template preview mode
- Template export/import functionality
- Template sharing between dealerships (with permission)
- More pre-set templates
- Template categories/tags
- Template search and filtering
- Template versioning
- Bulk template operations

## Migration Files

**Migration:** `backend/db/migrations/011_add_design_templates.sql`
- Creates `design_templates` table
- Seeds 8 pre-set templates
- Creates necessary indexes

**To Run Migration:**
```bash
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend/db/migrations/011_add_design_templates.sql
```

**To Verify:**
```bash
docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT * FROM design_templates WHERE is_preset = true;"
```

## Files Created/Modified

### New Files
- `backend/db/migrations/011_add_design_templates.sql` - Database migration
- `backend/db/designTemplates.js` - Database query functions
- `backend/routes/designTemplates.js` - API routes
- `frontend/src/components/admin/TemplateSelector.jsx` - UI component
- `DESIGN_TEMPLATES_FEATURE.md` - This documentation

### Modified Files
- `backend/server.js` - Added design templates route
- `frontend/src/pages/admin/DealerSettings.jsx` - Integrated template selector

## Summary

The Design Templates feature provides a powerful and user-friendly way for dealership administrators to manage their website's visual appearance. With 8 pre-set professional templates and the ability to create unlimited custom templates, dealerships can quickly establish and maintain a consistent brand identity across their website.
