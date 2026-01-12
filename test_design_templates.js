/**
 * Test script for Design Templates API
 * 
 * This script tests the design templates feature including:
 * - Fetching all templates (presets + custom)
 * - Creating custom templates
 * - Deleting custom templates
 * - Validation and error handling
 * 
 * Prerequisites:
 * - Backend server running (npm run dev in backend folder)
 * - Database with design_templates table and seed data
 * - Valid admin user credentials
 * 
 * Usage:
 * node test_design_templates.js
 */

const BASE_URL = 'http://localhost:5000';

// Test user credentials (update with valid admin user)
const TEST_USER = {
  username: 'admin',
  password: 'Admin123!'
};

let sessionCookie = '';

/**
 * Helper function to make authenticated API calls
 */
async function apiCall(endpoint, options = {}) {
  const url = `${BASE_URL}${endpoint}`;
  const headers = {
    'Content-Type': 'application/json',
    ...(sessionCookie ? { 'Cookie': sessionCookie } : {}),
    ...options.headers
  };

  const response = await fetch(url, {
    ...options,
    headers,
    credentials: 'include'
  });

  // Store session cookie from login
  if (response.headers.get('set-cookie')) {
    sessionCookie = response.headers.get('set-cookie').split(';')[0];
  }

  const data = await response.json();
  return { status: response.status, data };
}

/**
 * Test 1: User Login
 */
async function testLogin() {
  console.log('\n🔐 Test 1: User Login');
  console.log('═══════════════════════════════════════');

  try {
    const result = await apiCall('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(TEST_USER)
    });

    if (result.status === 200) {
      console.log('✅ Login successful');
      console.log(`   User: ${result.data.user.username}`);
      console.log(`   Dealership ID: ${result.data.user.dealership_id}`);
      return true;
    } else {
      console.log('❌ Login failed:', result.data.error);
      return false;
    }
  } catch (error) {
    console.log('❌ Error during login:', error.message);
    return false;
  }
}

/**
 * Test 2: Fetch All Templates
 */
async function testFetchTemplates() {
  console.log('\n📋 Test 2: Fetch All Templates');
  console.log('═══════════════════════════════════════');

  try {
    const result = await apiCall('/api/design-templates', {
      method: 'GET'
    });

    if (result.status === 200) {
      const templates = result.data;
      const presets = templates.filter(t => t.is_preset);
      const custom = templates.filter(t => !t.is_preset);

      console.log('✅ Templates fetched successfully');
      console.log(`   Total templates: ${templates.length}`);
      console.log(`   Pre-set templates: ${presets.length}`);
      console.log(`   Custom templates: ${custom.length}`);

      console.log('\n   Pre-set Templates:');
      presets.forEach(t => {
        console.log(`   - ${t.name}: ${t.theme_color} / ${t.font_family}`);
      });

      if (custom.length > 0) {
        console.log('\n   Custom Templates:');
        custom.forEach(t => {
          console.log(`   - ${t.name}: ${t.theme_color} / ${t.font_family}`);
        });
      }

      return templates;
    } else {
      console.log('❌ Failed to fetch templates:', result.data.error);
      return null;
    }
  } catch (error) {
    console.log('❌ Error fetching templates:', error.message);
    return null;
  }
}

/**
 * Test 3: Create Custom Template
 */
async function testCreateTemplate() {
  console.log('\n➕ Test 3: Create Custom Template');
  console.log('═══════════════════════════════════════');

  const newTemplate = {
    name: 'Test Template ' + Date.now(),
    description: 'Automated test template',
    theme_color: '#FF5733',
    secondary_theme_color: '#C70039',
    body_background_color: '#FFC300',
    font_family: 'roboto'
  };

  try {
    const result = await apiCall('/api/design-templates', {
      method: 'POST',
      body: JSON.stringify(newTemplate)
    });

    if (result.status === 201) {
      console.log('✅ Template created successfully');
      console.log(`   ID: ${result.data.id}`);
      console.log(`   Name: ${result.data.name}`);
      console.log(`   Colors: ${result.data.theme_color}, ${result.data.secondary_theme_color}, ${result.data.body_background_color}`);
      console.log(`   Font: ${result.data.font_family}`);
      return result.data;
    } else {
      console.log('❌ Failed to create template:', result.data.error);
      return null;
    }
  } catch (error) {
    console.log('❌ Error creating template:', error.message);
    return null;
  }
}

/**
 * Test 4: Create Duplicate Template (Should Fail)
 */
async function testCreateDuplicate(templateName) {
  console.log('\n🔁 Test 4: Create Duplicate Template (Expected to Fail)');
  console.log('═══════════════════════════════════════');

  const duplicateTemplate = {
    name: templateName,
    description: 'Duplicate test',
    theme_color: '#000000',
    secondary_theme_color: '#FFFFFF',
    body_background_color: '#FFFFFF',
    font_family: 'arial'
  };

  try {
    const result = await apiCall('/api/design-templates', {
      method: 'POST',
      body: JSON.stringify(duplicateTemplate)
    });

    if (result.status === 400) {
      console.log('✅ Duplicate validation working correctly');
      console.log(`   Error message: ${result.data.error}`);
      return true;
    } else {
      console.log('❌ Duplicate template was created (should have failed)');
      return false;
    }
  } catch (error) {
    console.log('❌ Error during duplicate test:', error.message);
    return false;
  }
}

