/**
 * @fileoverview Promotional panels component for Finance and Warranty sections.
 * Displays two side-by-side promotional cards with background images and CTA buttons.
 */

import { Link } from 'react-router-dom';

/**
 * PromotionalPanels - Displays Finance and Warranty promotional panels.
 *
 * @component
 *
 * Features:
 * - Side-by-side layout (responsive: stacked on mobile, side-by-side on desktop)
 * - Background image with dark overlay
 * - Promotional text overlay
 * - "View Our Policy" button linking to respective pages
 * - Fallback defaults if content not provided
 *
 * @param {Object} props - Component props
 * @param {string} [props.financeImage] - Finance panel background image URL
 * @param {string} [props.financeText] - Finance panel promotional text
 * @param {string} [props.warrantyImage] - Warranty panel background image URL
 * @param {string} [props.warrantyText] - Warranty panel promotional text
 *
 * @example
 * <PromotionalPanels
 *   financeImage="https://example.com/finance.jpg"
 *   financeText="Flexible Financing Options Available"
 *   warrantyImage="https://example.com/warranty.jpg"
 *   warrantyText="Comprehensive Warranty Coverage"
 * />
 */
function PromotionalPanels({ financeImage, financeText, warrantyImage, warrantyText }) {
  // Default promotional text if not provided
  const defaultFinanceText = 'Explore Our Financing Options';
  const defaultWarrantyText = 'Learn About Our Warranty';

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-8">
      {/* Finance Promotional Panel */}
      <div
        className="relative overflow-hidden rounded-lg shadow-lg min-h-[300px] flex items-center justify-center"
        style={{
          backgroundImage: financeImage
            ? `linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url(${financeImage})`
            : 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          backgroundSize: 'cover',
          backgroundPosition: 'center',
          backgroundRepeat: 'no-repeat'
        }}
      >
        <div className="text-center text-white px-6 py-8 relative z-10">
          <h2 className="text-2xl md:text-3xl font-bold mb-4 drop-shadow-lg">
            Finance
          </h2>
          <p className="text-lg md:text-xl mb-6 drop-shadow-md">
            {financeText || defaultFinanceText}
          </p>
          <Link
            to="/finance"
            className="inline-block font-bold px-6 py-3 rounded-lg transition text-base shadow-lg hover:opacity-90"
            style={{ backgroundColor: 'var(--theme-color)', color: 'var(--secondary-theme-color)' }}
          >
            View Our Policy
          </Link>
        </div>
      </div>

      {/* Warranty Promotional Panel */}
      <div
        className="relative overflow-hidden rounded-lg shadow-lg min-h-[300px] flex items-center justify-center"
        style={{
          backgroundImage: warrantyImage
            ? `linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url(${warrantyImage})`
            : 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
          backgroundSize: 'cover',
          backgroundPosition: 'center',
          backgroundRepeat: 'no-repeat'
        }}
      >
        <div className="text-center text-white px-6 py-8 relative z-10">
          <h2 className="text-2xl md:text-3xl font-bold mb-4 drop-shadow-lg">
            Warranty
          </h2>
          <p className="text-lg md:text-xl mb-6 drop-shadow-md">
            {warrantyText || defaultWarrantyText}
          </p>
          <Link
            to="/warranty"
            className="inline-block font-bold px-6 py-3 rounded-lg transition text-base shadow-lg hover:opacity-90"
            style={{ backgroundColor: 'var(--theme-color)', color: 'var(--secondary-theme-color)' }}
          >
            View Our Policy
          </Link>
        </div>
      </div>
    </div>
  );
}

export default PromotionalPanels;
