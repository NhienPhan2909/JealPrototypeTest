# Hero Media Feature - Visual Guide

## Overview
This guide provides visual descriptions and step-by-step instructions for using the new Hero Media feature.

---

## What You'll See

### Admin Panel - Hero Type Selector

When you open the **Settings** page in the CMS admin, scroll to the **Home Page Hero Section**. You'll see three option cards arranged horizontally:

```
┌────────────────────┐  ┌────────────────────┐  ┌────────────────────┐
│  ○ Single Image    │  │  ○ Video           │  │  ○ Image Carousel  │
│                    │  │                    │  │                    │
│  One static        │  │  Looping           │  │  Rotating          │
│  background image  │  │  background video  │  │  slideshow         │
└────────────────────┘  └────────────────────┘  └────────────────────┘
```

**Active Selection**: The selected option has a blue border and light blue background.

---

## User Workflows

### Workflow 1: Upload a Single Image

**Step 1**: Select "Single Image" radio button
```
┌────────────────────┐  
│  ● Single Image    │  ← Blue border (selected)
│  One static        │
│  background image  │
└────────────────────┘
```

**Step 2**: Upload interface appears
```
┌─────────────────────────────────────────────┐
│ Hero Background Image                       │
│                                             │
│ Upload a custom background image for your   │
│ dealership's home page hero section.        │
│                                             │
│  ┌────────────────────────────────┐         │
│  │ Upload Hero Background Image   │         │
│  └────────────────────────────────┘         │
└─────────────────────────────────────────────┘
```

**Step 3**: After upload, preview shows
```
┌─────────────────────────────────────────────┐
│ ┌─────────────────────────────────────────┐ │
│ │                                         │ │
│ │      [Your Image with Dark Overlay]     │ │
│ │       "Preview with overlay"            │ │
│ │                                         │ │
│ └─────────────────────────────────────────┘ │
│                                             │
│  [Change Image]  [Remove Image]             │
└─────────────────────────────────────────────┘
```

---

### Workflow 2: Upload a Video

**Step 1**: Select "Video" radio button
```
┌────────────────────┐  
│  ● Video           │  ← Blue border (selected)
│  Looping           │
│  background video  │
└────────────────────┘
```

**Step 2**: Upload interface appears
```
┌─────────────────────────────────────────────┐
│ Hero Background Video                       │
│                                             │
│ Upload a video to use as your hero          │
│ background. Video will loop automatically.  │
│ (Max 50MB)                                  │
│                                             │
│  ┌────────────────────────────────┐         │
│  │ Upload Hero Background Video   │         │
│  └────────────────────────────────┘         │
└─────────────────────────────────────────────┘
```

**Step 3**: After upload, preview shows
```
┌─────────────────────────────────────────────┐
│ ┌─────────────────────────────────────────┐ │
│ │                                         │ │
│ │      [Video Playing with Overlay]       │ │
│ │       "Preview with overlay"            │ │
│ │                                         │ │
│ └─────────────────────────────────────────┘ │
│                                             │
│  [Change Video]  [Remove Video]             │
└─────────────────────────────────────────────┘
```

---

### Workflow 3: Create Image Carousel

**Step 1**: Select "Image Carousel" radio button
```
┌────────────────────┐  
│  ● Image Carousel  │  ← Blue border (selected)
│  Rotating          │
│  slideshow         │
└────────────────────┘
```

**Step 2**: Upload first image
```
┌─────────────────────────────────────────────┐
│ Hero Carousel Images                        │
│                                             │
│ Upload multiple images to create a rotating │
│ carousel. Images will automatically         │
│ transition every 5 seconds.                 │
│                                             │
│  ┌────────────────┐                         │
│  │ Add First Image│                         │
│  └────────────────┘                         │
└─────────────────────────────────────────────┘
```

**Step 3**: After uploads, grid shows all images
```
┌─────────────────────────────────────────────┐
│ ┌─────────┐ ┌─────────┐ ┌─────────┐         │
│ │  #1     │ │  #2     │ │  #3     │         │
│ │ [Image] │ │ [Image] │ │ [Image] │         │
│ │    ×    │ │    ×    │ │    ×    │ ← Hover │
│ └─────────┘ └─────────┘ └─────────┘         │
│                                             │
│  ┌─────────────────┐                        │
│  │ Add Another Image│                       │
│  └─────────────────┘                        │
│                                             │
│  3 images in carousel                       │
└─────────────────────────────────────────────┘
```

---

## Public Homepage Display

### Single Image Display
```
┌───────────────────────────────────────────────────┐
│                                                   │
│         [Background Image with Dark Overlay]      │
│                                                   │
│              Dealership Name                      │
│                                                   │
│         Your dealership description text...       │
│                                                   │
│           [ Browse Inventory ]                    │
│                                                   │
└───────────────────────────────────────────────────┘
```

### Video Display
```
┌───────────────────────────────────────────────────┐
│                                                   │
│         [Looping Video with Dark Overlay]         │
│                                                   │
│              Dealership Name                      │
│                                                   │
│         Your dealership description text...       │
│                                                   │
│           [ Browse Inventory ]                    │
│                                                   │
└───────────────────────────────────────────────────┘
```

