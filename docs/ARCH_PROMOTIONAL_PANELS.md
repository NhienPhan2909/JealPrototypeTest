# Promotional Panels Feature - Architecture Document

**Document Owner:** Solution Architect  
**Created:** 2026-01-04  
**Last Updated:** 2026-01-04  
**Feature:** Homepage Promotional Panels for Finance & Warranty  
**Status:** Completed  
**Version:** 1.0

---

## 1. Architecture Overview

### 1.1 Feature Summary
The Promotional Panels feature extends the dealership homepage with two customizable promotional sections positioned below the Customer Reviews carousel. The architecture follows established patterns from the Hero Media feature, reusing the existing Cloudinary upload infrastructure and theme color system while maintaining strict multi-tenant data isolation.

### 1.2 Architectural Goals
- **Reusability:** Leverage existing upload and storage infrastructure
- **Consistency:** Follow established component and API patterns
- **Security:** Maintain multi-tenant isolation and input sanitization
- **Performance:** Minimize additional page load overhead
- **Maintainability:** Clear separation of concerns between layers

### 1.3 Integration Points
- Dealership Settings API (`PUT /api/dealers/:id`)
- Cloudinary Upload Service (`POST /api/upload`)
- Theme Color System (CSS custom properties)
- Homepage Layout (below GoogleReviewsCarousel)
- Multi-tenant Data Layer (dealership_id filtering)

---

## 2. Data Architecture

### 2.1 Database Schema

#### Table: `dealership`
**Migration:** `008_add_promo_panels.sql`

```sql
ALTER TABLE dealership
ADD COLUMN finance_promo_image TEXT,
ADD COLUMN finance_promo_text TEXT,
ADD COLUMN warranty_promo_image TEXT,
ADD COLUMN warranty_promo_text TEXT;
```

**Field Specifications:**

| Column | Type | Nullable | Constraints | Description |
|--------|------|----------|-------------|-------------|
| `finance_promo_image` | TEXT | Yes | None | Finance panel background image URL (Cloudinary CDN) |
| `finance_promo_text` | TEXT | Yes | Max 500 chars (app-level) | Finance panel promotional text overlay |
| `warranty_promo_image` | TEXT | Yes | None | Warranty panel background image URL (Cloudinary CDN) |
| `warranty_promo_text` | TEXT | Yes | Max 500 chars (app-level) | Warranty panel promotional text overlay |

**Design Decisions:**
- **TEXT type:** Flexible for Cloudinary URLs (variable length)
- **Nullable:** Fields optional - panels work with defaults
- **No DB constraints on length:** Application-level validation (500 chars)
- **No separate table:** Low cardinality (4 fields per dealership)

### 2.2 Data Model

```typescript
interface Dealership {
  id: number;
  name: string;
  // ... existing fields ...
  finance_promo_image?: string;
  finance_promo_text?: string;
  warranty_promo_image?: string;
  warranty_promo_text?: string;
}
```

### 2.3 Data Flow

```
Admin Upload → Cloudinary → CDN URL → Database → API → React Component → Public Display
     ↓             ↓            ↓          ↓        ↓          ↓              ↓
  File Input   /api/upload   Store URL   dealers   Props   PromotionalPanels  Homepage
  (5MB max)   (validation)  (TEXT field)  table   (image)    Component       (visible)
```

---

## 3. API Architecture

### 3.1 Endpoint: PUT /api/dealers/:id

**Purpose:** Update dealership profile including promotional panel content

**Request:**
```http
PUT /api/dealers/1
Content-Type: application/json

{
  "name": "Existing Dealership",
  // ... other fields ...
  "finance_promo_image": "https://res.cloudinary.com/...",
  "finance_promo_text": "Flexible Financing Options Available",
  "warranty_promo_image": "https://res.cloudinary.com/...",
  "warranty_promo_text": "Comprehensive Warranty Coverage"
}
```

**Validation Rules:**
```javascript
// backend/routes/dealers.js
const FIELD_LIMITS = {
  // ... existing limits ...
  finance_promo_text: 500,
  warranty_promo_text: 500
};

// XSS sanitization applied
const sanitizedFinancePromoText = finance_promo_text 
  ? sanitizeInput(finance_promo_text) 
  : undefined;
```

