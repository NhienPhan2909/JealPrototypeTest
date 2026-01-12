/**
 * @fileoverview Location Map page displaying dealership location with Google Maps.
 * Shows dealership address and embedded Google Map for easy navigation.
 */

import useDealership from '../../hooks/useDealership';
import { useDealershipContext } from '../../context/DealershipContext';
import { FaMapMarkerAlt, FaPhone, FaEnvelope } from 'react-icons/fa';

/**
 * Location - Displays dealership location with Google Maps integration.
 *
 * @component
 *
 * Features:
 * - Dealership address display
 * - Embedded Google Maps iframe showing location
 * - Contact information (phone, email)
 * - Responsive design for mobile and desktop
 *
 * @example
 * <Location />
 */
function Location() {
  const { currentDealershipId } = useDealershipContext();
  const { dealership, loading, error } = useDealership(currentDealershipId);

  // Loading state
  if (loading) {
    return (
      <div className="text-center text-gray-600 mt-10">
        Loading location information...
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="bg-red-50 border border-red-500 text-red-800 p-4 rounded max-w-2xl mx-auto mt-10">
        Unable to load location information. Please try again later.
      </div>
    );
  }

  // Clean and encode address for Google Maps URL
  // Trim whitespace and remove any hidden Unicode characters that could interfere with geocoding
  const cleanAddress = (dealership?.address || '').trim().replace(/[\u200B-\u200D\uFEFF]/g, '');
  const encodedAddress = encodeURIComponent(cleanAddress);
  
  // Use Google Maps iframe embed URL with zoom level for accurate location display
  // Format: https://maps.google.com/maps?q=ADDRESS&t=&z=ZOOM&ie=UTF8&iwloc=&output=embed
  const mapUrl = `https://maps.google.com/maps?q=${encodedAddress}&t=&z=16&ie=UTF8&iwloc=&output=embed`;
  
  // Google Maps Directions API URL for "Get Directions" button
  const directionsUrl = `https://www.google.com/maps/dir/?api=1&destination=${encodedAddress}`;

  return (
    <div className="max-w-6xl mx-auto px-4 py-8">
      {/* Page Title */}
      <h1 className="text-3xl md:text-4xl font-bold mb-8 text-center">
        Our Location
      </h1>

      <div className="grid md:grid-cols-2 gap-8">
        {/* Contact Information Card */}
        <div className="card">
          <h2 className="text-2xl font-semibold mb-6 flex items-center gap-2">
            <FaMapMarkerAlt className="text-theme" />
            Visit Us
          </h2>
          
          <div className="space-y-6">
            {/* Dealership Name */}
            <div>
              <h3 className="text-xl font-semibold mb-2">{dealership?.name}</h3>
            </div>

            {/* Address */}
            <div>
              <dt className="font-semibold text-gray-700 mb-1">Address:</dt>
              <dd className="text-gray-800 leading-relaxed">
                {dealership?.address}
              </dd>
            </div>

            {/* Phone */}
            <div>
              <dt className="font-semibold text-gray-700 mb-1 flex items-center gap-2">
                <FaPhone className="text-sm" />
                Phone:
              </dt>
              <dd>
                <a
                  href={`tel:${dealership?.phone}`}
                  className="text-theme hover:underline text-lg"
                >
                  {dealership?.phone}
                </a>
              </dd>
            </div>

            {/* Email */}
            <div>
              <dt className="font-semibold text-gray-700 mb-1 flex items-center gap-2">
                <FaEnvelope className="text-sm" />
                Email:
              </dt>
              <dd>
                <a
                  href={`mailto:${dealership?.email}`}
                  className="text-theme hover:underline"
                >
                  {dealership?.email}
                </a>
              </dd>
            </div>

            {/* Hours */}
            {dealership?.hours && (
              <div>
                <dt className="font-semibold text-gray-700 mb-1">Hours of Operation:</dt>
                <dd className="whitespace-pre-line text-gray-800">
                  {dealership.hours}
                </dd>
              </div>
            )}

            {/* Get Directions Button */}
            <div className="pt-4">
              <a
                href={directionsUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="btn-primary inline-flex items-center gap-2"
              >
                <FaMapMarkerAlt />
                Get Directions
              </a>
            </div>
          </div>
        </div>

        {/* Map Card */}
        <div className="card p-0 overflow-hidden">
          <div className="bg-gray-100 p-4 border-b">
            <h2 className="text-xl font-semibold">Map</h2>
          </div>
          <div className="relative" style={{ paddingBottom: '75%', height: 0 }}>
            <iframe
              src={mapUrl}
              className="absolute top-0 left-0 w-full h-full"
              style={{ border: 0 }}
              allowFullScreen=""
              loading="lazy"
              referrerPolicy="no-referrer-when-downgrade"
              title={`Map showing location of ${dealership?.name}`}
            />
          </div>
        </div>
      </div>

      {/* Additional Info Section */}
      <div className="card mt-8">
        <h2 className="text-2xl font-semibold mb-4">Plan Your Visit</h2>
        <p className="text-gray-700 leading-relaxed">
          We're conveniently located and easy to find. Whether you're looking to browse our inventory, 
          discuss financing options, or schedule a test drive, our team is here to help. 
          {dealership?.hours ? ' Check our hours above and stop by anytime during business hours.' : ' Give us a call to confirm our hours before visiting.'}
        </p>
      </div>
    </div>
  );
}

export default Location;
