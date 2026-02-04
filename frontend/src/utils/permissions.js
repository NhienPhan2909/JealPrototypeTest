/**
 * @fileoverview Permission utility functions for checking user permissions.
 * Used throughout the frontend to determine what sections users can access.
 */

/**
 * Check if user has a specific permission.
 * Admin and Owner always have all permissions.
 * Staff users check their permissions array.
 * 
 * @param {Object} user - User object from session
 * @param {string} permission - Permission to check (e.g., 'leads', 'vehicles')
 * @returns {boolean} True if user has permission
 */
export function hasPermission(user, permission) {
  if (!user) return false;

  // Admin and Owner have all permissions
  if (user.userType === 'admin' || user.userType === 'dealership_owner') {
    return true;
  }

  // Staff users check their permissions array
  if (user.userType === 'dealership_staff') {
    const permissions = Array.isArray(user.permissions) ? user.permissions : [];
    return permissions.includes(permission);
  }

  return false;
}

/**
 * Check if user can access any admin section.
 * Used to determine if user should even see admin interface.
 * 
 * @param {Object} user - User object from session
 * @returns {boolean} True if user has any admin access
 */
export function hasAnyAdminAccess(user) {
  if (!user) return false;
  return user.userType === 'admin' || 
         user.userType === 'dealership_owner' || 
         user.userType === 'dealership_staff';
}

/**
 * Get list of all permissions user has.
 * 
 * @param {Object} user - User object from session
 * @returns {Array<string>} Array of permission strings
 */
export function getUserPermissions(user) {
  if (!user) return [];

  // Admin and Owner have all permissions
  if (user.userType === 'admin' || user.userType === 'dealership_owner') {
    return ['leads', 'sales_requests', 'vehicles', 'blogs', 'settings'];
  }

  // Staff users return their specific permissions
  if (user.userType === 'dealership_staff') {
    return Array.isArray(user.permissions) ? user.permissions : [];
  }

  return [];
}

/**
 * Check if user is admin.
 * 
 * @param {Object} user - User object from session
 * @returns {boolean} True if user is admin
 */
export function isAdmin(user) {
  return user?.userType === 'admin';
}

/**
 * Check if user is owner.
 * 
 * @param {Object} user - User object from session
 * @returns {boolean} True if user is owner
 */
export function isOwner(user) {
  return user?.userType === 'dealership_owner';
}

/**
 * Check if user is staff.
 * 
 * @param {Object} user - User object from session
 * @returns {boolean} True if user is staff
 */
export function isStaff(user) {
  return user?.userType === 'dealership_staff';
}

/**
 * Check if user can manage other users.
 * Only admin and owner can manage users.
 * 
 * @param {Object} user - User object from session
 * @returns {boolean} True if user can manage users
 */
export function canManageUsers(user) {
  return user?.userType === 'admin' || user?.userType === 'dealership_owner';
}
