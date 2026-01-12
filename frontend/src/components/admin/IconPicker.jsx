/**
 * @component IconPicker
 * @fileoverview Icon selection component with search functionality for navigation manager.
 * Displays grid of popular icons and allows searching/selecting.
 *
 * Story: 5.2 - Navigation Manager Admin CMS UI
 *
 * @param {Object} props
 * @param {string} props.currentIcon - Currently selected icon name
 * @param {function} props.onSelect - Callback when icon is selected (iconName) => void
 * @param {boolean} [props.isOpen=false] - Whether picker is open
 * @param {function} props.onClose - Callback to close picker
 */

import { useState } from 'react';
import { getIconComponent, getAvailableIcons } from '../../utils/iconMapper';

function IconPicker({ currentIcon, onSelect, isOpen, onClose }) {
  const [searchTerm, setSearchTerm] = useState('');

  if (!isOpen) return null;

  // Get all available icons from iconMapper
  const allIcons = getAvailableIcons();

  // Filter icons based on search term
  const filteredIcons = allIcons.filter(iconName =>
    iconName.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleSelect = (iconName) => {
    onSelect(iconName);
    setSearchTerm('');
    onClose();
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50" onClick={onClose}>
      <div
        className="bg-white rounded-lg p-6 max-w-2xl w-full max-h-[80vh] overflow-auto"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-lg font-bold">Select Icon</h3>
          <button
            onClick={onClose}
            className="text-gray-500 hover:text-gray-700 text-2xl leading-none"
            aria-label="Close"
          >
            ×
          </button>
        </div>

        {/* Search input */}
        <input
          type="text"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          placeholder="Search icons..."
          className="input-field mb-4"
          autoFocus
        />

        {/* Icon grid */}
        <div className="grid grid-cols-4 sm:grid-cols-6 md:grid-cols-8 gap-3">
          {filteredIcons.map(iconName => {
            const IconComponent = getIconComponent(iconName);
            const isSelected = iconName === currentIcon;

            return (
              <button
                key={iconName}
                onClick={() => handleSelect(iconName)}
                className={`p-3 rounded border-2 hover:bg-blue-50 transition flex items-center justify-center ${
                  isSelected ? 'border-blue-500 bg-blue-100' : 'border-gray-300'
                }`}
                title={iconName}
              >
                <IconComponent className="w-6 h-6 text-gray-700" />
              </button>
            );
          })}
        </div>

        {filteredIcons.length === 0 && (
          <p className="text-center text-gray-500 py-8">No icons found for "{searchTerm}"</p>
        )}
      </div>
    </div>
  );
}

export default IconPicker;
