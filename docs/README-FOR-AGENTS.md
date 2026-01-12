# README for Development Agents

**Last Updated:** 2025-12-31
**Purpose:** Quick reference guide for AI agents implementing stories

---

## 📋 Recent Changes (What's New)

### 2025-12-31: Google Reviews Carousel Feature

**What Changed:**
- Implemented Google Reviews carousel on dealership homepages
- Displays 3-4 top-rated customer reviews from Google Maps
- Interactive carousel with navigation arrows and pagination dots
- "Read More Reviews" button linking to Google Maps
- Fully responsive design (mobile/tablet/desktop)

**Feature Overview:**
- **Location:** Homepage - Below "Find Your Perfect Vehicle" widget and "General Enquiry" form
- **Data Source:** Google Places API
- **Display:** Carousel showing reviewer name, photo, star rating, review text, and date
- **Filtering:** Only 4+ star reviews shown, sorted by rating
- **Integration:** Uses existing dealership address to auto-find Google Business listing

**Files Created:**
- `frontend/src/components/GoogleReviewsCarousel.jsx` - Main carousel component
- `backend/routes/googleReviews.js` - API route handler
- `test_google_reviews.js` - Test script
- 6 documentation files (see below)

**Files Modified:**
- `frontend/src/pages/public/Home.jsx` - Added carousel to homepage
- `backend/server.js` - Registered new API route
- `.env.example` - Added Google API key configuration

**Configuration Required:**
```env
GOOGLE_PLACES_API_KEY=your_api_key_here
```

**Key Features:**
- ✅ Interactive carousel with 3-4 reviews per view
- ✅ Navigation arrows and pagination dots
- ✅ Star rating display (1-5 stars)
- ✅ Reviewer profile photos and names
- ✅ "Read More" button to Google Maps
- ✅ Responsive design (mobile/tablet/desktop)
- ✅ Graceful error handling (silent failure)
- ✅ Theme color integration
- ✅ No database schema changes required

**Documentation Created:**
- `docs/AGENT_BRIEFING_GOOGLE_REVIEWS.md` - Complete agent briefing
- `docs/CHANGELOG-GOOGLE-REVIEWS-2025-12-31.md` - Detailed changelog
- `GOOGLE_REVIEWS_README.md` - Main implementation summary
- `GOOGLE_REVIEWS_DOCS_INDEX.md` - Documentation navigation
- `GOOGLE_REVIEWS_QUICK_START.md` - 5-minute setup guide
- `GOOGLE_REVIEWS_FEATURE.md` - Complete feature documentation
- `GOOGLE_REVIEWS_IMPLEMENTATION_SUMMARY.md` - Technical details
- `GOOGLE_REVIEWS_VISUAL_GUIDE.md` - Design specifications

**Quick Links:**
- **Agent Briefing:** `docs/AGENT_BRIEFING_GOOGLE_REVIEWS.md` ⭐ START HERE
- **Changelog:** `docs/CHANGELOG-GOOGLE-REVIEWS-2025-12-31.md`
- **Quick Start:** `GOOGLE_REVIEWS_QUICK_START.md`
- **Full Docs:** `GOOGLE_REVIEWS_DOCS_INDEX.md`
- **Test Script:** `test_google_reviews.js`

**API Endpoint:**
```
GET /api/google-reviews/:dealershipId
```

**Testing:**
```bash
node test_google_reviews.js
```

**Impact:**
- ✅ Builds customer trust with social proof
- ✅ Enhances homepage engagement
- ✅ Zero breaking changes
- ✅ Production ready (pending API key)
- ⚠️ Requires Google API key (costs apply after free tier)
- 💡 Future: Database caching recommended

**For Agents:**
- **PM:** See business value and future enhancements in agent briefing
- **Architect:** Review API integration and architecture in implementation summary
- **SM:** User story template provided in agent briefing
- **Dev:** Code is well-documented with inline comments
- **QA:** Test cases and manual testing steps in changelog

---

### 2025-12-31: AdminHeader Center Alignment Fix

