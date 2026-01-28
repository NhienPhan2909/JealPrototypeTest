/**
 * @fileoverview Vehicle Manager List View - Admin page for managing dealership vehicle inventory.
 * Displays vehicles in table format with Edit/Delete actions, status filtering, and delete confirmation.
 *
 * SECURITY (SEC-001): All API calls include dealershipId parameter for multi-tenant data isolation.
 * Only vehicles belonging to selected dealership are displayed and can be modified.
 *
 * Features:
 * - Fetches and displays vehicles for selected dealership
 * - Table view with columns: Thumbnail, Title, Price, Mileage, Condition, Status, Actions
 * - Status filter dropdown (All, Active, Sold, Pending, Draft)
 * - Delete confirmation modal with ownership validation
 * - Navigation to Add/Edit vehicle forms
 * - Loading, error, and empty states
 *
 * @component
 */

import { useState, useEffect, useContext } from 'react';
import { useNavigate } from 'react-router-dom';
import { AdminContext } from '../../context/AdminContext';
import AdminHeader from '../../components/AdminHeader';
import Unauthorized from '../../components/Unauthorized';
import { hasPermission } from '../../utils/permissions';
import apiRequest from '../../utils/api';

/**
 * VehicleList - Admin vehicle manager page component.
 * All authenticated users can VIEW vehicles.
 * Only users with 'vehicles' permission can EDIT/DELETE vehicles.
 */