/**
 * Test 5: Create Template with Invalid Data (Should Fail)
 */
async function testCreateInvalid() {
  console.log('\n⚠️  Test 5: Create Template with Invalid Data (Expected to Fail)');
  console.log('═══════════════════════════════════════');

  const invalidTemplate = {
    name: 'Invalid Template',
    description: 'Test with invalid color',
    theme_color: 'not-a-color', // Invalid hex color
    secondary_theme_color: '#FFFFFF',
    body_background_color: '#FFFFFF',
    font_family: 'roboto'
  };

  try {
    const result = await apiCall('/api/design-templates', {
      method: 'POST',
      body: JSON.stringify(invalidTemplate)
    });

    if (result.status === 400) {
      console.log('✅ Color validation working correctly');
      console.log(`   Error message: ${result.data.error}`);
      return true;
    } else {
      console.log('❌ Invalid template was created (should have failed)');
      return false;
    }
  } catch (error) {
    console.log('❌ Error during validation test:', error.message);
    return false;
  }
}

/**
 * Test 6: Delete Custom Template
 */
async function testDeleteTemplate(templateId) {
  console.log('\n🗑️  Test 6: Delete Custom Template');
  console.log('═══════════════════════════════════════');

  try {
    const result = await apiCall(`/api/design-templates/${templateId}`, {
      method: 'DELETE'
    });

    if (result.status === 200) {
      console.log('✅ Template deleted successfully');
      console.log(`   Message: ${result.data.message}`);
      return true;
    } else {
      console.log('❌ Failed to delete template:', result.data.error);
      return false;
    }
  } catch (error) {
    console.log('❌ Error deleting template:', error.message);
    return false;
  }
}

/**
 * Test 7: Try to Delete Preset Template (Should Fail)
 */
async function testDeletePreset() {
  console.log('\n🚫 Test 7: Try to Delete Pre-set Template (Expected to Fail)');
  console.log('═══════════════════════════════════════');

  // Try to delete template ID 1 (Modern Blue preset)
  try {
    const result = await apiCall('/api/design-templates/1', {
      method: 'DELETE'
    });

    if (result.status === 404) {
      console.log('✅ Preset deletion prevention working correctly');
      console.log(`   Error message: ${result.data.error}`);
      return true;
    } else {
      console.log('❌ Preset template was deleted (should have failed)');
      return false;
    }
  } catch (error) {
    console.log('❌ Error during preset deletion test:', error.message);
    return false;
  }
}

/**
 * Main test runner
 */
async function runTests() {
  console.log('\n╔══════════════════════════════════════════════════════╗');
  console.log('║   Design Templates API Test Suite                   ║');
  console.log('╚══════════════════════════════════════════════════════╝');

  let testsRun = 0;
  let testsPassed = 0;

  // Test 1: Login
  testsRun++;
  const loginSuccess = await testLogin();
  if (loginSuccess) testsPassed++;
  else {
    console.log('\n❌ Login failed. Cannot continue tests.');
    return;
  }

  // Test 2: Fetch templates
  testsRun++;
  const templates = await testFetchTemplates();
  if (templates) testsPassed++;

  // Test 3: Create template
  testsRun++;
  const newTemplate = await testCreateTemplate();
  if (newTemplate) testsPassed++;

  // Test 4: Create duplicate
  if (newTemplate) {
    testsRun++;
    const duplicateTest = await testCreateDuplicate(newTemplate.name);
    if (duplicateTest) testsPassed++;
  }

  // Test 5: Invalid data
  testsRun++;
  const invalidTest = await testCreateInvalid();
  if (invalidTest) testsPassed++;

  // Test 6: Delete custom template
  if (newTemplate) {
    testsRun++;
    const deleteTest = await testDeleteTemplate(newTemplate.id);
    if (deleteTest) testsPassed++;
  }

  // Test 7: Try to delete preset
  testsRun++;
  const presetDeleteTest = await testDeletePreset();
  if (presetDeleteTest) testsPassed++;

  // Summary
  console.log('\n╔══════════════════════════════════════════════════════╗');
  console.log('║   Test Summary                                       ║');
  console.log('╚══════════════════════════════════════════════════════╝');
  console.log(`\n   Tests Run: ${testsRun}`);
  console.log(`   Tests Passed: ${testsPassed}`);
  console.log(`   Tests Failed: ${testsRun - testsPassed}`);
  console.log(`   Success Rate: ${Math.round((testsPassed / testsRun) * 100)}%`);

  if (testsPassed === testsRun) {
    console.log('\n   ✅ All tests passed!\n');
  } else {
    console.log('\n   ❌ Some tests failed. Please review the output above.\n');
  }
}

// Run the tests
runTests().catch(error => {
  console.error('Fatal error:', error);
  process.exit(1);
});
