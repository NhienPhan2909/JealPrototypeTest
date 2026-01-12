/**
 * Test script to verify Location page URL generation
 * Run with: node test_location_url.js
 */

// Simulate the address from database
const dealershipAddress = '123 Main St, Springfield, IL 62701';

// Clean address (remove hidden Unicode characters)
const cleanAddress = dealershipAddress.trim().replace(/[\u200B-\u200D\uFEFF]/g, '');

// Encode for URL
const encodedAddress = encodeURIComponent(cleanAddress);

// Generate URLs
const mapUrl = `https://maps.google.com/maps?q=${encodedAddress}&t=&z=16&ie=UTF8&iwloc=&output=embed`;
const directionsUrl = `https://www.google.com/maps/dir/?api=1&destination=${encodedAddress}`;

console.log('=== Location Map URL Generation Test ===\n');
console.log('Original address from DB:', dealershipAddress);
console.log('Clean address:', cleanAddress);
console.log('Encoded address:', encodedAddress);
console.log('\n=== Generated URLs ===\n');
console.log('Map Embed URL:');
console.log(mapUrl);
console.log('\nGet Directions URL:');
console.log(directionsUrl);
console.log('\n=== Verification ===\n');
console.log('✓ Address length:', cleanAddress.length, 'characters');
console.log('✓ Contains "Jtsecurity":', cleanAddress.includes('Jtsecurity') ? 'YES (ERROR!)' : 'NO (GOOD)');
console.log('✓ URL is valid:', mapUrl.startsWith('https://') ? 'YES' : 'NO');
console.log('\n=== Copy/Paste Test ===\n');
console.log('Copy this URL and paste in browser to test map:');
console.log(mapUrl.replace('&output=embed', ''));
console.log('\nCopy this URL to test directions:');
console.log(directionsUrl);
