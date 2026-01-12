-- ============================================================================
-- Add theme_color column to dealership table
-- ============================================================================
-- This migration adds theme customization support for each dealership.
-- The theme_color is used for the header background and other branding elements.
--
-- Run this migration:
-- psql $DATABASE_URL < backend/db/migrations/add_theme_color.sql
-- ============================================================================

-- Add theme_color column with default value
ALTER TABLE dealership
ADD COLUMN IF NOT EXISTS theme_color VARCHAR(7) DEFAULT '#3B82F6';

-- Add comment for documentation
COMMENT ON COLUMN dealership.theme_color IS 'Hex color code for dealership theme (e.g., #3B82F6). Used for header background and branding elements.';

-- Verify column was added
SELECT column_name, data_type, column_default
FROM information_schema.columns
WHERE table_name = 'dealership' AND column_name = 'theme_color';

-- ============================================================================
-- MIGRATION COMPLETE
-- ============================================================================
