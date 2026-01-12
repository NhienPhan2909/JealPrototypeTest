# Promotional Panels Feature

## Overview
This feature adds two promotional panels for Finance and Warranty sections on the homepage, displayed below the Customer Reviews section. Each panel includes:
- Background image (customizable)
- Promotional text overlay
- "View Our Policy" button linking to the respective Finance or Warranty page
- Side-by-side layout (responsive: stacked on mobile, side-by-side on desktop)

## Implementation Summary

### 1. Database Changes
**Migration File:** `backend/db/migrations/008_add_promo_panels.sql`

Added four new columns to the `dealership` table:
- `finance_promo_image` (TEXT) - Finance panel background image URL
- `finance_promo_text` (TEXT) - Finance panel promotional text
- `warranty_promo_image` (TEXT) - Warranty panel background image URL
- `warranty_promo_text` (TEXT) - Warranty panel promotional text

### 2. Backend Changes

#### API Routes (`backend/routes/dealers.js`)
- Added new fields to the PUT `/api/dealers/:id` endpoint
- Added validation for promotional text fields (max 500 characters)
- Added sanitization for promotional text to prevent XSS attacks

#### Database Layer (`backend/db/dealers.js`)
- Updated `update()` function to handle new promotional panel fields
- Added proper parameter handling for the new columns

### 3. Frontend Changes

#### New Component (`frontend/src/components/PromotionalPanels.jsx`)
A reusable component that displays two promotional panels side-by-side:
- Accepts props for finance and warranty images and text
- Features gradient backgrounds as fallbacks when no image is provided
- Uses theme colors for CTA buttons
- Fully responsive design

**Props:**
- `financeImage` - Finance panel background image URL
- `financeText` - Finance panel promotional text
- `warrantyImage` - Warranty panel background image URL
- `warrantyText` - Warranty panel promotional text

#### Homepage (`frontend/src/pages/public/Home.jsx`)
- Imported and added `PromotionalPanels` component
- Positioned below the Google Reviews Carousel
- Passes dealership data as props to the component

#### Admin Settings (`frontend/src/pages/admin/DealerSettings.jsx`)
Added a new "Homepage Promotional Panels" section with:
- Image upload functionality for both panels (Finance and Warranty)
- Text input fields for promotional messages
- Image preview and remove functionality
- Character limit (500 chars) for promotional text
- Form submission includes new fields

**Upload Features:**
- File type validation (JPG, PNG, WebP only)
- File size validation (max 5MB)
- Image preview before saving
- Remove image option
- Uses existing `/api/upload` endpoint

## User Guide

### For Admin Users

#### Setting Up Promotional Panels

1. **Navigate to Admin Settings:**
   - Log in to the admin panel
   - Go to "Dealership Settings"

2. **Configure Finance Panel:**
   - Scroll to "Homepage Promotional Panels" section
   - Under "Finance Promotional Panel":
     - Click "Choose File" to upload a background image (recommended: 800x600px)
     - Enter promotional text (e.g., "Flexible Financing Options Available")
   
3. **Configure Warranty Panel:**
   - Under "Warranty Promotional Panel":
     - Click "Choose File" to upload a background image (recommended: 800x600px)
     - Enter promotional text (e.g., "Comprehensive Warranty Coverage")

4. **Save Changes:**
   - Click "Save Settings" at the bottom of the form
   - Wait for confirmation message

5. **Preview:**
   - Visit the homepage to see the promotional panels below Customer Reviews

#### Image Guidelines
- **Format:** JPG, PNG, or WebP
- **Size:** Maximum 5MB
- **Recommended Dimensions:** 800x600px (4:3 aspect ratio)
- **Content:** Use high-quality images that represent finance or warranty services
- **Text Overlay:** Keep promotional text concise (max 500 characters)

### For Public Users
The promotional panels appear on the homepage below the Customer Reviews section:
- Desktop: Two panels side-by-side
- Mobile/Tablet: Stacked vertically
- Each panel has a "View Our Policy" button linking to the respective page

## Technical Details

### Styling
- Uses Tailwind CSS for responsive layout
- Background images use `cover` sizing with dark overlay (rgba(0, 0, 0, 0.5))
- Gradient fallbacks provide visual appeal when no image is uploaded
- Text has drop shadows for readability over images
- CTA buttons use theme colors from dealership settings

### Default Behavior
If no promotional content is provided:
- Panels still display with gradient backgrounds
- Default text: "Explore Our Financing Options" / "Learn About Our Warranty"
- Buttons still link to Finance and Warranty pages

### Responsive Design
- Grid layout: `grid-cols-1 md:grid-cols-2`
- Minimum height: 300px per panel
- Gap between panels: 1.5rem (24px)
- Text and button sizes adjust for mobile

## Files Modified/Created

### Created:
1. `backend/db/migrations/008_add_promo_panels.sql`
2. `frontend/src/components/PromotionalPanels.jsx`

### Modified:
1. `backend/routes/dealers.js` - Added API support for promo fields
2. `backend/db/dealers.js` - Added database update logic
3. `frontend/src/pages/public/Home.jsx` - Added component to homepage
4. `frontend/src/pages/admin/DealerSettings.jsx` - Added admin UI for managing panels

## Testing

### Manual Testing Checklist
- [ ] Upload finance promotional image via admin panel
- [ ] Upload warranty promotional image via admin panel
- [ ] Enter promotional text for both panels
- [ ] Save settings and verify success message
- [ ] View homepage and verify panels appear below Customer Reviews
- [ ] Verify panels are side-by-side on desktop
- [ ] Verify panels are stacked on mobile (resize browser)
- [ ] Click "View Our Policy" buttons and verify navigation works
- [ ] Test with no images (verify gradient fallbacks)
- [ ] Test with no text (verify default text)
- [ ] Test image validation (wrong file type, oversized file)
- [ ] Test remove image functionality

## Future Enhancements
Possible improvements for future iterations:
1. Add multiple promotional panel slots (not just Finance/Warranty)
2. Allow custom button text and link destinations
3. Add background color/gradient customization
4. Support for video backgrounds
5. A/B testing capabilities
6. Analytics tracking for button clicks

## Security Considerations
- Image uploads go through existing validation (file type, size)
- Promotional text is sanitized to prevent XSS attacks
- URL validation prevents malicious image sources
- Character limits prevent database overflow
- All updates require authenticated session

## Performance Notes
- Images are served via Cloudinary CDN (fast loading)
- Lazy loading could be added for images below the fold
- Optimized image sizes recommended for better performance
- Background images use CSS for efficient rendering
