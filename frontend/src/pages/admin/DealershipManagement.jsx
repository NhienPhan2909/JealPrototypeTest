/**
 * @fileoverview Dealership Management Page - Admin interface for creating new dealerships.
 * Only accessible to System Administrators (user_type: 'admin').
 */

import { useState, useEffect, useContext } from 'react';
import { AdminContext } from '../../context/AdminContext';
import AdminHeader from '../../components/AdminHeader';
import { isAdmin } from '../../utils/permissions';

export default function DealershipManagement() {
  const { user } = useContext(AdminContext);
  const [dealerships, setDealerships] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  const [sortBy, setSortBy] = useState('id');
  const [sortOrder, setSortOrder] = useState('asc');

  // Form state
  const [formData, setFormData] = useState({
    name: '',
    address: '',
    phone: '',
    email: '',
    logo_url: '',
    hours: '',
    about: '',
    website_url: ''
  });

  useEffect(() => {
    fetchDealerships();
  }, []);

  const fetchDealerships = async () => {
    try {
      const response = await fetch('/api/dealers', { credentials: 'include' });
      if (response.ok) {
        const data = await response.json();
        setDealerships(data);
      } else {
        setError('Failed to load dealerships');
      }
    } catch (err) {
      console.error('Fetch dealerships error:', err);
      setError('Failed to load dealerships');
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateDealership = async (e) => {
    e.preventDefault();
    setError('');
    setSuccessMessage('');

    try {
      const response = await fetch('/api/dealers', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify(formData)
      });

      if (response.ok) {
        const newDealership = await response.json();
        await fetchDealerships();
        setShowCreateForm(false);
        resetForm();
        setSuccessMessage(`Successfully created dealership: ${newDealership.name}`);
      } else {
        const errorData = await response.json();
        setError(errorData.error || 'Failed to create dealership');
      }
    } catch (err) {
      console.error('Create dealership error:', err);
      setError('Failed to create dealership');
    }
  };

  const handleDeleteDealership = async (dealershipId, dealershipName) => {
    // Confirmation dialog with strong warning
    const confirmMessage = `⚠️ WARNING: This action is IRREVERSIBLE!\n\n` +
      `Deleting "${dealershipName}" will permanently delete:\n` +
      `• The dealership record\n` +
      `• ALL vehicles\n` +
      `• ALL leads and sales requests\n` +
      `• ALL blog posts\n` +
      `• ALL user accounts (owners and staff)\n\n` +
      `This data CANNOT be recovered.\n\n` +
      `Type the dealership name to confirm deletion:`;

    const userInput = prompt(confirmMessage);

    // Verify user typed exact dealership name
    if (userInput !== dealershipName) {
      if (userInput !== null) {
        setError('Deletion cancelled: Dealership name did not match');
      }
      return;
    }

    setError('');
    setSuccessMessage('');

    try {
      const response = await fetch(`/api/dealers/${dealershipId}`, {
        method: 'DELETE',
        credentials: 'include'
      });

      if (response.ok) {
        const data = await response.json();
        await fetchDealerships();
        setSuccessMessage(`Successfully deleted dealership: ${dealershipName}`);
      } else {
        const errorData = await response.json();
        setError(errorData.error || 'Failed to delete dealership');
      }
    } catch (err) {
      console.error('Delete dealership error:', err);
      setError('Failed to delete dealership');
    }
  };

  const resetForm = () => {
    setFormData({
      name: '',
      address: '',
      phone: '',
      email: '',
      logo_url: '',
      hours: '',
      about: '',
      website_url: ''
    });
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSort = (field) => {
    if (sortBy === field) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setSortBy(field);
      setSortOrder('asc');
    }
  };

  const sortedDealerships = [...dealerships].sort((a, b) => {
    let aValue = a[sortBy];
    let bValue = b[sortBy];

    if (sortBy === 'created_at') {
      aValue = new Date(aValue).getTime();
      bValue = new Date(bValue).getTime();
    } else if (sortBy === 'id') {
      aValue = Number(aValue);
      bValue = Number(bValue);
    } else {
      aValue = String(aValue || '').toLowerCase();
      bValue = String(bValue || '').toLowerCase();
    }

    if (sortOrder === 'asc') {
      return aValue > bValue ? 1 : aValue < bValue ? -1 : 0;
    } else {
      return aValue < bValue ? 1 : aValue > bValue ? -1 : 0;
    }
  });

  // Check if user is admin
  if (!isAdmin(user)) {
    return (
      <div className="min-h-screen bg-gray-100">
        <AdminHeader />
        <div className="max-w-7xl mx-auto px-4 py-8">
          <div className="bg-red-50 border border-red-200 rounded-lg p-4">
            <p className="text-red-600">Access Denied: Only System Administrators can manage dealerships.</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100">
      <AdminHeader />

      <div className="max-w-7xl mx-auto px-4 py-8">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-3xl font-bold text-gray-800">Dealership Management</h1>
          <button
            onClick={() => setShowCreateForm(true)}
            className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
          >
            + Create New Dealership
          </button>
        </div>

        {/* Success Message */}
        {successMessage && (
          <div className="bg-green-50 border border-green-200 rounded-lg p-4 mb-4">
            <p className="text-green-600">{successMessage}</p>
          </div>
        )}

        {/* Error Message */}
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-4">
            <p className="text-red-600">{error}</p>
          </div>
        )}

        {/* Create Form Modal */}
        {showCreateForm && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 max-w-2xl w-full max-h-[90vh] overflow-y-auto">
              <h2 className="text-2xl font-bold mb-4">Create New Dealership</h2>
              <form onSubmit={handleCreateDealership}>
                <div className="space-y-4">
                  {/* Name */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Dealership Name <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      name="name"
                      value={formData.name}
                      onChange={handleInputChange}
                      required
                      className="w-full border border-gray-300 rounded px-3 py-2"
                      placeholder="e.g., Acme Auto Sales"
                    />
                  </div>

                  {/* Address */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Address <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      name="address"
                      value={formData.address}
                      onChange={handleInputChange}
                      required
                      className="w-full border border-gray-300 rounded px-3 py-2"
                      placeholder="123 Main Street, City, State ZIP"
                    />
                  </div>

                  {/* Phone */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Phone <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="tel"
                      name="phone"
                      value={formData.phone}
                      onChange={handleInputChange}
                      required
                      className="w-full border border-gray-300 rounded px-3 py-2"
                      placeholder="(555) 123-4567"
                    />
                  </div>

                  {/* Email */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Email <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="email"
                      name="email"
                      value={formData.email}
                      onChange={handleInputChange}
                      required
                      className="w-full border border-gray-300 rounded px-3 py-2"
                      placeholder="contact@dealership.com"
                    />
                  </div>

                  {/* Website URL (Optional) */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Website URL (Optional)
                    </label>
                    <input
                      type="text"
                      name="website_url"
                      value={formData.website_url}
                      onChange={handleInputChange}
                      className="w-full border border-gray-300 rounded px-3 py-2"
                      placeholder="e.g., acme-auto.com"
                      maxLength={255}
                    />
                    <p className="text-xs text-gray-500 mt-1">
                      Custom URL/domain for this dealership. Must be unique.
                    </p>
                  </div>

                  {/* Logo URL (Optional) */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Logo URL (Optional)
                    </label>
                    <input
                      type="url"
                      name="logo_url"
                      value={formData.logo_url}
                      onChange={handleInputChange}
                      className="w-full border border-gray-300 rounded px-3 py-2"
                      placeholder="https://example.com/logo.png"
                    />
                  </div>

                  {/* Hours (Optional) */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      Business Hours (Optional)
                    </label>
                    <textarea
                      name="hours"
                      value={formData.hours}
                      onChange={handleInputChange}
                      rows="3"
                      className="w-full border border-gray-300 rounded px-3 py-2"
                      placeholder="Mon-Fri: 9am-6pm&#10;Sat: 10am-4pm&#10;Sun: Closed"
                    />
                  </div>

                  {/* About (Optional) */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                      About (Optional)
                    </label>
                    <textarea
                      name="about"
                      value={formData.about}
                      onChange={handleInputChange}
                      rows="4"
                      className="w-full border border-gray-300 rounded px-3 py-2"
                      placeholder="Tell customers about this dealership..."
                    />
                  </div>
                </div>

                {/* Form Actions */}
                <div className="flex justify-end space-x-3 mt-6">
                  <button
                    type="button"
                    onClick={() => {
                      setShowCreateForm(false);
                      resetForm();
                      setError('');
                    }}
                    className="px-4 py-2 border border-gray-300 rounded text-gray-700 hover:bg-gray-50"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
                  >
                    Create Dealership
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}

        {/* Dealerships List */}
        <div className="bg-white rounded-lg shadow">
          <div className="p-6">
            <h2 className="text-xl font-semibold mb-4">All Dealerships ({dealerships.length})</h2>
            
            {isLoading ? (
              <p className="text-gray-600">Loading dealerships...</p>
            ) : dealerships.length === 0 ? (
              <p className="text-gray-600">No dealerships found.</p>
            ) : (
              <div className="overflow-x-auto">
                <table className="min-w-full">
                  <thead className="bg-gray-50">
                    <tr>
                      <th 
                        className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                        onClick={() => handleSort('id')}
                      >
                        <div className="flex items-center">
                          ID
                          {sortBy === 'id' && (
                            <span className="ml-1">{sortOrder === 'asc' ? '↑' : '↓'}</span>
                          )}
                        </div>
                      </th>
                      <th 
                        className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                        onClick={() => handleSort('name')}
                      >
                        <div className="flex items-center">
                          Name
                          {sortBy === 'name' && (
                            <span className="ml-1">{sortOrder === 'asc' ? '↑' : '↓'}</span>
                          )}
                        </div>
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Website URL
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Email
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Phone
                      </th>
                      <th 
                        className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
                        onClick={() => handleSort('created_at')}
                      >
                        <div className="flex items-center">
                          Created
                          {sortBy === 'created_at' && (
                            <span className="ml-1">{sortOrder === 'asc' ? '↑' : '↓'}</span>
                          )}
                        </div>
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Actions
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {sortedDealerships.map((dealership) => (
                      <tr key={dealership.id} className="hover:bg-gray-50">
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                          {dealership.id}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                          {dealership.name}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                          {dealership.website_url || <span className="text-gray-400 italic">Not set</span>}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                          {dealership.email}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                          {dealership.phone}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                          {new Date(dealership.created_at).toLocaleDateString()}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                          <button
                            onClick={() => handleDeleteDealership(dealership.id, dealership.name)}
                            className="text-red-600 hover:text-red-800 font-medium"
                            title="Delete dealership"
                          >
                            Delete
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
