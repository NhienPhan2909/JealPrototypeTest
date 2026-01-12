# 7. External APIs Integration

External service dependencies: **Cloudinary** for image uploads/storage/CDN delivery, and **SMTP Email Service** (v1.4+) for lead notifications. Both services use environment variable configuration with graceful degradation.

## Cloudinary Integration

Cloudinary handles image uploads, storage, and CDN delivery. Use unsigned upload preset for fastest setup (no server-side signature generation). Frontend upload widget handles all uploads directly to Cloudinary. Backend only stores returned URLs in PostgreSQL.

## Cloudinary Account Setup (5 minutes)

**Step 1: Create Free Account**

1. Go to https://cloudinary.com/users/register/free
2. Sign up with email (no credit card required)
3. Note your **Cloud Name** from dashboard (e.g., `dxyz123abc`)

**Step 2: Create Upload Preset**

1. Navigate to **Settings → Upload** in Cloudinary dashboard
2. Scroll to **Upload presets**
3. Click **Add upload preset**
4. Configure:
   - **Preset name:** `vehicle-images`
   - **Signing mode:** **Unsigned** (simplest - no backend signature required)
   - **Folder:** `dealership-vehicles` (optional - organizes uploads)
   - **Format:** Leave as Auto (Cloudinary auto-converts to WebP with fallback)
   - **Transformation:** Add optional transformation:
     - **Quality:** Auto (Cloudinary optimizes automatically)
     - **Max width:** 1920 (prevents huge uploads)
5. Click **Save**
6. Note the **Preset name**: `vehicle-images`

**Result:** You now have an unsigned upload preset that allows direct browser uploads without server involvement.

## Environment Variables

**Frontend (.env or Vite config)**

```bash
VITE_CLOUDINARY_CLOUD_NAME=your-cloud-name
VITE_CLOUDINARY_UPLOAD_PRESET=vehicle-images
```

**Note:** Vite requires `VITE_` prefix for env vars accessible in browser. In production, set these on Railway/Render.

**Backend (.env) - Optional for fallback upload endpoint**

```bash
CLOUDINARY_CLOUD_NAME=your-cloud-name
CLOUDINARY_API_KEY=123456789012345
CLOUDINARY_API_SECRET=abcdefghijklmnopqrstuvwxyz
```

**When needed:** Only if implementing `/api/upload` fallback endpoint.

## Frontend Integration - Upload Widget

**Add Cloudinary Widget Script to index.html**

Add before closing `</body>` tag in `frontend/index.html`:

```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Dealership Platform</title>
  </head>
  <body>
    <div id="root"></div>
    <script type="module" src="/src/main.jsx"></script>

    <!-- Cloudinary Upload Widget -->
    <script src="https://upload-widget.cloudinary.com/global/all.js" type="text/javascript"></script>
  </body>
</html>
```

**Why:** Loads Cloudinary's upload widget globally. Widget is ~100KB, loaded once per session.

## Usage in VehicleForm.jsx (Multiple Images)

**Scenario:** Admin uploads 1-10 vehicle photos for a listing.

**IMPORTANT (Updated 2025-11-28):** VehicleForm now uses file input + `/api/upload` endpoint instead of Cloudinary widget due to reliability issues (page freeze bug). This matches the DealerSettings hero background implementation.

**Implementation:**

