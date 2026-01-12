# Hero Media Feature Documentation

## Overview
The Hero Media feature extends the dealership homepage hero section to support three types of backgrounds:
1. **Single Image** - A static background image (existing functionality)
2. **Video** - A looping background video
3. **Carousel** - Multiple images that automatically transition

## Feature Implementation Summary

### Database Changes
- **Migration File**: `backend/db/migrations/20260104_add_hero_media_options.sql`
- **New Columns**:
  - `hero_type` (VARCHAR(20), default: 'image') - Specifies media type: 'image', 'video', or 'carousel'
  - `hero_video_url` (TEXT) - Cloudinary URL for background video
  - `hero_carousel_images` (JSONB) - Array of Cloudinary image URLs for carousel

### Backend Changes
1. **Database Layer** (`backend/db/dealers.js`):
   - Updated `update()` function to handle new fields: `hero_type`, `hero_video_url`, `hero_carousel_images`
   - Added JSDoc documentation for new parameters

2. **API Routes** (`backend/routes/dealers.js`):
   - No changes required - dynamic field handling already supports new columns

### Frontend Changes

#### New Components
1. **HeroCarousel Component** (`frontend/src/components/HeroCarousel.jsx`):
   - Automatic image rotation with configurable interval (default 5 seconds)
   - Navigation controls (previous/next buttons)
   - Dot indicators for slide position
   - Responsive design with overlay support
   - Graceful fallback to gradient when no images provided

#### Updated Components
1. **Home Page** (`frontend/src/pages/public/Home.jsx`):
   - Conditional rendering based on `hero_type`:
     - **Carousel**: Uses HeroCarousel component
     - **Video**: HTML5 video with autoplay, loop, mute
     - **Image**: Static image with gradient overlay (existing)
   - Consistent overlay and content positioning across all types
   - Dark overlay (40% opacity) for better text readability

2. **Dealer Settings** (`frontend/src/pages/admin/DealerSettings.jsx`):
   - Added hero type selector with three radio button options
   - Conditional upload sections based on selected type:
     - **Image**: Single file upload with preview
     - **Video**: Video file upload with preview (50MB max)
     - **Carousel**: Multiple image uploads with grid preview and management
   - State management for all new fields
   - Upload handlers with validation:
     - Image files: JPG, PNG, WebP (5MB max)
     - Video files: MP4, WebM, OGG (50MB max)
   - Preview functionality for all media types
   - Remove/change functionality for all uploads

## How to Use (Admin)

### Accessing Hero Settings
1. Log into CMS admin panel
2. Navigate to **Settings** page
3. Scroll to **Home Page Hero Section**

### Setting Hero Type

#### Single Image
1. Select "Single Image" radio button
2. Click "Upload Hero Background Image"
3. Choose an image file (JPG, PNG, WebP)
4. Preview appears with dark overlay
5. Use "Change Image" to replace or "Remove Image" to clear

#### Video Background
1. Select "Video" radio button
2. Click "Upload Hero Background Video"
3. Choose a video file (MP4, WebM, OGG - max 50MB)
4. Preview plays automatically with overlay
5. Use "Change Video" to replace or "Remove Video" to clear

#### Image Carousel
1. Select "Image Carousel" radio button
2. Click "Add First Image" to upload initial image
3. Repeat for additional images (no limit)
4. Images appear in numbered grid
5. Hover over image thumbnail to reveal remove button (×)
6. Images automatically rotate every 5 seconds on public site

### Best Practices
- **Image Dimensions**: Recommended 1920x1080 or larger for best quality
- **Video Recommendations**:
  - Keep videos under 20MB for faster loading
  - Use MP4 format for best browser compatibility
  - Videos should be 15-30 seconds (will loop)
  - Avoid videos with important audio (auto-muted)
- **Carousel Tips**:
  - Use 3-5 images for optimal user experience
  - Maintain consistent dimensions across carousel images
  - Order images intentionally (first image shows first)

## Technical Details

