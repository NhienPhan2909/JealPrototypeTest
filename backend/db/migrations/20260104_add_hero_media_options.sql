-- ============================================================================
-- Migration: Add hero media options (image, video, carousel) to dealership table
-- ============================================================================
-- Extends hero section to support:
-- - Static image (existing functionality)
-- - Video background
-- - Image carousel
--
-- Run this migration via Docker (local development):
-- docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend\db\migrations\20260104_add_hero_media_options.sql
--
-- Or using local PostgreSQL:
-- psql -h localhost -p 5432 -U postgres -d jeal_prototype -f backend\db\migrations\20260104_add_hero_media_options.sql
-- ============================================================================

-- Add hero_type column to specify which media type is being used
ALTER TABLE dealership
ADD COLUMN IF NOT EXISTS hero_type VARCHAR(20) DEFAULT 'image' CHECK (hero_type IN ('image', 'video', 'carousel'));

-- Add hero_video_url column for video background
ALTER TABLE dealership
ADD COLUMN IF NOT EXISTS hero_video_url TEXT;

-- Add hero_carousel_images column for carousel images (JSONB array of URLs)
ALTER TABLE dealership
ADD COLUMN IF NOT EXISTS hero_carousel_images JSONB DEFAULT '[]'::jsonb;

-- Add comments for documentation
COMMENT ON COLUMN dealership.hero_type IS 'Type of hero media: image (default), video, or carousel';
COMMENT ON COLUMN dealership.hero_video_url IS 'Cloudinary URL for hero section background video';
COMMENT ON COLUMN dealership.hero_carousel_images IS 'Array of Cloudinary URLs for hero carousel images';

-- ============================================================================
-- MIGRATION COMPLETE
-- ============================================================================
-- Verify columns were added:
--   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "\d dealership"
-- ============================================================================
