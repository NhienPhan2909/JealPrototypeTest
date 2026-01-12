-- Migration: Add secondary_theme_color column to dealership table
-- This allows dealerships to customize a secondary color for their website branding
-- alongside the primary theme_color

ALTER TABLE dealership
ADD COLUMN secondary_theme_color VARCHAR(7) DEFAULT '#FFFFFF';

-- Update the comment for clarity
COMMENT ON COLUMN dealership.secondary_theme_color IS 'Secondary theme color in hex format (e.g., #FFFFFF) for website branding elements';
