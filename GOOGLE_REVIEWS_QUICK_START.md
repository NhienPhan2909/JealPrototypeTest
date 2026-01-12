# Google Reviews Quick Start Guide

## What This Feature Does
Displays customer reviews from Google Maps on your dealership homepage in an attractive carousel format.

## Quick Setup (5 minutes)

### Step 1: Get Google API Key
1. Go to https://console.cloud.google.com/
2. Create a project (or use existing)
3. Enable "Places API"
4. Create credentials → API Key
5. Copy the API key

### Step 2: Add to Environment
Add this line to your `.env` file:
```
GOOGLE_PLACES_API_KEY=paste_your_api_key_here
```

### Step 3: Restart Server
```bash
cd backend
npm run dev
```

### Step 4: Test
1. Visit http://localhost:3000
2. Scroll down below the search widget
3. You should see "Customer Reviews" carousel

## That's It! ✅

The carousel will:
- ✅ Automatically find your dealership on Google Maps
- ✅ Display 3-4 top reviews at a time
- ✅ Show star ratings and reviewer photos
- ✅ Include "Read More" button to Google Reviews
- ✅ Work on mobile and desktop

## Troubleshooting

**No reviews showing?**
- Check API key is correct in `.env`
- Verify Places API is enabled in Google Cloud
- Make sure dealership address is accurate in database
- Check backend console for error messages

**Wrong location found?**
- Update dealership address to be more specific
- Include city, state in address field

## Cost
- Google Places API: Free tier includes 28,000+ requests/month
- Typical usage: ~1 request per homepage visit
- Monitor usage at: https://console.cloud.google.com/

## Need Help?
See `GOOGLE_REVIEWS_FEATURE.md` for detailed documentation.
