/**
 * @component NavigationManager
 * @fileoverview Main navigation management component for admin CMS.
 * Allows adding, editing, reordering, and deleting navigation items with live preview.
 *
 * Story: 5.2 - Navigation Manager Admin CMS UI
 *
 * @param {Object} props
 * @param {Object} props.dealership - Current dealership object
 * @param {function} props.onSave - Callback after successful save
 */

import { useState } from 'react';
import { DragDropContext, Droppable } from '@hello-pangea/dnd';
import DraggableNavItem from './DraggableNavItem';
import NavigationButton from '../NavigationButton';
import { defaultNavigation } from '../../utils/defaultNavigation';
import { FaPlus } from 'react-icons/fa';
import apiRequest from '../../utils/api';

function NavigationManager({ dealership, onSave }) {
  const [navItems, setNavItems] = useState(() => {
    try {
      // Try to parse navigationConfig (camelCase from .NET)
      if (dealership.navigationConfig) {
        return JSON.parse(dealership.navigationConfig);
      }
      // Fallback to snake_case for backwards compatibility
      if (dealership.navigation_config) {
        return typeof dealership.navigation_config === 'string' 
          ? JSON.parse(dealership.navigation_config)
          : dealership.navigation_config;
      }
      return defaultNavigation;
    } catch (e) {
      console.error('Error parsing navigation config:', e);
      return defaultNavigation;
    }
  });
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState(null);

  /**
   * Handles drag end event - reorders navigation items.
   */
  const handleDragEnd = (result) => {
    if (!result.destination) return;

    const items = Array.from(navItems);
    const [reorderedItem] = items.splice(result.source.index, 1);
    items.splice(result.destination.index, 0, reorderedItem);

    // Update order field for all items
    const updatedItems = items.map((item, index) => ({
      ...item,
      order: index + 1
    }));

    setNavItems(updatedItems);
  };

  /**
   * Updates a navigation item.
   */
  const handleUpdate = (index, updates) => {
    const updatedItems = [...navItems];
    updatedItems[index] = { ...updatedItems[index], ...updates };
    setNavItems(updatedItems);
  };

  /**
   * Deletes a navigation item.
   */
  const handleDelete = (index) => {
    const updatedItems = navItems.filter((_, i) => i !== index);
    // Re-number order after deletion
    const reorderedItems = updatedItems.map((item, i) => ({
      ...item,
      order: i + 1
    }));
    setNavItems(reorderedItems);
  };

  /**
   * Adds a new navigation item with default values.
   */
  const handleAddItem = () => {
    const newItem = {
      id: `nav-${Date.now()}`, // Unique ID using timestamp
      label: 'New Link',
      route: '/',
      icon: 'FaCircle',
      order: navItems.length + 1,
      enabled: true,
      showIcon: true
    };
    setNavItems([...navItems, newItem]);
  };

  /**
   * Resets navigation to default configuration.
   */
  const handleResetToDefaults = () => {
    if (window.confirm('Reset navigation to default configuration? This will discard all customizations.')) {
      setNavItems(defaultNavigation);
      setMessage({ type: 'info', text: 'Navigation reset to defaults. Click Save to apply changes.' });
      setTimeout(() => setMessage(null), 3000);
    }
  };

  /**
   * Validates navigation config before saving.
   */
  const validateNavigation = () => {
    // Check for empty labels
    const hasEmptyLabels = navItems.some(item => !item.label || item.label.trim() === '');
    if (hasEmptyLabels) {
      return 'All navigation items must have a label';
    }

    // Check for duplicate IDs
    const ids = navItems.map(item => item.id);
    const uniqueIds = new Set(ids);
    if (ids.length !== uniqueIds.size) {
      return 'Duplicate item IDs detected';
    }

    // Check for invalid routes
    const hasInvalidRoutes = navItems.some(item => !item.route || item.route.trim() === '');
    if (hasInvalidRoutes) {
      return 'All navigation items must have a route';
    }

    return null;
  };

  /**
   * Saves navigation configuration to API.
   */
  const handleSave = async () => {
    // Validate before saving
    const validationError = validateNavigation();
    if (validationError) {
      setMessage({ type: 'error', text: validationError });
      setTimeout(() => setMessage(null), 5000);
      return;
    }

    setIsSaving(true);
    setMessage(null);

    try {
      const response = await apiRequest(`/api/dealers/${dealership.id}`, {
        method: 'PUT',
        body: JSON.stringify({ navigationConfig: JSON.stringify(navItems) })
      });

      if (response.ok) {
        setMessage({ type: 'success', text: 'Navigation settings saved successfully!' });
        setTimeout(() => setMessage(null), 3000);
        if (onSave) onSave(); // Refresh dealership data
      } else {
        const error = await response.json();
        setMessage({ type: 'error', text: error.error || 'Failed to save navigation settings' });
        setTimeout(() => setMessage(null), 5000);
      }
    } catch (error) {
      console.error('Error saving navigation:', error);
      setMessage({ type: 'error', text: 'Network error. Please try again.' });
      setTimeout(() => setMessage(null), 5000);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* Message display */}
      {message && (
        <div
          className={`p-4 rounded ${
            message.type === 'success'
              ? 'bg-green-100 text-green-800'
              : message.type === 'error'
              ? 'bg-red-100 text-red-800'
              : 'bg-blue-100 text-blue-800'
          }`}
        >
          {message.text}
        </div>
      )}

      {/* Desktop Navigation Preview - Full Width at Top */}
      <div className="bg-gray-100 rounded-lg p-6">
        <h3 className="text-lg font-bold mb-4">Desktop Navigation Preview</h3>
        <div
          className="rounded-lg p-4 shadow overflow-x-auto"
          style={{ backgroundColor: dealership.themeColor || '#3B82F6' }}
        >
          <nav className="flex gap-6 flex-wrap">
            {navItems.filter(item => item.enabled).map(item => (
              <div key={item.id} onClick={(e) => e.preventDefault()}>
                <NavigationButton
                  label={item.label}
                  route={item.route}
                  icon={item.icon}
                  showIcon={item.showIcon !== false}
                  isMobile={false}
                />
              </div>
            ))}
          </nav>
          {navItems.filter(item => item.enabled).length === 0 && (
            <p className="text-center text-white py-4">No enabled navigation items</p>
          )}
        </div>
      </div>

      {/* Bottom Section: Navigation Items (Left) + Mobile Preview (Right) */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Left column: Navigation items list */}
        <div>
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-lg font-bold">Navigation Items</h3>
            <button
              onClick={handleAddItem}
              className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition"
            >
              <FaPlus /> Add Item
            </button>
          </div>

          <DragDropContext onDragEnd={handleDragEnd}>
            <Droppable droppableId="navigation-items">
              {(provided) => (
                <div {...provided.droppableProps} ref={provided.innerRef}>
                  {navItems.map((item, index) => (
                    <DraggableNavItem
                      key={item.id}
                      item={item}
                      index={index}
                      onUpdate={handleUpdate}
                      onDelete={handleDelete}
                    />
                  ))}
                  {provided.placeholder}
                </div>
              )}
            </Droppable>
          </DragDropContext>

          {navItems.length === 0 && (
            <p className="text-center text-gray-500 py-8">No navigation items. Click "Add Item" to create one.</p>
          )}
        </div>

        {/* Right column: Mobile Navigation Preview */}
        <div>
          <div className="bg-gray-100 rounded-lg p-6">
            <h3 className="text-lg font-bold mb-4">Mobile Navigation Preview</h3>
            <div
              className="rounded-lg p-4 shadow max-w-xs mx-auto"
              style={{ backgroundColor: dealership.themeColor || '#3B82F6' }}
            >
              <nav className="space-y-1">
                {navItems.filter(item => item.enabled).map(item => (
                  <div key={item.id} onClick={(e) => e.preventDefault()}>
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
              {navItems.filter(item => item.enabled).length === 0 && (
                <p className="text-center text-white py-4">No enabled navigation items</p>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Action buttons */}
      <div className="flex gap-3 justify-end border-t pt-4">
        <button
          onClick={handleResetToDefaults}
          className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50 transition"
          disabled={isSaving}
        >
          Reset to Defaults
        </button>
        <button
          onClick={handleSave}
          className="px-6 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition disabled:bg-gray-400 disabled:cursor-not-allowed"
          disabled={isSaving}
        >
          {isSaving ? 'Saving...' : 'Save Navigation'}
        </button>
      </div>
    </div>
  );
}

export default NavigationManager;
