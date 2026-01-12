/**
 * @fileoverview User database query functions.
 * Provides CRUD operations for the hierarchical user system.
 * 
 * User Types:
 * - admin: Super admin with full access to all dealerships
 * - dealership_owner: Full access to their specific dealership, can manage staff
 * - dealership_staff: Limited access based on assigned permissions
 */

const pool = require('./index');
const bcrypt = require('bcrypt');

const SALT_ROUNDS = 10;

/**
 * Creates a new user account.
 * 
 * @param {Object} userData - User data
 * @param {string} userData.username - Unique username for login
 * @param {string} userData.password - Plain text password (will be hashed)
 * @param {string} userData.email - User email
 * @param {string} userData.full_name - User's full name
 * @param {string} userData.user_type - User type: 'admin', 'dealership_owner', or 'dealership_staff'
 * @param {number|null} userData.dealership_id - Dealership ID (required for owner/staff, must be null for admin)
 * @param {number|null} userData.created_by - User ID of creator
 * @param {Array<string>} [userData.permissions=[]] - Permission array for staff users
 * @returns {Promise<Object>} Created user object (without password_hash)
 * @throws {Error} If validation fails or database query fails
 */
async function create(userData) {
  const { username, password, email, full_name, user_type, dealership_id, created_by, permissions = [] } = userData;

  // Hash password
  const password_hash = await bcrypt.hash(password, SALT_ROUNDS);

  const query = `
    INSERT INTO app_user (username, password_hash, email, full_name, user_type, dealership_id, created_by, permissions)
    VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
    RETURNING id, username, email, full_name, user_type, dealership_id, created_by, permissions, is_active, created_at, updated_at
  `;

  const values = [username, password_hash, email, full_name, user_type, dealership_id, created_by, JSON.stringify(permissions)];
  const result = await pool.query(query, values);
  return result.rows[0];
}

/**
 * Finds a user by username (for login).
 * Returns user with password_hash for authentication.
 * 
 * @param {string} username - Username to search for
 * @returns {Promise<Object|null>} User object with password_hash, or null if not found
 */
async function findByUsername(username) {
  const query = 'SELECT * FROM app_user WHERE username = $1 AND is_active = true';
  const result = await pool.query(query, [username]);
  return result.rows[0] || null;
}

/**
 * Finds a user by ID.
 * 
 * @param {number} userId - User ID
 * @returns {Promise<Object|null>} User object (without password_hash), or null if not found
 */
async function findById(userId) {
  const query = `
    SELECT id, username, email, full_name, user_type, dealership_id, created_by, permissions, is_active, created_at, updated_at
    FROM app_user
    WHERE id = $1 AND is_active = true
  `;
  const result = await pool.query(query, [userId]);
  return result.rows[0] || null;
}

/**
 * Gets all users for a specific dealership (for dealership owner to manage staff).
 * 
 * @param {number} dealershipId - Dealership ID
 * @returns {Promise<Array<Object>>} Array of user objects (without password_hash)
 */
async function getByDealership(dealershipId) {
  const query = `
    SELECT id, username, email, full_name, user_type, dealership_id, created_by, permissions, is_active, created_at, updated_at
    FROM app_user
    WHERE dealership_id = $1 AND is_active = true
    ORDER BY user_type, full_name
  `;
  const result = await pool.query(query, [dealershipId]);
  return result.rows;
}

/**
 * Gets all dealership owners (admin only).
 * 
 * @returns {Promise<Array<Object>>} Array of dealership owner objects
 */
async function getAllOwners() {
  const query = `
    SELECT u.id, u.username, u.email, u.full_name, u.user_type, u.dealership_id, u.created_by, u.is_active, u.created_at, u.updated_at,
           d.name as dealership_name
    FROM app_user u
    LEFT JOIN dealership d ON u.dealership_id = d.id
    WHERE u.user_type = 'dealership_owner' AND u.is_active = true
    ORDER BY d.name, u.full_name
  `;
  const result = await pool.query(query);
  return result.rows;
}

/**
 * Gets all users (admin only).
 * 
 * @returns {Promise<Array<Object>>} Array of all user objects
 */