```javascript
import { useState } from 'react';

function VehicleForm() {
  const [images, setImages] = useState([]); // Array of Cloudinary URLs
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState('');

  const handlePhotoUpload = async (e) => {
    const files = Array.from(e.target.files || []);
    if (files.length === 0) return;

    // Validate total image count (max 10)
    if (images.length + files.length > 10) {
      setError(`Cannot upload ${files.length} files. Maximum total is 10 images (currently have ${images.length}).`);
      e.target.value = '';
      return;
    }

    // Validate file types
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp', 'image/jpg'];
    const invalidFiles = files.filter(file => !allowedTypes.includes(file.type));
    if (invalidFiles.length > 0) {
      setError('Invalid file type. Please upload JPG, PNG, or WebP images only.');
      e.target.value = '';
      return;
    }

    // Validate file sizes (5MB max per file)
    const maxSize = 5 * 1024 * 1024; // 5MB
    const oversizedFiles = files.filter(file => file.size > maxSize);
    if (oversizedFiles.length > 0) {
      setError('One or more files are too large. Maximum size is 5MB per file.');
      e.target.value = '';
      return;
    }

    setUploading(true);
    setError('');

    try {
      const uploadedUrls = [];

      // Upload each file sequentially to /api/upload
      for (const file of files) {
        const formData = new FormData();
        formData.append('image', file);

        const response = await fetch('/api/upload', {
          method: 'POST',
          credentials: 'include',
          body: formData
        });

        if (!response.ok) {
          const errorData = await response.json();
          throw new Error(errorData.error || 'Upload failed');
        }

        const data = await response.json();
        uploadedUrls.push(data.url);
      }

      // Add all uploaded URLs to images array
      setImages(prev => [...prev, ...uploadedUrls]);
    } catch (err) {
      console.error('Upload error:', err);
      setError('Failed to upload images: ' + err.message);
    } finally {
      setUploading(false);
      e.target.value = ''; // Reset input
    }
  };

  const handleRemoveImage = (indexToRemove) => {
    setImages((prev) => prev.filter((_, index) => index !== indexToRemove));
  };

  return (
    <div className="vehicle-form">
      <h2>Add Vehicle</h2>

      {/* Other form fields (make, model, year, etc.) */}

      {/* Image Upload Section */}
      <div className="mb-4">
        <label className="block font-semibold mb-2">Vehicle Photos</label>
        <p className="text-sm text-gray-600 mb-3">
          Upload up to 10 photos. Accepted formats: JPG, PNG, WebP (max 5MB per file)
        </p>

        {/* Upload Button (label wrapping hidden file input) */}
        <label
          className={`inline-block mb-4 ${
            images.length >= 10 || uploading
              ? 'btn-secondary cursor-not-allowed opacity-50'
              : 'btn-primary cursor-pointer'
          }`}
        >
          {uploading
            ? 'Uploading...'
            : images.length >= 10
            ? 'Maximum 10 photos reached'
            : 'Upload Photos'}
          <input
            type="file"
            accept="image/jpeg,image/png,image/webp,image/jpg"
            multiple
            onChange={handlePhotoUpload}
            disabled={images.length >= 10 || uploading}
            className="hidden"
          />
        </label>

        {error && <p className="text-red-500 text-sm">{error}</p>}

        {/* Display uploaded images */}
        <div className="grid grid-cols-4 gap-4 mt-4">
          {images.map((url, index) => (
            <div key={index} className="relative">
              <img
                src={url}
                alt={`Vehicle ${index + 1}`}
                className="w-full h-32 object-cover rounded"
              />
              <button
                type="button"
                onClick={() => handleRemoveImage(index)}
                className="absolute top-1 right-1 bg-red-500 text-white rounded-full w-6 h-6 text-xs"
              >
                ✕
              </button>
            </div>
          ))}
        </div>

        <p className="text-sm text-gray-600 mt-2">
          {images.length} / 10 photos uploaded
        </p>
      </div>

      {/* Form submit */}
      <button
        type="submit"
        onClick={handleSubmit}
        className="bg-green-500 text-white px-6 py-2 rounded"
      >
        Save Vehicle
      </button>
    </div>
  );

  async function handleSubmit(e) {
    e.preventDefault();

    // Collect form data (use React Hook Form in real implementation)
    const vehicleData = {
      dealership_id: 1, // From context
      make: 'Toyota',
      model: 'Camry',
      year: 2015,
      price: 15999.99,
      mileage: 75000,
      condition: 'used',
      status: 'active',
      title: '2015 Toyota Camry SE',
      description: 'Well-maintained sedan...',
      images: images, // Array of Cloudinary URLs
    };

    // POST to API
    const response = await fetch('/api/vehicles', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(vehicleData),
    });

    if (response.ok) {
      alert('Vehicle saved!');
      // Navigate back to vehicle list
    }
  }
}
```

**Why File Input Instead of Widget:**
- Eliminates Cloudinary widget script loading dependencies
- More reliable (no race conditions or page freeze issues)
- Consistent with DealerSettings hero background upload
- Native OS file picker is universally supported
- Full control over validation and error handling

**What Gets Stored in Database:**

```json
{
  "images": [
    "https://res.cloudinary.com/dxyz123abc/image/upload/v1732012345/dealership-vehicles/abc123.jpg",
    "https://res.cloudinary.com/dxyz123abc/image/upload/v1732012346/dealership-vehicles/def456.jpg",
    "https://res.cloudinary.com/dxyz123abc/image/upload/v1732012347/dealership-vehicles/ghi789.jpg"
  ]
}
```