**What Changed:**
- Fixed AdminHeader layout to center-align entire header section
- Removed large space between "Admin Panel" title and left margin
- Removed excessive right-alignment pushing "Log Out" button to edge
- Improved visual balance and spacing consistency

**Problem:**
- Large left margin on "Admin Panel" title
- "Log Out" button pushed to far right, nearly touching edge
- Header content not center-aligned as intended
- Inconsistent spacing between elements

**Solution:**
- Added `lg:justify-center` to main flex container
- Removed `lg:mx-8` from dealership selector (excessive 32px margins)
- Removed `flex-1 justify-end` from navigation (right-alignment)

**Files Changed:**
- `frontend/src/components/AdminHeader.jsx` - Three CSS class changes (lines 137, 142, 166)

**Impact:**
- ✅ Entire header section now center-aligned
- ✅ Equal spacing between all header elements
- ✅ Professional, balanced appearance
- ✅ Responsive behavior maintained (mobile stacks vertically)
- ✅ No breaking changes - fully backwards compatible
- ✅ No backend or API changes required

**Documentation Created:**
- `docs/CHANGELOG-ADMIN-HEADER-CENTER-ALIGNMENT-2025-12-31.md` - Complete fix details

**Quick Links:**
- Changelog: `docs/CHANGELOG-ADMIN-HEADER-CENTER-ALIGNMENT-2025-12-31.md`
- Component: `frontend/src/components/AdminHeader.jsx`
- Architecture: `docs/architecture/components.md`
- Bug Fixes Log: `docs/BUG_FIXES.md` (BUG-003)

---

### 2025-12-31: Header Layout Center Alignment Fix

**What Changed:**
- Fixed dealership name appearing vertically (each word stacked) in public header
- Changed header layout from left-aligned to center-aligned
- Improved responsive behavior for mobile and desktop views

**Problem:**
- Dealership name "Acme Auto Sales" was displaying vertically with each word on a separate line
- Header used `justify-between` which spread elements apart instead of centering
- Unprofessional appearance and poor user experience

**Solution:**
- Added `whitespace-nowrap` to h1 element to prevent text wrapping
- Changed header container from `justify-between` to `justify-center`
- Made layout responsive with `flex-col md:flex-row` for proper mobile/desktop handling
- Positioned mobile hamburger menu absolutely (top-right) while keeping branding centered

**Modified Files:**
- `frontend/src/components/Header.jsx` - Updated layout and text wrapping (Lines 72, 82, 103)

**Key Features:**
- ✅ Dealership name displays horizontally on single line
- ✅ All header elements center-aligned (branding + navigation)
- ✅ Responsive layout (stacked mobile, horizontal desktop)
- ✅ Mobile hamburger menu properly positioned
- ✅ No breaking changes - fully backwards compatible
- ✅ No backend or API changes required

**Documentation Created:**
- `docs/CHANGELOG-HEADER-LAYOUT-FIX-2025-12-31.md` - Complete fix details

**Quick Links:**
- Changelog: `docs/CHANGELOG-HEADER-LAYOUT-FIX-2025-12-31.md`
- Component: `frontend/src/components/Header.jsx`
- Related Story: `docs/stories/5.3.navigation-public-header.md`
- Bug Fixes Log: `docs/BUG_FIXES.md` (BUG-002)

---

### 2025-12-10: Lead Message Display Enhancement - Show More/Less Toggle

**What Changed:**
- Enhanced Lead Inbox message column with expandable/collapsible functionality
- Messages now truncate to 100 characters by default with "..." indicator
- Added "Show more" link to expand full message content on demand
- Added "Show less" link to collapse back to truncated view
- Improved space efficiency and scanning capability in Lead Inbox

**Problem:**
- Lead messages were displaying in full regardless of length
- Very long messages caused excessive vertical scrolling
- Difficult to quickly scan through multiple leads
- Inconsistent table row heights reduced usability

