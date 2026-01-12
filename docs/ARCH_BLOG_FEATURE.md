# Blog Feature - Technical Architecture Document

**Document Owner:** Solution Architect  
**Created:** 2025-12-31  
**Last Updated:** 2025-12-31  
**Status:** Implemented  
**Version:** 1.0

---

## 1. Architecture Overview

### 1.1 System Context
The blog feature is a full-stack module within the multi-tenant dealership CMS platform. It follows the existing architectural patterns and integrates seamlessly with current authentication, navigation, and data management systems.

### 1.2 Architectural Principles
- **Multi-Tenancy First**: All data operations enforce dealership isolation
- **Separation of Concerns**: Clear boundaries between presentation, business logic, and data layers
- **RESTful API Design**: Consistent endpoint patterns following existing conventions
- **Security by Design**: Authentication and authorization at every layer
- **Mobile-First Responsive**: Public pages optimized for all device sizes
- **Progressive Enhancement**: Core functionality works, enhanced features add value

---

## 2. System Architecture

### 2.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Frontend (React)                         │
├──────────────────────┬──────────────────────────────────────┤
│   Public Pages       │         Admin Pages                   │
│   - Blog.jsx         │         - BlogList.jsx                │
│   - BlogPost.jsx     │         - BlogForm.jsx                │
└──────────────────────┴──────────────────────────────────────┘
                              │
                              ▼
                    ┌─────────────────┐
                    │   API Routes    │
                    │  /api/blogs/*   │
                    └─────────────────┘
                              │
                    ┌─────────┴─────────┐
                    ▼                   ▼
          ┌──────────────┐    ┌──────────────┐
          │   Auth       │    │   Business   │
          │  Middleware  │    │    Logic     │
          └──────────────┘    └──────────────┘
                              │
                              ▼
                    ┌─────────────────┐
                    │  Data Access    │
                    │   blogs.js      │
                    └─────────────────┘
                              │
                              ▼
                    ┌─────────────────┐
                    │   PostgreSQL    │
                    │   blog_post     │
                    └─────────────────┘
```

### 2.2 Technology Stack

**Frontend:**
- React 18.x
- React Router v6
- React Hook Form (form management)
- Tailwind CSS (styling)
- Context API (state management)

**Backend:**
- Node.js 22.x
- Express.js 4.x
- PostgreSQL 14+
- express-session (authentication)

**Infrastructure:**
- Docker (database)
- Cloudinary (image storage)
- Session-based authentication

---

## 3. Database Architecture

### 3.1 Schema Design

```sql
CREATE TABLE blog_post (
  -- Primary Key
  id SERIAL PRIMARY KEY,
  
  -- Multi-Tenancy
  dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE,
  
  -- Content Fields
  title VARCHAR(255) NOT NULL,
  slug VARCHAR(255) NOT NULL,
  content TEXT NOT NULL,
  excerpt TEXT,
  
  -- Media
  featured_image_url TEXT,
  
  -- Metadata
  author_name VARCHAR(255) NOT NULL,
  status VARCHAR(20) DEFAULT 'draft' CHECK (status IN ('draft', 'published', 'archived')),
  
  -- Timestamps
  published_at TIMESTAMP,
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW(),
  
  -- Constraints
  UNIQUE(dealership_id, slug)
);
```

### 3.2 Indexes

```sql
-- Multi-tenancy filtering
CREATE INDEX idx_blog_post_dealership_id ON blog_post(dealership_id);

-- Public query optimization
CREATE INDEX idx_blog_post_status ON blog_post(status);
CREATE INDEX idx_blog_post_published_at ON blog_post(published_at DESC);

-- Composite index for common query pattern
CREATE INDEX idx_blog_post_dealership_status ON blog_post(dealership_id, status);
```

**Index Rationale:**
- `idx_blog_post_dealership_id`: Critical for multi-tenant queries (every query filters by dealership)
- `idx_blog_post_status`: Speeds up published post filtering on public site
- `idx_blog_post_published_at`: Optimizes chronological sorting
- `idx_blog_post_dealership_status`: Composite index for most common query pattern

### 3.3 Data Relationships

```
dealership (1) ──< (many) blog_post
  ON DELETE CASCADE
  
- Each blog post belongs to exactly one dealership
- Deleting a dealership removes all its blog posts
- Orphaned blog posts are not possible
```

### 3.4 Data Constraints

**Business Rules Enforced at Database Level:**
1. `dealership_id` is required (NOT NULL)
2. Foreign key ensures dealership exists
3. Status must be one of: draft, published, archived (CHECK constraint)
4. Slug must be unique per dealership (UNIQUE constraint on dealership_id + slug)
5. Title, content, author_name are required (NOT NULL)

---

## 4. API Architecture

### 4.1 Endpoint Design

**Public Endpoints (No Auth):**
```
GET  /api/blogs/published?dealershipId={id}
     → Returns: Array of published blog posts
     → Sorted by: published_at DESC
     
GET  /api/blogs/slug/{slug}?dealershipId={id}
     → Returns: Single blog post by slug
     → Filters: status = 'published' AND dealership_id = {id}
```

**Admin Endpoints (Auth Required):**
```
GET    /api/blogs?dealershipId={id}
       → Returns: All blog posts for dealership
       → Sorted by: created_at DESC
       
GET    /api/blogs/{id}?dealershipId={id}
       → Returns: Single blog post by ID
       → Validates: Ownership
       
POST   /api/blogs
       Body: { dealership_id, title, slug, content, ... }
       → Creates new blog post
       → Validates: Required fields, slug uniqueness
       
PUT    /api/blogs/{id}?dealershipId={id}
       → Updates blog post
       → Validates: Ownership, slug uniqueness
       
DELETE /api/blogs/{id}?dealershipId={id}
       → Deletes blog post
       → Validates: Ownership
```

### 4.2 Request/Response Patterns

**Standard Blog Post Response:**
```json
{
  "id": 1,
  "dealership_id": 1,
  "title": "Welcome to Our Blog",
  "slug": "welcome-to-our-blog",
  "content": "Full content here...",
  "excerpt": "Short summary...",
  "featured_image_url": "https://cloudinary.com/...",
  "author_name": "John Doe",
  "status": "published",
  "published_at": "2025-12-31T10:00:00.000Z",
  "created_at": "2025-12-31T09:00:00.000Z",
  "updated_at": "2025-12-31T10:00:00.000Z"
}
```

**Error Response:**
```json
{
  "error": "Descriptive error message"
}
```

### 4.3 Authentication Flow

```
Client Request
     ↓
Session Middleware
     ↓
Check req.session.isAuthenticated
     ↓
   true ──→ requireAuth passes ──→ Route Handler
     ↓
   false ──→ 401 Unauthorized
```

### 4.4 Multi-Tenancy Enforcement

**Pattern for POST (Create):**
```javascript
// Frontend sends dealership_id in body
const blogData = {
  dealership_id: selectedDealership.id,
  title: "...",
  content: "..."
};

// Backend validates and uses it
const dealershipId = parseInt(req.body.dealership_id, 10);
await blogsDb.create(dealershipId, blogData);
```

**Pattern for PUT/DELETE (Modify):**
```javascript
// Frontend sends dealershipId in query string
fetch(`/api/blogs/${id}?dealershipId=${selectedDealership.id}`)

// Backend validates ownership
const blog = await blogsDb.getById(id, dealershipId);
if (!blog) return 404; // Not found or not owned
await blogsDb.update(id, dealershipId, updates);
```

---

## 5. Frontend Architecture

### 5.1 Component Structure

```
src/
├── pages/
│   ├── admin/
│   │   ├── BlogList.jsx          # List view with CRUD actions
│   │   └── BlogForm.jsx          # Create/Edit form
│   └── public/
│       ├── Blog.jsx              # Public blog listing
│       └── BlogPost.jsx          # Single post view
├── components/
│   ├── Header.jsx                # Updated with Blog link
│   ├── AdminHeader.jsx           # Updated with Blog Manager link
│   └── NavigationButton.jsx     # Renders navigation items
├── context/
│   ├── AdminContext.jsx          # Dealership selection
│   └── DealershipContext.jsx    # Public site context
└── utils/
    ├── defaultNavigation.js      # Added Blog nav item
    └── iconMapper.js             # Added FaNewspaper icon
```

### 5.2 State Management

**Admin Context (AdminContext):**
```javascript
{
  selectedDealership: {
    id: 1,
    name: "Acme Auto Sales",
    ...
  },
  isAuthenticated: true
}
```

**Dealership Context (DealershipContext):**
```javascript
{
  currentDealershipId: 1,
  currentDealership: { ... }
}
```

**Local Component State:**
```javascript
// BlogList.jsx
const [blogs, setBlogs] = useState([]);
const [loading, setLoading] = useState(true);
const [statusFilter, setStatusFilter] = useState('All');

// BlogForm.jsx
const [featuredImage, setFeaturedImage] = useState('');
const [uploading, setUploading] = useState(false);
```

### 5.3 Routing Architecture

```javascript
// App.jsx
<Routes>
  {/* Public routes - wrapped in Layout */}
  <Route path="/" element={<Layout />}>
    <Route path="blog" element={<Blog />} />
    <Route path="blog/:slug" element={<BlogPost />} />
  </Route>
  
  {/* Admin routes - protected */}
  <Route path="/admin" element={<ProtectedRoute />}>
    <Route path="blogs" element={<BlogList />} />
    <Route path="blogs/new" element={<BlogForm />} />
    <Route path="blogs/edit/:id" element={<BlogForm />} />
  </Route>
