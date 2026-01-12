# Blog Feature Documentation

## Overview
The blog feature allows each dealership to manage and display blog articles on their website. Admins can create, edit, and delete blog posts from the CMS admin panel, while visitors can view published posts on the public website.

## Features Implemented

### Database Schema
- **Table**: `blog_post`
- **Fields**:
  - `id` - Primary key
  - `dealership_id` - Foreign key to dealership (multi-tenant isolation)
  - `title` - Blog post title
  - `slug` - URL-friendly slug (unique per dealership)
  - `content` - Full blog post content
  - `excerpt` - Short summary for listings
  - `featured_image_url` - Optional featured image
  - `author_name` - Post author name
  - `status` - draft/published/archived
  - `published_at` - Publication timestamp
  - `created_at` - Creation timestamp
  - `updated_at` - Last update timestamp

### Backend API Endpoints

All endpoints enforce multi-tenant data isolation via `dealershipId` parameter.

#### Public Endpoints (No authentication required)
- `GET /api/blogs/published?dealershipId=<id>` - List all published blog posts
- `GET /api/blogs/slug/:slug?dealershipId=<id>` - Get blog post by slug

#### Admin Endpoints (Authentication required)
- `GET /api/blogs?dealershipId=<id>` - List all blog posts (admin view)
- `GET /api/blogs/:id?dealershipId=<id>` - Get single blog post
- `POST /api/blogs` - Create new blog post
- `PUT /api/blogs/:id` - Update blog post
- `DELETE /api/blogs/:id` - Delete blog post

### Admin Pages

#### Blog List (`/admin/blogs`)
- Displays all blog posts for the selected dealership
- Table view with columns: Title, Author, Status, Published Date, Actions
- Status filter dropdown (All, Draft, Published, Archived)
- Add New Blog Post button
- Edit and Delete actions for each post
- Delete confirmation modal

#### Blog Form (`/admin/blogs/new` and `/admin/blogs/edit/:id`)
- Create and edit blog posts
- Fields:
  - Title (required)
  - Slug (auto-generated from title, can be edited)
  - Excerpt (optional summary)
  - Content (required, textarea)
  - Author Name (required)
  - Featured Image (optional upload)
  - Status (draft/published/archived)
- Auto-generates slug from title
- Image upload via existing `/api/upload` endpoint
- Form validation
- Success/error messaging

### Public Pages

#### Blog Listing (`/blog`)
- Displays all published blog posts
- Grid layout (3 columns desktop, 2 tablet, 1 mobile)
- Each card shows:
  - Featured image (or placeholder)
  - Title
  - Author and published date
  - Excerpt
  - "Read More" link
- Click to navigate to full post

#### Blog Post Detail (`/blog/:slug`)
- Displays full blog post content
- Shows featured image
- Author information with avatar
- Published date
- Full content with formatting
- "Back to Blog" navigation

### Navigation Updates

#### Public Navigation
- Added "Blog" link to default navigation (order 7)
- Icon: `FaNewspaper` from react-icons
- Route: `/blog`
- Can be customized per dealership via navigation config

#### Admin Navigation
- Added "Blog Manager" link to admin header
- Positioned between Vehicle Manager and Dealership Settings

## File Structure

### Backend
```
backend/
├── db/
│   ├── blogs.js                          # Blog database queries
│   └── migrations/
│       └── 007_add_blog_table.sql        # Blog table migration
├── routes/
│   └── blogs.js                          # Blog API routes
└── server.js                             # Updated with blog routes
```

### Frontend
```
frontend/src/
├── pages/
│   ├── admin/
│   │   ├── BlogList.jsx                  # Admin blog list page
│   │   └── BlogForm.jsx                  # Admin blog create/edit form
│   └── public/
│       ├── Blog.jsx                      # Public blog listing page
│       └── BlogPost.jsx                  # Public blog post detail page
├── components/
│   ├── AdminHeader.jsx                   # Updated with Blog Manager link
│   └── NavigationButton.jsx              # Supports FaNewspaper icon
├── utils/
│   ├── defaultNavigation.js              # Added blog navigation item
│   └── iconMapper.js                     # Added FaNewspaper icon
└── App.jsx                               # Updated with blog routes
```