**Solution:**
- Implemented show more/show less toggle system for message display
- Messages initially show truncated (100 char limit with "...")
- Click "Show more" to reveal full message, changes to "Show less"
- Click "Show less" to collapse back to truncated view
- Only one message expanded at a time for cleaner layout

**Modified Files:**
- `frontend/src/pages/admin/LeadInbox.jsx` - Added expandable message functionality
  - Added `expandedLeadId` state to track expanded message
  - Restored `truncateMessage()` helper (100 char limit)
  - Restored `toggleExpand()` click handler
  - Updated message cell with conditional rendering
  - Added "Show more"/"Show less" button

**Key Features:**
- ✅ Smart truncation (only messages > 100 chars)
- ✅ One-click expand/collapse toggle
- ✅ Consistent compact row heights
- ✅ Only one message expanded at a time
- ✅ Proper HTML entity decoding in both states
- ✅ No backend changes required
- ✅ Fully backward compatible

**Documentation Created:**
- `docs/CHANGELOG-LEAD-MESSAGE-DISPLAY-2025-12-10.md` - Complete implementation details

**Quick Links:**
- Changelog: `docs/CHANGELOG-LEAD-MESSAGE-DISPLAY-2025-12-10.md`
- Component: `frontend/src/pages/admin/LeadInbox.jsx`
- Related Story: `docs/stories/3.5.story.md` (Lead Inbox)

---

### 2025-12-09: General Enquiry Form - Homepage Lead Capture

**What Changed:**
- Added general enquiry form to homepage positioned next to vehicle search widget
- Created responsive grid layout (side-by-side on desktop, stacked on mobile)
- Full form validation with inline errors and character counter
- No backend or database changes required - uses existing infrastructure

**Problem:**
- Website visitors couldn't contact dealership without selecting a specific vehicle
- No easy way for customers to ask general questions about services, financing, etc.
- Missing lead capture opportunity for customers in early research phase

**Solution:**
- New `GeneralEnquiryForm.jsx` component with 4 required fields (name, email, phone, message)
- Positioned in responsive grid: SearchWidget (left) | GeneralEnquiryForm (right)
- Validates email format, phone (10+ digits), message length (5000 max)
- Submits to existing `/api/leads` endpoint with `vehicle_id = null`
- Appears in admin Lead Inbox with "General Enquiry" label

**New Files Created:**
- `frontend/src/components/GeneralEnquiryForm.jsx` - Main form component (298 lines)
- `docs/stories/6.1.general-enquiry-form.md` - Complete user story (382 lines)
- `docs/GENERAL_ENQUIRY_FORM.md` - Feature documentation (146 lines)
- `docs/GENERAL_ENQUIRY_FORM_INDEX.md` - Quick reference guide (280 lines)
- `docs/CHANGELOG-GENERAL-ENQUIRY-FORM-2025-12-09.md` - Implementation summary (380 lines)
- `test_general_enquiry.js` - Automated API test (46 lines)

**Modified Files:**
- `frontend/src/pages/public/Home.jsx` - Added grid layout for side-by-side widgets
- `frontend/src/components/SearchWidget.jsx` - Adjusted styling for grid layout
- `docs/prd.md` - Updated to v1.3, added FR24
- `docs/architecture.md` - Updated to v1.1, added component documentation

**Key Features:**
- ✅ Real-time validation with error clearing
- ✅ Character counter (5000 max for message)
- ✅ Success message with auto-hide (5 seconds)
- ✅ Form reset after submission
- ✅ Loading states during submission
- ✅ Responsive design (lg breakpoint at 1024px)
- ✅ Integration with existing Lead Inbox
- ✅ XSS prevention (backend sanitization)

**Documentation Updated:**
- PRD: Added FR24 requirement
- Architecture: Added component specifications
- Story 6.1: Complete implementation guide
- Three comprehensive documentation files created
- Testing instructions and automated test included

**Quick Links:**
- Story: `docs/stories/6.1.general-enquiry-form.md`
- Index: `docs/GENERAL_ENQUIRY_FORM_INDEX.md`
- PRD: `docs/prd.md` (v1.3, FR24)
- Architecture: `docs/architecture.md` (v1.1)

