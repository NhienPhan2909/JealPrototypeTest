/**
 * @fileoverview Shared layout wrapper component for public pages.
 * Provides consistent header and footer across all public routes.
 * Sets CSS custom properties for theme color to enable dynamic theming.
 */

import { useEffect } from 'react';
import { Outlet } from 'react-router-dom';
import Header from './Header';
import Footer from './Footer';
import useDealership from '../hooks/useDealership';
import { useDealershipContext } from '../context/DealershipContext';

/**
 * Layout - Shared layout wrapper component with header and footer.
 *
 * @component
 *
 * Wraps all public pages with consistent header (navigation) and footer.
 * Dynamically sets CSS custom properties based on dealership theme color.
 * Child routes render in the Outlet component.
 *
 * @example
 * <Layout />
 */
function Layout() {
  const { currentDealershipId } = useDealershipContext();
  const { dealership } = useDealership(currentDealershipId);

  // Set CSS custom properties for theme color
  useEffect(() => {
    if (dealership?.theme_color) {
      document.documentElement.style.setProperty('--theme-color', dealership.theme_color);

      // Calculate lighter and darker shades for hover states
      const hex = dealership.theme_color.replace('#', '');
      const r = parseInt(hex.substr(0, 2), 16);
      const g = parseInt(hex.substr(2, 2), 16);
      const b = parseInt(hex.substr(4, 2), 16);

      // Darker shade for hover (reduce brightness by 15%)
      const darkerR = Math.max(0, Math.floor(r * 0.85));
      const darkerG = Math.max(0, Math.floor(g * 0.85));
      const darkerB = Math.max(0, Math.floor(b * 0.85));
      const darkerColor = `#${darkerR.toString(16).padStart(2, '0')}${darkerG.toString(16).padStart(2, '0')}${darkerB.toString(16).padStart(2, '0')}`;

      document.documentElement.style.setProperty('--theme-color-dark', darkerColor);

      // Lighter shade for backgrounds (increase brightness by 90%)
      const lighterR = Math.min(255, Math.floor(r + (255 - r) * 0.9));
      const lighterG = Math.min(255, Math.floor(g + (255 - g) * 0.9));
      const lighterB = Math.min(255, Math.floor(b + (255 - b) * 0.9));
      const lighterColor = `#${lighterR.toString(16).padStart(2, '0')}${lighterG.toString(16).padStart(2, '0')}${lighterB.toString(16).padStart(2, '0')}`;

      document.documentElement.style.setProperty('--theme-color-light', lighterColor);
    }

    // Set secondary theme color and its variations
    if (dealership?.secondary_theme_color) {
      document.documentElement.style.setProperty('--secondary-theme-color', dealership.secondary_theme_color);

      // Calculate lighter and darker shades for secondary color
      const hex = dealership.secondary_theme_color.replace('#', '');
      const r = parseInt(hex.substr(0, 2), 16);
      const g = parseInt(hex.substr(2, 2), 16);
      const b = parseInt(hex.substr(4, 2), 16);

      // Darker shade for hover (reduce brightness by 15%)
      const darkerR = Math.max(0, Math.floor(r * 0.85));
      const darkerG = Math.max(0, Math.floor(g * 0.85));
      const darkerB = Math.max(0, Math.floor(b * 0.85));
      const darkerColor = `#${darkerR.toString(16).padStart(2, '0')}${darkerG.toString(16).padStart(2, '0')}${darkerB.toString(16).padStart(2, '0')}`;

      document.documentElement.style.setProperty('--secondary-theme-color-dark', darkerColor);

      // Lighter shade for backgrounds (increase brightness by 90%)
      const lighterR = Math.min(255, Math.floor(r + (255 - r) * 0.9));
      const lighterG = Math.min(255, Math.floor(g + (255 - g) * 0.9));
      const lighterB = Math.min(255, Math.floor(b + (255 - b) * 0.9));
      const lighterColor = `#${lighterR.toString(16).padStart(2, '0')}${lighterG.toString(16).padStart(2, '0')}${lighterB.toString(16).padStart(2, '0')}`;

      document.documentElement.style.setProperty('--secondary-theme-color-light', lighterColor);
    } else {
      // Set default secondary color if not specified
      document.documentElement.style.setProperty('--secondary-theme-color', '#FFFFFF');
      document.documentElement.style.setProperty('--secondary-theme-color-dark', '#E5E5E5');
      document.documentElement.style.setProperty('--secondary-theme-color-light', '#FFFFFF');
    }

    // Set body background color
    if (dealership?.body_background_color) {
      document.documentElement.style.setProperty('--body-background-color', dealership.body_background_color);
      document.body.style.backgroundColor = dealership.body_background_color;
    } else {
      // Set default background color if not specified
      document.documentElement.style.setProperty('--body-background-color', '#FFFFFF');
      document.body.style.backgroundColor = '#FFFFFF';
    }

    // Set font family
    if (dealership?.font_family) {
      const fontMapping = {
        system: '-apple-system, BlinkMacSystemFont, "Segoe UI", "Roboto", "Oxygen", "Ubuntu", "Cantarell", "Fira Sans", "Droid Sans", "Helvetica Neue", sans-serif',
        arial: 'Arial, Helvetica, sans-serif',
        times: '"Times New Roman", Times, serif',
        georgia: 'Georgia, serif',
        verdana: 'Verdana, Geneva, sans-serif',
        courier: '"Courier New", Courier, monospace',
        'comic-sans': '"Comic Sans MS", cursive, sans-serif',
        trebuchet: '"Trebuchet MS", Helvetica, sans-serif',
        impact: 'Impact, Charcoal, sans-serif',
        palatino: '"Palatino Linotype", "Book Antiqua", Palatino, serif'
      };

      const fontFamily = fontMapping[dealership.font_family] || fontMapping.system;
      document.body.style.fontFamily = fontFamily;
    }
  }, [dealership?.theme_color, dealership?.secondary_theme_color, dealership?.body_background_color, dealership?.font_family]);

  return (
    <div className="min-h-screen flex flex-col">
      <Header />

      <main className="flex-grow">
        <Outlet />
      </main>

      <Footer />
    </div>
  );
}

export default Layout;
