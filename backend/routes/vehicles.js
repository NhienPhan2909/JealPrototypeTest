/**
 * @fileoverview Vehicle CRUD API routes.
 * Handles all vehicle-related operations with multi-tenant filtering.
 *
 * SECURITY (SEC-001): All endpoints require dealershipId query parameter to enforce
 * multi-tenant data isolation and prevent cross-dealership access.
 *
 * Routes:
 * - GET    /api/vehicles?dealershipId=<id>              - List vehicles for dealership
 * - GET    /api/vehicles/:id?dealershipId=<id>          - Get single vehicle (with ownership check)
 * - POST   /api/vehicles                                 - Create vehicle (auth required)
 * - PUT    /api/vehicles/:id?dealershipId=<id>          - Update vehicle (auth required, with ownership check)
 * - DELETE /api/vehicles/:id?dealershipId=<id>          - Delete vehicle (auth required, with ownership check)
 */

const express = require('express');
const router = express.Router();
const vehiclesDb = require('../db/vehicles');
const { requireAuth, requirePermission, enforceDealershipScope } = require('../middleware/auth');

/**
 * Field length limits for input validation (defense-in-depth).
 * Prevents database errors and potential DoS from oversized inputs.
 */
const FIELD_LIMITS = {
  make: 100,
  model: 100,
  title: 200,
  description: 2000,
  condition: 10,
  status: 10
};

/**
 * Valid enum values for vehicle fields
 */
const VALID_CONDITIONS = ['new', 'used'];
const VALID_STATUSES = ['active', 'sold', 'pending', 'draft'];

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
 * Validates numeric fields are valid positive numbers.
 *
 * @param {Object} data - Data object with numeric fields
 * @returns {Object|null} Error object if validation fails, null if valid
 */
function validateNumericFields(data) {
  const { year, price, mileage } = data;

  if (year !== undefined) {
    const yearNum = parseInt(year, 10);
    if (isNaN(yearNum) || yearNum < 1900 || yearNum > 2100) {
      return { error: 'Year must be a valid number between 1900 and 2100' };
    }
  }

  if (price !== undefined) {
    const priceNum = parseFloat(price);
    if (isNaN(priceNum) || priceNum < 0) {
      return { error: 'Price must be a valid positive number' };
    }
  }

  if (mileage !== undefined) {
    const mileageNum = parseInt(mileage, 10);
    if (isNaN(mileageNum) || mileageNum < 0) {
      return { error: 'Mileage must be a valid positive number' };
    }
  }

  return null;
}

/**
 * Validates enum fields have valid values.
 *
 * @param {Object} data - Data object with enum fields
 * @returns {Object|null} Error object if validation fails, null if valid
 */
function validateEnumFields(data) {
  const { condition, status } = data;

  if (condition && !VALID_CONDITIONS.includes(condition)) {
    return { error: `Condition must be one of: ${VALID_CONDITIONS.join(', ')}` };
  }

  if (status && !VALID_STATUSES.includes(status)) {
    return { error: `Status must be one of: ${VALID_STATUSES.join(', ')}` };
  }

  return null;
}

/**
 * GET /api/vehicles - List vehicles for dealership with optional filters.
 * Used for public inventory page and admin vehicle manager.
 *
 * STORY 4.1 ENHANCEMENT: Added advanced search filters (brand, year range, price range)
 * for homepage search widget. Filters are processed server-side via SQL WHERE clauses.
 *
 * @query {number} dealershipId - Dealership ID to filter vehicles (REQUIRED)
 * @query {string} [status] - Optional status filter ('active', 'sold', 'pending', 'draft')
 * @query {string} [brand] - Optional brand/make filter (case-insensitive)
 * @query {number} [minYear] - Optional minimum year filter (inclusive, 1900-2100)
 * @query {number} [maxYear] - Optional maximum year filter (inclusive, 1900-2100)
 * @query {number} [minPrice] - Optional minimum price filter (inclusive, non-negative)
 * @query {number} [maxPrice] - Optional maximum price filter (inclusive, non-negative)
 * @returns {Array<Object>} Array of vehicle objects matching all filter criteria
 * @throws {400} If dealershipId query parameter is missing or filter validation fails
 * @throws {500} If database query fails
 */
