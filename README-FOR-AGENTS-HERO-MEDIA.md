# README FOR AGENTS - Hero Media Enhancement Feature

## 🎯 Quick Context

**What**: Homepage hero section now supports three media types: single image, video background, and image carousel.

**Why**: Dealerships needed more dynamic and engaging homepage options beyond a static image.

**Status**: ✅ Implemented, Deployed, and Tested (as of 2026-01-04)

---

## 📋 Feature Summary

### Business Value
- Enables dealerships to showcase multiple aspects of their business
- Creates more engaging homepage experiences
- Supports modern marketing with video content
- Provides flexibility for seasonal/promotional updates

### User Impact
- **Admins**: Can choose between 3 hero types and manage media from CMS
- **Visitors**: See more dynamic and professional dealership homepages

---

## 🏗️ Technical Architecture

### Database Layer
**Table**: `dealership`

```sql
-- New columns added via migration
hero_type VARCHAR(20) DEFAULT 'image' 
  CHECK (hero_type IN ('image', 'video', 'carousel'))
hero_video_url TEXT
hero_carousel_images JSONB DEFAULT '[]'::jsonb
```

**Migration File**: `backend/db/migrations/20260104_add_hero_media_options.sql`

### Backend Layer
**Files Modified**:
- `backend/db/dealers.js` - Database query functions
- `backend/routes/dealers.js` - API route handlers

**API Endpoint**:
```
PUT /api/dealers/:id
Body: {
  hero_type: 'carousel',
  hero_carousel_images: ['url1', 'url2', 'url3'],
  hero_video_url: null,
  // ... other fields
}
```

### Frontend Layer
**New Component**:
- `frontend/src/components/HeroCarousel.jsx` - Carousel with auto-rotation

**Modified Components**:
- `frontend/src/pages/admin/DealerSettings.jsx` - Admin UI for hero management
- `frontend/src/pages/public/Home.jsx` - Public hero rendering

---

## 🔑 Key Implementation Details

### 1. Hero Type Selection (Admin)
```jsx
// Radio button selector with 3 options
<RadioGroup value={heroType} onChange={setHeroType}>
  <Radio value="image">Single Image</Radio>
  <Radio value="video">Video</Radio>
  <Radio value="carousel">Image Carousel</Radio>
</RadioGroup>
```

### 2. Conditional Upload UI (Admin)
```jsx
{heroType === 'image' && <ImageUploadSection />}
{heroType === 'video' && <VideoUploadSection />}
{heroType === 'carousel' && <CarouselManagementSection />}
```

### 3. Conditional Rendering (Public)
```jsx
{dealership?.hero_type === 'carousel' && 
  <HeroCarousel images={dealership.hero_carousel_images}>
    {/* Content */}
  </HeroCarousel>
}

{dealership?.hero_type === 'video' && 
  <video autoPlay loop muted playsInline>
    <source src={dealership.hero_video_url} type="video/mp4" />
  </video>
}

{/* Default: Image hero */}
```

### 4. Carousel Auto-Rotation
```javascript
useEffect(() => {
  const timer = setInterval(() => {
    setCurrentIndex((prev) => (prev + 1) % images.length);
  }, 5000); // 5 second interval
  return () => clearInterval(timer);
}, [images.length]);
```

---

## 🐛 Known Issues & Fixes

### Issue 1: Backend Field Extraction (FIXED)
**Problem**: Backend route wasn't extracting new hero fields from request body

**Root Cause**: Missing field destructuring in `backend/routes/dealers.js`

**Fix**: Added `hero_type`, `hero_video_url`, `hero_carousel_images` to:
1. Request body destructuring (line 192)
2. updateData object (lines 247-251)

**Documentation**: `HERO_MEDIA_BUG_FIX.md`

### Issue 2: Image Cropping (FIXED)
**Problem**: Carousel images were cropped, showing only part of the car

**Root Cause**: CSS `background-size: cover` was cropping overflow

**Fix**: Changed to `background-size: contain` in:
1. `HeroCarousel.jsx` (line 68)
2. `Home.jsx` (line 119)

**Documentation**: `HERO_MEDIA_IMAGE_DISPLAY_FIX.md`

---

## 📝 Important Notes for Agents

### When Adding New Database Fields
Always update **3 locations** in backend:

