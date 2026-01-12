/**
 * @fileoverview Vehicle database query functions.
 * Provides CRUD operations for vehicle inventory with multi-tenant data isolation.
 *
 * CRITICAL SECURITY REQUIREMENT:
 * ALL query functions in this module MUST enforce multi-tenancy by filtering on dealership_id.
 * This prevents data leaks between dealerships (SEC-001 mitigation).
 *
 * Anti-Pattern Example (NEVER DO THIS):
 * ❌ SELECT * FROM vehicle WHERE status = 'active'
 *    ^ Missing dealership_id filter - returns ALL dealerships' vehicles!
 *
 * Correct Pattern:
 * ✅ SELECT * FROM vehicle WHERE dealership_id = $1 AND status = $2
 *    ^ Always includes dealership_id filter
 *
 * Routes:
 * - Used by GET    /api/vehicles           - List vehicles for dealership
 * - Used by GET    /api/vehicles/:id       - Get single vehicle
 * - Used by POST   /api/vehicles           - Create vehicle (auth required)
 * - Used by PUT    /api/vehicles/:id       - Update vehicle (auth required)
 * - Used by DELETE /api/vehicles/:id       - Delete vehicle (auth required)
 */

const pool = require('./index');

/**
 * Retrieves all vehicles for a specific dealership with optional filters.
 *
 * SECURITY: dealership_id parameter is MANDATORY to enforce multi-tenant isolation.
 *
 * STORY 4.1 ENHANCEMENT: Enhanced to support advanced search filters (brand, year range, price range).
 * Builds dynamic SQL WHERE clauses based on provided filters for server-side filtering.
 *
 * @param {number} dealershipId - The dealership ID to filter vehicles (REQUIRED)
 * @param {Object} [filters={}] - Optional filters object
 * @param {string} [filters.status] - Status filter ('active', 'sold', 'pending', 'draft')
 * @param {string} [filters.brand] - Brand/make filter (case-insensitive)
 * @param {number} [filters.minYear] - Minimum year filter (inclusive)
 * @param {number} [filters.maxYear] - Maximum year filter (inclusive)
 * @param {number} [filters.minPrice] - Minimum price filter (inclusive)
 * @param {number} [filters.maxPrice] - Maximum price filter (inclusive)
 * @returns {Promise<Array<Object>>} Array of vehicle objects matching all filter criteria
 * @throws {Error} If database query fails or dealershipId is missing
 *
 * @example
 * // Get all active vehicles for dealership 1
 * const vehicles = await getAll(1, { status: 'active' });
 *
 * @example
 * // Get Mazda vehicles from 2015-2025, priced $15k-$25k for dealership 1
 * const filteredVehicles = await getAll(1, {
 *   brand: 'mazda',
 *   minYear: 2015,
 *   maxYear: 2025,
 *   minPrice: 15000,
 *   maxPrice: 25000
 * });
 *
 * @example
 * // Backward compatible: Pass status as second parameter (legacy support)
 * const allVehicles = await getAll(2, { status: 'active' });
 */
