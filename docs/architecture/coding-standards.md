# 11. Coding Standards

## JSDoc Documentation Requirements

All functions, classes, and complex logic must include JSDoc comments for human readability and AI agent understanding.

**Backend Functions:**

```javascript
/**
 * Retrieves all vehicles for a specific dealership with optional status filter.
 *
 * @param {number} dealershipId - The dealership ID to filter vehicles
 * @param {string} [status] - Optional status filter ('active', 'sold', 'pending', 'draft')
 * @returns {Promise<Array<Object>>} Array of vehicle objects
 * @throws {Error} If database query fails
 *
 * @example
 * const vehicles = await getAll(1, 'active');
 */
async function getAll(dealershipId, status) {
  // Implementation
}
```

**Frontend Components:**

```javascript
/**
 * VehicleCard - Displays a vehicle summary in grid/list view.
 *
 * @component
 * @param {Object} props
 * @param {Object} props.vehicle - Vehicle object from API
 * @param {number} props.vehicle.id - Vehicle ID
 * @param {string} props.vehicle.title - Vehicle display title
 * @param {number} props.vehicle.price - Vehicle price in dollars
 *
 * @example
 * <VehicleCard vehicle={vehicleData} />
 */
function VehicleCard({ vehicle }) {
  // Implementation
}
```

**File Header Comments:**

```javascript
/**
 * @fileoverview Vehicle CRUD API routes.
 * Handles all vehicle-related operations with multi-tenant filtering.
 *
 * SECURITY (SEC-001): All endpoints require dealershipId query parameter to enforce
 * multi-tenant data isolation and prevent cross-dealership access.
 *
 * Routes:
 * - GET    /api/vehicles?dealershipId=<id>              - List vehicles for dealership
 * - GET    /api/vehicles/:id?dealershipId=<id>          - Get single vehicle (with ownership check)
 * - POST   /api/vehicles                                 - Create vehicle (auth required)
 * - PUT    /api/vehicles/:id?dealershipId=<id>          - Update vehicle (auth required, with ownership check)
 * - DELETE /api/vehicles/:id?dealershipId=<id>          - Delete vehicle (auth required, with ownership check)
 */
```

**Guidelines:**
- Document WHY, not just WHAT
- Explain business logic and constraints
- Note multi-tenancy requirements
- Include examples for complex functions
- Comment SQL queries with their purpose
- Note security considerations

---

## ESLint and Code Quality

**Linting Status:** All frontend code passes ESLint with zero errors.

**Common ESLint Exceptions:**

**React Context Pattern (react-refresh/only-export-components):**

When creating React context providers, you may encounter this ESLint warning:
```
Fast refresh only works when a file only exports components. Move your React context(s) to a separate file
```

**Acceptable Pattern:**
```javascript
/**
 * @fileoverview Context description.
 * ESLint Fast Refresh Exception: This file exports both context and provider component,
 * which is the standard React context pattern. Fast Refresh works correctly despite the warning.
 */

/* eslint-disable react-refresh/only-export-components */
import React, { createContext, useState } from 'react';

export const MyContext = createContext();

export function MyProvider({ children }) {
  // Provider implementation
  return <MyContext.Provider value={value}>{children}</MyContext.Provider>;
}
```

**Rationale:**
- This is the standard React context pattern
- Splitting into separate files adds unnecessary complexity for simple contexts
- Fast Refresh continues to work correctly despite the warning
- Always include a comment explaining why the rule is disabled

**Reference Implementation:** `frontend/src/context/AdminContext.jsx`

---

## Dynamic Theming with CSS Custom Properties

**Overview:** The application uses CSS custom properties (CSS variables) for dynamic theming, allowing each dealership to customize their brand color from the CMS admin panel.

**CSS Variables Defined:**
- `--theme-color`: Main theme color (set from dealership.theme_color)
- `--theme-color-dark`: Darker shade for hover states (calculated: 15% darker)
- `--theme-color-light`: Lighter shade for backgrounds (calculated: 90% lighter)

**Using Theme Colors in Components:**

**Recommended Approach - Use CSS Utility Classes:**

```javascript
// ✅ PREFERRED: Use pre-defined utility classes
function MyButton() {
  return <button className="bg-theme text-white hover:bg-theme">Click Me</button>;
}

function MyLink() {
  return <a className="text-theme hover:text-theme">Learn More</a>;
}

function MyBadge() {
  return <span className="border-2 border-theme text-theme">New</span>;
}
```

