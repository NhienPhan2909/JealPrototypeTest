# 12. Security Guidelines

## Overview

This document outlines mandatory security practices for all backend API development. These guidelines ensure protection against common vulnerabilities (OWASP Top 10) and maintain multi-tenant data isolation.

**Last Updated:** 2025-11-20
**Reviewed By:** Quinn (Test Architect)
**Status:** MANDATORY for all new development

---

## Critical Security Requirements

### 1. Multi-Tenancy Data Isolation (SEC-001)

**Status:** CRITICAL - Highest Priority
**Applies To:** All database queries involving tenant-scoped data (vehicles, leads, orders, etc.)

**Requirements:**
- ALL database queries MUST filter by `dealership_id`
- ALL GET endpoints MUST require `dealershipId` query parameter
- ALL ownership validation MUST verify cross-tenant relationships
- Return 400 Bad Request if dealershipId missing or invalid

**Implementation Pattern:**

```javascript
// Database layer (backend/db/*.js)
async function getAll(dealershipId) {
  const result = await pool.query(
    'SELECT * FROM resource WHERE dealership_id = $1 ORDER BY created_at DESC',
    [dealershipId]
  );
  return result.rows;
}

// Route layer (backend/routes/*.js)
router.get('/', async (req, res) => {
  const { dealershipId } = req.query;

  // Validate presence
  if (!dealershipId) {
    return res.status(400).json({ error: 'dealershipId query parameter is required' });
  }

  // Validate numeric format
  const dealershipIdNum = parseInt(dealershipId, 10);
  if (isNaN(dealershipIdNum) || dealershipIdNum <= 0) {
    return res.status(400).json({ error: 'dealershipId must be a valid positive number' });
  }

  const resources = await db.getAll(dealershipIdNum);
  res.json(resources);
});
```

**Cross-Tenant Relationship Validation:**

When a resource references another resource (e.g., lead → vehicle), validate ownership:

```javascript
async function validateVehicleOwnership(vehicleId, dealershipId) {
  const result = await pool.query(
    'SELECT id FROM vehicle WHERE id = $1 AND dealership_id = $2',
    [vehicleId, dealershipId]
  );
  return result.rows.length > 0;
}
```

**Testing Requirement:**
- MUST verify data from Dealership A does NOT appear in Dealership B queries
- MUST test cross-dealership relationship rejection (e.g., Dealership A lead cannot reference Dealership B vehicle)

**Reference:** Story 1.2 (SEC-001 risk identification), Stories 1.3-1.5 (implementation examples)

---

### 2. XSS Prevention (Cross-Site Scripting)

**Status:** CRITICAL for public-facing endpoints
**Applies To:** All endpoints accepting user-generated text (names, messages, descriptions, reviews, etc.)

**Risk:** User input containing malicious scripts could be stored in database and executed when displayed in admin UI or public website.

**Requirements:**
- Sanitize ALL user text inputs before storage
- Escape HTML special characters: `< > & " ' /`
- Apply to: name, message, phone, description, review text, etc.
- Do NOT sanitize: email (validated format), numeric fields, enum values

**Implementation Pattern:**

```javascript
/**
 * Sanitizes user input to prevent XSS attacks by escaping HTML special characters.
 *
 * SECURITY: This function prevents stored XSS attacks when user-provided data
 * is displayed in HTML contexts. Escapes <, >, &, ", ' characters.
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

// Usage in POST endpoint
router.post('/', async (req, res) => {
  const { name, message, phone } = req.body;

  // Sanitize text inputs
  const sanitizedName = sanitizeInput(name);
  const sanitizedMessage = sanitizeInput(message);
  const sanitizedPhone = sanitizeInput(phone);

  await db.create({
    name: sanitizedName,
    message: sanitizedMessage,
    phone: sanitizedPhone
  });
});
```

**Testing Requirement:**
- Test with `<script>alert('xss')</script>` - should store as `&lt;script&gt;...`
- Test with `<img src=x onerror=alert('xss')>` - should escape all HTML tags
- Verify sanitized data displays correctly without executing scripts

**Reference:** Story 1.5 (XSS prevention implementation in leads API)

---

### 3. SQL Injection Prevention

**Status:** CRITICAL - Already implemented project-wide
**Applies To:** ALL database queries

**Requirements:**
- ALWAYS use parameterized queries with `$1, $2, ...` placeholders
- NEVER concatenate user input into SQL strings
- Use pg client's query parameterization

**Implementation Pattern:**