Stored as **JSONB array** in PostgreSQL `vehicle.images` column.

## Usage in DealerSettings.jsx (Single Logo)

**Scenario:** Admin uploads dealership logo.

**Implementation:**

```javascript
import { useState } from 'react';

function DealerSettings() {
  const [logoUrl, setLogoUrl] = useState(''); // Single Cloudinary URL

  const handleLogoUpload = () => {
    window.cloudinary.openUploadWidget(
      {
        cloudName: import.meta.env.VITE_CLOUDINARY_CLOUD_NAME,
        uploadPreset: import.meta.env.VITE_CLOUDINARY_UPLOAD_PRESET,
        sources: ['local'],
        multiple: false, // Only one logo
        maxFileSize: 2000000, // 2MB max for logos
        clientAllowedFormats: ['jpg', 'png', 'svg'],
        cropping: true, // Enable cropping for logos (square crop recommended)
        croppingAspectRatio: 1, // Force square crop
        folder: 'dealership-logos',
      },
      (error, result) => {
        if (error) {
          console.error('Logo upload error:', error);
          return;
        }

        if (result.event === 'success') {
          setLogoUrl(result.info.secure_url);
        }
      }
    );
  };

  const handleRemoveLogo = () => {
    setLogoUrl('');
  };

  return (
    <div className="dealer-settings">
      <h2>Dealership Settings</h2>

      {/* Logo Upload */}
      <div className="mb-4">
        <label className="block font-semibold mb-2">Logo</label>

        {logoUrl ? (
          <div className="flex items-center gap-4">
            <img src={logoUrl} alt="Logo" className="w-24 h-24 object-contain" />
            <button
              type="button"
              onClick={handleRemoveLogo}
              className="text-red-500 underline"
            >
              Remove Logo
            </button>
          </div>
        ) : (
          <button
            type="button"
            onClick={handleLogoUpload}
            className="bg-blue-500 text-white px-4 py-2 rounded"
          >
            Upload Logo
          </button>
        )}
      </div>

      {/* Other dealership fields (name, address, etc.) */}

      <button
        type="submit"
        onClick={handleSave}
        className="bg-green-500 text-white px-6 py-2 rounded"
      >
        Save Settings
      </button>
    </div>
  );

  async function handleSave(e) {
    e.preventDefault();

    const dealershipData = {
      name: 'Acme Auto Sales',
      logo_url: logoUrl, // Cloudinary URL or empty string
      address: '123 Main St...',
      phone: '(555) 123-4567',
      email: 'sales@acmeauto.com',
      hours: 'Mon-Fri 9am-6pm',
      about: 'Family-owned dealership...',
    };

    const response = await fetch('/api/dealers/1', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(dealershipData),
    });

    if (response.ok) {
      alert('Settings saved!');
    }
  }
}
```

**What Gets Stored in Database:**

```json
{
  "logo_url": "https://res.cloudinary.com/dxyz123abc/image/upload/v1732012400/dealership-logos/logo_abc.png"
}
```

Stored as **TEXT** in PostgreSQL `dealership.logo_url` column.

## Usage in DealerSettings.jsx (Hero Background Image)

**Scenario:** Admin uploads custom hero background image for dealership home page.

**Technical Decision:** Uses standard file input instead of Cloudinary widget due to DOM compatibility issues with the widget in certain contexts. Uploads via `/api/upload` endpoint.

**Implementation:**

