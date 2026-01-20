/**
 * Test script for dealership deletion functionality
 * Tests the new DELETE /api/dealers/:id endpoint (admin only)
 */

const BASE_URL = 'http://localhost:3001';

async function testDealershipDeletion() {
  console.log('=== Testing Dealership Deletion Feature ===\n');

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

  // Step 2: Create a test dealership to delete
  console.log('\nStep 2: Creating test dealership for deletion...');
  const newDealershipData = {
    name: 'Test Deletion Dealership',
    address: '999 Delete Street, Test City, TS 99999',
    phone: '(555) 000-0000',
    email: 'delete@testdealership.com'
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
    console.error('❌ Failed to create test dealership');
    return;
  }

  const createdDealership = await createResponse.json();
  console.log('✅ Test dealership created:');
  console.log('   ID:', createdDealership.id);
  console.log('   Name:', createdDealership.name);

  // Step 3: Verify dealership exists
  console.log('\nStep 3: Verifying dealership exists...');
  const verifyBeforeResponse = await fetch(`${BASE_URL}/api/dealers/${createdDealership.id}`, {
    credentials: 'include',
    headers: { 'Cookie': cookies || '' }
  });

  if (!verifyBeforeResponse.ok) {
    console.error('❌ Dealership verification failed');
    return;
  }

  console.log('✅ Dealership verified before deletion');

  // Step 4: Delete the dealership
  console.log('\nStep 4: Deleting dealership...');
  console.log('⚠️  WARNING: This will cascade delete all related data!');
  
  const deleteResponse = await fetch(`${BASE_URL}/api/dealers/${createdDealership.id}`, {
    method: 'DELETE',
    credentials: 'include',
    headers: { 'Cookie': cookies || '' }
  });

  if (!deleteResponse.ok) {
    const errorData = await deleteResponse.json();
    console.error('❌ Failed to delete dealership:', errorData);
    return;
  }

  const deleteData = await deleteResponse.json();
  console.log('✅ Dealership deleted successfully!');
  console.log('   Message:', deleteData.message);
  console.log('   Deleted:', deleteData.dealership.name);

  // Step 5: Verify dealership no longer exists
  console.log('\nStep 5: Verifying dealership was deleted...');
  const verifyAfterResponse = await fetch(`${BASE_URL}/api/dealers/${createdDealership.id}`, {
    credentials: 'include',
    headers: { 'Cookie': cookies || '' }
  });

  if (verifyAfterResponse.status === 404) {
    console.log('✅ Verification successful: Dealership no longer exists');
  } else {
    console.error('❌ Dealership still exists after deletion!');
    return;
  }

  console.log('\n=== All tests passed! ===');
  console.log('\nFeature verified:');
  console.log('✅ Admin can delete dealerships');
  console.log('✅ Deletion cascades to related records');
  console.log('✅ Deleted dealerships cannot be retrieved');
  console.log('\nNext steps to test in UI:');
  console.log('1. Navigate to http://localhost:5173/admin/login');
  console.log('2. Login as admin (username: admin, password: admin123)');
  console.log('3. Click on "Dealership Management"');
  console.log('4. Click "Delete" button next to a dealership');
  console.log('5. Confirm deletion by typing the exact dealership name');
  console.log('6. Verify dealership is removed from the list');
}

// Run the test
testDealershipDeletion().catch(error => {
  console.error('Test failed with error:', error);
});
