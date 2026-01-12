# Epic 2: Public Dealership Website & Lead Capture

**Epic Goal:** Deliver complete visitor-facing experience that enables car buyers to discover vehicles, evaluate options, and submit enquiries. Build responsive React application with home page, vehicle inventory listing with search/filtering capabilities, detailed vehicle pages with image galleries, about/contact page, comprehensive footer with social media integration, and functional lead capture forms that integrate with the backend API. This epic delivers the core business value: generating qualified leads for dealerships.

## Story 2.1: Public Site Layout & Home Page

**As a** car buyer,
**I want** to land on a welcoming home page with clear navigation to browse vehicles,
**so that** I can quickly understand what the dealership offers and start shopping.

### Acceptance Criteria

1. React application initialized in `/frontend` folder with Create React App or Vite
2. React Router v6 installed and configured with basic routing structure
3. App layout component created with header (dealership name/logo, navigation links) and footer (comprehensive footer with contact info, hours, navigation, and social media - see Story 5.4)
4. Home page route `/` displays hero section with dealership name, tagline, and prominent "Browse Inventory" call-to-action button
5. Navigation header includes links: "Home", "Inventory", "About"
6. "Browse Inventory" button links to `/inventory` route
7. Dealership name and logo fetched from API (`GET /api/dealers/:id`) and displayed dynamically in header
8. Basic CSS styling applied (Tailwind CSS or plain CSS) for clean, professional appearance
9. App runs locally with `npm start` and displays home page without errors
10. Dealership ID is configurable (e.g., via URL parameter, environment variable, or hardcoded for prototype) so public site knows which dealership to display

## Story 2.2: Vehicle Listing Page with Grid View

**As a** car buyer,
**I want** to view all available vehicles in an organized grid layout,
**so that** I can quickly browse the dealership's inventory and compare options.

### Acceptance Criteria

1. Vehicle listing page route `/inventory` created and accessible from navigation
2. Page fetches vehicles from API (`GET /api/vehicles?dealershipId=<id>`) on component mount, then filters client-side to show only public-visible vehicles (`status='active'` or `status='pending'`)
   - **QA Note (2025-11-21):** Original AC specified `status=active` filter, but this contradicted AC10. Updated to fetch all vehicles with client-side filtering per Data Model public visibility specification.
3. Vehicles displayed in responsive grid layout (3 columns on desktop, 2 on tablet, 1 on mobile)
4. Each vehicle card displays: thumbnail image (first image from `images` array or placeholder if no images), make/model/year, price (formatted as currency), condition badge ("New" or "Used")
5. Vehicle cards are clickable and link to vehicle detail page `/inventory/:vehicleId`
6. Empty state displayed when no vehicles exist: "No vehicles available yet. Check back soon!" with dealership contact info
7. Loading state displayed while fetching vehicles from API ("Loading inventory...")
8. Error state displayed if API request fails: "Unable to load inventory. Please try again later."
9. Images use lazy loading (React library or native `loading="lazy"` attribute)
10. Vehicle status "Pending" displays badge on card ("Pending Sale")

## Story 2.3: Search & Filter Controls

**As a** car buyer,
**I want** to search and filter vehicles by keywords and condition,
**so that** I can quickly find vehicles that match my needs.

### Acceptance Criteria

1. Search input field added above vehicle grid with placeholder text "Search by make, model, or year..."
2. Search executes on text input change (debounced to avoid excessive API calls) or on "Search" button click
3. Search filters vehicles client-side (case-insensitive) across make, model, year, and title fields
4. Condition filter dropdown added with options: "All", "New", "Used"
5. Condition filter filters vehicles client-side based on `condition` field
6. Sort dropdown added with options: "Price: Low to High", "Price: High to Low", "Year: Newest", "Year: Oldest"
7. Sort applies to filtered vehicle list client-side
8. Vehicle count displayed: "Showing X vehicles" (updates based on search/filter results)
9. If search/filter returns no results, display message: "No vehicles match your search. Try different filters."
10. Clear/reset button resets all filters and search to show full inventory

## Story 2.4: Vehicle Detail Page with Image Gallery

**As a** car buyer,
**I want** to view detailed information about a specific vehicle including photos and specifications,
**so that** I can evaluate whether it meets my needs before contacting the dealership.

### Acceptance Criteria

1. Vehicle detail page route `/inventory/:vehicleId` created
2. Page fetches single vehicle from API (`GET /api/vehicles/:id?dealershipId=<id>`) using route parameter on component mount (IMPORTANT: must include dealershipId query parameter for multi-tenancy security - API v1.1+)
3. Page displays vehicle title (e.g., "2015 Toyota Camry") as H1 heading
4. Image gallery displays all vehicle images from `images` array with primary image shown large and thumbnails below (clickable to change primary image)
5. If no images exist, display placeholder image ("No photos available")
6. Image gallery supports navigation: click thumbnail to view full size, optional: previous/next arrows
7. Specifications section displays: Make, Model, Year, Price (formatted), Mileage (formatted with commas), Condition ("New" or "Used"), Status (with badge if "Pending")
8. Vehicle description displayed in full below specifications
9. "Back to Inventory" link/button returns to `/inventory` page
10. Loading state displayed while fetching vehicle ("Loading vehicle details...")
11. Error state if vehicle not found (404 from API): "Vehicle not found" with link back to inventory
12. Images optimized via Cloudinary transformations (responsive sizes, WebP format with fallback)

## Story 2.5: Enquiry Form & Lead Submission

