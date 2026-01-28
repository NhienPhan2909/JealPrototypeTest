/**
 * @fileoverview Warranty page displaying dealership warranty policy and coverage information.
 * Shows warranty terms with contact details for inquiries.
 */

import { Link } from 'react-router-dom';
import useDealership from '../../hooks/useDealership';
import { useDealershipContext } from '../../context/DealershipContext';

/**
 * Warranty - Displays dealership warranty policy and contact information.
 *
 * @component
 *
 * Fetches and displays:
 * - Dealership name in heading
 * - Warranty policy content (multi-line text with formatting preserved)
 * - Default message if no policy is set
 * - Contact call-to-action with phone and email links
 * - Browse inventory CTA button
 *
 * @example
 * <Warranty />
 */
function Warranty() {
  const { currentDealershipId } = useDealershipContext();
  const { dealership, loading, error } = useDealership(currentDealershipId);

  // Loading state
  if (loading) {
    return (
      <div className="text-center text-gray-600 mt-10">
        Loading warranty information...
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="bg-red-50 border border-red-500 text-red-800 p-4 rounded max-w-2xl mx-auto mt-10">
        Unable to load warranty information. Please try again later.
      </div>
    );
  }

  // Main content
  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      {/* Page Heading */}
      <h1 className="text-3xl md:text-4xl font-bold mb-6 text-center">
        {dealership?.name}
      </h1>

      {/* Warranty Policy Content */}
      <div className="card mb-6">
        <h2 className="text-2xl font-semibold mb-4">Warranty</h2>
        <p className="whitespace-pre-line text-gray-700 leading-relaxed">
          {dealership?.warrantyPolicy || `Contact us to learn about our warranty coverage. Call ${dealership?.phone} or submit an enquiry.`}
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
                href={`mailto:${dealership?.email}?subject=Warranty Inquiry`}
                className="text-theme hover:underline"
              >
                {dealership?.email}
              </a>
            </dd>
          </div>
        </dl>
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

export default Warranty;