async function getAll(dealershipId, filters = {}) {
  if (!dealershipId) {
    throw new Error('dealershipId is required to prevent multi-tenancy data leak');
  }

  // Handle legacy API: if filters is a string (status), convert to object
  if (typeof filters === 'string') {
    filters = { status: filters };
  }

  // Start building dynamic SQL query with multi-tenancy filter
  let query = 'SELECT * FROM vehicle WHERE dealership_id = $1';
  const params = [dealershipId];
  let paramIndex = 2;

  // Add status filter
  if (filters.status) {
    query += ` AND status = $${paramIndex}`;
    params.push(filters.status);
    paramIndex++;
  }

  // Add brand filter (case-insensitive match)
  // SECURITY: Uses parameterized query to prevent SQL injection
  if (filters.brand) {
    query += ` AND LOWER(make) = $${paramIndex}`;
    params.push(filters.brand.toLowerCase());
    paramIndex++;
  }

  // Add minimum year filter (inclusive)
  if (filters.minYear) {
    query += ` AND year >= $${paramIndex}`;
    params.push(parseInt(filters.minYear, 10));
    paramIndex++;
  }

  // Add maximum year filter (inclusive)
  if (filters.maxYear) {
    query += ` AND year <= $${paramIndex}`;
    params.push(parseInt(filters.maxYear, 10));
    paramIndex++;
  }

  // Add minimum price filter (inclusive)
  if (filters.minPrice) {
    query += ` AND price >= $${paramIndex}`;
    params.push(parseFloat(filters.minPrice));
    paramIndex++;
  }

  // Add maximum price filter (inclusive)
  if (filters.maxPrice) {
    query += ` AND price <= $${paramIndex}`;
    params.push(parseFloat(filters.maxPrice));
    paramIndex++;
  }

  // Order by created_at DESC (newest first)
  query += ' ORDER BY created_at DESC';

  const result = await pool.query(query, params);
  return result.rows;
}

/**
 * Retrieves a single vehicle by ID with dealership_id verification.
 *
 * SECURITY: This function enforces that the vehicle belongs to the specified dealership.
 * This prevents Dealership A from accessing Dealership B's vehicle data.
 *
 * @param {number} vehicleId - The vehicle ID to retrieve
 * @param {number} dealershipId - The dealership ID to verify ownership (REQUIRED)
 * @returns {Promise<Object|null>} Vehicle object or null if not found or wrong dealership
 * @throws {Error} If database query fails or dealershipId is missing
 *
 * @example
 * const vehicle = await getById(5, 1);
 * // Returns vehicle 5 ONLY if it belongs to dealership 1
 */
async function getById(vehicleId, dealershipId) {
  if (!dealershipId) {
    throw new Error('dealershipId is required to prevent multi-tenancy data leak');
  }

  // CRITICAL: WHERE clause includes BOTH id AND dealership_id
  const query = 'SELECT * FROM vehicle WHERE id = $1 AND dealership_id = $2';
  const result = await pool.query(query, [vehicleId, dealershipId]);
  return result.rows[0] || null;
}

/**
 * Creates a new vehicle for a specific dealership.
 *
 * SECURITY: dealership_id is explicitly set to prevent vehicle creation for wrong dealership.
 *
 * @param {number} dealershipId - The dealership ID owning this vehicle (REQUIRED)
 * @param {Object} vehicle - Vehicle data object
 * @param {string} vehicle.make - Vehicle make (e.g., 'Toyota')
 * @param {string} vehicle.model - Vehicle model (e.g., 'Camry')
 * @param {number} vehicle.year - Model year
 * @param {number} vehicle.price - Price in dollars
 * @param {number} vehicle.mileage - Mileage in miles
 * @param {string} vehicle.condition - 'new' or 'used'
 * @param {string} vehicle.status - 'active', 'sold', 'pending', or 'draft'
 * @param {string} vehicle.title - Display title
 * @param {string} [vehicle.description] - Full description
 * @param {Array<string>} [vehicle.images] - Array of Cloudinary image URLs
 * @returns {Promise<Object>} Newly created vehicle object with id
 * @throws {Error} If database query fails or dealershipId is missing
 *
 * @example
 * const newVehicle = await create(1, {
 *   make: 'Honda',
 *   model: 'Civic',
 *   year: 2023,
 *   price: 25000,
 *   mileage: 15000,
 *   condition: 'used',
 *   status: 'active',
 *   title: '2023 Honda Civic EX',
 *   description: 'Well-maintained...',
 *   images: ['https://...', 'https://...']
 * });
 */
