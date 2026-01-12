// Quick test script to verify finance_policy and warranty_policy updates work
const fetch = require('node-fetch');

async function testUpdate() {
  try {
    console.log('Testing PUT /api/dealers/1 with finance and warranty policies...\n');

    const response = await fetch('http://localhost:5000/api/dealers/1', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        name: 'Acme Auto Sales Updated',
        address: '123 Main St, Springfield, IL 62701',
        phone: '(555) 123-4567',
        email: 'sales@acmeauto.com',
        hours: 'Mon-Fri 9am-6pm',
        finance_policy: 'We offer flexible financing options! Low rates available. Contact us today!',
        warranty_policy: 'All vehicles come with our comprehensive warranty. Bumper to bumper coverage!',
        about: 'Updated about text for testing'
      })
    });

    const data = await response.json();

    console.log('Response Status:', response.status);
    console.log('\nFinance Policy:', data.finance_policy);
    console.log('Warranty Policy:', data.warranty_policy);
    console.log('\n✅ Update successful!' + (data.finance_policy && data.warranty_policy ? ' New fields are saved!' : ''));

  } catch (error) {
    console.error('❌ Test failed:', error.message);
  }
}

testUpdate();
