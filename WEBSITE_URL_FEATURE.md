# Website URL Management Feature

**Status:** ✅ Implemented  
**Last Updated:** 2026-01-14  
**Version:** 1.0

---

## Overview

Each dealership can now have a unique website URL/domain that can be managed by System Administrators through the CMS admin panel. This feature allows each dealership to have its own custom URL identifier.

---

## Key Features

✅ **Unique URLs** - Each dealership can have a custom URL (e.g., `acme-auto.com`, `premium-motors.com`)  
✅ **Admin Management** - Only System Administrators can manage website URLs  
✅ **Database Level** - URL uniqueness enforced at database level with unique constraint  
✅ **Optional Field** - Website URL is optional when creating dealerships  
✅ **UI Integration** - Available in both Dealership Settings and Dealership Management pages  

---

## Database Changes

### Migration: `012_add_website_url.sql`

**What was added:**
- New column `website_url` (VARCHAR 255, UNIQUE) to the `dealership` table
- Database index `idx_dealership_website_url` for fast lookups
- Column comment for documentation

**How to run:**
```bash
node backend/db/migrations/run_website_url_migration.js
```

**Schema changes:**
```sql
ALTER TABLE dealership
ADD COLUMN IF NOT EXISTS website_url VARCHAR(255) UNIQUE;

CREATE INDEX IF NOT EXISTS idx_dealership_website_url ON dealership(website_url);
```

---

## Backend Changes

### Database Layer (`backend/db/dealers.js`)

**Updated Functions:**
- `create()` - Now accepts `website_url` parameter when creating dealerships
- `update()` - Now accepts `website_url` parameter when updating dealerships

**Example Usage:**
```javascript
// Create dealership with website URL
const newDealer = await dealersDb.create({
  name: 'Acme Auto Sales',
  address: '123 Main St',
  phone: '555-1234',
  email: 'info@acme.com',
  website_url: 'acme-auto.com'
});

// Update website URL
const updated = await dealersDb.update(1, {
  website_url: 'new-acme-motors.com'
});
```

### API Routes (`backend/routes/dealers.js`)

**Updated Endpoints:**

**POST /api/dealers** - Create dealership
- Now accepts optional `website_url` field in request body
- Access: Admin only (`requireAuth`, `requireAdmin`)

**PUT /api/dealers/:id** - Update dealership
- Now accepts optional `website_url` field in request body
- Access: Authenticated users with settings permission

**Validation:**
- Maximum length: 255 characters
- Must be unique across all dealerships (enforced at DB level)
- No format validation (allows flexibility for subdomains, custom domains, etc.)

---

## Frontend Changes

### 1. Dealership Settings Page (`frontend/src/pages/admin/DealerSettings.jsx`)

**Location:** Admin Panel → Settings

**What was added:**
- New "Website URL" input field
- State management for `websiteUrl`
- Loads existing URL from database on page load
- Saves URL when form is submitted

**UI Placement:** Between "Dealership Name" and "Theme Color Picker" sections

**Features:**
- Text input with placeholder "e.g., acme-auto.com"
- Helper text explaining the field's purpose
- Max length: 255 characters
- Read-only for users without settings permission
- Saves along with other dealership settings

### 2. Dealership Management Page (`frontend/src/pages/admin/DealershipManagement.jsx`)

**Location:** Admin Panel → Dealerships

**What was added:**
- "Website URL" field in create dealership form
- "Website URL" column in dealerships list table
- State management in form data

**Create Form:**
- Optional field when creating new dealerships
- Validates uniqueness at backend
- Displays error if URL already exists

**List Table:**
- New column showing website URL for each dealership
- Displays "Not set" (italicized, gray) if no URL configured
- Positioned between "Name" and "Email" columns

---

## User Guide

### For System Administrators

#### Setting a Website URL (New Dealership)

1. Navigate to **Admin Panel → Dealerships**
2. Click **"Create New Dealership"** button
3. Fill in required fields (Name, Address, Phone, Email)
4. **Optional:** Enter Website URL (e.g., `acme-auto.com`)
5. Click **"Create Dealership"**

#### Setting a Website URL (Existing Dealership)

1. Navigate to **Admin Panel → Settings**
2. Select the dealership from the dropdown (top of page)
3. Find the **"Website URL"** field (below Dealership Name)
4. Enter the custom URL (e.g., `premium-motors.com`)
5. Scroll down and click **"Update Dealership Settings"**

#### Viewing Website URLs

1. Navigate to **Admin Panel → Dealerships**
2. View the **"Website URL"** column in the table
3. URLs are displayed if set, otherwise shows "Not set"

---

## Technical Details

### Database Structure

**Table:** `dealership`  
**Column:** `website_url`  
**Type:** VARCHAR(255)  
**Constraints:** UNIQUE, NULL allowed  
**Index:** `idx_dealership_website_url` (for fast lookups)

### Field Validation

**Backend:**
- Maximum length: 255 characters
- Uniqueness enforced at database level
- No specific format validation (flexible for various URL types)

**Frontend:**
- Maximum length: 255 characters (HTML maxLength attribute)
- Optional field (not required)
- Placeholder text provides examples

