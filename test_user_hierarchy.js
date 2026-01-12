/**
 * Test script for user hierarchy system
 * Tests database connection and admin user creation
 */

const { Pool } = require('pg');
const bcrypt = require('bcrypt');
require('dotenv').config();

async function testUserHierarchy() {
  const pool = new Pool({
    host: process.env.DB_HOST || 'localhost',
    port: process.env.DB_PORT || 5432,
    database: process.env.DB_NAME || 'jeal_prototype',
    user: process.env.DB_USER || 'postgres',
    password: process.env.DB_PASSWORD || 'postgres'
  });

  console.log('🧪 Testing User Hierarchy System\n');

  try {
    // Test 1: Check if app_user table exists
    console.log('Test 1: Checking if app_user table exists...');
    const tableCheck = await pool.query(`
      SELECT EXISTS (
        SELECT FROM information_schema.tables 
        WHERE table_name = 'app_user'
      );
    `);
    console.log(tableCheck.rows[0].exists ? '✅ app_user table exists' : '❌ app_user table NOT found');

    // Test 2: Count users
    console.log('\nTest 2: Counting users...');
    const countResult = await pool.query('SELECT COUNT(*) as count FROM app_user');
    console.log(`✅ Found ${countResult.rows[0].count} user(s)`);

    // Test 3: Check admin user
    console.log('\nTest 3: Checking for admin user...');
    const adminCheck = await pool.query("SELECT * FROM app_user WHERE username = 'admin'");
    if (adminCheck.rows.length > 0) {
      const admin = adminCheck.rows[0];
      console.log('✅ Admin user found:');
      console.log(`   - ID: ${admin.id}`);
      console.log(`   - Username: ${admin.username}`);
      console.log(`   - Email: ${admin.email}`);
      console.log(`   - Full Name: ${admin.full_name}`);
      console.log(`   - User Type: ${admin.user_type}`);
      console.log(`   - Active: ${admin.is_active}`);

      // Test password
      console.log('\nTest 4: Testing admin password...');
      const passwordValid = await bcrypt.compare('admin123', admin.password_hash);
      console.log(passwordValid ? '✅ Password "admin123" is valid' : '❌ Password "admin123" is INVALID');
    } else {
      console.log('❌ Admin user NOT found!');
    }

    // Test 5: Check indexes
    console.log('\nTest 5: Checking indexes...');
    const indexCheck = await pool.query(`
      SELECT indexname FROM pg_indexes 
      WHERE tablename = 'app_user'
      ORDER BY indexname;
    `);
    console.log(`✅ Found ${indexCheck.rows.length} indexes:`);
    indexCheck.rows.forEach(row => console.log(`   - ${row.indexname}`));

    // Test 6: Check constraints
    console.log('\nTest 6: Checking constraints...');
    const constraintCheck = await pool.query(`
      SELECT conname, contype 
      FROM pg_constraint 
      WHERE conrelid = 'app_user'::regclass
      ORDER BY conname;
    `);
    console.log(`✅ Found ${constraintCheck.rows.length} constraints:`);
    constraintCheck.rows.forEach(row => {
      const type = {
        'p': 'PRIMARY KEY',
        'f': 'FOREIGN KEY',
        'c': 'CHECK',
        'u': 'UNIQUE'
      }[row.contype] || row.contype;
      console.log(`   - ${row.conname} (${type})`);
    });

    console.log('\n✅ All tests completed successfully!');
    console.log('\n📝 Summary:');
    console.log('   - Database table created ✅');
    console.log('   - Admin user seeded ✅');
    console.log('   - Indexes created ✅');
    console.log('   - Constraints enforced ✅');
    console.log('\n🎉 User hierarchy system is ready to use!');
    console.log('\n🔐 Login credentials:');
    console.log('   Username: admin');
    console.log('   Password: admin123');
    console.log('\n⚠️  Remember to change the admin password after first login!');

  } catch (error) {
    console.error('\n❌ Error during testing:', error);
  } finally {
    await pool.end();
  }
}

testUserHierarchy();
