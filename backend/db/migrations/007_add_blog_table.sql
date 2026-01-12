-- Migration: Add blog_post table
-- Description: Creates table for storing dealership blog articles
-- Date: 2025-12-31

-- Create blog_post table
CREATE TABLE IF NOT EXISTS blog_post (
  id SERIAL PRIMARY KEY,
  dealership_id INTEGER NOT NULL REFERENCES dealership(id) ON DELETE CASCADE,
  title VARCHAR(255) NOT NULL,
  slug VARCHAR(255) NOT NULL,
  content TEXT NOT NULL,
  excerpt TEXT,
  featured_image_url TEXT,
  author_name VARCHAR(255) NOT NULL,
  status VARCHAR(20) DEFAULT 'draft' CHECK (status IN ('draft', 'published', 'archived')),
  published_at TIMESTAMP,
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW(),
  UNIQUE(dealership_id, slug)
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_blog_post_dealership_id ON blog_post(dealership_id);
CREATE INDEX IF NOT EXISTS idx_blog_post_status ON blog_post(status);
CREATE INDEX IF NOT EXISTS idx_blog_post_published_at ON blog_post(published_at DESC);
CREATE INDEX IF NOT EXISTS idx_blog_post_dealership_status ON blog_post(dealership_id, status);

-- Verify table creation
SELECT 'blog_post table created successfully' AS status;