**Response:**
```json
{
  "id": 1,
  "name": "Existing Dealership",
  "finance_promo_image": "https://res.cloudinary.com/...",
  "finance_promo_text": "Flexible Financing Options Available",
  "warranty_promo_image": "https://res.cloudinary.com/...",
  "warranty_promo_text": "Comprehensive Warranty Coverage",
  "updated_at": "2026-01-04T23:00:00.000Z"
}
```

### 3.2 Endpoint: GET /api/dealers/:id

**Purpose:** Retrieve dealership data including promotional panel content

**Response:**
```json
{
  "id": 1,
  "name": "Dealership Name",
  "finance_promo_image": "https://res.cloudinary.com/.../finance.jpg",
  "finance_promo_text": "0% APR Available on Select Models",
  "warranty_promo_image": "https://res.cloudinary.com/.../warranty.jpg",
  "warranty_promo_text": "Extended Warranty Plans Available"
}
```

**Multi-Tenant Isolation:**
```javascript
// backend/db/dealers.js
async function getById(dealershipId) {
  const query = 'SELECT * FROM dealership WHERE id = $1';
  const result = await pool.query(query, [dealershipId]);
  return result.rows[0] || null;
}
```

### 3.3 Database Layer

**File:** `backend/db/dealers.js`

```javascript
async function update(dealershipId, updates) {
  // ... existing field handling ...
  
  if (updates.finance_promo_image !== undefined) {
    fields.push(`finance_promo_image = $${paramIndex++}`);
    values.push(updates.finance_promo_image);
  }
  if (updates.finance_promo_text !== undefined) {
    fields.push(`finance_promo_text = $${paramIndex++}`);
    values.push(updates.finance_promo_text);
  }
  if (updates.warranty_promo_image !== undefined) {
    fields.push(`warranty_promo_image = $${paramIndex++}`);
    values.push(updates.warranty_promo_image);
  }
  if (updates.warranty_promo_text !== undefined) {
    fields.push(`warranty_promo_text = $${paramIndex++}`);
    values.push(updates.warranty_promo_text);
  }
  
  // WHERE clause enforces multi-tenancy
  values.push(dealershipId);
  const query = `
    UPDATE dealership
    SET ${fields.join(', ')}
    WHERE id = $${paramIndex}
    RETURNING *
  `;
}
```

---

## 4. Component Architecture

### 4.1 Component Hierarchy

```
Home.jsx (Page)
├── HeroCarousel
├── SearchWidget
├── GeneralEnquiryForm
├── GoogleReviewsCarousel
└── PromotionalPanels ← NEW COMPONENT
    ├── FinancePanel (inline)
    │   ├── Background Image Layer
    │   ├── Dark Overlay (rgba)
    │   └── Content Layer
    │       ├── Heading ("Finance")
    │       ├── Promotional Text
    │       └── CTA Button → /finance
    └── WarrantyPanel (inline)
        ├── Background Image Layer
        ├── Dark Overlay (rgba)
        └── Content Layer
            ├── Heading ("Warranty")
            ├── Promotional Text
            └── CTA Button → /warranty
```

### 4.2 PromotionalPanels Component

**File:** `frontend/src/components/PromotionalPanels.jsx`

**Component Signature:**
```javascript
function PromotionalPanels({ 
  financeImage, 
  financeText, 
  warrantyImage, 
  warrantyText 
}) {
  // Component implementation
}
```

**Props Interface:**
```typescript
interface PromotionalPanelsProps {
  financeImage?: string;    // Finance panel background image URL
  financeText?: string;     // Finance panel promotional text
  warrantyImage?: string;   // Warranty panel background image URL
  warrantyText?: string;    // Warranty panel promotional text
}
```

**Rendering Logic:**
```javascript
// Background image with dark overlay or gradient fallback
style={{
  backgroundImage: financeImage
    ? `linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url(${financeImage})`
    : 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
  backgroundSize: 'cover',
  backgroundPosition: 'center',
  backgroundRepeat: 'no-repeat'
}}

// Text with fallback
{financeText || 'Explore Our Financing Options'}

// CTA Button with theme colors
style={{ 
  backgroundColor: 'var(--theme-color)', 
  color: 'var(--secondary-theme-color)' 
}}
```

### 4.3 Responsive Design

