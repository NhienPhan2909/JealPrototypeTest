/**
 * Test script to verify URL parameter routing for dealerships
 * 
 * Usage:
 * 1. Start frontend dev server: npm run dev (in frontend directory)
 * 2. Open browser console
 * 3. Copy and paste this script
 * 4. It will test URL parameter routing
 */

console.log('🧪 Testing URL Parameter Routing...\n');

// Test 1: Check current URL and dealership
const urlParams = new URLSearchParams(window.location.search);
const urlDealershipId = urlParams.get('dealership');
console.log('Test 1: Current URL parameter');
console.log('  URL:', window.location.href);
console.log('  Dealership param:', urlDealershipId || 'Not set');

// Test 2: Check localStorage
const storedId = localStorage.getItem('selectedDealershipId');
console.log('\nTest 2: LocalStorage value');
console.log('  Stored dealership ID:', storedId || 'Not set');

// Test 3: Check React context (if available)
try {
  const dealershipContext = document.querySelector('[data-dealership-id]');
  if (dealershipContext) {
    console.log('\nTest 3: React context value');
    console.log('  Current dealership ID:', dealershipContext.dataset.dealershipId);
  }
} catch (e) {
  console.log('\nTest 3: React context not accessible from console');
}

// Test 4: Navigate to different dealership URLs
console.log('\n📝 Manual test instructions:');
console.log('  1. Visit: http://localhost:3000?dealership=1');
console.log('     Expected: Acme Auto Sales');
console.log('  2. Visit: http://localhost:3000?dealership=2');
console.log('     Expected: Premier Motors');
console.log('  3. Visit: http://localhost:3000 (no parameter)');
console.log('     Expected: Last selected or Acme Auto Sales (default)');

// Test 5: Check what's actually displayed
console.log('\n🔍 Checking current page content...');
setTimeout(() => {
  // Try to find dealership name in the page
  const bodyText = document.body.innerText;
  if (bodyText.includes('Acme Auto Sales')) {
    console.log('  ✅ Currently showing: Acme Auto Sales');
  } else if (bodyText.includes('Premier Motors')) {
    console.log('  ✅ Currently showing: Premier Motors');
  } else {
    console.log('  ⚠️  Could not determine which dealership is displayed');
  }
}, 1000);

console.log('\n✨ Test complete!\n');
