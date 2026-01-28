/**
 * @fileoverview About/Contact page displaying dealership information.
 * Shows dealership name, logo, about text, contact details, hours, and CTA to browse inventory.
 */

import { Link } from 'react-router-dom';
import useDealership from '../../hooks/useDealership';
import { useDealershipContext } from '../../context/DealershipContext';

/**
 * About - Displays dealership information including contact details, hours, and about text.
 *
 * @component
 *
 * Fetches and displays:
 * - Dealership logo (if available)
 * - Dealership name as H1 heading
 * - About Us section with dealership description
 * - Contact Information section with address, phone (tel: link), email (mailto: link)
 * - Hours of Operation section
 * - Call-to-action button to browse inventory
 *
 * @example
 * <About />
 */
function About() {
  const { currentDealershipId } = useDealershipContext();
  const { dealership, loading, error } = useDealership(currentDealershipId);

  // Loading state
  if (loading) {
    return (
      <div className="text-center text-gray-600 mt-10">
        Loading dealership information...
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="bg-red-50 border border-red-500 text-red-800 p-4 rounded max-w-2xl mx-auto mt-10">
        Unable to load dealership information. Please try again later.
      </div>
    );
  }

  // Main content
  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      {/* Logo */}
      {dealership?.logoUrl && (
        <img
          src={dealership.logoUrl}
          alt={`${dealership.name} Logo`}
          className="mx-auto mb-6 w-48 md:max-w-xs object-contain"
        />
      )}

      {/* Dealership Name */}
      <h1 className="text-3xl md:text-4xl font-bold mb-6 text-center">
        {dealership?.name}
      </h1>

      {/* About Us Section */}
      <div className="card mb-6">
        <h2 className="text-2xl font-semibold mb-4">About Us</h2>
        <p className="whitespace-pre-line text-gray-700 leading-relaxed">
          {dealership?.about || 'No information available.'}
        </p>
      </div>

      {/* Contact Information Section */}
      <div className="card mb-6">
        <h2 className="text-2xl font-semibold mb-4">Contact Us</h2>
        <dl className="space-y-2">
          <div>
            <dt className="font-semibold">Address:</dt>
            <dd className="text-gray-700">{dealership?.address}</dd>
          </div>
          <div>
            <dt className="font-semibold">Phone:</dt>
            <dd>
              <a
                href={`tel:${dealership?.phone}`}
                className="text-theme hover:underline"
              >
                {dealership?.phone}
              </a>
            </dd>
          </div>
          <div>
            <dt className="font-semibold">Email:</dt>
            <dd>
              <a
                href={`mailto:${dealership?.email}`}
                className="text-theme hover:underline"
              >
                {dealership?.email}
              </a>
            </dd>
          </div>
        </dl>
      </div>

      {/* Hours of Operation Section */}
      <div className="card mb-6">
        <h2 className="text-2xl font-semibold mb-4">Hours of Operation</h2>
        <p className="whitespace-pre-line text-gray-700">
          {dealership?.hours || 'Hours not available. Please call for hours.'}
        </p>
      </div>

      {/* Call-to-Action Button */}
      <div className="flex justify-center mt-8">
        <Link to="/inventory" className="btn-primary">
          Browse Our Inventory
        </Link>
      </div>
    </div>
  );
}

export default About;
