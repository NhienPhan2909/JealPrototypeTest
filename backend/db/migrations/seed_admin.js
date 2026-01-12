/**
 * @fileoverview Seed initial admin user with bcrypt hashed password.
 * Run this script once during initial setup to create the default admin account.
 * 
 * Usage: node backend/db/migrations/seed_admin.js
 */

const bcrypt = require('bcrypt');
const { Pool } = require('pg');
require('dotenv').config({ path: require('path').join(__dirname, '../../../.env') });

const SALT_ROUNDS = 10;
const DEFAULT_ADMIN_USERNAME = 'admin';
const DEFAULT_ADMIN_PASSWORD = 'admin123';
const DEFAULT_ADMIN_EMAIL = 'admin@example.com';
const DEFAULT_ADMIN_NAME = 'System Administrator';

async function seedAdminUser() {
  const pool = new Pool({
    host: process.env.DB_HOST || 'localhost',
    port: process.env.DB_PORT || 5432,
    database: process.env.DB_NAME || 'jeal_prototype',
    user: process.env.DB_USER || 'postgres',
    password: process.env.DB_PASSWORD || 'postgres'
  });

  try {
    console.log('Connecting to database...');
    
    // Check if admin user already exists
    const checkQuery = "SELECT id FROM app_user WHERE user_type = 'admin' LIMIT 1";
    const checkResult = await pool.query(checkQuery);
    
    if (checkResult.rows.length > 0) {
      console.log('⚠️  Admin user already exists. Skipping seed.');
      await pool.end();
      return;
    }

    // Hash password
    console.log('Hashing password...');
    const password_hash = await bcrypt.hash(DEFAULT_ADMIN_PASSWORD, SALT_ROUNDS);

    // Insert admin user
    console.log('Creating admin user...');
    const insertQuery = `
      INSERT INTO app_user (username, password_hash, email, full_name, user_type, dealership_id, permissions)
      VALUES ($1, $2, $3, $4, 'admin', NULL, '[]'::jsonb)
      RETURNING id, username, email, full_name, user_type
    `;
    
    const result = await pool.query(insertQuery, [
      DEFAULT_ADMIN_USERNAME,
      password_hash,
      DEFAULT_ADMIN_EMAIL,
      DEFAULT_ADMIN_NAME
    ]);

    console.log('\n✅ Default admin user created successfully!');
    console.log('═══════════════════════════════════════════');
    console.log(`Username: ${DEFAULT_ADMIN_USERNAME}`);
    console.log(`Password: ${DEFAULT_ADMIN_PASSWORD}`);
    console.log(`Email:    ${DEFAULT_ADMIN_EMAIL}`);
    console.log('═══════════════════════════════════════════');
    console.log('⚠️  IMPORTANT: Change the admin password immediately after first login!');
    console.log('\nYou can now log in to the CMS admin panel at /admin/login\n');

    await pool.end();
  } catch (error) {
    console.error('❌ Error seeding admin user:', error);
    await pool.end();
    process.exit(1);
  }
}

// Run the seed function
seedAdminUser();
