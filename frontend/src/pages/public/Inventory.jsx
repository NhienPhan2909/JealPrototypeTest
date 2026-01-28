import { useState, useEffect, useMemo } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import VehicleCard from '../../components/VehicleCard';
import { useDealershipContext } from '../../context/DealershipContext';
import apiRequest from '../../utils/api';

/**
 * Inventory - Public vehicle listing page with search, filter, and sort controls.
 *
 * @component
 *
 * Fetches all public-visible vehicles for the dealership and displays them
 * in a responsive grid layout with client-side search, filtering, and sorting.
 * Supports keyword search across make/model/year/title, condition filtering,
 * and sorting by price or year.
 *
 * Features:
 * - Responsive grid: 3 columns (desktop), 2 columns (tablet), 1 column (mobile)
 * - Client-side search with debouncing (300ms delay)
 * - Condition filter (All/New/Used)
 * - Sort options (price low-high, high-low, year newest, oldest)
 * - Vehicle count display
 * - Loading state while fetching vehicles from API
 * - Error state if API request fails
 * - Empty state when no vehicles exist
 * - No results state when filters exclude all vehicles
 *
 * @example
 * <Inventory />
 */
function Inventory() {
  const { currentDealershipId } = useDealershipContext();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const [vehicles, setVehicles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Extract URL parameters for advanced filters
  const urlBrand = searchParams.get('brand') || '';
  const urlMinYear = searchParams.get('minYear') || '';
  const urlMaxYear = searchParams.get('maxYear') || '';
  const urlMinPrice = searchParams.get('minPrice') || '';
  const urlMaxPrice = searchParams.get('maxPrice') || '';

  // Search and filter state
  const [searchQuery, setSearchQuery] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [conditionFilter, setConditionFilter] = useState('All');
  const [sortOption, setSortOption] = useState('');

  useEffect(() => {
    /**
     * Fetches vehicles from API for the current dealership.
     *
     * IMPORTANT: Fetches ALL vehicles then filters client-side to show only
     * public-visible statuses (active and pending). This approach allows us to:
     * - Display available vehicles (status='active')
     * - Display vehicles with "Pending Sale" badge (status='pending') per AC10
     * - Exclude sold and draft vehicles from public view
     *
     * NOTE: AC2 originally specified status=active filter, but this contradicted
     * AC10 requirement to show pending vehicles. Fixed during QA review.
     */
    const fetchVehicles = async () => {
      try {
        // Build query parameters with dealership ID and URL filters
        const params = new URLSearchParams({
          dealershipId: currentDealershipId
        });

        // Add URL filter parameters if present
        if (urlBrand) params.append('brand', urlBrand);
        if (urlMinYear) params.append('minYear', urlMinYear);
        if (urlMaxYear) params.append('maxYear', urlMaxYear);
        if (urlMinPrice) params.append('minPrice', urlMinPrice);
        if (urlMaxPrice) params.append('maxPrice', urlMaxPrice);

        // Fetch vehicles with filters (server-side filtering)
        const response = await apiRequest(`/api/vehicles?${params.toString()}`);

        if (!response.ok) {
          throw new Error('Failed to fetch vehicles');
        }

        const result = await response.json();
        const data = result.data || result.Data || result;

        // Filter to show only public-visible vehicles (active and pending)
        // Exclude 'sold' and 'draft' status vehicles from public inventory
        const publicVehicles = data.filter(
          vehicle => vehicle.status === 'active' || vehicle.status === 'pending'
        );

        setVehicles(publicVehicles);
        setLoading(false);
      } catch (err) {
        console.error('Failed to load vehicles:', err);
        setError(err.message);
        setLoading(false);
      }
    };

    fetchVehicles();
  }, [currentDealershipId, urlBrand, urlMinYear, urlMaxYear, urlMinPrice, urlMaxPrice]);

  /**
   * Debounce search input to avoid excessive filtering during typing.
   * 300ms delay is industry standard for search input debouncing.
   */
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(searchQuery);
    }, 300);

    // Cleanup timeout on unmount or when searchQuery changes
    return () => clearTimeout(timer);
  }, [searchQuery]);

  /**
   * Compute filtered and sorted vehicles using useMemo for performance.
   * Memoization prevents expensive filter/sort operations on every render.
   *
   * Filter chain: search → condition → sort
   *
   * Dependencies: vehicles, debouncedSearch, conditionFilter, sortOption
   */
  const filteredAndSortedVehicles = useMemo(() => {
    let result = [...vehicles];

    // Apply search filter (case-insensitive across make, model, year, title)
    if (debouncedSearch) {
      const query = debouncedSearch.toLowerCase();
      result = result.filter(vehicle =>
        vehicle.make.toLowerCase().includes(query) ||
        vehicle.model.toLowerCase().includes(query) ||
        vehicle.year.toString().includes(query) ||
        vehicle.title.toLowerCase().includes(query)
      );
    }

    // Apply condition filter
    if (conditionFilter !== 'All') {
      result = result.filter(vehicle =>
        vehicle.condition === conditionFilter.toLowerCase()
      );
    }

    // Apply sort
    if (sortOption === 'price-low-high') {
      result.sort((a, b) => parseFloat(a.price) - parseFloat(b.price));
    } else if (sortOption === 'price-high-low') {
      result.sort((a, b) => parseFloat(b.price) - parseFloat(a.price));
    } else if (sortOption === 'year-newest') {
      result.sort((a, b) => b.year - a.year);
    } else if (sortOption === 'year-oldest') {
      result.sort((a, b) => a.year - b.year);
    }

    return result;
  }, [vehicles, debouncedSearch, conditionFilter, sortOption]);

  /**
   * Resets all search and filter controls to default values.
   * Shows full inventory again.
   */
  const handleClearFilters = () => {
    setSearchQuery('');
    setConditionFilter('All');
    setSortOption('');
  };

  // Loading state
  if (loading) {
    return (
      <div className="container mx-auto px-4 py-12">
        <div className="text-center text-xl text-gray-600">
          Loading inventory...
        </div>
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="container mx-auto px-4 py-12">
        <div className="text-center text-xl text-red-600">
          Unable to load inventory. Please try again later.
        </div>
      </div>
    );
  }

  // Empty state
  if (vehicles.length === 0) {
    return (
      <div className="container mx-auto px-4 py-12">
        <div className="text-center">
          <h2 className="text-2xl font-bold mb-4 text-gray-800">
            No vehicles available yet. Check back soon!
          </h2>
          <p className="text-gray-600 mb-2">Contact us for more information:</p>
          <p className="text-theme font-semibold text-lg">(555) 123-4567</p>
        </div>
      </div>
    );
  }

  // No results state - filters produced empty results but vehicles exist
  const noResults = filteredAndSortedVehicles.length === 0;

  /**
   * Clears all URL-based filters and resets to full inventory.
   */
  const handleClearAllFilters = () => {
    navigate('/inventory');
  };

  // Check if any URL filters are active
  const hasActiveFilters = urlBrand || urlMinYear || urlMaxYear || urlMinPrice || urlMaxPrice;

  // Build filter display text
  const getFilterDisplayText = () => {
    const filters = [];
    if (urlBrand) filters.push(`Brand: ${urlBrand.charAt(0).toUpperCase() + urlBrand.slice(1)}`);
    if (urlMinYear || urlMaxYear) {
      if (urlMinYear && urlMaxYear) {
        filters.push(`Year: ${urlMinYear}-${urlMaxYear}`);
      } else if (urlMinYear) {
        filters.push(`Year: ${urlMinYear}+`);
      } else {
        filters.push(`Year: Up to ${urlMaxYear}`);
      }
    }
    if (urlMinPrice || urlMaxPrice) {
      if (urlMinPrice && urlMaxPrice) {
        filters.push(`Price: $${parseInt(urlMinPrice).toLocaleString()}-$${parseInt(urlMaxPrice).toLocaleString()}`);
      } else if (urlMinPrice) {
        filters.push(`Price: $${parseInt(urlMinPrice).toLocaleString()}+`);
      } else {
        filters.push(`Price: Up to $${parseInt(urlMaxPrice).toLocaleString()}`);
      }
    }
    return filters.join(', ');
  };

  // Main content - vehicle grid with search/filter controls
  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-8 text-gray-900">Our Inventory</h1>

      {/* Active Filters Display */}
      {hasActiveFilters && (
        <div className="mb-6 p-4 border border-theme rounded-lg" style={{ backgroundColor: 'var(--theme-color-light)' }}>
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-3">
            <div>
              <p className="text-sm font-medium text-gray-700">Active Filters:</p>
              <p className="text-theme font-semibold">{getFilterDisplayText()}</p>
            </div>
            <button
              onClick={handleClearAllFilters}
              className="btn-secondary whitespace-nowrap self-start md:self-auto"
            >
              Clear All Filters
            </button>
          </div>
        </div>
      )}

      {/* Search and Filter Controls */}
      <div className="mb-6 space-y-4 md:space-y-0 md:flex md:gap-4 md:items-center">
        {/* Search Input */}
        <input
          type="text"
          placeholder="Search by make, model, or year..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="input-field md:flex-1"
        />

        {/* Condition Filter */}
        <select
          value={conditionFilter}
          onChange={(e) => setConditionFilter(e.target.value)}
          className="input-field md:w-40"
        >
          <option value="All">All Conditions</option>
          <option value="New">New</option>
          <option value="Used">Used</option>
        </select>

        {/* Sort Dropdown */}
        <select
          value={sortOption}
          onChange={(e) => setSortOption(e.target.value)}
          className="input-field md:w-48"
        >
          <option value="">Sort By</option>
          <option value="price-low-high">Price: Low to High</option>
          <option value="price-high-low">Price: High to Low</option>
          <option value="year-newest">Year: Newest</option>
          <option value="year-oldest">Year: Oldest</option>
        </select>

        {/* Clear Filters Button */}
        <button
          onClick={handleClearFilters}
          className="btn-secondary whitespace-nowrap"
        >
          Clear Filters
        </button>
      </div>

      {/* Vehicle Count */}
      <p className="text-gray-600 mb-4">
        Showing {filteredAndSortedVehicles.length} vehicles
      </p>

      {/* No Results Message */}
      {noResults && (
        <div className="text-center py-12">
          <p className="text-xl text-gray-600">
            No vehicles match your search. Try different filters.
          </p>
        </div>
      )}

      {/* Vehicle Grid */}
      {!noResults && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredAndSortedVehicles.map(vehicle => (
            <VehicleCard key={vehicle.id} vehicle={vehicle} />
          ))}
        </div>
      )}
    </div>
  );
}

export default Inventory;
