# 10. Development Workflow

## Part 1: Local Development Setup (30-45 minutes)

**Step 1: Prerequisites Check**

```bash
node --version    # Should be v18 or higher
npm --version     # Should be v9 or higher
git --version     # Any recent version
```

**Note:** No local PostgreSQL required. Railway provides the database.

**Step 2-7:** Create project structure, install dependencies (see Section 9 for commands)

**Step 8: Set Up Railway PostgreSQL Database**

1. Create Railway account: https://railway.app/ (free, sign up with GitHub)
2. Create new project → **"Provision PostgreSQL"**
3. Copy DATABASE_URL from PostgreSQL service → Connect tab
4. Paste into `.env` file

**Step 9: Run Database Migrations**

```bash
# Install Railway CLI
npm install -g @railway/cli
railway login
railway link

# Run migrations
railway run psql $DATABASE_URL < backend/db/schema.sql
railway run psql $DATABASE_URL < backend/db/seed.sql

# Verify
railway run psql $DATABASE_URL -c "SELECT COUNT(*) FROM dealership;"
```

**Step 10: Start Development Servers**

```bash
npm run dev
```

**Expected:** Backend on port 5000, Frontend on port 3000

## Part 1.5: Story Implementation Workflow (Mandatory for Dev Agents)

**CRITICAL: Before implementing any story that requires backend API endpoints, follow this workflow:**

### Step 1: Review Backend Routes Status (MANDATORY)

**Document:** `docs/BACKEND_ROUTES_IMPLEMENTATION_STATUS.md`

**Purpose:** Check if required backend routes already exist before creating duplicates

**Action Items:**
1. ✅ **Read the Backend Routes Status document** - Always loaded automatically via devLoadAlwaysFiles
2. ✅ **Identify required endpoints** - List what API endpoints your story needs
3. ✅ **Check if routes exist** - Look up each endpoint in the status document
4. ✅ **Decide action:**
   - **If route EXISTS:** Use it directly, add your story to "Used By Stories" list
   - **If route MISSING:** Plan to create new route file and DB module

### Step 2: Verify API Specifications

**Document:** `docs/architecture/api-specification.md`

**Action Items:**
1. ✅ Check request/response format for existing routes
2. ✅ Verify security requirements (multi-tenancy, authentication)
3. ✅ Review example implementations

### Step 3: Implement Story

Follow standard development workflow with awareness of existing backend infrastructure.

**Example: Story 3.3 (Vehicle Manager)**

**Required Endpoints:**
- GET /api/vehicles?dealershipId=X (list vehicles)
- POST /api/vehicles (create vehicle)
- PUT /api/vehicles/:id?dealershipId=X (update vehicle)
- DELETE /api/vehicles/:id?dealershipId=X (delete vehicle)

**Backend Routes Status Check:**
- ✅ All 4 endpoints exist in `backend/routes/vehicles.js`
- ✅ Multi-tenancy security (SEC-001) already implemented
- ✅ Input validation and sanitization already implemented
- ✅ Database queries in `backend/db/vehicles.js`

**Conclusion:** NO backend development needed, just build frontend UI to call existing APIs

---

## Part 2: Testing Core Flows

See Section 10 (Development Workflow) in full document for detailed testing checklists.

## Part 3: Railway Production Deployment

See Section 10 (Development Workflow) in full document for step-by-step deployment guide.

---