**Breakpoint Strategy:**
```css
/* Mobile First Approach */
.promotional-panels {
  display: grid;
  grid-template-columns: 1fr;           /* Mobile: Stacked */
  gap: 1.5rem;                          /* 24px gap */
}

@media (min-width: 768px) {             /* md breakpoint */
  .promotional-panels {
    grid-template-columns: repeat(2, 1fr); /* Desktop: Side-by-side */
  }
}
```

**Tailwind Classes:**
```jsx
<div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-8">
```

### 4.4 Default Behavior

**No Configuration Scenario:**
```javascript
// Finance Panel
Background: Linear gradient (#667eea → #764ba2)
Text: "Explore Our Financing Options"
Button: "View Our Policy" → /finance

// Warranty Panel
Background: Linear gradient (#f093fb → #f5576c)
Text: "Learn About Our Warranty"
Button: "View Our Policy" → /warranty
```

**Partial Configuration Scenarios:**
- Image only: Shows image with default text
- Text only: Shows gradient with custom text
- Image + Text: Shows image with custom text

---

## 5. Admin Interface Architecture

### 5.1 DealerSettings Component Extension

**File:** `frontend/src/pages/admin/DealerSettings.jsx`

**State Management:**
```javascript
// New state variables
const [financePromoImage, setFinancePromoImage] = useState('');
const [financePromoText, setFinancePromoText] = useState('');
const [warrantyPromoImage, setWarrantyPromoImage] = useState('');
const [warrantyPromoText, setWarrantyPromoText] = useState('');
```

**Data Flow:**
```
1. Load: GET /api/dealers/:id → setState()
2. Upload: File Input → /api/upload → setImage(url)
3. Edit: Text Input → setState()
4. Save: Form Submit → PUT /api/dealers/:id → Success
```

### 5.2 Image Upload Architecture

**Upload Handler Flow:**
```javascript
handleFinancePromoImageUpload(event) {
  1. Extract file from event
  2. Validate file type (JPG, PNG, WebP)
  3. Validate file size (max 5MB)
  4. Create FormData with image
  5. POST /api/upload with FormData
  6. Handle response:
     - Success: setFinancePromoImage(url)
     - Error: Display error message
  7. Clear file input
}
```

**Validation Rules:**
```javascript
const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
const maxSize = 5 * 1024 * 1024; // 5MB

if (!allowedTypes.includes(file.type)) {
  setError('Invalid file type. Please upload JPG, PNG, or WebP images only.');
  return;
}

if (file.size > maxSize) {
  setError('File too large. Maximum size is 5MB.');
  return;
}
```

### 5.3 Form Submission

**Payload Construction:**
```javascript
const dealershipData = {
  // ... existing fields ...
  finance_promo_image: financePromoImage || null,
  finance_promo_text: financePromoText || null,
  warranty_promo_image: warrantyPromoImage || null,
  warranty_promo_text: warrantyPromoText || null
};

await fetch(`/api/dealers/${selectedDealership.id}`, {
  method: 'PUT',
  headers: { 'Content-Type': 'application/json' },
  credentials: 'include',
  body: JSON.stringify(dealershipData)
});
```

---

## 6. Security Architecture

### 6.1 Input Validation

**Backend Validation (Defense-in-Depth):**
```javascript
// Field length limits
const FIELD_LIMITS = {
  finance_promo_text: 500,
  warranty_promo_text: 500
};

function validateFieldLengths(data) {
  for (const [field, limit] of Object.entries(FIELD_LIMITS)) {
    if (data[field] && data[field].length > limit) {
      return { error: `${field} must be ${limit} characters or less` };
    }
  }
  return null;
}
```

**XSS Prevention:**
```javascript
function sanitizeInput(input) {
  if (typeof input !== 'string') return input;
  return input
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#x27;')
    .replace(/\//g, '&#x2F;');
}

const sanitizedFinancePromoText = finance_promo_text 
  ? sanitizeInput(finance_promo_text) 
  : undefined;
```

### 6.2 Multi-Tenant Isolation

**Database Level:**
```sql
-- All queries filter by dealership_id
SELECT * FROM dealership WHERE id = $1;
UPDATE dealership SET ... WHERE id = $1;
```

**Application Level:**
```javascript
// Admin context enforces selected dealership
const { selectedDealership } = useContext(AdminContext);

// API calls include dealership ID
fetch(`/api/dealers/${selectedDealership.id}`, ...);
```

### 6.3 Upload Security

