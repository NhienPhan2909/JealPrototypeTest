# 6. Components

Flat, domain-organized component structure for rapid 2-day implementation. Backend uses simple route handlers + database modules. Frontend uses page-level components for routes + minimal shared components.

## Backend Components

### File Structure

```
backend/
├── server.js                 # Express app setup, middleware, route mounting
├── routes/
│   ├── auth.js              # POST /auth/login, /auth/logout, GET /auth/me
│   ├── dealers.js           # GET/PUT /dealers
│   ├── vehicles.js          # GET/POST/PUT/DELETE /vehicles
│   ├── leads.js             # GET/POST /leads (with email notifications)
│   ├── googleReviews.js     # GET /google-reviews/:dealershipId
│   └── upload.js            # POST /upload (Cloudinary)
├── services/
│   └── emailService.js      # Email notification service (nodemailer)
├── db/
│   ├── index.js             # PostgreSQL connection pool setup
│   ├── dealers.js           # Dealership queries (getAll, getById, update)
│   ├── vehicles.js          # Vehicle queries (getAll, getById, create, update, delete)
│   └── leads.js             # Lead queries (getByDealership, create)
├── middleware/
│   └── auth.js              # requireAuth middleware
└── package.json
```

### server.js

**Responsibility:** Main Express app setup, middleware configuration, route mounting, static file serving.

**Key Functions:**
- Initialize Express app
- Configure middleware (JSON parser, CORS, session, Morgan logging)
- Mount API routes (`/api/auth`, `/api/dealers`, `/api/vehicles`, `/api/leads`, `/api/upload`)
- Serve React build in production (`express.static('frontend/dist')`)
- Catch-all route for React Router (return `index.html` for non-API routes)
- Start server on port 5000

**Implementation Sketch:**
```javascript
const express = require('express');
const session = require('express-session');
const cors = require('cors');
const morgan = require('morgan');
const path = require('path');

const app = express();

// Middleware
app.use(express.json());
app.use(cors({ origin: 'http://localhost:3000', credentials: true })); // Dev only
app.use(morgan('dev'));
app.use(session({
  secret: process.env.SESSION_SECRET,
  resave: false,
  saveUninitialized: false,
  cookie: { secure: false } // Set true in production with HTTPS
}));

// API Routes
app.use('/api/auth', require('./routes/auth'));
app.use('/api/dealers', require('./routes/dealers'));
app.use('/api/vehicles', require('./routes/vehicles'));
app.use('/api/leads', require('./routes/leads'));
app.use('/api/upload', require('./routes/upload'));
app.use('/api/google-reviews', require('./routes/googleReviews'));

// Serve React app in production
if (process.env.NODE_ENV === 'production') {
  app.use(express.static(path.join(__dirname, '../frontend/dist')));
  app.get('*', (req, res) => {
    res.sendFile(path.join(__dirname, '../frontend/dist', 'index.html'));
  });
}

const PORT = process.env.PORT || 5000;
app.listen(PORT, () => console.log(`Server running on port ${PORT}`));
```

### routes/*.js

**Responsibility:** Handle HTTP requests for specific domain (auth, dealers, vehicles, leads).

**Dependencies:** Database modules (`db/*.js`), auth middleware

**Pattern:** Express Router with route handlers that call database functions.

### services/emailService.js

**Responsibility:** Send email notifications for business events (new leads, status updates, etc.).

**Dependencies:** nodemailer (SMTP client)

**Key Functions:**
- `sendNewLeadNotification(dealershipEmail, leadData)` - Sends formatted email notification when new lead is created

**Features:**
- HTML and plain text email formats (HTML for modern clients, text fallback)
- Graceful degradation: Skips email if configuration missing (development mode support)
- Non-blocking: Email failures logged but don't affect lead creation
- Rich email template with CSS styling and dealership branding

**Environment Configuration Required:**
- `EMAIL_HOST` - SMTP server hostname (e.g., smtp.gmail.com)
- `EMAIL_PORT` - SMTP port (587 for TLS, 465 for SSL)
- `EMAIL_USER` - SMTP authentication username
- `EMAIL_PASSWORD` - SMTP authentication password/app password
- `EMAIL_FROM` - Sender email address (defaults to EMAIL_USER)

**Implementation Pattern:**
```javascript
const { sendNewLeadNotification } = require('../services/emailService');

// In lead creation route (after lead saved to database)
try {
  const dealership = await dealersDb.getById(dealership_id);
  if (dealership && dealership.email) {
    await sendNewLeadNotification(dealership.email, {
      name: customerName,
      email: customerEmail,
      phone: customerPhone,
      message: customerMessage,
      vehicleInfo: vehicleTitle // Optional
    });
  }
} catch (emailError) {
  // Log error but don't fail request - lead already created
  console.error('Email notification failed:', emailError);
}
```

