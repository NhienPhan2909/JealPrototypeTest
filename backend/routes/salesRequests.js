/**
 * @fileoverview Sales Request API routes.
 * Handles customer sales request submission (POST) and admin sales request management (GET, PATCH, DELETE).
 *
 * SECURITY (SEC-001): GET/PATCH/DELETE endpoints require dealershipId query parameter to enforce
 * multi-tenant data isolation and prevent cross-dealership access.
 *
 * SECURITY (XSS Prevention): POST endpoint sanitizes user inputs to prevent stored XSS attacks.
 *
 * SECURITY (Input Validation): Validates field lengths and formats to prevent database
 * errors, DoS attacks, and ensures data integrity.
 *
 * Routes:
 * - GET    /api/sales-requests?dealershipId=<id>              - List sales requests for dealership
 * - POST   /api/sales-requests                                 - Submit customer sales request (public)
 * - PATCH  /api/sales-requests/:id/status?dealershipId=<id>  - Update sales request status (admin)
 * - DELETE /api/sales-requests/:id?dealershipId=<id>         - Delete sales request (admin)
 */

const express = require('express');
const router = express.Router();
const salesRequestsDb = require('../db/salesRequests');
const dealersDb = require('../db/dealers');
const { sendNewSalesRequestNotification } = require('../services/emailService');
const captchaVerification = require('../middleware/captchaVerification');
const { requireAuth, requirePermission, enforceDealershipScope } = require('../middleware/auth');

/**
 * Email validation regex.
 */
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

/**
 * Field length limits for input validation.
 */
const FIELD_LIMITS = {
  name: 255,
  email: 255,
  phone: 20,
  make: 100,
  model: 100,
  additional_message: 5000
};

/**
 * Sanitizes user input to prevent XSS attacks by escaping HTML special characters.
 *
 * @param {string} input - User-provided string to sanitize
 * @returns {string} Sanitized string safe for HTML display
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
 * Validates required fields are present in sales request data.
 */
function validateRequiredFields(data) {
  const requiredFields = ['dealership_id', 'name', 'email', 'phone', 'make', 'model', 'year', 'kilometers'];
  const missingFields = requiredFields.filter(field => !data[field]);

  if (missingFields.length > 0) {
    return { error: `Missing required fields: ${missingFields.join(', ')}` };
  }

  return null;
}

/**
 * Validates email format.
 */
function validateEmailFormat(email) {
  return EMAIL_REGEX.test(email);
}

/**
 * Validates field lengths against defined limits.
 */
function validateFieldLengths(data) {
  const { name, email, phone, make, model, additional_message } = data;

  if (name && name.length > FIELD_LIMITS.name) {
    return { error: `Name must be ${FIELD_LIMITS.name} characters or less` };
  }
  if (email && email.length > FIELD_LIMITS.email) {
    return { error: `Email must be ${FIELD_LIMITS.email} characters or less` };
  }
  if (phone && phone.length > FIELD_LIMITS.phone) {
    return { error: `Phone must be ${FIELD_LIMITS.phone} characters or less` };
  }
  if (make && make.length > FIELD_LIMITS.make) {
    return { error: `Make must be ${FIELD_LIMITS.make} characters or less` };
  }
  if (model && model.length > FIELD_LIMITS.model) {
    return { error: `Model must be ${FIELD_LIMITS.model} characters or less` };
  }
  if (additional_message && additional_message.length > FIELD_LIMITS.additional_message) {
    return { error: `Additional message must be ${FIELD_LIMITS.additional_message} characters or less` };
  }

  return null;
}

/**
 * GET /api/sales-requests?dealershipId=<id>
 *
 * Retrieves all sales requests for a specific dealership, sorted by newest first.
 * 
 * NOTE: Viewing sales requests is read-only and available to all authenticated users.
 * Editing sales requests requires 'sales_requests' permission.
 *
 * @returns {200} Array of sales request objects sorted by created_at DESC
 * @returns {400} Missing dealershipId query parameter
 * @returns {401} Not authenticated
 * @returns {500} Database error
 */