### Video Implementation
- Uses HTML5 `<video>` element
- Attributes: `autoPlay`, `loop`, `muted`, `playsInline`
- 40% black overlay for text readability
- `object-cover` ensures proper scaling
- Fallback text if browser doesn't support video

### Carousel Implementation
- React component with automatic transitions
- Uses CSS transitions for smooth fading
- Interval configurable (default: 5000ms)
- Navigation:
  - Arrow buttons (previous/next)
  - Dot indicators (click to jump to slide)
- Accessible with ARIA labels
- Pauses on manual navigation (restarts after interaction)

### Data Storage
- Image/Video URLs stored as Cloudinary URLs (TEXT)
- Carousel images stored as JSONB array: `["url1", "url2", "url3"]`
- Hero type stored as VARCHAR with CHECK constraint

### Upload Workflow
1. User selects file from local machine
2. Frontend validates file type and size
3. File sent to `/api/upload` endpoint
4. Cloudinary processes and returns URL
5. URL stored in component state
6. On save, URL stored in database

## API Reference

### Dealership Update Endpoint
**PUT** `/api/dealers/:id`

**New Request Body Fields**:
```json
{
  "hero_type": "image|video|carousel",
  "hero_video_url": "https://res.cloudinary.com/.../video.mp4",
  "hero_carousel_images": [
    "https://res.cloudinary.com/.../image1.jpg",
    "https://res.cloudinary.com/.../image2.jpg"
  ]
}
```

**Response**: Updated dealership object with all fields

## Files Modified/Created

### Created Files
- `backend/db/migrations/20260104_add_hero_media_options.sql`
- `frontend/src/components/HeroCarousel.jsx`
- `HERO_MEDIA_FEATURE.md` (this file)

### Modified Files
- `backend/db/dealers.js` - Added hero media field handling
- `frontend/src/pages/public/Home.jsx` - Added conditional hero rendering
- `frontend/src/pages/admin/DealerSettings.jsx` - Added hero media management UI

## Testing

### Manual Testing Steps
1. **Database Migration**:
   ```bash
   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "\d dealership"
   ```
   Verify `hero_type`, `hero_video_url`, `hero_carousel_images` columns exist

2. **Admin UI**:
   - Verify radio buttons switch between types
   - Test image upload (single)
   - Test video upload
   - Test carousel image uploads (multiple)
   - Test remove/change functionality
   - Verify previews display correctly

3. **Public Homepage**:
   - Set type to "image" → Verify static image displays
   - Set type to "video" → Verify video loops with overlay
   - Set type to "carousel" → Verify images rotate automatically
   - Test navigation controls (arrows, dots)

4. **Browser Compatibility**:
   - Test on Chrome, Firefox, Safari, Edge
   - Test on mobile devices
   - Verify video plays on iOS (requires `playsInline` attribute)

## Future Enhancements
- [ ] Carousel transition effects (slide, fade options)
- [ ] Configurable carousel interval from admin
- [ ] Video upload progress indicator
- [ ] Drag-and-drop reordering for carousel images
- [ ] Video thumbnail preview in admin
- [ ] Support for YouTube/Vimeo embed links
- [ ] Lazy loading for carousel images
- [ ] Image optimization presets for hero images

## Migration Commands

### Apply Migration
```bash
# Using Docker
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend\db\migrations\20260104_add_hero_media_options.sql

# Using local PostgreSQL
psql -h localhost -p 5432 -U postgres -d jeal_prototype -f backend\db\migrations\20260104_add_hero_media_options.sql
```

### Verify Migration
```bash
docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "\d dealership"
```

### Rollback (if needed)
```sql
ALTER TABLE dealership DROP COLUMN IF EXISTS hero_type;
ALTER TABLE dealership DROP COLUMN IF EXISTS hero_video_url;
ALTER TABLE dealership DROP COLUMN IF EXISTS hero_carousel_images;
```

## Support
For issues or questions about this feature, refer to:
- Database schema: `backend/db/schema.sql`
- API documentation: `backend/routes/dealers.js`
- Component documentation: JSDoc comments in source files
