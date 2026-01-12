# Product Requirements Document - Hero Media Enhancement Feature

## Document Control
- **Feature ID**: HERO-MEDIA-001
- **Version**: 1.0
- **Date**: 2026-01-04
- **Status**: Implemented & Deployed
- **Owner**: Product Management
- **Epic**: Epic 5 - Website Customization & Navigation

---

## Executive Summary

Enhancement to the dealership website homepage hero section to support three types of background media: single image (existing), video background, and image carousel. This provides dealerships with more dynamic and engaging options to showcase their brand and inventory on the homepage.

---

## Business Context

### Problem Statement
Currently, dealerships can only set a single static image as their homepage hero background. This limitation prevents them from:
- Showcasing multiple aspects of their business (showroom, service center, inventory)
- Creating more engaging and dynamic homepage experiences
- Utilizing video content for brand storytelling
- Highlighting seasonal promotions or featured inventory through rotating images

### Business Value
- **Increased Engagement**: Video and carousel create more dynamic, eye-catching homepages
- **Better Storytelling**: Multiple images allow showcasing different business aspects
- **Competitive Advantage**: Modern, professional website presentation
- **Marketing Flexibility**: Easy to update seasonal/promotional content
- **Improved Conversion**: More engaging hero sections drive higher inventory browsing

### Success Metrics
- Dealership adoption rate of new hero types (video/carousel)
- Average time on homepage (expected increase)
- Click-through rate to inventory from hero section
- Admin satisfaction with customization options

---

## User Stories

### US-1: Admin Sets Carousel Hero
**As a** dealership administrator  
**I want to** upload multiple images for a rotating carousel hero section  
**So that** I can showcase different aspects of my business on the homepage

**Acceptance Criteria:**
- [ ] Can select "Image Carousel" option in CMS Settings
- [ ] Can upload 3-10 images from local machine
- [ ] Can preview carousel in admin panel
- [ ] Can remove individual carousel images
- [ ] Images auto-rotate every 5 seconds on public site
- [ ] Navigation controls (arrows, dots) work correctly
- [ ] Changes save successfully and reflect immediately on public site

### US-2: Admin Sets Video Hero
**As a** dealership administrator  
**I want to** upload a video as the homepage hero background  
**So that** I can create a more dynamic and engaging homepage experience

**Acceptance Criteria:**
- [ ] Can select "Video" option in CMS Settings
- [ ] Can upload video file (MP4, WebM, OGG) up to 50MB
- [ ] Video auto-plays, loops, and is muted on public site
- [ ] Can preview video with overlay in admin panel
- [ ] Can replace or remove video
- [ ] Video works on mobile devices (iOS, Android)
- [ ] Changes save successfully and reflect immediately on public site

### US-3: Admin Views Full Images Without Cropping
**As a** dealership administrator  
**I want to** see complete car/product images in the hero section  
**So that** my uploaded photos display properly without important parts being cut off

**Acceptance Criteria:**
- [ ] Full image visible in carousel (no cropping)
- [ ] Full image visible in single image mode (no cropping)
- [ ] Hero section maintains consistent height
- [ ] Images are centered properly
- [ ] Text overlay remains readable on all image types

### US-4: Visitor Views Hero Media
**As a** website visitor  
**I want to** see engaging hero content on the homepage  
**So that** I get a good first impression of the dealership

**Acceptance Criteria:**
- [ ] Video plays automatically without sound
- [ ] Carousel transitions smoothly between images
- [ ] All hero types display text overlay clearly
- [ ] Hero section loads quickly without layout shift
- [ ] Hero works on mobile, tablet, and desktop
- [ ] "Browse Inventory" CTA button is always visible and clickable

---

## Functional Requirements

### FR-1: Hero Type Selection
- Admin can select one of three hero types: Image, Video, or Carousel
- Radio button interface for type selection
- Selected type is visually highlighted
- Only one type can be active at a time

### FR-2: Image Upload (Single & Carousel)
- Support JPG, PNG, WebP formats
- Maximum file size: 5MB per image
- Upload via file input from local machine
- Upload to Cloudinary for storage
- Image optimization by Cloudinary
- Preview images with dark overlay (40% opacity)

