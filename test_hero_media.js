/**
 * Test script to verify hero media fields are properly saved
 * 
 * Run this after fixing backend and restarting server:
 * node test_hero_media.js
 */

const testHeroMedia = async () => {
  const DEALERSHIP_ID = 1;
  const API_URL = 'http://localhost:5000';

  console.log('🧪 Testing Hero Media API...\n');

  // Test 1: Update to carousel type with images
  console.log('Test 1: Setting hero to carousel type with test images...');
  try {
    const updateResponse = await fetch(`${API_URL}/api/dealers/${DEALERSHIP_ID}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        name: 'Acme Auto Sales',
        address: '123 Main St',
        phone: '555-0100',
        email: 'info@acme.com',
        hero_type: 'carousel',
        hero_carousel_images: [
          'https://res.cloudinary.com/demo/image/upload/sample1.jpg',
          'https://res.cloudinary.com/demo/image/upload/sample2.jpg',
          'https://res.cloudinary.com/demo/image/upload/sample3.jpg'
        ]
      })
    });

    if (updateResponse.ok) {
      const data = await updateResponse.json();
      console.log('✅ Update successful!');
      console.log('   hero_type:', data.hero_type);
      console.log('   carousel_images count:', data.hero_carousel_images?.length || 0);
    } else {
      const error = await updateResponse.json();
      console.log('❌ Update failed:', error);
    }
  } catch (err) {
    console.log('❌ Error:', err.message);
  }

  console.log('\n');

  // Test 2: Fetch and verify
  console.log('Test 2: Fetching dealership to verify saved data...');
  try {
    const getResponse = await fetch(`${API_URL}/api/dealers/${DEALERSHIP_ID}`);
    
    if (getResponse.ok) {
      const data = await getResponse.json();
      console.log('✅ Fetch successful!');
      console.log('   hero_type:', data.hero_type);
      console.log('   hero_video_url:', data.hero_video_url || 'null');
      console.log('   hero_carousel_images:', JSON.stringify(data.hero_carousel_images, null, 2));
      
      // Validation
      if (data.hero_type === 'carousel' && Array.isArray(data.hero_carousel_images) && data.hero_carousel_images.length === 3) {
        console.log('\n✅ All tests passed! Hero media is working correctly.');
      } else {
        console.log('\n❌ Test failed: Data not saved correctly');
        console.log('   Expected: hero_type="carousel", 3 images');
        console.log('   Got:', { hero_type: data.hero_type, image_count: data.hero_carousel_images?.length });
      }
    } else {
      console.log('❌ Fetch failed');
    }
  } catch (err) {
    console.log('❌ Error:', err.message);
  }

  console.log('\n');

  // Test 3: Update to video type
  console.log('Test 3: Setting hero to video type...');
  try {
    const updateResponse = await fetch(`${API_URL}/api/dealers/${DEALERSHIP_ID}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        name: 'Acme Auto Sales',
        address: '123 Main St',
        phone: '555-0100',
        email: 'info@acme.com',
        hero_type: 'video',
        hero_video_url: 'https://res.cloudinary.com/demo/video/upload/sample.mp4'
      })
    });

    if (updateResponse.ok) {
      const data = await updateResponse.json();
      console.log('✅ Update successful!');
      console.log('   hero_type:', data.hero_type);
      console.log('   hero_video_url:', data.hero_video_url);
    } else {
      const error = await updateResponse.json();
      console.log('❌ Update failed:', error);
    }
  } catch (err) {
    console.log('❌ Error:', err.message);
  }

  console.log('\n🏁 Test complete!\n');
};

// Run tests
testHeroMedia().catch(console.error);
