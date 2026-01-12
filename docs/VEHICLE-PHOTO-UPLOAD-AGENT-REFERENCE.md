# Vehicle Photo Upload - Quick Reference for Agents

**Date:** 2025-11-28  
**Type:** Bug Fix & Implementation Change  
**File:** `frontend/src/pages/admin/VehicleForm.jsx`

---

## What Happened

**Problem:** Page freeze when clicking "Upload Photos" in Vehicle Manager  
**Cause:** Cloudinary widget had race conditions and loading issues  
**Solution:** Switched to file input + `/api/upload` endpoint (same as DealerSettings hero background)

---

## Implementation Pattern

### OLD (Removed)
```javascript
// Cloudinary widget approach (caused freeze)
const handleUploadClick = () => {
  window.cloudinary.openUploadWidget({ ... }, (error, result) => {
    // Complex callback handling
  });
};

<button onClick={handleUploadClick}>Upload Photos</button>
```

### NEW (Current)
```javascript
// File input + API approach (reliable)
const handlePhotoUpload = async (e) => {
  const files = Array.from(e.target.files || []);
  
  // Client validation
  // - Max 10 images total
  // - JPG/PNG/WebP only
  // - 5MB max per file
  
  // Upload each file
  for (const file of files) {
    const formData = new FormData();
    formData.append('image', file);
    const response = await fetch('/api/upload', {
      method: 'POST',
      credentials: 'include',
      body: formData
    });
    const data = await response.json();
    uploadedUrls.push(data.url);
  }
  
  setImages(prev => [...prev, ...uploadedUrls]);
};

<label className="btn-primary cursor-pointer">
  Upload Photos
  <input
    type="file"
    accept="image/jpeg,image/png,image/webp"
    multiple
    onChange={handlePhotoUpload}
    className="hidden"
  />
</label>
```

---

## Upload Strategy by Feature

| Feature | Method | Why |
|---------|--------|-----|
| **Vehicle Photos** | File Input + API | Multiple files, reliable, no freeze |
| **Hero Background** | File Input + API | Large files, proven reliable |
| **Logo Upload** | Cloudinary Widget | Needs built-in cropping, works reliably |

**Rule:** Use file input + `/api/upload` for multiple files or when widget causes issues.

---

## Key Files

### Modified
- `frontend/src/pages/admin/VehicleForm.jsx` - Complete upload rewrite

### Unchanged (Already Exists)
- `backend/routes/upload.js` - Upload endpoint (from Story 1.6)
- `backend/routes/vehicles.js` - Vehicle CRUD
- Database schema - No changes (JSONB array of URLs)

---

## Validation Checklist

Client-side (VehicleForm.jsx):
- [ ] File type: JPG/PNG/WebP only
- [ ] File size: 5MB max per file
- [ ] Total count: 10 images max
- [ ] Empty selection: Return early

Server-side (backend/routes/upload.js):
- [ ] Multer file filter (MIME type)
- [ ] Multer size limit (5MB)
- [ ] Cloudinary upload error handling

---

## User Experience

### States
1. **Default:** "Upload Photos" button enabled
2. **Uploading:** "Uploading..." with disabled button
3. **Success:** Green message "Successfully uploaded X image(s)!"
4. **Error:** Red message with validation/upload error
5. **Max Reached:** "Maximum 10 photos reached" + disabled button

### Counter
- "X / 10 photos uploaded" updates dynamically

---

## Testing Checklist

Basic Functionality:
- [ ] Click "Upload Photos" → file picker opens (no freeze)
- [ ] Select 1 image → uploads, thumbnail appears
- [ ] Select 5 images → all upload successfully
- [ ] Reach 10 images → button disables
- [ ] Click X on thumbnail → image removes
- [ ] Save vehicle → images persist

Validation:
- [ ] Upload PDF → error message
- [ ] Upload 7MB file → error message
- [ ] Select 12 files → error message

---

## API Endpoint

```
POST /api/upload
Content-Type: multipart/form-data

Body:
  image: <File>

Response (Success):
{
  "url": "https://res.cloudinary.com/.../abc123.jpg"
}

Response (Error):
{
  "error": "File too large. Maximum size is 5MB."
}
```

**Location:** `backend/routes/upload.js` (no changes needed)

---

## Database Schema

```sql
-- vehicles table (unchanged)
images JSONB DEFAULT '[]'
```

**Format:**
```json
["https://res.cloudinary.com/...", "https://res.cloudinary.com/..."]
```

---

## Related Documentation

- **Comprehensive:** `docs/CHANGELOG-VEHICLE-PHOTO-UPLOAD-FIX-2025-11-28.md`
- **Architecture:** `docs/architecture/external-apis-cloudinary-integration.md`
- **Components:** `docs/architecture/components.md`
- **Quick Updates:** `docs/README-FOR-AGENTS.md`
- **Original Story:** `docs/stories/1.6.story.md` (Cloudinary integration)

---

## Quick Debugging

**If upload fails:**
1. Check browser console for fetch errors
2. Check network tab for 400/500 responses
3. Verify `/api/upload` endpoint is running
4. Check Cloudinary credentials in `.env`
5. Verify file meets validation criteria

**If page freezes:**
- Should no longer happen (widget removed)
- If it does, check for JavaScript errors in console
- Verify file input is not triggering form submission

---

## Future Agents: Pattern to Follow

When implementing image uploads in new features:

1. **Use file input + `/api/upload`** for:
   - Multiple file uploads
   - Large file uploads
   - When reliability is critical

2. **Use Cloudinary widget** only when:
   - Need built-in cropping UI
   - Single file only
   - Widget features are essential

3. **Always include:**
   - Client-side validation (type, size, count)
   - Server-side validation (multer)
   - Upload progress indicator
   - Success/error feedback
   - File input reset after upload

---

**Last Updated:** 2025-11-28  
**Status:** ✅ Deployed & Working
