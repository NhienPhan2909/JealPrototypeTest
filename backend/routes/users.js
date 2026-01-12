/**
 * @fileoverview User Management API routes.
 * Handles user CRUD operations with proper authorization.
 * 
 * Routes:
 * - GET    /api/users              - Get all users (admin) or dealership users (owner)
 * - GET    /api/users/:id          - Get specific user
 * - POST   /api/users              - Create new user (admin creates owners, owners create staff)
 * - PUT    /api/users/:id          - Update user
 * - PUT    /api/users/:id/password - Update user password
 * - DELETE /api/users/:id          - Delete user (hard delete - permanently removes from database)
 */

const express = require('express');
const router = express.Router();
const userDb = require('../db/users');
const { requireAuth, requireAdmin, requireAdminOrOwner } = require('../middleware/auth');

/**
 * Get all users.
 * - Admin: Gets all users across all dealerships
 * - Owner: Gets only users from their dealership
 * 
 * @route GET /api/users
 */
router.get('/', requireAuth, async (req, res) => {
  try {
    const currentUser = req.session.user;

    let users;
    if (currentUser.user_type === 'admin') {
      // Admin can see all users
      users = await userDb.getAll();
    } else if (currentUser.user_type === 'dealership_owner') {
      // Owner can see users from their dealership
      users = await userDb.getByDealership(currentUser.dealership_id);
    } else {
      // Staff cannot list users
      return res.status(403).json({ error: 'Insufficient permissions' });
    }

    return res.json(users);
  } catch (error) {
    console.error('Get users error:', error);
    return res.status(500).json({ error: 'Failed to retrieve users' });
  }
});

/**
 * Get specific user by ID.
 * - Admin: Can view any user
 * - Owner: Can view users from their dealership
 * - Staff: Can only view themselves
 * 
 * @route GET /api/users/:id
 */
router.get('/:id', requireAuth, async (req, res) => {
  try {
    const userId = parseInt(req.params.id);
    const currentUser = req.session.user;

    const user = await userDb.findById(userId);
    if (!user) {
      return res.status(404).json({ error: 'User not found' });
    }

    // Authorization check
    if (currentUser.user_type === 'admin') {
      // Admin can view any user
      return res.json(user);
    } else if (currentUser.user_type === 'dealership_owner') {
      // Owner can view users from their dealership
      if (user.dealership_id === currentUser.dealership_id) {
        return res.json(user);
      }
    } else if (currentUser.user_type === 'dealership_staff') {
      // Staff can only view themselves
      if (user.id === currentUser.id) {
        return res.json(user);
      }
    }

    return res.status(403).json({ error: 'Access denied' });
  } catch (error) {
    console.error('Get user error:', error);
    return res.status(500).json({ error: 'Failed to retrieve user' });
  }
});

/**
 * Create new user.
 * - Admin: Can create dealership_owner accounts
 * - Owner: Can create dealership_staff accounts for their dealership
 * - Staff: Cannot create accounts
 * 
 * @route POST /api/users
 */
router.post('/', requireAuth, async (req, res) => {
  try {
    const currentUser = req.session.user;
    const { username, password, email, full_name, user_type, dealership_id, permissions } = req.body;

    // Validate required fields
    if (!username || !password || !email || !full_name || !user_type) {
      return res.status(400).json({ error: 'Missing required fields' });
    }

    // Authorization and validation
    if (currentUser.user_type === 'admin') {
      // Admin can create dealership_owner or other admin accounts
      if (user_type === 'dealership_owner' && !dealership_id) {
        return res.status(400).json({ error: 'dealership_id required for dealership_owner' });
      }
      if (user_type === 'admin' && dealership_id) {
        return res.status(400).json({ error: 'admin users cannot have a dealership_id' });
      }
    } else if (currentUser.user_type === 'dealership_owner') {
      // Owner can only create staff for their dealership
      if (user_type !== 'dealership_staff') {
        return res.status(403).json({ error: 'Owners can only create staff accounts' });
      }
      if (dealership_id !== currentUser.dealership_id) {
        return res.status(403).json({ error: 'Can only create staff for your dealership' });
      }
    } else {
      // Staff cannot create accounts
      return res.status(403).json({ error: 'Insufficient permissions' });
    }

    // Create user
    const newUser = await userDb.create({
      username,
      password,
      email,
      full_name,
      user_type,
      dealership_id: user_type === 'admin' ? null : dealership_id,
      created_by: currentUser.id,
      permissions: user_type === 'dealership_staff' ? (permissions || []) : []
    });

    return res.status(201).json(newUser);
  } catch (error) {
    console.error('Create user error:', error);
    if (error.message?.includes('duplicate key') || error.code === '23505') {
      return res.status(400).json({ error: 'Username already exists' });
    }
    return res.status(500).json({ error: 'Failed to create user' });
  }
});

