/**
 * @fileoverview Express API server entry point.
 * Configures middleware, mounts routes, and starts HTTP server.
 */

const express = require('express');
const morgan = require('morgan');
const cors = require('cors');
const session = require('express-session');
const cookieParser = require('cookie-parser');
const path = require('path');
require('dotenv').config({ path: path.join(__dirname, '..', '.env') });

const app = express();

// SECURITY: Fail-fast if SESSION_SECRET is not configured
if (!process.env.SESSION_SECRET) {
  console.error('FATAL: SESSION_SECRET environment variable is not set');
  console.error('Please set SESSION_SECRET in your .env file before starting the server');
  process.exit(1);
}

// Middleware
app.use(express.json());
app.use(cors({ origin: 'http://localhost:3000', credentials: true }));
app.use(morgan('dev'));
app.use(cookieParser());

// Session configuration - must be configured before routes
app.use(session({
  secret: process.env.SESSION_SECRET,
  resave: false,
  saveUninitialized: false,
  cookie: {
    secure: false,        // Set to true in production with HTTPS
    httpOnly: true,       // Prevents JavaScript access to cookie (XSS protection)
    maxAge: 24 * 60 * 60 * 1000  // 24 hours
  }
}));

// Import route modules
const authRouter = require('./routes/auth');
const usersRouter = require('./routes/users');
const dealersRouter = require('./routes/dealers');
const vehiclesRouter = require('./routes/vehicles');
const leadsRouter = require('./routes/leads');
const uploadRouter = require('./routes/upload');
const salesRequestsRouter = require('./routes/salesRequests');
const blogsRouter = require('./routes/blogs');
const googleReviewsRouter = require('./routes/googleReviews');
const designTemplatesRouter = require('./routes/designTemplates');

// Mount API routes
app.use('/api/auth', authRouter);
app.use('/api/users', usersRouter);
app.use('/api/dealers', dealersRouter);
app.use('/api/vehicles', vehiclesRouter);
app.use('/api/leads', leadsRouter);
app.use('/api/upload', uploadRouter);
app.use('/api/sales-requests', salesRequestsRouter);
app.use('/api/blogs', blogsRouter);
app.use('/api/google-reviews', googleReviewsRouter);
app.use('/api/design-templates', designTemplatesRouter);

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
