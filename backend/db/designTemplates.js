/**
 * @fileoverview Design Templates database query functions.
 * Provides CRUD operations for design template records.
 *
 * Routes:
 * - Used by GET  /api/design-templates - Get all available templates (presets + dealership custom)
 * - Used by POST /api/design-templates - Create new custom template
 * - Used by DELETE /api/design-templates/:id - Delete custom template
 */

const pool = require('./index');

/**
 * Retrieves all design templates available to a dealership.
 * Includes both preset templates and dealership-specific custom templates.
 *
 * @param {number} dealershipId - The dealership ID to retrieve templates for
 * @returns {Promise<Array<Object>>} Array of template objects
 * @throws {Error} If database query fails
 *
 * @example
 * const templates = await getAllForDealership(1);
 * // Returns: [{ id: 1, name: 'Modern Blue', is_preset: true, ... }, ...]
 */
async function getAllForDealership(dealershipId) {
  const query = `
    SELECT * FROM design_templates
    WHERE is_preset = true OR dealership_id = $1
    ORDER BY is_preset DESC, name ASC
  `;
  const result = await pool.query(query, [dealershipId]);
  return result.rows;
}

/**
 * Retrieves a single design template by ID.
 *
 * @param {number} templateId - The template ID to retrieve
 * @returns {Promise<Object|null>} Template object or null if not found
 * @throws {Error} If database query fails
 *
 * @example
 * const template = await getById(1);
 * // Returns: { id: 1, name: 'Modern Blue', theme_color: '#3B82F6', ... }
 */
async function getById(templateId) {
  const query = 'SELECT * FROM design_templates WHERE id = $1';
  const result = await pool.query(query, [templateId]);
  return result.rows[0] || null;
}

/**
 * Creates a new custom design template for a dealership.
 *
 * @param {number} dealershipId - The dealership ID creating the template
 * @param {Object} templateData - Template data
 * @param {string} templateData.name - Template name
 * @param {string} [templateData.description] - Template description
 * @param {string} templateData.theme_color - Primary theme color in hex format
 * @param {string} templateData.secondary_theme_color - Secondary theme color in hex format
 * @param {string} templateData.body_background_color - Body background color in hex format
 * @param {string} templateData.font_family - Font family identifier
 * @returns {Promise<Object>} Created template object
 * @throws {Error} If database query fails or template name already exists
 *
 * @example
 * const newTemplate = await create(1, {
 *   name: 'My Custom Theme',
 *   theme_color: '#FF5733',
 *   secondary_theme_color: '#FFFFFF',
 *   body_background_color: '#F5F5F5',
 *   font_family: 'roboto'
 * });
 */
async function create(dealershipId, templateData) {
  const { name, description, theme_color, secondary_theme_color, body_background_color, font_family } = templateData;

  const query = `
    INSERT INTO design_templates (
      name, description, dealership_id, is_preset,
      theme_color, secondary_theme_color, body_background_color, font_family
    )
    VALUES ($1, $2, $3, false, $4, $5, $6, $7)
    RETURNING *
  `;

  const result = await pool.query(query, [
    name,
    description || null,
    dealershipId,
    theme_color,
    secondary_theme_color,
    body_background_color,
    font_family
  ]);

  return result.rows[0];
}

/**
 * Deletes a custom design template.
 * Preset templates cannot be deleted (enforced by database constraint).
 *
 * @param {number} templateId - The template ID to delete
 * @param {number} dealershipId - The dealership ID (for security verification)
 * @returns {Promise<boolean>} True if deleted, false if not found or unauthorized
 * @throws {Error} If database query fails
 *
 * @example
 * const deleted = await deleteTemplate(5, 1);
 * // Returns: true if template 5 was deleted by dealership 1
 */
async function deleteTemplate(templateId, dealershipId) {
  const query = `
    DELETE FROM design_templates
    WHERE id = $1 AND dealership_id = $2 AND is_preset = false
    RETURNING id
  `;

  const result = await pool.query(query, [templateId, dealershipId]);
  return result.rowCount > 0;
}

module.exports = {
  getAllForDealership,
  getById,
  create,
  deleteTemplate
};
