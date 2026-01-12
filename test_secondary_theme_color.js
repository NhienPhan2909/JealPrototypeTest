/**
 * Test script for Secondary Theme Color feature
 * 
 * This script tests the API endpoints to ensure the secondary_theme_color
 * field is properly handled in GET and PUT requests.
 * 
 * Prerequisites:
 * - Backend server must be running (npm run dev in backend/)
 * - Database must have the secondary_theme_color column added
 * 
 * Run with: node test_secondary_theme_color.js
 */

const BASE_URL = 'http://localhost:3000';
const DEALERSHIP_ID = 1; // Test with dealership ID 1

async function testGetDealership() {
  console.log('\n=== Testing GET /api/dealers/:id ===');
  try {
    const response = await fetch(`${BASE_URL}/api/dealers/${DEALERSHIP_ID}`);
    const data = await response.json();
    
    console.log('Status:', response.status);
    console.log('Dealership Name:', data.name);
    console.log('Primary Theme Color:', data.theme_color || '(not set)');
    console.log('Secondary Theme Color:', data.secondary_theme_color || '(not set)');
    
    if (response.ok) {
      console.log('✅ GET request successful');
      return data;
    } else {
      console.log('❌ GET request failed');
      return null;
    }
  } catch (error) {
    console.error('❌ Error:', error.message);
    return null;
  }
}

async function testUpdateDealership() {
  console.log('\n=== Testing PUT /api/dealers/:id ===');
  
  // First, get current dealership data
  const currentData = await testGetDealership();
  if (!currentData) {
    console.log('❌ Cannot proceed with update test - GET failed');
    return;
  }
  
  // Update only the secondary theme color
  const updateData = {
    name: currentData.name,
    address: currentData.address,
    phone: currentData.phone,
    email: currentData.email,
    secondary_theme_color: '#FF5733' // Test color (orange-red)
  };
  
  console.log('\nUpdating secondary_theme_color to:', updateData.secondary_theme_color);
  
  try {
    const response = await fetch(`${BASE_URL}/api/dealers/${DEALERSHIP_ID}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(updateData)
    });
    
    const data = await response.json();
    
    console.log('Status:', response.status);
    console.log('Updated Secondary Theme Color:', data.secondary_theme_color);
    
    if (response.ok && data.secondary_theme_color === updateData.secondary_theme_color) {
      console.log('✅ PUT request successful - color updated correctly');
    } else {
      console.log('❌ PUT request failed or color not updated');
      console.log('Response:', data);
    }
  } catch (error) {
    console.error('❌ Error:', error.message);
  }
}

async function testColorValidation() {
  console.log('\n=== Testing Color Validation ===');
  
  const invalidColors = [
    'red',           // Invalid: not hex format
    '#12345',        // Invalid: wrong length
    '#GGGGGG',       // Invalid: non-hex characters
    'rgb(255,0,0)',  // Invalid: not hex format
  ];
  
  const validColors = [
    '#FF0000',       // Valid: 6-digit hex
    '#F00',          // Valid: 3-digit hex
    '#1a2b3c',       // Valid: lowercase hex
  ];
  
  console.log('\nTesting invalid colors (should fail):');
  for (const color of invalidColors) {
    try {
      const response = await fetch(`${BASE_URL}/api/dealers/${DEALERSHIP_ID}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          name: 'Test Dealership',
          address: '123 Test St',
          phone: '555-0000',
          email: 'test@test.com',
          secondary_theme_color: color
        })
      });
      
      const data = await response.json();
      
      if (response.status === 400) {
        console.log(`  ✅ "${color}" correctly rejected`);
      } else {
        console.log(`  ❌ "${color}" was accepted (should have been rejected)`);
      }
    } catch (error) {
      console.log(`  ❌ Error testing "${color}":`, error.message);
    }
  }
  
  console.log('\nTesting valid colors (should succeed):');
  for (const color of validColors) {
    try {
      const currentData = await fetch(`${BASE_URL}/api/dealers/${DEALERSHIP_ID}`).then(r => r.json());
      
      const response = await fetch(`${BASE_URL}/api/dealers/${DEALERSHIP_ID}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          name: currentData.name,
          address: currentData.address,
          phone: currentData.phone,
          email: currentData.email,
          secondary_theme_color: color
        })
      });
      
      const data = await response.json();
      
      if (response.ok) {
        console.log(`  ✅ "${color}" correctly accepted`);
      } else {
        console.log(`  ❌ "${color}" was rejected (should have been accepted)`);
        console.log('     Error:', data.error);
      }
    } catch (error) {
      console.log(`  ❌ Error testing "${color}":`, error.message);
    }
  }
}

async function runTests() {
  console.log('╔════════════════════════════════════════════════╗');
  console.log('║  Secondary Theme Color Feature Test Suite     ║');
  console.log('╚════════════════════════════════════════════════╝');
  
  await testGetDealership();
  await testUpdateDealership();
  await testColorValidation();
  
  console.log('\n╔════════════════════════════════════════════════╗');
  console.log('║  Test Suite Complete                           ║');
  console.log('╚════════════════════════════════════════════════╝\n');
}

// Run tests
runTests().catch(error => {
  console.error('Fatal error running tests:', error);
  process.exit(1);
});