</Routes>
```

**Layout Component Pattern:**
```javascript
// Layout wraps public pages with Header/Footer
<Layout>
  <Header />
  <main>
    <Outlet /> {/* Blog.jsx or BlogPost.jsx renders here */}
  </main>
  <Footer />
</Layout>
```

### 5.4 Form Management

**React Hook Form Pattern:**
```javascript
const {
  register,
  handleSubmit,
  formState: { errors },
  reset,
  setValue,
  watch
} = useForm({
  defaultValues: { /* ... */ }
});

// Auto-generate slug from title
const watchTitle = watch('title');
useEffect(() => {
  if (watchTitle && !isEditMode) {
    const slug = generateSlug(watchTitle);
    setValue('slug', slug);
  }
}, [watchTitle]);
```

---

## 6. Security Architecture

### 6.1 Security Layers

```
┌─────────────────────────────────────────┐
│  Layer 1: Authentication                 │
│  - Session-based auth                    │
│  - requireAuth middleware                │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│  Layer 2: Input Validation               │
│  - Required field checks                 │
│  - Field length validation               │
│  - Enum validation (status)              │
│  - Slug format validation                │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│  Layer 3: Multi-Tenant Isolation         │
│  - dealershipId required on all ops      │
│  - Ownership verification                │
│  - Database-level constraints            │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│  Layer 4: SQL Injection Prevention       │
│  - Parameterized queries                 │
│  - No string concatenation               │
└─────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│  Layer 5: XSS Prevention                 │
│  - Input sanitization (future)           │
│  - React's built-in escaping            │
│  - Content-Security-Policy (future)      │
└─────────────────────────────────────────┘
```

### 6.2 Authentication Implementation

**Middleware:**
```javascript
// backend/middleware/auth.js
function requireAuth(req, res, next) {
  if (!req.session.isAuthenticated) {
    return res.status(401).json({ error: 'Authentication required' });
  }
  next();
}
```

**Usage:**
```javascript
router.post('/api/blogs', requireAuth, async (req, res) => {
  // Only authenticated users reach here
});
```

### 6.3 Multi-Tenancy Security

**Pattern 1: Ownership Verification**
```javascript
// Before update/delete, verify ownership
const existingBlog = await blogsDb.getById(id, dealershipId);
if (!existingBlog) {
  return res.status(404).json({ error: 'Not found' });
}
// Proceed with operation
```

**Pattern 2: Database-Level Isolation**
```javascript
// All queries include dealership_id in WHERE clause
const query = 'SELECT * FROM blog_post WHERE dealership_id = $1';
const result = await pool.query(query, [dealershipId]);
```

**Pattern 3: Unique Constraints**
```sql
-- Slug unique per dealership (not globally)
UNIQUE(dealership_id, slug)
```

### 6.4 Input Validation

**Field Validation:**
```javascript
const FIELD_LIMITS = {
  title: 255,
  slug: 255,
  content: 50000,
  excerpt: 1000,
  author_name: 255
};

