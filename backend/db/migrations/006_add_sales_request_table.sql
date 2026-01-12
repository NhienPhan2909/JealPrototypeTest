-- Migration: Add sales_request table
-- Description: Creates table for storing customer vehicle sales requests
-- Date: 2025-12-17

-- Create sales_request table
CREATE TABLE IF NOT EXISTS sales_request (
  id SERIAL PRIMARY KEY,
  dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE,
  name VARCHAR(255) NOT NULL,
  email VARCHAR(255) NOT NULL,
  phone VARCHAR(20) NOT NULL,
  make VARCHAR(100) NOT NULL,
  model VARCHAR(100) NOT NULL,
  year INTEGER NOT NULL,
  kilometers INTEGER NOT NULL,
  additional_message TEXT,
  status VARCHAR(20) DEFAULT 'received' CHECK (status IN ('received', 'in progress', 'done')),
  created_at TIMESTAMP DEFAULT NOW()
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_sales_request_dealership_id ON sales_request(dealership_id);
CREATE INDEX IF NOT EXISTS idx_sales_request_created_at ON sales_request(created_at DESC);

-- Verify table creation
SELECT 'sales_request table created successfully' AS status;