```javascript
import { useState } from 'react';

function DealerSettings() {
  const [heroBackgroundUrl, setHeroBackgroundUrl] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleHeroBackgroundUpload = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Client-side validation
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      setError('Invalid file type. Please upload JPG, PNG, or WebP images only.');
      e.target.value = '';
      return;
    }

    const maxSize = 5 * 1024 * 1024; // 5MB
    if (file.size > maxSize) {
      setError('File too large. Maximum size is 5MB.');
      e.target.value = '';
      return;
    }

    setLoading(true);
    setError('');

    try {
      const formData = new FormData();
      formData.append('image', file);

      const response = await fetch('/api/upload', {
        method: 'POST',
        credentials: 'include',
        body: formData
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || 'Upload failed');
      }

      const data = await response.json();
      setHeroBackgroundUrl(data.url);
    } catch (err) {
      console.error('Upload error:', err);
      setError('Failed to upload image: ' + err.message);
    } finally {
      setLoading(false);
      e.target.value = ''; // Reset input
    }
  };

  const handleRemoveHeroBackground = () => {
    setHeroBackgroundUrl('');
  };

  return (
    <div className="dealer-settings">
      <h2>Dealership Settings</h2>

      {/* Hero Background Upload */}
      <div className="mb-4">
        <label className="block font-medium mb-1">
          Home Page Hero Background Image
        </label>
        <p className="text-sm text-gray-600 mb-2">
          Upload a custom background image for your dealership's home page hero section.
        </p>

        {heroBackgroundUrl ? (
          <div className="space-y-3">
            {/* Preview with overlay to show how it will look */}
            <div className="relative w-full h-48 border rounded overflow-hidden">
              <img
                src={heroBackgroundUrl.replace('/upload/', '/upload/w_800,h_400,c_fill,f_auto/')}
                alt="Hero Background Preview"
                className="w-full h-full object-cover"
              />
              <div className="absolute inset-0 bg-black bg-opacity-40 flex items-center justify-center">
                <p className="text-white text-lg font-semibold drop-shadow-lg">
                  Preview with overlay
                </p>
              </div>
            </div>
            <div className="flex gap-3">
              <label className="btn-secondary cursor-pointer">
                Change Image
                <input
                  type="file"
                  accept="image/jpeg,image/png,image/webp"
                  onChange={handleHeroBackgroundUpload}
                  disabled={loading}
                  className="hidden"
                />
              </label>
              <button
                type="button"
                onClick={handleRemoveHeroBackground}
                disabled={loading}
                className="btn-danger"
              >
                Remove Background Image
              </button>
            </div>
          </div>
        ) : (
          <label className="btn-secondary cursor-pointer inline-block">
            Upload Hero Background
            <input
              type="file"
              accept="image/jpeg,image/png,image/webp"
              onChange={handleHeroBackgroundUpload}
              disabled={loading}
              className="hidden"
            />
          </label>
        )}

        {error && <p className="text-red-500 text-sm mt-2">{error}</p>}
      </div>

      {/* Other dealership fields */}

      <button type="submit" onClick={handleSave} className="btn-primary">
        Save Settings
      </button>
    </div>
  );

  async function handleSave(e) {
    e.preventDefault();

    const dealershipData = {
      name: 'Acme Auto Sales',
      logo_url: logoUrl,
      hero_background_image: heroBackgroundUrl || null, // Store as null if not set
      address: '123 Main St...',
      phone: '(555) 123-4567',
      email: 'sales@acmeauto.com',
      hours: 'Mon-Fri 9am-6pm',
      about: 'Family-owned dealership...',
    };

    const response = await fetch('/api/dealers/1', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(dealershipData)
    });

    if (response.ok) {
      alert('Settings saved!');
    }
  }
}
```

**What Gets Stored in Database:**

```json
{
  "hero_background_image": "https://res.cloudinary.com/dxyz123abc/image/upload/v1732012500/dealership-hero-backgrounds/hero_bg_123.jpg"
}
```

Stored as **TEXT** in PostgreSQL `dealership.hero_background_image` column.

**Backend Upload Endpoint Configuration:**

The `/api/upload` endpoint should be configured to store hero backgrounds in the `dealership-hero-backgrounds` Cloudinary folder:

```javascript
// backend/routes/upload.js
const cloudinary = require('cloudinary').v2;
const multer = require('multer');

const upload = multer({
  storage: multer.memoryStorage(),
  limits: { fileSize: 5 * 1024 * 1024 } // 5MB
});

router.post('/upload', requireAuth, upload.single('image'), async (req, res) => {
  try {
    if (!req.file) {
      return res.status(400).json({ error: 'No file uploaded' });
    }

    // Upload to Cloudinary
    const result = await new Promise((resolve, reject) => {
      const uploadStream = cloudinary.uploader.upload_stream(
        {
          folder: 'dealership-hero-backgrounds',
          resource_type: 'image',
          allowed_formats: ['jpg', 'png', 'webp']
        },
        (error, result) => {
          if (error) reject(error);
          else resolve(result);
        }
      );

      uploadStream.end(req.file.buffer);
    });

    res.json({ url: result.secure_url });
  } catch (error) {
    console.error('Upload error:', error);
    res.status(500).json({ error: 'Upload failed' });
  }
});
```