const VALID_STATUSES = ['draft', 'published', 'archived'];
```

**Slug Validation:**
```javascript
// Must match: lowercase letters, numbers, hyphens only
const slugPattern = /^[a-z0-9-]+$/;
if (!slugPattern.test(slug)) {
  return res.status(400).json({ error: 'Invalid slug format' });
}
```

---

## 7. Data Flow

### 7.1 Create Blog Post Flow

```
Admin UI (BlogForm.jsx)
    ↓ User fills form
    ↓ Sets status to 'published'
    ↓ Clicks "Create Blog Post"
    ↓
Frontend Logic
    ↓ Validates required fields (React Hook Form)
    ↓ Generates slug from title
    ↓ Adds dealership_id from AdminContext
    ↓ POST /api/blogs
    ↓
Backend API
    ↓ requireAuth middleware
    ↓ Validates: dealership_id, required fields
    ↓ Checks slug uniqueness
    ↓ Calls blogsDb.create()
    ↓
Database Layer
    ↓ INSERT INTO blog_post
    ↓ Sets published_at = NOW() (if status=published)
    ↓ Returns created record
    ↓
Response
    ↓ 201 Created with blog post object
    ↓ Frontend shows success message
    ↓ Redirects to /admin/blogs
```

### 7.2 View Public Blog Flow

```
Public Website Visitor
    ↓ Navigates to /blog
    ↓
