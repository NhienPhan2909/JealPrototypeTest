-- Migration: Add social media fields to dealership table
-- Purpose: Store Facebook and Instagram URLs for footer display
-- Date: 2025-12-08
-- Story: Footer social media links feature

ALTER TABLE dealership
ADD COLUMN IF NOT EXISTS facebook_url TEXT,
ADD COLUMN IF NOT EXISTS instagram_url TEXT;

-- Add comments for documentation
COMMENT ON COLUMN dealership.facebook_url IS 'Facebook page URL for dealership (displayed in footer)';
COMMENT ON COLUMN dealership.instagram_url IS 'Instagram profile URL for dealership (displayed in footer)';