## Displaying Images on Public Site

**Hero Background Image - Home Page**

Display custom hero background with overlay for text readability:

```javascript
function Home() {
  const [dealership, setDealership] = useState(null);

  // Fetch dealership data
  useEffect(() => {
    fetch(`/api/dealers/${dealershipId}`)
      .then(res => res.json())
      .then(data => setDealership(data));
  }, [dealershipId]);

  return (
    <div
      className="text-white py-12 md:py-20 relative bg-gradient-to-r from-blue-500 to-blue-700"
      style={
        dealership?.hero_background_image
          ? {
              backgroundImage: `linear-gradient(rgba(0, 0, 0, 0.4), rgba(0, 0, 0, 0.4)), url(${dealership.hero_background_image})`,
              backgroundSize: 'cover',
              backgroundPosition: 'center',
              backgroundRepeat: 'no-repeat'
            }
          : undefined
      }
    >
      <div className="container mx-auto px-4 text-center relative z-10">
        <h1 className="text-3xl md:text-4xl lg:text-5xl font-bold mb-4 drop-shadow-lg">
          {dealership?.name || 'Welcome'}
        </h1>
        <p className="text-lg md:text-xl mb-8 max-w-2xl mx-auto drop-shadow-md">
          {dealership?.about ? dealership.about.substring(0, 150) + '...' : 'Quality vehicles at great prices.'}
        </p>
        <Link to="/inventory" className="inline-block bg-white text-blue-600 font-bold px-8 py-3 rounded-lg hover:bg-gray-100 transition shadow-lg">
          Browse Inventory
        </Link>
      </div>
    </div>
  );
}
```

**Key Implementation Details:**
- **40% black overlay:** `linear-gradient(rgba(0, 0, 0, 0.4), rgba(0, 0, 0, 0.4))` ensures text is readable over any background image
- **Graceful fallback:** If `hero_background_image` is null, defaults to blue gradient (`from-blue-500 to-blue-700`)
- **Text shadows:** `drop-shadow-lg` and `drop-shadow-md` on text elements provide additional contrast
- **Responsive:** Works across all screen sizes with appropriate padding and font sizing
- **No transformation needed:** Original Cloudinary URL can be used directly as Cloudinary automatically optimizes delivery

**Vehicle Listing - Thumbnails**

Use Cloudinary's URL transformation for automatic thumbnails:

```javascript
function VehicleCard({ vehicle }) {
  // Get first image or use placeholder
  const primaryImage = vehicle.images?.[0] || 'https://via.placeholder.com/400x300?text=No+Image';

  // Transform URL to thumbnail (400px width, auto quality, WebP)
  const thumbnailUrl = primaryImage.replace('/upload/', '/upload/w_400,f_auto,q_auto/');

  return (
    <div className="vehicle-card">
      <img src={thumbnailUrl} alt={vehicle.title} className="w-full h-48 object-cover" />
      <h3>{vehicle.title}</h3>
      <p>${vehicle.price.toLocaleString()}</p>
    </div>
  );
}
```

**URL Transformation Examples:**

| Original URL | Transformed URL | Effect |
|--------------|-----------------|--------|
| `.../upload/v123/image.jpg` | `.../upload/w_400,f_auto,q_auto/v123/image.jpg` | 400px width, auto format (WebP if supported), auto quality |
| `.../upload/v123/image.jpg` | `.../upload/w_1920,h_1080,c_limit/v123/image.jpg` | Max 1920x1080, maintain aspect ratio |
| `.../upload/v123/image.jpg` | `.../upload/c_thumb,g_auto,w_200,h_200/v123/image.jpg` | 200x200 square thumbnail, auto-crop to focus |

**For MVP, use simple transformations:**
- Thumbnails: `w_400,f_auto,q_auto`
- Gallery: `w_1920,f_auto,q_auto`
- Logo: `w_200,h_200,c_fit,f_auto`

**No Backend Code Required:** Transformations happen via URL syntax. Cloudinary CDN serves optimized images.

## Vehicle Detail - Image Gallery