---

### 2025-12-08: Footer Layout Optimization - Social Media Icons Repositioned

**What Changed:**
- Moved social media icons from separate section to "Opening Hours" column
- Improved space efficiency and visual consistency
- Added "Follow Us" heading above social media icons

**Problem:**
- Large empty space under Opening Hours column
- Separate social media section took up additional vertical space
- Inconsistent content density across footer columns

**Solution:**
- Integrated social media icons into Opening Hours column
- Left-aligned icons with "Follow Us" heading
- Removed standalone social media section below grid
- Better space utilization and cleaner design

**Modified Files:**
- `frontend/src/components/Footer.jsx` - Moved social media section into Column 2

**New Footer Layout:**
```
Footer (3 columns on desktop):
├── Column 1: Contact Information
│   ├── Dealership name
│   ├── Address
│   ├── Phone (clickable)
│   └── Email (clickable)
├── Column 2: Opening Hours & Social Media
│   ├── Opening hours text
│   └── Follow Us section
│       ├── Facebook icon (if URL set)
│       └── Instagram icon (if URL set)
└── Column 3: Quick Links
    └── Navigation items (excludes admin/login)
```

**Visual Changes:**
- Social media icons now appear directly under opening hours
- Icons are left-aligned with gap-4 spacing (was centered with gap-6)
- Added "Follow Us" subheading in Opening Hours column
- Removed separate bordered section for social media
- More compact footer with better vertical space usage

**Benefits:**
- Eliminated wasted space in Opening Hours column
- More consistent visual balance across all columns
- Cleaner, more professional appearance
- Still fully responsive on mobile devices

**Documentation Updated:**
- `docs/README-FOR-AGENTS.md` (this section)
- `docs/stories/5.4.footer-enhancement.md` (updated)
- `docs/CHANGELOG-FOOTER-FEATURE-2025-12-08.md` (updated)

---

### 2025-12-08: Enhanced Footer with Social Media Integration (Story 5.4)

**What Changed:**
- Replaced basic footer with comprehensive Footer component
- Added social media URL management in CMS admin
- Added database columns for Facebook and Instagram URLs
- Footer displays contact info, hours, navigation links, and social media icons

**Problem:**
- Footer only showed basic copyright text
- No way to display dealership contact information in footer
- No social media integration
- Footer didn't match header theme color

**Solution:**
- Created new Footer component with full dealership information
- Added "Social Media Links" section in Dealership Settings
- Database migration added facebook_url and instagram_url columns
- Footer uses theme color and responsive design

**Modified Files:**
- `frontend/src/components/Footer.jsx` - NEW: Comprehensive footer component
- `frontend/src/components/Layout.jsx` - Replaced basic footer with Footer component
- `frontend/src/pages/admin/DealerSettings.jsx` - Added social media URL inputs
- `backend/db/dealers.js` - Added social media field handling
- `backend/routes/dealers.js` - Added social media URL support
- `backend/db/migrations/005_add_social_media_fields.sql` - NEW: Database migration

**Footer Features:**
```
Footer Layout (3 columns on desktop):
├── Contact Information
│   ├── Dealership name
│   ├── Address (full text)
│   ├── Phone (clickable tel: link)
│   └── Email (clickable mailto: link)
├── Opening Hours
│   └── Hours text (multi-line support)
└── Quick Links
    └── Navigation items (excludes admin/login)

Social Media Section (conditional):
├── Facebook icon + link (opens in new tab)
└── Instagram icon + link (opens in new tab)

Copyright:
└── "© 2025 Dealership Name. All rights reserved."
```

**Key Highlights:**
- Background color matches dealership theme
- Responsive design (3-column → stacked on mobile)
- Social media icons only show when URLs are configured
- Graceful handling of missing information
- Uses same navigation config as header for consistency

**Documentation:** 
- `docs/stories/5.4.footer-enhancement.md` (complete user story)
- `docs/CHANGELOG-FOOTER-FEATURE-2025-12-08.md` (comprehensive changelog)
- `docs/architecture/database-schema.md` (updated schema)
- `docs/architecture/components.md` (updated component docs)