1. **Request Destructuring**:
   ```javascript
   const { existing_fields, NEW_FIELD } = req.body;
   ```

2. **Update Object**:
   ```javascript
   if (NEW_FIELD !== undefined) updateData.NEW_FIELD = NEW_FIELD;
   ```

3. **JSDoc**:
   ```javascript
   * @param {type} [req.body.NEW_FIELD] - Description
   ```

### CSS Background Properties
- `background-size: cover` - Fills container, may crop image
- `background-size: contain` - Shows full image, may have empty space
- **Current choice**: `contain` (shows full image per user requirement)

### File Upload Limits
- **Images**: 5MB max (JPG, PNG, WebP)
- **Videos**: 50MB max (MP4, WebM, OGG)
- **Validation**: Client-side AND server-side

### JSONB Best Practices
```javascript
// Storing array in PostgreSQL JSONB
hero_carousel_images: JSON.stringify(['url1', 'url2', 'url3'])

// Database will automatically parse as JSONB array
// Retrieved as: ['url1', 'url2', 'url3']
```

---

## 🧪 Testing Checklist

### Manual Testing
- [ ] Can select each hero type (image/video/carousel)
- [ ] Can upload files for each type
- [ ] Can preview uploads in admin
- [ ] Can remove/change uploaded media
- [ ] Changes save successfully
- [ ] Public homepage displays correct hero type
- [ ] Carousel auto-rotates (5s interval)
- [ ] Carousel navigation works (arrows, dots)
- [ ] Video auto-plays and loops
- [ ] Works on mobile devices
- [ ] Full images display without cropping

### API Testing
```bash
# Test carousel save
curl -X PUT http://localhost:5000/api/dealers/1 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Dealership",
    "address": "123 Main St",
    "phone": "555-0100",
    "email": "test@test.com",
    "hero_type": "carousel",
    "hero_carousel_images": ["url1", "url2", "url3"]
  }'
```

### Database Verification
```sql
-- Check hero data
SELECT id, name, hero_type, hero_carousel_images 
FROM dealership 
WHERE id = 1;
```

---

## 📚 Documentation Map

```
Hero Media Feature Documentation
│
├── User Documentation
│   ├── HERO_MEDIA_QUICK_START.md .......... Quick setup guide
│   ├── HERO_MEDIA_VISUAL_GUIDE.md ......... Visual UI guide
│   └── HERO_MEDIA_DOCS_INDEX.md ........... Documentation index
│
├── Technical Documentation
│   ├── HERO_MEDIA_FEATURE.md .............. Implementation details
│   ├── HERO_MEDIA_BUG_FIX.md .............. Backend bug fix
│   └── HERO_MEDIA_IMAGE_DISPLAY_FIX.md .... Display fix
│
├── Project Documentation
│   ├── docs/prd/PRD_HERO_MEDIA_FEATURE.md . Product requirements
│   ├── docs/architecture/hero-media-architecture.md ... Architecture
│   ├── docs/stories/hero-media-sprint-plan.md ........ Sprint planning
│   └── README-FOR-AGENTS-HERO-MEDIA.md ............... This file
│
└── Test Scripts
    └── test_hero_media.js ................. API test script
```

---

## 🚀 Quick Commands

### Run Database Migration
```bash
# Apply migration
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend/db/migrations/20260104_add_hero_media_options.sql

# Verify
docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "\d dealership"
```

### Rebuild Frontend
```bash
cd frontend
npm run build
```

### Restart Backend
```bash
cd backend
npm start
```

### Run API Tests
```bash
node test_hero_media.js
```

---

## 🎨 UI/UX Details

### Admin Hero Type Selector
```
┌────────────────┐  ┌────────────────┐  ┌────────────────┐
│ ○ Single Image │  │ ○ Video        │  │ ● Carousel     │
│ One static bg  │  │ Looping video  │  │ Rotating imgs  │
└────────────────┘  └────────────────┘  └────────────────┘
```

### Public Carousel Display
```
┌────────────────────────────────────────────┐
│ [←]         Hero Content              [→] │
│                                            │
│         Dealership Name                    │
│         Description text                   │
│         [ Browse Inventory ]               │
│                                            │
│              ○ ● ○ ○                       │
└────────────────────────────────────────────┘
```

---

## 🔒 Security Considerations

### File Upload Validation
1. **Client-side**: File type and size checks before upload
2. **Server-side**: MIME type validation, size limits enforced
3. **Cloudinary**: Automatic malware scanning

