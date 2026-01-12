/**
 * @fileoverview Icon mapper utility for converting icon name strings to React icon components.
 * Maps icon names (stored in database) to their corresponding react-icons components.
 *
 * Story: 5.3 - Public Header Navigation Button Components
 *
 * Usage:
 * import { getIconComponent } from './utils/iconMapper';
 * const IconComponent = getIconComponent('FaHome');
 * return <IconComponent className="w-5 h-5" />;
 */

import {
  FaHome,
  FaCar,
  FaInfoCircle,
  FaMoneyBillWave,
  FaShieldAlt,
  FaSignInAlt,
  FaCircle,
  FaPhone,
  FaEnvelope,
  FaMapMarkerAlt,
  FaStar,
  FaCog,
  FaUser,
  FaSearch,
  FaShoppingCart,
  FaHeart,
  FaTruck,
  FaTools,
  FaCalendar,
  FaFileAlt,
  FaQuestionCircle,
  FaDollarSign,
  FaNewspaper
} from 'react-icons/fa';

/**
 * Icon mapping object.
 * Maps icon name strings to their corresponding React icon components.
 */
const iconMap = {
  // Default navigation icons
  FaHome,
  FaCar,
  FaInfoCircle,
  FaMoneyBillWave,
  FaShieldAlt,
  FaSignInAlt,
  FaDollarSign,
  FaNewspaper,

  // Additional popular icons
  FaPhone,
  FaEnvelope,
  FaMapMarkerAlt,
  FaStar,
  FaCog,
  FaUser,
  FaSearch,
  FaShoppingCart,
  FaHeart,
  FaTruck,
  FaTools,
  FaCalendar,
  FaFileAlt,
  FaQuestionCircle,

  // Fallback icon
  FaCircle
};

/**
 * Gets a React icon component from an icon name string.
 * Returns fallback icon (FaCircle) if icon name not found in map.
 *
 * @param {string} iconName - The icon name (e.g., 'FaHome', 'FaCar')
 * @returns {React.Component} The icon component, or FaCircle if not found
 *
 * @example
 * const HomeIcon = getIconComponent('FaHome');
 * const FallbackIcon = getIconComponent('UnknownIcon'); // Returns FaCircle
 */
export function getIconComponent(iconName) {
  return iconMap[iconName] || FaCircle;
}

/**
 * Gets an array of all available icon names.
 * Useful for icon picker components.
 *
 * @returns {Array<string>} Array of icon name strings
 */
export function getAvailableIcons() {
  return Object.keys(iconMap).filter(name => name !== 'FaCircle');
}