**For Development Agents:**
When working with footer-related features:
- Footer component uses `useDealership()` hook (same as Header)
- Social media URLs stored in `facebook_url` and `instagram_url` columns
- Footer filters navigation to exclude admin/login links
- Footer respects dealership theme color and font family

---

### 2025-12-01: Navigation Settings Layout Improvements (UI Enhancement)

**What Changed:**
- Improved layout of Navigation Settings section in Dealership Settings page
- Split DealerSettings into two containers with different widths
- Reorganized NavigationManager with optimized preview layout

**Problem:**
- Desktop preview was truncating on smaller screens
- Limited workspace for editing navigation items
- Preview sections competing for horizontal space

**Solution:**
- Basic settings use narrower `max-w-3xl` container (focused layout)
- Navigation Manager uses wider `max-w-7xl` container (more workspace)
- Desktop preview moved to full-width top section
- Navigation items list and mobile preview placed side-by-side below

**Modified Files:**
- `frontend/src/pages/admin/DealerSettings.jsx` - Split into two separate containers
- `frontend/src/components/admin/NavigationManager.jsx` - Reorganized layout structure

**New Layout Structure:**
```
DealerSettings Page:
├── Basic Settings (max-w-3xl)
│   ├── Theme Color
│   ├── Font Family
│   ├── Logo Upload
│   └── Contact Info, etc.
│
└── Navigation Manager (max-w-7xl)
    ├── Desktop Preview (full-width top)
    └── Bottom Grid (2 columns)
        ├── Left: Navigation Items List (drag-and-drop)
        └── Right: Mobile Preview
```

**Benefits:**
- No more horizontal truncation on desktop preview
- Better use of screen space for complex editing
- Side-by-side view of items and mobile preview
- Clearer visual separation of settings categories

**Documentation:** 
- `docs/CHANGELOG-NAVIGATION-LAYOUT-2025-12-01.md` (comprehensive changelog)
- `docs/stories/5.2.navigation-admin-cms.md` (updated story)
- `docs/architecture-navigation-enhancement.md` (updated architecture)

---

### 2025-11-28: Vehicle Photo Upload Fix (Bug Fix)

**What Changed:**
- Fixed critical page freeze bug in Vehicle Manager photo upload
- Replaced Cloudinary widget with file input + `/api/upload` approach
- Now matches DealerSettings hero background upload implementation

**Problem:**
- Clicking "Upload Photos" in Vehicle Manager caused page to freeze
- Users unable to upload vehicle photos (critical feature blocker)

**Solution:**
- Removed Cloudinary widget dependency from VehicleForm
- Implemented native file input with FormData POST to `/api/upload`
- Added comprehensive client-side validation (type, size, count)
- Added upload progress and error feedback

**Modified File:**
- `frontend/src/pages/admin/VehicleForm.jsx` - Complete upload implementation rewrite

**How It Works Now:**
```javascript
// User clicks "Upload Photos" label (wrapping hidden file input)
<label className="btn-primary cursor-pointer">
  Upload Photos
  <input
    type="file"
    accept="image/jpeg,image/png,image/webp"
    multiple
    onChange={handlePhotoUpload}  // Posts to /api/upload
    className="hidden"
  />
</label>

// Each selected file uploads to /api/upload
for (const file of files) {
  const formData = new FormData();
  formData.append('image', file);
  const response = await fetch('/api/upload', {
    method: 'POST',
    body: formData
  });
  // Returns { url: "https://res.cloudinary.com/..." }
}
```

**Architecture Note:**
- **Vehicle Photos:** File input + `/api/upload` (reliable, supports multiple files)
- **Hero Background:** File input + `/api/upload` (reliable, large files)
- **Logo Upload:** Cloudinary widget (uses built-in cropping, works reliably)