Blog.jsx Component
    ↓ useEffect on mount
    ↓ Gets currentDealershipId from context
    ↓ GET /api/blogs/published?dealershipId={id}
    ↓
Backend API (No Auth Required)
    ↓ Validates dealershipId parameter
    ↓ Calls blogsDb.getPublishedByDealership()
    ↓
Database Query
    ↓ SELECT * FROM blog_post
    ↓ WHERE dealership_id = $1 AND status = 'published'
    ↓ ORDER BY published_at DESC
    ↓
Response
    ↓ Returns array of published posts
    ↓ Frontend renders grid of blog cards
    ↓
User Clicks Post
    ↓ Navigates to /blog/{slug}
    ↓
BlogPost.jsx Component
    ↓ GET /api/blogs/slug/{slug}?dealershipId={id}
    ↓ Backend filters by published status
    ↓ Returns single post
    ↓ Frontend displays full content
```

---

## 8. Performance Considerations

### 8.1 Database Optimization

**Indexes:**
- All queries benefit from `idx_blog_post_dealership_id`
- Public queries use `idx_blog_post_dealership_status` (composite)
- Chronological sorting uses `idx_blog_post_published_at`

**Query Patterns:**
```sql
-- Optimized: Uses composite index
SELECT * FROM blog_post 
WHERE dealership_id = $1 AND status = 'published'
ORDER BY published_at DESC;

