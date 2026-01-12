# 3. Tech Stack

**This is the DEFINITIVE technology selection for the entire project. All development must use these exact versions.**

## Technology Stack Table

| Category | Technology | Version | Purpose | Rationale |
|----------|-----------|---------|---------|-----------|
| **Frontend Language** | JavaScript | ES2022 | React development | No TypeScript - saves setup time, type complexity; plain JS sufficient for 2-day MVP; add TS in Phase 2 if needed |
| **Frontend Framework** | React | 18.2+ | UI rendering, SPA | Industry standard, huge documentation, fast to iterate, CRA/Vite scaffolding gets you running in 5 minutes |
| **UI Component Library** | None | - | N/A | No component library (Material-UI, Ant Design) - they're too heavyweight; build custom components with Tailwind |
| **State Management** | React Context + useState | Built-in | Global state (auth, dealership selector) | Built-in, zero dependencies, sufficient for small app; avoid Redux/Zustand overhead |
| **CSS Framework** | Tailwind CSS | 3.4+ | Rapid styling | Utility-first = fast iteration, no context switching to CSS files, well-documented, zero runtime JS; alternative: plain CSS modules (viable but slower) |
| **Theming System** | CSS Custom Properties | Native CSS | Dynamic theme colors | Runtime theme switching without rebuild; enables per-dealership brand colors; automatically calculates lighter/darker shades |
| **Backend Language** | Node.js | 18 LTS | Server runtime | Long-term support, Railway/Render default, stable, widely documented |
| **Backend Framework** | Express | 4.18+ | REST API server | Minimal, unopinionated, battle-tested, dead simple to deploy; no NestJS/Fastify complexity |
| **API Style** | REST | - | Client-server communication | JSON over HTTP, testable with curl/Postman, universally understood; no GraphQL/tRPC learning curve |
| **Email Service** | Nodemailer | 7.0+ | Email notifications | Industry standard SMTP client, supports HTML/text emails, flexible transports; used for new lead notifications to dealerships |
| **Database** | PostgreSQL | 14+ | Relational data storage | Foreign keys enforce multi-tenancy integrity, ACID guarantees, free tier on Railway/Render, SQL is standard |
| **Database Client** | pg (node-postgres) | 8.11+ | PostgreSQL driver | Official Postgres client, parameterized queries (SQL injection protection), no ORM overhead (Prisma/Sequelize adds complexity) |
| **Cache** | None | - | N/A | Not needed for MVP; small data volumes, free tier DB is fast enough; defer Redis to Phase 2 |
| **File Storage** | Cloudinary | Free tier | Image uploads, CDN | Upload widget = no backend file handling, auto-optimization (WebP, resize), 25GB free, CDN included |
| **Authentication** | express-session + cookie | express-session 1.17+ | Admin login | Simple session-based auth, sufficient for prototype; no JWT complexity; upgrade to Passport.js/bcrypt in Phase 2 |
| **Frontend Testing** | None (manual) | - | N/A | Manual testing only for 2-day MVP; Jest + React Testing Library deferred to Phase 2 |
| **Backend Testing** | None (manual) | - | N/A | Manual Postman/curl testing; Jest/Supertest deferred to Phase 2 |
| **E2E Testing** | None (manual) | - | N/A | Manual browser testing; Playwright/Cypress deferred to Phase 2 |
| **Build Tool** | Vite | 5.0+ | Frontend build/dev server | Faster than CRA, minimal config, HMR out of box; alternative: CRA (more stable but slower) |
| **Package Manager** | npm | 9+ | Dependency management | Built-in with Node, workspaces support for monorepo, universally documented |
| **IaC Tool** | None | - | N/A | Railway/Render use platform UI for config; no Terraform/Pulumi needed for free tier single-service deployment |
| **CI/CD** | Railway/Render Auto-Deploy | Platform feature | Git-based deployment | Push to main = auto-deploy; zero config needed; GitHub Actions deferred to Phase 2 |
| **Monitoring** | Railway Logs | Platform feature | Error visibility | Built-in log viewer; Sentry/Datadog deferred to Phase 2 |
| **Logging** | console.log + Morgan | morgan 1.10+ | HTTP request logging | Morgan logs all requests (method, path, status, time); console.log for errors; structured logging (Winston) deferred |
| **Routing** | React Router | 6.20+ | Client-side navigation | SPA routing, declarative, nested routes for admin panel, excellent docs |
| **HTTP Client** | fetch API | Built-in | API calls from React | Native browser API, zero dependencies, adequate for simple GET/POST/PUT/DELETE; axios unnecessary |
| **Forms** | React Hook Form | 7.49+ | Form state management | Reduces boilerplate for 10+ field vehicle form, built-in validation, uncontrolled inputs (better performance); faster than manual controlled components |
| **Environment Config** | dotenv | 16.3+ | Local .env file parsing | Standard for local dev; Railway/Render use platform env vars in production |
| **Dev Server** | Nodemon | 3.0+ | Backend hot reload | Auto-restart on file changes; essential for rapid iteration |
| **CORS** | cors (Express middleware) | 2.8+ | Dev mode API access | Allow React dev server (localhost:3000) to call Express (localhost:5000); not needed in production (same origin) |

