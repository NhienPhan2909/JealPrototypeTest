/**
 * SalesRequests - Displays customer sales requests for the selected dealership.
 * Fetches sales requests from API, displays in table with sorting, status management, and delete capability.
 * Requires 'sales_requests' permission to access.
 *
 * @component
 * @example
 * <SalesRequests />
 */

import { useState, useEffect, useContext } from 'react';
import { AdminContext } from '../../context/AdminContext';
import AdminHeader from '../../components/AdminHeader';
import Unauthorized from '../../components/Unauthorized';
import { hasPermission } from '../../utils/permissions';
import apiRequest from '../../utils/api';

function SalesRequests() {
  const { selectedDealership, user } = useContext(AdminContext);

  // Check permission
  if (!hasPermission(user, 'sales_requests')) {
    return <Unauthorized section="Sales Requests" />;
  }

  const [salesRequests, setSalesRequests] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [expandedRequestId, setExpandedRequestId] = useState(null);
  const [dateFilter, setDateFilter] = useState('All time');
  const [deleteModal, setDeleteModal] = useState({
    isOpen: false,
    requestId: null,
    requestName: ''
  });

  /**
   * Decodes HTML entities to make text human-readable.
   */
  const decodeHtmlEntities = (text) => {
    if (!text) return text;
    const element = document.createElement('div');
    element.innerHTML = text;
    return element.textContent || element.innerText || text;
  };

  /**
   * Fetches sales requests for the selected dealership
   */
  const fetchSalesRequests = async () => {
    if (!selectedDealership?.id) return;

    setLoading(true);
    setError(null);

    try {
      const response = await apiRequest(`/api/sales-requests/dealership/${selectedDealership.id}`);

      if (!response.ok) {
        throw new Error('Failed to fetch sales requests');
      }

      const result = await response.json();
      const data = result.data || result.Data || result;
      setSalesRequests(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (selectedDealership?.id) {
      fetchSalesRequests();
    }
  }, [selectedDealership]);

  /**
   * Sorts sales requests by created_at descending (newest first)
   */
  const getSortedRequests = () => {
    return [...salesRequests].sort((a, b) => new Date(b.created_at) - new Date(a.created_at));
  };

  /**
   * Filters sales requests based on selected date range
   */
  const getFilteredRequests = () => {
    const sortedRequests = getSortedRequests();
    
    if (dateFilter === 'All time') {
      return sortedRequests;
    }

    const now = new Date();
    const daysAgo = dateFilter === 'Last 7 days' ? 7 : 30;
    const cutoffDate = new Date(now.setDate(now.getDate() - daysAgo));

    return sortedRequests.filter(request => new Date(request.created_at) >= cutoffDate);
  };

  /**
   * Truncates message to 100 characters with ellipsis
   */
  const truncateMessage = (message) => {
    if (!message) return '';
    return message.length > 100 ? message.substring(0, 100) + '...' : message;
  };

  /**
   * Toggles expanded state for a request row
   */
  const toggleExpand = (requestId) => {
    setExpandedRequestId(expandedRequestId === requestId ? null : requestId);
  };

  /**
   * Formats ISO date string to "MM/DD/YYYY HH:MM AM/PM"
   */
  const formatDate = (isoString) => {
    const date = new Date(isoString);
    return date.toLocaleString('en-US', {
      month: '2-digit',
      day: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      hour12: true
    });
  };

  /**
   * Updates the status of a sales request
   */
  const handleStatusChange = async (requestId, newStatus) => {
    try {
      const response = await fetch(`/api/sales-requests/${requestId}/status?dealershipId=${selectedDealership.id}`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify({ status: newStatus })
      });

      if (!response.ok) {
        throw new Error('Failed to update sales request status');
      }

      const updatedRequest = await response.json();
      
      setSalesRequests(salesRequests.map(request => 
        request.id === requestId ? updatedRequest : request
      ));
    } catch (err) {
      console.error('Error updating sales request status:', err);
      setError(err.message);
    }
  };

  /**
   * Opens delete confirmation modal for specified sales request
   */
  const openDeleteModal = (request) => {
    setDeleteModal({
      isOpen: true,
      requestId: request.id,
      requestName: request.name
    });
  };

  /**
   * Closes delete confirmation modal without action
   */
  const closeDeleteModal = () => {
    setDeleteModal({
      isOpen: false,
      requestId: null,
      requestName: ''
    });
  };

  /**
   * Deletes sales request after confirmation
   */
  const confirmDelete = async (requestId) => {
    try {
      const response = await apiRequest(`/api/sales-requests/${requestId}?dealershipId=${selectedDealership.id}`, {
        method: 'DELETE',
        });

      if (!response.ok) {
        if (response.status === 404) {
          throw new Error('Sales request not found or does not belong to this dealership');
        }
        throw new Error('Failed to delete sales request');
      }

      setSalesRequests(salesRequests.filter(r => r.id !== requestId));
      closeDeleteModal();
    } catch (err) {
      console.error('Error deleting sales request:', err);
      setError(err.message);
    }
  };

  /**
   * Status badge color mapping for visual indicators
   */
  const statusColors = {
    'received': 'bg-blue-100 text-blue-800',
    'in progress': 'bg-yellow-100 text-yellow-800',
    'done': 'bg-green-100 text-green-800'
  };

  const filteredRequests = getFilteredRequests();

  return (
    <div className="min-h-screen bg-gray-100">
      <AdminHeader />

      <div className="max-w-7xl mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold mb-6 text-gray-800">Sales Requests</h1>

        {!selectedDealership && (
          <div className="card">
            <p className="text-gray-600">Loading dealership...</p>
          </div>
        )}

        {selectedDealership && (
          <>
          <div className="flex justify-between items-center mb-4">
            <p className="text-gray-700">Showing {filteredRequests.length} sales requests</p>
            
            <select
              value={dateFilter}
              onChange={(e) => setDateFilter(e.target.value)}
              className="border rounded px-3 py-2"
            >
              <option>All time</option>
              <option>Last 7 days</option>
              <option>Last 30 days</option>
            </select>
          </div>

          {error && (
            <div className="bg-red-100 text-red-800 p-4 mb-4 rounded">
              {error}
            </div>
          )}

          {loading && <p className="text-gray-600">Loading sales requests...</p>}

          {!loading && filteredRequests.length === 0 && (
            <p className="text-gray-600">
              No sales requests yet. Requests submitted through the website will appear here.
            </p>
          )}

          {!loading && filteredRequests.length > 0 && (
            <div className="overflow-x-auto">
              <table className="min-w-full bg-white border">
                <thead className="bg-gray-100">
                  <tr>
                    <th className="px-4 py-2 border text-left">Name</th>
                    <th className="px-4 py-2 border text-left">Email</th>
                    <th className="px-4 py-2 border text-left">Phone</th>
                    <th className="px-4 py-2 border text-left">Vehicle</th>
                    <th className="px-4 py-2 border text-left">Kilometers</th>
                    <th className="px-4 py-2 border text-left">Additional Info</th>
                    <th className="px-4 py-2 border text-left">Date Submitted</th>
                    <th className="px-4 py-2 border text-left">Status</th>
                    <th className="px-4 py-2 border text-left">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredRequests.map((request) => (
                    <tr
                      key={request.id}
                      className="hover:bg-gray-50"
                    >
                      <td className="px-4 py-2 border">{decodeHtmlEntities(request.name)}</td>
                      <td className="px-4 py-2 border">{request.email}</td>
                      <td className="px-4 py-2 border">{decodeHtmlEntities(request.phone)}</td>
                      <td className="px-4 py-2 border">
                        <div>
                          <div className="font-medium">
                            {decodeHtmlEntities(`${request.year} ${request.make} ${request.model}`)}
                          </div>
                        </div>
                      </td>
                      <td className="px-4 py-2 border">{request.kilometers.toLocaleString()} km</td>
                      <td className="px-4 py-2 border">
                        {request.additional_message ? (
                          <div>
                            <p>
                              {expandedRequestId === request.id 
                                ? decodeHtmlEntities(request.additional_message)
                                : decodeHtmlEntities(truncateMessage(request.additional_message))
                              }
                            </p>
                            {request.additional_message.length > 100 && (
                              <button
                                onClick={() => toggleExpand(request.id)}
                                className="text-blue-600 hover:text-blue-800 text-sm mt-1"
                              >
                                {expandedRequestId === request.id ? 'Show less' : 'Show more'}
                              </button>
                            )}
                          </div>
                        ) : (
                          <span className="text-gray-400">None</span>
                        )}
                      </td>
                      <td className="px-4 py-2 border whitespace-nowrap">
                        {formatDate(request.created_at)}
                      </td>
                      <td className="px-4 py-2 border">
                        <select
                          value={request.status || 'received'}
                          onChange={(e) => handleStatusChange(request.id, e.target.value)}
                          className={`px-2 py-1 rounded text-sm border ${statusColors[request.status || 'received']}`}
                        >
                          <option value="received">Received</option>
                          <option value="in progress">In Progress</option>
                          <option value="done">Done</option>
                        </select>
                      </td>
                      <td className="px-4 py-2 border">
                        <div className="flex gap-2">
                          <a
                            href={`tel:${request.phone}`}
                            className="bg-blue-500 text-white px-3 py-1 rounded hover:bg-blue-600 text-sm"
                          >
                            Call
                          </a>
                          <a
                            href={`mailto:${request.email}?subject=${encodeURIComponent(`Re: Your ${request.year} ${request.make} ${request.model}`)}`}
                            className="bg-green-500 text-white px-3 py-1 rounded hover:bg-green-600 text-sm"
                          >
                            Email
                          </a>
                          <button
                            onClick={() => openDeleteModal(request)}
                            className="bg-red-500 text-white px-3 py-1 rounded hover:bg-red-600 text-sm"
                          >
                            Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {deleteModal.isOpen && (
            <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
              <div className="bg-white rounded-lg p-6 max-w-md">
                <h2 className="text-xl font-bold mb-4">Confirm Delete</h2>
                <p className="mb-6">
                  Are you sure you want to delete this sales request?
                  <br />
                  <strong>{decodeHtmlEntities(deleteModal.requestName)}</strong>
                </p>
                <div className="flex justify-end gap-4">
                  <button onClick={closeDeleteModal} className="px-4 py-2 bg-gray-300 text-gray-800 rounded hover:bg-gray-400">
                    Cancel
                  </button>
                  <button
                    onClick={() => confirmDelete(deleteModal.requestId)}
                    className="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
                  >
                    Confirm
                  </button>
                </div>
              </div>
            </div>
          )}
          </>
        )}
      </div>
    </div>
  );
}

export default SalesRequests;