router.get('/', requireAuth, enforceDealershipScope, async (req, res) => {
  try {
    const { dealershipId } = req.query;

    if (!dealershipId) {
      return res.status(400).json({ error: 'dealershipId query parameter is required' });
    }

    const dealershipIdNum = parseInt(dealershipId, 10);
    if (isNaN(dealershipIdNum) || dealershipIdNum <= 0) {
      return res.status(400).json({ error: 'dealershipId must be a valid positive number' });
    }

    const salesRequests = await salesRequestsDb.getAll(dealershipIdNum);
    res.status(200).json(salesRequests);
  } catch (error) {
    console.error('Error fetching sales requests:', error);
    res.status(500).json({ error: 'Failed to fetch sales requests' });
  }
});

/**
 * POST /api/sales-requests
 *
 * Submits a new customer sales request (public endpoint, no auth required).
 *
 * SECURITY: CAPTCHA verification required to prevent spam and automated submissions.
 *
 * @returns {201} Created sales request object
 * @returns {400} Validation error (CAPTCHA failure, invalid data)
 * @returns {500} Database error
 */
router.post('/', captchaVerification, async (req, res) => {
  try {
    const { 
      dealership_id, 
      name, 
      email, 
      phone, 
      make, 
      model, 
      year, 
      kilometers, 
      additional_message 
    } = req.body;

    // Validate required fields
    const fieldValidation = validateRequiredFields(req.body);
    if (fieldValidation) {
      return res.status(400).json(fieldValidation);
    }

    // Validate field lengths
    const lengthValidation = validateFieldLengths(req.body);
    if (lengthValidation) {
      return res.status(400).json(lengthValidation);
    }

    // Validate email format
    if (!validateEmailFormat(email)) {
      return res.status(400).json({ error: 'Invalid email format' });
    }

    // Validate year
    const yearNum = parseInt(year, 10);
    const currentYear = new Date().getFullYear();
    if (isNaN(yearNum) || yearNum < 1900 || yearNum > currentYear + 1) {
      return res.status(400).json({ error: 'Invalid year' });
    }

    // Validate kilometers
    const kilometersNum = parseInt(kilometers, 10);
    if (isNaN(kilometersNum) || kilometersNum < 0) {
      return res.status(400).json({ error: 'Kilometers must be a positive number' });
    }

    // Sanitize user inputs
    const sanitizedName = sanitizeInput(name);
    const sanitizedPhone = sanitizeInput(phone);
    const sanitizedMake = sanitizeInput(make);
    const sanitizedModel = sanitizeInput(model);
    const sanitizedAdditionalMessage = additional_message ? sanitizeInput(additional_message) : null;

    // Create sales request record
    const salesRequest = await salesRequestsDb.create({
      dealership_id,
      name: sanitizedName,
      email,
      phone: sanitizedPhone,
      make: sanitizedMake,
      model: sanitizedModel,
      year: yearNum,
      kilometers: kilometersNum,
      additional_message: sanitizedAdditionalMessage
    });

    // Send email notification to dealership
    try {
      // Get dealership email
      const dealership = await dealersDb.getById(dealership_id);

      if (dealership && dealership.email) {
        // Send email notification
        await sendNewSalesRequestNotification(dealership.email, {
          name: sanitizedName,
          email,
          phone: sanitizedPhone,
          make: sanitizedMake,
          model: sanitizedModel,
          year: yearNum,
          kilometers: kilometersNum,
          additional_message: sanitizedAdditionalMessage
        });

        console.log('Sales request notification email sent to:', dealership.email);
      } else {
        console.warn('Dealership email not found for dealership_id:', dealership_id);
      }
    } catch (emailError) {
      // Log email error but don't fail the request
      // The sales request was created successfully even if email fails
      console.error('Error sending sales request notification email:', emailError);
    }

    res.status(201).json(salesRequest);
  } catch (error) {
    console.error('Error creating sales request:', error);
    res.status(500).json({ error: 'Failed to create sales request' });
  }
});