router.get('/', async (req, res) => {
  try {
    const { dealershipId, status, brand, minYear, maxYear, minPrice, maxPrice } = req.query;

    // CRITICAL: Validate dealershipId is present
    if (!dealershipId) {
      return res.status(400).json({ error: 'dealershipId query parameter is required' });
    }

    // Validate dealershipId is numeric and positive
    const dealershipIdNum = parseInt(dealershipId, 10);
    if (isNaN(dealershipIdNum) || dealershipIdNum <= 0) {
      return res.status(400).json({ error: 'dealershipId must be a valid positive number' });
    }

    // Validate filter parameters (Story 4.1)
    if (minYear) {
      const minYearNum = parseInt(minYear, 10);
      if (isNaN(minYearNum) || minYearNum < 1900 || minYearNum > 2100) {
        return res.status(400).json({ error: 'minYear must be a valid number between 1900 and 2100' });
      }
    }

    if (maxYear) {
      const maxYearNum = parseInt(maxYear, 10);
      if (isNaN(maxYearNum) || maxYearNum < 1900 || maxYearNum > 2100) {
        return res.status(400).json({ error: 'maxYear must be a valid number between 1900 and 2100' });
      }
    }

    if (minPrice) {
      const minPriceNum = parseFloat(minPrice);
      if (isNaN(minPriceNum) || minPriceNum < 0) {
        return res.status(400).json({ error: 'minPrice must be a non-negative number' });
      }
    }

    if (maxPrice) {
      const maxPriceNum = parseFloat(maxPrice);
      if (isNaN(maxPriceNum) || maxPriceNum < 0) {
        return res.status(400).json({ error: 'maxPrice must be a non-negative number' });
      }
    }

    // Build filters object for database layer
    const filters = {
      status,
      brand,
      minYear,
      maxYear,
      minPrice,
      maxPrice
    };

    const vehicles = await vehiclesDb.getAll(dealershipIdNum, filters);
    res.json(vehicles);
  } catch (error) {
    console.error('Error fetching vehicles:', error);
    res.status(500).json({ error: 'Failed to fetch vehicles' });
  }
});

/**
 * GET /api/vehicles/:id - Get single vehicle by ID with dealership verification.
 * Used for public vehicle detail page and admin edit form.
 *
 * @param {number} req.params.id - Vehicle ID
 * @query {number} dealershipId - Dealership ID to verify ownership (REQUIRED)
 * @returns {Object} Vehicle object with all fields including images array
 * @throws {400} If dealershipId query parameter is missing
 * @throws {404} If vehicle not found or belongs to different dealership
 * @throws {500} If database query fails
 */
router.get('/:id', async (req, res) => {
  try {
    const { dealershipId } = req.query;

    // CRITICAL: Validate dealershipId is present
    if (!dealershipId) {
      return res.status(400).json({ error: 'dealershipId query parameter is required' });
    }

    // Validate dealershipId is numeric and positive
    const dealershipIdNum = parseInt(dealershipId, 10);
    if (isNaN(dealershipIdNum) || dealershipIdNum <= 0) {
      return res.status(400).json({ error: 'dealershipId must be a valid positive number' });
    }

    // Validate vehicle ID is numeric and positive
    const vehicleId = parseInt(req.params.id, 10);
    if (isNaN(vehicleId) || vehicleId <= 0) {
      return res.status(400).json({ error: 'Vehicle ID must be a valid positive number' });
    }

    const vehicle = await vehiclesDb.getById(vehicleId, dealershipIdNum);

    if (!vehicle) {
      return res.status(404).json({ error: 'Vehicle not found' });
    }

    res.json(vehicle);
  } catch (error) {
    console.error('Error fetching vehicle:', error);
    res.status(500).json({ error: 'Failed to fetch vehicle' });
  }
});