```javascript
// ✅ CORRECT - Parameterized query (SQL injection safe)
const result = await pool.query(
  'SELECT * FROM vehicle WHERE dealership_id = $1 AND status = $2',
  [dealershipId, status]
);

// ❌ WRONG - String concatenation (SQL injection vulnerable)
const result = await pool.query(
  `SELECT * FROM vehicle WHERE dealership_id = ${dealershipId} AND status = '${status}'`
);
```

**Why This Works:**
- pg driver escapes parameters automatically
- User input is treated as data, not SQL code
- Prevents injection like `'; DROP TABLE vehicle; --`

**Reference:** All Stories 1.2-1.5 (consistent parameterized query usage)

---

### 4. Input Length Validation

**Status:** RECOMMENDED - Defense-in-depth
**Applies To:** All text fields with database column length limits

**Risk:** Oversized inputs can cause database errors, consume excessive memory/bandwidth, or enable DoS attacks.

**Requirements:**
- Define field length limits matching database schema
- Validate before processing or database calls
- Return 400 Bad Request with descriptive error

**Implementation Pattern:**

```javascript
const FIELD_LIMITS = {
  name: 255,
  email: 255,
  phone: 20,
  message: 5000,
  title: 200,
  description: 2000
};

function validateFieldLengths(data) {
  for (const [field, limit] of Object.entries(FIELD_LIMITS)) {
    if (data[field] && data[field].length > limit) {
      return { error: `${field} must be ${limit} characters or less` };
    }
  }
  return null;
}

// Usage in POST endpoint
router.post('/', async (req, res) => {
  const lengthValidation = validateFieldLengths(req.body);
  if (lengthValidation) {
    return res.status(400).json(lengthValidation);
  }
  // Continue processing...
});
```

**Database Schema Reference:**
- VARCHAR(255): Standard text fields (name, email, title)
- VARCHAR(20): Short fields (phone)
- TEXT: Long content (message, description) - impose reasonable limit (e.g., 5000 chars)

**Reference:** Story 1.5 (input length validation implementation)

---

### 5. Input Format Validation

**Status:** MANDATORY for structured fields
**Applies To:** Email, URLs, phone numbers, dates, numeric values

**Requirements:**
- Validate format BEFORE processing
- Return 400 Bad Request with specific error message
- Use appropriate validation methods

**Implementation Patterns:**

**Email Validation:**
```javascript
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

function validateEmailFormat(email) {
  return EMAIL_REGEX.test(email);
}

// Usage
if (!validateEmailFormat(email)) {
  return res.status(400).json({ error: 'Invalid email format' });
}
```

**Numeric ID Validation:**
```javascript
const id = parseInt(req.params.id, 10);
if (isNaN(id) || id <= 0) {
  return res.status(400).json({ error: 'Invalid ID - must be positive number' });
}
```

**Enum Validation:**
```javascript
const VALID_STATUSES = ['active', 'sold', 'pending', 'draft'];

if (status && !VALID_STATUSES.includes(status)) {
  return res.status(400).json({
    error: `Invalid status. Must be one of: ${VALID_STATUSES.join(', ')}`
  });
}
```

**Reference:** Story 1.5 (email validation, dealershipId numeric validation)

---

### 6. Rate Limiting

**Status:** DEFERRED to pre-production (Story 1.9 or Phase 2)
**Applies To:** Public-facing endpoints (lead submission, contact forms, login)

**Risk:** Brute force attacks, spam submissions, DoS attacks

**Future Implementation (Reference Only):**

```javascript
const rateLimit = require('express-rate-limit');

const leadSubmissionLimiter = rateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 10, // 10 requests per windowMs per IP
  message: 'Too many enquiries submitted. Please try again later.'
});

app.use('/api/leads', leadSubmissionLimiter);
```

**Note:** Not required for MVP (all endpoints currently open for testing). MUST be added before production deployment.

---

## Security Checklist for New Endpoints

Use this checklist when implementing any new API endpoint:

### All Endpoints
- [ ] Parameterized queries used (no string concatenation)
- [ ] Error handling with try-catch
- [ ] Generic error messages returned (no info leakage)
- [ ] Errors logged with console.error()
- [ ] JSDoc includes security notes

### GET Endpoints (List/Retrieve)
- [ ] dealershipId query parameter required (if tenant-scoped)
- [ ] dealershipId validated (present, numeric, positive)
- [ ] Database queries filter by dealership_id
- [ ] 400 returned if dealershipId missing/invalid
- [ ] Tested: cross-dealership access blocked

