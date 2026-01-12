-- Migration: Add body_background_color column to dealership table
-- This allows dealerships to customize the background color of the website body
-- alongside the primary and secondary theme colors

ALTER TABLE dealership
ADD COLUMN body_background_color VARCHAR(7) DEFAULT '#FFFFFF';

-- Update the comment for clarity
COMMENT ON COLUMN dealership.body_background_color IS 'Body background color in hex format (e.g., #FFFFFF) for website body background';