**As a** car buyer,
**I want** to submit an enquiry about a vehicle directly from the detail page,
**so that** I can express interest and the dealership can follow up with me.

### Acceptance Criteria

1. Enquiry form component added to vehicle detail page below vehicle information
2. Form includes fields: Name (required), Email (required), Phone (required), Message (textarea, required)
3. Form has clear heading: "Interested in this vehicle? Contact us!"
4. Client-side validation: all fields required, email must be valid format, display error messages below fields if validation fails
5. "Submit Enquiry" button triggers form validation and API submission
6. On valid submission, POST request sent to `/api/leads` with `dealershipId`, `vehicle_id`, name, email, phone, message
7. Success state: form cleared, success message displayed: "Thank you! We'll contact you soon." with confirmation that enquiry was submitted
8. Error state: if API submission fails, display error message: "Unable to submit enquiry. Please try again or call us at [dealership phone]."
9. Submit button disabled during API request (prevent duplicate submissions)
10. Form auto-populates message with vehicle information: "I'm interested in the [year make model]."
11. After successful submission, option to "Submit Another Enquiry" (resets form) or "Back to Inventory"

## Story 2.6: About/Contact Page

**As a** car buyer,
**I want** to view dealership information including location, hours, and contact details,
**so that** I can visit the dealership or contact them outside of vehicle-specific enquiries.

### Acceptance Criteria

1. About/Contact page route `/about` created and accessible from navigation
2. Page fetches dealership information from API (`GET /api/dealers/:id`)
3. Page displays dealership name as H1 heading
4. "About Us" section displays dealership `about` text (2-3 paragraphs)
5. Contact information section displays: Address (formatted), Phone (clickable `tel:` link on mobile), Email (clickable `mailto:` link)
6. Hours section displays dealership `hours` (formatted as table or list)
7. Logo displayed prominently if `logo_url` exists
8. Optional: embed Google Maps iframe or static map image showing dealership address
9. "Browse Our Inventory" call-to-action button links to `/inventory`
10. Page is mobile-responsive with readable text and properly formatted contact info

## Story 2.7: Mobile Responsiveness & Browser Testing

**As a** car buyer using a mobile device,
**I want** the website to work seamlessly on my phone or tablet,
**so that** I can browse vehicles and submit enquiries from any device.

### Acceptance Criteria

1. All public website pages (home, inventory listing, vehicle detail, about) tested on mobile viewports (iPhone, Android phone sizes: 375px, 414px width)
2. Navigation menu collapses to mobile hamburger menu or simplified layout on small screens
3. Vehicle grid layout adjusts to single column on mobile (<768px width)
4. Vehicle detail page image gallery is touch-friendly (swipe to change images or tap thumbnails)
5. Enquiry form fields are full-width on mobile with adequate touch target sizes (min 44px height)
6. Text is readable without zooming (minimum 16px font size for body text)
7. Images responsive and properly sized (no horizontal scrolling, images scale to container width)
8. Call-to-action buttons have adequate size and spacing for touch interaction
9. Public website tested on mobile Safari (iOS) and Chrome (Android): all features functional
10. Public website tested on desktop browsers (Chrome, Firefox, Safari): all features functional
11. Page load performance acceptable on mobile (< 3 seconds on simulated 3G connection)
12. No JavaScript errors in browser console for any page or user interaction

## Story 2.8: Finance Page

**As a** car buyer,
**I want** to view the dealership's financing policy and options,
**so that** I can understand available financing before contacting the dealership.

### Acceptance Criteria

1. Finance page route `/finance` created and accessible from header navigation
2. Navigation header "Finance" link added between "About" and existing links
3. Page fetches dealership information from API (`GET /api/dealers/:id`)
4. Page displays "Financing Options" as H1 heading with dealership name
5. Finance policy content section displays dealership `finance_policy` text (supports multiple paragraphs, formatted with line breaks)
6. If `finance_policy` is null or empty, display default message: "Contact us to learn about our financing options. Call [phone] or submit an enquiry."
7. Contact call-to-action section displays: Phone (clickable `tel:` link), Email (clickable `mailto:` link with subject "Financing Inquiry")
8. "Browse Our Inventory" call-to-action button links to `/inventory`
9. Page layout consistent with About page styling (Story 2.6)
10. Page is mobile-responsive with readable text and properly formatted content
11. Loading state displayed while fetching dealership data
12. Error state handled if API request fails

## Story 2.9: Warranty Page

**As a** car buyer,
**I want** to view the dealership's warranty policy and coverage information,
**so that** I can understand warranty terms before making a purchase decision.

### Acceptance Criteria

1. Warranty page route `/warranty` created and accessible from header navigation
2. Navigation header "Warranty" link added after "Finance" link
3. Page fetches dealership information from API (`GET /api/dealers/:id`)
4. Page displays "Warranty Information" as H1 heading with dealership name
5. Warranty policy content section displays dealership `warranty_policy` text (supports multiple paragraphs, formatted with line breaks)
6. If `warranty_policy` is null or empty, display default message: "Contact us to learn about our warranty coverage. Call [phone] or submit an enquiry."
7. Contact call-to-action section displays: Phone (clickable `tel:` link), Email (clickable `mailto:` link with subject "Warranty Inquiry")
8. "Browse Our Inventory" call-to-action button links to `/inventory`
9. Page layout consistent with About page styling (Story 2.6)
10. Page is mobile-responsive with readable text and properly formatted content
11. Loading state displayed while fetching dealership data
12. Error state handled if API request fails

---