### POST/PUT Endpoints (Create/Update)
- [ ] Required fields validated (return 400 if missing)
- [ ] Field lengths validated (return 400 if too long)
- [ ] Email format validated (if email field present)
- [ ] Numeric IDs validated (NaN check)
- [ ] Text inputs sanitized (XSS prevention)
- [ ] Cross-tenant relationships validated (if applicable)
- [ ] Tested: XSS attempts escaped
- [ ] Tested: oversized inputs rejected
- [ ] Tested: invalid formats rejected

### DELETE Endpoints
- [ ] dealershipId query parameter required
- [ ] Ownership verification (resource belongs to dealership)
- [ ] 404 returned if resource not found
- [ ] 400 returned if ownership check fails

---

## Testing Requirements

### Security Testing Checklist

**Multi-Tenancy Isolation (SEC-001):**
1. Create resource for Dealership A
2. Query with Dealership B's ID
3. Verify resource does NOT appear in results
4. Test cross-dealership relationship rejection

**XSS Prevention:**
1. Submit `<script>alert('xss')</script>` in text field
2. Verify stored as `&lt;script&gt;...` (HTML entities)
3. Retrieve and display in test HTML page
4. Verify script does not execute

**SQL Injection:**
1. Submit `1' OR '1'='1` as numeric ID
2. Submit `'; DROP TABLE vehicle; --` in text field
3. Verify no unexpected behavior or errors
4. Verify data integrity maintained

**Input Validation:**
1. Submit missing required fields → expect 400
2. Submit invalid email format → expect 400
3. Submit oversized text (> limit) → expect 400
4. Submit non-numeric ID as string → expect 400

**Manual Testing Tools:**
- Postman or curl for API endpoint testing
- Browser DevTools Network tab for response inspection
- Simple HTML page for XSS testing

---

## Security Anti-Patterns to Avoid

### ❌ DON'T: Trust client-side validation
```javascript
// Client says it's valid, but still validate server-side!
router.post('/', async (req, res) => {
  // Missing server-side validation - BAD!
  await db.create(req.body);
});
```

### ❌ DON'T: Concatenate SQL
```javascript
// SQL injection vulnerable - BAD!
const query = `SELECT * FROM users WHERE email = '${email}'`;
```

### ❌ DON'T: Skip multi-tenancy filtering
```javascript
// Leaks data across tenants - BAD!
async function getAll() {
  return await pool.query('SELECT * FROM vehicle');
}
```

### ❌ DON'T: Return database errors to client
```javascript
// Information leakage - BAD!
} catch (error) {
  res.status(500).json({ error: error.message });
}
```

### ❌ DON'T: Store unsanitized user input
```javascript
// XSS vulnerable - BAD!
await db.create({
  name: req.body.name, // Could contain <script> tags!
  message: req.body.message
});
```

---

## Quick Reference: Security Functions Library

Copy and adapt these functions for new endpoints:

```javascript
// XSS Prevention
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

// Email Validation
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
function validateEmailFormat(email) {
  return EMAIL_REGEX.test(email);
}

// Numeric ID Validation
function validateNumericId(id, paramName = 'ID') {
  const numId = parseInt(id, 10);
  if (isNaN(numId) || numId <= 0) {
    return { error: `${paramName} must be a valid positive number` };
  }
  return numId;
}

// Field Length Validation
function validateFieldLengths(data, limits) {
  for (const [field, limit] of Object.entries(limits)) {
    if (data[field] && data[field].length > limit) {
      return { error: `${field} must be ${limit} characters or less` };
    }
  }
  return null;
}

// Required Fields Validation
function validateRequiredFields(data, requiredFields) {
  const missing = requiredFields.filter(field => !data[field]);
  if (missing.length > 0) {
    return { error: `Missing required fields: ${missing.join(', ')}` };
  }
  return null;
}
```

---

## Compliance and Updates

**Mandatory Compliance:** All new API endpoints MUST follow these guidelines.

**Review Frequency:** Security guidelines reviewed during each story QA review.

**Update Process:** When new security patterns are identified, update this document and notify development team.

**Questions?** Contact Quinn (Test Architect) or refer to implemented examples in Stories 1.3-1.5.

---

## Related Documentation

- **SEC-001 Risk Details:** Story 1.2 QA Results - Risk Profile
- **Implementation Examples:** Stories 1.3 (Dealers API), 1.4 (Vehicles API), 1.5 (Leads API)
- **Database Schema:** docs/architecture/database-schema.md
- **API Specifications:** docs/architecture/api-specification.md
- **Coding Standards:** docs/architecture/coding-standards.md

---

**Document Version:** 1.0
**Created:** 2025-11-20
**Created By:** Quinn (Test Architect)
**Last Updated:** 2025-11-20