### Future Enhancements (Not Yet Implemented)

The following features could be added in future iterations:

1. **Domain-based routing** - Automatically route users to the correct dealership based on domain
2. **URL format validation** - Enforce specific URL patterns (e.g., must be valid domain)
3. **URL availability checker** - Real-time validation in UI
4. **Custom domain mapping** - Map custom domains to dealership IDs
5. **URL history tracking** - Track URL changes over time

---

## Testing

### Manual Testing Checklist

**Database Migration:**
- [x] Migration runs successfully
- [x] Column is created with correct type and constraints
- [x] Index is created
- [x] Existing dealerships have NULL values

**Backend API:**
- [ ] POST /api/dealers accepts website_url
- [ ] PUT /api/dealers/:id accepts website_url
- [ ] Duplicate URLs are rejected
- [ ] NULL values are accepted
- [ ] URLs are returned in GET requests

**Frontend - Settings Page:**
- [ ] Field appears in settings form
- [ ] Existing URL is loaded on page load
- [ ] URL can be updated
- [ ] Success message displays after save
- [ ] Field is read-only for users without permission

**Frontend - Management Page:**
- [ ] Field appears in create form
- [ ] URL can be set when creating dealership
- [ ] Column appears in dealerships table
- [ ] "Not set" displays for NULL values
- [ ] Actual URLs display when set

### Test Data

```javascript
// Create test dealership with URL
const testData = {
  name: 'Test Auto Sales',
  address: '123 Test St',
  phone: '555-0000',
  email: 'test@test.com',
  website_url: 'test-auto.com'
};

// Create test dealership without URL
const testData2 = {
  name: 'Another Auto Sales',
  address: '456 Main St',
  phone: '555-0001',
  email: 'another@test.com'
  // website_url omitted (should be NULL)
};
```

---

## Files Modified

### Backend
1. `backend/db/migrations/012_add_website_url.sql` - Migration SQL
2. `backend/db/migrations/run_website_url_migration.js` - Migration runner
3. `backend/db/dealers.js` - Database functions updated
4. `backend/routes/dealers.js` - API routes updated

### Frontend
1. `frontend/src/pages/admin/DealerSettings.jsx` - Settings page updated
2. `frontend/src/pages/admin/DealershipManagement.jsx` - Management page updated

### Documentation
1. `WEBSITE_URL_FEATURE.md` - This file

---

## API Reference

### POST /api/dealers

**Request Body:**
```json
{
  "name": "Acme Auto Sales",
  "address": "123 Main St",
  "phone": "555-1234",
  "email": "info@acme.com",
  "website_url": "acme-auto.com"  // Optional
}
```

**Response (Success):**
```json
{
  "id": 3,
  "name": "Acme Auto Sales",
  "address": "123 Main St",
  "phone": "555-1234",
  "email": "info@acme.com",
  "website_url": "acme-auto.com",
  "created_at": "2026-01-14T02:47:46.523Z",
  ...
}
```

### PUT /api/dealers/:id

**Request Body:**
```json
{
  "name": "Updated Name",
  "website_url": "new-url.com"  // Optional
}
```

**Response (Success):**
```json
{
  "id": 1,
  "name": "Updated Name",
  "website_url": "new-url.com",
  ...
}
```

**Error (Duplicate URL):**
```json
{
  "error": "Failed to update dealership"
}
```
*Note: Database will reject duplicate URLs with UNIQUE constraint error*

---

## Troubleshooting

### Issue: Migration fails with "column already exists"
**Solution:** Migration already ran. Check with:
```sql
SELECT column_name FROM information_schema.columns 
WHERE table_name = 'dealership' AND column_name = 'website_url';
```

### Issue: Duplicate URL error when saving
**Solution:** Each URL must be unique. Choose a different URL or remove the existing one from another dealership.

### Issue: Website URL field not showing in UI
**Solution:** Clear browser cache and reload. Ensure you're logged in as admin.

### Issue: Cannot save empty website URL
**Solution:** This should work. Empty/null values are allowed. Check browser console for errors.

---

## Security Considerations

✅ **Access Control** - Only System Administrators can create/manage dealerships  
✅ **Input Validation** - Maximum length enforced (255 chars)  
✅ **Uniqueness** - Database constraint prevents duplicates  
✅ **SQL Injection** - Protected by parameterized queries  
✅ **XSS Prevention** - URLs are not sanitized as HTML (stored as-is)  

**Note:** URL format validation is intentionally minimal to allow flexibility. Add stricter validation if needed.

---

## Migration History

| Version | Date | Description |
|---------|------|-------------|
| 1.0 | 2026-01-14 | Initial implementation - added website_url field |

---

## Related Documentation

- `DEALERSHIP_MANAGEMENT_FEATURE.md` - Dealership management overview
- `DEALERSHIP_CREATION_IMPLEMENTATION_SUMMARY.md` - Dealership creation details
- `backend/db/schema.sql` - Complete database schema

---

## Support

For issues or questions about this feature:
1. Check the troubleshooting section above
2. Review the test checklist for common issues
3. Check database schema with `\d dealership` in PostgreSQL

---

**End of Documentation**
