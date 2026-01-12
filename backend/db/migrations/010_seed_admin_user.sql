-- ============================================================================
-- Seed initial admin user
-- ============================================================================
-- Creates a default admin account for initial setup.
-- Password: admin123 (should be changed after first login)
-- This seed should only be run once during initial setup.
-- ============================================================================

-- Check if any admin users exist before inserting
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM app_user WHERE user_type = 'admin') THEN
    -- Insert default admin user
    -- Username: admin
    -- Password: admin123 (bcrypt hash)
    INSERT INTO app_user (username, password_hash, email, full_name, user_type, dealership_id, permissions)
    VALUES (
      'admin',
      '$2b$10$YourBcryptHashHere', -- This will be replaced by migration script
      'admin@example.com',
      'System Administrator',
      'admin',
      NULL,
      '[]'::jsonb
    );
    
    RAISE NOTICE 'Default admin user created: username=admin, password=admin123';
    RAISE NOTICE 'IMPORTANT: Change the admin password immediately after first login!';
  ELSE
    RAISE NOTICE 'Admin user already exists, skipping seed.';
  END IF;
END $$;