### Carousel Display
```
┌───────────────────────────────────────────────────┐
│ ←                                             →   │ ← Navigation
│         [Rotating Images with Dark Overlay]       │
│                                                   │
│              Dealership Name                      │
│                                                   │
│         Your dealership description text...       │
│                                                   │
│           [ Browse Inventory ]                    │
│                                                   │
│              ○ ● ○ ○                              │ ← Dots
└───────────────────────────────────────────────────┘
```

**Carousel Controls**:
- **← →** arrows: Click to manually navigate
- **○ ● ○** dots: Click to jump to specific slide (filled = current)
- Auto-advances every 5 seconds

---

## File Type & Size Reference

### Accepted File Types

**Images** (Single Image & Carousel):
- ✓ JPEG (.jpg, .jpeg)
- ✓ PNG (.png)
- ✓ WebP (.webp)
- ✗ GIF, BMP, TIFF not recommended

**Videos**:
- ✓ MP4 (.mp4) - **Recommended**
- ✓ WebM (.webm)
- ✓ OGG (.ogg)
- ✗ AVI, MOV, WMV not supported

### File Size Limits

| Type          | Max Size | Recommended    |
|---------------|----------|----------------|
| Image         | 5 MB     | 1-2 MB         |
| Video         | 50 MB    | 10-20 MB       |

### Recommended Dimensions

| Type          | Dimensions      | Aspect Ratio |
|---------------|-----------------|--------------|
| Hero Image    | 1920 × 1080 px  | 16:9         |
| Carousel      | 1920 × 1080 px  | 16:9         |
| Video         | 1920 × 1080 px  | 16:9         |

---

## Success & Error Messages

### Success Messages (Green)
```
┌─────────────────────────────────────────────┐
│ ✓ Hero background image uploaded            │
│   successfully!                             │
└─────────────────────────────────────────────┘
```

```
┌─────────────────────────────────────────────┐
│ ✓ Hero video uploaded successfully!         │
└─────────────────────────────────────────────┘
```

```
┌─────────────────────────────────────────────┐
│ ✓ Carousel image added successfully!        │
└─────────────────────────────────────────────┘
```

### Error Messages (Red)
```
┌─────────────────────────────────────────────┐
│ ✗ Invalid file type. Please upload JPG,    │
│   PNG, or WebP images only.                 │
└─────────────────────────────────────────────┘
```

```
┌─────────────────────────────────────────────┐
│ ✗ File too large. Maximum size is 5MB.     │
└─────────────────────────────────────────────┘
```

```
┌─────────────────────────────────────────────┐
│ ✗ File too large. Maximum size is 50MB.    │
└─────────────────────────────────────────────┘
```

---

## Browser Compatibility

| Browser           | Image | Video | Carousel |
|-------------------|-------|-------|----------|
| Chrome (latest)   | ✓     | ✓     | ✓        |
| Firefox (latest)  | ✓     | ✓     | ✓        |
| Safari (latest)   | ✓     | ✓     | ✓        |
| Edge (latest)     | ✓     | ✓     | ✓        |
| Mobile (iOS)      | ✓     | ✓*    | ✓        |
| Mobile (Android)  | ✓     | ✓     | ✓        |

*Video requires `playsInline` attribute (already implemented)

---

## Common UI Patterns

### Remove Button (Hover Effect)
When hovering over carousel image thumbnails, an × button appears:

```
Before hover:               After hover:
┌─────────┐                ┌─────────┐
│  #1     │                │  #1  ×  │ ← Red × button
│ [Image] │                │ [Image] │
└─────────┘                └─────────┘
```

### Preview Overlay
All preview sections show a dark overlay (40% black) with text:

```
┌───────────────────────┐
│ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓ │
│ ▓▓▓ Your Media  ▓▓▓▓▓ │
│ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓ │
│                       │
│  Preview with overlay │ ← White text
│                       │
└───────────────────────┘
```

This matches how content will appear on the public homepage.

---

## Tips for Best Visual Results

### Image Selection
- ✓ Use professional, high-quality photos
- ✓ Ensure important subjects are centered
- ✓ Avoid busy backgrounds (text overlay readability)
- ✓ Test on mobile screens

### Video Selection
- ✓ Use simple, looping content
- ✓ Avoid text or fine details
- ✓ Keep it short (15-30 seconds)
- ✓ Test with slow internet connection

### Carousel Creation
- ✓ Use 3-5 images (optimal)
- ✓ Maintain visual consistency
- ✓ Tell a story with sequence
- ✓ First image makes first impression

---

## Quick Reference Card

```
╔════════════════════════════════════════════╗
║      HERO MEDIA QUICK REFERENCE            ║
╠════════════════════════════════════════════╣
║ IMAGE:                                     ║
║  • Types: JPG, PNG, WebP                   ║
║  • Max: 5MB                                ║
║  • Size: 1920×1080px                       ║
║                                            ║
║ VIDEO:                                     ║
║  • Types: MP4, WebM, OGG                   ║
║  • Max: 50MB                               ║
║  • Size: 1920×1080px                       ║
║  • Duration: 15-30 seconds                 ║
║                                            ║
║ CAROUSEL:                                  ║
║  • Count: 3-5 images recommended           ║
║  • Interval: Auto 5 seconds                ║
║  • Same specs as single image              ║
╚════════════════════════════════════════════╝
```

---

## Need Help?

- **Documentation**: See `HERO_MEDIA_FEATURE.md` for technical details
- **Quick Start**: See `HERO_MEDIA_QUICK_START.md` for setup instructions
- **Issues**: Check browser console for error messages
