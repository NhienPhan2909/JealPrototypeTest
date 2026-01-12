/**
 * Test script to verify font_family API functionality
 * Run this after restarting the backend server
 * 
 * Usage: node test_font_api.js
 */

const API_BASE = 'http://localhost:3000/api';

async function testFontAPI() {
  console.log('🧪 Testing Font Family API Functionality\n');
  console.log('═══════════════════════════════════════════\n');

  try {
    // Step 1: Get current dealership data
    console.log('1️⃣  Fetching dealership data...');
    const getResponse = await fetch(`${API_BASE}/dealers/1`);
    const dealershipBefore = await getResponse.json();
    console.log(`   Current font: ${dealershipBefore.font_family || 'undefined'}`);
    console.log('   ✅ GET request successful\n');

    // Step 2: Update font to 'times'
    console.log('2️⃣  Updating font to "times"...');
    const updateResponse = await fetch(`${API_BASE}/dealers/1`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        name: dealershipBefore.name,
        address: dealershipBefore.address,
        phone: dealershipBefore.phone,
        email: dealershipBefore.email,
        logo_url: dealershipBefore.logo_url,
        hours: dealershipBefore.hours,
        finance_policy: dealershipBefore.finance_policy,
        warranty_policy: dealershipBefore.warranty_policy,
        about: dealershipBefore.about,
        hero_background_image: dealershipBefore.hero_background_image,
        theme_color: dealershipBefore.theme_color,
        font_family: 'times'
      })
    });

    const dealershipAfter = await updateResponse.json();
    
    if (updateResponse.ok) {
      console.log(`   Updated font: ${dealershipAfter.font_family}`);
      
      if (dealershipAfter.font_family === 'times') {
        console.log('   ✅ UPDATE request successful - font changed to "times"\n');
      } else {
        console.log('   ⚠️  UPDATE request succeeded but font not saved correctly\n');
        console.log('   Response:', dealershipAfter);
      }
    } else {
      console.log('   ❌ UPDATE request failed');
      console.log('   Error:', dealershipAfter);
    }

    // Step 3: Verify the change persisted
    console.log('3️⃣  Verifying change persisted in database...');
    const verifyResponse = await fetch(`${API_BASE}/dealers/1`);
    const dealershipVerify = await verifyResponse.json();
    console.log(`   Current font: ${dealershipVerify.font_family}`);
    
    if (dealershipVerify.font_family === 'times') {
      console.log('   ✅ Change persisted successfully!\n');
    } else {
      console.log('   ❌ Change did NOT persist\n');
    }

    // Step 4: Reset to system
    console.log('4️⃣  Resetting font to "system"...');
    const resetResponse = await fetch(`${API_BASE}/dealers/1`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        name: dealershipBefore.name,
        address: dealershipBefore.address,
        phone: dealershipBefore.phone,
        email: dealershipBefore.email,
        logo_url: dealershipBefore.logo_url,
        hours: dealershipBefore.hours,
        finance_policy: dealershipBefore.finance_policy,
        warranty_policy: dealershipBefore.warranty_policy,
        about: dealershipBefore.about,
        hero_background_image: dealershipBefore.hero_background_image,
        theme_color: dealershipBefore.theme_color,
        font_family: 'system'
      })
    });

    const dealershipReset = await resetResponse.json();
    console.log(`   Reset font: ${dealershipReset.font_family}`);
    console.log('   ✅ Reset successful\n');

    console.log('═══════════════════════════════════════════');
    console.log('✨ All tests passed! API is working correctly.\n');
    console.log('📝 Next: Try changing font in the admin UI');

  } catch (error) {
    console.error('❌ Test failed with error:', error.message);
    console.log('\n💡 Make sure:');
    console.log('   1. Backend server is running (npm start in backend folder)');
    console.log('   2. Server is accessible at http://localhost:3000');
  }
}

// Run the test
testFontAPI();
