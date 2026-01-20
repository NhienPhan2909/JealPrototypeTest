/**
 * Test script for dealership creation functionality
 * Tests the new POST /api/dealers endpoint (admin only)
 */

const BASE_URL = 'http://localhost:3001';

async function testDealershipCreation() {
  console.log('=== Testing Dealership Creation Feature ===\n');

  // Step 1: Login as admin
  console.log('Step 1: Logging in as admin...');
  const loginResponse = await fetch(`${BASE_URL}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify({
      username: 'admin',
      password: 'admin123'
    })
  });

  if (!loginResponse.ok) {
    console.error('❌ Login failed');
    return;
  }
  
  const loginData = await loginResponse.json();
  console.log('✅ Login successful:', loginData.user.username);
  
  // Get session cookie
  const cookies = loginResponse.headers.get('set-cookie');

  // Step 2: Get current dealerships count
  console.log('\nStep 2: Fetching current dealerships...');
  const dealershipsResponse = await fetch(`${BASE_URL}/api/dealers`, {
    credentials: 'include',
    headers: { 'Cookie': cookies || '' }
  });

  if (!dealershipsResponse.ok) {
    console.error('❌ Failed to fetch dealerships');
    return;
  }

  const dealerships = await dealershipsResponse.json();
  console.log(`✅ Current dealerships count: ${dealerships.length}`);

  // Step 3: Create a new dealership
  console.log('\nStep 3: Creating new dealership...');
  const newDealershipData = {
    name: 'Test Auto Sales',
    address: '456 Test Street, Test City, TS 12345',
    phone: '(555) 987-6543',
    email: 'info@testautosales.com',
    hours: 'Mon-Fri: 9am-6pm\nSat: 10am-4pm\nSun: Closed',
    about: 'This is a test dealership created to verify the dealership creation feature.'
  };

  const createResponse = await fetch(`${BASE_URL}/api/dealers`, {
    method: 'POST',
    headers: { 
      'Content-Type': 'application/json',
      'Cookie': cookies || ''
    },
    credentials: 'include',
    body: JSON.stringify(newDealershipData)
  });

  if (!createResponse.ok) {
    const errorData = await createResponse.json();
    console.error('❌ Failed to create dealership:', errorData);
    return;
  }

  const createdDealership = await createResponse.json();
  console.log('✅ Dealership created successfully!');
  console.log('   ID:', createdDealership.id);
  console.log('   Name:', createdDealership.name);
  console.log('   Email:', createdDealership.email);
  console.log('   Phone:', createdDealership.phone);

  // Step 4: Verify the dealership was created
  console.log('\nStep 4: Verifying dealership was created...');
  const verifyResponse = await fetch(`${BASE_URL}/api/dealers/${createdDealership.id}`, {
    credentials: 'include',
    headers: { 'Cookie': cookies || '' }
  });

  if (!verifyResponse.ok) {
    console.error('❌ Failed to fetch created dealership');
    return;
  }

  const fetchedDealership = await verifyResponse.json();
  console.log('✅ Dealership verified:');
  console.log('   Name:', fetchedDealership.name);
  console.log('   Address:', fetchedDealership.address);

  console.log('\n=== All tests passed! ===');
  console.log('\nNext steps:');
  console.log('1. Navigate to http://localhost:5173/admin/login');
  console.log('2. Login as admin (username: admin, password: admin123)');
  console.log('3. Click on "Dealership Management" in the navigation');
  console.log('4. You should see the "Test Auto Sales" dealership in the list');
  console.log('5. Click "+ Create New Dealership" to test the UI form');
}

// Run the test
testDealershipCreation().catch(error => {
  console.error('Test failed with error:', error);
});