**Documentation:** 
- `docs/CHANGELOG-VEHICLE-PHOTO-UPLOAD-FIX-2025-11-28.md` (comprehensive)
- `docs/architecture/external-apis-cloudinary-integration.md` (updated)

---

### 2025-11-24: Admin "View Website" Button

**What Changed:**
- Added "View Website" button to AdminHeader navigation
- Button allows admins to navigate from admin panel to public website
- Automatically syncs public site dealership with admin selection

**Modified File:**
- `frontend/src/components/AdminHeader.jsx` - Added view website functionality

**How It Works:**
```javascript
// When admin clicks "View Website"
const handleViewWebsite = () => {
  if (selectedDealership) {
    setCurrentDealershipId(selectedDealership.id); // Sync contexts
    navigate('/'); // Navigate to public home
  }
};
```

**User Experience:**
- Admin selects dealership in admin panel
- Clicks "View Website" button
- Browser navigates to public site showing selected dealership

**Documentation:** `docs/ADMIN_VIEW_WEBSITE_FEATURE.md`

---

### 2025-11-24: Public Website Dealership Selector (Story 3.6.1)

**What Changed:**
- Added global `DealershipContext` for managing dealership selection on public website
- Created `DealershipSelector` component with dropdown for choosing dealership
- Removed hardcoded `dealershipId = 1` from all public pages
- All public pages now use `useDealershipContext()` hook

**New Files:**
- `frontend/src/context/DealershipContext.jsx` - Global dealership selection state
- `frontend/src/components/DealershipSelector.jsx` - Dropdown component
- `docs/stories/3.6.1.story.md` - Complete implementation documentation

**Modified Components:**
- `frontend/src/App.jsx` - Added DealershipProvider wrapper
- `frontend/src/components/Header.jsx` - Integrated selector, uses context
- `frontend/src/pages/public/Home.jsx` - Uses context instead of hardcoded ID
- `frontend/src/pages/public/Inventory.jsx` - Uses context instead of hardcoded ID
- `frontend/src/pages/public/About.jsx` - Uses context instead of hardcoded ID
- `frontend/src/pages/public/VehicleDetail.jsx` - Uses context instead of hardcoded ID

**Usage Pattern:**
```javascript
import { useDealershipContext } from '../../context/DealershipContext';

function MyComponent() {
  const { currentDealershipId } = useDealershipContext();
  // Use currentDealershipId for API calls
}
```

**Key Features:**
- Selection persists across sessions via localStorage
- Defaults to dealership ID 1 on first visit
- Selector only shows when 2+ dealerships exist
- Mobile-responsive design

**For Future Stories:** When implementing public pages that need dealership context, always use `useDealershipContext()` instead of hardcoding dealership IDs.

---

## ⚠️ CRITICAL: Read This Before Implementing Any Story

### **Mandatory Pre-Implementation Checklist**

Before writing any code, complete these steps:

#### 1. ✅ **Review Backend Routes Status** (MANDATORY)

**File:** [`docs/BACKEND_ROUTES_IMPLEMENTATION_STATUS.md`](./BACKEND_ROUTES_IMPLEMENTATION_STATUS.md)

**Why:** Avoid duplicating existing backend API endpoints

**What to Check:**
- Does the route you need already exist?
- Which stories created/use this route?
- What security requirements does it implement?
- What database modules does it use?

**This file is automatically loaded** via `devLoadAlwaysFiles` in `.bmad-core/core-config.yaml`

#### 2. ✅ **Review Always-Loaded Architecture Files**

These files are automatically loaded for every dev agent:

1. **`docs/architecture/coding-standards.md`**
   - JSDoc documentation requirements
   - Security requirements (SEC-001, XSS, SQL injection)
   - File header comment format

2. **`docs/architecture/tech-stack.md`**
   - Technology decisions (React, Express, PostgreSQL, Tailwind)
   - Why certain technologies were chosen over alternatives
   - Manual testing only for MVP (no Jest/RTL)

3. **`docs/architecture/source-tree.md`**
   - Complete project file structure
   - Where to create new files
   - Backend and frontend organization

