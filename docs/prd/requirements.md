# Requirements

## Functional Requirements

1. **FR1:** The public website displays a home page with dealership name, tagline, and "Browse Inventory" call-to-action button
2. **FR2:** The vehicle listing page displays all active vehicles for the selected dealership in grid/list view with thumbnail, make/model/year, and price
3. **FR3:** The vehicle listing page provides case-insensitive text search with partial matching across make, model, year, and description fields, plus simple filtering (condition: new/used, sorting by price/year/mileage)
4. **FR4:** The vehicle detail page displays full vehicle information including image gallery, specifications (make, model, year, price, mileage, condition), description, and enquiry form
5. **FR5:** The enquiry form captures customer information (name, email, phone, message) and associates it with the related vehicle and dealership
6. **FR6:** The about/contact page displays dealership information including name, logo, address, phone, email, opening hours, and about text
7. **FR7:** The admin CMS provides a dealership selector (dropdown or URL parameter) allowing admin to switch between dealerships
8. **FR8:** The vehicle manager displays all vehicles for selected dealership with edit/delete actions and create/edit form for all vehicle attributes
9. **FR9:** The vehicle manager integrates image upload (Cloudinary upload widget on desktop, fallback file input on mobile) with mobile-friendly interface for uploading vehicle photos (primary image + gallery)
10. **FR10:** The dealership settings form allows editing of core dealership profile information (name, logo, address, phone, email, about text, opening hours)
11. **FR11:** Changes to dealership settings are immediately reflected on the public website
12. **FR12:** The lead inbox displays customer enquiries (name, email, phone, message, related vehicle, timestamp) filtered by selected dealership
13. **FR13:** The admin panel is protected by basic authentication (login form with credentials)
14. **FR14:** All database queries filter data by `dealershipId` to ensure multi-tenant data isolation
15. **FR15:** Public site routes use dealership identifier for routing (e.g., `/dealer/:dealershipId/inventory`)
16. **FR16:** The system must validate uploaded images (max file size 5MB, accepted formats: JPG/PNG/WebP) before upload
17. **FR17:** Vehicle status must support multiple states: Active (public-visible), Sold (public-hidden, admin-visible), Pending (public-visible with badge), Draft (admin-only)
18. **FR18:** The vehicle listing page must filter vehicles by status (default: Active + Pending only)
19. **FR19:** The admin vehicle manager must display vehicle status prominently and allow status filtering/sorting
20. **FR20:** When no vehicles exist for a dealership, the vehicle listing page displays helpful empty state: "No vehicles available yet. Check back soon!" with dealership contact info
21. **FR21:** When no vehicles exist in admin vehicle manager, display empty state with prominent "Add Your First Vehicle" call-to-action button

### Sales Request Feature (Epic 6)

22. **FR22:** The public website header displays a "Sell Your Car" navigation link accessible from all pages
23. **FR23:** The sales request form collects customer information (name, email, phone) with validation
24. **FR24:** The sales request form collects vehicle information (make, model, year, kilometers) with validation
25. **FR25:** The sales request form validates email format and year range (1900 to current year + 1)
26. **FR26:** The sales request form optionally collects additional message (max 5000 characters)
27. **FR27:** The sales request form displays success message and resets after successful submission
28. **FR28:** The admin panel displays "Sales Requests" navigation link for dealership staff
29. **FR29:** The sales requests admin page displays all requests in table format filtered by selected dealership
30. **FR30:** Sales requests are sorted by newest first (created_at DESC)
31. **FR31:** Each sales request has an editable status field (received, in progress, done) with color-coded badges
32. **FR32:** The admin interface provides Call and Email action buttons for quick customer contact
33. **FR33:** The admin interface allows deletion of sales requests with confirmation modal
34. **FR34:** The sales requests page provides date filtering (All time, Last 7 days, Last 30 days)
35. **FR35:** Long additional messages are truncated with "Show more" expansion capability
36. **FR36:** All sales request operations enforce multi-tenant data isolation via dealershipId filtering

### Homepage Promotional Panels Feature (Epic 7)

37. **FR37:** The homepage displays two promotional panels below Customer Reviews section for Finance and Warranty services
38. **FR38:** Each promotional panel displays a background image (customizable) or gradient fallback
39. **FR39:** Each promotional panel displays promotional text overlay with dark semi-transparent background for readability
40. **FR40:** Each promotional panel includes a "View Our Policy" button linking to the respective policy page (/finance or /warranty)
41. **FR41:** Promotional panels are displayed side-by-side on desktop and stacked vertically on mobile devices
42. **FR42:** The admin panel provides interface to upload background images for Finance and Warranty panels
43. **FR43:** The admin panel provides text input fields for promotional text (max 500 characters per panel)
44. **FR44:** Image uploads are validated for file type (JPG, PNG, WebP only) and size (max 5MB)
45. **FR45:** Promotional panels use dealership theme colors for CTA buttons
46. **FR46:** Promotional panels display sensible defaults when content is not configured (gradient backgrounds and default text)
47. **FR47:** All promotional panel data is stored per dealership with multi-tenant isolation enforced

## Non-Functional Requirements

1. **NFR1:** The platform must run entirely on free tiers (Cloudinary < 25GB storage/bandwidth, PostgreSQL < 1GB data, Railway/Render free tier)
2. **NFR2:** Page load time must be < 3 seconds on standard broadband (target: < 2s for listing pages)
3. **NFR3:** Images must be optimized via Cloudinary CDN with lazy loading and responsive images
4. **NFR4:** The platform must support 100+ vehicle listings per dealership without significant performance degradation
5. **NFR5:** The platform must be compatible with modern browsers (Chrome, Firefox, Safari, Edge—latest 2 versions) on desktop, tablet, and mobile
6. **NFR6:** The platform must be deployed to a live URL (Railway or Render) and remain accessible during demo period with 99%+ uptime
7. **NFR7:** Database schema must use parameterized queries to prevent SQL injection
8. **NFR8:** The platform must complete backend + frontend + deployment within 48-hour window (2-day timeline)
9. **NFR9:** The repository must use monorepo structure with `/backend` and `/frontend` folders
10. **NFR10:** Multi-tenant data must have zero cross-contamination between dealerships (enforced by `dealershipId` filtering and foreign key constraints)
11. **NFR11:** Images must be automatically resized/compressed by Cloudinary to standard dimensions (1920px max width for gallery, 400px for thumbnails)
12. **NFR12:** The admin CMS must be mobile-responsive and fully functional on tablets (iPad) and large phones (iPhone 14 Pro+)
13. **NFR13:** Core admin workflows (add vehicle, view leads, edit dealership settings) must be testable on mobile Safari and Chrome during development
14. **NFR14:** Search must return results with partial matches (e.g., "camry" matches "Toyota Camry 2015")

---
