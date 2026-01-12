# Technical Assumptions

## Repository Structure: Monorepo

**Decision:** Monorepo with `/backend` and `/frontend` folders, shared root `package.json` for dev scripts.

**Rationale:**
- Simplifies development workflow (single `git clone`, unified dependency management)
- Enables running backend + frontend in parallel via `concurrently` for local dev
- Reduces deployment complexity (single Git repository for Railway/Render auto-deploy)
- Appropriate for small team/solo developer and 2-day timeline
- Avoids overhead of polyrepo coordination (multiple repos, separate deployments, version alignment)

**Implementation Details:**
- Root `package.json` with scripts: `npm run dev` (runs both backend and frontend), `npm run build` (builds frontend)
- Backend: Express server in `/backend` with API routes, database models, Cloudinary integration
- Frontend: React app in `/frontend` (public site + admin panel in single codebase, separated by routes)

## Service Architecture: Monolith (Unified Full-Stack Deployment)

**Decision:** Single service deployment combining backend API and frontend static build.

**Rationale:**
- **Speed:** Fastest path to deployment within 2-day timeline (no microservices orchestration, no separate frontend hosting)
- **Cost:** Fits within single free-tier hosting instance (Railway/Render)
- **Simplicity:** Backend serves built React app via `express.static('frontend/build')` in production—no CORS complexity, no separate domain coordination
- **Scalability:** Monolith sufficient for 2-5 dealerships with <100 vehicles each; can refactor to microservices post-MVP if needed

**Architecture Components:**
- **RESTful API Endpoints:** `/api/vehicles`, `/api/dealers`, `/api/leads`, `/api/upload` (Cloudinary integration), `/api/auth` (basic login)
- **Multi-Tenancy:** All vehicle/lead queries scoped by `dealershipId` filtering in API layer
- **Static Asset Serving:** Backend serves frontend build in production; React dev server proxy to backend in development

**Deferred for Post-MVP:**
- Microservices architecture (separate inventory service, lead service, etc.)
- Serverless functions (AWS Lambda, Vercel Functions)
- Separate frontend hosting (Vercel, Netlify)

## Testing Requirements: Manual Testing Only (Prototype)

**Decision:** Developer-led manual testing during 2-day build; no automated test suites for MVP.

**Rationale:**
- **Timeline Constraint:** Writing unit/integration/e2e tests would consume 30-50% of 48-hour budget—unacceptable for prototype
- **Risk Mitigation:** Manual testing sufficient to validate core user journeys (visitor → enquiry submission; admin → add vehicle → view on public site)
- **Post-MVP Plan:** Add automated tests in Phase 2 (Jest for backend API, React Testing Library for components, Playwright for e2e)

**Manual Testing Checklist (Day 2):**
- Multi-tenancy validation: Create/edit/delete vehicles for Dealership A and B, verify no cross-contamination
- End-to-end public journey: Browse listing → view detail → submit enquiry → verify in admin lead inbox
- End-to-end admin journey: Add vehicle with photos → verify on public site, edit dealership settings → verify updates live
- Mobile testing: Test public site and admin panel on iPhone/iPad, verify responsive layouts and image upload
- Browser testing: Chrome, Firefox, Safari (desktop and mobile)

**Test Data Seeding:**
- Seed 2 dealerships with 5-10 sample vehicles each (diverse makes/models/prices)
- Include edge cases: vehicle with 1 photo vs. 10 photos, very long descriptions, no description, sold vehicles

## Additional Technical Assumptions and Requests

**Frontend Stack:**
- **React 18+** with functional components and hooks (no class components)
- **React Router v6** for client-side routing (public routes: `/`, `/inventory`, `/inventory/:id`, `/about`; admin routes: `/admin`, `/admin/vehicles`, `/admin/settings`, `/admin/leads`)
- **Styling:** Tailwind CSS (utility-first framework for rapid UI development) OR plain CSS modules (if Tailwind setup time is concern)
- **Forms:** Controlled components with basic validation; React Hook Form if it accelerates development without complexity
- **HTTP Client:** `fetch` API (built-in) or `axios` for API calls
- **State Management:** React Context + useState/useReducer for global state (dealership selector, auth token); avoid Redux/Zustand for MVP simplicity

