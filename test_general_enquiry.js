/**
 * Test script for general enquiry form submission
 * Tests the POST /api/leads endpoint with general enquiry data (no vehicle_id)
 */

const testGeneralEnquiry = async () => {
  try {
    console.log('Testing general enquiry submission...\n');

    const enquiryData = {
      dealership_id: 1,
      name: 'John Doe',
      email: 'john.doe@example.com',
      phone: '(555) 123-4567',
      message: 'Hi, I am interested in learning more about your dealership and the vehicles you have available. Could you please contact me at your earliest convenience?'
    };

    console.log('Sending enquiry:', JSON.stringify(enquiryData, null, 2));

    const response = await fetch('http://localhost:5000/api/leads', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(enquiryData)
    });

    const result = await response.json();

    if (response.ok) {
      console.log('\n✅ SUCCESS! Enquiry submitted successfully');
      console.log('Response:', JSON.stringify(result, null, 2));
      console.log('\nNote: vehicle_id should be null for general enquiries');
    } else {
      console.error('\n❌ FAILED! Error submitting enquiry');
      console.error('Status:', response.status);
      console.error('Error:', result);
    }
  } catch (error) {
    console.error('\n❌ FAILED! Exception occurred');
    console.error('Error:', error.message);
  }
};

testGeneralEnquiry();