### FR-3: Video Upload
- Support MP4, WebM, OGG formats
- Maximum file size: 50MB
- Upload via file input from local machine
- Upload to Cloudinary for storage
- Preview video with dark overlay
- Video attributes: autoplay, loop, muted, playsinline

### FR-4: Carousel Functionality
- Auto-rotate images every 5 seconds
- Previous/Next navigation buttons
- Dot indicators showing current position
- Click dot to jump to specific image
- Smooth fade transitions between images
- No limit on number of images (recommended 3-5)

### FR-5: Image Display
- Images display using CSS `background-size: contain`
- Full image always visible (no cropping)
- Images centered within hero section
- Dark overlay (40% black) for text readability
- Hero section maintains consistent height across all types

### FR-6: Data Persistence
- Hero type stored in `dealership.hero_type` (VARCHAR)
- Single image URL stored in `dealership.hero_background_image` (TEXT)
- Video URL stored in `dealership.hero_video_url` (TEXT)
- Carousel images stored in `dealership.hero_carousel_images` (JSONB array)
- Changes saved via PUT `/api/dealers/:id`
- Changes reflected immediately on public site

---

## Non-Functional Requirements

### NFR-1: Performance
- Video files optimized by Cloudinary
- Image files compressed and optimized
- Carousel transitions use CSS (no JavaScript animation)
- Hero section renders in < 2 seconds
- No layout shift during media loading

### NFR-2: Browser Compatibility
- Chrome (latest 2 versions)
- Firefox (latest 2 versions)
- Safari (latest 2 versions)
- Edge (latest 2 versions)
- Mobile Safari (iOS 14+)
- Mobile Chrome (Android 10+)

### NFR-3: Responsive Design
- Hero section responsive on mobile, tablet, desktop
- Video plays on mobile with `playsinline` attribute
- Carousel controls accessible on touch devices
- Text overlay readable on all screen sizes

### NFR-4: Accessibility
- ARIA labels on carousel navigation
- Alt text for images (future enhancement)
- Keyboard navigation for carousel (future enhancement)
- Sufficient color contrast for text overlay

### NFR-5: Security
- File type validation (client & server)
- File size validation (client & server)
- Uploaded files scanned by Cloudinary
- No executable file uploads allowed

---

## Technical Constraints

### Database
- PostgreSQL JSONB for carousel images array
- Database migration required for new columns
- Backward compatible with existing hero_background_image

### Storage
- Cloudinary for all media storage
- Media URLs stored in database (not files)
- No local file storage

### Frontend
- React functional components
- Tailwind CSS for styling
- No external carousel library required

### Backend
- Express.js API routes
- Existing `/api/upload` endpoint for file uploads
- Existing PUT `/api/dealers/:id` for saving settings

---

## User Interface Specifications

### Admin Settings Page

