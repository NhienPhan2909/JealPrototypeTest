/**
 * @fileoverview Default navigation configuration for dealership websites.
 * Used when dealership.navigation_config is null or invalid.
 * Matches the current hardcoded navigation in Header.jsx.
 *
 * Story: 5.1 - Navigation Configuration Database & Backend API
 */

/**
 * Default navigation menu configuration
 * @type {Array<Object>}
 */
const defaultNavigation = [
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
    id: 'sell-your-car',
    label: 'Sell Your Car',
    route: '/sell-your-car',
    icon: 'FaDollarSign',
    order: 7,
    enabled: true,
    showIcon: true
  },
  {
    id: 'login',
    label: 'Log In',
    route: '/admin/login',
    icon: 'FaSignInAlt',
    order: 8,
    enabled: true,
    showIcon: true
  }
];

module.exports = defaultNavigation;
