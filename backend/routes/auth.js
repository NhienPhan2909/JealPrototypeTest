/**
 * @fileoverview Authentication API routes.
 * Handles login, logout, and authentication status checks for hierarchical user system.
 *
 * Routes:
 * - POST   /api/auth/login   - Authenticate user with username/password
 * - POST   /api/auth/logout  - Destroy session and log out
 * - GET    /api/auth/me      - Get current authenticated user info
 */

const express = require('express');
const router = express.Router();
const userDb = require('../db/users');

/**
 * Login endpoint - validates credentials against database.
 * Sets session.user with full user object on success.
 *
 * @route POST /api/auth/login
 * @param {Object} req.body - Request body containing username and password
 * @param {string} req.body.username - Username to validate
 * @param {string} req.body.password - Password to validate
 * @returns {Object} JSON response with user data (excluding sensitive info)
 */
router.post('/login', async (req, res) => {
  try {
    const { username, password } = req.body;

    // Validate required fields
    if (!username || !password) {
      return res.status(400).json({ error: 'Username and password are required' });
    }

    // Find user by username
    const user = await userDb.findByUsername(username);
    if (!user) {
      return res.status(401).json({ error: 'Invalid username or password' });
    }

    // Verify password
    const isValid = await userDb.verifyPassword(password, user.password_hash);
    if (!isValid) {
      return res.status(401).json({ error: 'Invalid username or password' });
    }

    // Store user in session (exclude password_hash)
    const { password_hash, ...userWithoutPassword } = user;
    req.session.user = userWithoutPassword;

    return res.json({ 
      success: true, 
      message: 'Logged in successfully',
      user: userWithoutPassword
    });
  } catch (error) {
    console.error('Login error:', error);
    return res.status(500).json({ error: 'Internal server error' });
  }
});

/**
 * Logout endpoint - destroys session and clears authentication state.
 *
 * @route POST /api/auth/logout
 * @returns {Object} JSON response with success status and message
 */
router.post('/logout', (req, res) => {
  try {
    req.session.destroy((err) => {
      if (err) {
        console.error('Session destruction error:', err);
        return res.status(500).json({ error: 'Failed to log out' });
      }
      return res.json({ success: true, message: 'Logged out successfully' });
    });
  } catch (error) {
    console.error('Logout error:', error);
    return res.status(500).json({ error: 'Internal server error' });
  }
});

/**
 * Authentication status check endpoint.
 * Returns current user information if authenticated.
 *
 * @route GET /api/auth/me
 * @returns {Object} JSON response with authenticated status and user data
 */
router.get('/me', (req, res) => {
  if (req.session.user) {
    return res.json({ 
      authenticated: true,
      user: req.session.user
    });
  }
  return res.json({ authenticated: false });
});

module.exports = router;