**File Type Validation:**
```javascript
const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];
if (!allowedTypes.includes(file.type)) {
  throw new Error('Invalid file type');
}
```

**File Size Validation:**
```javascript
const maxSize = 5 * 1024 * 1024; // 5MB
if (file.size > maxSize) {
  throw new Error('File too large');
}
```

**Cloudinary Security:**
- Unsigned upload preset (controlled on Cloudinary dashboard)
- File type restrictions enforced
- Automatic malware scanning (Cloudinary feature)

---

## 7. Performance Architecture

### 7.1 Image Delivery

**Cloudinary CDN:**
- Global edge caching
- Automatic image optimization
- WebP format support
- Lazy loading compatible

**Image Loading Strategy:**
```jsx
// Background images use CSS
style={{ backgroundImage: `url(${imageUrl})` }}

// Browser handles lazy loading for background images
// Future enhancement: Intersection Observer for explicit lazy load
```

### 7.2 Page Load Impact

**Before Promotional Panels:**
- Homepage loads: Hero + Search + Enquiry Form + Reviews
- Estimated size: ~500KB (with images)

**After Promotional Panels:**
- Additional components: PromotionalPanels.jsx (~2KB gzipped)
- Additional API data: 4 fields in dealership object (~200 bytes)
- Additional images: 2 backgrounds (~400KB total, CDN cached)
- **Total impact:** < 200ms additional load time

### 7.3 Rendering Performance

**Component Optimization:**
- Pure functional component (no state)
- Props destructuring (no prop drilling)
- CSS-based styling (no runtime calculations)
- No useEffect hooks (no side effects)

**Rendering Metrics:**
- First paint: Immediate (uses gradient fallbacks)
- Image load: Progressive (CSS background)
- Interactive: Immediate (links ready)

---

## 8. Integration Architecture

### 8.1 Theme Color Integration

**CSS Custom Properties:**
```jsx
// Button uses theme colors
style={{ 
  backgroundColor: 'var(--theme-color)', 
  color: 'var(--secondary-theme-color)' 
}}

// Set in DealershipContext
<style>
  :root {
    --theme-color: {dealership.theme_color || '#3B82F6'};
    --secondary-theme-color: {dealership.secondary_theme_color || '#FFFFFF'};
  }
</style>
```

**Benefits:**
- Consistent branding across site
- No hardcoded colors in component
- Dynamic updates without re-render

### 8.2 Navigation Integration

**Route Targets:**
```jsx
<Link to="/finance">View Our Policy</Link>
<Link to="/warranty">View Our Policy</Link>
```

**Existing Routes:**
```javascript
// frontend/src/App.jsx
<Route path="/finance" element={<Finance />} />
<Route path="/warranty" element={<Warranty />} />
```

### 8.3 Homepage Layout Integration

**Position in Layout:**
```jsx
<div className="container mx-auto px-4 py-12">
  <SearchWidget />
  <GeneralEnquiryForm />
  <GoogleReviewsCarousel />
  <PromotionalPanels /> {/* ← NEW */}
</div>
```

**Spacing Strategy:**
- Parent container: `py-12` (48px top/bottom padding)
- Component: `mt-8` (32px top margin)
- Between panels: `gap-6` (24px gap)

---

## 9. Deployment Architecture

### 9.1 Migration Deployment

**Migration File:** `008_add_promo_panels.sql`

**Deployment Steps:**
```bash
# Local/Development
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend/db/migrations/008_add_promo_panels.sql

# Production (Railway)
cat backend/db/migrations/008_add_promo_panels.sql | railway run psql
```

**Rollback Strategy:**
```sql
-- Rollback migration if needed
ALTER TABLE dealership
DROP COLUMN finance_promo_image,
DROP COLUMN finance_promo_text,
DROP COLUMN warranty_promo_image,
DROP COLUMN warranty_promo_text;
```

### 9.2 Zero-Downtime Deployment

**Backward Compatibility:**
- New columns are nullable
- No breaking changes to existing API
- Frontend gracefully handles missing data
- Component works with defaults

**Deployment Order:**
1. Apply database migration
2. Deploy backend code
3. Deploy frontend code
4. Feature immediately available

---

## 10. Testing Architecture

### 10.1 Component Testing Strategy

