/**
 * Test script for Sales Request Email Notification
 * 
 * This script tests the email notification feature for new "sell your car" requests.
 * 
 * Prerequisites:
 * - Backend server must be running (npm start in backend directory)
 * - Email configuration in .env file (EMAIL_HOST, EMAIL_USER, EMAIL_PASSWORD, EMAIL_FROM)
 * - At least one dealership in the database
 * 
 * Usage:
 *   node test_sales_request_email.js
 */

const axios = require('axios');

// Configuration
const API_BASE_URL = 'http://localhost:3000/api';
const DEALERSHIP_ID = 1; // Change this to match your test dealership ID

// Test data
const testSalesRequest = {
  dealership_id: DEALERSHIP_ID,
  name: 'John Test Seller',
  email: 'john.seller@example.com',
  phone: '(555) 987-6543',
  make: 'Toyota',
  model: 'Camry',
  year: 2018,
  kilometers: 85000,
  additional_message: 'Vehicle is in excellent condition. Regular maintenance performed. Non-smoker owner. Clean interior and exterior.'
};

async function testSalesRequestEmail() {
  console.log('🧪 Testing Sales Request Email Notification\n');
  console.log('Test Data:');
  console.log(JSON.stringify(testSalesRequest, null, 2));
  console.log('\n---\n');

  try {
    console.log('📤 Submitting sales request...');
    const response = await axios.post(`${API_BASE_URL}/sales-requests`, testSalesRequest);

    if (response.status === 201) {
      console.log('✅ Sales request created successfully!');
      console.log('Response:', JSON.stringify(response.data, null, 2));
      console.log('\n---\n');
      console.log('📧 Email notification should be sent to the dealership email.');
      console.log('   Check the backend console logs for email sending confirmation.');
      console.log('   Check the dealership inbox for the notification email.');
      console.log('\n✨ Test completed successfully!');
    } else {
      console.log('❌ Unexpected response status:', response.status);
    }
  } catch (error) {
    console.error('❌ Error during test:');
    if (error.response) {
      console.error('Status:', error.response.status);
      console.error('Data:', error.response.data);
    } else {
      console.error(error.message);
    }
    console.log('\n💡 Troubleshooting:');
    console.log('1. Make sure the backend server is running (npm start in backend directory)');
    console.log('2. Verify dealership_id exists in database');
    console.log('3. Check email configuration in .env file');
    console.log('4. Review backend console logs for more details');
  }
}

// Run the test
testSalesRequestEmail();
