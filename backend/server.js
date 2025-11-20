/**
 * @fileoverview Express API server entry point.
 * Configures middleware, mounts routes, and starts HTTP server.
 */

const express = require('express');
const morgan = require('morgan');
const cors = require('cors');
require('dotenv').config();

const app = express();

// Middleware
app.use(express.json());
app.use(cors({ origin: 'http://localhost:3000', credentials: true }));
app.use(morgan('dev'));

/**
 * Health check endpoint - verifies server is running.
 *
 * @route GET /api/health
 * @returns {Object} JSON object with status and ISO timestamp
 *
 * @example
 * GET /api/health
 * Response: { "status": "ok", "timestamp": "2025-11-20T12:34:56.789Z" }
 */
app.get('/api/health', (req, res) => {
  res.json({ status: 'ok', timestamp: new Date().toISOString() });
});

// Start server
const PORT = process.env.PORT || 5000;
app.listen(PORT, () => console.log(`Server running on port ${PORT}`));
