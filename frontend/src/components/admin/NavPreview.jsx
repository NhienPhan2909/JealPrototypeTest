/**
 * @component NavPreview
 * @fileoverview Live preview of navigation buttons as they will appear on the public site.
 * Shows both desktop and mobile layouts with applied theme color.
 *
 * Story: 5.2 - Navigation Manager Admin CMS UI
 *
 * @param {Object} props
 * @param {Array<Object>} props.navItems - Navigation items to preview
 * @param {string} props.themeColor - Theme color to apply
 */

import NavigationButton from '../NavigationButton';

function NavPreview({ navItems, themeColor }) {
  // Filter enabled items for preview
  const enabledItems = navItems.filter(item => item.enabled);

  return (
    <div className="bg-gray-100 rounded-lg p-6">
      <h3 className="text-lg font-bold mb-4">Live Preview</h3>

      {/* Desktop Preview */}
      <div className="mb-6">
        <p className="text-sm text-gray-600 mb-2">Desktop Navigation:</p>
        <div
          className="rounded-lg p-4 shadow"
          style={{ backgroundColor: themeColor || '#3B82F6' }}
        >
          <nav className="flex gap-6 justify-center">
            {enabledItems.map(item => (
              <div key={item.id} onClick={(e) => e.preventDefault()}>
                <NavigationButton
                  label={item.label}
                  route={item.route}
                  icon={item.icon}
                  isMobile={false}
                />
              </div>
            ))}
          </nav>
        </div>
      </div>

      {/* Mobile Preview */}
      <div>
        <p className="text-sm text-gray-600 mb-2">Mobile Navigation:</p>
        <div
          className="rounded-lg p-4 shadow max-w-xs"
          style={{ backgroundColor: themeColor || '#3B82F6' }}
        >
          <nav className="space-y-1">
            {enabledItems.map(item => (
              <div key={item.id} onClick={(e) => e.preventDefault()}>
                <NavigationButton
                  label={item.label}
                  route={item.route}
                  icon={item.icon}
                  isMobile={true}
                />
              </div>
            ))}
          </nav>
        </div>
      </div>

      {enabledItems.length === 0 && (
        <p className="text-center text-gray-500 py-4">No enabled navigation items to preview</p>
      )}
    </div>
  );
}

export default NavPreview;
