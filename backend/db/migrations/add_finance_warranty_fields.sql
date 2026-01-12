-- ============================================
-- Migration: Add Finance and Warranty Policy Fields
-- Stories: 2.8, 2.9, 3.8
-- Date: 2025-11-27
-- ============================================

-- Add finance_policy and warranty_policy columns to dealership table
ALTER TABLE dealership
  ADD COLUMN IF NOT EXISTS finance_policy TEXT,
  ADD COLUMN IF NOT EXISTS warranty_policy TEXT;

-- Optional: Update existing dealerships with default sample content
-- Uncomment if you want to populate existing dealerships with sample policies

-- UPDATE dealership
-- SET finance_policy = 'We offer flexible financing options to fit your budget. Our finance team works with multiple lenders to secure competitive rates. Contact us for a personalized financing quote.',
--     warranty_policy = 'All vehicles come with a limited warranty covering major mechanical components. Extended warranty options available for purchase. Contact us for complete warranty terms and conditions.'
-- WHERE finance_policy IS NULL OR warranty_policy IS NULL;

-- Verification query
SELECT id, name,
  CASE WHEN finance_policy IS NULL THEN 'Not Set' ELSE 'Set' END as finance_status,
  CASE WHEN warranty_policy IS NULL THEN 'Not Set' ELSE 'Set' END as warranty_status
FROM dealership;
