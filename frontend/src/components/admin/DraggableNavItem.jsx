/**
 * @component DraggableNavItem
 * @fileoverview Draggable navigation item for admin navigation manager.
 * Allows editing label, icon, enabled state, and deletion.
 *
 * Story: 5.2 - Navigation Manager Admin CMS UI
 *
 * @param {Object} props
 * @param {Object} props.item - Navigation item object {id, label, route, icon, order, enabled, showIcon}
 * @param {number} props.index - Index in the list
 * @param {function} props.onUpdate - Callback when item is updated (index, updates) => void
 * @param {function} props.onDelete - Callback when item is deleted (index) => void
 */

import { useState } from 'react';
import { Draggable } from '@hello-pangea/dnd';
import { getIconComponent } from '../../utils/iconMapper';
import IconPicker from './IconPicker';
import { FaGripVertical, FaTrash } from 'react-icons/fa';

function DraggableNavItem({ item, index, onUpdate, onDelete }) {
  const [isIconPickerOpen, setIsIconPickerOpen] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  const IconComponent = getIconComponent(item.icon);

  const handleLabelChange = (e) => {
    onUpdate(index, { label: e.target.value });
  };

  const handleRouteChange = (e) => {
    onUpdate(index, { route: e.target.value });
  };

  const handleIconSelect = (iconName) => {
    onUpdate(index, { icon: iconName });
  };

  const handleEnabledToggle = () => {
    onUpdate(index, { enabled: !item.enabled });
  };

  const handleShowIconToggle = () => {
    onUpdate(index, { showIcon: !(item.showIcon !== false) });
  };

  const handleDeleteClick = () => {
    setShowDeleteConfirm(true);
  };

  const handleDeleteConfirm = () => {
    onDelete(index);
    setShowDeleteConfirm(false);
  };

  return (
    <Draggable draggableId={item.id} index={index}>
      {(provided, snapshot) => (
        <div
          ref={provided.innerRef}
          {...provided.draggableProps}
          className={`bg-white border rounded-lg p-4 mb-3 ${
            snapshot.isDragging ? 'shadow-lg opacity-90' : 'shadow'
          }`}
        >
          <div className="flex items-center gap-3">
            {/* Drag handle */}
            <div {...provided.dragHandleProps} className="cursor-grab active:cursor-grabbing text-gray-400 hover:text-gray-600">
              <FaGripVertical className="w-5 h-5" />
            </div>

            {/* Icon preview and picker button */}
            <button
              type="button"
              onClick={() => setIsIconPickerOpen(true)}
              className="p-2 border-2 border-gray-300 rounded hover:border-blue-500 hover:bg-blue-50 transition"
              title="Change icon"
            >
              <IconComponent className="w-6 h-6 text-gray-700" />
            </button>

            {/* Label input */}
            <div className="flex-1">
              <label className="block text-xs text-gray-500 mb-1">Label</label>
              <input
                type="text"
                value={item.label}
                onChange={handleLabelChange}
                className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Label"
              />
            </div>

            {/* Route input */}
            <div className="flex-1">
              <label className="block text-xs text-gray-500 mb-1">Route</label>
              <input
                type="text"
                value={item.route}
                onChange={handleRouteChange}
                className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Route"
              />
            </div>

            {/* Enabled toggle */}
            <div className="flex flex-col items-center">
              <label className="block text-xs text-gray-500 mb-1">Enabled</label>
              <label className="relative inline-block w-12 h-6">
                <input
                  type="checkbox"
                  checked={item.enabled}
                  onChange={handleEnabledToggle}
                  className="sr-only peer"
                />
                <div className="w-full h-full bg-gray-300 rounded-full peer peer-checked:bg-blue-600 transition"></div>
                <div className="absolute left-1 top-1 w-4 h-4 bg-white rounded-full transition peer-checked:translate-x-6"></div>
              </label>
            </div>

            {/* Show Icon toggle */}
            <div className="flex flex-col items-center">
              <label className="block text-xs text-gray-500 mb-1">Show Icon</label>
              <label className="relative inline-block w-12 h-6">
                <input
                  type="checkbox"
                  checked={item.showIcon !== false}
                  onChange={handleShowIconToggle}
                  className="sr-only peer"
                />
                <div className="w-full h-full bg-gray-300 rounded-full peer peer-checked:bg-blue-600 transition"></div>
                <div className="absolute left-1 top-1 w-4 h-4 bg-white rounded-full transition peer-checked:translate-x-6"></div>
              </label>
            </div>

            {/* Delete button */}
            <button
              type="button"
              onClick={handleDeleteClick}
              className="p-2 text-red-600 hover:bg-red-50 rounded transition"
              title="Delete item"
            >
              <FaTrash className="w-5 h-5" />
            </button>
          </div>

          {/* Icon Picker Modal */}
          <IconPicker
            currentIcon={item.icon}
            onSelect={handleIconSelect}
            isOpen={isIconPickerOpen}
            onClose={() => setIsIconPickerOpen(false)}
          />

          {/* Delete Confirmation Modal */}
          {showDeleteConfirm && (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50" onClick={() => setShowDeleteConfirm(false)}>
              <div
                className="bg-white rounded-lg p-6 max-w-sm"
                onClick={(e) => e.stopPropagation()}
              >
                <h3 className="text-lg font-bold mb-3">Delete Navigation Item?</h3>
                <p className="text-gray-600 mb-4">Are you sure you want to delete "{item.label}"?</p>
                <div className="flex gap-3 justify-end">
                  <button
                    onClick={() => setShowDeleteConfirm(false)}
                    className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50"
                  >
                    Cancel
                  </button>
                  <button
                    onClick={handleDeleteConfirm}
                    className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700"
                  >
                    Delete
                  </button>
                </div>
              </div>
            </div>
          )}
        </div>
      )}
    </Draggable>
  );
}

export default DraggableNavItem;
