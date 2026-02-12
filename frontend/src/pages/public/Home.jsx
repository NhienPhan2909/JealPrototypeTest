/**
 * @fileoverview Public home page with hero section, call-to-action, and vehicle search widget.
 * Displays dealership welcome message with inventory CTA and advanced search filters.
 */

import { Link } from 'react-router-dom';
import useDealership from '../../hooks/useDealership';
import { useDealershipContext } from '../../context/DealershipContext';
import SearchWidget from '../../components/SearchWidget';
import GeneralEnquiryForm from '../../components/GeneralEnquiryForm';
import GoogleReviewsCarousel from '../../components/GoogleReviewsCarousel';
import HeroCarousel from '../../components/HeroCarousel';
import PromotionalPanels from '../../components/PromotionalPanels';

/**
 * Home - Public home page with hero section and CTA.
 *
 * @component
 *
 * Displays dealership information from shared useDealership hook with
 * "Browse Inventory" call-to-action button.
 *
 * TODO: Make dealershipId configurable
 * Options: URL param, env variable (VITE_DEALERSHIP_ID), subdomain routing
 *
 * @example
 * <Home />
 */
function Home() {
  const { currentDealershipId } = useDealershipContext();
  const { dealership, loading, error } = useDealership(currentDealershipId);

  if (loading) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-500 text-lg">Loading...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="text-center py-12">
        <p className="text-red-500 text-lg">Error loading dealership information</p>
      </div>
    );
  }

  return (
    <>
      {/* Hero Section */}
      {dealership?.heroType === 'carousel' && dealership?.heroCarouselImages?.length > 0 ? (
        // Carousel Hero
        <HeroCarousel images={dealership.heroCarouselImages}>
          {dealership?.heroTitle && (
            <h1 className="text-3xl md:text-4xl lg:text-5xl font-bold mb-4 drop-shadow-lg">
              {dealership.heroTitle}
            </h1>
          )}

          {dealership?.heroSubtitle && (
            <p className="text-lg md:text-xl mb-8 max-w-2xl mx-auto drop-shadow-md">
              {dealership.heroSubtitle}
            </p>
          )}

          <Link
            to="/inventory"
            className="inline-block font-bold px-6 md:px-8 py-3 rounded-lg transition text-base md:text-lg shadow-lg hover:opacity-90"
            style={{ backgroundColor: 'var(--theme-color)', color: 'var(--secondary-theme-color)' }}
          >
            Browse Inventory
          </Link>
        </HeroCarousel>
      ) : dealership?.heroType === 'video' && dealership?.heroVideoUrl ? (
        // Video Hero
        <div className="text-white py-12 md:py-20 relative overflow-hidden">
          {/* Video Background */}
          <video
            autoPlay
            loop
            muted
            playsInline
            className="absolute inset-0 w-full h-full object-cover"
          >
            <source src={dealership.heroVideoUrl} type="video/mp4" />
            Your browser does not support the video tag.
          </video>

          {/* Dark Overlay */}
          <div className="absolute inset-0 bg-black bg-opacity-40"></div>

          {/* Content */}
          <div className="container mx-auto px-4 text-center relative z-10">
            {dealership?.heroTitle && (
              <h1 className="text-3xl md:text-4xl lg:text-5xl font-bold mb-4 drop-shadow-lg">
                {dealership.heroTitle}
              </h1>
            )}

            {dealership?.heroSubtitle && (
              <p className="text-lg md:text-xl mb-8 max-w-2xl mx-auto drop-shadow-md">
                {dealership.heroSubtitle}
              </p>
            )}

            <Link
              to="/inventory"
              className="inline-block font-bold px-6 md:px-8 py-3 rounded-lg transition text-base md:text-lg shadow-lg hover:opacity-90"
              style={{ backgroundColor: 'var(--theme-color)', color: 'var(--secondary-theme-color)' }}
            >
              Browse Inventory
            </Link>
          </div>
        </div>
      ) : (
        // Default Image Hero
        <div
          className="text-white py-12 md:py-20 relative bg-gradient-to-r from-blue-500 to-blue-700"
          style={
            dealership?.heroBackgroundImage
              ? {
                  backgroundImage: `linear-gradient(rgba(0, 0, 0, 0.4), rgba(0, 0, 0, 0.4)), url(${dealership.heroBackgroundImage})`,
                  backgroundSize: 'contain',
                  backgroundPosition: 'center',
                  backgroundRepeat: 'no-repeat'
                }
              : undefined
          }
        >
          <div className="container mx-auto px-4 text-center relative z-10">
            {dealership?.heroTitle && (
              <h1 className="text-3xl md:text-4xl lg:text-5xl font-bold mb-4 drop-shadow-lg">
                {dealership.heroTitle}
              </h1>
            )}

            {dealership?.heroSubtitle && (
              <p className="text-lg md:text-xl mb-8 max-w-2xl mx-auto drop-shadow-md">
                {dealership.heroSubtitle}
              </p>
            )}

            <Link
              to="/inventory"
              className="inline-block font-bold px-6 md:px-8 py-3 rounded-lg transition text-base md:text-lg shadow-lg hover:opacity-90"
              style={{ backgroundColor: 'var(--theme-color)', color: 'var(--secondary-theme-color)' }}
            >
              Browse Inventory
            </Link>
          </div>
        </div>
      )}

      {/* Vehicle Search Widget and General Enquiry Form - Side by Side */}
      <div className="container mx-auto px-4 py-12">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* Search Widget - Left Side */}
          <div>
            <SearchWidget />
          </div>

          {/* General Enquiry Form - Right Side */}
          <div>
            <GeneralEnquiryForm />
          </div>
        </div>

        {/* Google Reviews Carousel - Full Width Below */}
        <div className="mt-8">
          <GoogleReviewsCarousel />
        </div>

        {/* Promotional Panels for Finance and Warranty */}
        <PromotionalPanels
          financeImage={dealership?.financePromoImage}
          financeText={dealership?.financePromoText}
          warrantyImage={dealership?.warrantyPromoImage}
          warrantyText={dealership?.warrantyPromoText}
        />
      </div>
    </>
  );
}

export default Home;
