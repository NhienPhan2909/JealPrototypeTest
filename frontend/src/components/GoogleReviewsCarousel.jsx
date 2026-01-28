/**
 * @fileoverview Google Reviews Carousel component for homepage.
 * Displays top-rated Google reviews in a carousel format with navigation arrows.
 * Shows 3-4 reviews at a time with a "Read More" button linking to Google Reviews.
 */

import { useState, useEffect } from 'react';
import { useDealershipContext } from '../context/DealershipContext';
import apiRequest from '../utils/api';

/**
 * GoogleReviewsCarousel - Display Google reviews in a carousel.
 *
 * @component
 *
 * Fetches and displays top-rated Google reviews for the dealership.
 * Features:
 * - Shows 3-4 reviews at a time in a carousel format
 * - Navigation arrows to scroll through reviews
 * - Star ratings display
 * - "Read More" button linking to Google Reviews page
 * - Responsive design
 *
 * @example
 * <GoogleReviewsCarousel />
 */
function GoogleReviewsCarousel() {
  const { currentDealershipId } = useDealershipContext();
  const [reviews, setReviews] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [googleMapsUrl, setGoogleMapsUrl] = useState('');

  const reviewsPerPage = 3;

  useEffect(() => {
    fetchReviews();
  }, [currentDealershipId]);

  /**
   * Fetches Google reviews from backend API
   */
  const fetchReviews = async () => {
    try {
      setLoading(true);
      setError(null);

      const response = await apiRequest(`/api/google-reviews/${currentDealershipId}`);
      
      if (!response.ok) {
        throw new Error('Failed to fetch reviews');
      }

      const data = await response.json();
      setReviews(data.reviews || []);
      setGoogleMapsUrl(data.googleMapsUrl || '');
    } catch (err) {
      console.error('Error fetching Google reviews:', err);
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  /**
   * Renders star rating display
   * @param {number} rating - Rating value (1-5)
   * @returns {JSX.Element} Star rating component
   */
  const renderStars = (rating) => {
    const stars = [];
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 >= 0.5;

    for (let i = 0; i < 5; i++) {
      if (i < fullStars) {
        stars.push(
          <span key={i} className="text-yellow-400">★</span>
        );
      } else if (i === fullStars && hasHalfStar) {
        stars.push(
          <span key={i} className="text-yellow-400">⯨</span>
        );
      } else {
        stars.push(
          <span key={i} className="text-gray-300">★</span>
        );
      }
    }

    return <div className="flex text-xl">{stars}</div>;
  };

  /**
   * Navigate to previous reviews
   */
  const handlePrevious = () => {
    setCurrentIndex((prev) => 
      prev === 0 ? Math.max(0, reviews.length - reviewsPerPage) : Math.max(0, prev - reviewsPerPage)
    );
  };

  /**
   * Navigate to next reviews
   */
  const handleNext = () => {
    setCurrentIndex((prev) => 
      prev + reviewsPerPage >= reviews.length ? 0 : prev + reviewsPerPage
    );
  };

  if (loading) {
    return (
      <div className="bg-white rounded-lg shadow-md p-6">
        <h2 className="text-2xl font-bold mb-4 text-gray-900">Customer Reviews</h2>
        <div className="flex justify-center items-center py-12">
          <p className="text-gray-500">Loading reviews...</p>
        </div>
      </div>
    );
  }

  if (error || reviews.length === 0) {
    return null; // Don't show carousel if there's an error or no reviews
  }

  const visibleReviews = reviews.slice(currentIndex, currentIndex + reviewsPerPage);
  const showNavigation = reviews.length > reviewsPerPage;

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-2xl font-bold text-gray-900">Customer Reviews</h2>
        <img 
          src="https://www.gstatic.com/images/branding/googlelogo/2x/googlelogo_color_92x30dp.png" 
          alt="Google"
          className="h-6"
        />
      </div>

      <div className="relative">
        {/* Navigation Arrows */}
        {showNavigation && (
          <>
            <button
              onClick={handlePrevious}
              className="absolute left-0 top-1/2 -translate-y-1/2 -translate-x-4 bg-white rounded-full shadow-md p-2 hover:bg-gray-100 transition z-10"
              aria-label="Previous reviews"
            >
              <svg className="w-6 h-6 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
              </svg>
            </button>
            <button
              onClick={handleNext}
              className="absolute right-0 top-1/2 -translate-y-1/2 translate-x-4 bg-white rounded-full shadow-md p-2 hover:bg-gray-100 transition z-10"
              aria-label="Next reviews"
            >
              <svg className="w-6 h-6 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
              </svg>
            </button>
          </>
        )}

        {/* Reviews Grid */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
          {visibleReviews.map((review, index) => (
            <div key={index} className="border border-gray-200 rounded-lg p-4 flex flex-col">
              {/* Reviewer Name */}
              <div className="flex items-center mb-2">
                <div className="w-10 h-10 rounded-full mr-3 flex-shrink-0 bg-gray-200 flex items-center justify-center overflow-hidden">
                  {review.profilePhotoUrl ? (
                    <img 
                      src={review.profilePhotoUrl} 
                      alt={review.authorName}
                      className="w-full h-full object-cover"
                      referrerPolicy="no-referrer"
                      crossOrigin="anonymous"
                      onError={(e) => {
                        console.log('Failed to load image:', review.profilePhotoUrl);
                        e.target.style.display = 'none';
                        e.target.parentElement.innerHTML = `<svg class="w-6 h-6 text-gray-400" fill="currentColor" viewBox="0 0 20 20"><path fill-rule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clip-rule="evenodd"></path></svg>`;
                      }}
                    />
                  ) : (
                    <svg className="w-6 h-6 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clipRule="evenodd"></path>
                    </svg>
                  )}
                </div>
                <div>
                  <p className="font-semibold text-gray-900">{review.authorName}</p>
                  <p className="text-xs text-gray-500">{review.relativeTimeDescription}</p>
                </div>
              </div>

              {/* Star Rating */}
              <div className="mb-2">
                {renderStars(review.rating)}
              </div>

              {/* Review Text */}
              <p className="text-gray-700 text-sm flex-grow overflow-hidden" 
                 style={{
                   display: '-webkit-box',
                   WebkitLineClamp: 4,
                   WebkitBoxOrient: 'vertical',
                   overflow: 'hidden'
                 }}>
                {review.text}
              </p>
            </div>
          ))}
        </div>

        {/* Pagination Dots */}
        {showNavigation && (
          <div className="flex justify-center gap-2 mb-4">
            {Array.from({ length: Math.ceil(reviews.length / reviewsPerPage) }).map((_, idx) => (
              <button
                key={idx}
                onClick={() => setCurrentIndex(idx * reviewsPerPage)}
                className={`w-2 h-2 rounded-full transition ${
                  Math.floor(currentIndex / reviewsPerPage) === idx
                    ? 'bg-blue-600 w-4'
                    : 'bg-gray-300'
                }`}
                aria-label={`Go to review page ${idx + 1}`}
              />
            ))}
          </div>
        )}
      </div>

      {/* Read More Button */}
      {googleMapsUrl && (
        <div className="flex justify-center mt-4">
          <a
            href={googleMapsUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="px-6 py-2 rounded font-semibold transition hover:opacity-90"
            style={{ backgroundColor: 'var(--theme-color)', color: 'var(--secondary-theme-color)' }}
          >
            Read More Reviews
          </a>
        </div>
      )}
    </div>
  );
}

export default GoogleReviewsCarousel;
