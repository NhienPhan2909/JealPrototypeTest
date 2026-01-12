/**
 * @fileoverview Hero carousel component for homepage hero section.
 * Displays rotating image carousel with automatic transitions.
 */

import { useState, useEffect } from 'react';

/**
 * HeroCarousel - Automatic image carousel for hero section.
 *
 * @component
 * @param {Object} props
 * @param {Array<string>} props.images - Array of image URLs
 * @param {number} [props.interval=5000] - Transition interval in milliseconds
 * @param {React.ReactNode} props.children - Content to overlay on carousel
 *
 * @example
 * <HeroCarousel images={['url1', 'url2', 'url3']}>
 *   <h1>Welcome</h1>
 * </HeroCarousel>
 */
function HeroCarousel({ images, interval = 5000, children }) {
  const [currentIndex, setCurrentIndex] = useState(0);

  useEffect(() => {
    if (images.length <= 1) return;

    const timer = setInterval(() => {
      setCurrentIndex((prev) => (prev + 1) % images.length);
    }, interval);

    return () => clearInterval(timer);
  }, [images.length, interval]);

  const goToSlide = (index) => {
    setCurrentIndex(index);
  };

  const goToPrevious = () => {
    setCurrentIndex((prev) => (prev - 1 + images.length) % images.length);
  };

  const goToNext = () => {
    setCurrentIndex((prev) => (prev + 1) % images.length);
  };

  if (!images || images.length === 0) {
    return (
      <div className="text-white py-12 md:py-20 relative bg-gradient-to-r from-blue-500 to-blue-700">
        <div className="container mx-auto px-4 text-center relative z-10">
          {children}
        </div>
      </div>
    );
  }

  return (
    <div className="text-white py-12 md:py-20 relative overflow-hidden">
      {/* Carousel Images */}
      <div className="absolute inset-0">
        {images.map((image, index) => (
          <div
            key={index}
            className="absolute inset-0 transition-opacity duration-1000"
            style={{
              opacity: index === currentIndex ? 1 : 0,
              backgroundImage: `linear-gradient(rgba(0, 0, 0, 0.4), rgba(0, 0, 0, 0.4)), url(${image})`,
              backgroundSize: 'contain',
              backgroundPosition: 'center',
              backgroundRepeat: 'no-repeat'
            }}
          />
        ))}
      </div>

      {/* Content Overlay */}
      <div className="container mx-auto px-4 text-center relative z-10">
        {children}
      </div>

      {/* Navigation Controls - Only show if more than 1 image */}
      {images.length > 1 && (
        <>
          {/* Previous/Next Buttons */}
          <button
            onClick={goToPrevious}
            className="absolute left-4 top-1/2 -translate-y-1/2 z-20 bg-black bg-opacity-50 text-white p-2 rounded-full hover:bg-opacity-75 transition"
            aria-label="Previous slide"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
          </button>
          <button
            onClick={goToNext}
            className="absolute right-4 top-1/2 -translate-y-1/2 z-20 bg-black bg-opacity-50 text-white p-2 rounded-full hover:bg-opacity-75 transition"
            aria-label="Next slide"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </button>

          {/* Dots Indicator */}
          <div className="absolute bottom-4 left-1/2 -translate-x-1/2 z-20 flex gap-2">
            {images.map((_, index) => (
              <button
                key={index}
                onClick={() => goToSlide(index)}
                className={`w-3 h-3 rounded-full transition ${
                  index === currentIndex ? 'bg-white' : 'bg-white bg-opacity-50'
                }`}
                aria-label={`Go to slide ${index + 1}`}
              />
            ))}
          </div>
        </>
      )}
    </div>
  );
}

export default HeroCarousel;