**Email Template Features:**
- Professional HTML layout with inline CSS
- Blue-themed branding matching platform design (#3B82F6)
- Structured lead details display
- Mobile-responsive design
- Call-to-action reminder for quick response

### db/*.js

**Responsibility:** Encapsulate all database queries for a domain.

**Functions:** getAll, getById, create, update, delete (as needed)

**Pattern:** Async functions that use parameterized queries with pg pool.

### middleware/auth.js

**Responsibility:** Protect admin routes.

**Function:**
```javascript
function requireAuth(req, res, next) {
  if (!req.session.isAuthenticated) {
    return res.status(401).json({ error: 'Authentication required' });
  }
  next();
}

module.exports = { requireAuth };
```

## Frontend Components

### File Structure

```
frontend/src/
├── main.jsx                       # App entry point, React Router setup
├── App.jsx                        # Root component with routes
├── index.css                      # Tailwind directives + global styles
├── pages/
│   ├── public/
│   │   ├── Home.jsx               # Public home page
│   │   ├── Inventory.jsx          # Vehicle listing page with search/filter
│   │   ├── VehicleDetail.jsx      # Single vehicle detail + enquiry form
│   │   └── About.jsx              # Dealership about/contact page
│   └── admin/
│       ├── Login.jsx              # Admin login page
│       ├── Dashboard.jsx          # Admin dashboard with dealership selector
│       ├── VehicleList.jsx        # Vehicle manager (list + edit/delete)
│       ├── VehicleForm.jsx        # Create/edit vehicle form
│       ├── DealerSettings.jsx     # Dealership settings form
│       └── LeadInbox.jsx          # Customer enquiries list
├── components/
│   ├── Layout.jsx                 # Shared layout wrapper (header, footer, children)
│   ├── Header.jsx                 # Public site header with nav
│   ├── AdminHeader.jsx            # Admin header with dealership selector + logout
│   ├── VehicleCard.jsx            # Vehicle grid item (thumbnail, title, price)
│   ├── EnquiryForm.jsx            # Reusable enquiry form component
│   ├── ImageGallery.jsx           # Vehicle detail image gallery
│   ├── GoogleReviewsCarousel.jsx  # Google reviews carousel (homepage)
│   └── ProtectedRoute.jsx         # Auth wrapper for admin routes
├── context/
│   └── AdminContext.jsx           # Global state for admin (selected dealership, auth status)
└── utils/
    └── api.js                     # Fetch wrappers for API calls (optional)
```

### Public Pages

**pages/public/Home.jsx**
- Fetch dealership info from `/api/dealers/:id`
- Display hero section with name, logo, tagline
- Hero background: Dynamic background image with 40% black overlay (if `hero_background_image` set), otherwise blue gradient
- Background styling: `cover`, `center`, `no-repeat` for optimal display
- Text readability: Drop shadows on hero text ensure visibility over any background
- "Browse Inventory" CTA → link to `/inventory`

**pages/public/Inventory.jsx**
- Fetch vehicles from `/api/vehicles?dealershipId=X` (all vehicles for dealership)
- Filter client-side for public-visible statuses: `status === 'active' || status === 'pending'`
- Display grid of VehicleCard components (shows available + pending sale vehicles)
- Client-side search, filter, sort
- **Note**: Implemented in Story 2.2 with client-side filtering to support both active and pending vehicles per Data Model public visibility specification

**pages/public/VehicleDetail.jsx**
- Fetch vehicle from `/api/vehicles/:id`
- Display ImageGallery, specs, description
- Render EnquiryForm component

**pages/public/About.jsx**
- Fetch dealership from `/api/dealers/:id`
- Display contact info, hours, about text

### Admin Pages

**pages/admin/Login.jsx**
- Username/password form
- POST to `/api/auth/login`
- On success: update AdminContext, redirect to `/admin`

**pages/admin/Dashboard.jsx**
- Fetch dealerships (`/api/dealers`)
- Dealership selector dropdown (updates AdminContext)
- Display stats: vehicle count, lead count
- Navigation to other admin pages

**pages/admin/VehicleList.jsx**
- Fetch vehicles (`/api/vehicles?dealershipId=X`)
- Table with edit/delete actions
- "Add Vehicle" button → navigate to `/admin/vehicles/new`

**pages/admin/VehicleForm.jsx**
- React Hook Form for all fields
- **Image Upload (Updated 2025-11-28):** File input + `/api/upload` endpoint (reliable, no page freeze)
  - Native OS file picker for selecting 1-10 images
  - Client-side validation: JPG/PNG/WebP, max 5MB per file, 10 total max
  - Sequential upload to `/api/upload` endpoint
  - Real-time thumbnail grid with remove buttons
  - Progress indicator: "Uploading..." state
  - Success/error feedback messages
- Submit → POST or PUT `/api/vehicles` with images array (Cloudinary URLs)

**pages/admin/DealerSettings.jsx**
- Fetch and edit dealership settings
- Cloudinary widget upload for logo (with square cropping)
- File input upload for hero background image (uses `/api/upload` endpoint)
  - Client-side validation: JPG/PNG/WebP, max 5MB
  - Preview with 40% overlay to simulate hero appearance
  - Change/Remove functionality
- Submit → PUT `/api/dealers/:id` with `hero_background_image` field

**pages/admin/LeadInbox.jsx**
- Fetch leads (`/api/leads?dealershipId=X`)
- Display table with contact action buttons
- **Message Display**: Smart truncation with show more/less toggle
  - Messages truncated to 100 characters initially with "..." indicator
  - "Show more" link appears for messages > 100 characters
  - Click expands to show full message, changes to "Show less"
  - Only one message expanded at a time
  - State: `expandedLeadId` tracks currently expanded message
  - Helpers: `truncateMessage()`, `toggleExpand()`
- **Status Management**: Dropdown selector with color-coded badges (received/in progress/done)
- **Delete Functionality**: Delete button with confirmation modal
- **Vehicle Linking**: Fetches vehicle titles client-side for associated leads
- **Date Filtering**: Filter by Last 7 days, Last 30 days, or All time
- **Sorting**: Newest leads first (created_at descending)
- HTML entity decoding for safe display of user input


### Shared Components

**components/Layout.jsx**
- Wrapper with Header and Footer components
- Applies dealership theme color and font family via CSS custom properties
- Contains Outlet for child routes

**components/Header.jsx**
- Public site navigation (Home, Inventory, About, Log In)
- Uses dealership theme color as background
- Displays dealership logo and name
- Uses navigation_config from database for customizable menu
- Mobile-responsive with hamburger menu
- "Log In" link navigates to /admin/login for dealership staff access
- **Layout:** Center-aligned header with dealership branding and navigation (updated 2025-12-31)
- **Dealership Name:** Displays horizontally with `whitespace-nowrap` to prevent vertical stacking (fixed 2025-12-31)
- **Responsive:** `flex-col md:flex-row` layout for mobile (stacked) and desktop (horizontal) views
- **Mobile Menu:** Hamburger button positioned absolutely (top-right) while branding stays centered

**components/Footer.jsx**
- Displays dealership contact information (address, phone, email)
- Shows opening hours with multi-line support
- Quick links navigation (filtered to exclude admin links)
- Social media icons (Facebook, Instagram) integrated in Opening Hours column
- Copyright notice with dynamic year
- Uses dealership theme color as background
- Fully responsive (3-column desktop, stacked mobile)

**Key Features:**
```javascript
// Footer uses same hooks as Header
const { currentDealershipId } = useDealershipContext();
const { dealership } = useDealership(currentDealershipId);

// Filter navigation to exclude admin links
const footerNavItems = navigationConfig
  .filter(item => item.enabled && !item.route.includes('admin'))
  .sort((a, b) => a.order - b.order);

// Social media section integrated in Column 2 (Opening Hours)
// Updated 2025-12-08: Moved from separate section to Opening Hours column
{(dealership?.facebook_url || dealership?.instagram_url) && (
  <div className="mt-6">
    <h4 className="font-semibold mb-3 text-white/90">Follow Us</h4>
    <div className="flex gap-4">
      {/* Facebook and Instagram icons */}
    </div>
  </div>
)}
```

**Layout Structure (Updated 2025-12-08):**
```
Footer Component
├── Three-Column Grid
│   ├── Column 1: Contact Information
│   ├── Column 2: Opening Hours & Social Media
│   │   ├── Opening hours text
│   │   └── Follow Us (social icons)
│   └── Column 3: Quick Links
└── Copyright Section
```

**components/AdminHeader.jsx**
- Dealership selector + nav + "View Website" button + logout
- Fetches all dealerships from API on mount
- Updates AdminContext when dealership selection changes
- "View Website" button syncs public DealershipContext and navigates to home page
- Logout button calls `/api/auth/logout` and redirects to login

**Layout Structure (Updated 2025-12-31):**
- Header uses single-line horizontal layout with three main sections, center-aligned
- Section 1: "Admin Panel" title (left, with flex-shrink-0)
- Section 2: Dealership Selector (middle)
- Section 3: Navigation links (right, including Dashboard, Vehicle Manager, Blog Manager, Settings, Lead Inbox, Sales Requests, View Website, Log Out)
- Responsive: stacks vertically on mobile (gap-4), horizontal and centered on large screens (lg:flex-row lg:justify-center)
- Optimized for reduced vertical height while maintaining visual balance and center alignment

**Key Features:**
```javascript
// View Website functionality (Story 3.2.1)
const handleViewWebsite = () => {
  if (selectedDealership) {
    setCurrentDealershipId(selectedDealership.id); // Sync contexts
    navigate('/'); // Navigate to public home
  }
};

// Layout structure (Updated 2025-12-12)
// Main container with three sections in flex row
<div className="flex flex-col lg:flex-row lg:items-center gap-4">
  {/* Admin Panel Title */}
  <h1 className="text-2xl font-bold whitespace-nowrap flex-shrink-0">Admin Panel</h1>

  {/* Dealership Selector - Centered with equal spacing */}
  <div className="flex-shrink-0 lg:mx-8">
    {/* Selector dropdown and "Managing: X" display */}
  </div>

  {/* Navigation Menu */}
  <nav className="flex flex-wrap items-center gap-3 md:gap-4">
    {/* Navigation links */}
  </nav>
</div>
```

**components/VehicleCard.jsx**
- Props: vehicle object
- Displays thumbnail, title, price, condition badge

**components/EnquiryForm.jsx**
- Props: vehicleId, dealershipId
- Form with name, email, phone, message
- Submit → POST `/api/leads`

**components/ImageGallery.jsx**
- Props: images array
- Main image + thumbnail navigation

**components/GoogleReviewsCarousel.jsx** (Added 2025-12-31)
- Displays Google reviews in interactive carousel format
- Fetches reviews from `/api/google-reviews/:dealershipId`
- Shows 3-4 reviews per view with navigation arrows
- Features:
  - Star rating display (1-5 stars)
  - Reviewer profile photos and names
  - Review text (truncated to 4 lines)
  - Relative time (e.g., "2 months ago")
  - Pagination dots for position indicator
  - "Read More Reviews" button linking to Google Maps
- Responsive design (mobile: 1 review, tablet: 2-3, desktop: 3)
- Graceful error handling (silent failure if reviews unavailable)
- Uses dealership theme color for CTA button
- **Location:** Homepage, below search widget and enquiry form
- **Configuration:** Requires `GOOGLE_PLACES_API_KEY` in `.env`
- **Documentation:** `GOOGLE_REVIEWS_DOCS_INDEX.md`

**components/ProtectedRoute.jsx**
- Checks AdminContext.isAuthenticated
- Redirects to `/admin/login` if not authenticated

### context/AdminContext.jsx

**Provides:**
- `isAuthenticated` (boolean)
- `setIsAuthenticated` (function)
- `selectedDealership` (object or null)
- `setSelectedDealership` (function)

**Pattern:** React Context API with useState and useEffect hooks.

### context/DealershipContext.jsx

**Purpose:** Manages currently selected dealership for public website viewing.

**Provides:**
- `currentDealershipId` (number) - Currently selected dealership ID
- `setCurrentDealershipId` (function) - Update selected dealership

**Features:**
- Persists selection to localStorage
- Loads saved selection on mount
- Defaults to dealership ID 1 on first visit
- Used by all public pages (Home, Inventory, About, VehicleDetail, Header)

**Pattern:** React Context API with useState and useEffect hooks.

**Implementation:**
```javascript
export function DealershipProvider({ children }) {
  const [currentDealershipId, setCurrentDealershipId] = useState(() => {
    const saved = localStorage.getItem('selectedDealershipId');
    return saved ? parseInt(saved, 10) : 1;
  });

  useEffect(() => {
    localStorage.setItem('selectedDealershipId', currentDealershipId.toString());
  }, [currentDealershipId]);

  return (
    <DealershipContext.Provider value={{ currentDealershipId, setCurrentDealershipId }}>
      {children}
    </DealershipContext.Provider>
  );
}
```

**Usage:**
```javascript
import { useDealershipContext } from '../../context/DealershipContext';

function MyComponent() {
  const { currentDealershipId } = useDealershipContext();
  // Use currentDealershipId for API calls
}
```

---
