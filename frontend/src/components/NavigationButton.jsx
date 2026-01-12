/**
 * @component NavigationButton
 * @fileoverview Navigation button component with icon and text.
 * Used in public header for both desktop and mobile navigation.
 *
 * Story: 5.3 - Public Header Navigation Button Components
 *
 * Features:
 * - Displays icon + text horizontally
 * - Applies theme color styling
 * - Responsive design (desktop/mobile variants)
 * - Accessible (keyboard navigation, screen readers)
 * - Hover effects with darker theme shade
 *
 * @param {Object} props
 * @param {string} props.label - Button text label
 * @param {string} props.route - Route path for Link component
 * @param {string} props.icon - Icon name (e.g., 'FaHome')
 * @param {boolean} [props.showIcon=true] - Whether to display the icon (text always shows)
 * @param {boolean} [props.isMobile=false] - Mobile styling variant
 *
 * @example
 * <NavigationButton
 *   label="Home"
 *   route="/"
 *   icon="FaHome"
 *   isMobile={false}
 * />
 */

import { Link } from 'react-router-dom';
import { getIconComponent } from '../utils/iconMapper';

function NavigationButton({ label, route, icon, showIcon = true, isMobile = false }) {
  const IconComponent = getIconComponent(icon);

  // Base classes for both variants
  const baseClasses = 'flex items-center gap-2 transition font-medium focus:outline-none focus:ring-2 focus:ring-opacity-50 whitespace-nowrap';

  // Desktop variant: horizontal layout with hover effect
  const desktopClasses = 'py-2 px-3 rounded hover:bg-white hover:bg-opacity-10';

  // Mobile variant: full-width block with padding
  const mobileClasses = 'py-3 px-2 rounded hover:bg-white hover:bg-opacity-20 w-full';

  const classes = `${baseClasses} ${isMobile ? mobileClasses : desktopClasses}`;

  return (
    <Link
      to={route}
      className={classes}
      style={{ color: 'var(--secondary-theme-color)' }}
      aria-label={label}
    >
      {showIcon && <IconComponent className={isMobile ? 'w-5 h-5' : 'w-4 h-4'} aria-hidden="true" />}
      <span>{label}</span>
    </Link>
  );
}

export default NavigationButton;
