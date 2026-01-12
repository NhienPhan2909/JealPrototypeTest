/**
 * @fileoverview Cloudinary image upload API route.
 * Handles file uploads for vehicle photos and dealership logos with validation.
 *
 * SECURITY:
 * - File size limited to 5MB via multer configuration
 * - File type restricted to JPG, PNG, WebP via multer file filter
 * - Cloudinary API Secret kept server-side (never exposed to frontend)
 * - Auth middleware will be added in Story 1.7
 *
 * Routes:
 * - POST /api/upload - Upload image file, returns Cloudinary URL
 */

const express = require('express');
const router = express.Router();
const multer = require('multer');
const cloudinary = require('cloudinary').v2;

// Validate required environment variables at startup
const requiredEnvVars = ['CLOUDINARY_CLOUD_NAME', 'CLOUDINARY_API_KEY', 'CLOUDINARY_API_SECRET'];
const missingEnvVars = requiredEnvVars.filter(varName => !process.env[varName]);
if (missingEnvVars.length > 0) {
  console.error(`CLOUDINARY CONFIG ERROR: Missing environment variables: ${missingEnvVars.join(', ')}`);
  console.error('Upload endpoint will not function correctly. Please add these to your .env file.');
}

// Configure Cloudinary
cloudinary.config({
  cloud_name: process.env.CLOUDINARY_CLOUD_NAME,
  api_key: process.env.CLOUDINARY_API_KEY,
  api_secret: process.env.CLOUDINARY_API_SECRET
});

// Configure multer with memory storage and validation
const storage = multer.memoryStorage();
const upload = multer({
  storage: storage,
  limits: {
    fileSize: 5 * 1024 * 1024 // 5MB in bytes
  },
  fileFilter: (req, file, cb) => {
    // Accept only JPG, PNG, WebP
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];

    if (allowedTypes.includes(file.mimetype)) {
      cb(null, true); // Accept file
    } else {
      cb(new Error('Invalid file type. Only JPG, PNG, and WebP are allowed.'), false); // Reject file
    }
  }
});

/**
 * POST /api/upload - Uploads image file to Cloudinary.
 *
 * Accepts multipart/form-data with 'image' field containing file.
 * Validates file size (max 5MB) and type (JPG/PNG/WebP only).
 * Uploads to Cloudinary 'dealership-vehicles' folder with automatic optimization.
 *
 * @route POST /api/upload
 * @param {Object} req - Express request object
 * @param {Object} req.file - Uploaded file object from multer (buffer, mimetype, size)
 * @param {Object} res - Express response object
 * @returns {Object} JSON response with Cloudinary URL: { url: "https://res.cloudinary.com/..." }
 * @throws {400} If no file uploaded, file too large, or invalid file type
 * @throws {500} If Cloudinary upload fails
 *
 * @example
 * // Postman: POST http://localhost:5000/api/upload
 * // Body: form-data with 'image' field (File type)
 * // Response: { "url": "https://res.cloudinary.com/dxyz/image/upload/v123/abc.jpg" }
 */
router.post('/', upload.single('image'), async (req, res) => {
  try {
    // 1. Validate file exists
    if (!req.file) {
      return res.status(400).json({ error: 'No file uploaded' });
    }

    // 2. Convert buffer to base64 data URI for Cloudinary upload
    const fileStr = `data:${req.file.mimetype};base64,${req.file.buffer.toString('base64')}`;

    // 3. Upload to Cloudinary
    const uploadResponse = await cloudinary.uploader.upload(fileStr, {
      folder: 'dealership-vehicles',
      resource_type: 'image'
    });

    // 4. Return secure URL
    res.json({ url: uploadResponse.secure_url });

  } catch (error) {
    console.error('Upload error:', error);

    // Handle multer validation errors
    if (error.code === 'LIMIT_FILE_SIZE') {
      return res.status(400).json({ error: 'File too large. Maximum size is 5MB.' });
    }

    if (error.message && error.message.includes('Invalid file type')) {
      return res.status(400).json({ error: error.message });
    }

    // Handle Cloudinary errors with more specific messages
    if (error.http_code) {
      // Cloudinary-specific error
      console.error('Cloudinary error details:', { code: error.http_code, message: error.message });
      return res.status(500).json({ error: 'Failed to upload image to Cloudinary. Please try again.' });
    }

    // Generic server error
    res.status(500).json({ error: 'Failed to upload image to Cloudinary' });
  }
});

module.exports = router;
