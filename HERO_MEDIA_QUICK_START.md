# Hero Media Quick Start Guide

## What's New?
Your homepage hero section now supports three types of backgrounds:
- 📷 **Static Image** (existing)
- 🎬 **Video Loop** (NEW!)
- 🎠 **Image Carousel** (NEW!)

## Quick Setup (5 minutes)

### Step 1: Run Database Migration
```bash
# Apply the migration
Get-Content backend\db\migrations\20260104_add_hero_media_options.sql | docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype

# Verify (should show hero_type, hero_video_url, hero_carousel_images)
docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "\d dealership"
```

### Step 2: Access Admin Settings
1. Navigate to CMS admin panel
2. Go to **Settings** page
3. Scroll to **Home Page Hero Section**

### Step 3: Choose Your Hero Type

#### Option A: Single Image (Default)
```
✓ Upload one image
✓ JPG, PNG, or WebP
✓ Recommended: 1920x1080px
✓ Max size: 5MB
```

#### Option B: Video Background
```
✓ Upload one video
✓ MP4, WebM, or OGG
✓ Recommended: 15-30 seconds
✓ Max size: 50MB
✓ Will auto-loop and mute
```

#### Option C: Image Carousel
```
✓ Upload 3-5 images
✓ Same specs as single image
✓ Auto-rotates every 5 seconds
✓ Navigation arrows + dots
```

### Step 4: Test on Homepage
1. Save your changes
2. Visit public homepage
3. Verify your media displays correctly

## Example Workflows

### Switch from Image to Video
1. Select "Video" radio button
2. Click "Upload Hero Background Video"
3. Choose your MP4 file
4. Preview appears automatically
5. Click "Save Settings"
6. Homepage now shows video background

### Create a Carousel
1. Select "Image Carousel" radio button
2. Click "Add First Image" → Select image → Upload
3. Click "Add Another Image" → Repeat 2-4 more times
4. Hover over thumbnails to remove unwanted images
5. Click "Save Settings"
6. Homepage shows rotating carousel

### Revert to Default
1. Select "Single Image" radio button
2. If image exists, click "Remove Image"
3. Click "Save Settings"
4. Homepage shows blue gradient (default)

## Troubleshooting

**Video won't play**
- Ensure MP4 format (best compatibility)
- Check file size < 50MB
- Clear browser cache

**Carousel not rotating**
- Need at least 2 images
- Check browser console for errors
- Verify all images uploaded successfully

**Upload fails**
- Check file type (images: JPG/PNG/WebP, video: MP4/WebM/OGG)
- Verify file size limits (images: 5MB, video: 50MB)
- Ensure stable internet connection

## Tips for Best Results

### Images
- Use high-quality professional photos
- Maintain consistent aspect ratio (16:9)
- Ensure important content in center (safe zone)
- Test on mobile devices

### Videos
- Keep file size small for faster loading
- Use simple, looping content
- Avoid text/details (will be overlaid)
- Test on mobile data connection

### Carousel
- Use 3-5 images (sweet spot)
- Tell a visual story with sequence
- Maintain visual consistency (color palette, style)
- First image shows first (choose wisely)

## What Happens on Save?

1. Media URLs stored in database
2. Public homepage immediately updates
3. Old media remains in Cloudinary (no auto-delete)
4. Changes visible to all visitors

## Need Help?

See full documentation: `HERO_MEDIA_FEATURE.md`

Common questions:
- **Can I use both video and carousel?** No, choose one type
- **Can I reorder carousel images?** Not yet - upload in desired order
- **Will video play on mobile?** Yes, with `playsInline` attribute
- **Can I use animated GIFs?** Yes, as images (not recommended for performance)
