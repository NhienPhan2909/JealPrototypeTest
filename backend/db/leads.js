/**
 * @fileoverview Lead database query functions.
 * Handles lead creation and retrieval with multi-tenant filtering.
 *
 * SECURITY (SEC-001): All queries enforce dealership_id filtering to prevent
 * cross-dealership data leaks. Vehicle ownership validation ensures leads only
 * reference vehicles belonging to the specified dealership.
 *
 * Functions:
 * - getAll(dealershipId) - Retrieve all leads for a dealership, sorted newest first
 * - create(leadData) - Create new lead with validation
 * - validateVehicleOwnership(vehicleId, dealershipId) - Verify vehicle belongs to dealership
 */

const pool = require('./index');

/**
 * Retrieves all leads for a specific dealership, sorted by newest first.
 *
 * SECURITY: ALWAYS filters by dealership_id to enforce multi-tenancy isolation (SEC-001).
 * This prevents dealerships from viewing leads belonging to other dealerships.
 *
 * @param {number} dealershipId - The dealership ID to filter leads (REQUIRED)
 * @returns {Promise<Array<Object>>} Array of lead objects sorted by created_at DESC
 * @throws {Error} If database query fails
 *
 * @example
 * const leads = await getAll(1); // Get all leads for dealership 1, newest first
 */
async function getAll(dealershipId) {
  // Query leads filtered by dealership_id, sorted newest first for admin inbox
  const result = await pool.query(
    'SELECT * FROM lead WHERE dealership_id = $1 ORDER BY created_at DESC',
    [dealershipId]
  );
  return result.rows;
}

/**
 * Creates a new lead record.
 *
 * SECURITY: Validates dealership_id is present to enforce multi-tenancy.
 * SECURITY: If vehicle_id provided, caller must validate vehicle belongs to specified dealership.
 *
 * @param {Object} leadData - Lead data object
 * @param {number} leadData.dealership_id - Dealership ID (REQUIRED)
 * @param {number} [leadData.vehicle_id] - Vehicle ID (optional, nullable for general enquiries)
 * @param {string} leadData.name - Customer name (REQUIRED)
 * @param {string} leadData.email - Customer email (REQUIRED, validated format)
 * @param {string} leadData.phone - Customer phone (REQUIRED)
 * @param {string} leadData.message - Enquiry message (REQUIRED)
 * @returns {Promise<Object>} Created lead object with generated ID and created_at timestamp
 * @throws {Error} If database query fails
 *
 * @example
 * const lead = await create({
 *   dealership_id: 1,
 *   vehicle_id: 5,
 *   name: 'John Doe',
 *   email: 'john@example.com',
 *   phone: '(555) 111-2222',
 *   message: 'Interested in this vehicle'
 * });
 */
async function create(leadData) {
  const { dealership_id, vehicle_id, name, email, phone, message } = leadData;

  // Insert new lead record, return created row with generated id and created_at
  const result = await pool.query(
    `INSERT INTO lead (dealership_id, vehicle_id, name, email, phone, message)
     VALUES ($1, $2, $3, $4, $5, $6)
     RETURNING *`,
    [dealership_id, vehicle_id || null, name, email, phone, message]
  );

  return result.rows[0];
}

/**
 * Validates that a vehicle exists and belongs to the specified dealership.
 *
 * SECURITY: Critical for multi-tenancy - prevents leads from referencing vehicles
 * from other dealerships (cross-tenant data integrity violation). This ensures
 * a dealership cannot create leads that reference another dealership's vehicles.
 *
 * @param {number} vehicleId - The vehicle ID to validate
 * @param {number} dealershipId - The dealership ID that should own the vehicle
 * @returns {Promise<boolean>} True if vehicle exists and belongs to dealership, false otherwise
 * @throws {Error} If database query fails
 *
 * @example
 * const isValid = await validateVehicleOwnership(5, 1); // Check if vehicle 5 belongs to dealership 1
 */
async function validateVehicleOwnership(vehicleId, dealershipId) {
  // Query vehicle with both id and dealership_id to ensure ownership
  const result = await pool.query(
    'SELECT id FROM vehicle WHERE id = $1 AND dealership_id = $2',
    [vehicleId, dealershipId]
  );
  return result.rows.length > 0;
}

/**
 * Updates the status of a lead.
 *
 * SECURITY: Validates lead belongs to specified dealership to enforce multi-tenancy (SEC-001).
 *
 * @param {number} leadId - The lead ID to update
 * @param {number} dealershipId - The dealership ID that owns the lead
 * @param {string} status - New status ('received', 'in progress', 'done')
 * @returns {Promise<Object|null>} Updated lead object or null if not found/not owned
 * @throws {Error} If database query fails
 */
async function updateStatus(leadId, dealershipId, status) {
  const result = await pool.query(
    `UPDATE lead 
     SET status = $1 
     WHERE id = $2 AND dealership_id = $3 
     RETURNING *`,
    [status, leadId, dealershipId]
  );
  return result.rows[0] || null;
}

/**
 * Deletes a lead.
 *
 * SECURITY: Validates lead belongs to specified dealership to enforce multi-tenancy (SEC-001).
 *
 * @param {number} leadId - The lead ID to delete
 * @param {number} dealershipId - The dealership ID that owns the lead
 * @returns {Promise<boolean>} True if deleted, false if not found/not owned
 * @throws {Error} If database query fails
 */
async function deleteLead(leadId, dealershipId) {
  const result = await pool.query(
    'DELETE FROM lead WHERE id = $1 AND dealership_id = $2 RETURNING id',
    [leadId, dealershipId]
  );
  return result.rows.length > 0;
}

module.exports = {
  getAll,
  create,
  validateVehicleOwnership,
  updateStatus,
  deleteLead
};
