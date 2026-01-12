import { Link } from 'react-router-dom';

/**
 * VehicleCard - Displays a vehicle summary in grid view.
 *
 * @component
 * @param {Object} props
 * @param {Object} props.vehicle - Vehicle object from API
 * @param {number} props.vehicle.id - Vehicle ID
 * @param {string} props.vehicle.title - Vehicle display title (e.g., "2015 Toyota Camry SE")
 * @param {number|string} props.vehicle.price - Vehicle price in dollars
 * @param {string} props.vehicle.condition - Vehicle condition ("new" or "used")
 * @param {string} props.vehicle.status - Vehicle status ("active", "pending", "sold", "draft")
 * @param {string[]} props.vehicle.images - Array of Cloudinary image URLs
 *
 * Features:
 * - Displays vehicle thumbnail (first image or placeholder if no images)
 * - Formats price as USD currency ($15,999)
 * - Shows condition badge ("New" or "Used")
 * - Shows "Pending Sale" badge if status is "pending"
 * - Lazy loads images for performance
 * - Entire card is clickable link to vehicle detail page
 *
 * @example
 * <VehicleCard vehicle={vehicleData} />
 */
function VehicleCard({ vehicle }) {
  /**
   * Gets the thumbnail image URL.
   * Uses first image from vehicle.images array, or placeholder if no images exist.
   */
  const thumbnail = vehicle.images && vehicle.images.length > 0
    ? vehicle.images[0]
    : 'https://via.placeholder.com/400x300?text=No+Image';

  /**
   * Formats the vehicle price as USD currency.
   * Example: 15999.99 → "$15,999"
   * Handles null/undefined prices gracefully.
   */
  const formattedPrice = vehicle.price
    ? new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD',
        minimumFractionDigits: 0
      }).format(vehicle.price)
    : 'Price unavailable';

  return (
    <Link
      to={`/inventory/${vehicle.id}`}
      className="card hover:shadow-xl transition-shadow duration-200 block overflow-hidden"
    >
      {/* Vehicle thumbnail image with lazy loading */}
      <img
        src={thumbnail}
        alt={vehicle.title}
        className="w-full h-48 object-cover"
        loading="lazy"
      />

      {/* Vehicle details */}
      <div className="p-4">
        {/* Vehicle title (make/model/year) */}
        <h3 className="text-lg font-semibold mb-2 text-gray-900 line-clamp-2">
          {vehicle.title}
        </h3>

        {/* Price */}
        <p className="text-2xl font-bold mb-3" style={{ color: 'var(--secondary-theme-color)' }}>
          {formattedPrice}
        </p>

        {/* Badges: Condition and Status */}
        <div className="flex gap-2 flex-wrap">
          {/* Condition badge: New (green) or Used (gray) */}
          <span
            className={`px-3 py-1 rounded text-sm font-medium ${
              vehicle.condition === 'new'
                ? 'bg-green-100 text-green-800'
                : 'bg-gray-100 text-gray-800'
            }`}
          >
            {vehicle.condition === 'new' ? 'New' : 'Used'}
          </span>

          {/* Pending Sale badge (only shown if status is "pending") */}
          {vehicle.status === 'pending' && (
            <span className="px-3 py-1 rounded text-sm font-medium bg-yellow-100 text-yellow-800">
              Pending Sale
            </span>
          )}
        </div>

        {/* View Details button with primary theme color background */}
        <div className="mt-4">
          <span 
            className="inline-block text-sm font-semibold px-4 py-2 rounded transition-opacity hover:opacity-90"
            style={{ backgroundColor: 'var(--theme-color)', color: 'var(--secondary-theme-color)' }}
          >
            View Details →
          </span>
        </div>
      </div>
    </Link>
  );
}

export default VehicleCard;
