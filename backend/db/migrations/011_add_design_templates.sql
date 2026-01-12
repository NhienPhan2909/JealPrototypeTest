-- ============================================================================
-- Migration: Add Design Templates Feature
-- ============================================================================
-- This migration creates a table to store pre-set design templates and
-- custom templates created by dealership admins.
--
-- Features:
-- - Pre-set templates (is_preset = true) are global and read-only
-- - Custom templates (is_preset = false) are dealership-specific
-- - Templates store combinations of theme colors, background color, and font
--
-- Run this migration:
-- docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend\db\migrations\011_add_design_templates.sql
-- ============================================================================

-- Create design_templates table
CREATE TABLE IF NOT EXISTS design_templates (
  id SERIAL PRIMARY KEY,
  name VARCHAR(100) NOT NULL,
  description TEXT,
  dealership_id INTEGER REFERENCES dealership(id) ON DELETE CASCADE,
  is_preset BOOLEAN DEFAULT false,
  theme_color VARCHAR(7) NOT NULL,
  secondary_theme_color VARCHAR(7) NOT NULL,
  body_background_color VARCHAR(7) NOT NULL,
  font_family VARCHAR(100) NOT NULL,
  created_at TIMESTAMP DEFAULT NOW(),
  
  -- Ensure preset templates have no dealership_id
  CONSTRAINT preset_no_dealership CHECK (
    (is_preset = true AND dealership_id IS NULL) OR
    (is_preset = false AND dealership_id IS NOT NULL)
  ),
  
  -- Unique template names per dealership (or globally for presets)
  CONSTRAINT unique_template_name UNIQUE (name, dealership_id)
);

-- Create index for faster querying
CREATE INDEX idx_design_templates_dealership_id ON design_templates(dealership_id);
CREATE INDEX idx_design_templates_is_preset ON design_templates(is_preset);

-- ============================================================================
-- Seed Pre-set Templates
-- ============================================================================
-- Insert default pre-set templates that all dealerships can use

INSERT INTO design_templates (name, description, is_preset, theme_color, secondary_theme_color, body_background_color, font_family) VALUES
  ('Modern Blue', 'Clean and professional blue theme with modern sans-serif font', true, '#3B82F6', '#FFFFFF', '#FFFFFF', 'inter'),
  ('Classic Black', 'Bold black and white contrast with elegant serif font', true, '#000000', '#FFFFFF', '#F9FAFB', 'playfair'),
  ('Luxury Gold', 'Premium gold accents with sophisticated styling', true, '#D4AF37', '#1F2937', '#FFFFFF', 'montserrat'),
  ('Sporty Red', 'Energetic red theme for performance-focused dealerships', true, '#DC2626', '#FFFFFF', '#FAFAFA', 'poppins'),
  ('Elegant Silver', 'Refined silver and gray palette with clean aesthetics', true, '#71717A', '#FFFFFF', '#F4F4F5', 'roboto'),
  ('Eco Green', 'Fresh green theme for eco-friendly and electric vehicles', true, '#10B981', '#FFFFFF', '#F0FDF4', 'lato'),
  ('Premium Navy', 'Sophisticated navy blue with professional appearance', true, '#1E3A8A', '#FFFFFF', '#EFF6FF', 'opensans'),
  ('Sunset Orange', 'Warm orange tones for a welcoming atmosphere', true, '#F97316', '#FFFFFF', '#FFF7ED', 'nunito');

-- ============================================================================
-- Migration Complete
-- ============================================================================
-- Verify table creation:
--   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "\d design_templates"
--
-- Verify pre-set templates:
--   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT * FROM design_templates WHERE is_preset = true;"
-- ============================================================================
