# Hero Text Customization Feature

## Overview
This feature adds the ability to customize the hero section title and subtitle text for each dealership from the CMS admin panel. Previously, the hero section displayed the dealership name and either the "About" text or a hardcoded default message. Now, dealership administrators can set custom text for both the title and subtitle.

## What Changed

### Backend (.NET Core)

#### 1. Database Schema
**File**: `backend-dotnet/hero_text_migration.sql`
- Added `hero_title` column (TEXT, nullable)
- Added `hero_subtitle` column (TEXT, nullable)

**Migration SQL**:
```sql
ALTER TABLE dealership ADD COLUMN IF NOT EXISTS hero_title TEXT;
ALTER TABLE dealership ADD COLUMN IF NOT EXISTS hero_subtitle TEXT;
```

#### 2. Domain Entity
**File**: `JealPrototype.Domain/Entities/Dealership.cs`
- Added `HeroTitle` property (string?, nullable)
- Added `HeroSubtitle` property (string?, nullable)
- Updated `UpdateHeroSettings()` method to accept `heroTitle` and `heroSubtitle` parameters

#### 3. DTOs
**Files Modified**:
- `JealPrototype.Application/DTOs/Dealership/UpdateDealershipDto.cs`
  - Added `HeroTitle` property
  - Added `HeroSubtitle` property
  
- `JealPrototype.Application/DTOs/Dealership/DealershipResponseDto.cs`
  - Added `HeroTitle` property
  - Added `HeroSubtitle` property

#### 4. Database Configuration
**File**: `JealPrototype.Infrastructure/Persistence/Configurations/DealershipConfiguration.cs`
- Configured `HeroTitle` to map to `hero_title` column
- Configured `HeroSubtitle` to map to `hero_subtitle` column

#### 5. Use Case
**File**: `JealPrototype.Application/UseCases/Dealership/UpdateDealershipUseCase.cs`
- Updated hero settings update logic to pass `request.HeroTitle` and `request.HeroSubtitle` to `UpdateHeroSettings()`

### Frontend (React)

#### 1. Admin Settings Page
**File**: `frontend/src/pages/admin/DealerSettings.jsx`

**State Management**:
- Added `heroTitle` state variable
- Added `heroSubtitle` state variable

**Data Fetching**:
- Loads `heroTitle` and `heroSubtitle` from API response
- Sets default empty strings if not present

**Form Submission**:
- Includes `heroTitle` and `heroSubtitle` in the PUT request payload
- Sends `null` if fields are empty

**UI Components Added**:
```jsx
<div className="border-t pt-4 mt-4">
  <h2>Hero Text Content</h2>
  
  {/* Hero Title Input */}
  <input
    type="text"
    value={heroTitle}
    onChange={(e) => setHeroTitle(e.target.value)}
    placeholder="e.g., Hot Spot Autos"
    maxLength={100}
  />
  
  {/* Hero Subtitle Textarea */}
  <textarea
    value={heroSubtitle}
    onChange={(e) => setHeroSubtitle(e.target.value)}
    placeholder="e.g., Quality vehicles at great prices..."
    maxLength={300}
  />
</div>
```

#### 2. Public Homepage
**File**: `frontend/src/pages/public/Home.jsx`

**Updated all three hero types** (carousel, video, static image):

**Title Logic**:
```jsx
{dealership?.heroTitle && (
  <h1>
    {dealership.heroTitle}
  </h1>
)}
```

**Subtitle Logic**:
```jsx
{dealership?.heroSubtitle && (
  <p>
    {dealership.heroSubtitle}
  </p>
)}
```

## Fallback Behavior

### Title
- If `heroTitle` is set → display custom title
- If `heroTitle` is blank/null → **hide title (show nothing)**

### Subtitle
- If `heroSubtitle` is set → display custom subtitle
- If `heroSubtitle` is blank/null → **hide subtitle (show nothing)**

