/**
 * @fileoverview Homepage vehicle search widget with advanced filtering.
 * Provides brand, year range, and price range filters that navigate to inventory page with URL query parameters.
 *
 * Features:
 * - 5 filter inputs: brand dropdown, min/max year, min/max price
 * - Client-side validation with error messages
 * - URL-based navigation to filtered inventory results
 * - Responsive design (mobile stacked, desktop 2-column grid)
 * - Tailwind CSS styling following project patterns
 */

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

/**
 * SearchWidget - Homepage vehicle search form with advanced filters.
 *
 * @component
 *
 * Allows users to filter vehicles by brand, year range, and price range.
 * On submit, navigates to /inventory with query parameters for server-side filtering.
 * All fields are optional - users can search with any combination of filters.
 *
 * Validation Rules:
 * - minYear must be ≤ maxYear
 * - minPrice must be ≤ maxPrice
 * - Year range: 1900-2100
 * - Prices must be non-negative
 *
 * @example
 * // Basic usage on homepage
 * <SearchWidget />
 *
 * // User fills: brand=Mazda, minYear=2015, maxYear=2025, minPrice=15000, maxPrice=25000
 * // Navigates to: /inventory?brand=mazda&minYear=2015&maxYear=2025&minPrice=15000&maxPrice=25000
 */
