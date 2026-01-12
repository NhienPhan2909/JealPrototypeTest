import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import ImageGallery from '../../components/ImageGallery';
import EnquiryForm from '../../components/EnquiryForm';
import { useDealershipContext } from '../../context/DealershipContext';

/**
 * VehicleDetail - Public vehicle detail page with image gallery and specifications.
 *
 * @component
 *
 * Fetches and displays complete vehicle information including image gallery,
 * specifications, and description. Supports loading and error states.
 *
 * SECURITY (SEC-001): Includes dealershipId query parameter in API call to enforce
 * multi-tenant data isolation. API requires dealershipId to prevent cross-dealership
 * data access.
 *
 * Features:
 * - Responsive image gallery with Cloudinary transformations
 * - Formatted specifications (price, mileage, condition, status)
 * - Loading and error state handling
 * - "Back to Inventory" navigation
 * - 404 error handling for non-existent vehicles
 *
 * @example
 * <Route path="/inventory/:vehicleId" element={<VehicleDetail />} />
 */
function VehicleDetail() {
  const { vehicleId } = useParams();
  const { currentDealershipId } = useDealershipContext();
  const [vehicle, setVehicle] = useState(null);
  const [dealership, setDealership] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    // Guard: Don't fetch if dealershipId is not available yet
    if (!currentDealershipId) return;

    /**
     * Fetches vehicle details from API.
     *
     * SECURITY (SEC-001): Includes dealershipId query parameter to enforce
     * multi-tenant data isolation.
     */
    const fetchVehicle = async () => {
      try {
        setLoading(true);
        setError(null);

        // IMPORTANT: Include dealershipId query parameter (SEC-001 requirement)
        const response = await fetch(`/api/vehicles/${vehicleId}?dealershipId=${currentDealershipId}`);

        // Handle 404 - vehicle not found or belongs to different dealership
        if (response.status === 404) {
          setError('Vehicle not found');
          setLoading(false);
          return;
        }

        // Handle other HTTP errors
        if (!response.ok) {
          throw new Error('Failed to fetch vehicle');
        }

        const data = await response.json();
        setVehicle(data);
        setLoading(false);
      } catch (err) {
        console.error('Failed to load vehicle:', err);
        setError('Unable to load vehicle details. Please try again later.');
        setLoading(false);
      }
    };

    /**
     * Fetches dealership details from API.
     * Required to get dealership phone number for EnquiryForm error messages.
     */
    const fetchDealership = async () => {
      try {
        const response = await fetch(`/api/dealers/${currentDealershipId}`);
        if (response.ok) {
          const data = await response.json();
          setDealership(data);
        }
      } catch (err) {
        console.error('Failed to load dealership:', err);
        // Non-critical error - EnquiryForm will still work without phone number
      }
    };

    fetchVehicle();
    fetchDealership();
  }, [vehicleId, currentDealershipId]);

  // Loading state
  if (loading) {
    return (
      <div className="container mx-auto px-4 py-12">
        <div className="text-center text-xl text-gray-600">
          Loading vehicle details...
        </div>
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="container mx-auto px-4 py-12">
        <div className="text-center">
          <p className="text-xl text-red-600 mb-4">{error}</p>
          <Link to="/inventory" className="btn-secondary">
            Back to Inventory
          </Link>
        </div>
      </div>
    );
  }

  // Format price as currency
  const formattedPrice = parseFloat(vehicle.price).toLocaleString('en-US', {
    style: 'currency',
    currency: 'USD'
  });

  // Format mileage with commas
  const formattedMileage = vehicle.mileage.toLocaleString();

  // Capitalize condition
  const displayCondition = vehicle.condition === 'new' ? 'New' : 'Used';

  return (
    <div className="container mx-auto px-4 py-8">
      {/* Back to Inventory Link */}
      <Link to="/inventory" className="btn-secondary mb-6 inline-block">
        ← Back to Inventory
      </Link>

      {/* Vehicle Title */}
      <h1 className="text-3xl md:text-4xl font-bold mb-6 text-gray-900">
        {vehicle.title}
      </h1>

      {/* Image Gallery */}
      <ImageGallery images={vehicle.images} />

      {/* Specifications Section */}
      <div className="card mt-6">
        <h2 className="text-2xl font-semibold mb-4 text-gray-900">Specifications</h2>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
          <div>
            <span className="font-semibold text-gray-700">Make:</span>{' '}
            <span className="text-gray-900">{vehicle.make}</span>
          </div>
          <div>
            <span className="font-semibold text-gray-700">Model:</span>{' '}
            <span className="text-gray-900">{vehicle.model}</span>
          </div>
          <div>
            <span className="font-semibold text-gray-700">Year:</span>{' '}
            <span className="text-gray-900">{vehicle.year}</span>
          </div>
          <div>
            <span className="font-semibold text-gray-700">Price:</span>{' '}
            <span className="font-bold" style={{ color: 'var(--secondary-theme-color)', fontSize: '1.125rem' }}>{formattedPrice}</span>
          </div>
          <div>
            <span className="font-semibold text-gray-700">Mileage:</span>{' '}
            <span className="text-gray-900">{formattedMileage} miles</span>
          </div>
          <div>
            <span className="font-semibold text-gray-700">Condition:</span>{' '}
            <span className="text-gray-900">{displayCondition}</span>
          </div>
        </div>

        {/* Pending Sale Badge */}
        {vehicle.status === 'pending' && (
          <div className="mt-4">
            <span className="bg-yellow-500 text-white px-3 py-1 rounded text-sm font-medium">
              Pending Sale
            </span>
          </div>
        )}
      </div>

      {/* Description Section */}
      <div className="card mt-6">
        <h2 className="text-2xl font-semibold mb-4 text-gray-900">Description</h2>
        <p className="text-gray-700 leading-relaxed whitespace-pre-line">
          {vehicle.description || 'No description available.'}
        </p>
      </div>

      {/* Enquiry Form */}
      {dealership && (
        <EnquiryForm
          vehicleId={vehicle.id}
          dealershipId={currentDealershipId}
          vehicleTitle={vehicle.title}
          dealershipPhone={dealership.phone}
        />
      )}

      {/* Bottom Back to Inventory Link */}
      <div className="mt-8">
        <Link to="/inventory" className="btn-secondary">
          ← Back to Inventory
        </Link>
      </div>
    </div>
  );
}

export default VehicleDetail;
