# Promotional Panels Feature - Product Requirements Document

**Document Owner:** Product Manager  
**Created:** 2026-01-04  
**Last Updated:** 2026-01-04  
**Feature:** Homepage Promotional Panels for Finance & Warranty  
**Status:** Completed  
**Version:** 1.0

---

## 1. Executive Summary

### 1.1 Feature Overview
The Promotional Panels feature adds two customizable promotional sections to the dealership homepage, positioned below the Customer Reviews section. Each panel promotes a key dealership service (Finance or Warranty) with a background image, promotional text overlay, and a call-to-action button linking to the respective policy page.

### 1.2 Business Value
- **Increased Engagement:** Prominently showcases finance and warranty options to homepage visitors
- **Conversion Optimization:** Direct CTAs drive traffic to policy pages where customers can take action
- **Brand Consistency:** Customizable images and text allow dealerships to maintain brand identity
- **Visual Appeal:** Attractive panels with gradient fallbacks enhance homepage aesthetics
- **Mobile Responsive:** Ensures promotional content is visible on all devices

### 1.3 Target Users
- **Dealership Staff (Admin):** Configure promotional content via CMS
- **Website Visitors (Public):** View promotional panels and navigate to policy pages

---

## 2. Functional Requirements

### 2.1 Core Requirements

#### FR-PP1: Homepage Display
**Priority:** High  
**Description:** Two promotional panels must be displayed on the homepage below the Customer Reviews section.

**Acceptance Criteria:**
- Panels appear immediately below Google Reviews Carousel
- Desktop: Panels displayed side-by-side
- Mobile: Panels stacked vertically
- Minimum panel height: 300px
- Responsive gap between panels: 1.5rem (24px)

#### FR-PP2: Finance Promotional Panel
**Priority:** High  
**Description:** The Finance panel must display customizable content promoting financing options.