-- Index coverage: idx_blog_post_dealership_status + idx_blog_post_published_at
```

### 8.2 Frontend Optimization

**Code Splitting:**
- Admin pages lazy-loaded (protected routes)
- Public pages part of main bundle
- Icons imported on-demand

**State Management:**
- Local state for component-specific data
- Context for shared dealership info
- No unnecessary re-renders

**Image Optimization:**
- Cloudinary automatic optimization
- Responsive image sizing (future enhancement)

### 8.3 Caching Strategy (Future)

**Potential Improvements:**
- Redis cache for published posts
- ETags for conditional requests
- Browser caching headers
- CDN for static assets

---

## 9. Error Handling

### 9.1 Error Handling Strategy

**Backend Pattern:**
```javascript
try {
  // Operation
  const blog = await blogsDb.create(dealershipId, blogData);
  res.status(201).json(blog);
} catch (err) {
  console.error('Error creating blog:', err);
  
  // Handle specific errors
  if (err.code === '23505') { // Unique constraint violation
    return res.status(400).json({ 
      error: 'A blog post with this slug already exists' 
    });
  }
  
  // Generic error
  res.status(500).json({ error: 'Failed to create blog post' });
}
```

**Frontend Pattern:**
```javascript
try {
  const response = await fetch(url, options);
  
  if (!response.ok) {
    const errorData = await response.json();
    throw new Error(errorData.error || 'Operation failed');
  }
  
  // Success handling
} catch (err) {
  console.error('Error:', err);
  setError(err.message);
}
```

### 9.2 Error Response Codes

- `400 Bad Request`: Invalid input, missing required fields
- `401 Unauthorized`: Authentication required
- `403 Forbidden`: Authenticated but not authorized
- `404 Not Found`: Resource doesn't exist or not owned
- `409 Conflict`: Duplicate slug (alternative to 400)
- `500 Internal Server Error`: Unexpected errors

---

## 10. Deployment Architecture

### 10.1 Migration Process

```bash
# Apply migration
Get-Content "backend\db\migrations\007_add_blog_table.sql" | 
  docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype

# Verify
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype -c "\dt blog_post"
```

### 10.2 Environment Variables

**Required:**
- `SESSION_SECRET` - Session encryption key
- `ADMIN_USERNAME` - Admin login username
- `ADMIN_PASSWORD` - Admin login password
- `DATABASE_URL` - PostgreSQL connection string
- `CLOUDINARY_*` - Image upload credentials

**Optional:**
- `PORT` - Server port (default: 5000)
- `NODE_ENV` - Environment (development/production)

---

## 11. Monitoring & Observability

### 11.1 Logging

**Current Implementation:**
```javascript
console.error('Error creating blog:', err);
console.log('Server running on port', PORT);
```

**Future Enhancements:**
- Structured logging (winston/pino)
- Request ID tracking
- Performance metrics
- Error aggregation (Sentry)

### 11.2 Key Metrics to Monitor

**Performance:**
- Blog listing response time
- Blog post detail response time
- Database query execution time
- Image load time

**Usage:**
- Blog posts created per day
- Blog views per dealership
- Average time on blog pages
- Most viewed posts

**Errors:**
- Failed blog creation attempts
- 404 on blog posts
- Authentication failures
- Slug conflicts

---

## 12. Technical Debt & Improvements

### 12.1 Current Technical Debt

1. **No Rich Text Editor**
   - Impact: Limited formatting options
   - Priority: Medium
   - Effort: Medium (integrate TinyMCE or Quill)

2. **Plain Text Content Storage**
   - Impact: No HTML formatting preserved
   - Priority: Low
   - Effort: Low (change TEXT to allow HTML)

3. **No Image Optimization**
   - Impact: Slower page loads
   - Priority: Medium
   - Effort: Low (Cloudinary transformations)

4. **No Pagination**
   - Impact: Performance degrades with many posts
   - Priority: Low (< 100 posts per dealership)
   - Effort: Medium

### 12.2 Architectural Improvements

**Short Term:**
1. Add input sanitization middleware
2. Implement proper error logging
3. Add request validation middleware
4. Image transformation parameters

**Long Term:**
1. Content versioning system
2. Draft preview functionality
3. Scheduled publishing
4. Multi-language support
5. Advanced SEO features

---

## 13. Integration Points

### 13.1 Existing System Integration

**Authentication System:**
```javascript
// Reuses existing session-based auth
const { requireAuth } = require('../middleware/auth');
router.post('/api/blogs', requireAuth, handler);
```

**Upload System:**
```javascript
// Reuses existing Cloudinary upload
const formData = new FormData();
formData.append('image', file);
await fetch('/api/upload', { method: 'POST', body: formData });
```

**Navigation System:**
```javascript
// Extends defaultNavigation.js
{
  id: 'blog',
  label: 'Blog',
  route: '/blog',
  icon: 'FaNewspaper',
  order: 7,
  enabled: true
}
```

**Theming System:**
```javascript
// Uses existing theme color from dealership
const themeColor = dealership?.theme_color || '#3B82F6';
```

### 13.2 Future Integration Opportunities

- Analytics integration (Google Analytics)
- Social media auto-posting
- Email newsletter integration
- SEO tools integration
- Content management platforms

---

## 14. Testing Architecture

### 14.1 Testing Strategy

**Unit Tests (Future):**
- Database query functions
- Slug generation logic
- Validation functions
- Business logic

**Integration Tests (Future):**
- API endpoint testing
- Authentication flow
- Multi-tenancy verification
- Error handling

**E2E Tests (Future):**
- Complete blog creation workflow
- Publish/unpublish flow
- Multi-dealership isolation
- Public viewing

### 14.2 Test Data Requirements

**Seed Data:**
```sql
INSERT INTO blog_post (dealership_id, title, slug, content, author_name, status, published_at)
VALUES
  (1, 'Test Post 1', 'test-post-1', 'Content...', 'Author', 'published', NOW()),
  (1, 'Draft Post', 'draft-post', 'Content...', 'Author', 'draft', NULL),
  (2, 'Other Dealer', 'other-dealer', 'Content...', 'Author', 'published', NOW());
