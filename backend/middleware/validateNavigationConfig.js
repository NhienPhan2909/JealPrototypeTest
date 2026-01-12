/**
 * @fileoverview Validation middleware for navigation_config field.
 * Validates the structure and content of navigation menu configuration.
 *
 * Story: 5.1 - Navigation Configuration Database & Backend API
 *
 * Validation Rules:
 * - navigation_config must be array or null
 * - Each item must have required fields: id, label, route, icon, order, enabled
 * - Optional fields: showIcon (boolean)
 * - id must be unique within the array
 * - id, label, route, icon must be non-empty strings
 * - order must be a positive integer
 * - enabled must be a boolean
 * - showIcon (if present) must be a boolean
 *
 * Usage:
 * Import and use as middleware in PUT /api/dealers/:id route
 *
 * @example
 * const validateNavigationConfig = require('./middleware/validateNavigationConfig');
 * router.put('/dealers/:id', validateNavigationConfig, dealersController.update);
 */

/**
 * Validates navigation_config structure and content.
 *
 * @param {Object} req - Express request object
 * @param {Object} res - Express response object
 * @param {Function} next - Express next middleware function
 * @returns {void|Response} Calls next() if valid, returns 400 error if invalid
 */
function validateNavigationConfig(req, res, next) {
  const { navigation_config } = req.body;

  // Allow null (means use default navigation)
  if (navigation_config === null || navigation_config === undefined) {
    return next();
  }

  // Must be an array
  if (!Array.isArray(navigation_config)) {
    return res.status(400).json({
      error: 'Invalid navigation_config: must be an array or null'
    });
  }

  // Validate each navigation item
  const ids = new Set();
  for (let i = 0; i < navigation_config.length; i++) {
    const item = navigation_config[i];

    // Check required fields exist
    if (!item.id || !item.label || !item.route || !item.icon ||
        item.order === undefined || item.enabled === undefined) {
      return res.status(400).json({
        error: `Invalid navigation_config: item at index ${i} is missing required fields (id, label, route, icon, order, enabled)`
      });
    }

    // Validate field types
    if (typeof item.id !== 'string' || item.id.trim() === '') {
      return res.status(400).json({
        error: `Invalid navigation_config: item at index ${i} has invalid id (must be non-empty string)`
      });
    }

    if (typeof item.label !== 'string' || item.label.trim() === '') {
      return res.status(400).json({
        error: `Invalid navigation_config: item at index ${i} has invalid label (must be non-empty string)`
      });
    }

    if (typeof item.route !== 'string' || item.route.trim() === '') {
      return res.status(400).json({
        error: `Invalid navigation_config: item at index ${i} has invalid route (must be non-empty string)`
      });
    }

    if (typeof item.icon !== 'string' || item.icon.trim() === '') {
      return res.status(400).json({
        error: `Invalid navigation_config: item at index ${i} has invalid icon (must be non-empty string)`
      });
    }

    if (typeof item.order !== 'number' || !Number.isInteger(item.order) || item.order < 1) {
      return res.status(400).json({
        error: `Invalid navigation_config: item at index ${i} has invalid order (must be positive integer)`
      });
    }

    if (typeof item.enabled !== 'boolean') {
      return res.status(400).json({
        error: `Invalid navigation_config: item at index ${i} has invalid enabled (must be boolean)`
      });
    }

    // Validate optional showIcon field (if present)
    if (item.showIcon !== undefined && typeof item.showIcon !== 'boolean') {
      return res.status(400).json({
        error: `Invalid navigation_config: item at index ${i} has invalid showIcon (must be boolean)`
      });
    }

    // Check for duplicate IDs
    if (ids.has(item.id)) {
      return res.status(400).json({
        error: `Invalid navigation_config: duplicate id "${item.id}" found`
      });
    }
    ids.add(item.id);
  }

  // Optional: Limit array length to prevent abuse (20 items max suggested)
  if (navigation_config.length > 20) {
    return res.status(400).json({
      error: 'Invalid navigation_config: maximum 20 navigation items allowed'
    });
  }

  // Validation passed
  next();
}

module.exports = validateNavigationConfig;
