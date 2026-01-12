/**
 * @fileoverview Blog post CRUD API routes.
 * Handles all blog-related operations with multi-tenant filtering.
 *
 * SECURITY (SEC-001): All endpoints require dealershipId query parameter to enforce
 * multi-tenant data isolation and prevent cross-dealership access.
 *
 * Routes:
 * - GET    /api/blogs?dealershipId=<id>              - List all blog posts for dealership (admin)
 * - GET    /api/blogs/published?dealershipId=<id>    - List published blog posts (public)
 * - GET    /api/blogs/:id?dealershipId=<id>          - Get single blog post
 * - POST   /api/blogs                                 - Create blog post (auth required)
 * - PUT    /api/blogs/:id?dealershipId=<id>          - Update blog post (auth required)
 * - DELETE /api/blogs/:id?dealershipId=<id>          - Delete blog post (auth required)
 */

const express = require('express');
const router = express.Router();
const blogsDb = require('../db/blogs');
const { requireAuth, requirePermission, enforceDealershipScope } = require('../middleware/auth');

/**
 * Field length limits for input validation
 */
const FIELD_LIMITS = {
  title: 255,
  slug: 255,
  content: 50000,
  excerpt: 1000,
  author_name: 255,
  status: 20
};

/**
 * Valid enum values for blog post fields
 */
const VALID_STATUSES = ['draft', 'published', 'archived'];

/**
 * Sanitizes user input to prevent XSS attacks by escaping HTML special characters.
 *
 * @param {string} input - User-provided string to sanitize
 * @returns {string} Sanitized string safe for HTML display
 */
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

/**
 * Validates field lengths against defined limits.
 *
 * @param {Object} data - Data object to validate
 * @returns {Object|null} Error object if validation fails, null if valid
 */
function validateFieldLengths(data) {
  for (const [field, limit] of Object.entries(FIELD_LIMITS)) {
    if (data[field] && data[field].length > limit) {
      return { error: `${field} must be ${limit} characters or less` };
    }
  }
  return null;
}

/**
 * Generates URL-friendly slug from title.
 *
 * @param {string} title - Blog post title
 * @returns {string} URL-friendly slug
 */
function generateSlug(title) {
  return title
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
    .substring(0, 200);
}

/**
 * GET /api/blogs?dealershipId=<id>
 * List all blog posts for a dealership (admin view).
 *
 * SECURITY: Requires dealershipId to enforce multi-tenant data isolation.
 */
router.get('/', async (req, res) => {
  try {
    const dealershipId = parseInt(req.query.dealershipId, 10);

    if (!dealershipId || isNaN(dealershipId)) {
      return res.status(400).json({ error: 'dealershipId query parameter is required' });
    }

    const blogs = await blogsDb.getAllByDealership(dealershipId);
    res.json(blogs);
  } catch (err) {
    console.error('Error fetching blog posts:', err);
    res.status(500).json({ error: 'Failed to fetch blog posts' });
  }
});

/**
 * GET /api/blogs/published?dealershipId=<id>
 * List published blog posts for public display.
 *
 * SECURITY: Only returns published posts. Requires dealershipId.
 */
router.get('/published', async (req, res) => {
  try {
    const dealershipId = parseInt(req.query.dealershipId, 10);

    if (!dealershipId || isNaN(dealershipId)) {
      return res.status(400).json({ error: 'dealershipId query parameter is required' });
    }

    const blogs = await blogsDb.getPublishedByDealership(dealershipId);
    res.json(blogs);
  } catch (err) {
    console.error('Error fetching published blog posts:', err);
    res.status(500).json({ error: 'Failed to fetch blog posts' });
  }
});

/**
 * GET /api/blogs/slug/:slug?dealershipId=<id>
 * Get blog post by slug (for public URLs).
 *
 * SECURITY: Requires dealershipId for multi-tenant isolation.
 */
router.get('/slug/:slug', async (req, res) => {
  try {
    const { slug } = req.params;
    const dealershipId = parseInt(req.query.dealershipId, 10);

    if (!dealershipId || isNaN(dealershipId)) {
      return res.status(400).json({ error: 'dealershipId query parameter is required' });
    }

    const blog = await blogsDb.getBySlug(slug, dealershipId);

    if (!blog) {
      return res.status(404).json({ error: 'Blog post not found' });
    }

    res.json(blog);
  } catch (err) {
    console.error('Error fetching blog post by slug:', err);
    res.status(500).json({ error: 'Failed to fetch blog post' });
  }
});

/**
 * GET /api/blogs/:id?dealershipId=<id>
 * Get a single blog post by ID.
 *
 * SECURITY: Requires dealershipId to verify ownership before returning data.
 */
router.get('/:id', async (req, res) => {
  try {
    const id = parseInt(req.params.id, 10);
    const dealershipId = parseInt(req.query.dealershipId, 10);

    if (!dealershipId || isNaN(dealershipId)) {
      return res.status(400).json({ error: 'dealershipId query parameter is required' });
    }

    if (isNaN(id)) {
      return res.status(400).json({ error: 'Invalid blog post ID' });
    }

    const blog = await blogsDb.getById(id, dealershipId);

    if (!blog) {
      return res.status(404).json({ error: 'Blog post not found' });
    }

    res.json(blog);
  } catch (err) {
    console.error('Error fetching blog post:', err);
    res.status(500).json({ error: 'Failed to fetch blog post' });
  }
});

