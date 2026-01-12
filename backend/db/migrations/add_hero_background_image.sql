-- ============================================================================
-- Migration: Add hero_background_image field to dealership table
-- ============================================================================
-- Adds support for customizable hero background images on dealership home pages.
-- Each dealership can upload a unique hero background image via CMS.
--
-- Run this migration via Docker (local development):
-- docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend\db\migrations\add_hero_background_image.sql
--
-- Or using local PostgreSQL:
-- psql -h localhost -p 5432 -U postgres -d jeal_prototype -f backend\db\migrations\add_hero_background_image.sql
-- ============================================================================

-- Add hero_background_image column to dealership table
ALTER TABLE dealership
ADD COLUMN IF NOT EXISTS hero_background_image TEXT;

-- Add comment for documentation
COMMENT ON COLUMN dealership.hero_background_image IS 'Cloudinary URL for hero section background image on public home page';

-- ============================================================================
-- MIGRATION COMPLETE
-- ============================================================================
-- Verify column was added:
--   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "\d dealership"
-- ============================================================================