## Security

### Multi-Tenant Isolation
- All database queries require `dealershipId` parameter
- Foreign key constraint: `blog_post.dealership_id` references `dealership(id)`
- Unique constraint on `(dealership_id, slug)` prevents slug conflicts
- Admin endpoints verify ownership before modifications

### Authentication
- Create, update, and delete operations require authentication
- Uses session-based authentication via `requireAuth` middleware
- Session contains `dealershipId` for ownership verification

### Input Validation
- Field length limits enforced
- Slug format validation (lowercase, numbers, hyphens only)
- Status enum validation (draft/published/archived)
- XSS protection via input sanitization

## Usage Guide

### For Admins

#### Creating a Blog Post
1. Navigate to `/admin/blogs`
2. Click "Add New Blog Post"
3. Fill in the form:
   - Enter a title (slug auto-generates)
   - Optionally customize the slug
   - Write your content in the textarea
   - Add an excerpt (optional)
   - Upload a featured image (optional)
   - Enter author name
   - Select status (draft for preview, published to go live)
4. Click "Create Blog Post"

#### Editing a Blog Post
1. Navigate to `/admin/blogs`
2. Find the post in the list
3. Click "Edit"
4. Update any fields
5. Click "Update Blog Post"

#### Deleting a Blog Post
1. Navigate to `/admin/blogs`
2. Find the post in the list
3. Click "Delete"
4. Confirm deletion in the modal

#### Managing Post Status
- **Draft**: Post is saved but not visible to public
- **Published**: Post is live on the public website
- **Archived**: Post is hidden from public but preserved in admin

### For Public Visitors

#### Viewing Blog Posts
1. Navigate to `/blog` from the main navigation
2. Browse published posts in grid layout
3. Click on any post to read the full content
4. Use "Back to Blog" to return to the listing

## Database Migration

The blog table was created using migration `007_add_blog_table.sql`:

```bash
# Run migration
Get-Content "backend\db\migrations\007_add_blog_table.sql" | docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype
```

## Testing

### Manual Testing Checklist

#### Admin Functionality
- [ ] Login to admin panel
- [ ] Navigate to Blog Manager
- [ ] Create a new draft blog post
- [ ] Edit the draft post
- [ ] Upload a featured image
- [ ] Publish the post
- [ ] Filter by status (Draft/Published/Archived)
- [ ] Delete a post with confirmation
- [ ] Verify slug uniqueness validation

#### Public Functionality
- [ ] View blog listing page
- [ ] Verify only published posts appear
- [ ] Click to view full post
- [ ] Check featured image displays
- [ ] Verify author and date display
- [ ] Test back navigation
- [ ] Test with no published posts (empty state)

#### Multi-Tenant Isolation
- [ ] Create posts for multiple dealerships
- [ ] Verify each dealership only sees their own posts
- [ ] Test slug uniqueness per dealership (same slug OK for different dealerships)

## Future Enhancements

Potential features to add in future iterations:
- Rich text editor (WYSIWYG) for content
- Categories and tags
- Comments system
- Social media sharing
- SEO metadata (meta description, keywords)
- Search functionality
- Featured post designation
- Read time estimation
- Related posts suggestions
- Draft preview functionality
- Schedule publishing (future publish date)
- Image gallery support within posts
- Author profiles with bio and photo

## Notes

- Blog content is currently plain text with whitespace preservation
- Featured images are optional but recommended for better visual appeal
- Slug must be unique per dealership (enforced by database constraint)
- Published date is automatically set when status changes to "published"
- Navigation can be customized per dealership to hide/show blog link
