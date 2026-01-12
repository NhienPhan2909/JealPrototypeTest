/**
 * Test script for Google Reviews API endpoint
 * 
 * Usage: node test_google_reviews.js
 * 
 * Prerequisites:
 * 1. Backend server must be running (npm run dev)
 * 2. GOOGLE_PLACES_API_KEY must be set in .env
 * 3. Dealership with ID 1 must exist in database
 */

const BACKEND_URL = 'http://localhost:5000';
const DEALERSHIP_ID = 1;

async function testGoogleReviews() {
  console.log('🧪 Testing Google Reviews API...\n');

  try {
    console.log(`📡 Fetching reviews for dealership ID: ${DEALERSHIP_ID}`);
    console.log(`   Endpoint: ${BACKEND_URL}/api/google-reviews/${DEALERSHIP_ID}\n`);

    const response = await fetch(`${BACKEND_URL}/api/google-reviews/${DEALERSHIP_ID}`);
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    const data = await response.json();

    console.log('✅ API Response received!\n');
    console.log('📊 Results:');
    console.log('─────────────────────────────────────────────────');
    console.log(`Total Reviews Returned: ${data.reviews?.length || 0}`);
    console.log(`Average Rating: ${data.averageRating || 'N/A'}`);
    console.log(`Total Ratings: ${data.totalRatings || 'N/A'}`);
    console.log(`Google Maps URL: ${data.googleMapsUrl ? '✓ Available' : '✗ Not found'}`);
    
    if (data.message) {
      console.log(`Message: ${data.message}`);
    }

    console.log('─────────────────────────────────────────────────\n');

    if (data.reviews && data.reviews.length > 0) {
      console.log('📝 Sample Reviews:\n');
      data.reviews.slice(0, 3).forEach((review, index) => {
        console.log(`Review ${index + 1}:`);
        console.log(`  Author: ${review.author_name}`);
        console.log(`  Rating: ${'⭐'.repeat(review.rating)} (${review.rating}/5)`);
        console.log(`  Date: ${review.relative_time_description}`);
        console.log(`  Text: ${review.text.substring(0, 100)}${review.text.length > 100 ? '...' : ''}`);
        console.log('');
      });

      console.log('✅ Test PASSED - Reviews loaded successfully!');
      console.log('\n💡 Next steps:');
      console.log('   1. Visit http://localhost:3000 to see the carousel');
      console.log('   2. Scroll down below the search widget');
      console.log('   3. Reviews should appear automatically\n');
    } else {
      console.log('⚠️  No reviews found');
      console.log('\nPossible reasons:');
      console.log('   • Google Places API key not configured');
      console.log('   • Dealership location not found on Google Maps');
      console.log('   • Dealership has no Google reviews');
      console.log('   • Dealership address in database is incorrect\n');
      console.log('💡 Check backend console for more details\n');
    }

  } catch (error) {
    console.log('❌ Test FAILED\n');
    console.log('Error Details:');
    console.log('─────────────────────────────────────────────────');
    console.log(error.message);
    console.log('─────────────────────────────────────────────────\n');
    console.log('Troubleshooting:');
    console.log('   1. Is the backend server running? (npm run dev)');
    console.log('   2. Is GOOGLE_PLACES_API_KEY set in .env?');
    console.log('   3. Does dealership with ID 1 exist in database?');
    console.log('   4. Check backend logs for errors\n');
  }
}

// Run test
testGoogleReviews();