## Key Technology Decisions

**JavaScript over TypeScript:**
TypeScript adds 2-4 hours setup (tsconfig, type definitions, build pipeline). Plain JS with JSDoc comments for critical functions is faster. Add TS in Phase 2 when codebase stabilizes.

**Tailwind over CSS Modules:**
Tailwind's utility-first approach eliminates CSS file creation and naming decisions. `className="flex items-center gap-4"` is faster than creating `.container { display: flex; ... }` and importing stylesheets. Tradeoff: class clutter in JSX (acceptable for 2-day sprint).

**CSS Custom Properties for Theming:**
CSS variables enable runtime theme customization without rebuilding the application. Each dealership can set their brand color in the CMS, which dynamically updates `--theme-color`, `--theme-color-dark`, and `--theme-color-light` variables. This approach was chosen over Tailwind's JIT compilation because it supports true runtime theming - dealerships can change their brand color and see results immediately without a rebuild. The system automatically calculates darker (15% darker for hover states) and lighter (90% lighter for backgrounds) shades using JavaScript RGB manipulation.

**React Hook Form over Controlled Components:**
The vehicle create/edit form has 10+ fields (make, model, year, price, mileage, condition, status, title, description, images). Manual controlled components require `useState` + `onChange` for each field = 20+ lines of boilerplate. RHF reduces this to `register("make")` = 1 line per field. Saves ~2 hours over 48-hour timeline.

**pg over Prisma/Sequelize:**
Raw SQL with parameterized queries (`db.query('SELECT * FROM vehicles WHERE dealership_id = $1', [id])`) is explicit and debuggable. Prisma requires schema definition, migration setup, and code generation. Sequelize has learning curve for associations. Raw SQL is faster to write when you know SQL.

**Express Session over JWT:**
Session cookies are simpler than JWT (no token expiry logic, refresh tokens, local storage security concerns). For single-server deployment, session store in memory is fine (stateful but acceptable for prototype).

**Vite over Create React App:**
Vite's dev server starts in <1 second vs CRA's 10-30 seconds. HMR is instant. Build times are faster. Minimal config (`npm create vite@latest`). CRA is deprecated in favor of Next.js, but Next.js is overkill for SPA.

**Nodemailer for Email Notifications:**
Nodemailer is the industry-standard Node.js email library with 16M+ weekly downloads. It supports any SMTP provider (Gmail, SendGrid, AWS SES, etc.) without vendor lock-in. For the MVP, SMTP configuration is optional - the application gracefully handles missing email configuration in development mode. Email sending is non-blocking, ensuring lead creation succeeds even if email delivery fails. This keeps the user-facing functionality robust while providing helpful notifications to dealerships when properly configured.

**No ORM, No GraphQL, No Microservices:**
Every abstraction layer adds time. Direct SQL, REST, and monolith are the fastest path to working software.

## Dependencies Installation Commands

**Backend (`backend/package.json`):**
```bash
npm install express pg dotenv cloudinary morgan cors express-session cookie-parser nodemailer
npm install --save-dev nodemon
```

**Frontend (`frontend/package.json`):**
```bash
npm install react react-dom react-router-dom react-hook-form
npm install --save-dev vite @vitejs/plugin-react tailwindcss postcss autoprefixer
```

**Root (`package.json`):**
```bash
npm install --save-dev concurrently
```

---