```

---

## 15. Documentation & Knowledge Transfer

### 15.1 Technical Documentation

**Created:**
- `PRD_BLOG_FEATURE.md` - Product requirements
- `ARCH_BLOG_FEATURE.md` - This document
- `BLOG_FEATURE.md` - Implementation guide
- `BLOG_QUICK_START.md` - Quick start guide
- `BLOG_STATUS_GUIDE.md` - Status workflow

**Code Documentation:**
- JSDoc comments in all files
- Inline comments for complex logic
- README updates (future)

### 15.2 Architectural Decision Records (ADRs)

**ADR-001: Use Session-Based Auth (Not JWT)**
- Decision: Continue using existing session-based authentication
- Rationale: Consistency with existing system, simpler implementation
- Consequences: Requires session storage, but already in place

**ADR-002: Dealership ID in Request (Not Session)**
- Decision: Pass dealership_id in request body/query, not session
- Rationale: Frontend manages dealership selection, simpler backend
- Consequences: More flexible, matches existing vehicle pattern

**ADR-003: Plain Text Content (Not Rich HTML)**
- Decision: Store content as plain text with whitespace preservation
- Rationale: Simpler MVP, prevents security issues, can enhance later
- Consequences: Limited formatting, but safer and faster to implement

**ADR-004: Status-Based Visibility (Not Published Flag)**
- Decision: Use status enum (draft/published/archived) instead of boolean
- Rationale: More flexible workflow, supports future states
- Consequences: Slightly more complex queries, but more powerful

---

## 16. Approval & Sign-off

**Solution Architect:** Approved - 2025-12-31  
**Lead Developer:** Approved - 2025-12-31  
**Security Reviewer:** Approved - 2025-12-31  

---

## Change Log

| Date | Version | Author | Changes |
|------|---------|--------|---------|
| 2025-12-31 | 1.0 | Architect Agent | Initial architecture documentation |

