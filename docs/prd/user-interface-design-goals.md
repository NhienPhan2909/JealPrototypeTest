# User Interface Design Goals

## Overall UX Vision

Clean, professional, and trustworthy design that builds confidence with car buyers while remaining simple enough for dealership staff to manage without technical expertise. Public-facing website prioritizes fast, friction-free vehicle discovery with clear call-to-actions for lead capture. Admin CMS emphasizes efficiency and clarity—dealership staff should be able to add a vehicle listing in under 5 minutes with minimal cognitive load.

**Design Philosophy:** "Get out of the way"—the platform should showcase vehicles and facilitate transactions without drawing attention to itself through overly complex UI or unnecessary features.

## Key Interaction Paradigms

- **Public Website:** Browse-first navigation (immediate access to inventory from home page), progressive disclosure (vehicle cards → detail page → enquiry form), mobile-optimized touch targets for filtering and CTAs
- **Admin CMS:** Dashboard-style layout with clear section navigation (Vehicles, Dealership Settings, Leads), inline editing where possible (click to edit), immediate feedback on actions (success/error toasts), single-page workflows for common tasks (add vehicle without navigating away)
- **Multi-Tenancy UX:** Prominent dealership selector in admin header with clear indication of "currently managing: [Dealership Name]" to prevent accidental cross-dealership edits

## Core Screens and Views

**Public Website:**
1. **Home Page** - Hero section with dealership branding, tagline, "Browse Inventory" CTA
2. **Vehicle Listing Page** - Grid view of vehicles with search/filter controls, sorting options
3. **Vehicle Detail Page** - Large image gallery, specs table, description, prominent enquiry form
4. **About/Contact Page** - Dealership info, map/address, hours, contact methods

**Admin CMS:**
5. **Login Screen** - Simple username/password form
6. **Admin Dashboard** - Overview with dealership selector, quick stats (total vehicles, recent leads), navigation to main sections
7. **Vehicle Manager** - Table/list of all vehicles with inline actions, "Add Vehicle" button, status indicators
8. **Vehicle Create/Edit Form** - Multi-field form with image upload, save/cancel actions
9. **Dealership Settings** - Edit form for dealership profile (name, logo, contact info, hours)
10. **Lead Inbox** - Table of enquiries with sorting/filtering, view lead details

## Accessibility: WCAG AA

Target WCAG 2.1 Level AA compliance to ensure platform is usable by dealership staff and car buyers with disabilities. Key considerations:
- Sufficient color contrast (4.5:1 for text)
- Keyboard navigation for all interactive elements
- Screen reader compatibility (semantic HTML, ARIA labels where needed)
- Form labels and error messages clearly associated with inputs
- Focus indicators visible on all focusable elements

**Rationale:** WCAG AA balances accessibility with implementation effort for 2-day timeline; AAA deferred to Phase 2.

## Branding

Minimal, neutral default theme that doesn't compete with dealership branding. Neutral color palette (whites, grays, subtle blue for CTAs) with dealership logo and name prominently displayed. Platform should feel like "the dealership's website" not "a platform hosting dealerships."

**Customization Scope (MVP):** Logo upload, dealership name/tagline customization only. Advanced theming (custom colors, fonts, layouts) deferred to post-MVP.

## Target Device and Platforms: Web Responsive

- **Primary:** Desktop browsers (Chrome, Firefox, Safari, Edge) for admin CMS and public browsing
- **Secondary:** Mobile phones (iOS Safari, Android Chrome) for public website vehicle browsing and enquiry submission
- **Tertiary:** Tablets (iPad, Android tablets) for admin CMS mobile management

**Responsive Breakpoints:**
- Mobile: < 768px (stacked layouts, simplified navigation, touch-optimized)
- Tablet: 768px - 1024px (hybrid layouts, accessible admin panel)
- Desktop: > 1024px (full multi-column layouts, optimized for productivity)

---
