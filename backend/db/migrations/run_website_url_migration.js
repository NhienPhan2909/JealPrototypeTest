/**
 * @fileoverview Migration runner to add website_url field to dealership table
 * Run with: node backend/db/migrations/run_website_url_migration.js
 */

const { Pool } = require('pg');
const fs = require('fs');
const path = require('path');
require('dotenv').config({ path: path.join(__dirname, '../../../.env') });

const pool = new Pool({
  connectionString: process.env.DATABASE_URL
});

async function runMigration() {
  try {
    console.log('🔍 Checking current database schema...');

    // Check if column already exists
    const checkQuery = `
      SELECT column_name
      FROM information_schema.columns
      WHERE table_name = 'dealership'
      AND column_name = 'website_url'
    `;

    const existingColumns = await pool.query(checkQuery);

    if (existingColumns.rows.length > 0) {
      console.log('✅ Column website_url already exists');
      console.log('Migration already applied. Skipping.');
      await pool.end();
      return;
    }

    console.log('📝 Reading migration SQL file...');
    const migrationSQL = fs.readFileSync(
      path.join(__dirname, '012_add_website_url.sql'),
      'utf8'
    );

    console.log('⚙️  Running migration...');
    await pool.query(migrationSQL);

    console.log('✅ Migration completed successfully!');

    // Verify migration
    const verifyQuery = `
      SELECT id, name,
        CASE WHEN website_url IS NULL THEN 'Not Set' ELSE website_url END as website_url
      FROM dealership
    `;

    const verification = await pool.query(verifyQuery);
    console.log('\n📊 Dealership Website URL Status:');
    console.table(verification.rows);

    await pool.end();
    process.exit(0);
  } catch (error) {
    console.error('❌ Migration failed:', error.message);
    await pool.end();
    process.exit(1);
  }
}

runMigration();