export default function VehicleList() {
  const navigate = useNavigate();
  const { selectedDealership, user } = useContext(AdminContext);

  // Check if user can edit vehicles
  const canEditVehicles = hasPermission(user, 'vehicles');

  // State management
  const [vehicles, setVehicles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [statusFilter, setStatusFilter] = useState('All');
  const [deleteModal, setDeleteModal] = useState({
    isOpen: false,
    vehicleId: null,
    vehicleTitle: ''
  });

  /**
   * Fetches vehicles for selected dealership from API.
   * Triggered on component mount and when selectedDealership changes.
   *
   * SECURITY: Includes dealershipId query parameter for multi-tenant filtering (SEC-001).
   */
  useEffect(() => {
    const fetchVehicles = async () => {
      // Guard: Don't fetch if no dealership selected
      if (!selectedDealership) {
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);

        const response = await fetch(
          `/api/vehicles?dealershipId=${selectedDealership.id}`,
          { credentials: 'include' }
        );

        if (!response.ok) {
          throw new Error('Failed to fetch vehicles');
        }

        const result = await response.json();
        const data = result.data || result.Data || result;
        setVehicles(data);
      } catch (err) {
        console.error('Error fetching vehicles:', err);
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchVehicles();
  }, [selectedDealership]);

  /**
   * Filters vehicles based on selected status.
   * Returns all vehicles if 'All' is selected, otherwise filters by status.
   */
  const filteredVehicles = statusFilter === 'All'
    ? vehicles
    : vehicles.filter(vehicle => vehicle.status === statusFilter.toLowerCase());

  /**
   * Navigates to Add Vehicle form.
   */
  const handleAddVehicle = () => {
    navigate('/admin/vehicles/new');
  };

  /**
   * Navigates to Edit Vehicle form for specified vehicle.
   *
   * @param {number} vehicleId - ID of vehicle to edit
   */
  const handleEdit = (vehicleId) => {
    navigate(`/admin/vehicles/edit/${vehicleId}`);
  };

  /**
   * Opens delete confirmation modal for specified vehicle.
   *
   * @param {Object} vehicle - Vehicle object to delete
   */
  const openDeleteModal = (vehicle) => {
    setDeleteModal({
      isOpen: true,
      vehicleId: vehicle.id,
      vehicleTitle: vehicle.title
    });
  };

  /**
   * Closes delete confirmation modal without action.
   */
  const closeDeleteModal = () => {
    setDeleteModal({
      isOpen: false,
      vehicleId: null,
      vehicleTitle: ''
    });
  };

  /**
   * Deletes vehicle after confirmation.
   * Sends DELETE request to API with dealershipId for multi-tenancy security.
   *
   * SECURITY (SEC-001): DELETE endpoint requires dealershipId query parameter
   * to enforce multi-tenant data isolation and prevent cross-dealership deletions.
   *
   * @param {number} vehicleId - ID of vehicle to delete
   */
  const confirmDelete = async (vehicleId) => {
    try {
      const response = await apiRequest(
        `/api/vehicles/${vehicleId}?dealershipId=${selectedDealership.id}`,
        {
          method: 'DELETE'
        }
      );

      if (!response.ok) {
        if (response.status === 404) {
          throw new Error('Vehicle not found or does not belong to this dealership');
        }
        throw new Error('Failed to delete vehicle');
      }

      // Remove vehicle from local state
      setVehicles(vehicles.filter(v => v.id !== vehicleId));
      closeDeleteModal();
    } catch (err) {
      console.error('Error deleting vehicle:', err);
      setError(err.message);
    }
  };

  /**
   * Handles status filter dropdown change.
   *
   * @param {Event} e - Change event from select element
   */
  const handleFilterChange = (e) => {
    setStatusFilter(e.target.value);
  };

  /**
   * Status badge color mapping for visual indicators.
   * Active (green), Sold (gray), Pending (yellow), Draft (blue)
   */
  const statusColors = {
    active: 'bg-green-600 text-white',
    sold: 'bg-gray-600 text-white',
    pending: 'bg-yellow-600 text-white',
    draft: 'bg-blue-600 text-white'
  };

  return (
    <div className="min-h-screen bg-gray-100">
      <AdminHeader />

      <div className="max-w-7xl mx-auto px-4 py-8">
        {/* Loading state */}
        {loading && (
          <div className="card">
            <p className="text-gray-600">Loading vehicles...</p>
          </div>
        )}

        {/* Error state */}
        {error && !loading && (
          <div className="card bg-red-50 border-red-200">
            <p className="text-red-600">Error loading vehicles: {error}</p>
          </div>
        )}

        {/* No dealership selected state */}
        {!loading && !selectedDealership && (
          <div className="card">
            <p className="text-gray-600">Please select a dealership to view vehicles.</p>
          </div>
        )}

        {/* Main content */}
        {!loading && !error && selectedDealership && (<>
      {/* Read-only banner for users without edit permission */}
      {!canEditVehicles && (
        <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 mb-6">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg className="h-5 w-5 text-yellow-400" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
              </svg>
            </div>
            <div className="ml-3">
              <p className="text-sm text-yellow-700">
                <strong>View Only:</strong> You can view all vehicles but cannot add, edit, or delete them. Contact your dealership owner to request vehicle management permission.
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Page Header */}
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Vehicle Manager</h1>
        {canEditVehicles && (
          <button onClick={handleAddVehicle} className="btn-primary">
            Add Vehicle
          </button>
        )}
      </div>

      {/* Filters and Count */}
      <div className="flex justify-between items-center mb-4">
        <div>
          <label className="mr-2 text-gray-700">Filter by Status:</label>
          <select
            value={statusFilter}
            onChange={handleFilterChange}
            className="input-field w-auto"
          >
            <option value="All">All</option>
            <option value="Active">Active</option>
            <option value="Sold">Sold</option>
            <option value="Pending">Pending</option>
            <option value="Draft">Draft</option>
          </select>
        </div>
        <p className="text-gray-600">Showing {filteredVehicles.length} vehicles</p>
      </div>

      {/* Empty State */}
      {filteredVehicles.length === 0 && (
        <div className="text-center py-12">
          <p className="text-gray-600 text-lg mb-4">
            {canEditVehicles 
              ? "No vehicles yet. Click 'Add Vehicle' to get started."
              : "No vehicles available for this dealership."}
          </p>
          {canEditVehicles && (
            <button onClick={handleAddVehicle} className="btn-primary">
              Add Vehicle
            </button>
          )}
        </div>
      )}

      {/* Vehicle Table */}
      {filteredVehicles.length > 0 && (
        <div className="overflow-x-auto">
          <table className="w-full border-collapse bg-white shadow-md rounded-lg">
            <thead>
              <tr className="bg-gray-200">
                <th className="p-3 text-left">Thumbnail</th>
                <th className="p-3 text-left">Title</th>
                <th className="p-3 text-left">Price</th>
                <th className="p-3 text-left">Mileage</th>
                <th className="p-3 text-left">Condition</th>
                <th className="p-3 text-left">Status</th>
                {canEditVehicles && <th className="p-3 text-left">Actions</th>}
              </tr>
            </thead>
            <tbody>
              {filteredVehicles.map((vehicle) => (
                <tr key={vehicle.id} className="border-b hover:bg-gray-50">
                  <td className="p-3">
                    <img
                      src={vehicle.images && vehicle.images.length > 0 ? vehicle.images[0] : '/placeholder.jpg'}
                      alt={vehicle.title}
                      className="w-16 h-16 object-cover rounded"
                    />
                  </td>
                  <td className="p-3">{vehicle.title}</td>
                  <td className="p-3">${parseFloat(vehicle.price).toLocaleString()}</td>
                  <td className="p-3">{vehicle.mileage.toLocaleString()} mi</td>
                  <td className="p-3 capitalize">{vehicle.condition}</td>
                  <td className="p-3">
                    <span className={`px-2 py-1 rounded text-sm ${statusColors[vehicle.status]}`}>
                      {vehicle.status.charAt(0).toUpperCase() + vehicle.status.slice(1)}
                    </span>
                  </td>
                  {canEditVehicles && (
                    <td className="p-3">
                      <button
                        onClick={() => handleEdit(vehicle.id)}
                        className="btn-primary mr-2 text-sm"
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => openDeleteModal(vehicle)}
                        className="btn-danger text-sm"
                      >
                        Delete
                      </button>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

        {/* Delete Confirmation Modal */}
        {deleteModal.isOpen && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
            <div className="bg-white rounded-lg p-6 max-w-md">
              <h2 className="text-xl font-bold mb-4">Confirm Delete</h2>
              <p className="mb-6">
                Are you sure you want to delete this vehicle?
                <br />
                <strong>{deleteModal.vehicleTitle}</strong>
              </p>
              <div className="flex justify-end gap-4">
                <button onClick={closeDeleteModal} className="btn-secondary">
                  Cancel
                </button>
                <button
                  onClick={() => confirmDelete(deleteModal.vehicleId)}
                  className="btn-danger"
                >
                  Confirm
                </button>
              </div>
            </div>
          </div>
        )}
        </>)}
      </div>
    </div>
  );
}
