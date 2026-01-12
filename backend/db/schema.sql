-- ============================================================================
-- Database Schema for Multi-Dealership Car Website Platform
-- ============================================================================
-- This script creates the complete database schema with:
-- - 3 core tables: dealership, vehicle, lead
-- - Foreign key constraints for multi-tenancy enforcement
-- - CHECK constraints for data validation
-- - Performance indexes for common query patterns
--
-- Run this script via Docker (local development):
-- docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype < backend\db\schema.sql
-- ============================================================================

-- Drop existing tables (in reverse dependency order to avoid FK conflicts)
DROP TABLE IF EXISTS sales_request CASCADE;
DROP TABLE IF EXISTS lead CASCADE;
DROP TABLE IF EXISTS vehicle CASCADE;
DROP TABLE IF EXISTS dealership CASCADE;

-- ============================================================================
-- DEALERSHIP TABLE
-- ============================================================================
-- Primary tenant entity. Each dealership is independent with its own:
-- - Profile information (name, logo, contact details)
-- - Inventory (vehicles)
-- - Customer enquiries (leads)
--
-- Multi-tenancy root: All vehicle and lead records reference a dealership_id

CREATE TABLE dealership (
  id SERIAL PRIMARY KEY,
  name VARCHAR(255) NOT NULL,
  logo_url TEXT,
  address TEXT NOT NULL,
  phone VARCHAR(20) NOT NULL,
  email VARCHAR(255) NOT NULL,
  hours TEXT,
  about TEXT,
  created_at TIMESTAMP DEFAULT NOW()
);

-- ============================================================================
-- VEHICLE TABLE
-- ============================================================================
-- Dealership inventory items with multi-tenant isolation via dealership_id FK.
--
-- Key Features:
-- - JSONB images array for Cloudinary URLs
-- - CHECK constraints enforce valid condition (new/used) and status values
-- - ON DELETE CASCADE ensures vehicles are deleted when dealership is removed
-- - Status field controls public visibility (active/pending visible, sold/draft hidden)

CREATE TABLE vehicle (
  id SERIAL PRIMARY KEY,
  dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE,
  make VARCHAR(100) NOT NULL,
  model VARCHAR(100) NOT NULL,
  year INTEGER NOT NULL,
  price DECIMAL(10,2) NOT NULL,
  mileage INTEGER NOT NULL,
  condition VARCHAR(10) NOT NULL CHECK (condition IN ('new', 'used')),
  status VARCHAR(10) NOT NULL DEFAULT 'draft' CHECK (status IN ('active', 'sold', 'pending', 'draft')),
  title VARCHAR(255) NOT NULL,
  description TEXT,
  images JSONB DEFAULT '[]'::jsonb,
  created_at TIMESTAMP DEFAULT NOW()
);

-- ============================================================================
-- LEAD TABLE
-- ============================================================================
-- Customer enquiries from public website.
--
-- Key Features:
-- - Multi-tenancy via dealership_id FK (ON DELETE CASCADE)
-- - Optional vehicle_id FK (ON DELETE SET NULL preserves lead history if vehicle deleted)
-- - Stores customer contact info and message for dealership follow-up

CREATE TABLE lead (
  id SERIAL PRIMARY KEY,
  dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE,
  vehicle_id INTEGER REFERENCES vehicle(id) ON DELETE SET NULL,
  name VARCHAR(255) NOT NULL,
  email VARCHAR(255) NOT NULL,
  phone VARCHAR(20) NOT NULL,
  message TEXT NOT NULL,
  status VARCHAR(20) DEFAULT 'received' CHECK (status IN ('received', 'in progress', 'done')),
  created_at TIMESTAMP DEFAULT NOW()
);

-- ============================================================================
-- SALES_REQUEST TABLE
-- ============================================================================
-- Customer requests to sell their vehicles to dealerships.
--
-- Key Features:
-- - Multi-tenancy via dealership_id FK (ON DELETE CASCADE)
-- - Stores customer contact info and vehicle details
-- - Status field for tracking (received, in progress, done)

CREATE TABLE sales_request (
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

-- ============================================================================
-- PERFORMANCE INDEXES
-- ============================================================================
-- Indexes optimized for multi-tenant query patterns:
-- - All vehicle/lead queries filter by dealership_id
-- - Public site filters vehicles by status (active/pending)
-- - Admin lead inbox sorts by created_at DESC

-- Critical multi-tenancy indexes (prevent full table scans)
CREATE INDEX idx_vehicle_dealership_id ON vehicle(dealership_id);
CREATE INDEX idx_lead_dealership_id ON lead(dealership_id);
CREATE INDEX idx_sales_request_dealership_id ON sales_request(dealership_id);

-- Public site filtering indexes
CREATE INDEX idx_vehicle_status ON vehicle(status);

-- Admin panel query optimization
CREATE INDEX idx_lead_created_at ON lead(created_at DESC);
CREATE INDEX idx_sales_request_created_at ON sales_request(created_at DESC);

-- Composite index for common query pattern (dealership + status filtering)
CREATE INDEX idx_vehicle_dealership_status ON vehicle(dealership_id, status);

-- ============================================================================
-- SCHEMA SETUP COMPLETE
-- ============================================================================
-- Verify table creation:
--   docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "\dt"
--
-- Expected output: dealership, vehicle, lead tables
-- ============================================================================
