/**
 * @fileoverview Blog post database query functions.
 * Provides CRUD operations for blog posts with multi-tenant data isolation.
 *
 * CRITICAL SECURITY REQUIREMENT:
 * All query functions enforce multi-tenancy by requiring dealership_id parameter.
 * This prevents data leaks between dealerships.
 */

const pool = require('./index');

/**
 * Retrieves all blog posts for a dealership.
 *
 * @param {number} dealershipId - The dealership ID
 * @param {Object} options - Query options
 * @param {string} [options.status] - Filter by status (draft/published/archived)
 * @returns {Promise<Array>} Array of blog post objects
 */
async function getAllByDealership(dealershipId, options = {}) {
  let query = 'SELECT * FROM blog_post WHERE dealership_id = $1';
  const params = [dealershipId];
  
  if (options.status) {
    query += ' AND status = $2';
    params.push(options.status);
  }
  
  query += ' ORDER BY created_at DESC';
  
  const result = await pool.query(query, params);
  return result.rows;
}

/**
 * Retrieves published blog posts for public display.
 *
 * @param {number} dealershipId - The dealership ID
 * @returns {Promise<Array>} Array of published blog post objects
 */
async function getPublishedByDealership(dealershipId) {
  const query = `
    SELECT * FROM blog_post 
    WHERE dealership_id = $1 AND status = 'published'
    ORDER BY published_at DESC
  `;
  const result = await pool.query(query, [dealershipId]);
  return result.rows;
}

/**
 * Retrieves a single blog post by ID.
 *
 * @param {number} id - The blog post ID
 * @param {number} dealershipId - The dealership ID (for multi-tenant security)
 * @returns {Promise<Object|null>} Blog post object or null if not found
 */
async function getById(id, dealershipId) {
  const query = 'SELECT * FROM blog_post WHERE id = $1 AND dealership_id = $2';
  const result = await pool.query(query, [id, dealershipId]);
  return result.rows[0] || null;
}

/**
 * Retrieves a single blog post by slug.
 *
 * @param {string} slug - The blog post slug
 * @param {number} dealershipId - The dealership ID
 * @returns {Promise<Object|null>} Blog post object or null if not found
 */
async function getBySlug(slug, dealershipId) {
  const query = 'SELECT * FROM blog_post WHERE slug = $1 AND dealership_id = $2';
  const result = await pool.query(query, [slug, dealershipId]);
  return result.rows[0] || null;
}

/**
 * Creates a new blog post.
 *
 * @param {number} dealershipId - The dealership ID
 * @param {Object} blogData - Blog post data
 * @param {string} blogData.title - Post title
 * @param {string} blogData.slug - URL-friendly slug
 * @param {string} blogData.content - Post content
 * @param {string} [blogData.excerpt] - Short excerpt
 * @param {string} [blogData.featured_image_url] - Featured image URL
 * @param {string} blogData.author_name - Author name
 * @param {string} [blogData.status='draft'] - Post status
 * @returns {Promise<Object>} The created blog post
 */
async function create(dealershipId, blogData) {
  const {
    title,
    slug,
    content,
    excerpt = null,
    featured_image_url = null,
    author_name,
    status = 'draft'
  } = blogData;

  const publishedAt = status === 'published' ? 'NOW()' : null;

  const query = `
    INSERT INTO blog_post (
      dealership_id, title, slug, content, excerpt, 
      featured_image_url, author_name, status, published_at
    )
    VALUES ($1, $2, $3, $4, $5, $6, $7, $8, ${publishedAt})
    RETURNING *
  `;

  const params = [
    dealershipId,
    title,
    slug,
    content,
    excerpt,
    featured_image_url,
    author_name,
    status
  ];

  const result = await pool.query(query, params);
  return result.rows[0];
}

/**
 * Updates an existing blog post.
 *
 * @param {number} id - The blog post ID
 * @param {number} dealershipId - The dealership ID (for multi-tenant security)
 * @param {Object} updates - Fields to update
 * @returns {Promise<Object|null>} Updated blog post or null if not found
 */
async function update(id, dealershipId, updates) {
  const allowedFields = [
    'title', 'slug', 'content', 'excerpt', 
    'featured_image_url', 'author_name', 'status'
  ];
  
  const fields = [];
  const values = [];
  let paramCount = 1;

  for (const [key, value] of Object.entries(updates)) {
    if (allowedFields.includes(key)) {
      fields.push(`${key} = $${paramCount}`);
      values.push(value);
      paramCount++;
    }
  }

  if (fields.length === 0) {
    throw new Error('No valid fields to update');
  }

  // Update published_at when status changes to published
  if (updates.status === 'published') {
    fields.push(`published_at = CASE WHEN published_at IS NULL THEN NOW() ELSE published_at END`);
  }

  // Always update updated_at
  fields.push('updated_at = NOW()');

  values.push(id, dealershipId);

  const query = `
    UPDATE blog_post 
    SET ${fields.join(', ')}
    WHERE id = $${paramCount} AND dealership_id = $${paramCount + 1}
    RETURNING *
  `;

  const result = await pool.query(query, values);
  return result.rows[0] || null;
}

/**
 * Deletes a blog post.
 *
 * @param {number} id - The blog post ID
 * @param {number} dealershipId - The dealership ID (for multi-tenant security)
 * @returns {Promise<boolean>} True if deleted, false if not found
 */
async function deleteById(id, dealershipId) {
  const query = 'DELETE FROM blog_post WHERE id = $1 AND dealership_id = $2';
  const result = await pool.query(query, [id, dealershipId]);
  return result.rowCount > 0;
}

module.exports = {
  getAllByDealership,
  getPublishedByDealership,
  getById,
  getBySlug,
  create,
  update,
  deleteById
};