/**
 * Update user information.
 * - Admin: Can update any user
 * - Owner: Can update staff from their dealership
 * - Staff: Can update themselves (limited fields)
 * 
 * @route PUT /api/users/:id
 */
router.put('/:id', requireAuth, async (req, res) => {
  try {
    const userId = parseInt(req.params.id);
    const currentUser = req.session.user;
    const { email, full_name, permissions } = req.body;

    // Get target user
    const targetUser = await userDb.findById(userId);
    if (!targetUser) {
      return res.status(404).json({ error: 'User not found' });
    }

    // Authorization check
    let canUpdate = false;
    let canUpdatePermissions = false;

    if (currentUser.user_type === 'admin') {
      canUpdate = true;
      canUpdatePermissions = true;
    } else if (currentUser.user_type === 'dealership_owner') {
      // Owner can update staff from their dealership
      if (targetUser.dealership_id === currentUser.dealership_id && 
          targetUser.user_type === 'dealership_staff') {
        canUpdate = true;
        canUpdatePermissions = true;
      }
    } else if (currentUser.user_type === 'dealership_staff') {
      // Staff can update themselves (but not permissions)
      if (targetUser.id === currentUser.id) {
        canUpdate = true;
        canUpdatePermissions = false;
      }
    }

    if (!canUpdate) {
      return res.status(403).json({ error: 'Access denied' });
    }

    // Build updates object
    const updates = {};
    if (email !== undefined) updates.email = email;
    if (full_name !== undefined) updates.full_name = full_name;
    if (permissions !== undefined && canUpdatePermissions) {
      updates.permissions = permissions;
    }

    const updatedUser = await userDb.update(userId, updates);
    
    // Update session if user updated themselves
    if (userId === currentUser.id) {
      req.session.user = { ...req.session.user, ...updatedUser };
    }

    return res.json(updatedUser);
  } catch (error) {
    console.error('Update user error:', error);
    return res.status(500).json({ error: 'Failed to update user' });
  }
});

/**
 * Update user password.
 * - Admin: Can update any user's password
 * - Owner: Can update staff passwords from their dealership
 * - Staff: Can update their own password
 * 
 * @route PUT /api/users/:id/password
 */
router.put('/:id/password', requireAuth, async (req, res) => {
  try {
    const userId = parseInt(req.params.id);
    const currentUser = req.session.user;
    const { password } = req.body;

    if (!password || password.length < 6) {
      return res.status(400).json({ error: 'Password must be at least 6 characters' });
    }

    // Get target user
    const targetUser = await userDb.findById(userId);
    if (!targetUser) {
      return res.status(404).json({ error: 'User not found' });
    }

    // Authorization check
    let canUpdate = false;

    if (currentUser.user_type === 'admin') {
      canUpdate = true;
    } else if (currentUser.user_type === 'dealership_owner') {
      // Owner can update staff passwords from their dealership
      if (targetUser.dealership_id === currentUser.dealership_id && 
          targetUser.user_type === 'dealership_staff') {
        canUpdate = true;
      }
    } else if (currentUser.user_type === 'dealership_staff') {
      // Staff can update their own password
      if (targetUser.id === currentUser.id) {
        canUpdate = true;
      }
    }

    if (!canUpdate) {
      return res.status(403).json({ error: 'Access denied' });
    }

    await userDb.updatePassword(userId, password);
    return res.json({ success: true, message: 'Password updated successfully' });
  } catch (error) {
    console.error('Update password error:', error);
    return res.status(500).json({ error: 'Failed to update password' });
  }
});

/**
 * Delete user (hard delete - permanently removes from database).
 * - Admin: Can delete any user
 * - Owner: Can delete staff from their dealership
 * - Staff: Cannot delete users
 * 
 * @route DELETE /api/users/:id
 */
router.delete('/:id', requireAuth, async (req, res) => {
  try {
    const userId = parseInt(req.params.id);
    const currentUser = req.session.user;

    // Cannot delete yourself
    if (userId === currentUser.id) {
      return res.status(400).json({ error: 'Cannot delete your own account' });
    }

    // Get target user
    const targetUser = await userDb.findById(userId);
    if (!targetUser) {
      return res.status(404).json({ error: 'User not found' });
    }

    // Authorization check
    let canDelete = false;

    if (currentUser.user_type === 'admin') {
      canDelete = true;
    } else if (currentUser.user_type === 'dealership_owner') {
      // Owner can delete staff from their dealership
      if (targetUser.dealership_id === currentUser.dealership_id && 
          targetUser.user_type === 'dealership_staff') {
        canDelete = true;
      }
    }

    if (!canDelete) {
      return res.status(403).json({ error: 'Access denied' });
    }

    await userDb.deactivate(userId);
    return res.json({ success: true, message: 'User deleted successfully' });
  } catch (error) {
    console.error('Delete user error:', error);
    return res.status(500).json({ error: 'Failed to delete user' });
  }
});

module.exports = router;
