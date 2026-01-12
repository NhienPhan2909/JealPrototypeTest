# Hero Media Bug Fix - Carousel Not Displaying

## Issue Description
When selecting "Image Carousel" option in the CMS admin and uploading carousel images, clicking "Save Settings" shows a success message, but the public website still displays the old single image instead of the carousel.

## Root Cause
The backend API route (`backend/routes/dealers.js`) was **not** extracting or saving the new hero media fields from the request body:
- `hero_type`
- `hero_video_url`
- `hero_carousel_images`

Even though the frontend was correctly sending these fields in the PUT request, the backend was ignoring them and only saving the old `hero_background_image` field.

## Database Evidence
Before fix:
```sql
SELECT id, name, hero_type, hero_carousel_images FROM dealership WHERE id = 1;

 id |      name       | hero_type | hero_carousel_images
----+-----------------+-----------+----------------------
  1 | Acme Auto Sales | image     | []
```

The `hero_type` remained `'image'` even after selecting carousel and uploading images.

## Fix Applied

### File: `backend/routes/dealers.js`

**Change 1: Line 192** - Added new fields to request body destructuring:
```javascript
// BEFORE
const { name, address, phone, email, logo_url, hours, finance_policy, warranty_policy, about, hero_background_image, theme_color, secondary_theme_color, font_family, navigation_config, facebook_url, instagram_url } = req.body;

// AFTER
const { name, address, phone, email, logo_url, hours, finance_policy, warranty_policy, about, hero_background_image, hero_type, hero_video_url, hero_carousel_images, theme_color, secondary_theme_color, font_family, navigation_config, facebook_url, instagram_url } = req.body;
```

**Change 2: Lines 247-251** - Added new fields to updateData object:
```javascript
// BEFORE
if (sanitizedAbout !== undefined) updateData.about = sanitizedAbout;
if (hero_background_image !== undefined) updateData.hero_background_image = hero_background_image;
if (theme_color !== undefined) updateData.theme_color = theme_color;

// AFTER
if (sanitizedAbout !== undefined) updateData.about = sanitizedAbout;
if (hero_background_image !== undefined) updateData.hero_background_image = hero_background_image;
if (hero_type !== undefined) updateData.hero_type = hero_type;
if (hero_video_url !== undefined) updateData.hero_video_url = hero_video_url;
if (hero_carousel_images !== undefined) updateData.hero_carousel_images = hero_carousel_images;
if (theme_color !== undefined) updateData.theme_color = theme_color;
```

**Change 3: Lines 169-176** - Updated JSDoc documentation:
```javascript
/**
 * @param {string} [req.body.hero_background_image] - Hero background image URL (optional)
 * @param {string} [req.body.hero_type] - Hero type: 'image', 'video', or 'carousel' (optional, default: 'image')
 * @param {string} [req.body.hero_video_url] - Hero background video URL (optional)
 * @param {Array<string>} [req.body.hero_carousel_images] - Array of hero carousel image URLs (optional)
 */
```

## How to Apply Fix

### Step 1: Restart Backend Server
The code changes have been applied, but the server needs to be restarted:

```bash
# If running backend in terminal:
# Press Ctrl+C to stop, then:
cd backend
npm start

# If running with nodemon (auto-restart):
# Just save the file, it will restart automatically
```

### Step 2: Test the Fix

#### Option A: Manual Testing in CMS
1. Open CMS admin → **Settings** page
2. Scroll to **Home Page Hero Section**
3. Select **"Image Carousel"** radio button
4. Upload 2-3 carousel images
5. Click **"Save Settings"**
6. Visit public homepage
7. ✅ Carousel should now display with rotating images

#### Option B: Automated Testing
Run the test script:
```bash
node test_hero_media.js
```

Expected output:
```
✅ Update successful!
   hero_type: carousel
   carousel_images count: 3

✅ Fetch successful!
   hero_type: carousel
   hero_carousel_images: [...]

✅ All tests passed! Hero media is working correctly.
```

### Step 3: Verify Database
Check that hero_type is correctly saved:
```bash
docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT id, name, hero_type, hero_carousel_images FROM dealership WHERE id = 1;"
```

Expected result after saving carousel:
```
 id |      name       | hero_type | hero_carousel_images
----+-----------------+-----------+----------------------
  1 | Acme Auto Sales | carousel  | ["url1", "url2", ...]
```

## Why This Happened

The feature was implemented in multiple stages:
1. ✅ Database migration - Columns created successfully
2. ✅ Database layer (`db/dealers.js`) - Update function handles new fields
3. ✅ Frontend admin UI - Sends all fields correctly
4. ✅ Frontend public page - Renders all types correctly
5. ❌ **Backend API route - MISSED extracting new fields from request**

The backend route was using destructuring to extract specific fields from `req.body`, and the new fields were not added to this destructuring statement. This meant they were silently ignored even though they were present in the request.

## Prevention for Future

When adding new database fields that need to be user-editable:

1. **Database**: Add columns (migration) ✓
2. **Database Layer**: Update query functions ✓
3. **Backend Route**: 
   - ⚠️ Add to destructuring in PUT/POST handlers
   - ⚠️ Add to updateData/createData objects
   - ⚠️ Update JSDoc comments
4. **Frontend Admin**: Add UI controls ✓
5. **Frontend Public**: Add rendering logic ✓

**Checklist for Backend Routes**:
```javascript
// 1. Destructure from req.body
const { existing_fields, NEW_FIELD } = req.body;

// 2. Add to update/create object
if (NEW_FIELD !== undefined) updateData.NEW_FIELD = NEW_FIELD;

// 3. Update JSDoc
* @param {type} [req.body.NEW_FIELD] - Description
```

## Related Files
- **Fixed**: `backend/routes/dealers.js`
- **Already Correct**: 
  - `backend/db/dealers.js`
  - `frontend/src/pages/admin/DealerSettings.jsx`
  - `frontend/src/pages/public/Home.jsx`
  - `frontend/src/components/HeroCarousel.jsx`

## Testing Checklist

After applying fix and restarting backend:

- [ ] Can select "Image Carousel" radio button
- [ ] Can upload multiple carousel images
- [ ] Click "Save Settings" shows success
- [ ] Database shows `hero_type = 'carousel'`
- [ ] Database shows carousel images in JSONB array
- [ ] Public homepage displays carousel
- [ ] Carousel auto-rotates every 5 seconds
- [ ] Navigation arrows work (previous/next)
- [ ] Dot indicators work (click to jump)
- [ ] Can switch back to "Single Image"
- [ ] Can switch to "Video" type
- [ ] All types save and display correctly

## Status
- **Identified**: 2026-01-04
- **Fixed**: 2026-01-04
- **Tested**: Pending backend restart
- **Severity**: High (core feature non-functional)
- **Impact**: Carousel and video hero types could not be saved

## Next Steps
1. Restart backend server
2. Test carousel functionality
3. Test video functionality  
4. Update documentation if needed
5. Consider adding integration tests for API routes