4. **`docs/architecture/security-guidelines.md`**
   - Multi-tenancy enforcement (SEC-001)
   - XSS prevention (sanitizeInput function)
   - SQL injection prevention (parameterized queries)
   - Input validation patterns

5. **`docs/BACKEND_ROUTES_IMPLEMENTATION_STATUS.md`**
   - Complete backend route inventory
   - Story dependencies for each route
   - When to create new routes vs. use existing

#### 3. ✅ **Review Story-Specific Documentation**

Check the story file's **Dev Notes** section for:
- Previous story insights
- API specifications for required endpoints
- Component architecture patterns
- Tailwind CSS styling examples
- Testing requirements

---

## 🚫 Common Mistakes to Avoid

### **Mistake #1: Creating Duplicate Backend Routes**

**❌ WRONG:**
```markdown
Story: Implement Vehicle Manager

Tasks:
- [ ] Create backend/routes/vehicles.js with CRUD endpoints
- [ ] Create backend/db/vehicles.js with database queries
- [ ] Mount routes in backend/server.js
```

**✅ CORRECT:**
```markdown
Story: Implement Vehicle Manager

Pre-Implementation Check:
- ✅ Reviewed BACKEND_ROUTES_IMPLEMENTATION_STATUS.md
- ✅ Found vehicles.js already exists with all CRUD endpoints
- ✅ Verified multi-tenancy security (SEC-001) already implemented
- ✅ Confirmed database module vehicles.js exists

Tasks:
- [ ] Create frontend/src/pages/admin/VehicleManager.jsx
- [ ] Use existing GET /api/vehicles?dealershipId=X endpoint
- [ ] Use existing POST /api/vehicles endpoint
- [ ] Use existing PUT /api/vehicles/:id endpoint
- [ ] Use existing DELETE /api/vehicles/:id endpoint
```

### **Mistake #2: Ignoring Multi-Tenancy Requirements**

**❌ WRONG:**
```javascript
// Missing dealershipId parameter
fetch('/api/vehicles')
```

**✅ CORRECT:**
```javascript
// Include dealershipId for multi-tenancy isolation (SEC-001)
fetch(`/api/vehicles?dealershipId=${selectedDealership.id}`)
```

### **Mistake #3: Not Including Session Credentials**

**❌ WRONG:**
```javascript
fetch('/api/vehicles')
```

**✅ CORRECT:**
```javascript
fetch('/api/vehicles', {
  credentials: 'include'  // Required for session cookies
})
```

### **Mistake #4: Creating Automated Tests**

**❌ WRONG:**
```markdown
Tasks:
- [ ] Write Jest unit tests for component
- [ ] Write React Testing Library integration tests
```

**✅ CORRECT:**
```markdown
Tasks:
- [ ] Manual testing per tech-stack.md (no Jest/RTL for MVP)
- [ ] Test on Chrome browser with DevTools
- [ ] Verify responsive layout on mobile (375px) and tablet (768px)
```

---

## 📋 Quick Reference: Backend API Inventory

### **Authentication Routes** (backend/routes/auth.js)
- POST /api/auth/login - Admin login
- POST /api/auth/logout - Admin logout
- GET /api/auth/me - Check auth status

### **Dealership Routes** (backend/routes/dealers.js)
- GET /api/dealers - List all dealerships
- GET /api/dealers/:id - Get single dealership
- PUT /api/dealers/:id - Update dealership (auth required)

### **Vehicle Routes** (backend/routes/vehicles.js)
- GET /api/vehicles?dealershipId=X - List vehicles (multi-tenant)
- GET /api/vehicles/:id?dealershipId=X - Get single vehicle (multi-tenant)
- POST /api/vehicles - Create vehicle (auth required)
- PUT /api/vehicles/:id?dealershipId=X - Update vehicle (auth required)
- DELETE /api/vehicles/:id?dealershipId=X - Delete vehicle (auth required)

### **Lead Routes** (backend/routes/leads.js)
- GET /api/leads?dealershipId=X - List leads (auth required, multi-tenant)
- POST /api/leads - Submit enquiry (public)

