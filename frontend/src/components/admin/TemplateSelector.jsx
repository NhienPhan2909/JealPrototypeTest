/**
 * TemplateSelector - Component for selecting and managing design templates.
 * 
 * @component
 * @description Allows dealership admins to:
 * - Select from pre-set design templates
 * - Apply templates to their dealership
 * - Save current design settings as a custom template
 * - Delete custom templates
 * 
 * Features:
 * - Displays preset and custom templates in separate sections
 * - Shows template preview cards with colors and font
 * - One-click template application
 * - Template creation from current settings
 * - Template deletion (custom only)
 * 
 * @example
 * <TemplateSelector
 *   currentSettings={{ themeColor: '#3B82F6', ... }}
 *   onApplyTemplate={(template) => applyTemplate(template)}
 * />
 */

import { useState, useEffect, useContext } from 'react';
import { AdminContext } from '../../context/AdminContext';
import apiRequest from '../../utils/api';

function TemplateSelector({ currentSettings, onApplyTemplate }) {
  const { selectedDealership } = useContext(AdminContext);
  const [templates, setTemplates] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  const [showSaveDialog, setShowSaveDialog] = useState(false);
  const [newTemplateName, setNewTemplateName] = useState('');
  const [newTemplateDescription, setNewTemplateDescription] = useState('');

  /**
   * Font display names mapping
   */
  const fontDisplayNames = {
    'system': 'System Default',
    'times': 'Times New Roman',
    'arial': 'Arial',
    'courier': 'Courier New',
    'verdana': 'Verdana',
    'georgia': 'Georgia',
    'inter': 'Inter',
    'roboto': 'Roboto',
    'opensans': 'Open Sans',
    'lato': 'Lato',
    'montserrat': 'Montserrat',
    'poppins': 'Poppins',
    'playfair': 'Playfair Display',
    'nunito': 'Nunito'
  };

  /**
   * Fetch templates on component mount
   */
  useEffect(() => {
    fetchTemplates();
  }, [selectedDealership]);

  /**
   * Fetches all available templates for the dealership
   */
  const fetchTemplates = async () => {
    if (!selectedDealership) return;

    setLoading(true);
    setError('');

    try {
      // Pass dealershipId as query param for admin users
      const url = `/api/design-templates?dealershipId=${selectedDealership.id}`;
      
      const response = await apiRequest(url);

      console.log('Design Templates API Response:', {
        status: response.status,
        statusText: response.statusText,
        ok: response.ok
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ error: 'Unknown error' }));
        console.error('API Error Response:', errorData);
        throw new Error(errorData.error || 'Failed to fetch templates');
      }

      const data = await response.json();
      console.log('Templates loaded:', data.length, 'templates');
      setTemplates(data);
    } catch (err) {
      console.error('Error fetching templates:', err);
      setError('Failed to load templates. Please refresh the page.');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Handles template application
   */
  const handleApplyTemplate = (template) => {
    onApplyTemplate({
      themeColor: template.themeColor,
      secondaryThemeColor: template.secondaryThemeColor,
      bodyBackgroundColor: template.bodyBackgroundColor,
      fontFamily: template.fontFamily
    });

    setSuccessMessage(`Template "${template.name}" applied successfully!`);
    setTimeout(() => setSuccessMessage(''), 5000);
  };

  /**
   * Handles saving current settings as a new template
   */
  const handleSaveAsTemplate = async (e) => {
    e.preventDefault();

    if (!newTemplateName.trim()) {
      setError('Please enter a template name');
      return;
    }

    setLoading(true);
    setError('');

    try {
      const response = await apiRequest('/api/design-templates', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          dealershipId: selectedDealership.id,
          name: newTemplateName.trim(),
          description: newTemplateDescription.trim() || undefined,
          themeColor: currentSettings.themeColor,
          secondaryThemeColor: currentSettings.secondaryThemeColor,
          bodyBackgroundColor: currentSettings.bodyBackgroundColor,
          fontFamily: currentSettings.fontFamily
        })
      });

      if (!response.ok) {
        let errorMessage = 'Failed to save template';
        try {
          const errorData = await response.json();
          errorMessage = errorData.message || errorData.error || errorMessage;
        } catch {
          // No JSON body (like 401)
          if (response.status === 401) {
            errorMessage = 'Unauthorized. Please log in again.';
          }
        }
        throw new Error(errorMessage);
      }

      const newTemplate = await response.json();
      setTemplates([...templates, newTemplate]);
      setSuccessMessage('Template saved successfully!');
      setShowSaveDialog(false);
      setNewTemplateName('');
      setNewTemplateDescription('');

      setTimeout(() => setSuccessMessage(''), 5000);
    } catch (err) {
      console.error('Error saving template:', err);
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  /**
   * Handles template deletion
   */
  const handleDeleteTemplate = async (templateId, templateName) => {
    if (!confirm(`Are you sure you want to delete the template "${templateName}"?`)) {
      return;
    }

    setLoading(true);
    setError('');

    try {
      // Include dealership_id for admin users
      const url = `/api/design-templates/${templateId}?dealershipId=${selectedDealership.id}`;
      
      const response = await apiRequest(url, {
        method: 'DELETE'
      });

      if (!response.ok) {
        let errorMessage = 'Failed to delete template';
        try {
          const errorData = await response.json();
          errorMessage = errorData.message || errorData.error || errorMessage;
        } catch {
          if (response.status === 401) {
            errorMessage = 'Unauthorized. Please log in again.';
          }
        }
        throw new Error(errorMessage);
      }

      setTemplates(templates.filter(t => t.id !== templateId));
      setSuccessMessage('Template deleted successfully!');

      setTimeout(() => setSuccessMessage(''), 5000);
    } catch (err) {
      console.error('Error deleting template:', err);
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const presetTemplates = templates.filter(t => t.isPreset);
  const customTemplates = templates.filter(t => !t.isPreset);

  return (
    <div className="bg-white rounded-lg shadow-md p-6 mb-6">
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Design Templates</h2>
        <button
          onClick={() => setShowSaveDialog(true)}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
        >
          Save Current as Template
        </button>
      </div>

      {error && (
        <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-md">
          <p className="text-red-800">{error}</p>
        </div>
      )}

      {successMessage && (
        <div className="mb-4 p-4 bg-green-50 border border-green-200 rounded-md">
          <p className="text-green-800">{successMessage}</p>
        </div>
      )}

      {/* Save Template Dialog */}
      {showSaveDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <h3 className="text-xl font-bold mb-4">Save as New Template</h3>
            <form onSubmit={handleSaveAsTemplate}>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Template Name *
                </label>
                <input
                  type="text"
                  value={newTemplateName}
                  onChange={(e) => setNewTemplateName(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="My Custom Theme"
                  required
                  maxLength={100}
                />
              </div>
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Description (Optional)
                </label>
                <textarea
                  value={newTemplateDescription}
                  onChange={(e) => setNewTemplateDescription(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="Describe this template..."
                  rows={3}
                  maxLength={500}
                />
              </div>
              <div className="flex justify-end gap-3">
                <button
                  type="button"
                  onClick={() => {
                    setShowSaveDialog(false);
                    setNewTemplateName('');
                    setNewTemplateDescription('');
                    setError('');
                  }}
                  className="px-4 py-2 text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200"
                  disabled={loading}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50"
                  disabled={loading}
                >
                  {loading ? 'Saving...' : 'Save Template'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Preset Templates */}
      <div className="mb-8">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Pre-set Templates</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
          {presetTemplates.map((template) => (
            <div
              key={template.id}
              className="border border-gray-200 rounded-lg p-4 hover:border-blue-500 transition-colors"
            >
              <h4 className="font-semibold text-gray-900 mb-2">{template.name}</h4>
              {template.description && (
                <p className="text-sm text-gray-600 mb-3">{template.description}</p>
              )}
              
              {/* Color Preview */}
              <div className="flex gap-2 mb-3">
                <div
                  className="w-8 h-8 rounded border border-gray-300"
                  style={{ backgroundColor: template.themeColor }}
                  title={`Primary: ${template.themeColor}`}
                />
                <div
                  className="w-8 h-8 rounded border border-gray-300"
                  style={{ backgroundColor: template.secondaryThemeColor }}
                  title={`Secondary: ${template.secondaryThemeColor}`}
                />
                <div
                  className="w-8 h-8 rounded border border-gray-300"
                  style={{ backgroundColor: template.bodyBackgroundColor }}
                  title={`Background: ${template.bodyBackgroundColor}`}
                />
              </div>

              <p className="text-xs text-gray-500 mb-3">
                Font: {fontDisplayNames[template.fontFamily] || template.fontFamily}
              </p>

              <button
                onClick={() => handleApplyTemplate(template)}
                className="w-full px-3 py-2 bg-blue-600 text-white text-sm rounded-md hover:bg-blue-700 transition-colors"
                disabled={loading}
              >
                Apply Template
              </button>
            </div>
          ))}
        </div>
      </div>

      {/* Custom Templates */}
      {customTemplates.length > 0 && (
        <div>
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Your Custom Templates</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            {customTemplates.map((template) => (
              <div
                key={template.id}
                className="border border-gray-200 rounded-lg p-4 hover:border-blue-500 transition-colors"
              >
                <h4 className="font-semibold text-gray-900 mb-2">{template.name}</h4>
                {template.description && (
                  <p className="text-sm text-gray-600 mb-3">{template.description}</p>
                )}
                
                {/* Color Preview */}
                <div className="flex gap-2 mb-3">
                  <div
                    className="w-8 h-8 rounded border border-gray-300"
                    style={{ backgroundColor: template.themeColor }}
                    title={`Primary: ${template.themeColor}`}
                  />
                  <div
                    className="w-8 h-8 rounded border border-gray-300"
                    style={{ backgroundColor: template.secondaryThemeColor }}
                    title={`Secondary: ${template.secondaryThemeColor}`}
                  />
                  <div
                    className="w-8 h-8 rounded border border-gray-300"
                    style={{ backgroundColor: template.bodyBackgroundColor }}
                    title={`Background: ${template.bodyBackgroundColor}`}
                  />
                </div>

                <p className="text-xs text-gray-500 mb-3">
                  Font: {fontDisplayNames[template.fontFamily] || template.fontFamily}
                </p>

                <div className="flex gap-2">
                  <button
                    onClick={() => handleApplyTemplate(template)}
                    className="flex-1 px-3 py-2 bg-blue-600 text-white text-sm rounded-md hover:bg-blue-700 transition-colors"
                    disabled={loading}
                  >
                    Apply
                  </button>
                  <button
                    onClick={() => handleDeleteTemplate(template.id, template.name)}
                    className="px-3 py-2 bg-red-600 text-white text-sm rounded-md hover:bg-red-700 transition-colors"
                    disabled={loading}
                  >
                    Delete
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {loading && templates.length === 0 && (
        <div className="text-center py-8">
          <p className="text-gray-500">Loading templates...</p>
        </div>
      )}
    </div>
  );
}

export default TemplateSelector;