```javascript
function ImageGallery({ images }) {
  const [selectedIndex, setSelectedIndex] = useState(0);

  if (!images || images.length === 0) {
    return <img src="https://via.placeholder.com/800x600?text=No+Images" alt="Placeholder" />;
  }

  const currentImage = images[selectedIndex];
  const galleryUrl = currentImage.replace('/upload/', '/upload/w_1920,f_auto,q_auto/');

  return (
    <div className="image-gallery">
      {/* Main Image */}
      <img
        src={galleryUrl}
        alt={`Vehicle image ${selectedIndex + 1}`}
        className="w-full h-96 object-contain bg-gray-100"
      />

      {/* Thumbnail Navigation */}
      <div className="flex gap-2 mt-4">
        {images.map((url, index) => {
          const thumbUrl = url.replace('/upload/', '/upload/w_200,h_150,c_fill,f_auto,q_auto/');
          return (
            <button
              key={index}
              onClick={() => setSelectedIndex(index)}
              className={`border-2 ${index === selectedIndex ? 'border-blue-500' : 'border-gray-300'}`}
            >
              <img src={thumbUrl} alt={`Thumbnail ${index + 1}`} className="w-20 h-16 object-cover" />
            </button>
          );
        })}
      </div>
    </div>
  );
}
```

## What URLs to Store in Database

**Always store the full Cloudinary URL:**

```
https://res.cloudinary.com/your-cloud-name/image/upload/v1732012345/dealership-vehicles/abc123.jpg
```

**Do NOT store:**
- Relative paths (`/dealership-vehicles/abc123.jpg`) ❌
- Transformed URLs (`...w_400.../abc123.jpg`) ❌

**Why:** Store original URL, apply transformations on-the-fly in frontend via URL manipulation. This allows changing transformation logic without migrating database.

**Database Storage:**
- `vehicle.images`: JSONB array of full URLs
- `dealership.logo_url`: TEXT field with single full URL (or NULL)

## Free Tier Limits

**Cloudinary Free Tier:**
- **Storage:** 25GB
- **Bandwidth:** 25GB/month
- **Transformations:** Unlimited

**Estimation for 2-5 Dealerships:**
- Average 10 vehicles per dealership with 5 photos each = 250 images
- Average 2MB per image = 500MB storage
- Monthly traffic (~1000 views, 3 images/view) = ~6GB bandwidth

**Result:** Well within free tier. Upgrade to paid tier ($89/month for 85GB bandwidth) only if traffic grows significantly.

## Testing Checklist

- [ ] Create Cloudinary account and upload preset
- [ ] Add script tag to `index.html`
- [ ] Test widget opens in VehicleForm (desktop Chrome)
- [ ] Upload 3-5 sample images, verify URLs returned
- [ ] Save vehicle with images, verify stored in database
- [ ] View vehicle detail page, verify images display
- [ ] Test logo upload in DealerSettings
- [ ] Test on mobile Safari (iOS) - widget should work
- [ ] Test image removal (delete from state, not from Cloudinary)
- [ ] Verify transformed URLs (thumbnails) load correctly

**Estimated Integration Time:** 1 hour (setup + testing)

## Quick Setup Summary

1. **Create Cloudinary account** → Note Cloud Name
2. **Create upload preset** `vehicle-images` (unsigned)
3. **Add env vars** to `.env`: `VITE_CLOUDINARY_CLOUD_NAME`, `VITE_CLOUDINARY_UPLOAD_PRESET`
4. **Add script tag** to `frontend/index.html`
5. **Copy-paste widget code** into VehicleForm and DealerSettings
6. **Store returned URLs** in database (images array, logo_url)
7. **Display with transformations** (add `w_400,f_auto,q_auto` to URL)

**No backend code needed for MVP.** Widget handles everything. Backend `/api/upload` only if issues arise.

## SMTP Email Service Integration (v1.4+)

SMTP email service handles automated lead notifications to dealerships. Nodemailer library provides flexible SMTP transport supporting Gmail, SendGrid, AWS SES, Mailgun, and any SMTP provider. Email delivery is non-blocking and includes graceful degradation for development environments.

### Email Service Features

**Automatic Lead Notifications:**
- Triggered when customer submits enquiry via `/api/leads` endpoint
- Sends formatted email to `dealerships.email` address
- Includes customer details (name, email, phone, message)
- Shows vehicle information or "General Enquiry" label
- Professional HTML template with brand colors

**Non-blocking Architecture:**
- Email sent asynchronously after lead created in database
- Lead creation returns 201 success even if email fails
- Email errors logged but don't impact customer experience
- Essential for production reliability

