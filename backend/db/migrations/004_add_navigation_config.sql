-- Migration 004: Add navigation_config JSONB column to dealership table
-- Story: 5.1 - Navigation Configuration Database & Backend API
-- Purpose: Store customizable navigation menu configuration for each dealership
-- Author: James (Developer)
-- Date: 2025-12-01

-- Add navigation_config column (nullable, defaults to NULL for backwards compatibility)
ALTER TABLE dealership
ADD COLUMN navigation_config JSONB DEFAULT NULL;

-- Add comment for documentation
COMMENT ON COLUMN dealership.navigation_config IS
'JSONB array storing navigation menu configuration. Each item has: id (string), label (string), route (string), icon (string), order (integer), enabled (boolean). Null means use default navigation.';

-- Verification query (run manually to verify migration success)
-- SELECT id, name, navigation_config FROM dealership;
