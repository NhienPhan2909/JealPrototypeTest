# Architecture Document - Hero Media Enhancement

## Document Control
- **Component**: Hero Media System
- **Version**: 1.0
- **Date**: 2026-01-04
- **Status**: Implemented
- **Owner**: Architecture Team
- **Related PRD**: `docs/prd/PRD_HERO_MEDIA_FEATURE.md`

---

## Table of Contents
1. [Overview](#overview)
2. [System Architecture](#system-architecture)
3. [Database Design](#database-design)
4. [API Design](#api-design)
5. [Frontend Architecture](#frontend-architecture)
6. [Component Design](#component-design)
7. [Data Flow](#data-flow)
8. [Security Considerations](#security-considerations)
9. [Performance Optimization](#performance-optimization)
10. [Scalability](#scalability)

---

## Overview

### Purpose
This document describes the technical architecture for the Hero Media Enhancement feature, which extends the dealership homepage hero section to support three types of backgrounds: single image, video, and image carousel.

### Goals
- Provide flexible media options for homepage hero section
- Maintain backward compatibility with existing single image functionality
- Ensure optimal performance across all media types
- Support responsive design and mobile devices
- Enable seamless admin management of hero content

### Scope
- Database schema changes
- Backend API modifications
- Frontend component architecture
- Media storage and delivery
- User interface implementations

---

## System Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Client Layer                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────┐              ┌──────────────────┐        │
│  │  Admin CMS       │              │  Public Website  │        │
│  │  (DealerSettings)│              │  (Home)          │        │
│  │                  │              │                  │        │
│  │  - Type Selector │              │  - HeroCarousel  │        │
│  │  - Image Upload  │              │  - Video Hero    │        │
│  │  - Video Upload  │              │  - Image Hero    │        │
│  │  - Carousel Mgmt │              │                  │        │
│  └────────┬─────────┘              └────────┬─────────┘        │
│           │                                 │                  │
└───────────┼─────────────────────────────────┼──────────────────┘
            │                                 │
            │  PUT /api/dealers/:id           │  GET /api/dealers/:id
            │  POST /api/upload               │
            │                                 │
┌───────────▼─────────────────────────────────▼──────────────────┐
│                      Application Layer                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │               Express.js Server                          │  │
│  │                                                          │  │
│  │  ┌──────────────────┐        ┌──────────────────┐      │  │
│  │  │  Dealers Routes  │        │  Upload Routes   │      │  │
│  │  │  - GET /:id      │        │  - POST /upload  │      │  │
│  │  │  - PUT /:id      │        │                  │      │  │
│  │  └────────┬─────────┘        └────────┬─────────┘      │  │
│  │           │                           │                │  │
│  │  ┌────────▼──────────────────────────▼─────────┐      │  │
│  │  │         Middleware Layer                    │      │  │
│  │  │  - Validation (file type, size)             │      │  │
│  │  │  - Sanitization (XSS prevention)            │      │  │
│  │  │  - Error handling                           │      │  │
│  │  └────────┬────────────────────────────────────┘      │  │
│  └───────────┼──────────────────────────────────────────┘  │
│              │                                             │
└──────────────┼─────────────────────────────────────────────┘
               │
               │  SQL Queries
               │
┌──────────────▼─────────────────────────────────────────────────┐
│                       Data Layer                               │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │              PostgreSQL Database                          │ │
│  │                                                           │ │
│  │  dealership Table:                                        │ │
│  │  - hero_type (VARCHAR)                                    │ │
│  │  - hero_background_image (TEXT)                           │ │
│  │  - hero_video_url (TEXT)                                  │ │
│  │  - hero_carousel_images (JSONB)                           │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                                │
└────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│                    External Services                           │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │                  Cloudinary CDN                           │ │
│  │                                                           │ │
│  │  - Image storage & optimization                           │ │
│  │  - Video storage & transcoding                            │ │
│  │  - CDN delivery                                           │ │
│  │  - Transformation API (resize, crop, format)              │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

---

## Database Design

### Schema Changes

**Migration File**: `backend/db/migrations/20260104_add_hero_media_options.sql`

#### New Columns

```sql
ALTER TABLE dealership
ADD COLUMN IF NOT EXISTS hero_type VARCHAR(20) DEFAULT 'image' 
  CHECK (hero_type IN ('image', 'video', 'carousel'));

ALTER TABLE dealership
ADD COLUMN IF NOT EXISTS hero_video_url TEXT;

ALTER TABLE dealership
ADD COLUMN IF NOT EXISTS hero_carousel_images JSONB DEFAULT '[]'::jsonb;
```

### Table: `dealership`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `hero_type` | VARCHAR(20) | CHECK ('image', 'video', 'carousel'), DEFAULT 'image' | Active hero media type |
| `hero_background_image` | TEXT | NULL | Legacy/single image URL (backward compatible) |
| `hero_video_url` | TEXT | NULL | Cloudinary video URL |
| `hero_carousel_images` | JSONB | DEFAULT '[]' | Array of image URLs |

### Data Types Rationale

**VARCHAR(20) for hero_type**
- Limited enum values (image, video, carousel)
- CHECK constraint enforces valid values
- Fast comparison and indexing
- Future-proof for additional types

**TEXT for URLs**
- Cloudinary URLs can be long (transformations add parameters)
- No practical length limit needed
- Flexible for future URL structure changes

**JSONB for carousel_images**
- Native array support in PostgreSQL
- JSON query capabilities (future analytics)
- Efficient storage (binary format)
- Easy to append/remove images
- Indexable if needed

### Example Data

```json
{
  "id": 1,
  "name": "Acme Auto Sales",
  "hero_type": "carousel",
  "hero_background_image": "https://res.cloudinary.com/.../image.jpg",
  "hero_video_url": null,
  "hero_carousel_images": [
    "https://res.cloudinary.com/.../car1.jpg",
    "https://res.cloudinary.com/.../car2.jpg",
    "https://res.cloudinary.com/.../car3.jpg"
  ]
}
```

---

## API Design

### Endpoints

#### GET `/api/dealers/:id`

**Purpose**: Retrieve dealership data including hero media configuration

**Response**:
```json
{
  "id": 1,
  "name": "Acme Auto Sales",
  "hero_type": "carousel",
  "hero_background_image": "https://...",
  "hero_video_url": null,
  "hero_carousel_images": [
    "https://res.cloudinary.com/.../image1.jpg",
    "https://res.cloudinary.com/.../image2.jpg"
  ],
  "theme_color": "#3B82F6",
  "...": "other fields"
}
```

#### PUT `/api/dealers/:id`

**Purpose**: Update dealership hero media configuration

**Request Body**:
```json
{
  "name": "Acme Auto Sales",
  "address": "123 Main St",
  "phone": "555-0100",
  "email": "info@acme.com",
  "hero_type": "carousel",
  "hero_carousel_images": [
    "https://res.cloudinary.com/.../image1.jpg",
    "https://res.cloudinary.com/.../image2.jpg",
    "https://res.cloudinary.com/.../image3.jpg"
  ]
}
```

**Validation**:
- `hero_type`: Must be 'image', 'video', or 'carousel'
- `hero_video_url`: Valid URL format if provided
- `hero_carousel_images`: Array of valid URL strings

**Response**: Updated dealership object

#### POST `/api/upload`

**Purpose**: Upload image or video file to Cloudinary

**Request**: Multipart form data
- Field: `image` (file)
- Accept: image/jpeg, image/png, image/webp, video/mp4, video/webm, video/ogg

**Validation**:
- File type (MIME type check)
- File size (5MB for images, 50MB for videos)
- Malware scan (Cloudinary)

**Response**:
```json
{
  "url": "https://res.cloudinary.com/.../uploaded-file.jpg",
  "publicId": "dealership-vehicles/abc123",
  "format": "jpg",
  "width": 1920,
  "height": 1080
}
```

---

## Frontend Architecture

### Component Hierarchy

```
App
└── Routes
    ├── Public Routes
    │   └── Home
    │       ├── HeroCarousel (if hero_type === 'carousel')
    │       ├── Video Hero (if hero_type === 'video')
    │       └── Image Hero (if hero_type === 'image')
    │
    └── Admin Routes
        └── DealerSettings
            ├── Hero Type Selector (radio buttons)
            ├── Image Upload Section (if type === 'image')
            ├── Video Upload Section (if type === 'video')
            └── Carousel Upload Section (if type === 'carousel')
```

### State Management

#### Admin Component State
```javascript
const [heroType, setHeroType] = useState('image');
const [heroBackgroundUrl, setHeroBackgroundUrl] = useState('');
const [heroVideoUrl, setHeroVideoUrl] = useState('');
const [heroCarouselImages, setHeroCarouselImages] = useState([]);
```

#### HeroCarousel Component State
```javascript
const [currentIndex, setCurrentIndex] = useState(0);
// Auto-rotation managed via useEffect + setInterval
```

### Data Flow - Admin

```
User selects carousel type
         │
         ▼
setHeroType('carousel')
         │
         ▼
Conditional render: Carousel upload UI
         │
         ▼
User uploads images ──► POST /api/upload ──► Cloudinary
         │                                        │
         ▼                                        ▼
setHeroCarouselImages([...images, newUrl])   Returns URL
         │
         ▼
User clicks Save
         │
         ▼
PUT /api/dealers/:id with {hero_type, hero_carousel_images}
         │
         ▼
Database updated
         │
         ▼
Success message displayed
```

### Data Flow - Public

```
Page loads
    │
    ▼
GET /api/dealers/:id
    │
    ▼
Receive dealership data
    │
    ▼
Check hero_type
    │
    ├─ 'image' ──► Render static image hero
    │
    ├─ 'video' ──► Render video hero with autoplay
    │
    └─ 'carousel' ──► Render HeroCarousel component
                            │
                            ▼
                     Auto-rotation starts (5s interval)
                            │
                            ▼
                     User can navigate (arrows/dots)
```

---

## Component Design

### HeroCarousel Component

**File**: `frontend/src/components/HeroCarousel.jsx`

#### Props
```typescript
interface HeroCarouselProps {
  images: string[];      // Array of Cloudinary URLs
  interval?: number;     // Rotation interval (default: 5000ms)
  children: ReactNode;   // Text overlay content
}
```

#### Internal State
```javascript
const [currentIndex, setCurrentIndex] = useState(0);
```

#### Key Features
- Auto-rotation using `setInterval` in `useEffect`
- Previous/Next navigation buttons
- Dot indicators for position
- Smooth fade transitions (CSS opacity)
- Responsive arrow and dot positioning
- Cleanup interval on unmount

#### CSS Implementation
```javascript
// Background size: contain (no cropping)
// Background position: center
// Opacity transition: 1000ms
// Dark overlay: rgba(0, 0, 0, 0.4)
```

### DealerSettings Component

**File**: `frontend/src/pages/admin/DealerSettings.jsx`

#### Hero Media Section Structure
```jsx
<div>
  {/* Type Selector */}
  <RadioGroup onChange={setHeroType}>
    <Radio value="image">Single Image</Radio>
    <Radio value="video">Video</Radio>
    <Radio value="carousel">Carousel</Radio>
  </RadioGroup>

  {/* Conditional Upload UI */}
  {heroType === 'image' && <ImageUpload />}
  {heroType === 'video' && <VideoUpload />}
  {heroType === 'carousel' && <CarouselUpload />}
</div>
```

#### Upload Handlers
```javascript
const handleHeroBackgroundUpload = async (e) => {
  // Validate file, upload to /api/upload, set state
};

const handleHeroVideoUpload = async (e) => {
  // Validate video file, upload, set state
};

const handleCarouselImageUpload = async (e) => {
  // Validate image, upload, append to array
};
```

---

## Data Flow

### Upload Flow

```
┌─────────────┐
│ User selects│
│    file     │
└──────┬──────┘
       │
       ▼
┌─────────────────────────┐
│ Client-side validation  │
│ - File type             │
│ - File size             │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│ FormData creation       │
│ formData.append('image')│
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│ POST /api/upload        │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│ Server validation       │
│ - MIME type check       │
│ - Size check            │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│ Upload to Cloudinary    │
│ - Optimization          │
│ - Transformation        │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│ Return Cloudinary URL   │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│ Update component state  │
│ setHeroVideoUrl(url)    │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│ User clicks Save        │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│ PUT /api/dealers/:id    │
│ with all hero fields    │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│ Database UPDATE query   │
│ via dealers.update()    │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│ Success response        │
└──────┬──────────────────┘
       │
       ▼
┌─────────────────────────┐
│ Refresh public homepage │
│ - Fetch updated data    │
│ - Render new hero type  │
└─────────────────────────┘
```

---

## Security Considerations

### File Upload Security

#### Client-Side
- ✅ File type validation (MIME type check)
- ✅ File size validation (5MB images, 50MB videos)
- ✅ User feedback for invalid files
- ✅ Input sanitization (filenames)

#### Server-Side
- ✅ MIME type validation (whitelist: jpeg, png, webp, mp4, webm, ogg)
- ✅ File size validation (hard limits enforced)
- ✅ Cloudinary security features (malware scan, rate limiting)
- ✅ No executable file types allowed
- ✅ No directory traversal in filenames

### Data Validation

#### hero_type
```javascript
// Database constraint
CHECK (hero_type IN ('image', 'video', 'carousel'))

// Backend validation (implicit via database)
// Frontend validation (radio buttons - limited options)
```

#### URLs
```javascript
// Stored as-is (from Cloudinary)
// No user-provided URLs accepted
// All URLs generated by trusted source (Cloudinary)
```

#### JSONB Array
```javascript
// Validated as array of strings
// Each string validated as Cloudinary URL
// No arbitrary JSON accepted
```

### XSS Prevention
- ✅ Text fields sanitized in backend (`sanitizeInput()`)
- ✅ React auto-escapes JSX content
- ✅ URLs from trusted source only (Cloudinary)
- ✅ No `dangerouslySetInnerHTML` used

---

## Performance Optimization

### Image Optimization

#### Cloudinary Transformations
```
Original: https://res.cloudinary.com/.../image.jpg
Preview:  https://res.cloudinary.com/.../w_800,h_400,c_fill,f_auto/image.jpg
Thumb:    https://res.cloudinary.com/.../w_400,h_250,c_fill,f_auto/image.jpg
```

**Parameters**:
- `w_` / `h_`: Width/height
- `c_fill`: Crop mode
- `f_auto`: Format auto-detection (WebP for supported browsers)
- `q_auto`: Quality auto-optimization

### Video Optimization

#### Cloudinary Video Processing
- Automatic format selection (WebM for Chrome, MP4 for Safari)
- Adaptive bitrate delivery
- Lazy loading (defer until needed)
- Streaming optimization

#### HTML5 Video Attributes
```html
<video autoPlay loop muted playsInline>
  <source src="{url}" type="video/mp4" />
</video>
```

**Benefits**:
- `muted`: Allows autoplay (browser policy)
- `playsInline`: Prevents fullscreen on iOS
- `loop`: Continuous playback (no restart flash)

### Carousel Performance

#### CSS Transitions
```css
.carousel-image {
  transition: opacity 1000ms ease-in-out;
}
```

**Benefits**:
- Hardware-accelerated (GPU)
- No JavaScript animation overhead
- Smooth 60fps transitions
- Low battery impact on mobile

#### React Optimization
```javascript
// No unnecessary re-renders
// useEffect cleanup prevents memory leaks
// Interval cleared on unmount
```

### Lazy Loading (Future)
```javascript
// Not implemented yet, but recommended:
<img loading="lazy" src={url} alt="..." />
```

---

## Scalability

### Database Scalability

#### JSONB Array Size
- PostgreSQL JSONB efficient for arrays < 1000 items
- Current recommendation: 3-5 carousel images
- Maximum practical: 10-20 images
- No hard limit enforced (admin discretion)

#### Indexing Strategy
```sql
-- Not currently indexed, but if query performance becomes an issue:
CREATE INDEX idx_dealership_hero_type ON dealership(hero_type);

-- JSONB indexing for future analytics:
CREATE INDEX idx_carousel_images ON dealership USING GIN (hero_carousel_images);
```

### CDN Scalability

#### Cloudinary Benefits
- Global CDN distribution
- Automatic edge caching
- Bandwidth scaling
- DDoS protection
- Image transformation caching

#### Cache Strategy
```
Browser Cache: 1 year (immutable URLs)
CDN Cache: Indefinite (until purged)
Database: No caching (always fresh)
```

### Frontend Scalability

#### Bundle Size
- HeroCarousel component: ~2KB gzipped
- No external dependencies
- CSS-only animations
- Minimal JavaScript execution

#### Rendering Performance
```javascript
// Virtual DOM efficiency
// Conditional rendering (only active type)
// No unnecessary re-renders (React.memo candidates)
```

---

## Monitoring & Observability

### Metrics to Track

#### Usage Metrics
- Percentage of dealerships using each hero type
- Average carousel image count
- Video vs image preference
- Upload success/failure rates

#### Performance Metrics
- Hero section load time
- Carousel transition smoothness (FPS)
- Video playback success rate
- Mobile vs desktop performance

#### Error Tracking
- Upload failures (file type, size, network)
- Database save errors
- Cloudinary service errors
- Client-side JavaScript errors

### Logging Strategy

```javascript
// Backend logging
logger.info('Hero media updated', { 
  dealershipId, 
  heroType, 
  imageCount: carousel_images.length 
});

// Error logging
logger.error('Hero upload failed', { 
  error: err.message, 
  fileType, 
  fileSize 
});
```

---

## Testing Strategy

### Unit Tests

#### HeroCarousel Component
```javascript
describe('HeroCarousel', () => {
  test('renders all images', () => {...});
  test('auto-rotates every 5 seconds', () => {...});
  test('next button advances slide', () => {...});
  test('dot click jumps to slide', () => {...});
  test('cleans up interval on unmount', () => {...});
});
```

#### Upload Handlers
```javascript
describe('handleCarouselImageUpload', () => {
  test('validates file type', () => {...});
  test('validates file size', () => {...});
  test('uploads to API', () => {...});
  test('updates state on success', () => {...});
  test('shows error on failure', () => {...});
});
```

### Integration Tests

#### API Tests
```javascript
describe('PUT /api/dealers/:id', () => {
  test('saves hero_type', () => {...});
  test('saves hero_carousel_images array', () => {...});
  test('validates hero_type values', () => {...});
  test('returns updated dealership', () => {...});
});
```

#### Database Tests
```javascript
describe('dealers.update()', () => {
  test('updates hero_carousel_images JSONB', () => {...});
  test('handles empty array', () => {...});
  test('handles null values', () => {...});
});
```

### End-to-End Tests

```javascript
describe('Hero Media Feature', () => {
  test('admin can set carousel and upload images', () => {
    // 1. Login to admin
    // 2. Navigate to Settings
    // 3. Select carousel type
    // 4. Upload 3 images
    // 5. Click Save
    // 6. Visit public homepage
    // 7. Verify carousel displays
    // 8. Verify auto-rotation works
  });
});
```

---

## Deployment Architecture

### Production Environment

```
┌────────────────────────────────────────────────────────┐
│                  Load Balancer (Nginx)                 │
└─────────────┬──────────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────────────────────┐
│              Application Servers (Node.js)              │
│  - Backend API (Express)                                │
│  - Static Frontend (React build)                        │
└─────────────┬───────────────────────────────────────────┘
              │
              ├──► PostgreSQL Database (Primary)
              │
              └──► Cloudinary (External Service)
```

### Deployment Steps

1. **Database Migration**
   ```bash
   psql -f backend/db/migrations/20260104_add_hero_media_options.sql
   ```

2. **Backend Deployment**
   ```bash
   cd backend
   npm install
   npm start
   ```

3. **Frontend Build**
   ```bash
   cd frontend
   npm run build
   cp -r dist/* /var/www/html/
   ```

4. **Verification**
   - Smoke test all hero types
   - Test file uploads
   - Verify mobile responsiveness

---

## Rollback Procedure

### Database Rollback
```sql
ALTER TABLE dealership DROP COLUMN IF EXISTS hero_type;
ALTER TABLE dealership DROP COLUMN IF EXISTS hero_video_url;
ALTER TABLE dealership DROP COLUMN IF EXISTS hero_carousel_images;
```

### Code Rollback
```bash
git revert <commit-hash>
git push origin main
```

### Data Preservation
- Existing `hero_background_image` data preserved
- Dealerships revert to single image mode
- No data loss

---

## Appendix

### Related Documents
- PRD: `docs/prd/PRD_HERO_MEDIA_FEATURE.md`
- Implementation: `HERO_MEDIA_FEATURE.md`
- Quick Start: `HERO_MEDIA_QUICK_START.md`
- Visual Guide: `HERO_MEDIA_VISUAL_GUIDE.md`

### Technology Stack
- **Database**: PostgreSQL 14
- **Backend**: Node.js 18, Express.js 4
- **Frontend**: React 18, Vite 4, Tailwind CSS 3
- **Storage**: Cloudinary
- **Hosting**: [To be determined]

### Change Log

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-04 | Architecture Team | Initial architecture document |

---

**Document Status**: ✅ Reviewed and Approved

**Sign-off**:
- Architecture Lead: _____________
- Tech Lead: _____________
- Security Lead: _____________