#### Hero Type Selector
```
┌────────────────────────────────────────────────────────────────┐
│ Home Page Hero Section                                        │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│ Hero Background Type                                           │
│ Choose how you want your home page hero section to appear.    │
│                                                                │
│ ┌──────────────┐  ┌──────────────┐  ┌──────────────┐         │
│ │ ○ Single     │  │ ○ Video      │  │ ● Carousel   │         │
│ │   Image      │  │   Background │  │   Slideshow  │         │
│ │              │  │              │  │              │         │
│ │ One static   │  │ Looping      │  │ Rotating     │         │
│ │ background   │  │ video        │  │ images       │         │
│ └──────────────┘  └──────────────┘  └──────────────┘         │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

#### Carousel Upload Interface
```
┌────────────────────────────────────────────────────────────────┐
│ Hero Carousel Images                                           │
│ Upload multiple images to create a rotating carousel.          │
│ Images will automatically transition every 5 seconds.          │
│                                                                │
│ ┌──────────┐ ┌──────────┐ ┌──────────┐                        │
│ │ Image #1 │ │ Image #2 │ │ Image #3 │                        │
│ │   [×]    │ │   [×]    │ │   [×]    │                        │
│ └──────────┘ └──────────┘ └──────────┘                        │
│                                                                │
│ [ + Add Another Image ]                                        │
│                                                                │
│ 3 images in carousel                                           │
└────────────────────────────────────────────────────────────────┘
```

### Public Homepage Hero

#### Carousel Display
```
┌────────────────────────────────────────────────────────────────┐
│ [←]                    HERO CAROUSEL                      [→] │
│                                                                │
│              Dealership Name                                   │
│         Description text with overlay...                       │
│           [ Browse Inventory Button ]                          │
│                                                                │
│                    ○ ● ○ ○                                     │
└────────────────────────────────────────────────────────────────┘
```

---

## Out of Scope

The following are explicitly out of scope for this release:

- YouTube/Vimeo video embedding
- Drag-and-drop carousel image reordering
- Custom carousel transition effects
- Configurable carousel interval from admin
- Per-image captions or titles
- Image crop/edit tools in admin
- Video thumbnail generation
- Multiple video support
- Audio in videos
- Carousel pause on hover
- Fullscreen video mode

---

## Implementation Phases

### Phase 1: Database & Backend (Completed)
- ✅ Database migration for new columns
- ✅ Backend API route updates
- ✅ File upload validation

### Phase 2: Frontend Components (Completed)
- ✅ HeroCarousel component
- ✅ Admin UI for type selection
- ✅ Admin UI for media uploads
- ✅ Public homepage rendering

### Phase 3: Bug Fixes (Completed)
- ✅ Fixed backend route field extraction
- ✅ Fixed image cropping (contain vs cover)

### Phase 4: Documentation (Completed)
- ✅ Technical documentation
- ✅ Quick start guide
- ✅ Visual guide
- ✅ Bug fix documentation
- ✅ Product requirements (this document)

---

## Testing Requirements

### Unit Tests
- HeroCarousel component rendering
- Image transition logic
- Navigation button handlers

### Integration Tests
- File upload to Cloudinary
- Database save/retrieve
- API endpoint responses

### Manual Testing
- All hero types display correctly
- File uploads work (image & video)
- Carousel navigation functions
- Mobile responsiveness
- Browser compatibility
- Image display (full, no cropping)

---

## Deployment Checklist

- [x] Database migration executed
- [x] Backend deployed with updated routes
- [x] Frontend built and deployed
- [x] Cloudinary integration tested
- [x] All hero types tested on production
- [x] Documentation published
- [x] Admin users notified of new feature

---

## Rollback Plan

If issues arise in production:

1. **Database Rollback**: Run reverse migration (DROP columns)
2. **Code Rollback**: Revert to previous git commit
3. **Frontend Rollback**: Deploy previous build
4. **Data Integrity**: Existing `hero_background_image` preserved

---

## Future Enhancements

### Priority 1 (Next Sprint)
- Drag-and-drop carousel image reordering
- Configurable carousel interval (3-10 seconds)

### Priority 2 (Future)
- YouTube/Vimeo video embedding
- Custom transition effects (slide, fade, zoom)
- Video upload progress indicator
- Per-image captions

### Priority 3 (Backlog)
- AI-powered image optimization suggestions
- Template carousel sets (e.g., "Showroom Tour")
- Analytics on hero engagement
- A/B testing different hero types

---

## Stakeholder Sign-off

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Product Manager | [PM Name] | _________ | 2026-01-04 |
| Tech Lead | [TL Name] | _________ | 2026-01-04 |
| QA Lead | [QA Name] | _________ | 2026-01-04 |
| UI/UX Designer | [UX Name] | _________ | 2026-01-04 |

---

## Appendix

### Related Documentation
- Technical Docs: `HERO_MEDIA_FEATURE.md`
- Quick Start: `HERO_MEDIA_QUICK_START.md`
- Visual Guide: `HERO_MEDIA_VISUAL_GUIDE.md`
- Bug Fixes: `HERO_MEDIA_BUG_FIX.md`, `HERO_MEDIA_IMAGE_DISPLAY_FIX.md`
- Architecture: `docs/architecture/hero-media-architecture.md`
- SM Planning: `docs/stories/hero-media-sprint-plan.md`

### Glossary
- **Hero Section**: Large banner area at top of homepage
- **Carousel**: Rotating slideshow of images
- **CTA**: Call-to-action (Browse Inventory button)
- **Cloudinary**: Cloud-based media management service
- **JSONB**: PostgreSQL JSON binary storage format

---

**Document History**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-04 | PM Team | Initial release post-implementation |