function SearchWidget() {
  const navigate = useNavigate();

  // Form state - all fields optional
  const [brand, setBrand] = useState('');
  const [minYear, setMinYear] = useState('');
  const [maxYear, setMaxYear] = useState('');
  const [minPrice, setMinPrice] = useState('');
  const [maxPrice, setMaxPrice] = useState('');

  // Validation error state
  const [yearError, setYearError] = useState('');
  const [priceError, setPriceError] = useState('');

  /**
   * Validates year range inputs.
   * Ensures minYear ≤ maxYear and both are within 1900-2100 range.
   *
   * @returns {boolean} True if valid, false otherwise
   */
  const validateYearRange = () => {
    // Clear previous error
    setYearError('');

    // Skip validation if both fields empty
    if (!minYear && !maxYear) {
      return true;
    }

    // Validate year range (1900-2100)
    if (minYear) {
      const minYearNum = parseInt(minYear, 10);
      if (isNaN(minYearNum) || minYearNum < 1900 || minYearNum > 2100) {
        setYearError('Minimum year must be between 1900 and 2100');
        return false;
      }
    }

    if (maxYear) {
      const maxYearNum = parseInt(maxYear, 10);
      if (isNaN(maxYearNum) || maxYearNum < 1900 || maxYearNum > 2100) {
        setYearError('Maximum year must be between 1900 and 2100');
        return false;
      }
    }

    // Validate minYear ≤ maxYear
    if (minYear && maxYear) {
      const minYearNum = parseInt(minYear, 10);
      const maxYearNum = parseInt(maxYear, 10);
      if (minYearNum > maxYearNum) {
        setYearError('Minimum year must be less than or equal to maximum year');
        return false;
      }
    }

    return true;
  };

  /**
   * Validates price range inputs.
   * Ensures minPrice ≤ maxPrice and both are non-negative.
   *
   * @returns {boolean} True if valid, false otherwise
   */
  const validatePriceRange = () => {
    // Clear previous error
    setPriceError('');

    // Skip validation if both fields empty
    if (!minPrice && !maxPrice) {
      return true;
    }

    // Validate prices are non-negative
    if (minPrice) {
      const minPriceNum = parseFloat(minPrice);
      if (isNaN(minPriceNum) || minPriceNum < 0) {
        setPriceError('Minimum price must be a non-negative number');
        return false;
      }
    }

    if (maxPrice) {
      const maxPriceNum = parseFloat(maxPrice);
      if (isNaN(maxPriceNum) || maxPriceNum < 0) {
        setPriceError('Maximum price must be a non-negative number');
        return false;
      }
    }

    // Validate minPrice ≤ maxPrice
    if (minPrice && maxPrice) {
      const minPriceNum = parseFloat(minPrice);
      const maxPriceNum = parseFloat(maxPrice);
      if (minPriceNum > maxPriceNum) {
        setPriceError('Minimum price must be less than or equal to maximum price');
        return false;
      }
    }

    return true;
  };

  /**
   * Handles form submission.
   * Validates inputs, builds URL query string, and navigates to inventory page.
   *
   * @param {Event} e - Form submit event
   */
  const handleSubmit = (e) => {
    e.preventDefault();

    // Validate inputs
    const isYearValid = validateYearRange();
    const isPriceValid = validatePriceRange();

    if (!isYearValid || !isPriceValid) {
      return; // Stop submission if validation fails
    }

    // Build query parameters (only include non-empty filters)
    const params = new URLSearchParams();

    if (brand) {
      params.append('brand', brand.toLowerCase()); // Case-insensitive search
    }
    if (minYear) {
      params.append('minYear', minYear);
    }
    if (maxYear) {
      params.append('maxYear', maxYear);
    }
    if (minPrice) {
      params.append('minPrice', minPrice);
    }
    if (maxPrice) {
      params.append('maxPrice', maxPrice);
    }

    // Navigate to inventory page with query parameters
    const queryString = params.toString();
    navigate(queryString ? `/inventory?${queryString}` : '/inventory');
  };

  // Check if form has validation errors (disable submit button)
  const hasErrors = yearError || priceError;

  return (
    <div className="bg-white rounded-lg shadow-md p-6 h-full">
      <h2 className="text-2xl font-bold mb-4 text-gray-900">Find Your Perfect Vehicle</h2>
      <p className="text-gray-600 mb-6">Search our inventory using the filters below</p>

      <form onSubmit={handleSubmit}>
        {/* Filter Inputs - Responsive Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
          {/* Brand Dropdown */}
          <div>
            <label htmlFor="brand" className="block text-sm font-medium text-gray-700 mb-1">
              Brand
            </label>
            <select
              id="brand"
              value={brand}
              onChange={(e) => setBrand(e.target.value)}
              className="input-field"
            >
              <option value="">All Brands</option>
              <option value="mazda">Mazda</option>
              <option value="toyota">Toyota</option>
              <option value="honda">Honda</option>
              <option value="ford">Ford</option>
              <option value="chevrolet">Chevrolet</option>
              <option value="bmw">BMW</option>
              <option value="mercedes-benz">Mercedes-Benz</option>
              <option value="nissan">Nissan</option>
              <option value="hyundai">Hyundai</option>
              <option value="kia">Kia</option>
            </select>
          </div>

          {/* Placeholder for grid alignment */}
          <div></div>

          {/* Minimum Year */}
          <div>
            <label htmlFor="minYear" className="block text-sm font-medium text-gray-700 mb-1">
              Minimum Year
            </label>
            <input
              type="number"
              id="minYear"
              value={minYear}
              onChange={(e) => setMinYear(e.target.value)}
              onBlur={validateYearRange}
              placeholder="e.g., 2015"
              className="input-field"
              min="1900"
              max="2100"
            />
          </div>

          {/* Maximum Year */}
          <div>
            <label htmlFor="maxYear" className="block text-sm font-medium text-gray-700 mb-1">
              Maximum Year
            </label>
            <input
              type="number"
              id="maxYear"
              value={maxYear}
              onChange={(e) => setMaxYear(e.target.value)}
              onBlur={validateYearRange}
              placeholder="e.g., 2025"
              className="input-field"
              min="1900"
              max="2100"
            />
          </div>

          {/* Year Range Error Message */}
          {yearError && (
            <div className="md:col-span-2">
              <p className="text-red-600 text-sm">{yearError}</p>
            </div>
          )}

          {/* Minimum Price */}
          <div>
            <label htmlFor="minPrice" className="block text-sm font-medium text-gray-700 mb-1">
              Minimum Price ($)
            </label>
            <input
              type="number"
              id="minPrice"
              value={minPrice}
              onChange={(e) => setMinPrice(e.target.value)}
              onBlur={validatePriceRange}
              placeholder="e.g., 15000"
              className="input-field"
              min="0"
              step="100"
            />
          </div>

          {/* Maximum Price */}
          <div>
            <label htmlFor="maxPrice" className="block text-sm font-medium text-gray-700 mb-1">
              Maximum Price ($)
            </label>
            <input
              type="number"
              id="maxPrice"
              value={maxPrice}
              onChange={(e) => setMaxPrice(e.target.value)}
              onBlur={validatePriceRange}
              placeholder="e.g., 25000"
              className="input-field"
              min="0"
              step="100"
            />
          </div>

          {/* Price Range Error Message */}
          {priceError && (
            <div className="md:col-span-2">
              <p className="text-red-600 text-sm">{priceError}</p>
            </div>
          )}
        </div>

        {/* Submit Button */}
        <div className="flex justify-center">
          <button
            type="submit"
            disabled={hasErrors}
            className={`px-8 py-3 text-lg rounded font-semibold transition ${
              hasErrors ? 'opacity-50 cursor-not-allowed' : 'hover:opacity-90'
            }`}
            style={{ backgroundColor: 'var(--theme-color)', color: 'var(--secondary-theme-color)' }}
          >
            Search Vehicles
          </button>
        </div>
      </form>
    </div>
  );
}

export default SearchWidget;
