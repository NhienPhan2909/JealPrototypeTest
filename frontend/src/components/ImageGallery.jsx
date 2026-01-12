import { useState } from 'react';

/**
 * ImageGallery - Vehicle image gallery with main view and thumbnail navigation.
 *
 * @component
 * @param {Object} props
 * @param {string[]} props.images - Array of Cloudinary image URLs
 *
 * Displays large main image with clickable thumbnail navigation below.
 * Applies Cloudinary URL transformations for optimization (WebP, responsive sizing).
 * Shows placeholder if images array is empty.
 *
 * Cloudinary Transformations:
 * - Main image: w_1920,f_auto,q_auto (1920px max width, auto format, auto quality)
 * - Thumbnails: w_200,h_150,c_fill,f_auto,q_auto (200x150px, crop fill, auto format, auto quality)
 * - f_auto serves WebP to compatible browsers with JPEG/PNG fallback
 *
 * @example
 * <ImageGallery images={['https://res.cloudinary.com/...', '...']} />
 */
function ImageGallery({ images }) {
  const [selectedIndex, setSelectedIndex] = useState(0);

  // Handle no images case - display placeholder
  if (!images || images.length === 0) {
    return (
      <div className="bg-gray-100 rounded-lg overflow-hidden">
        <img
          src="https://via.placeholder.com/800x600?text=No+Images"
          alt="No photos available"
          className="w-full h-96 object-contain"
        />
        <p className="text-center text-gray-600 py-4">No photos available</p>
      </div>
    );
  }

  const currentImage = images[selectedIndex];

  // Apply Cloudinary transformation for main gallery image (1920px width, auto format, auto quality)
  const galleryUrl = currentImage.replace('/upload/', '/upload/w_1920,f_auto,q_auto/');

  return (
    <div className="image-gallery">
      {/* Main Image */}
      <div className="bg-gray-100 rounded-lg overflow-hidden mb-4">
        <img
          src={galleryUrl}
          alt={`Vehicle image ${selectedIndex + 1}`}
          className="w-full h-96 object-contain"
        />
      </div>

      {/* Thumbnail Navigation */}
      {images.length > 1 && (
        <div className="flex gap-2 overflow-x-auto pb-2">
          {images.map((url, index) => {
            // Apply Cloudinary transformation for thumbnails (200x150, crop fill, auto format, auto quality)
            const thumbUrl = url.replace('/upload/', '/upload/w_200,h_150,c_fill,f_auto,q_auto/');

            return (
              <button
                key={index}
                onClick={() => setSelectedIndex(index)}
                className={`flex-shrink-0 border-2 rounded overflow-hidden transition-all focus:outline-none focus:ring-2 ${
                  index === selectedIndex ? 'border-theme' : 'border-gray-300'
                }`}
                style={index === selectedIndex ? {} : { '--tw-ring-color': 'var(--theme-color)' }}
                aria-label={`View image ${index + 1}`}
                aria-current={index === selectedIndex ? 'true' : 'false'}
              >
                <img
                  src={thumbUrl}
                  alt={`Thumbnail ${index + 1}`}
                  className="w-24 h-20 md:w-20 md:h-16 object-cover"
                />
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}

export default ImageGallery;