/**
 * POST /api/vehicles - Create new vehicle.
 * Used by admin CMS to add vehicles to inventory.
 * Auth will be required in Story 1.7 (deferred for MVP).
 *
 * SECURITY Measures:
 * - Input sanitization: Escapes HTML characters in text fields to prevent XSS
 * - Numeric validation: Validates year, price, mileage are valid numbers
 * - Enum validation: Validates condition and status have valid values
 * - Length validation: Prevents oversized inputs
 *
 * @body {Object} req.body - Vehicle data
 * @body {number} req.body.dealership_id - Dealership ID (REQUIRED)
 * @body {string} req.body.make - Vehicle make (REQUIRED)
 * @body {string} req.body.model - Vehicle model (REQUIRED)
 * @body {number} req.body.year - Model year (REQUIRED)
 * @body {number} req.body.price - Price in dollars (REQUIRED)
 * @body {number} req.body.mileage - Mileage in miles (REQUIRED)
 * @body {string} req.body.condition - 'new' or 'used' (REQUIRED)
 * @body {string} req.body.status - 'active', 'sold', 'pending', or 'draft' (REQUIRED)
 * @body {string} req.body.title - Display title (REQUIRED)
 * @body {string} [req.body.description] - Full description (optional)
 * @body {Array<string>} [req.body.images] - Array of Cloudinary URLs (optional)
 * @returns {Object} Created vehicle object with generated ID
 * @throws {400} If required fields missing, invalid values, or field too long
 * @throws {500} If database query fails
 */
router.post('/', requireAuth, enforceDealershipScope, requirePermission('vehicles'), async (req, res) => {
  try {
    const { dealership_id, make, model, year, price, mileage, condition, status, title, description, images } = req.body;

    // Input validation - check required fields
    if (!dealership_id || !make || !model || !year || !price || !mileage || !condition || !status || !title) {
      return res.status(400).json({
        error: 'Missing required fields: dealership_id, make, model, year, price, mileage, condition, status, title'
      });
    }

    // Validate dealership_id is numeric and positive
    const dealershipIdNum = parseInt(dealership_id, 10);
    if (isNaN(dealershipIdNum) || dealershipIdNum <= 0) {
      return res.status(400).json({ error: 'dealership_id must be a valid positive number' });
    }

    // Validate field lengths (defense-in-depth)
    const lengthValidation = validateFieldLengths(req.body);
    if (lengthValidation) {
      return res.status(400).json(lengthValidation);
    }

    // Validate numeric fields
    const numericValidation = validateNumericFields(req.body);
    if (numericValidation) {
      return res.status(400).json(numericValidation);
    }

    // Validate enum fields
    const enumValidation = validateEnumFields(req.body);
    if (enumValidation) {
      return res.status(400).json(enumValidation);
    }

    // Sanitize user inputs to prevent XSS attacks
    const sanitizedMake = sanitizeInput(make);
    const sanitizedModel = sanitizeInput(model);
    const sanitizedTitle = sanitizeInput(title);
    const sanitizedDescription = description ? sanitizeInput(description) : description;

    // Create vehicle with sanitized and validated data
    const newVehicle = await vehiclesDb.create(dealershipIdNum, {
      make: sanitizedMake,
      model: sanitizedModel,
      year,
      price,
      mileage,
      condition,
      status,
      title: sanitizedTitle,
      description: sanitizedDescription,
      images
    });

    res.status(201).json(newVehicle);
  } catch (error) {
    console.error('Error creating vehicle:', error);
    res.status(500).json({ error: 'Failed to create vehicle' });
  }
});

/**
 * PUT /api/vehicles/:id - Update existing vehicle with dealership verification.
 * Used by admin CMS to edit vehicle details.
 * Auth will be required in Story 1.7 (deferred for MVP).
 *
 * SECURITY Measures:
 * - Input sanitization: Escapes HTML characters in text fields to prevent XSS
 * - Numeric validation: Validates year, price, mileage are valid numbers
 * - Enum validation: Validates condition and status have valid values
 * - Length validation: Prevents oversized inputs
 * - Multi-tenancy: Verifies vehicle belongs to specified dealership
 *
 * @param {number} req.params.id - Vehicle ID
 * @query {number} dealershipId - Dealership ID to verify ownership (REQUIRED)
 * @body {Object} req.body - Fields to update (all optional)
 * @returns {Object} Updated vehicle object
 * @throws {400} If dealershipId missing, invalid values, or field too long
 * @throws {404} If vehicle not found or belongs to different dealership
 * @throws {500} If database query fails
 */
