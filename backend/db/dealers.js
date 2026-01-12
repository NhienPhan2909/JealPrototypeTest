/**
 * @fileoverview Dealership database query functions.
 * Provides CRUD operations for dealership records with multi-tenant data isolation.
 *
 * CRITICAL SECURITY REQUIREMENT:
 * All query functions in this module enforce multi-tenancy by requiring dealership_id parameter.
 * This prevents data leaks between dealerships (SEC-001 mitigation).
 *
 * Routes:
 * - Used by GET  /api/dealers/:id - Get dealership profile
 * - Used by PUT  /api/dealers/:id - Update dealership profile (auth required)
 */

const pool = require('./index');

/**
 * Retrieves a single dealership by ID.
 *
 * @param {number} dealershipId - The dealership ID to retrieve
 * @returns {Promise<Object|null>} Dealership object or null if not found
 * @throws {Error} If database query fails
 *
 * @example
 * const dealership = await getById(1);
 * // Returns: { id: 1, name: 'Acme Auto Sales', address: '...', ... }
 */
async function getById(dealershipId) {
  const query = 'SELECT * FROM dealership WHERE id = $1';
  const result = await pool.query(query, [dealershipId]);
  return result.rows[0] || null;
}

/**
 * Updates dealership profile information.
 *
 * SECURITY: This function enforces that only the specified dealership's data can be updated.
 * The WHERE clause MUST include dealership_id to prevent unauthorized updates.
 *
 * @param {number} dealershipId - The dealership ID to update
 * @param {Object} updates - Object containing fields to update
 * @param {string} [updates.name] - Dealership name
 * @param {string} [updates.logo_url] - Logo image URL
 * @param {string} [updates.address] - Street address
 * @param {string} [updates.phone] - Phone number
 * @param {string} [updates.email] - Email address
 * @param {string} [updates.hours] - Business hours text
 * @param {string} [updates.finance_policy] - Financing policy text
 * @param {string} [updates.warranty_policy] - Warranty policy text
 * @param {string} [updates.about] - About text
 * @param {string} [updates.hero_background_image] - Hero section background image URL
 * @param {string} [updates.hero_type] - Hero media type: 'image', 'video', or 'carousel'
 * @param {string} [updates.hero_video_url] - Hero section background video URL
 * @param {Array<string>} [updates.hero_carousel_images] - Array of hero carousel image URLs (JSONB)
 * @param {string} [updates.theme_color] - Theme color in hex format (e.g., #3B82F6)
 * @param {string} [updates.secondary_theme_color] - Secondary theme color in hex format (e.g., #FFFFFF)
 * @param {string} [updates.body_background_color] - Body background color in hex format (e.g., #FFFFFF)
 * @param {string} [updates.font_family] - Font family for website text (e.g., 'Times New Roman, serif')
 * @param {Array<Object>|null} [updates.navigation_config] - Navigation menu configuration array (JSONB)
 * @param {string} [updates.facebook_url] - Facebook page URL
 * @param {string} [updates.instagram_url] - Instagram profile URL
 * @param {string} [updates.finance_promo_image] - Finance promotional panel background image URL
 * @param {string} [updates.finance_promo_text] - Finance promotional panel text overlay
 * @param {string} [updates.warranty_promo_image] - Warranty promotional panel background image URL
 * @param {string} [updates.warranty_promo_text] - Warranty promotional panel text overlay
 * @returns {Promise<Object|null>} Updated dealership object or null if not found
 * @throws {Error} If database query fails
 *
 * @example
 * const updated = await update(1, {
 *   hours: 'Mon-Sat 9am-7pm',
 *   about: 'New description text'
 * });
 */
