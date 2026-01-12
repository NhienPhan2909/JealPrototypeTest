-- Migration: Add promotional panels fields for Finance and Warranty sections
-- Purpose: Add image and text fields for promotional panels displayed on homepage

ALTER TABLE dealership
ADD COLUMN finance_promo_image TEXT,
ADD COLUMN finance_promo_text TEXT,
ADD COLUMN warranty_promo_image TEXT,
ADD COLUMN warranty_promo_text TEXT;

COMMENT ON COLUMN dealership.finance_promo_image IS 'Background image URL for Finance promotional panel on homepage';
COMMENT ON COLUMN dealership.finance_promo_text IS 'Promotional text overlay for Finance panel on homepage';
COMMENT ON COLUMN dealership.warranty_promo_image IS 'Background image URL for Warranty promotional panel on homepage';
COMMENT ON COLUMN dealership.warranty_promo_text IS 'Promotional text overlay for Warranty panel on homepage';