**Graceful Degradation:**
- Skips email if EMAIL_* environment variables not configured
- Logs warning with lead data to console in development
- Allows development without SMTP setup
- Production deployments require email configuration

### SMTP Provider Options

**Option 1: Gmail (Easiest for Testing)**

Best for: Development, small-scale testing, personal projects

Setup steps:
1. Enable 2-factor authentication on Gmail account
2. Generate app-specific password: Google Account → Security → App Passwords
3. Use credentials:
   - `EMAIL_HOST=smtp.gmail.com`
   - `EMAIL_PORT=587`
   - `EMAIL_USER=your-email@gmail.com`
   - `EMAIL_PASSWORD=<16-character app password>`

Limitations:
- 500 emails/day limit
- May flag as spam if high volume
- Not recommended for production

**Option 2: SendGrid (Recommended for Production)**

Best for: Production deployments, scalability, deliverability

Setup steps:
1. Create free SendGrid account (100 emails/day free tier)
2. Verify sender email address or domain
3. Create API key: Settings → API Keys → Create API Key
4. Use credentials:
   - `EMAIL_HOST=smtp.sendgrid.net`
   - `EMAIL_PORT=587`
   - `EMAIL_USER=apikey` (literal string "apikey")
   - `EMAIL_PASSWORD=<your SendGrid API key>`

Advantages:
- Professional deliverability infrastructure
- Email analytics dashboard
- Webhook support for delivery tracking
- 100 emails/day free, scalable paid tiers

**Option 3: AWS SES (Enterprise Scale)**

Best for: High-volume production, AWS ecosystem integration

Setup steps:
1. Create AWS account, enable SES service
2. Verify sender email or domain
3. Request production access (SES starts in sandbox mode)
4. Generate SMTP credentials from SES console
5. Use provided SMTP endpoint and credentials

Advantages:
- $0.10 per 1,000 emails (cheapest at scale)
- Integrated with AWS infrastructure
- High deliverability reputation
- Requires AWS account complexity

### Environment Variables Configuration

Add to `.env` file (backend):

```bash
# Email Configuration (for lead notifications)
# Required for production, optional for development

# SMTP server settings
EMAIL_HOST=smtp.gmail.com              # SMTP hostname (varies by provider)
EMAIL_PORT=587                          # 587 for TLS, 465 for SSL
EMAIL_USER=your-email@gmail.com         # SMTP username (often email address)
EMAIL_PASSWORD=your-app-password        # SMTP password or API key
EMAIL_FROM=noreply@yourdomain.com       # Sender address (optional, defaults to EMAIL_USER)
```

**Security Note:** Never commit `.env` file to git. Use Railway/Render environment variables for production. Rotate credentials if exposed.

### Implementation Details

**Service Architecture:**

File: `backend/services/emailService.js`

Key functions:
- `createTransporter()` - Configures nodemailer SMTP transport from env vars
- `sendNewLeadNotification(dealershipEmail, leadData)` - Sends formatted notification

**Integration Point:**

File: `backend/routes/leads.js` (POST /api/leads endpoint)

Flow:
1. Validate and sanitize request body
2. Create lead record in PostgreSQL
3. Fetch dealership details (for email address)
4. Fetch vehicle details (if vehicle_id provided)
5. Call `sendNewLeadNotification()` in try-catch
6. Return 201 response (regardless of email outcome)

**Email Template:**