### **Upload Routes** (backend/routes/upload.js)
- POST /api/upload - Upload image to Cloudinary (auth required)

**For complete details:** See `docs/architecture/api-specification.md`

---

## 🔒 Security Requirements (SEC-001)

### **Multi-Tenancy Enforcement**

All tenant-scoped endpoints MUST include dealershipId:

```javascript
// Frontend API call
const response = await fetch(
  `/api/vehicles?dealershipId=${selectedDealership.id}`,
  { credentials: 'include' }
);

// Backend route validation
const dealershipIdNum = parseInt(dealershipId, 10);
if (isNaN(dealershipIdNum) || dealershipIdNum <= 0) {
  return res.status(400).json({
    error: 'dealershipId must be a valid positive number'
  });
}

// Database query with isolation
const results = await db.query(
  'SELECT * FROM vehicles WHERE dealership_id = $1',
  [dealershipIdNum]
);
```

**See:** `docs/architecture/security-guidelines.md` for complete patterns

---

## 🎨 Frontend Standards

### **React Patterns**
- Context API for global state (auth, selectedDealership)
- Hooks: useContext, useState, useEffect, custom hooks
- No Redux/Zustand (per tech-stack.md)

### **Styling**
- Tailwind CSS utility classes
- Responsive breakpoints: md:, lg:
- Utility classes: .btn-primary, .btn-secondary, .input-field, .card

### **Routing**
- React Router 6.20+
- Nested routes for admin panel
- ProtectedRoute wrapper for auth-required pages

---

## 📝 Documentation Standards

### **JSDoc Comments Required**

```javascript
/**
 * @fileoverview Component description and purpose.
 * Additional context about functionality.
 */

/**
 * ComponentName - Brief description.
 *
 * @component
 * @param {Object} props
 * @param {string} props.title - Prop description
 *
 * @example
 * <ComponentName title="Example" />
 */
```

### **Backend Route Header**

```javascript
/**
 * @fileoverview Route description.
 *
 * SECURITY (SEC-001): Multi-tenancy enforcement details.
 * SECURITY (XSS): Input sanitization details.
 *
 * Routes:
 * - GET    /api/resource        - Description
 * - POST   /api/resource        - Description
 */
```

---

## ✅ Story Completion Checklist

Before marking a story as "Ready for Review":

- [ ] All acceptance criteria met
- [ ] All tasks/subtasks completed
- [ ] JSDoc comments added to all functions
- [ ] Security requirements implemented (if backend)
- [ ] Manual testing completed
- [ ] File List updated in Dev Agent Record
- [ ] Completion Notes documented
- [ ] No console errors in browser/server
- [ ] Responsive layout tested (if UI story)
- [ ] Session authentication working (if admin story)

---

## 🆘 Need Help?

### **Architecture Questions**
- See `docs/architecture/` directory
- See `docs/architecture/table-of-contents.md` for navigation

### **API Questions**
- See `docs/architecture/api-specification.md`
- See `docs/BACKEND_ROUTES_IMPLEMENTATION_STATUS.md`

### **Security Questions**
- See `docs/architecture/security-guidelines.md`
- Reference Story 1.5 for example implementation

### **Story Questions**
- See previous story files in `docs/stories/`
- Check "Previous Story Insights" in current story's Dev Notes

---

## 📊 Development Workflow Summary

```
1. Read story file → Understand ACs and tasks
2. ✅ Review BACKEND_ROUTES_IMPLEMENTATION_STATUS.md → Check existing routes
3. ✅ Review devLoadAlwaysFiles → Understand standards
4. Plan implementation → Avoid duplicating existing backend
5. Implement code → Follow coding standards
6. Test manually → Per tech-stack.md
7. Update File List → Document all changes
8. Mark "Ready for Review" → QA agent will review
```

---

**Remember:** The goal is to deliver working software efficiently, not to over-engineer. Follow the MVP approach: simple, functional, and meeting all acceptance criteria.

**Good luck with your implementation! 🚀**