/**
 * POST /api/blogs
 * Create a new blog post.
 *
 * SECURITY: Requires authentication. Uses dealership_id from request body.
 */
router.post('/', requireAuth, enforceDealershipScope, requirePermission('blogs'), async (req, res) => {
  try {
    const { dealership_id, title, slug, content, excerpt, featured_image_url, author_name, status } = req.body;

    // Validate dealership_id
    if (!dealership_id) {
      return res.status(400).json({ error: 'dealership_id is required' });
    }

    const dealershipId = parseInt(dealership_id, 10);
    if (isNaN(dealershipId) || dealershipId <= 0) {
      return res.status(400).json({ error: 'dealership_id must be a valid positive number' });
    }

    // Validate required fields
    if (!title || !content || !author_name) {
      return res.status(400).json({ error: 'title, content, and author_name are required' });
    }

    // Validate field lengths
    const lengthError = validateFieldLengths(req.body);
    if (lengthError) {
      return res.status(400).json(lengthError);
    }

    // Validate status
    if (status && !VALID_STATUSES.includes(status)) {
      return res.status(400).json({ error: `status must be one of: ${VALID_STATUSES.join(', ')}` });
    }

    // Generate slug if not provided
    const finalSlug = slug || generateSlug(title);

    // Check for duplicate slug
    const existingBlog = await blogsDb.getBySlug(finalSlug, dealershipId);
    if (existingBlog) {
      return res.status(400).json({ error: 'A blog post with this slug already exists' });
    }

    const blogData = {
      title,
      slug: finalSlug,
      content,
      excerpt: excerpt || null,
      featured_image_url: featured_image_url || null,
      author_name,
      status: status || 'draft'
    };

    const newBlog = await blogsDb.create(dealershipId, blogData);
    res.status(201).json(newBlog);
  } catch (err) {
    console.error('Error creating blog post:', err);
    
    // Handle unique constraint violation
    if (err.code === '23505') {
      return res.status(400).json({ error: 'A blog post with this slug already exists' });
    }
    
    res.status(500).json({ error: 'Failed to create blog post' });
  }
});

/**
 * PUT /api/blogs/:id?dealershipId=<id>
 * Update an existing blog post.
 *
 * SECURITY: Requires authentication. Verifies ownership via dealershipId query parameter.
 */
router.put('/:id', requireAuth, enforceDealershipScope, requirePermission('blogs'), async (req, res) => {
  try {
    const id = parseInt(req.params.id, 10);
    const dealershipId = parseInt(req.query.dealershipId, 10);

    if (!dealershipId || isNaN(dealershipId)) {
      return res.status(400).json({ error: 'dealershipId query parameter is required' });
    }

    if (isNaN(id)) {
      return res.status(400).json({ error: 'Invalid blog post ID' });
    }

    // Verify ownership
    const existingBlog = await blogsDb.getById(id, dealershipId);
    if (!existingBlog) {
      return res.status(404).json({ error: 'Blog post not found' });
    }

    // Validate field lengths
    const lengthError = validateFieldLengths(req.body);
    if (lengthError) {
      return res.status(400).json(lengthError);
    }

    // Validate status if provided
    if (req.body.status && !VALID_STATUSES.includes(req.body.status)) {
      return res.status(400).json({ error: `status must be one of: ${VALID_STATUSES.join(', ')}` });
    }

    // Check for duplicate slug if slug is being changed
    if (req.body.slug && req.body.slug !== existingBlog.slug) {
      const duplicateBlog = await blogsDb.getBySlug(req.body.slug, dealershipId);
      if (duplicateBlog) {
        return res.status(400).json({ error: 'A blog post with this slug already exists' });
      }
    }

    const updatedBlog = await blogsDb.update(id, dealershipId, req.body);
    res.json(updatedBlog);
  } catch (err) {
    console.error('Error updating blog post:', err);
    
    // Handle unique constraint violation
    if (err.code === '23505') {
      return res.status(400).json({ error: 'A blog post with this slug already exists' });
    }
    
    res.status(500).json({ error: 'Failed to update blog post' });
  }
});

/**
 * DELETE /api/blogs/:id?dealershipId=<id>
 * Delete a blog post.
 *
 * SECURITY: Requires authentication. Verifies ownership via dealershipId query parameter.
 */
router.delete('/:id', requireAuth, enforceDealershipScope, requirePermission('blogs'), async (req, res) => {
  try {
    const id = parseInt(req.params.id, 10);
    const dealershipId = parseInt(req.query.dealershipId, 10);

    if (!dealershipId || isNaN(dealershipId)) {
      return res.status(400).json({ error: 'dealershipId query parameter is required' });
    }

    if (isNaN(id)) {
      return res.status(400).json({ error: 'Invalid blog post ID' });
    }

    const deleted = await blogsDb.deleteById(id, dealershipId);

    if (!deleted) {
      return res.status(404).json({ error: 'Blog post not found' });
    }

    res.json({ message: 'Blog post deleted successfully' });
  } catch (err) {
    console.error('Error deleting blog post:', err);
    res.status(500).json({ error: 'Failed to delete blog post' });
  }
});

module.exports = router;
