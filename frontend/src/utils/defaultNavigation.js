/**
 * @fileoverview Default navigation configuration for dealership websites (frontend).
 * Used when dealership.navigation_config is null or invalid.
 * Matches the backend default configuration in backend/config/defaultNavigation.js.
 *
 * Story: 5.3 - Public Header Navigation Button Components
 */

/**
 * Default navigation menu configuration.
 * This is the fallback used when a dealership hasn't customized their navigation.
 *
 * @type {Array<Object>}
 */
export const defaultNavigation = [
  {
    id: 'home',
    label: 'Home',
    route: '/',
    icon: 'FaHome',
    order: 1,
    enabled: true,
    showIcon: true
  },
  {
    id: 'inventory',
    label: 'Inventory',
    route: '/inventory',
    icon: 'FaCar',
    order: 2,
    enabled: true,
    showIcon: true
  },
  {
    id: 'about',
    label: 'About',
    route: '/about',
    icon: 'FaInfoCircle',
    order: 3,
    enabled: true,
    showIcon: true
  },
  {
    id: 'finance',
    label: 'Finance',
    route: '/finance',
    icon: 'FaMoneyBillWave',
    order: 4,
    enabled: true,
    showIcon: true
  },
  {
    id: 'warranty',
    label: 'Warranty',
    route: '/warranty',
    icon: 'FaShieldAlt',
    order: 5,
    enabled: true,
    showIcon: true
  },
  {
    id: 'location',
    label: 'Location',
    route: '/location',
    icon: 'FaMapMarkerAlt',
    order: 6,
    enabled: true,
    showIcon: true
  },
  {
    id: 'blog',
    label: 'Blog',
    route: '/blog',
    icon: 'FaNewspaper',
    order: 7,
    enabled: true,
    showIcon: true
  },
  {
    id: 'sell-your-car',
    label: 'Sell Your Car',
    route: '/sell-your-car',
    icon: 'FaDollarSign',
    order: 8,
    enabled: true,
    showIcon: true
  },
  {
    id: 'login',
    label: 'Log In',
    route: '/admin/login',
    icon: 'FaSignInAlt',
    order: 9,
    enabled: true,
    showIcon: true
  }
];

/**
 * Validates navigation config structure.
 * Returns true if navigation config is valid, false otherwise.
 *
 * @param {Array<Object>|null} navigationConfig - Navigation config to validate
 * @returns {boolean} True if valid, false if invalid
 */
export function isValidNavigationConfig(navigationConfig) {
  if (!navigationConfig) return false;
  if (!Array.isArray(navigationConfig)) return false;

  // Check if all items have required fields
  return navigationConfig.every(item =>
    item &&
    typeof item.id === 'string' &&
    typeof item.label === 'string' &&
    typeof item.route === 'string' &&
    typeof item.icon === 'string' &&
    typeof item.order === 'number' &&
    typeof item.enabled === 'boolean' &&
    (item.showIcon === undefined || typeof item.showIcon === 'boolean')
  );
}

/**
 * Gets validated navigation config with fallback to defaults.
 * Returns the provided navigation config if valid, otherwise returns default navigation.
 *
 * @param {Array<Object>|null} navigationConfig - Navigation config from API
 * @returns {Array<Object>} Valid navigation config or default
 */
export function getValidatedNavigation(navigationConfig) {
  return isValidNavigationConfig(navigationConfig) ? navigationConfig : defaultNavigation;
}
