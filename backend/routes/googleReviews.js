/**
 * @fileoverview Google Reviews API routes.
 * Fetches Google Reviews for dealerships using Google Places API.
 * 
 * Routes:
 * - GET /api/google-reviews/:dealershipId - Get reviews for a dealership
 */

const express = require('express');
const router = express.Router();
const dealersDb = require('../db/dealers');

/**
 * Google Places API configuration
 */
const GOOGLE_PLACES_API_KEY = process.env.GOOGLE_PLACES_API_KEY || '';
const GOOGLE_PLACES_BASE_URL = 'https://maps.googleapis.com/maps/api/place';

/**
 * Finds a place using Google Places Text Search API.
 * 
 * @param {string} query - Search query (dealership name + address)
 * @returns {Promise<Object|null>} Place details including place_id, or null if not found
 */
async function findPlaceByAddress(query) {
  if (!GOOGLE_PLACES_API_KEY) {
    throw new Error('Google Places API key not configured');
  }

  const searchUrl = `${GOOGLE_PLACES_BASE_URL}/textsearch/json?query=${encodeURIComponent(query)}&key=${GOOGLE_PLACES_API_KEY}`;

  const response = await fetch(searchUrl);
  const data = await response.json();

  if (data.status === 'OK' && data.results && data.results.length > 0) {
    return data.results[0]; // Return first result
  }

  return null;
}

/**
 * Fetches place details including reviews from Google Places API.
 * 
 * @param {string} placeId - Google Place ID
 * @returns {Promise<Object>} Place details with reviews
 */
async function getPlaceDetails(placeId) {
  if (!GOOGLE_PLACES_API_KEY) {
    throw new Error('Google Places API key not configured');
  }

  const detailsUrl = `${GOOGLE_PLACES_BASE_URL}/details/json?place_id=${placeId}&fields=name,rating,reviews,url,user_ratings_total&key=${GOOGLE_PLACES_API_KEY}`;

  const response = await fetch(detailsUrl);
  const data = await response.json();

  if (data.status === 'OK' && data.result) {
    return data.result;
  }

  throw new Error(`Google Places API error: ${data.status}`);
}

/**
 * GET /api/google-reviews/:dealershipId
 * Fetches Google reviews for a specific dealership.
 * 
 * Process:
 * 1. Fetch dealership address from database
 * 2. Search for place using address
 * 3. Fetch place details and reviews
 * 4. Filter and return top-rated reviews
 * 
 * @param {number} req.params.dealershipId - Dealership ID
 * @returns {Object} Reviews data with Google Maps URL
 * @returns {Array} reviews - Array of top-rated reviews (max 10)
 * @returns {string} googleMapsUrl - URL to Google Maps listing
 * @throws {400} If dealership ID is invalid
 * @throws {404} If dealership not found
 * @throws {503} If Google Places API is not configured or fails
 */
router.get('/:dealershipId', async (req, res) => {
  try {
    // Validate dealership ID
    const dealershipId = parseInt(req.params.dealershipId, 10);
    if (isNaN(dealershipId) || dealershipId <= 0) {
      return res.status(400).json({ error: 'Invalid dealership ID' });
    }

    // Fetch dealership from database
    const dealership = await dealersDb.getById(dealershipId);
    if (!dealership) {
      return res.status(404).json({ error: 'Dealership not found' });
    }

    // Check if Google Places API is configured
    if (!GOOGLE_PLACES_API_KEY) {
      console.warn('Google Places API key not configured');
      return res.json({
        reviews: [],
        googleMapsUrl: '',
        message: 'Google Reviews not configured'
      });
    }

    // Build search query using dealership name and address
    const searchQuery = `${dealership.name} ${dealership.address}`;

    // Find place on Google
    const place = await findPlaceByAddress(searchQuery);
    if (!place) {
      console.warn(`No Google Place found for: ${searchQuery}`);
      return res.json({
        reviews: [],
        googleMapsUrl: '',
        message: 'Location not found on Google Maps'
      });
    }

    // Fetch place details with reviews
    const placeDetails = await getPlaceDetails(place.place_id);

    // Filter and sort reviews by rating (highest first)
    const reviews = (placeDetails.reviews || [])
      .filter(review => review.rating >= 4) // Only 4+ star reviews
      .sort((a, b) => b.rating - a.rating) // Sort by rating descending
      .slice(0, 10); // Limit to top 10 reviews

    // Return reviews and Google Maps URL
    res.json({
      reviews: reviews.map(review => ({
        author_name: review.author_name,
        rating: review.rating,
        text: review.text,
        time: review.time,
        relative_time_description: review.relative_time_description,
        profile_photo_url: review.profile_photo_url
      })),
      googleMapsUrl: placeDetails.url || '',
      totalRatings: placeDetails.user_ratings_total || 0,
      averageRating: placeDetails.rating || 0
    });

  } catch (error) {
    console.error('Error fetching Google reviews:', error);
    
    // Return empty reviews instead of error to prevent UI breakage
    res.json({
      reviews: [],
      googleMapsUrl: '',
      message: 'Unable to load reviews at this time'
    });
  }
});

module.exports = router;
