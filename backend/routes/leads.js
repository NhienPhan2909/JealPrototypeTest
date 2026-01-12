/**
 * @fileoverview Lead API routes.
 * Handles customer enquiry submission (POST) and admin lead inbox listing (GET).
 *
 * SECURITY (SEC-001): GET endpoint requires dealershipId query parameter to enforce
 * multi-tenant data isolation and prevent cross-dealership lead access.
 *
 * SECURITY (XSS Prevention): POST endpoint sanitizes user inputs (name, phone, message)
 * by escaping HTML special characters to prevent stored XSS attacks when displayed in admin UI.
 *
 * SECURITY (Input Validation): Validates field lengths and formats to prevent database
 * errors, DoS attacks, and ensures data integrity.
 *
 * Routes:
 * - GET    /api/leads?dealershipId=<id>              - List leads for dealership (admin inbox, sorted newest first)
 * - POST   /api/leads                                 - Submit customer enquiry (public, no auth required)
 * - PATCH  /api/leads/:id/status?dealershipId=<id>  - Update lead status (admin)
 * - DELETE /api/leads/:id?dealershipId=<id>         - Delete lead (admin)
 */

const express = require('express');
const router = express.Router();
const leadsDb = require('../db/leads');
const dealersDb = require('../db/dealers');
const vehiclesDb = require('../db/vehicles');
const { sendNewLeadNotification } = require('../services/emailService');
const captchaVerification = require('../middleware/captchaVerification');
const { requireAuth, requirePermission, enforceDealershipScope } = require('../middleware/auth');

/**
 * Email validation regex.
 * Basic format check: requires @ symbol and domain with TLD (e.g., user@example.com)
 */
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

/**
 * Field length limits for input validation (defense-in-depth).
 * Prevents database errors and potential DoS from oversized inputs.
 */
const FIELD_LIMITS = {
  name: 255,
  email: 255,
  phone: 20,
  message: 5000
};

/**
 * Sanitizes user input to prevent XSS attacks by escaping HTML special characters.
 *
 * SECURITY: This function prevents stored XSS attacks when user-provided data
 * (name, message, phone) is displayed in the admin UI. Escapes <, >, &, ", ' characters.
 *
 * @param {string} input - User-provided string to sanitize
 * @returns {string} Sanitized string safe for HTML display
 *
 * @example
 * sanitizeInput("<script>alert('xss')</script>")
 * // Returns: "&lt;script&gt;alert(&#x27;xss&#x27;)&lt;/script&gt;"
 */
