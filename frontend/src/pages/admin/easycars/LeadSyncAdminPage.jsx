/**
 * @fileoverview LeadSyncAdminPage - Main admin page for EasyCars lead synchronization (Story 3.5)
 * Displays lead sync status dashboard, manual sync trigger, and sync history with polling mechanism
 */

import { useState, useEffect, useContext, useRef } from 'react';
import { AdminContext } from '../../../context/AdminContext';
import AdminHeader from '../../../components/AdminHeader';
import apiRequest from '../../../utils/api';
import LeadSyncDashboard from './components/LeadSyncDashboard';
import SyncHistoryTable from './components/SyncHistoryTable';
import SyncDetailsModal from './components/SyncDetailsModal';
import SyncConflictsTable from './components/SyncConflictsTable';

/**
 * LeadSyncAdminPage - Admin interface for EasyCars lead sync management
 */
export default function LeadSyncAdminPage() {
  const { selectedDealership } = useContext(AdminContext);
  
  // State management
  const [syncStatus, setSyncStatus] = useState(null);
  const [hasCredentials, setHasCredentials] = useState(false);
  const [syncHistory, setSyncHistory] = useState([]);
  const [conflicts, setConflicts] = useState([]);
  const [isSyncing, setIsSyncing] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedLogId, setSelectedLogId] = useState(null);
  
  // Polling refs
  const pollingIntervalRef = useRef(null);
  const pollingTimeoutRef = useRef(null);
  const syncTriggerTimeRef = useRef(null);

  /**
   * Fetch lead sync status from API
   */
  const fetchSyncStatus = async () => {
    try {
      const dealershipId = selectedDealership?.id;
      const response = await apiRequest(`/api/easycars/lead-sync-status?dealershipId=${dealershipId}`);
      
      if (response.ok) {
        const data = await response.json();
        setSyncStatus({
          lastSyncedAt: data.lastSyncedAt,
          status: data.status,
          itemsProcessed: data.itemsProcessed,
          itemsSucceeded: data.itemsSucceeded,
          itemsFailed: data.itemsFailed,
          durationMs: data.durationMs
        });
        setHasCredentials(data.hasCredentials);
        return data;
      } else {
        console.error('Failed to fetch lead sync status');
        setError('Failed to fetch lead sync status');
      }
    } catch (err) {
      console.error('Error fetching lead sync status:', err);
      setError('Failed to fetch lead sync status');
    }
    return null;
  };

  /**
   * Fetch lead sync history from API
   */
  const fetchSyncHistory = async () => {
    try {
      const dealershipId = selectedDealership?.id;
      const response = await apiRequest(`/api/easycars/lead-sync-history?page=1&pageSize=10&dealershipId=${dealershipId}`);
      
      if (response.ok) {
        const data = await response.json();
        setSyncHistory(data.logs || []);
      } else {
        console.error('Failed to fetch lead sync history');
      }
    } catch (err) {
      console.error('Error fetching lead sync history:', err);
    }
  };

  /**
   * Fetch unresolved status conflicts from API
   */
  const fetchConflicts = async () => {
    try {
      const dealershipId = selectedDealership?.id;
      const response = await apiRequest(`/api/easycars/lead-sync-conflicts?dealershipId=${dealershipId}`);
      if (response.ok) {
        const data = await response.json();
        setConflicts(data || []);
      }
    } catch (err) {
      console.error('Error fetching conflicts:', err);
    }
  };

  /**
   * Handle resolving a status conflict
   */
  const handleResolveConflict = async (conflictId, resolution) => {
    try {
      const response = await apiRequest(
        `/api/easycars/lead-sync-conflicts/${conflictId}/resolve?dealershipId=${selectedDealership?.id}`,
        { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ resolution }) }
      );
      if (response.ok) {
        showToast('success', `Conflict resolved (${resolution} wins)`);
        await fetchConflicts();
      } else {
        showToast('error', 'Failed to resolve conflict');
      }
    } catch (err) {
      showToast('error', 'Failed to resolve conflict');
    }
  };

  /**
   * Initial data fetch on mount
   */
  useEffect(() => {
    const fetchInitialData = async () => {
      setLoading(true);
      setError(null);
      
      await Promise.all([
        fetchSyncStatus(),
        fetchSyncHistory(),
        fetchConflicts()
      ]);
      
      setLoading(false);
    };

    if (selectedDealership) {
      fetchInitialData();
    }
  }, [selectedDealership]);

  /**
   * Start polling for sync status updates
   */
  const startPolling = () => {
    stopPolling();

    pollingIntervalRef.current = setInterval(async () => {
      const status = await fetchSyncStatus();
      
      if (status && status.status && status.status !== 'InProgress') {
        const syncedAt = status.lastSyncedAt ? new Date(status.lastSyncedAt) : null;
        if (syncedAt && syncTriggerTimeRef.current && syncedAt >= syncTriggerTimeRef.current) {
          stopPolling();
          setIsSyncing(false);
          
          await fetchSyncHistory();
          
          if (status.status === 'Success') {
            showToast('success', `Lead sync completed successfully! ${status.itemsSucceeded} leads synced.`);
          } else if (status.status === 'Failed') {
            showToast('error', 'Lead sync failed. Check details for more information.');
          } else if (status.status === 'PartialSuccess') {
            showToast('warning', `Lead sync partially successful: ${status.itemsSucceeded} succeeded, ${status.itemsFailed} failed.`);
          }
        }
      }
    }, 5000);

    pollingTimeoutRef.current = setTimeout(() => {
      stopPolling();
      setIsSyncing(false);
      showToast('warning', 'Lead sync is taking longer than expected. Check history for results.');
    }, 300000);
  };

  /**
   * Stop polling
   */
  const stopPolling = () => {
    if (pollingIntervalRef.current) {
      clearInterval(pollingIntervalRef.current);
      pollingIntervalRef.current = null;
    }
    if (pollingTimeoutRef.current) {
      clearTimeout(pollingTimeoutRef.current);
      pollingTimeoutRef.current = null;
    }
  };

  /**
   * Cleanup polling on unmount
   */
  useEffect(() => {
    return () => {
      stopPolling();
    };
  }, []);

  /**
   * Handle manual sync trigger
   */
  const handleSyncNow = async () => {
    if (!hasCredentials || isSyncing) {
      return;
    }

    try {
      setIsSyncing(true);
      syncTriggerTimeRef.current = new Date();
      showToast('info', 'Lead sync started...');

      const response = await apiRequest(`/api/easycars/lead-sync/trigger?dealershipId=${selectedDealership?.id}`, {
        method: 'POST'
      });

      if (response.ok) {
        const data = await response.json();
        console.log('Lead sync triggered:', data);
        startPolling();
      } else {
        let errorMsg = 'Unknown error';
        try {
          const errorData = await response.json();
          errorMsg = errorData.error || errorMsg;
        } catch (_) { /* empty body */ }
        setIsSyncing(false);
        showToast('error', `Failed to start lead sync: ${errorMsg}`);
      }
    } catch (err) {
      console.error('Error triggering lead sync:', err);
      setIsSyncing(false);
      showToast('error', 'Failed to start lead sync. Please try again.');
    }
  };

  /**
   * Handle view details click
   */
  const handleViewDetails = (logId) => {
    setSelectedLogId(logId);
  };

  /**
   * Close details modal
   */
  const handleCloseModal = () => {
    setSelectedLogId(null);
  };

  /**
   * Simple toast notification
   */
  const showToast = (type, message) => {
    const icons = {
      success: '✅',
      error: '❌',
      warning: '⚠️',
      info: 'ℹ️'
    };
    
    console.log(`${icons[type] || ''} ${message}`);
    
    const toast = document.createElement('div');
    toast.className = `fixed top-20 right-4 z-50 px-6 py-4 rounded-lg shadow-lg text-white font-medium max-w-md ${
      type === 'success' ? 'bg-green-600' :
      type === 'error' ? 'bg-red-600' :
      type === 'warning' ? 'bg-yellow-600' :
      'bg-blue-600'
    }`;
    toast.textContent = message;
    document.body.appendChild(toast);

    const duration = type === 'error' || type === 'warning' ? 10000 : 5000;
    setTimeout(() => {
      toast.remove();
    }, duration);
  };

  if (loading) {
    return (
      <>
        <AdminHeader />
        <div className="container mx-auto px-4 py-8">
          <div className="flex items-center justify-center py-12">
            <svg className="animate-spin h-8 w-8 text-blue-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
          </div>
        </div>
      </>
    );
  }

  if (error) {
    return (
      <>
        <AdminHeader />
        <div className="container mx-auto px-4 py-8">
          <div className="bg-red-50 border border-red-200 rounded-lg p-6 text-red-800">
            <h2 className="text-lg font-semibold mb-2">Error Loading Page</h2>
            <p>{error}</p>
            <button
              onClick={() => window.location.reload()}
              className="mt-4 px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 transition"
            >
              Retry
            </button>
          </div>
        </div>
      </>
    );
  }

  return (
    <>
      <AdminHeader />
      <div className="container mx-auto px-4 py-8">
        <div className="mb-6">
          <h1 className="text-3xl font-bold text-gray-900">EasyCars Lead Sync</h1>
          <p className="text-gray-600 mt-2">
            Monitor and manage your lead synchronization with EasyCars
          </p>
        </div>

        <div className="space-y-6">
          {/* Dashboard Section */}
          <LeadSyncDashboard
            syncStatus={syncStatus}
            hasCredentials={hasCredentials}
            isSyncing={isSyncing}
            onSyncNow={handleSyncNow}
          />

          {/* History Section */}
          <SyncHistoryTable
            syncHistory={syncHistory}
            onViewDetails={handleViewDetails}
          />

          {/* Status Conflicts Section */}
          <SyncConflictsTable
            conflicts={conflicts}
            onResolve={handleResolveConflict}
          />
        </div>

        {/* Details Modal */}
        {selectedLogId && (
          <SyncDetailsModal
            syncLogId={selectedLogId}
            dealershipId={selectedDealership?.id}
            onClose={handleCloseModal}
            apiBasePath="/api/easycars/lead-sync-logs"
            itemsLabel="Leads Processed"
          />
        )}
      </div>
    </>
  );
}
