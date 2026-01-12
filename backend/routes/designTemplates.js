/**
 * @fileoverview Design Templates API routes.
 * Handles design template management operations for dealerships.
 *
 * Routes:
 * - GET    /api/design-templates       - List all templates available to dealership
 * - POST   /api/design-templates       - Create new custom template
 * - DELETE /api/design-templates/:id   - Delete custom template
 */

const express = require('express');
const router = express.Router();
const designTemplatesDb = require('../db/designTemplates');
const { requireAuth, requirePermission, enforceDealershipScope } = require('../middleware/auth');

/**
 * Field length limits for input validation.
 */
const FIELD_LIMITS = {
  name: 100,
  description: 500,
  theme_color: 7, // Hex color format: #RRGGBB
  secondary_theme_color: 7,
  body_background_color: 7,
  font_family: 100
};

/**
 * Sanitizes user input to prevent XSS attacks.
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
 * Validates hex color format.
 *
 * @param {string} color - Hex color code to validate
 * @returns {boolean} True if color format is valid
 */
function validateColorFormat(color) {
  const HEX_COLOR_REGEX = /^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$/;
  return HEX_COLOR_REGEX.test(color);
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
 * GET /api/design-templates - List all templates available to the dealership.
 * Returns both preset templates and dealership-specific custom templates.
 *
 * @returns {Array<Object>} Array of template objects
 * @throws {400} If dealership_id is missing or invalid
 * @throws {500} If database query fails
 */
router.get('/', requireAuth, async (req, res) => {
  try {
    const user = req.session.user;
    
    // Get dealership_id from user session or query parameter (for admin users)
    const dealershipId = user.dealership_id || parseInt(req.query.dealership_id);
    
    // If still no dealership_id (admin without selection), return only preset templates  
    if (!dealershipId) {
      const query = 'SELECT * FROM design_templates WHERE is_preset = true ORDER BY name ASC';
      const pool = require('../db/index');
      const result = await pool.query(query);
      return res.json(result.rows);
    }

    const templates = await designTemplatesDb.getAllForDealership(dealershipId);
    res.json(templates);
  } catch (error) {
    console.error('Error fetching design templates:', error);
    res.status(500).json({ error: 'Failed to fetch design templates' });
  }
});

/**
 * POST /api/design-templates - Create a new custom design template.
 *
 * @param {Object} req.body - Template data
 * @param {string} req.body.name - Template name (required)
 * @param {string} [req.body.description] - Template description (optional)
 * @param {string} req.body.theme_color - Primary theme color in hex format (required)
 * @param {string} req.body.secondary_theme_color - Secondary theme color in hex format (required)
 * @param {string} req.body.body_background_color - Body background color in hex format (required)
 * @param {string} req.body.font_family - Font family identifier (required)
 * @returns {Object} Created template object
 * @throws {400} If required fields missing, invalid format, or template name already exists
 * @throws {500} If database query fails
 */
router.post('/', requireAuth, requirePermission('settings'), async (req, res) => {
  try {
    const user = req.session.user;
    const dealershipId = user.dealership_id || parseInt(req.body.dealership_id) || parseInt(req.query.dealership_id);
    const { name, description, theme_color, secondary_theme_color, body_background_color, font_family } = req.body;

    // Validate dealership_id
    if (!dealershipId || isNaN(dealershipId)) {
      return res.status(400).json({ error: 'Dealership ID is required' });
    }

    // Validate required fields
    if (!name || !theme_color || !secondary_theme_color || !body_background_color || !font_family) {
      return res.status(400).json({
        error: 'Missing required fields: name, theme_color, secondary_theme_color, body_background_color, font_family'
      });
    }

    // Validate field lengths
    const lengthValidation = validateFieldLengths(req.body);
    if (lengthValidation) {
      return res.status(400).json(lengthValidation);
    }

    // Validate color formats
    if (!validateColorFormat(theme_color)) {
      return res.status(400).json({ error: 'Invalid theme color format. Use hex format: #RRGGBB or #RGB' });
    }
    if (!validateColorFormat(secondary_theme_color)) {
      return res.status(400).json({ error: 'Invalid secondary theme color format. Use hex format: #RRGGBB or #RGB' });
    }
    if (!validateColorFormat(body_background_color)) {
      return res.status(400).json({ error: 'Invalid body background color format. Use hex format: #RRGGBB or #RGB' });
    }

    // Sanitize inputs
    const sanitizedName = sanitizeInput(name);
    const sanitizedDescription = description ? sanitizeInput(description) : undefined;

    // Create template
    const newTemplate = await designTemplatesDb.create(dealershipId, {
      name: sanitizedName,
      description: sanitizedDescription,
      theme_color,
      secondary_theme_color,
      body_background_color,
      font_family
    });

    res.status(201).json(newTemplate);
  } catch (error) {
    console.error('Error creating design template:', error);

    // Handle unique constraint violation (duplicate template name)
    if (error.code === '23505') {
      return res.status(400).json({ error: 'A template with this name already exists' });
    }

    res.status(500).json({ error: 'Failed to create design template' });
  }
});

/**
 * DELETE /api/design-templates/:id - Delete a custom design template.
 * Only custom templates (not presets) can be deleted.
 *
 * @param {number} req.params.id - Template ID to delete
 * @returns {Object} Success message
 * @throws {400} If template ID is invalid
 * @throws {403} If trying to delete a preset template
 * @throws {404} If template not found
 * @throws {500} If database query fails
 */
router.delete('/:id', requireAuth, requirePermission('settings'), async (req, res) => {
  try {
    const user = req.session.user;
    const dealershipId = user.dealership_id || parseInt(req.query.dealership_id);
    const templateId = parseInt(req.params.id, 10);

    if (isNaN(templateId) || templateId <= 0) {
      return res.status(400).json({ error: 'Template ID must be a valid positive number' });
    }

    if (!dealershipId || isNaN(dealershipId)) {
      return res.status(400).json({ error: 'Dealership ID is required' });
    }

    const deleted = await designTemplatesDb.deleteTemplate(templateId, dealershipId);

    if (!deleted) {
      return res.status(404).json({ error: 'Template not found or cannot be deleted' });
    }

    res.json({ message: 'Template deleted successfully' });
  } catch (error) {
    console.error('Error deleting design template:', error);
    res.status(500).json({ error: 'Failed to delete design template' });
  }
});

module.exports = router;