**Backend Stack:**
- **Node.js v18+ LTS** (latest stable with long-term support)
- **Express.js** for REST API server
- **pg (node-postgres)** for PostgreSQL database client with parameterized queries (SQL injection protection)
- **Cloudinary Node.js SDK** for image upload handling (or frontend-only Cloudinary upload widget)
- **dotenv** for environment variable management (`.env` file for local dev, platform env vars for production)
- **nodemon** for development hot-reload (restart server on file changes)
- **bcrypt** for password hashing (if implementing proper auth beyond hard-coded credentials)

**Database:**
- **PostgreSQL 14+** (relational database for structured data, multi-tenancy, data integrity)
- **Schema Entities:** `Dealership` (id, name, logo_url, address, phone, email, hours, about), `Vehicle` (id, dealership_id FK, make, model, year, price, mileage, condition, status, title, description, images JSON/array), `Lead` (id, dealership_id FK, vehicle_id FK nullable, name, email, phone, message, created_at)
- **Indexes:** `dealership_id` on Vehicle and Lead tables for query performance
- **Hosting:** Supabase free tier, Railway Postgres add-on, or Render Postgres (whichever provides easiest setup + free tier)

**Image Storage & CDN:**
- **Cloudinary Free Tier** (25GB storage, 25GB bandwidth/month)
- **Upload Method:** Cloudinary upload widget (frontend integration) with server-side signature generation for security
- **Image Transformations:** Automatic resizing (1920px max width for gallery, 400px for thumbnails), format optimization (WebP with JPG fallback), lazy loading
- **Fallback Plan:** If Cloudinary widget integration stalls, use simple file upload to local backend storage temporarily (migrate to Cloudinary post-demo)

**Hosting & Deployment:**
- **Primary Choice:** Railway free tier (unified backend + frontend + Postgres, Git-based auto-deploy, generous free tier limits)
- **Alternative:** Render free tier (similar capabilities, slightly different UX)
- **SSL/HTTPS:** Provided automatically by Railway/Render
- **Environment Variables:** Database URL, Cloudinary API credentials, admin auth secret stored as platform env vars
- **Build Process:** `npm run build` in frontend generates production build → backend serves static files

**Security Practices:**
- Parameterized SQL queries (prevent SQL injection)
- Input validation on API endpoints (sanitize user input)
- CORS configuration (allow frontend origin in dev, not needed for unified production deployment)
- Environment variables for secrets (never commit `.env` to Git)
- HTTPS enforcement (platform-provided)
- **Deferred for Post-MVP:** Rate limiting, CSRF tokens, advanced auth (OAuth, SSO), audit logging

**Development Workflow:**
- **Version Control:** Git with GitHub/GitLab repository
- **Branching Strategy:** Simple `main` branch for prototype (no feature branches for 2-day sprint)
- **Deployment:** Git push to `main` triggers auto-deploy on Railway/Render
- **Local Development:** `npm run dev` starts both backend (port 5000) and frontend (port 3000), frontend proxies API requests to backend

**Key Technical Constraints for Architect:**

1. **MUST use free/open-source technologies only** (no licensed software, paid APIs, or commercial platforms)
2. **MUST complete within 48-hour timeline** (any tech choice that adds >2 hours setup time should be reconsidered)
3. **MUST support multi-tenancy via `dealershipId` filtering** (all data access layers must enforce this)
4. **MUST be deployable to Railway or Render free tier** (architecture must fit within free tier CPU/memory/storage limits)
5. **MUST be production-viable** (prototype architecture should not require complete rewrite for real dealership deployment)

---