**Acceptance Criteria:**
- Displays background image (if uploaded) or gradient fallback (#667eea to #764ba2)
- Shows promotional text overlay with dark semi-transparent background
- Displays "Finance" heading
- Includes "View Our Policy" button linking to `/finance` page
- Text has drop shadow for readability
- Button uses dealership theme colors

#### FR-PP3: Warranty Promotional Panel
**Priority:** High  
**Description:** The Warranty panel must display customizable content promoting warranty coverage.

**Acceptance Criteria:**
- Displays background image (if uploaded) or gradient fallback (#f093fb to #f5576c)
- Shows promotional text overlay with dark semi-transparent background
- Displays "Warranty" heading
- Includes "View Our Policy" button linking to `/warranty` page
- Text has drop shadow for readability
- Button uses dealership theme colors

#### FR-PP4: Admin Configuration Interface
**Priority:** High  
**Description:** Dealership staff must be able to configure promotional panel content via CMS.

**Acceptance Criteria:**
- "Homepage Promotional Panels" section exists in Dealership Settings
- Two sub-sections: "Finance Promotional Panel" and "Warranty Promotional Panel"
- Each panel has image upload capability
- Each panel has text input field (max 500 characters)
- Image preview shown after upload
- Remove image button available
- Changes persist on Save Settings

#### FR-PP5: Image Upload Validation
**Priority:** High  
**Description:** Uploaded images must meet quality and security standards.

**Acceptance Criteria:**
- Accepted formats: JPG, PNG, WebP only
- Maximum file size: 5MB
- File type validation before upload
- File size validation before upload
- Error messages for invalid uploads
- Upload uses existing `/api/upload` endpoint

#### FR-PP6: Default Content
**Priority:** Medium  
**Description:** Panels must display sensible defaults when content is not configured.

**Acceptance Criteria:**
- Finance panel default text: "Explore Our Financing Options"
- Warranty panel default text: "Learn About Our Warranty"
- Gradient backgrounds when no images uploaded
- All buttons functional regardless of configuration

#### FR-PP7: Multi-Tenant Data Isolation
**Priority:** Critical  
**Description:** Promotional content must be isolated per dealership.

**Acceptance Criteria:**
- Finance/warranty promo data stored per dealership
- `dealership_id` foreign key enforced
- No cross-dealership data leakage
- API filters by selected dealership

### 2.2 Data Requirements

#### DR-PP1: Database Fields
**Priority:** High  
**Description:** New database fields must store promotional panel content.

**Fields Required:**
- `finance_promo_image` (TEXT, nullable) - Finance panel background image URL
- `finance_promo_text` (TEXT, nullable) - Finance panel promotional text
- `warranty_promo_image` (TEXT, nullable) - Warranty panel background image URL
- `warranty_promo_text` (TEXT, nullable) - Warranty panel promotional text

**Constraints:**
- All fields nullable (optional configuration)
- Text fields max 500 characters (enforced in backend)
- Image URLs validated as TEXT type

### 2.3 API Requirements

#### API-PP1: Dealership Update Endpoint
**Priority:** High  
**Description:** PUT `/api/dealers/:id` must accept promotional panel fields.

**Request Body Fields:**
```json
{
  "finance_promo_image": "string (optional)",
  "finance_promo_text": "string (optional, max 500)",
  "warranty_promo_image": "string (optional)",
  "warranty_promo_text": "string (optional, max 500)"
}
```

**Validations:**
- Text fields sanitized for XSS prevention
- Character limits enforced (500 chars)
- Partial updates supported

#### API-PP2: Dealership Retrieval
**Priority:** High  
**Description:** GET `/api/dealers/:id` must return promotional panel data.

**Response Includes:**
```json
{
  "id": 1,
  "name": "Dealership Name",
  ...
  "finance_promo_image": "https://...",
  "finance_promo_text": "Flexible Financing Available",
  "warranty_promo_image": "https://...",
  "warranty_promo_text": "Comprehensive Coverage"
}
```

---

## 3. Non-Functional Requirements

### 3.1 Performance
- **NFR-PP1:** Panel images must load efficiently via Cloudinary CDN
- **NFR-PP2:** Background images use CSS `cover` sizing for optimal display
- **NFR-PP3:** Page load time increase < 200ms with panels added

### 3.2 Security
- **NFR-PP4:** Text inputs sanitized to prevent XSS attacks
- **NFR-PP5:** Image uploads validated for type and size
- **NFR-PP6:** Multi-tenant isolation enforced at database and API layers

### 3.3 Usability
- **NFR-PP7:** Admin interface provides clear instructions for image dimensions
- **NFR-PP8:** Character counters shown for text inputs
- **NFR-PP9:** Image previews available before saving
- **NFR-PP10:** Default content ensures panels always look professional

### 3.4 Compatibility
- **NFR-PP11:** Panels responsive on desktop, tablet, and mobile devices
- **NFR-PP12:** Compatible with modern browsers (Chrome, Firefox, Safari, Edge)
- **NFR-PP13:** Integrates seamlessly with existing theme color system

---

## 4. User Stories

### 4.1 Admin User Stories

**US-PP1:** As a dealership admin, I want to upload a background image for the Finance panel so that I can visually promote our financing options.

**US-PP2:** As a dealership admin, I want to add promotional text to the Finance panel so that I can highlight specific offers like "0% APR Available".

**US-PP3:** As a dealership admin, I want to upload a background image for the Warranty panel so that I can visually communicate warranty coverage.

**US-PP4:** As a dealership admin, I want to add promotional text to the Warranty panel so that I can emphasize warranty benefits.

**US-PP5:** As a dealership admin, I want to preview images before saving so that I can ensure they look good on the homepage.

**US-PP6:** As a dealership admin, I want to remove an uploaded image so that I can change promotional visuals.

### 4.2 Public User Stories

**US-PP7:** As a website visitor, I want to see promotional panels on the homepage so that I can learn about financing and warranty options.

**US-PP8:** As a website visitor, I want to click "View Our Policy" buttons so that I can read detailed information about services.

**US-PP9:** As a mobile user, I want panels to display properly on my phone so that I can view promotional content on any device.

---

## 5. Implementation Checklist

### 5.1 Database Layer
- [x] Create migration file `008_add_promo_panels.sql`
- [x] Add four columns to `dealership` table
- [x] Add column comments for documentation
- [x] Apply migration to database

### 5.2 Backend Layer
- [x] Update `backend/routes/dealers.js` to accept new fields
- [x] Add validation for promotional text fields
- [x] Add sanitization for XSS prevention
- [x] Update `backend/db/dealers.js` update function
- [x] Add JSDoc documentation for new parameters

### 5.3 Frontend - Component
- [x] Create `frontend/src/components/PromotionalPanels.jsx`
- [x] Implement responsive grid layout
- [x] Add gradient fallback backgrounds
- [x] Implement text overlays with drop shadows
- [x] Add CTA buttons with theme color support
- [x] Add prop documentation

### 5.4 Frontend - Homepage
- [x] Import PromotionalPanels component in Home.jsx
- [x] Add component below GoogleReviewsCarousel
- [x] Pass dealership promo data as props

### 5.5 Frontend - Admin
- [x] Add state variables for promo images and text
- [x] Add upload handlers for both panels
- [x] Create UI section in DealerSettings
- [x] Add image preview functionality
- [x] Add remove image buttons
- [x] Add text input fields with character limits
- [x] Update form submission to include promo data
- [x] Add field population on data fetch

### 5.6 Documentation
- [x] Create PROMOTIONAL_PANELS_FEATURE.md
- [x] Create PROMOTIONAL_PANELS_QUICK_START.md
- [x] Create PRD_PROMOTIONAL_PANELS.md
- [ ] Create SM_PROMOTIONAL_PANELS.md
- [ ] Create ARCH_PROMOTIONAL_PANELS.md
- [ ] Update main requirements.md

---

## 6. Testing Requirements

### 6.1 Manual Testing
- [ ] Upload finance promotional image
- [ ] Upload warranty promotional image
- [ ] Enter promotional text for both panels
- [ ] Save settings and verify success
- [ ] View homepage and verify panels display
- [ ] Verify side-by-side layout on desktop
- [ ] Verify stacked layout on mobile
- [ ] Click both "View Our Policy" buttons
- [ ] Test with no images (verify gradients)
- [ ] Test with no text (verify defaults)
- [ ] Test image validation (wrong type, oversized)
- [ ] Test remove image functionality

### 6.2 Security Testing
- [ ] Verify XSS prevention in text fields
- [ ] Verify image upload validation
- [ ] Verify multi-tenant isolation
- [ ] Verify character limit enforcement

---

## 7. Recommended Image Guidelines

### 7.1 Specifications
- **Format:** JPG, PNG, or WebP
- **Size:** Maximum 5MB
- **Dimensions:** 800x600px (4:3 aspect ratio recommended)
- **Quality:** High-resolution, optimized for web

### 7.2 Content Suggestions
**Finance Panel:**
- Handshake imagery
- Car keys
- Calculator or financial documents
- Professional business settings
- Happy customers signing paperwork

**Warranty Panel:**
- Shield or protection symbols
- Mechanic working on vehicle
- Car covered by protective layer
- Checkmark or approval imagery
- Quality assurance visuals

---

## 8. Success Metrics

### 8.1 Key Performance Indicators
- **Click-through rate:** Track clicks on "View Our Policy" buttons
- **Engagement:** Monitor time spent on Finance/Warranty pages
- **Configuration rate:** Percentage of dealerships uploading custom content
- **Mobile usage:** Verify mobile traffic to policy pages

### 8.2 Quality Metrics
- **Load time:** Panel section loads within 200ms
- **Error rate:** < 1% upload failures
- **Browser compatibility:** 100% functionality across modern browsers
- **Responsive accuracy:** Proper display on all screen sizes

---

## 9. Future Enhancements

### 9.1 Potential Improvements
1. **Multiple Panel Slots:** Allow more than just Finance/Warranty panels
2. **Custom Button Text:** Let admins customize button labels
3. **Custom Links:** Allow buttons to link to any page/URL
4. **Background Color Picker:** Custom gradient colors instead of defaults
5. **Video Backgrounds:** Support video in addition to images
6. **Analytics Integration:** Built-in click tracking
7. **A/B Testing:** Test different images/text for optimization
8. **Scheduling:** Schedule promotional content changes

### 9.2 Integration Opportunities
- Integrate with marketing automation tools
- Connect to Google Analytics for event tracking
- Link to third-party financing applications
- Connect to warranty provider systems

---

## 10. Appendix

### 10.1 Related Documentation
- `PROMOTIONAL_PANELS_FEATURE.md` - Technical implementation details
- `PROMOTIONAL_PANELS_QUICK_START.md` - User guide
- `backend/db/migrations/008_add_promo_panels.sql` - Database migration
- `frontend/src/components/PromotionalPanels.jsx` - Component source

### 10.2 Dependencies
- Existing dealership settings infrastructure
- Cloudinary image upload system
- Theme color system
- Responsive layout framework (Tailwind CSS)

### 10.3 Risks and Mitigations
| Risk | Impact | Mitigation |
|------|--------|------------|
| Image quality varies | Medium | Provide image guidelines and examples |
| Text overflow on mobile | Low | Character limits and responsive design |
| Upload failures | Medium | Comprehensive validation and error messages |
| Inconsistent branding | Low | Default gradients ensure visual appeal |

---

**Document Status:** Complete  
**Next Review:** After initial user feedback  
**Change Requests:** Submit via product backlog
