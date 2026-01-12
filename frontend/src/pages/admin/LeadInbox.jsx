/**
 * LeadInbox - Displays customer enquiries for the selected dealership.
 * Fetches leads from API, displays in table with sorting, vehicle linking, contact actions,
 * status management, and delete capability.
 * Requires 'leads' permission to access.
 *
 * @component
 * @example
 * <LeadInbox />
 */

import { useState, useEffect, useContext } from 'react';
import { AdminContext } from '../../context/AdminContext';
import AdminHeader from '../../components/AdminHeader';
import Unauthorized from '../../components/Unauthorized';
import { hasPermission } from '../../utils/permissions';

function LeadInbox() {
  const { selectedDealership, user } = useContext(AdminContext);

  // Check if user can edit leads (for showing/hiding action buttons)
  const canEditLeads = hasPermission(user, 'leads');

  const [leads, setLeads] = useState([]);
  const [vehicleMap, setVehicleMap] = useState({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [expandedLeadId, setExpandedLeadId] = useState(null);
  const [dateFilter, setDateFilter] = useState('All time');
  const [deleteModal, setDeleteModal] = useState({
    isOpen: false,
    leadId: null,
    leadName: ''
  });

  /**
   * Decodes HTML entities to make text human-readable.
   * Converts escaped HTML characters back to their original form.
   * Uses DOMParser for more reliable decoding of all HTML entities.
   * 
   * @param {string} text - Text containing HTML entities
   * @returns {string} Decoded text
   * 
   * @example
   * decodeHtmlEntities("I&#x27;m interested") // Returns: "I'm interested"
   * decodeHtmlEntities("&lt;script&gt;") // Returns: "<script>"
   */
  const decodeHtmlEntities = (text) => {
    if (!text) return text;
    
    // Create a temporary element to decode HTML entities
    const element = document.createElement('div');
    element.innerHTML = text;
    
    // Get the decoded text content
    return element.textContent || element.innerText || text;
  };

  /**
   * Fetches leads for the selected dealership
   */
  const fetchLeads = async () => {
    if (!selectedDealership?.id) return;

    setLoading(true);
    setError(null);

    try {
      const response = await fetch(`/api/leads?dealershipId=${selectedDealership.id}`, {
        credentials: 'include'
      });

      if (!response.ok) {
        throw new Error('Failed to fetch leads');
      }

      const data = await response.json();
      setLeads(data);

      // Fetch vehicle titles for leads with vehicle_id
      await fetchVehicleTitles(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  /**
   * Fetches vehicle titles for leads that have vehicle_id
   * @param {Array} leadsData - Array of lead objects
   */
  const fetchVehicleTitles = async (leadsData) => {
    const uniqueVehicleIds = [...new Set(
      leadsData
        .filter(lead => lead.vehicle_id)
        .map(lead => lead.vehicle_id)
    )];

    const titleMap = {};

    for (const vehicleId of uniqueVehicleIds) {
      try {
        const response = await fetch(`/api/vehicles/${vehicleId}?dealershipId=${selectedDealership.id}`, {
          credentials: 'include'
        });

        if (response.ok) {
          const vehicle = await response.json();
          titleMap[vehicleId] = vehicle.title;
        }
      } catch (err) {
        console.error(`Failed to fetch vehicle ${vehicleId}:`, err);
        titleMap[vehicleId] = 'Unknown Vehicle';
      }
    }

    setVehicleMap(titleMap);
  };

  useEffect(() => {
    // Only fetch leads when we have a selected dealership
    if (selectedDealership?.id) {
      fetchLeads();
    }
  }, [selectedDealership]);

  /**
   * Sorts leads by created_at descending (newest first)
   * @returns {Array} Sorted leads array
   */
  const getSortedLeads = () => {
    return [...leads].sort((a, b) => new Date(b.created_at) - new Date(a.created_at));
  };

  /**
   * Filters leads based on selected date range
   * @returns {Array} Filtered leads array
   */
  const getFilteredLeads = () => {
    const sortedLeads = getSortedLeads();
    
    if (dateFilter === 'All time') {
      return sortedLeads;
    }

    const now = new Date();
    const daysAgo = dateFilter === 'Last 7 days' ? 7 : 30;
    const cutoffDate = new Date(now.setDate(now.getDate() - daysAgo));

    return sortedLeads.filter(lead => new Date(lead.created_at) >= cutoffDate);
  };

  /**
   * Truncates message to 100 characters with ellipsis
   * @param {string} message - Full message text
   * @returns {string} Truncated message
   */
  const truncateMessage = (message) => {
    return message.length > 100 ? message.substring(0, 100) + '...' : message;
  };

  /**
   * Toggles expanded state for a lead row
   * @param {number} leadId - Lead ID to toggle
   */
  const toggleExpand = (leadId) => {
    setExpandedLeadId(expandedLeadId === leadId ? null : leadId);
  };


  /**
   * Formats ISO date string to "MM/DD/YYYY HH:MM AM/PM"
   * @param {string} isoString - ISO date string
   * @returns {string} Formatted date string
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
   * Updates the status of a lead
   * @param {number} leadId - Lead ID to update
   * @param {string} newStatus - New status value
   */
  const handleStatusChange = async (leadId, newStatus) => {
    try {
      const response = await fetch(`/api/leads/${leadId}/status?dealershipId=${selectedDealership.id}`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify({ status: newStatus })
      });

      if (!response.ok) {
        throw new Error('Failed to update lead status');
      }

      const updatedLead = await response.json();
      
      // Update lead in local state
      setLeads(leads.map(lead => 
        lead.id === leadId ? updatedLead : lead
      ));
    } catch (err) {
      console.error('Error updating lead status:', err);
      setError(err.message);
    }
  };

  /**
   * Opens delete confirmation modal for specified lead
   * @param {Object} lead - Lead object to delete
   */
  const openDeleteModal = (lead) => {
    setDeleteModal({
      isOpen: true,
      leadId: lead.id,
      leadName: lead.name
    });
  };

  /**
   * Closes delete confirmation modal without action
   */
  const closeDeleteModal = () => {
    setDeleteModal({
      isOpen: false,
      leadId: null,
      leadName: ''
    });
  };

  /**
   * Deletes lead after confirmation
   * @param {number} leadId - Lead ID to delete
   */
  const confirmDelete = async (leadId) => {
    try {
      const response = await fetch(`/api/leads/${leadId}?dealershipId=${selectedDealership.id}`, {
        method: 'DELETE',
        credentials: 'include'
      });

      if (!response.ok) {
        if (response.status === 404) {
          throw new Error('Lead not found or does not belong to this dealership');
        }
        throw new Error('Failed to delete lead');
      }

      // Remove lead from local state
      setLeads(leads.filter(l => l.id !== leadId));
      closeDeleteModal();
    } catch (err) {
      console.error('Error deleting lead:', err);
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

  const filteredLeads = getFilteredLeads();

  return (
    <div className="min-h-screen bg-gray-100">
      <AdminHeader />

      <div className="max-w-7xl mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold mb-6 text-gray-800">Lead Inbox</h1>

        {/* Read-only notice for users without edit permission */}
        {!canEditLeads && (
          <div className="card bg-blue-50 border-blue-200 mb-4">
            <p className="text-blue-800">
              <strong>View Only:</strong> You can view all leads but cannot edit or delete them. 
              Contact your dealership owner to request edit access.
            </p>
          </div>
        )}

        {/* No dealership selected yet */}
        {!selectedDealership && (
          <div className="card">
            <p className="text-gray-600">Loading dealership...</p>
          </div>
        )}

        {/* Dealership selected, show lead management UI */}
        {selectedDealership && (
          <>
          {/* Lead count and date filter */}
          <div className="flex justify-between items-center mb-4">
            <p className="text-gray-700">Showing {filteredLeads.length} leads</p>
            
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

          {/* Error message */}
          {error && (
            <div className="bg-red-100 text-red-800 p-4 mb-4 rounded">
              {error}
            </div>
          )}

          {/* Loading state */}
          {loading && <p className="text-gray-600">Loading leads...</p>}

          {/* Empty state */}
          {!loading && filteredLeads.length === 0 && (
            <p className="text-gray-600">
              No leads yet. Leads submitted through the website will appear here.
            </p>
          )}

          {/* Leads table */}
          {!loading && filteredLeads.length > 0 && (
            <div className="overflow-x-auto">
              <table className="min-w-full bg-white border">
                <thead className="bg-gray-100">
                  <tr>
                    <th className="px-4 py-2 border text-left">Name</th>
                    <th className="px-4 py-2 border text-left">Email</th>
                    <th className="px-4 py-2 border text-left">Phone</th>
                    <th className="px-4 py-2 border text-left">Vehicle</th>
                    <th className="px-4 py-2 border text-left">Message</th>
                    <th className="px-4 py-2 border text-left">Date Submitted</th>
                    <th className="px-4 py-2 border text-left">Status</th>
                    <th className="px-4 py-2 border text-left">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredLeads.map((lead) => (
                    <tr
                      key={lead.id}
                      className="hover:bg-gray-50"
                    >
                      <td className="px-4 py-2 border">{decodeHtmlEntities(lead.name)}</td>
                      <td className="px-4 py-2 border">{lead.email}</td>
                      <td className="px-4 py-2 border">{decodeHtmlEntities(lead.phone)}</td>
                      <td className="px-4 py-2 border">
                        {lead.vehicle_id ? (
                          <a
                            href={`/inventory/${lead.vehicle_id}`}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="text-blue-600 underline hover:text-blue-800"
                          >
                            {vehicleMap[lead.vehicle_id] || 'Loading...'}
                          </a>
                        ) : (
                          <span className="text-gray-500">General Enquiry</span>
                        )}
                      </td>
                      <td className="px-4 py-2 border">
                        <div>
                          <p>
                            {expandedLeadId === lead.id 
                              ? decodeHtmlEntities(lead.message)
                              : decodeHtmlEntities(truncateMessage(lead.message))
                            }
                          </p>
                          {lead.message.length > 100 && (
                            <button
                              onClick={() => toggleExpand(lead.id)}
                              className="text-blue-600 hover:text-blue-800 text-sm mt-1"
                            >
                              {expandedLeadId === lead.id ? 'Show less' : 'Show more'}
                            </button>
                          )}
                        </div>
                      </td>
                      <td className="px-4 py-2 border whitespace-nowrap">
                        {formatDate(lead.created_at)}
                      </td>
                      <td className="px-4 py-2 border">
                        <select
                          value={lead.status || 'received'}
                          onChange={(e) => handleStatusChange(lead.id, e.target.value)}
                          className={`px-2 py-1 rounded text-sm border ${statusColors[lead.status || 'received']}`}
                          disabled={!canEditLeads}
                        >
                          <option value="received">Received</option>
                          <option value="in progress">In Progress</option>
                          <option value="done">Done</option>
                        </select>
                      </td>
                      <td className="px-4 py-2 border">
                        <div className="flex gap-2">
                          <a
                            href={`tel:${lead.phone}`}
                            className="bg-blue-500 text-white px-3 py-1 rounded hover:bg-blue-600 text-sm"
                          >
                            Call
                          </a>
                          <a
                            href={`mailto:${lead.email}?subject=${encodeURIComponent(`Re: Your enquiry about ${vehicleMap[lead.vehicle_id] || 'our dealership'}`)}`}
                            className="bg-green-500 text-white px-3 py-1 rounded hover:bg-green-600 text-sm"
                          >
                            Email
                          </a>
                          {canEditLeads && (
                            <button
                              onClick={() => openDeleteModal(lead)}
                              className="bg-red-500 text-white px-3 py-1 rounded hover:bg-red-600 text-sm"
                            >
                              Delete
                            </button>
                          )}
                        </div>
                      </td>
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
                  Are you sure you want to delete this lead?
                  <br />
                  <strong>{decodeHtmlEntities(deleteModal.leadName)}</strong>
                </p>
                <div className="flex justify-end gap-4">
                  <button onClick={closeDeleteModal} className="px-4 py-2 bg-gray-300 text-gray-800 rounded hover:bg-gray-400">
                    Cancel
                  </button>
                  <button
                    onClick={() => confirmDelete(deleteModal.leadId)}
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

export default LeadInbox;
