/**
 * @fileoverview Finance page displaying dealership financing policy and options.
 * Shows financing information with contact details for inquiries.
 */

import { Link } from 'react-router-dom';
import useDealership from '../../hooks/useDealership';
import { useDealershipContext } from '../../context/DealershipContext';

/**
 * Finance - Displays dealership financing policy and contact information.
 *
 * @component
 *
 * Fetches and displays:
 * - Dealership name in heading
 * - Finance policy content (multi-line text with formatting preserved)
 * - Default message if no policy is set
 * - Contact call-to-action with phone and email links
 * - Browse inventory CTA button
 *
 * @example
 * <Finance />
 */
function Finance() {
  const { currentDealershipId } = useDealershipContext();
  const { dealership, loading, error } = useDealership(currentDealershipId);

  // Loading state
  if (loading) {
    return (
      <div className="text-center text-gray-600 mt-10">
        Loading financing information...
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="bg-red-50 border border-red-500 text-red-800 p-4 rounded max-w-2xl mx-auto mt-10">
        Unable to load financing information. Please try again later.
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

      {/* Finance Policy Content */}
      <div className="card mb-6">
        <h2 className="text-2xl font-semibold mb-4">Financing</h2>
        <p className="whitespace-pre-line text-gray-700 leading-relaxed">
          {dealership?.finance_policy || `Contact us to learn about our financing options. Call ${dealership?.phone} or submit an enquiry.`}
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
                href={`mailto:${dealership?.email}?subject=Financing Inquiry`}
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

export default Finance;