HTML format with:
- Blue-themed header (#3B82F6 matching platform design)
- Structured lead details display
- Customer contact information
- Vehicle information or "General Enquiry" label
- Professional formatting with inline CSS
- Plain text fallback for legacy email clients

### Testing Email Integration

**Development Testing (No SMTP Required):**

1. Start backend without EMAIL_* env vars
2. Submit test lead via frontend or Postman
3. Check console output for warning message
4. Verify lead created successfully despite missing email config

Expected console output:
```
Email configuration not set. Skipping email notification.
Lead data: { name: 'John Doe', email: 'john@example.com', ... }
```

**SMTP Testing (Gmail Example):**

1. Configure EMAIL_* env vars with Gmail app password
2. Restart backend server (env vars loaded on startup)
3. Submit test lead
4. Check Gmail inbox for notification email
5. Verify email contains correct lead details

Expected console output:
```
Lead notification email sent to: dealership@example.com
```

**Testing Checklist:**

- [ ] Email received in dealership inbox (not spam folder)
- [ ] Subject line shows customer name and vehicle/enquiry type
- [ ] HTML formatting displays correctly
- [ ] Customer contact details accurate
- [ ] Vehicle information matches lead (if applicable)
- [ ] Plain text version readable (test in text-only email client)
- [ ] Email delivery failure doesn't break lead creation

### Troubleshooting

**Issue: "Email configuration not set" warning**

Cause: EMAIL_HOST or EMAIL_USER environment variables missing

Solution:
- Verify `.env` file contains all EMAIL_* variables
- Restart backend server (nodemon auto-restarts, manual `node server.js` requires restart)
- Check for typos in variable names (must match exactly)

**Issue: "Invalid login" or "Authentication failed"**

Cause: Incorrect credentials or 2FA blocking

Solution (Gmail):
- Enable 2-factor authentication on Google account
- Generate new app-specific password (not regular account password)
- Use 16-character app password without spaces

Solution (SendGrid):
- Verify `EMAIL_USER=apikey` (literal string, not your username)
- Regenerate API key if compromised
- Check API key permissions include "Mail Send"

**Issue: Email sent but not received**

Cause: Spam filters, incorrect recipient address

Solution:
- Check spam/junk folder in dealership inbox
- Verify `dealerships.email` field in database is correct
- Test with personal email address first
- Use SendGrid for better deliverability (Gmail often flagged)
- Configure SPF/DKIM records for custom domain (advanced)

**Issue: "Connection timeout" or "ETIMEDOUT"**

Cause: Firewall blocking SMTP port, incorrect host/port

Solution:
- Verify EMAIL_PORT (587 for TLS, not 465 unless using SSL)
- Check Railway/Render allows outbound SMTP connections
- Test connection: `telnet smtp.gmail.com 587` (should connect)
- Try alternative port if 587 blocked (Gmail also supports 465 with SSL)

**Issue: Lead created but email error logged**

Cause: Expected behavior - non-blocking design

Solution:
- Check specific error message in console
- Lead creation succeeded (verify in database)
- Fix email configuration issue separately
- Customer experience unaffected

### Production Deployment

**Railway Configuration:**

1. Navigate to project → Variables tab
2. Add email environment variables:
   ```
   EMAIL_HOST=smtp.sendgrid.net
   EMAIL_PORT=587
   EMAIL_USER=apikey
   EMAIL_PASSWORD=<SendGrid API key>
   EMAIL_FROM=noreply@yourdomain.com
   ```
3. Deploy (Railway auto-deploys on config change)
4. Test with production URL

**Render Configuration:**

1. Navigate to service → Environment tab
2. Add email variables (same as Railway)
3. Save changes (triggers auto-deploy)
4. Test with production URL

**Security Best Practices:**

- Use environment variables (never hardcode credentials)
- Rotate SMTP passwords quarterly
- Monitor for unauthorized access (check provider dashboard)
- Use app-specific passwords (Gmail) or API keys (SendGrid)
- Enable 2FA on email service provider accounts
- Set up email delivery monitoring/alerts (SendGrid webhooks)

### Monitoring and Analytics

**Development Monitoring:**

Check backend logs for:
- "Lead notification email sent to: ..." (success)
- "Email notification failed: ..." (error with details)
- "Email configuration not set..." (skipped)

**Production Monitoring:**

SendGrid dashboard provides:
- Delivery rates (% successfully delivered)
- Bounce rates (invalid email addresses)
- Spam reports (flagged as spam)
- Click tracking (if enabled)

Recommended alerts:
- Delivery rate drops below 95%
- Bounce rate exceeds 5%
- Any spam reports

### Email Service Quick Setup Summary

1. **Choose SMTP provider**: Gmail (testing) or SendGrid (production)
2. **Generate credentials**: App password (Gmail) or API key (SendGrid)
3. **Add env vars** to `.env`:
   ```
   EMAIL_HOST=smtp.gmail.com (or smtp.sendgrid.net)
   EMAIL_PORT=587
   EMAIL_USER=your-email@gmail.com (or "apikey" for SendGrid)
   EMAIL_PASSWORD=<app password or API key>
   EMAIL_FROM=noreply@yourdomain.com
   ```
4. **Restart backend** to load env vars
5. **Test lead submission** via frontend or Postman
6. **Check dealership inbox** for notification email
7. **Deploy to Railway/Render** with production email configuration

**Optional for development.** Application works without email config. Required for production lead notifications.

---