**Available Utility Classes:**
- `.text-theme` - Theme color text
- `.bg-theme` - Theme color background
- `.border-theme` - Theme color border
- `.hover:text-theme` - Theme color text on hover
- `.hover:bg-theme` - Theme color background on hover
- `.btn-primary` - Primary button with theme color background

**Inline Styles (when utility classes aren't sufficient):**

```javascript
// ✅ ACCEPTABLE: Direct CSS variable reference for complex styling
function CustomComponent() {
  return (
    <div style={{
      backgroundColor: 'var(--theme-color)',
      borderLeft: '4px solid var(--theme-color-dark)',
      boxShadow: '0 2px 4px var(--theme-color-light)'
    }}>
      Content
    </div>
  );
}
```

**Setting Theme Variables (Context Setup):**

Both public site (Layout.jsx) and admin panel (AdminContext.jsx) must set CSS variables when dealership data loads:

```javascript
// ✅ REQUIRED PATTERN: Set CSS variables in useEffect
useEffect(() => {
  if (dealership?.theme_color) {
    const root = document.documentElement;
    const color = dealership.theme_color;

    // Parse hex to RGB
    const hex = color.replace('#', '');
    const r = parseInt(hex.substring(0, 2), 16);
    const g = parseInt(hex.substring(2, 4), 16);
    const b = parseInt(hex.substring(4, 6), 16);

    // Calculate darker shade (15% darker)
    const darkerR = Math.round(r * 0.85);
    const darkerG = Math.round(g * 0.85);
    const darkerB = Math.round(b * 0.85);

    // Calculate lighter shade (90% lighter)
    const lighterR = Math.round(r + (255 - r) * 0.9);
    const lighterG = Math.round(g + (255 - g) * 0.9);
    const lighterB = Math.round(b + (255 - b) * 0.9);

    // Set CSS variables
    root.style.setProperty('--theme-color', color);
    root.style.setProperty('--theme-color-dark', `rgb(${darkerR}, ${darkerG}, ${darkerB})`);
    root.style.setProperty('--theme-color-light', `rgb(${lighterR}, ${lighterG}, ${lighterB})`);
  }
}, [dealership]);
```

**What NOT to Do:**

```javascript
// ❌ WRONG: Hardcoded Tailwind color classes
function WrongButton() {
  return <button className="bg-blue-600">Click Me</button>; // Won't adapt to theme
}

// ❌ WRONG: Direct hex color in inline styles
function WrongComponent() {
  return <div style={{ color: '#3B82F6' }}>Text</div>; // Not dynamic
}

// ❌ WRONG: Accessing theme color from props/context for styling
function WrongApproach({ dealership }) {
  return <div style={{ color: dealership.theme_color }}>Text</div>; // Use CSS variables instead
}
```

**When to Use Each Approach:**
- **Utility Classes** (90% of cases): Buttons, text links, borders, backgrounds
- **Direct CSS Variables** (10% of cases): Complex gradients, shadows, or when Tailwind classes don't suffice
- **Never**: Hardcoded colors for theme-able elements

**Reference Implementations:**
- `frontend/src/components/Layout.jsx` - Public site theme setup
- `frontend/src/context/AdminContext.jsx` - Admin panel theme setup
- `frontend/src/index.css` - CSS variable definitions and utility classes
- `frontend/src/components/Header.jsx` - Using bg-theme for header background
- `frontend/src/components/VehicleCard.jsx` - Using text-theme for prices

---

## Security Requirements

**MANDATORY: All backend development MUST follow security guidelines.**

All API endpoints and database operations must implement:
- Multi-tenancy data isolation (SEC-001)
- XSS prevention via input sanitization
- SQL injection prevention via parameterized queries
- Comprehensive input validation (format, length, type)

**📖 Complete Security Guidelines:** [docs/architecture/security-guidelines.md](./security-guidelines.md)

**Key Requirements:**
- ALL database queries filter by dealership_id (multi-tenancy)
- ALL user text inputs sanitized before storage (XSS prevention)
- ALL queries use parameterized syntax with $1, $2 placeholders (SQL injection prevention)
- ALL endpoints validate input format, length, and type
- Use security checklist when implementing new endpoints

**When implementing ANY API endpoint, review security-guidelines.md for:**
- Security functions library (copy-paste ready)
- Endpoint-specific checklists (GET/POST/PUT/DELETE)
- Testing requirements
- Anti-patterns to avoid

**Reference Implementation:** Story 1.5 (Lead API) demonstrates all security patterns.

---
