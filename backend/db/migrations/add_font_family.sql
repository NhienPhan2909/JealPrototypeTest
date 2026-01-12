-- ============================================================================
-- Add font_family column to dealership table
-- ============================================================================
-- This migration adds font customization support for each dealership.
-- The font_family is applied globally to all text on the dealership website.
--
-- Run this migration:
-- docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend/db/migrations/add_font_family.sql
-- ============================================================================

-- Add font_family column with default value
ALTER TABLE dealership
ADD COLUMN IF NOT EXISTS font_family VARCHAR(100) DEFAULT 'system';

-- Add comment for documentation
COMMENT ON COLUMN dealership.font_family IS 'Font family for dealership website. Options: system, arial, times, georgia, verdana, courier, comic-sans, trebuchet, impact, palatino. Default is system (uses browser default).';

-- Verify column was added
SELECT column_name, data_type, column_default
FROM information_schema.columns
WHERE table_name = 'dealership' AND column_name = 'font_family';

-- ============================================================================
-- MIGRATION COMPLETE
-- ============================================================================