async function update(dealershipId, updates) {
  // Build dynamic UPDATE query based on provided fields
  const fields = [];
  const values = [];
  let paramIndex = 1;

  // Only include provided fields in the UPDATE
  if (updates.name !== undefined) {
    fields.push(`name = $${paramIndex++}`);
    values.push(updates.name);
  }
  if (updates.logo_url !== undefined) {
    fields.push(`logo_url = $${paramIndex++}`);
    values.push(updates.logo_url);
  }
  if (updates.address !== undefined) {
    fields.push(`address = $${paramIndex++}`);
    values.push(updates.address);
  }
  if (updates.phone !== undefined) {
    fields.push(`phone = $${paramIndex++}`);
    values.push(updates.phone);
  }
  if (updates.email !== undefined) {
    fields.push(`email = $${paramIndex++}`);
    values.push(updates.email);
  }
  if (updates.hours !== undefined) {
    fields.push(`hours = $${paramIndex++}`);
    values.push(updates.hours);
  }
  if (updates.finance_policy !== undefined) {
    fields.push(`finance_policy = $${paramIndex++}`);
    values.push(updates.finance_policy);
  }
  if (updates.warranty_policy !== undefined) {
    fields.push(`warranty_policy = $${paramIndex++}`);
    values.push(updates.warranty_policy);
  }
  if (updates.about !== undefined) {
    fields.push(`about = $${paramIndex++}`);
    values.push(updates.about);
  }
  if (updates.hero_background_image !== undefined) {
    fields.push(`hero_background_image = $${paramIndex++}`);
    values.push(updates.hero_background_image);
  }
  if (updates.hero_type !== undefined) {
    fields.push(`hero_type = $${paramIndex++}`);
    values.push(updates.hero_type);
  }
  if (updates.hero_video_url !== undefined) {
    fields.push(`hero_video_url = $${paramIndex++}`);
    values.push(updates.hero_video_url);
  }
  if (updates.hero_carousel_images !== undefined) {
    fields.push(`hero_carousel_images = $${paramIndex++}`);
    values.push(JSON.stringify(updates.hero_carousel_images));
  }
  if (updates.theme_color !== undefined) {
    fields.push(`theme_color = $${paramIndex++}`);
    values.push(updates.theme_color);
  }
  if (updates.secondary_theme_color !== undefined) {
    fields.push(`secondary_theme_color = $${paramIndex++}`);
    values.push(updates.secondary_theme_color);
  }
  if (updates.body_background_color !== undefined) {
    fields.push(`body_background_color = $${paramIndex++}`);
    values.push(updates.body_background_color);
  }
  if (updates.font_family !== undefined) {
    fields.push(`font_family = $${paramIndex++}`);
    values.push(updates.font_family);
  }
  if (updates.navigation_config !== undefined) {
    fields.push(`navigation_config = $${paramIndex++}`);
    values.push(JSON.stringify(updates.navigation_config));
  }
  if (updates.facebook_url !== undefined) {
    fields.push(`facebook_url = $${paramIndex++}`);
    values.push(updates.facebook_url);
  }
  if (updates.instagram_url !== undefined) {
    fields.push(`instagram_url = $${paramIndex++}`);
    values.push(updates.instagram_url);
  }
  if (updates.finance_promo_image !== undefined) {
    fields.push(`finance_promo_image = $${paramIndex++}`);
    values.push(updates.finance_promo_image);
  }
  if (updates.finance_promo_text !== undefined) {
    fields.push(`finance_promo_text = $${paramIndex++}`);
    values.push(updates.finance_promo_text);
  }
  if (updates.warranty_promo_image !== undefined) {
    fields.push(`warranty_promo_image = $${paramIndex++}`);
    values.push(updates.warranty_promo_image);
  }
  if (updates.warranty_promo_text !== undefined) {
    fields.push(`warranty_promo_text = $${paramIndex++}`);
    values.push(updates.warranty_promo_text);
  }

  if (fields.length === 0) {
    throw new Error('No fields to update');
  }

  // CRITICAL: WHERE clause includes dealership_id to enforce multi-tenancy
  values.push(dealershipId);
  const query = `
    UPDATE dealership
    SET ${fields.join(', ')}
    WHERE id = $${paramIndex}
    RETURNING *
  `;

  const result = await pool.query(query, values);
  return result.rows[0] || null;
}

/**
 * Retrieves all dealerships (admin-only function).
 *
 * NOTE: This function does NOT enforce dealership_id filtering because it's intended
 * for admin/system use only. NEVER expose this to tenant-scoped API endpoints.
 *
 * @returns {Promise<Array<Object>>} Array of all dealership objects
 * @throws {Error} If database query fails
 *
 * @example
 * const allDealerships = await getAll();
 * // Returns: [{ id: 1, name: '...' }, { id: 2, name: '...' }]
 */
async function getAll() {
  const query = 'SELECT * FROM dealership ORDER BY name';
  const result = await pool.query(query);
  return result.rows;
}

module.exports = {
  getById,
  update,
  getAll
};
