-- Migration: Add hero_title and hero_subtitle fields to dealership table
-- Purpose: Allow customization of hero section text (title and subtitle/tagline)
-- Date: 2026-02-09

-- Add hero_title column (optional custom title, falls back to dealership name)
ALTER TABLE dealership
ADD COLUMN IF NOT EXISTS hero_title TEXT;

-- Add hero_subtitle column (optional custom subtitle, falls back to about text or default message)
ALTER TABLE dealership
ADD COLUMN IF NOT EXISTS hero_subtitle TEXT;

-- Add comments for documentation
COMMENT ON COLUMN dealership.hero_title IS 'Custom hero section title text (optional, defaults to dealership name)';
COMMENT ON COLUMN dealership.hero_subtitle IS 'Custom hero section subtitle/tagline text (optional, defaults to about text or default message)';