### XSS Prevention
- Text fields sanitized via `sanitizeInput()` function
- React auto-escapes JSX content
- No user-provided URLs (only Cloudinary)

### SQL Injection Prevention
- Parameterized queries throughout
- No string concatenation in SQL
- JSONB data properly escaped

---

## ⚡ Performance Tips

### Image Optimization
```javascript
// Use Cloudinary transformations
const optimizedUrl = url.replace(
  '/upload/',
  '/upload/w_800,h_400,c_fill,f_auto/'
);
```

### Carousel Performance
- CSS transitions (GPU-accelerated)
- No JavaScript animation
- Cleanup intervals on unmount
- Lazy loading (future enhancement)

### Video Best Practices
- Keep videos < 20MB for fast loading
- Use MP4 format (best compatibility)
- Auto-optimize via Cloudinary
- Muted autoplay (allows autoplay on mobile)

---

## 🔄 Future Enhancements (Backlog)

### Priority 1
- [ ] Drag-and-drop carousel image reordering
- [ ] Configurable carousel interval (admin control)
- [ ] Unit tests for components
- [ ] Integration tests for API

### Priority 2
- [ ] YouTube/Vimeo video embedding
- [ ] Custom transition effects
- [ ] Video upload progress indicator
- [ ] Per-image captions

### Priority 3
- [ ] A/B testing different hero types
- [ ] Analytics on hero engagement
- [ ] Template carousel sets
- [ ] AI-powered image suggestions

---

## 🆘 Troubleshooting

### Carousel Not Displaying
1. Check browser console for errors
2. Verify `hero_type === 'carousel'` in database
3. Verify `hero_carousel_images` is non-empty array
4. Check network tab - are images loading?
5. Clear browser cache

### Video Not Playing
1. Check video format (MP4 recommended)
2. Verify `autoPlay` attribute present
3. Ensure `muted` attribute (required for autoplay)
4. Check mobile: needs `playsInline` attribute
5. Test in different browsers

### Upload Failing
1. Check file type (JPG/PNG/WebP or MP4/WebM/OGG)
2. Check file size (5MB images, 50MB videos)
3. Verify network connection
4. Check browser console for errors
5. Verify Cloudinary credentials in backend

### Backend Not Saving Hero Type
1. Restart backend server after code changes
2. Verify field destructuring in route handler
3. Check updateData object includes new fields
4. Test with curl/Postman directly
5. Check database logs for errors

---

## 📞 Contact & Support

### For Development Questions
- Check architecture doc: `docs/architecture/hero-media-architecture.md`
- Review implementation: `HERO_MEDIA_FEATURE.md`
- See code comments (JSDoc)

### For Bug Reports
- Document in similar format to `HERO_MEDIA_BUG_FIX.md`
- Include steps to reproduce
- Provide database state
- Share browser console errors

### For Feature Requests
- Add to backlog in sprint planning doc
- Estimate story points
- Define acceptance criteria
- Consider backward compatibility

---

## ✅ Agent Checklist

When working with this feature, ensure you:

- [ ] Understand the 3 hero types (image, video, carousel)
- [ ] Know database schema (4 new/modified columns)
- [ ] Know API fields (hero_type, hero_video_url, hero_carousel_images)
- [ ] Understand conditional rendering logic
- [ ] Know file upload limits and types
- [ ] Understand CSS display mode (contain vs cover)
- [ ] Can locate all documentation files
- [ ] Can run tests and migrations
- [ ] Know rollback procedures
- [ ] Understand backward compatibility

---

**Last Updated**: 2026-01-04  
**Maintained By**: Development Team  
**Version**: 1.0  
**Status**: Production-Ready ✅

---

## Quick Links

- **Main Feature Docs**: `HERO_MEDIA_FEATURE.md`
- **User Guide**: `HERO_MEDIA_QUICK_START.md`
- **PRD**: `docs/prd/PRD_HERO_MEDIA_FEATURE.md`
- **Architecture**: `docs/architecture/hero-media-architecture.md`
- **Sprint Plan**: `docs/stories/hero-media-sprint-plan.md`
- **Bug Fixes**: `HERO_MEDIA_BUG_FIX.md`, `HERO_MEDIA_IMAGE_DISPLAY_FIX.md`
