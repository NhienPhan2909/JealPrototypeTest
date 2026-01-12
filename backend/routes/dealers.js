/**
 * @fileoverview Dealership CRUD API routes.
 * Handles all dealership-related operations for public website and admin CMS.
 *
 * Routes:
 * - GET    /api/dealers       - List all dealerships
 * - GET    /api/dealers/:id   - Get single dealership
 * - PUT    /api/dealers/:id   - Update dealership (auth not required for MVP)
 */

const express = require('express');
const router = express.Router();
const dealersDb = require('../db/dealers');
const validateNavigationConfig = require('../middleware/validateNavigationConfig');
const { requireAuth, requirePermission, enforceDealershipScope } = require('../middleware/auth');

/**
 * Email validation regex.
 * Basic format check: requires @ symbol and domain with TLD (e.g., user@example.com)
 */
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

/**
 * Field length limits for input validation (defense-in-depth).
 * Prevents database errors and potential DoS from oversized inputs.
 */
const FIELD_LIMITS = {
  name: 255,
  address: 255,
  phone: 20,
  email: 255,
  hours: 500,
  finance_policy: 2000,
  warranty_policy: 2000,
  about: 2000,
  theme_color: 7, // Hex color format: #RRGGBB
  secondary_theme_color: 7, // Hex color format: #RRGGBB
  body_background_color: 7, // Hex color format: #RRGGBB
  font_family: 100, // Font identifier (e.g., 'times', 'arial', 'system')
  finance_promo_text: 500,
  warranty_promo_text: 500
};

/**
 * Sanitizes user input to prevent XSS attacks by escaping HTML special characters.
 *
 * SECURITY: This function prevents stored XSS attacks when user-provided data
 * is displayed in HTML contexts. Escapes <, >, &, ", ' characters.
 *
 * @param {string} input - User-provided string to sanitize
 * @returns {string} Sanitized string safe for HTML display
 *
 * @example
 * sanitizeInput("<script>alert('xss')</script>")
 * // Returns: "&lt;script&gt;alert(&#x27;xss&#x27;)&lt;/script&gt;"
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
 * Validates email format using basic regex.
 *
 * @param {string} email - Email address to validate
 * @returns {boolean} True if email format is valid, false otherwise
 */
function validateEmailFormat(email) {
  return EMAIL_REGEX.test(email);
}

/**
 * Validates hex color format.
 * Accepts formats: #RGB or #RRGGBB
 *
 * @param {string} color - Hex color code to validate
 * @returns {boolean} True if color format is valid, false otherwise
 */
function validateColorFormat(color) {
  const HEX_COLOR_REGEX = /^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$/;
  return HEX_COLOR_REGEX.test(color);
}

/**
 * Validates field lengths against defined limits.
 *
 * SECURITY: Defense-in-depth - prevents oversized inputs that could cause
 * database errors or potential DoS attacks.
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
 * GET /api/dealers - List all dealerships.
 * Used for admin dealership selector dropdown and public site.
 *
 * @returns {Array<Object>} Array of dealership objects with all fields
 * @throws {500} If database query fails
 */
router.get('/', async (req, res) => {
  try {
    const dealers = await dealersDb.getAll();
    res.json(dealers);
  } catch (error) {
    console.error('Error fetching dealers:', error);
    res.status(500).json({ error: 'Failed to fetch dealerships' });
  }
});

/**
 * GET /api/dealers/:id - Get single dealership by ID.
 * Used for public About page and admin settings form.
 *
 * @param {number} req.params.id - Dealership ID
 * @returns {Object} Dealership object with all fields
 * @throws {400} If dealership ID is invalid (non-numeric or negative)
 * @throws {404} If dealership not found
 * @throws {500} If database query fails
 */
router.get('/:id', async (req, res) => {
  try {
    // Validate dealership ID is numeric and positive
    const dealershipId = parseInt(req.params.id, 10);
    if (isNaN(dealershipId) || dealershipId <= 0) {
      return res.status(400).json({ error: 'Dealership ID must be a valid positive number' });
    }

    const dealer = await dealersDb.getById(dealershipId);

    if (!dealer) {
      return res.status(404).json({ error: 'Dealership not found' });
    }

    res.json(dealer);
  } catch (error) {
    console.error('Error fetching dealer:', error);
    res.status(500).json({ error: 'Failed to fetch dealership' });
  }
});

