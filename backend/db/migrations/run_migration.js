/**
 * @fileoverview Migration runner script to add finance_policy and warranty_policy fields
 * Run with: node backend/db/migrations/run_migration.js
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

    // Check if columns already exist
    const checkQuery = `
      SELECT column_name
      FROM information_schema.columns
      WHERE table_name = 'dealership'
      AND column_name IN ('finance_policy', 'warranty_policy')
    `;

    const existingColumns = await pool.query(checkQuery);

    if (existingColumns.rows.length > 0) {
      console.log('✅ Columns already exist:', existingColumns.rows.map(r => r.column_name).join(', '));
      console.log('Migration already applied. Skipping.');
      await pool.end();
      return;
    }

    console.log('📝 Reading migration SQL file...');
    const migrationSQL = fs.readFileSync(
      path.join(__dirname, 'add_finance_warranty_fields.sql'),
      'utf8'
    );

    console.log('⚙️  Running migration...');
    await pool.query(migrationSQL);

    console.log('✅ Migration completed successfully!');

    // Verify migration
    const verifyQuery = `
      SELECT id, name,
        CASE WHEN finance_policy IS NULL THEN 'Not Set' ELSE 'Set' END as finance_status,
        CASE WHEN warranty_policy IS NULL THEN 'Not Set' ELSE 'Set' END as warranty_status
      FROM dealership
    `;

    const verification = await pool.query(verifyQuery);
    console.log('\n📊 Dealership Finance & Warranty Status:');
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
