/**
 * @fileoverview Public site header with dealership branding and navigation.
 * Displays dealership logo, name, and navigation links using shared hook.
 * Each dealership has its own isolated website - no selector for end users.
 *
 * Story 5.3: Updated to use NavigationButton component with customizable icons.
 */

import { useState } from 'react';
import { Link } from 'react-router-dom';
import useDealership from '../hooks/useDealership';
import { useDealershipContext } from '../context/DealershipContext';
import NavigationButton from './NavigationButton';
import { getValidatedNavigation } from '../utils/defaultNavigation';

/**
 * Header - Public site header with responsive navigation and dealership branding.
 *
 * @component
 *
 * Displays dealership information from shared useDealership hook:
 * - Dealership logo (if logo_url exists)
 * - Dealership name
 * - Navigation links: Home, Inventory, About, Log In
 * - Mobile hamburger menu for small screens (< 768px)
 *
 * The "Log In" link navigates to /admin/login for dealership staff access.
 * No dealership selector - each dealership website is isolated.
 *
 * @example
 * <Header />
 */
function Header() {
  const { currentDealershipId } = useDealershipContext();
  const { dealership, loading, error } = useDealership(currentDealershipId);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  // Get theme color from dealership data, default to blue if not set
  const themeColor = dealership?.themeColor || '#3B82F6';

  // Get navigation config with validation and fallback to defaults
  const navigationConfig = getValidatedNavigation(dealership?.navigation_config);

  // Sort by order field and filter enabled items
  const enabledNavItems = navigationConfig
    .filter(item => item.enabled !== false)
    .sort((a, b) => a.order - b.order);

  if (loading) {
    return (
      <header className="bg-white shadow">
        <div className="container mx-auto px-4 py-4">
          <p className="text-gray-500">Loading...</p>
        </div>
      </header>
    );
  }

  if (error) {
    return (
      <header className="bg-white shadow">
        <div className="container mx-auto px-4 py-4">
          <p className="text-red-500">Error loading dealership information</p>
        </div>
      </header>
    );
  }

  return (
    <header className="shadow" style={{ backgroundColor: themeColor }}>
      <div className="container mx-auto px-4 py-4">
        <div className="flex flex-col md:flex-row justify-center items-center gap-4">
          {/* Dealership branding */}
          <Link to="/" className="flex items-center gap-2 md:gap-4 hover:opacity-80 transition">
            {dealership?.logoUrl && (
              <img
                src={dealership.logoUrl}
                alt={`${dealership.name} logo`}
                className="h-10 md:h-12 object-contain"
              />
            )}
            <h1 className="text-xl md:text-2xl font-bold whitespace-nowrap" style={{ color: 'var(--secondary-theme-color)' }}>
              {dealership?.name || 'Dealership'}
            </h1>
          </Link>

          {/* Desktop Navigation (hidden on mobile) */}
          <nav className="hidden md:flex gap-6">
            {enabledNavItems.map(item => (
              <NavigationButton
                key={item.id}
                label={item.label}
                route={item.route}
                icon={item.icon}
                showIcon={item.showIcon !== false}
                isMobile={false}
              />
            ))}
          </nav>

          {/* Mobile Hamburger Menu Button (visible on mobile only) */}
          <button
            className="md:hidden focus:outline-none focus:ring-2 rounded p-2 absolute top-4 right-4"
            style={{ color: 'var(--secondary-theme-color)' }}
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            aria-label="Toggle menu"
            aria-expanded={mobileMenuOpen}
          >
            {/* Hamburger icon (3 horizontal lines) */}
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
            </svg>
          </button>
        </div>

        {/* Mobile Navigation Menu (shown when mobileMenuOpen is true) */}
        {mobileMenuOpen && (
          <nav className="md:hidden mt-4 pb-2 border-t border-opacity-30 pt-4" style={{ borderColor: 'var(--secondary-theme-color)' }}>
            {enabledNavItems.map(item => (
              <div key={item.id} onClick={() => setMobileMenuOpen(false)}>
                <NavigationButton
                  label={item.label}
                  route={item.route}
                  icon={item.icon}
                  showIcon={item.showIcon !== false}
                  isMobile={true}
                />
              </div>
            ))}
          </nav>
        )}
      </div>
    </header>
  );
}

export default Header;