**Note**: When both fields are blank, only the "Browse Inventory" button will be visible on the hero section.

## How to Use

### For Dealership Administrators

1. **Navigate to Settings**:
   - Log in to the admin panel
   - Go to "Dealership Settings"

2. **Find Hero Text Content Section**:
   - Scroll down to the "Hero Text Content" section (below hero media settings)

3. **Customize the Text**:
   - **Hero Title**: Enter custom title (max 100 characters)
     - Leave blank to hide the title completely
     - Example: "Hot Spot Autos"
   
   - **Hero Subtitle**: Enter custom tagline (max 300 characters)
     - Leave blank to hide the subtitle completely
     - Example: "Quality vehicles at great prices. Browse our inventory to find your next car."

4. **Save Changes**:
   - Click "Save Settings" at the bottom of the page
   - Changes appear immediately on the homepage

### For Developers

#### Running the Migration

**Database Migration**:
```bash
# Connect to your PostgreSQL database
psql -U your_username -d your_database

# Run the migration script
\i backend-dotnet/hero_text_migration.sql
```

Or use your preferred database migration tool (e.g., Entity Framework migrations, Flyway, etc.)

#### API Endpoints

**GET /api/dealerships/{id}**
Response includes:
```json
{
  "data": {
    "id": 1,
    "name": "Acme Motors",
    "heroTitle": "Hot Spot Autos",
    "heroSubtitle": "Your trusted partner for quality vehicles",
    ...
  }
}
```

**PUT /api/dealerships/{id}**
Request body:
```json
{
  "heroTitle": "Hot Spot Autos",
  "heroSubtitle": "Quality vehicles at great prices. Browse our inventory to find your next car."
}
```

## Files Modified

### Backend (.NET)
1. `JealPrototype.Domain/Entities/Dealership.cs`
2. `JealPrototype.Application/DTOs/Dealership/UpdateDealershipDto.cs`
3. `JealPrototype.Application/DTOs/Dealership/DealershipResponseDto.cs`
4. `JealPrototype.Infrastructure/Persistence/Configurations/DealershipConfiguration.cs`
5. `JealPrototype.Application/UseCases/Dealership/UpdateDealershipUseCase.cs`

### Frontend (React)
1. `frontend/src/pages/admin/DealerSettings.jsx`
2. `frontend/src/pages/public/Home.jsx`

### Database
1. `backend-dotnet/hero_text_migration.sql` (new file)

## Testing

### Manual Testing Steps

1. **Run the migration**:
   ```bash
   psql -U postgres -d dealership_db -f backend-dotnet/hero_text_migration.sql
   ```

2. **Start the backend**:
   ```bash
   cd backend-dotnet
   dotnet run --project JealPrototype.API
   ```

3. **Start the frontend**:
   ```bash
   cd frontend
   npm run dev
   ```

4. **Test the feature**:
   - Log in as admin
   - Go to Dealership Settings
   - Scroll to "Hero Text Content"
   - Enter custom title: "Hot Spot Autos"
   - Enter custom subtitle: "Quality vehicles at competitive prices"
   - Save settings
   - Navigate to the homepage (public view)
   - Verify custom text appears in the hero section

5. **Test fallback behavior**:
   - Clear the custom title and subtitle fields
   - Save settings
   - Verify homepage shows no title or subtitle (only "Browse Inventory" button visible)

## Notes

- Both fields are **optional** (nullable in database)
- Character limits: Title (100 chars), Subtitle (300 chars)
- Empty values are stored as `NULL` in the database
- The feature works with all three hero types: image, video, and carousel
- No breaking changes - existing dealerships will continue to show name + about text by default

## Future Enhancements

Possible improvements for future versions:
- Rich text editor for subtitle formatting
- Preview mode to see changes before saving
- Multiple subtitle variations for A/B testing
- Localization support for multi-language dealerships
- Character counter UI feedback