function sanitizeInput(input) {
  if (typeof input !== 'string') return input;
  return input
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#x27;')
    .replace(/\//g, '&#x2F;');
}

/**
 * Validates required fields are present in lead data.
 *
 * @param {Object} leadData - Lead data object from request body
 * @returns {Object|null} Error object with missing fields, or null if valid
 */
function validateRequiredFields(leadData) {
  const requiredFields = ['dealership_id', 'name', 'email', 'phone', 'message'];
  const missingFields = requiredFields.filter(field => !leadData[field]);

  if (missingFields.length > 0) {
    return { error: `Missing required fields: ${missingFields.join(', ')}` };
  }

  return null;
}

/**
 * Validates email format using basic regex.
 *
 * @param {string} email - Email address to validate
 * @returns {boolean} True if email format is valid, false otherwise
 */
function validateEmailFormat(email) {
  return EMAIL_REGEX.test(email);
}

/**
 * Validates field lengths against defined limits.
 *
 * SECURITY: Defense-in-depth - prevents oversized inputs that could cause
 * database errors or potential DoS attacks.
 *
 * @param {Object} leadData - Lead data object to validate
 * @returns {Object|null} Error object if validation fails, null if valid
 */
function validateFieldLengths(leadData) {
  const { name, email, phone, message } = leadData;

  if (name && name.length > FIELD_LIMITS.name) {
    return { error: `Name must be ${FIELD_LIMITS.name} characters or less` };
  }
  if (email && email.length > FIELD_LIMITS.email) {
    return { error: `Email must be ${FIELD_LIMITS.email} characters or less` };
  }
  if (phone && phone.length > FIELD_LIMITS.phone) {
    return { error: `Phone must be ${FIELD_LIMITS.phone} characters or less` };
  }
  if (message && message.length > FIELD_LIMITS.message) {
    return { error: `Message must be ${FIELD_LIMITS.message} characters or less` };
  }

  return null;
}

/**
 * GET /api/leads?dealershipId=<id>
 *
 * Retrieves all leads for a specific dealership, sorted by newest first (admin inbox).
 *
 * SECURITY (SEC-001): Requires dealershipId query parameter to enforce multi-tenancy.
 * Returns 400 Bad Request if dealershipId is missing.
 * 
 * NOTE: Viewing leads is read-only and available to all authenticated users.
 * Editing leads requires 'leads' permission.
 *
 * @returns {200} Array of lead objects sorted by created_at DESC
 * @returns {400} Missing dealershipId query parameter
 * @returns {401} Not authenticated
 * @returns {500} Database error
 */
router.get('/', requireAuth, enforceDealershipScope, async (req, res) => {
  try {
    const { dealershipId } = req.query;

    // Validate dealershipId is present (SEC-001 enforcement)
    if (!dealershipId) {
      return res.status(400).json({ error: 'dealershipId query parameter is required' });
    }

    // Validate dealershipId is a valid number
    const dealershipIdNum = parseInt(dealershipId, 10);
    if (isNaN(dealershipIdNum) || dealershipIdNum <= 0) {
      return res.status(400).json({ error: 'dealershipId must be a valid positive number' });
    }

    // Fetch leads filtered by dealership, sorted newest first
    const leads = await leadsDb.getAll(dealershipIdNum);
    res.status(200).json(leads);
  } catch (error) {
    console.error('Error fetching leads:', error);
    res.status(500).json({ error: 'Failed to fetch leads' });
  }
});

/**
 * POST /api/leads
 *
 * Submits a new customer enquiry (public endpoint, no auth required).
 *
 * Validates:
 * - CAPTCHA verification: Required to prevent spam and automated submissions
 * - Required fields: dealership_id, name, email, phone, message
 * - Field lengths: name (255), email (255), phone (20), message (5000)
 * - Email format (basic regex)
 * - Vehicle ownership (if vehicle_id provided, must belong to specified dealership)
 *
 * SECURITY Measures:
 * - CAPTCHA verification: Prevents automated form submissions and spam
 * - Input sanitization: Escapes HTML characters in name, phone, message to prevent XSS
 * - Length validation: Prevents oversized inputs and potential DoS
 * - Vehicle ownership validation: Enforces cross-tenant data integrity (SEC-001)
 *
 * @returns {201} Created lead object with generated id and created_at
 * @returns {400} Validation error (CAPTCHA failure, missing fields, invalid email, field too long, vehicle ownership mismatch)
 * @returns {500} Database error
 */
router.post('/', captchaVerification, async (req, res) => {
  try {
    const { dealership_id, vehicle_id, name, email, phone, message } = req.body;

    // Validate required fields
    const fieldValidation = validateRequiredFields(req.body);
    if (fieldValidation) {
      return res.status(400).json(fieldValidation);
    }

    // Validate field lengths (defense-in-depth)
    const lengthValidation = validateFieldLengths(req.body);
    if (lengthValidation) {
      return res.status(400).json(lengthValidation);
    }

    // Validate email format
    if (!validateEmailFormat(email)) {
      return res.status(400).json({ error: 'Invalid email format' });
    }

    // If vehicle_id provided, validate vehicle exists and belongs to dealership
    if (vehicle_id) {
      const isValidOwnership = await leadsDb.validateVehicleOwnership(vehicle_id, dealership_id);
      if (!isValidOwnership) {
        return res.status(400).json({ error: 'Vehicle does not belong to specified dealership' });
      }
    }

    // Sanitize user inputs to prevent XSS attacks
    const sanitizedName = sanitizeInput(name);
    const sanitizedPhone = sanitizeInput(phone);
    const sanitizedMessage = sanitizeInput(message);

    // Create lead record with sanitized data
    const lead = await leadsDb.create({
      dealership_id,
      vehicle_id,
      name: sanitizedName,
      email, // Email not sanitized - already validated format
      phone: sanitizedPhone,
      message: sanitizedMessage
    });

    // Send email notification to dealership
    try {
      // Get dealership email
      const dealership = await dealersDb.getById(dealership_id);

      if (dealership && dealership.email) {
        // Get vehicle information if vehicle_id is provided
        let vehicleInfo = null;
        if (vehicle_id) {
          const vehicle = await vehiclesDb.getById(vehicle_id, dealership_id);
          if (vehicle) {
            vehicleInfo = `${vehicle.year} ${vehicle.make} ${vehicle.model}`;
          }
        }

        // Send email notification
        await sendNewLeadNotification(dealership.email, {
          name: sanitizedName,
          email,
          phone: sanitizedPhone,
          message: sanitizedMessage,
          vehicleInfo
        });

        console.log('Lead notification email sent to:', dealership.email);
      } else {
        console.warn('Dealership email not found for dealership_id:', dealership_id);
      }
    } catch (emailError) {
      // Log email error but don't fail the request
      // The lead was created successfully even if email fails
      console.error('Error sending lead notification email:', emailError);
    }

    res.status(201).json(lead);
  } catch (error) {
    console.error('Error creating lead:', error);
    res.status(500).json({ error: 'Failed to create lead' });
  }
});

/**
 * PATCH /api/leads/:id/status?dealershipId=<id>
 *
 * Updates the status of a lead (admin endpoint).
 *
 * SECURITY (SEC-001): Requires dealershipId query parameter to enforce multi-tenancy.
 * Only updates lead if it belongs to specified dealership.
 * PERMISSION: Requires 'leads' permission for staff users.
 *
 * @param {number} id - Lead ID (route parameter)
 * @param {string} status - New status ('received', 'in progress', 'done') in request body
 * @returns {200} Updated lead object
 * @returns {400} Missing/invalid parameters or invalid status
 * @returns {401} Not authenticated
 * @returns {403} Missing 'leads' permission
 * @returns {404} Lead not found or doesn't belong to dealership
 * @returns {500} Database error
 */
router.patch('/:id/status', requireAuth, enforceDealershipScope, requirePermission('leads'), async (req, res) => {
  try {
    const { id } = req.params;
    const { dealershipId } = req.query;
    const { status } = req.body;

    // Validate dealershipId is present (SEC-001 enforcement)
    if (!dealershipId) {
      return res.status(400).json({ error: 'dealershipId query parameter is required' });
    }

    // Validate dealershipId is a valid number
    const dealershipIdNum = parseInt(dealershipId, 10);
    if (isNaN(dealershipIdNum) || dealershipIdNum <= 0) {
      return res.status(400).json({ error: 'dealershipId must be a valid positive number' });
    }

    // Validate lead ID
    const leadIdNum = parseInt(id, 10);
    if (isNaN(leadIdNum) || leadIdNum <= 0) {
      return res.status(400).json({ error: 'Lead ID must be a valid positive number' });
    }

    // Validate status value
    const validStatuses = ['received', 'in progress', 'done'];
    if (!status || !validStatuses.includes(status)) {
      return res.status(400).json({ error: `Status must be one of: ${validStatuses.join(', ')}` });
    }

    // Update lead status with ownership validation
    const updatedLead = await leadsDb.updateStatus(leadIdNum, dealershipIdNum, status);

    if (!updatedLead) {
      return res.status(404).json({ error: 'Lead not found or does not belong to this dealership' });
    }

    res.status(200).json(updatedLead);
  } catch (error) {
    console.error('Error updating lead status:', error);
    res.status(500).json({ error: 'Failed to update lead status' });
  }
});

/**
 * DELETE /api/leads/:id?dealershipId=<id>
 *
 * Deletes a lead (admin endpoint).
 *
 * SECURITY (SEC-001): Requires dealershipId query parameter to enforce multi-tenancy.
 * Only deletes lead if it belongs to specified dealership.
 * PERMISSION: Requires 'leads' permission for staff users.
 *
 * @param {number} id - Lead ID (route parameter)
 * @returns {200} Success message
 * @returns {400} Missing/invalid parameters
 * @returns {401} Not authenticated
 * @returns {403} Missing 'leads' permission
 * @returns {404} Lead not found or doesn't belong to dealership
 * @returns {500} Database error
 */
router.delete('/:id', requireAuth, enforceDealershipScope, requirePermission('leads'), async (req, res) => {
  try {
    const { id } = req.params;
    const { dealershipId } = req.query;

    // Validate dealershipId is present (SEC-001 enforcement)
    if (!dealershipId) {
      return res.status(400).json({ error: 'dealershipId query parameter is required' });
    }

    // Validate dealershipId is a valid number
    const dealershipIdNum = parseInt(dealershipId, 10);
    if (isNaN(dealershipIdNum) || dealershipIdNum <= 0) {
      return res.status(400).json({ error: 'dealershipId must be a valid positive number' });
    }

    // Validate lead ID
    const leadIdNum = parseInt(id, 10);
    if (isNaN(leadIdNum) || leadIdNum <= 0) {
      return res.status(400).json({ error: 'Lead ID must be a valid positive number' });
    }

    // Delete lead with ownership validation
    const deleted = await leadsDb.deleteLead(leadIdNum, dealershipIdNum);

    if (!deleted) {
      return res.status(404).json({ error: 'Lead not found or does not belong to this dealership' });
    }

    res.status(200).json({ message: 'Lead deleted successfully' });
  } catch (error) {
    console.error('Error deleting lead:', error);
    res.status(500).json({ error: 'Failed to delete lead' });
  }
});

module.exports = router;
