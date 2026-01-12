# Google Reviews Carousel Feature

## Overview
This feature adds a Google Reviews carousel to each dealership website homepage, displaying top-rated customer reviews from Google Maps/Google Business Profile.

## Location
The carousel appears below the "Find Your Perfect Vehicle" widget and "General Enquiry" form on the homepage.

## Features
- **Carousel Display**: Shows 3-4 top-rated reviews at a time
- **Navigation**: Arrow buttons to scroll through reviews
- **Pagination Dots**: Visual indicators for carousel position
- **Star Ratings**: Visual star rating display (1-5 stars)
- **Author Information**: Shows reviewer name, photo, and review date
- **Read More Button**: Links directly to the dealership's Google Reviews page
- **Automatic Search**: Uses dealership address to find Google Business listing
- **Responsive Design**: Works on mobile, tablet, and desktop

## Setup Instructions

### 1. Get Google Places API Key

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the following APIs:
   - **Places API**
   - **Places API (New)** (recommended)
4. Create API credentials:
   - Navigate to "Credentials" in the sidebar
   - Click "Create Credentials" → "API Key"
   - Copy the generated API key
   - **Important**: Restrict the API key to your domain/IP for security

### 2. Configure Environment Variable

Add the following to your `.env` file in the project root:

```env
GOOGLE_PLACES_API_KEY=your_api_key_here
```

Replace `your_api_key_here` with your actual Google Places API key.

### 3. Restart the Backend Server

```bash
cd backend
npm run dev
```

The backend will automatically load the API key from the `.env` file.

## How It Works

### Backend Process
1. Frontend requests reviews for a specific dealership ID
2. Backend fetches dealership address from database
3. Backend searches Google Places API using dealership name + address
4. Backend retrieves place details and reviews
5. Backend filters for 4-5 star reviews only
6. Backend returns top 10 reviews sorted by rating

### Frontend Display
1. Component fetches reviews from backend API
2. Displays 3 reviews per page in a carousel
3. Users can navigate using arrow buttons
4. "Read More" button opens Google Maps listing in new tab

## API Endpoint

**GET** `/api/google-reviews/:dealershipId`

**Response:**
```json
{
  "reviews": [
    {
      "author_name": "John Doe",
      "rating": 5,
      "text": "Great service and excellent vehicles!",
      "time": 1640995200,
      "relative_time_description": "2 months ago",
      "profile_photo_url": "https://..."
    }
  ],
  "googleMapsUrl": "https://maps.google.com/...",
  "totalRatings": 156,
  "averageRating": 4.8
}
```

## Files Created/Modified

### New Files
- `frontend/src/components/GoogleReviewsCarousel.jsx` - Carousel component
- `backend/routes/googleReviews.js` - API route handler

### Modified Files
- `frontend/src/pages/public/Home.jsx` - Added carousel to homepage
- `backend/server.js` - Registered Google Reviews route

## Customization Options

### Change Number of Reviews Per Page
Edit `GoogleReviewsCarousel.jsx`:
```javascript
const reviewsPerPage = 3; // Change to 4 or any number
```

### Change Minimum Rating Filter
Edit `backend/routes/googleReviews.js`:
```javascript
.filter(review => review.rating >= 4) // Change to 5 for only 5-star reviews
```

### Change Maximum Reviews Returned
Edit `backend/routes/googleReviews.js`:
```javascript
.slice(0, 10); // Change to 15, 20, etc.
```

## Styling

The component uses:
- **Theme Colors**: Integrates with dealership theme colors (`var(--theme-color)`)
- **Tailwind CSS**: Responsive design with utility classes
- **Star Icons**: Unicode star characters (★) with yellow/gray colors
- **Google Branding**: Official Google logo displayed

## Error Handling

The carousel gracefully handles errors:
- **No API Key**: Shows nothing (silent failure)
- **Location Not Found**: Shows nothing (silent failure)
- **No Reviews**: Shows nothing (silent failure)
- **API Error**: Shows nothing (silent failure)

This ensures the homepage never breaks even if Google Reviews fails to load.

## Performance Considerations

- Reviews are fetched on page load (one API call per homepage visit)
- Google Places API has usage limits and costs
- Consider implementing caching in the future to reduce API calls

## Future Enhancements

Potential improvements:
1. **Cache Reviews**: Store reviews in database, refresh periodically
2. **Auto-Refresh**: Background job to update reviews nightly
3. **Review Moderation**: Filter out negative reviews or specific keywords
4. **Schema Markup**: Add structured data for SEO benefits
5. **Load More**: Pagination for more than 10 reviews
6. **Average Rating Display**: Show overall rating with star count

## Troubleshooting

### Reviews Not Showing
1. Check if `GOOGLE_PLACES_API_KEY` is set in `.env`
2. Verify API key has Places API enabled
3. Check browser console for errors
4. Verify dealership has a Google Business listing
5. Check dealership address is accurate in database

### Wrong Location Found
- Update dealership address in database to be more specific
- Include city and state in address field
- Ensure dealership name matches Google Business listing

## Security Notes

- API key should be kept secret (server-side only)
- Restrict API key to your server's IP address
- Monitor API usage in Google Cloud Console
- Set up billing alerts to avoid unexpected charges

## Testing

Test the feature:
1. Visit homepage: `http://localhost:3000`
2. Scroll below the search widget and enquiry form
3. Verify carousel appears with reviews
4. Test navigation arrows
5. Click "Read More" button to verify Google Maps link

## Support

For issues or questions:
- Check backend logs for API errors
- Verify Google Cloud Console for API quota/usage
- Test API key with direct Google Places API calls