async function getAll() {
  const query = `
    SELECT u.id, u.username, u.email, u.full_name, u.user_type, u.dealership_id, u.created_by, u.permissions, u.is_active, u.created_at, u.updated_at,
           d.name as dealership_name
    FROM app_user u
    LEFT JOIN dealership d ON u.dealership_id = d.id
    WHERE u.is_active = true
    ORDER BY u.user_type, d.name, u.full_name
  `;
  const result = await pool.query(query);
  return result.rows;
}

/**
 * Updates user information.
 * Cannot update user_type or dealership_id (security constraint).
 * 
 * @param {number} userId - User ID to update
 * @param {Object} updates - Fields to update
 * @param {string} [updates.email] - New email
 * @param {string} [updates.full_name] - New full name
 * @param {Array<string>} [updates.permissions] - New permissions (staff only)
 * @returns {Promise<Object|null>} Updated user object, or null if not found
 */
async function update(userId, updates) {
  const fields = [];
  const values = [];
  let paramIndex = 1;

  if (updates.email !== undefined) {
    fields.push(`email = $${paramIndex++}`);
    values.push(updates.email);
  }
  if (updates.full_name !== undefined) {
    fields.push(`full_name = $${paramIndex++}`);
    values.push(updates.full_name);
  }
  if (updates.permissions !== undefined) {
    fields.push(`permissions = $${paramIndex++}`);
    values.push(JSON.stringify(updates.permissions));
  }

  if (fields.length === 0) {
    throw new Error('No fields to update');
  }

  fields.push(`updated_at = NOW()`);
  values.push(userId);

  const query = `
    UPDATE app_user
    SET ${fields.join(', ')}
    WHERE id = $${paramIndex}
    RETURNING id, username, email, full_name, user_type, dealership_id, created_by, permissions, is_active, created_at, updated_at
  `;

  const result = await pool.query(query, values);
  return result.rows[0] || null;
}

/**
 * Updates user password.
 * 
 * @param {number} userId - User ID
 * @param {string} newPassword - New plain text password (will be hashed)
 * @returns {Promise<boolean>} True if update successful
 */
async function updatePassword(userId, newPassword) {
  const password_hash = await bcrypt.hash(newPassword, SALT_ROUNDS);
  const query = `
    UPDATE app_user
    SET password_hash = $1, updated_at = NOW()
    WHERE id = $2 AND is_active = true
  `;
  const result = await pool.query(query, [password_hash, userId]);
  return result.rowCount > 0;
}

/**
 * Deletes a user from the database (hard delete).
 * 
 * @param {number} userId - User ID to delete
 * @returns {Promise<boolean>} True if deletion successful
 */
async function deactivate(userId) {
  const query = `
    DELETE FROM app_user
    WHERE id = $1
  `;
  const result = await pool.query(query, [userId]);
  return result.rowCount > 0;
}

/**
 * Verifies user password.
 * 
 * @param {string} plainPassword - Plain text password to verify
 * @param {string} hashedPassword - Hashed password from database
 * @returns {Promise<boolean>} True if password matches
 */
async function verifyPassword(plainPassword, hashedPassword) {
  return await bcrypt.compare(plainPassword, hashedPassword);
}

/**
 * Checks if user has specific permission.
 * 
 * @param {Object} user - User object
 * @param {string} permission - Permission to check (e.g., 'leads', 'vehicles')
 * @returns {boolean} True if user has permission
 */
function hasPermission(user, permission) {
  // Admin and dealership_owner have all permissions
  if (user.user_type === 'admin' || user.user_type === 'dealership_owner') {
    return true;
  }

  // Staff users check their permissions array
  if (user.user_type === 'dealership_staff') {
    const permissions = Array.isArray(user.permissions) ? user.permissions : [];
    return permissions.includes(permission);
  }

  return false;
}

module.exports = {
  create,
  findByUsername,
  findById,
  getByDealership,
  getAllOwners,
  getAll,
  update,
  updatePassword,
  deactivate,
  verifyPassword,
  hasPermission
};
