/**
 * @fileoverview Public site footer with dealership information and navigation.
 * Displays contact info, social media links, opening hours, and navigation links.
 * Styled with dealership theme color for consistent branding.
 */

import { Link } from 'react-router-dom';
import useDealership from '../hooks/useDealership';
import { useDealershipContext } from '../context/DealershipContext';
import { getValidatedNavigation } from '../utils/defaultNavigation';

/**
 * Footer - Public site footer with dealership information and navigation.
 *
 * @component
 *
 * Displays dealership information from shared useDealership hook:
 * - Dealership name
 * - Contact information (phone, email, address)
 * - Opening hours
 * - Social media links (Facebook, Instagram)
 * - Navigation links (filtered to exclude admin/login)
 * - Background color matches dealership theme
 *
 * @example
 * <Footer />
 */
function Footer() {
  const { currentDealershipId } = useDealershipContext();
  const { dealership, loading, error } = useDealership(currentDealershipId);

  // Get theme color from dealership data, default to blue if not set
  const themeColor = dealership?.themeColor || '#3B82F6';

  // Get navigation config and filter out admin/login links
  const navigationConfig = getValidatedNavigation(dealership?.navigation_config);
  const footerNavItems = navigationConfig
    .filter(item => item.enabled !== false && !item.route.includes('admin') && !item.route.includes('login'))
    .sort((a, b) => a.order - b.order);

  if (loading) {
    return (
      <footer className="py-6 mt-auto" style={{ backgroundColor: themeColor, color: 'var(--secondary-theme-color)' }}>
        <div className="container mx-auto px-4 text-center">
          <p>Loading...</p>
        </div>
      </footer>
    );
  }

  if (error) {
    return (
      <footer className="py-6 mt-auto" style={{ backgroundColor: themeColor, color: 'var(--secondary-theme-color)' }}>
        <div className="container mx-auto px-4 text-center">
          <p>&copy; {new Date().getFullYear()} Multi-Dealership Platform. All rights reserved.</p>
        </div>
      </footer>
    );
  }

  return (
    <footer className="py-8 mt-auto" style={{ backgroundColor: themeColor, color: 'var(--secondary-theme-color)' }}>
      <div className="container mx-auto px-4">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          {/* Column 1: Dealership Info & Contact */}
          <div>
            <h3 className="text-xl font-bold mb-4">{dealership?.name || 'Dealership'}</h3>
            
            {/* Address */}
            {dealership?.address && (
              <div className="mb-3">
                <h4 className="font-semibold mb-1 opacity-90">Address</h4>
                <p className="opacity-80 text-sm">{dealership.address}</p>
              </div>
            )}

            {/* Phone */}
            {dealership?.phone && (
              <div className="mb-3">
                <h4 className="font-semibold mb-1 opacity-90">Phone</h4>
                <a 
                  href={`tel:${dealership.phone.replace(/\s/g, '')}`}
                  className="opacity-80 hover:opacity-100 text-sm transition-opacity"
                >
                  {dealership.phone}
                </a>
              </div>
            )}

            {/* Email */}
            {dealership?.email && (
              <div className="mb-3">
                <h4 className="font-semibold mb-1 opacity-90">Email</h4>
                <a 
                  href={`mailto:${dealership.email}`}
                  className="opacity-80 hover:opacity-100 text-sm transition-opacity"
                >
                  {dealership.email}
                </a>
              </div>
            )}
          </div>

          {/* Column 2: Opening Hours & Social Media */}
          <div>
            <h3 className="text-xl font-bold mb-4">Opening Hours</h3>
            {dealership?.hours ? (
              <p className="opacity-80 text-sm whitespace-pre-line mb-4">{dealership.hours}</p>
            ) : (
              <p className="opacity-60 text-sm italic mb-4">No opening hours available</p>
            )}

            {/* Social Media Links */}
            {(dealership?.facebookUrl || dealership?.instagramUrl) && (
              <div className="mt-6">
                <h4 className="font-semibold mb-3 opacity-90">Follow Us</h4>
                <div className="flex gap-4">
                  {dealership?.facebookUrl && (
                    <a
                      href={dealership.facebookUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="opacity-80 hover:opacity-100 transition-opacity"
                      aria-label="Visit our Facebook page"
                    >
                      <svg className="w-8 h-8" fill="currentColor" viewBox="0 0 24 24">
                        <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"/>
                      </svg>
                    </a>
                  )}

                  {dealership?.instagramUrl && (
                    <a
                      href={dealership.instagramUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="opacity-80 hover:opacity-100 transition-opacity"
                      aria-label="Visit our Instagram page"
                    >
                      <svg className="w-8 h-8" fill="currentColor" viewBox="0 0 24 24">
                        <path d="M12 2.163c3.204 0 3.584.012 4.85.07 3.252.148 4.771 1.691 4.919 4.919.058 1.265.069 1.645.069 4.849 0 3.205-.012 3.584-.069 4.849-.149 3.225-1.664 4.771-4.919 4.919-1.266.058-1.644.07-4.85.07-3.204 0-3.584-.012-4.849-.07-3.26-.149-4.771-1.699-4.919-4.92-.058-1.265-.07-1.644-.07-4.849 0-3.204.013-3.583.07-4.849.149-3.227 1.664-4.771 4.919-4.919 1.266-.057 1.645-.069 4.849-.069zm0-2.163c-3.259 0-3.667.014-4.947.072-4.358.2-6.78 2.618-6.98 6.98-.059 1.281-.073 1.689-.073 4.948 0 3.259.014 3.668.072 4.948.2 4.358 2.618 6.78 6.98 6.98 1.281.058 1.689.072 4.948.072 3.259 0 3.668-.014 4.948-.072 4.354-.2 6.782-2.618 6.979-6.98.059-1.28.073-1.689.073-4.948 0-3.259-.014-3.667-.072-4.947-.196-4.354-2.617-6.78-6.979-6.98-1.281-.059-1.69-.073-4.949-.073zm0 5.838c-3.403 0-6.162 2.759-6.162 6.162s2.759 6.163 6.162 6.163 6.162-2.759 6.162-6.163c0-3.403-2.759-6.162-6.162-6.162zm0 10.162c-2.209 0-4-1.79-4-4 0-2.209 1.791-4 4-4s4 1.791 4 4c0 2.21-1.791 4-4 4zm6.406-11.845c-.796 0-1.441.645-1.441 1.44s.645 1.44 1.441 1.44c.795 0 1.439-.645 1.439-1.44s-.644-1.44-1.439-1.44z"/>
                      </svg>
                    </a>
                  )}
                </div>
              </div>
            )}
          </div>

          {/* Column 3: Quick Links */}
          <div>
            <h3 className="text-xl font-bold mb-4">Quick Links</h3>
            <nav className="space-y-2">
              {footerNavItems.map(item => (
                <Link
                  key={item.id}
                  to={item.route}
                  className="block opacity-80 hover:opacity-100 text-sm transition-opacity"
                >
                  {item.label}
                </Link>
              ))}
            </nav>
          </div>
        </div>

        {/* Copyright */}
        <div className="border-t pt-6 mt-6" style={{ borderColor: 'var(--secondary-theme-color)', opacity: 0.2 }}>
          <p className="text-center opacity-70 text-sm">
            &copy; {new Date().getFullYear()} {dealership?.name || 'Multi-Dealership Platform'}. All rights reserved.
          </p>
        </div>
      </div>
    </footer>
  );
}

export default Footer;