router.put('/:id', requireAuth, enforceDealershipScope, requirePermission('vehicles'), async (req, res) => {
  try {
    const { dealershipId } = req.query;

    // CRITICAL: Validate dealershipId is present
    if (!dealershipId) {
      return res.status(400).json({ error: 'dealershipId query parameter is required' });
    }

    // Validate dealershipId is numeric and positive
    const dealershipIdNum = parseInt(dealershipId, 10);
    if (isNaN(dealershipIdNum) || dealershipIdNum <= 0) {
      return res.status(400).json({ error: 'dealershipId must be a valid positive number' });
    }

    // Validate vehicle ID is numeric and positive
    const vehicleId = parseInt(req.params.id, 10);
    if (isNaN(vehicleId) || vehicleId <= 0) {
      return res.status(400).json({ error: 'Vehicle ID must be a valid positive number' });
    }

    // Validate field lengths (defense-in-depth)
    const lengthValidation = validateFieldLengths(req.body);
    if (lengthValidation) {
      return res.status(400).json(lengthValidation);
    }

    // Validate numeric fields (if provided)
    const numericValidation = validateNumericFields(req.body);
    if (numericValidation) {
      return res.status(400).json(numericValidation);
    }

    // Validate enum fields (if provided)
    const enumValidation = validateEnumFields(req.body);
    if (enumValidation) {
      return res.status(400).json(enumValidation);
    }

    // Sanitize text inputs to prevent XSS attacks
    const sanitizedUpdates = { ...req.body };
    if (sanitizedUpdates.make) sanitizedUpdates.make = sanitizeInput(sanitizedUpdates.make);
    if (sanitizedUpdates.model) sanitizedUpdates.model = sanitizeInput(sanitizedUpdates.model);
    if (sanitizedUpdates.title) sanitizedUpdates.title = sanitizeInput(sanitizedUpdates.title);
    if (sanitizedUpdates.description) sanitizedUpdates.description = sanitizeInput(sanitizedUpdates.description);

    const updatedVehicle = await vehiclesDb.update(vehicleId, dealershipIdNum, sanitizedUpdates);

    if (!updatedVehicle) {
      return res.status(404).json({ error: 'Vehicle not found' });
    }

    res.json(updatedVehicle);
  } catch (error) {
    console.error('Error updating vehicle:', error);
    res.status(500).json({ error: 'Failed to update vehicle' });
  }
});

/**
 * DELETE /api/vehicles/:id - Delete vehicle with dealership verification.
 * Used by admin CMS to remove vehicles from inventory.
 * Auth will be required in Story 1.7 (deferred for MVP).
 *
 * SECURITY: Multi-tenancy verification ensures vehicle belongs to specified dealership
 *
 * @param {number} req.params.id - Vehicle ID
 * @query {number} dealershipId - Dealership ID to verify ownership (REQUIRED)
 * @returns {204} No content on successful deletion
 * @throws {400} If dealershipId query parameter is missing or invalid
 * @throws {404} If vehicle not found or belongs to different dealership
 * @throws {500} If database query fails
 */
router.delete('/:id', requireAuth, enforceDealershipScope, requirePermission('vehicles'), async (req, res) => {
  try {
    const { dealershipId } = req.query;

    // CRITICAL: Validate dealershipId is present
    if (!dealershipId) {
      return res.status(400).json({ error: 'dealershipId query parameter is required' });
    }

    // Validate dealershipId is numeric and positive
    const dealershipIdNum = parseInt(dealershipId, 10);
    if (isNaN(dealershipIdNum) || dealershipIdNum <= 0) {
      return res.status(400).json({ error: 'dealershipId must be a valid positive number' });
    }

    // Validate vehicle ID is numeric and positive
    const vehicleId = parseInt(req.params.id, 10);
    if (isNaN(vehicleId) || vehicleId <= 0) {
      return res.status(400).json({ error: 'Vehicle ID must be a valid positive number' });
    }

    const deleted = await vehiclesDb.remove(vehicleId, dealershipIdNum);

    if (!deleted) {
      return res.status(404).json({ error: 'Vehicle not found' });
    }

    res.status(204).send();
  } catch (error) {
    console.error('Error deleting vehicle:', error);
    res.status(500).json({ error: 'Failed to delete vehicle' });
  }
});

module.exports = router;