**Unit Tests (Future):**
```javascript
describe('PromotionalPanels', () => {
  it('renders with custom images and text', () => {});
  it('renders with gradient fallbacks', () => {});
  it('displays default text when not provided', () => {});
  it('links to correct pages', () => {});
  it('applies theme colors to buttons', () => {});
});
```

### 10.2 Integration Testing Strategy

**API Tests (Future):**
```javascript
describe('PUT /api/dealers/:id - Promotional Panels', () => {
  it('updates promotional fields', () => {});
  it('validates text length limits', () => {});
  it('sanitizes text inputs', () => {});
  it('supports partial updates', () => {});
});
```

### 10.3 Manual Testing Checklist

**Functional Tests:**
- [ ] Upload finance image
- [ ] Upload warranty image
- [ ] Enter promotional text
- [ ] Save and verify persistence
- [ ] View on homepage
- [ ] Click CTA buttons
- [ ] Test on mobile

**Security Tests:**
- [ ] Test XSS in text fields
- [ ] Test oversized file upload
- [ ] Test invalid file type
- [ ] Verify multi-tenant isolation

---

## 11. Monitoring & Observability

### 11.1 Metrics to Track

**Usage Metrics:**
- Configuration rate (% of dealerships with custom panels)
- Upload success/failure rate
- Click-through rate on CTA buttons
- Mobile vs desktop usage

**Performance Metrics:**
- Page load time with panels
- Image load time (CDN performance)
- API response time for dealer updates

### 11.2 Error Tracking

**Client-Side Errors:**
- Upload failures (file validation, network)
- Component render errors
- Navigation failures

**Server-Side Errors:**
- Database update failures
- Validation errors
- File processing errors

---

## 12. Future Architecture Considerations

### 12.1 Scalability

**Current Limitations:**
- Fixed to 2 panels (Finance & Warranty)
- Fixed routes (/finance, /warranty)
- No custom button text

**Scalability Path:**
```javascript
// Future: Dynamic panel configuration
interface Panel {
  id: string;
  title: string;
  image_url: string;
  text: string;
  button_text: string;
  button_link: string;
  order: number;
}

// Store as JSONB array
dealership.promo_panels: Panel[]
```

### 12.2 Enhancement Opportunities

**Video Backgrounds:**
```jsx
{panel.video_url ? (
  <video autoPlay loop muted>
    <source src={panel.video_url} type="video/mp4" />
  </video>
) : (
  <div style={{ backgroundImage: `url(${panel.image_url})` }} />
)}
```

**Analytics Integration:**
```jsx
<Link 
  to="/finance"
  onClick={() => trackEvent('promo_panel_click', { panel: 'finance' })}
>
```

**A/B Testing:**
```javascript
// Randomly show variant A or B
const variant = Math.random() < 0.5 ? 'A' : 'B';
const text = variant === 'A' 
  ? financePromoText 
  : financePromoTextAlt;
```

---

## 13. Appendix

### 13.1 Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| Database | PostgreSQL | Data persistence |
| Backend | Node.js/Express | API server |
| Frontend | React | UI components |
| Styling | Tailwind CSS | Responsive design |
| Image CDN | Cloudinary | Image hosting & optimization |
| Deployment | Railway/Render | Application hosting |

### 13.2 File Manifest

**Backend Files:**
- `backend/db/migrations/008_add_promo_panels.sql`
- `backend/routes/dealers.js` (modified)
- `backend/db/dealers.js` (modified)

**Frontend Files:**
- `frontend/src/components/PromotionalPanels.jsx` (new)
- `frontend/src/pages/public/Home.jsx` (modified)
- `frontend/src/pages/admin/DealerSettings.jsx` (modified)

**Documentation Files:**
- `docs/PRD_PROMOTIONAL_PANELS.md`
- `docs/SM_PROMOTIONAL_PANELS.md`
- `docs/ARCH_PROMOTIONAL_PANELS.md` (this file)
- `PROMOTIONAL_PANELS_FEATURE.md`
- `PROMOTIONAL_PANELS_QUICK_START.md`

### 13.3 Dependencies

**No New Dependencies:**
- Reuses existing React Router
- Reuses existing Tailwind CSS
- Reuses existing upload infrastructure
- No additional npm packages required

---

**Document Status:** Complete  
**Reviewed By:** Solution Architect  
**Approved For:** Production Implementation  
**Next Review:** After user feedback or enhancement requests