async function create(dealershipId, vehicle) {
  if (!dealershipId) {
    throw new Error('dealershipId is required to prevent multi-tenancy data leak');
  }

  // Validate JSONB images array structure
  const images = vehicle.images || [];
  if (!Array.isArray(images)) {
    throw new Error('images must be an array of URL strings');
  }

  const query = `
    INSERT INTO vehicle (
      dealership_id, make, model, year, price, mileage,
      condition, status, title, description, images
    )
    VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11)
    RETURNING *
  `;

  const params = [
    dealershipId,
    vehicle.make,
    vehicle.model,
    vehicle.year,
    vehicle.price,
    vehicle.mileage,
    vehicle.condition,
    vehicle.status,
    vehicle.title,
    vehicle.description || null,
    JSON.stringify(images) // Convert array to JSONB
  ];

  const result = await pool.query(query, params);
  return result.rows[0];
}

/**
 * Updates an existing vehicle with dealership_id verification.
 *
 * SECURITY: WHERE clause includes dealership_id to prevent updating another dealership's vehicle.
 *
 * @param {number} vehicleId - The vehicle ID to update
 * @param {number} dealershipId - The dealership ID to verify ownership (REQUIRED)
 * @param {Object} updates - Object containing fields to update (same as create)
 * @returns {Promise<Object|null>} Updated vehicle object or null if not found/wrong dealership
 * @throws {Error} If database query fails or dealershipId is missing
 *
 * @example
 * const updated = await update(5, 1, {
 *   price: 23500,
 *   status: 'pending'
 * });
 */
async function update(vehicleId, dealershipId, updates) {
  if (!dealershipId) {
    throw new Error('dealershipId is required to prevent multi-tenancy data leak');
  }

  // Validate JSONB images array if provided
  if (updates.images !== undefined && !Array.isArray(updates.images)) {
    throw new Error('images must be an array of URL strings');
  }

  // Build dynamic UPDATE query
  const fields = [];
  const values = [];
  let paramIndex = 1;

  const allowedFields = [
    'make', 'model', 'year', 'price', 'mileage',
    'condition', 'status', 'title', 'description', 'images'
  ];

  allowedFields.forEach(field => {
    if (updates[field] !== undefined) {
      fields.push(`${field} = $${paramIndex++}`);
      // Convert images array to JSONB string
      values.push(field === 'images' ? JSON.stringify(updates[field]) : updates[field]);
    }
  });

  if (fields.length === 0) {
    throw new Error('No fields to update');
  }

  // CRITICAL: WHERE clause includes BOTH id AND dealership_id
  values.push(vehicleId, dealershipId);
  const query = `
    UPDATE vehicle
    SET ${fields.join(', ')}
    WHERE id = $${paramIndex++} AND dealership_id = $${paramIndex}
    RETURNING *
  `;

  const result = await pool.query(query, values);
  return result.rows[0] || null;
}

/**
 * Deletes a vehicle with dealership_id verification.
 *
 * SECURITY: WHERE clause includes dealership_id to prevent deleting another dealership's vehicle.
 *
 * @param {number} vehicleId - The vehicle ID to delete
 * @param {number} dealershipId - The dealership ID to verify ownership (REQUIRED)
 * @returns {Promise<boolean>} True if deleted, false if not found/wrong dealership
 * @throws {Error} If database query fails or dealershipId is missing
 *
 * @example
 * const deleted = await remove(5, 1);
 * // Returns true if vehicle 5 belonged to dealership 1 and was deleted
 */
async function remove(vehicleId, dealershipId) {
  if (!dealershipId) {
    throw new Error('dealershipId is required to prevent multi-tenancy data leak');
  }

  // CRITICAL: WHERE clause includes BOTH id AND dealership_id
  const query = 'DELETE FROM vehicle WHERE id = $1 AND dealership_id = $2 RETURNING id';
  const result = await pool.query(query, [vehicleId, dealershipId]);
  return result.rowCount > 0;
}

module.exports = {
  getAll,
  getById,
  create,
  update,
  remove
};
