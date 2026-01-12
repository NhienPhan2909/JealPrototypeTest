-- Migration: Add status column to lead table
-- Description: Adds status tracking to leads with default 'received' value
-- Date: 2025-12-10

-- Add status column with default value 'received'
ALTER TABLE lead 
ADD COLUMN IF NOT EXISTS status VARCHAR(20) DEFAULT 'received' 
CHECK (status IN ('received', 'in progress', 'done'));

-- Update any existing leads to have 'received' status
UPDATE lead SET status = 'received' WHERE status IS NULL;

-- Add comment for documentation
COMMENT ON COLUMN lead.status IS 'Lead progress status: received (new lead), in progress (being processed), done (completed)';