/**
 * PATCH /api/sales-requests/:id/status?dealershipId=<id>
 *
 * Updates the status of a sales request (admin endpoint).
 * PERMISSION: Requires 'sales_requests' permission for staff users.
 *
 * @returns {200} Updated sales request object
 * @returns {400} Missing/invalid parameters or invalid status
 * @returns {401} Not authenticated
 * @returns {403} Missing 'sales_requests' permission
 * @returns {404} Sales request not found or doesn't belong to dealership
 * @returns {500} Database error
 */
router.patch('/:id/status', requireAuth, enforceDealershipScope, requirePermission('sales_requests'), async (req, res) => {
  try {
    const { id } = req.params;
    const { dealershipId } = req.query;
    const { status } = req.body;

    if (!dealershipId) {
      return res.status(400).json({ error: 'dealershipId query parameter is required' });
    }

    const dealershipIdNum = parseInt(dealershipId, 10);
    if (isNaN(dealershipIdNum) || dealershipIdNum <= 0) {
      return res.status(400).json({ error: 'dealershipId must be a valid positive number' });
    }

    const idNum = parseInt(id, 10);
    if (isNaN(idNum) || idNum <= 0) {
      return res.status(400).json({ error: 'ID must be a valid positive number' });
    }

    const validStatuses = ['received', 'in progress', 'done'];
    if (!status || !validStatuses.includes(status)) {
      return res.status(400).json({ error: `Status must be one of: ${validStatuses.join(', ')}` });
    }

    const updatedSalesRequest = await salesRequestsDb.updateStatus(idNum, dealershipIdNum, status);

    if (!updatedSalesRequest) {
      return res.status(404).json({ error: 'Sales request not found or does not belong to this dealership' });
    }

    res.status(200).json(updatedSalesRequest);
  } catch (error) {
    console.error('Error updating sales request status:', error);
    res.status(500).json({ error: 'Failed to update sales request status' });
  }
});

/**
 * DELETE /api/sales-requests/:id?dealershipId=<id>
 *
 * Deletes a sales request (admin endpoint).
 * PERMISSION: Requires 'sales_requests' permission for staff users.
 *
 * @returns {200} Success message
 * @returns {400} Missing/invalid parameters
 * @returns {401} Not authenticated
 * @returns {403} Missing 'sales_requests' permission
 * @returns {404} Sales request not found or doesn't belong to dealership
 * @returns {500} Database error
 */
router.delete('/:id', requireAuth, enforceDealershipScope, requirePermission('sales_requests'), async (req, res) => {
  try {
    const { id } = req.params;
    const { dealershipId } = req.query;

    if (!dealershipId) {
      return res.status(400).json({ error: 'dealershipId query parameter is required' });
    }

    const dealershipIdNum = parseInt(dealershipId, 10);
    if (isNaN(dealershipIdNum) || dealershipIdNum <= 0) {
      return res.status(400).json({ error: 'dealershipId must be a valid positive number' });
    }

    const idNum = parseInt(id, 10);
    if (isNaN(idNum) || idNum <= 0) {
      return res.status(400).json({ error: 'ID must be a valid positive number' });
    }

    const deleted = await salesRequestsDb.deleteSalesRequest(idNum, dealershipIdNum);

    if (!deleted) {
      return res.status(404).json({ error: 'Sales request not found or does not belong to this dealership' });
    }

    res.status(200).json({ message: 'Sales request deleted successfully' });
  } catch (error) {
    console.error('Error deleting sales request:', error);
    res.status(500).json({ error: 'Failed to delete sales request' });
  }
});

module.exports = router;
