-- ============================================================================
-- User Hierarchy Migration
-- ============================================================================
-- Creates a hierarchical user system with 4 user types:
-- 1. admin - Can edit anything about any dealership (super admin)
-- 2. dealership_owner - Can see/edit everything for their specific dealership
-- 3. dealership_staff - Can see everything for their dealership, edit only assigned sections
-- 4. end_user - Public website visitors (no database record needed)
--
-- Permissions hierarchy:
-- - admin: Full access to all dealerships
-- - dealership_owner: Full access to their dealership, can manage staff
-- - dealership_staff: Read-only except for assigned permissions
-- ============================================================================

-- Drop existing table if it exists
DROP TABLE IF EXISTS app_user CASCADE;

-- ============================================================================
-- APP_USER TABLE
-- ============================================================================
-- Stores all authenticated users (admin, dealership owners, and staff)
-- End users don't need accounts (they just visit the public website)

CREATE TABLE app_user (
  id SERIAL PRIMARY KEY,
  username VARCHAR(255) NOT NULL UNIQUE,
  password_hash VARCHAR(255) NOT NULL,
  email VARCHAR(255) NOT NULL,
  full_name VARCHAR(255) NOT NULL,
  user_type VARCHAR(20) NOT NULL CHECK (user_type IN ('admin', 'dealership_owner', 'dealership_staff')),
  dealership_id INTEGER REFERENCES dealership(id) ON DELETE CASCADE,
  created_by INTEGER REFERENCES app_user(id) ON DELETE SET NULL,
  permissions JSONB DEFAULT '[]'::jsonb,
  is_active BOOLEAN DEFAULT true,
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW(),
  
  -- Constraints:
  -- - admin users must NOT have a dealership_id (they manage all dealerships)
  -- - dealership_owner and dealership_staff MUST have a dealership_id
  CONSTRAINT check_admin_no_dealership CHECK (
    (user_type = 'admin' AND dealership_id IS NULL) OR
    (user_type IN ('dealership_owner', 'dealership_staff') AND dealership_id IS NOT NULL)
  )
);

-- ============================================================================
-- INDEXES
-- ============================================================================

-- Username lookup (for login)
CREATE UNIQUE INDEX idx_user_username ON app_user(username);

-- Email lookup
CREATE INDEX idx_user_email ON app_user(email);

-- Dealership-scoped user queries
CREATE INDEX idx_user_dealership_id ON app_user(dealership_id);

-- User type filtering
CREATE INDEX idx_user_type ON app_user(user_type);

-- Active users only
CREATE INDEX idx_user_is_active ON app_user(is_active);

-- ============================================================================
-- PERMISSIONS STRUCTURE (stored in JSONB)
-- ============================================================================
-- For dealership_staff, the permissions field contains an array of allowed sections:
-- Examples:
--   [] - No permissions (read-only access)
--   ["leads"] - Can manage leads only
--   ["vehicles"] - Can manage vehicles only
--   ["leads", "vehicles", "settings"] - Can manage multiple sections
--
-- Available permission values:
--   - "leads" - Manage lead inbox
--   - "sales_requests" - Manage sales requests
--   - "vehicles" - Manage vehicle inventory
--   - "blogs" - Manage blog posts
--   - "settings" - Edit dealership settings
--
-- For admin and dealership_owner, permissions field is ignored (they have full access)

-- ============================================================================
-- COMMENTS
-- ============================================================================

COMMENT ON TABLE app_user IS 'Hierarchical user system for CMS authentication and authorization';
COMMENT ON COLUMN app_user.user_type IS 'User role: admin (super admin), dealership_owner (full dealership access), dealership_staff (limited access)';
COMMENT ON COLUMN app_user.dealership_id IS 'Foreign key to dealership. NULL for admin, required for owner/staff';
COMMENT ON COLUMN app_user.created_by IS 'User who created this account (admin creates owners, owners create staff)';
COMMENT ON COLUMN app_user.permissions IS 'JSONB array of permission strings for dealership_staff (e.g., ["leads", "vehicles"])';
COMMENT ON COLUMN app_user.is_active IS 'Account status (soft delete). Inactive users cannot log in';

-- ============================================================================
-- MIGRATION COMPLETE
-- ============================================================================
