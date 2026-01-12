/**
 * @fileoverview Sales Request database query functions.
 * Handles sales request creation and retrieval with multi-tenant filtering.
 *
 * SECURITY (SEC-001): All queries enforce dealership_id filtering to prevent
 * cross-dealership data leaks.
 *
 * Functions:
 * - getAll(dealershipId) - Retrieve all sales requests for a dealership, sorted newest first
 * - create(salesRequestData) - Create new sales request with validation
 * - updateStatus(id, dealershipId, status) - Update sales request status
 * - deleteSalesRequest(id, dealershipId) - Delete sales request
 */

const pool = require('./index');

/**
 * Retrieves all sales requests for a specific dealership, sorted by newest first.
 *
 * SECURITY: ALWAYS filters by dealership_id to enforce multi-tenancy isolation (SEC-001).
 *
 * @param {number} dealershipId - The dealership ID to filter sales requests (REQUIRED)
 * @returns {Promise<Array<Object>>} Array of sales request objects sorted by created_at DESC
 * @throws {Error} If database query fails
 */
async function getAll(dealershipId) {
  const result = await pool.query(
    'SELECT * FROM sales_request WHERE dealership_id = $1 ORDER BY created_at DESC',
    [dealershipId]
  );
  return result.rows;
}

/**
 * Creates a new sales request record.
 *
 * SECURITY: Validates dealership_id is present to enforce multi-tenancy.
 *
 * @param {Object} salesRequestData - Sales request data object
 * @param {number} salesRequestData.dealership_id - Dealership ID (REQUIRED)
 * @param {string} salesRequestData.name - Customer name (REQUIRED)
 * @param {string} salesRequestData.email - Customer email (REQUIRED)
 * @param {string} salesRequestData.phone - Customer phone (REQUIRED)
 * @param {string} salesRequestData.make - Vehicle make (REQUIRED)
 * @param {string} salesRequestData.model - Vehicle model (REQUIRED)
 * @param {number} salesRequestData.year - Vehicle year (REQUIRED)
 * @param {number} salesRequestData.kilometers - Vehicle kilometers (REQUIRED)
 * @param {string} [salesRequestData.additional_message] - Additional message (OPTIONAL)
 * @returns {Promise<Object>} Created sales request object with generated ID and created_at timestamp
 * @throws {Error} If database query fails
 */
async function create(salesRequestData) {
  const { 
    dealership_id, 
    name, 
    email, 
    phone, 
    make, 
    model, 
    year, 
    kilometers, 
    additional_message 
  } = salesRequestData;

  const result = await pool.query(
    `INSERT INTO sales_request 
     (dealership_id, name, email, phone, make, model, year, kilometers, additional_message)
     VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)
     RETURNING *`,
    [dealership_id, name, email, phone, make, model, year, kilometers, additional_message || null]
  );

  return result.rows[0];
}

/**
 * Updates the status of a sales request.
 *
 * SECURITY: Validates sales request belongs to specified dealership to enforce multi-tenancy (SEC-001).
 *
 * @param {number} id - The sales request ID to update
 * @param {number} dealershipId - The dealership ID that owns the sales request
 * @param {string} status - New status ('received', 'in progress', 'done')
 * @returns {Promise<Object|null>} Updated sales request object or null if not found/not owned
 * @throws {Error} If database query fails
 */
async function updateStatus(id, dealershipId, status) {
  const result = await pool.query(
    `UPDATE sales_request 
     SET status = $1 
     WHERE id = $2 AND dealership_id = $3 
     RETURNING *`,
    [status, id, dealershipId]
  );
  return result.rows[0] || null;
}

/**
 * Deletes a sales request.
 *
 * SECURITY: Validates sales request belongs to specified dealership to enforce multi-tenancy (SEC-001).
 *
 * @param {number} id - The sales request ID to delete
 * @param {number} dealershipId - The dealership ID that owns the sales request
 * @returns {Promise<boolean>} True if deleted, false if not found/not owned
 * @throws {Error} If database query fails
 */
async function deleteSalesRequest(id, dealershipId) {
  const result = await pool.query(
    'DELETE FROM sales_request WHERE id = $1 AND dealership_id = $2 RETURNING id',
    [id, dealershipId]
  );
  return result.rows.length > 0;
}

module.exports = {
  getAll,
  create,
  updateStatus,
  deleteSalesRequest
};
