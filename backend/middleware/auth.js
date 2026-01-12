/**
 * @fileoverview Authentication and authorization middleware for the hierarchical user system.
 * Provides authentication checks and role-based access control.
 */

/**
 * Middleware to require authentication for protected routes.
 * Checks if user has valid session with authenticated user data.
 *
 * @param {Object} req - Express request object
 * @param {Object} res - Express response object
 * @param {Function} next - Express next middleware function
 * @returns {void}
 */
function requireAuth(req, res, next) {
  if (!req.session.user) {
    return res.status(401).json({ error: 'Authentication required' });
  }
  next();
}

/**
 * Middleware to require admin role.
 * Must be used after requireAuth.
 *
 * @param {Object} req - Express request object
 * @param {Object} res - Express response object
 * @param {Function} next - Express next middleware function
 * @returns {void}
 */
function requireAdmin(req, res, next) {
  if (!req.session.user || req.session.user.user_type !== 'admin') {
    return res.status(403).json({ error: 'Admin access required' });
  }
  next();
}

/**
 * Middleware to require dealership owner role.
 * Must be used after requireAuth.
 *
 * @param {Object} req - Express request object
 * @param {Object} res - Express response object
 * @param {Function} next - Express next middleware function
 * @returns {void}
 */
function requireOwner(req, res, next) {
  if (!req.session.user || req.session.user.user_type !== 'dealership_owner') {
    return res.status(403).json({ error: 'Dealership owner access required' });
  }
  next();
}

/**
 * Middleware to require admin OR dealership owner role.
 * Must be used after requireAuth.
 *
 * @param {Object} req - Express request object
 * @param {Object} res - Express response object
 * @param {Function} next - Express next middleware function
 * @returns {void}
 */
function requireAdminOrOwner(req, res, next) {
  if (!req.session.user || 
      (req.session.user.user_type !== 'admin' && req.session.user.user_type !== 'dealership_owner')) {
    return res.status(403).json({ error: 'Admin or owner access required' });
  }
  next();
}

/**
 * Middleware to enforce dealership scope.
 * Ensures users can only access data from their own dealership.
 * Admin users bypass this check (can access all dealerships).
 * 
 * @param {Object} req - Express request object
 * @param {Object} res - Express response object
 * @param {Function} next - Express next middleware function
 * @returns {void}
 */
function enforceDealershipScope(req, res, next) {
  const user = req.session.user;
  
  // Admin can access all dealerships
  if (user.user_type === 'admin') {
    return next();
  }

  // Get dealership_id from request (body, params, or query)
  const requestedDealershipId = parseInt(
    req.body?.dealership_id || 
    req.params?.dealershipId || 
    req.query?.dealership_id
  );

  // Ensure non-admin users can only access their own dealership
  if (requestedDealershipId && requestedDealershipId !== user.dealership_id) {
    return res.status(403).json({ error: 'Access denied to this dealership' });
  }

  // Inject dealership_id for owner/staff if not provided
  if (!requestedDealershipId) {
    req.dealershipId = user.dealership_id;
  }

  next();
}

/**
 * Middleware to check specific permission.
 * Must be used after requireAuth.
 * 
 * @param {string} permission - Required permission (e.g., 'leads', 'vehicles')
 * @returns {Function} Express middleware function
 */
function requirePermission(permission) {
  return (req, res, next) => {
    const user = req.session.user;

    // Admin and owner have all permissions
    if (user.user_type === 'admin' || user.user_type === 'dealership_owner') {
      return next();
    }

    // Check staff permissions
    if (user.user_type === 'dealership_staff') {
      const permissions = Array.isArray(user.permissions) ? user.permissions : [];
      if (permissions.includes(permission)) {
        return next();
      }
    }

    return res.status(403).json({ error: `Permission '${permission}' required` });
  };
}

module.exports = { 
  requireAuth,
  requireAdmin,
  requireOwner,
  requireAdminOrOwner,
  enforceDealershipScope,
  requirePermission
};
