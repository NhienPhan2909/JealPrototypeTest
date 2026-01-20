-- Migration: Add website_url field to dealership table
-- Purpose: Allow each dealership to have a unique URL/domain
-- Run via: node backend/db/migrations/run_migration.js

-- Add website_url column to dealership table
-- This field will store the custom URL/domain for each dealership
-- Examples: 'acme-auto.com', 'premium-motors.com', etc.
ALTER TABLE dealership
ADD COLUMN IF NOT EXISTS website_url VARCHAR(255) UNIQUE;

-- Add index for faster lookups by URL
CREATE INDEX IF NOT EXISTS idx_dealership_website_url ON dealership(website_url);

-- Add comment for documentation
COMMENT ON COLUMN dealership.website_url IS 'Custom URL/domain for dealership website (e.g., acme-auto.com). Must be unique across all dealerships.';
