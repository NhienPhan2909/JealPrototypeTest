/**
 * @fileoverview PostgreSQL database connection pool.
 * Establishes connection to Railway PostgreSQL database with SSL support for production.
 *
 * This module creates a connection pool for efficient database access across the application.
 * All database queries should use this pool instance to leverage connection reuse and pooling.
 *
 * Environment Variables Required:
 * - DATABASE_URL: PostgreSQL connection string (format: postgresql://user:password@host:port/database)
 * - NODE_ENV: Environment mode ('production' enables SSL with rejectUnauthorized: false for Railway/Render)
 */

const { Pool } = require('pg');

/**
 * PostgreSQL connection pool instance.
 * Configured with SSL for production environments (Railway/Render requirement).
 *
 * @type {Pool}
 */
const pool = new Pool({
  connectionString: process.env.DATABASE_URL,
  ssl: process.env.NODE_ENV === 'production'
    ? { rejectUnauthorized: false }
    : false
});

/**
 * Test database connection on module load.
 * Logs success or error to console for startup verification.
 */
pool.query('SELECT NOW()', (err, res) => {
  if (err) {
    console.error('Database connection error:', err);
  } else {
    console.log('Database connected successfully at', res.rows[0].now);
  }
});

module.exports = pool;