/**
 * PUT /api/dealers/:id - Update dealership profile.
 * Used by admin CMS to update dealership settings.
 *
 * SECURITY Measures:
 * - Input sanitization: Escapes HTML characters in text fields to prevent XSS
 * - Email validation: Validates email format using regex
 * - Length validation: Prevents oversized inputs
 * - Numeric ID validation: Ensures valid positive dealership ID
 *
 * @param {number} req.params.id - Dealership ID
 * @param {Object} req.body - Update fields
 * @param {string} req.body.name - Dealership name (required)
 * @param {string} req.body.address - Street address (required)
 * @param {string} req.body.phone - Phone number (required)
 * @param {string} req.body.email - Email address (required)
 * @param {string} [req.body.logo_url] - Logo image URL (optional)
 * @param {string} [req.body.hours] - Business hours (optional)
 * @param {string} [req.body.finance_policy] - Financing policy text (optional)
 * @param {string} [req.body.warranty_policy] - Warranty policy text (optional)
 * @param {string} [req.body.about] - About text (optional)
 * @param {string} [req.body.hero_background_image] - Hero background image URL (optional)
 * @param {string} [req.body.hero_type] - Hero type: 'image', 'video', or 'carousel' (optional, default: 'image')
 * @param {string} [req.body.hero_video_url] - Hero background video URL (optional)
 * @param {Array<string>} [req.body.hero_carousel_images] - Array of hero carousel image URLs (optional)
 * @param {string} [req.body.theme_color] - Theme color in hex format #RRGGBB (optional, default: #3B82F6)
 * @param {string} [req.body.secondary_theme_color] - Secondary theme color in hex format #RRGGBB (optional, default: #FFFFFF)
 * @param {string} [req.body.body_background_color] - Body background color in hex format #RRGGBB (optional, default: #FFFFFF)
 * @param {string} [req.body.font_family] - Font family identifier (optional, default: 'system')
 * @param {Array<Object>|null} [req.body.navigation_config] - Navigation menu configuration (optional, JSONB array)
 * @param {string} [req.body.facebook_url] - Facebook page URL (optional)
 * @param {string} [req.body.instagram_url] - Instagram profile URL (optional)
 * @param {string} [req.body.finance_promo_image] - Finance promotional panel background image URL (optional)
 * @param {string} [req.body.finance_promo_text] - Finance promotional panel text overlay (optional)
 * @param {string} [req.body.warranty_promo_image] - Warranty promotional panel background image URL (optional)
 * @param {string} [req.body.warranty_promo_text] - Warranty promotional panel text overlay (optional)
 * @returns {Object} Updated dealership object
 * @throws {400} If required fields missing, invalid email format, field too long, or invalid ID
 * @throws {404} If dealership not found
 * @throws {500} If database query fails
 */
router.put('/:id', requireAuth, enforceDealershipScope, requirePermission('settings'), validateNavigationConfig, async (req, res) => {
  try {
    // Validate dealership ID is numeric and positive
    const dealershipId = parseInt(req.params.id, 10);
    if (isNaN(dealershipId) || dealershipId <= 0) {
      return res.status(400).json({ error: 'Dealership ID must be a valid positive number' });
    }

    const { name, address, phone, email, logo_url, hours, finance_policy, warranty_policy, about, hero_background_image, hero_type, hero_video_url, hero_carousel_images, theme_color, secondary_theme_color, body_background_color, font_family, navigation_config, facebook_url, instagram_url, finance_promo_image, finance_promo_text, warranty_promo_image, warranty_promo_text } = req.body;

    // Check if this is a partial update (only updating optional fields like navigation_config)
    const isPartialUpdate = !name && !address && !phone && !email;

    // Input validation - check required fields only if updating basic info
    if (!isPartialUpdate) {
      if (!name || !address || !phone || !email) {
        return res.status(400).json({
          error: 'Missing required fields: name, address, phone, email'
        });
      }
    }

    // Validate field lengths (defense-in-depth)
    const lengthValidation = validateFieldLengths(req.body);
    if (lengthValidation) {
      return res.status(400).json(lengthValidation);
    }

    // Validate email format (only if email is provided)
    if (email && !validateEmailFormat(email)) {
      return res.status(400).json({ error: 'Invalid email format' });
    }

    // Validate theme color format (if provided)
    if (theme_color && !validateColorFormat(theme_color)) {
      return res.status(400).json({ error: 'Invalid theme color format. Use hex format: #RRGGBB or #RGB' });
    }

    // Validate secondary theme color format (if provided)
    if (secondary_theme_color && !validateColorFormat(secondary_theme_color)) {
      return res.status(400).json({ error: 'Invalid secondary theme color format. Use hex format: #RRGGBB or #RGB' });
    }

    // Validate body background color format (if provided)
    if (body_background_color && !validateColorFormat(body_background_color)) {
      return res.status(400).json({ error: 'Invalid body background color format. Use hex format: #RRGGBB or #RGB' });
    }

    // Sanitize user inputs to prevent XSS attacks (only for provided fields)
    const sanitizedName = name ? sanitizeInput(name) : undefined;
    const sanitizedAddress = address ? sanitizeInput(address) : undefined;
    const sanitizedPhone = phone ? sanitizeInput(phone) : undefined;
    const sanitizedHours = hours ? sanitizeInput(hours) : undefined;
    const sanitizedFinancePolicy = finance_policy ? sanitizeInput(finance_policy) : undefined;
    const sanitizedWarrantyPolicy = warranty_policy ? sanitizeInput(warranty_policy) : undefined;
    const sanitizedAbout = about ? sanitizeInput(about) : undefined;
    const sanitizedFinancePromoText = finance_promo_text ? sanitizeInput(finance_promo_text) : undefined;
    const sanitizedWarrantyPromoText = warranty_promo_text ? sanitizeInput(warranty_promo_text) : undefined;

    // Build update object with only provided fields
    const updateData = {};
    if (sanitizedName !== undefined) updateData.name = sanitizedName;
    if (sanitizedAddress !== undefined) updateData.address = sanitizedAddress;
    if (sanitizedPhone !== undefined) updateData.phone = sanitizedPhone;
    if (email !== undefined) updateData.email = email;
    if (logo_url !== undefined) updateData.logo_url = logo_url;
    if (sanitizedHours !== undefined) updateData.hours = sanitizedHours;
    if (sanitizedFinancePolicy !== undefined) updateData.finance_policy = sanitizedFinancePolicy;
    if (sanitizedWarrantyPolicy !== undefined) updateData.warranty_policy = sanitizedWarrantyPolicy;
    if (sanitizedAbout !== undefined) updateData.about = sanitizedAbout;
    if (hero_background_image !== undefined) updateData.hero_background_image = hero_background_image;
    if (hero_type !== undefined) updateData.hero_type = hero_type;
    if (hero_video_url !== undefined) updateData.hero_video_url = hero_video_url;
    if (hero_carousel_images !== undefined) updateData.hero_carousel_images = hero_carousel_images;
    if (theme_color !== undefined) updateData.theme_color = theme_color;
    if (secondary_theme_color !== undefined) updateData.secondary_theme_color = secondary_theme_color;
    if (body_background_color !== undefined) updateData.body_background_color = body_background_color;
    if (font_family !== undefined) updateData.font_family = font_family;
    if (navigation_config !== undefined) updateData.navigation_config = navigation_config;
    if (facebook_url !== undefined) updateData.facebook_url = facebook_url;
    if (instagram_url !== undefined) updateData.instagram_url = instagram_url;
    if (finance_promo_image !== undefined) updateData.finance_promo_image = finance_promo_image;
    if (sanitizedFinancePromoText !== undefined) updateData.finance_promo_text = sanitizedFinancePromoText;
    if (warranty_promo_image !== undefined) updateData.warranty_promo_image = warranty_promo_image;
    if (sanitizedWarrantyPromoText !== undefined) updateData.warranty_promo_text = sanitizedWarrantyPromoText;

    // Update dealership with sanitized data
    const updatedDealer = await dealersDb.update(dealershipId, updateData);

    if (!updatedDealer) {
      return res.status(404).json({ error: 'Dealership not found' });
    }

    res.json(updatedDealer);
  } catch (error) {
    console.error('Error updating dealer:', error);
    res.status(500).json({ error: 'Failed to update dealership' });
  }
});

module.exports = router;
