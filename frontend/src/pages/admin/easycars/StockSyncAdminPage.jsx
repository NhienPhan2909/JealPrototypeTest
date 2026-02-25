/**
 * @fileoverview StockSyncAdminPage - Main admin page for EasyCars stock synchronization (Story 2.5)
 * Displays sync status dashboard, manual sync trigger, and sync history with polling mechanism
 */

import { useState, useEffect, useContext, useRef } from 'react';
import { AdminContext } from '../../../context/AdminContext';
import AdminHeader from '../../../components/AdminHeader';
import apiRequest from '../../../utils/api';
import StockSyncDashboard from './components/StockSyncDashboard';
import SyncHistoryTable from './components/SyncHistoryTable';
import SyncDetailsModal from './components/SyncDetailsModal';

/**
 * StockSyncAdminPage - Admin interface for EasyCars stock sync management
 * Implements all 12 acceptance criteria from Story 2.5
 */
export default function StockSyncAdminPage() {
  const { selectedDealership } = useContext(AdminContext);
  
  // State management
  const [syncStatus, setSyncStatus] = useState(null);
  const [hasCredentials, setHasCredentials] = useState(false);
  const [syncHistory, setSyncHistory] = useState([]);
  const [isSyncing, setIsSyncing] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedLogId, setSelectedLogId] = useState(null);
  
  // Polling refs
  const pollingIntervalRef = useRef(null);
  const pollingTimeoutRef = useRef(null);

  /**
   * Fetch sync status from API
   */
  const fetchSyncStatus = async () => {
    try {
      const dealershipId = selectedDealership?.id;
      const response = await apiRequest(`/api/easycars/sync-status?dealershipId=${dealershipId}`);
      
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
        console.error('Failed to fetch sync status');
        setError('Failed to fetch sync status');
      }
    } catch (err) {
      console.error('Error fetching sync status:', err);
      setError('Failed to fetch sync status');
    }
    return null;
  };

  /**
   * Fetch sync history from API
   */
  const fetchSyncHistory = async () => {
    try {
      const dealershipId = selectedDealership?.id;
      const response = await apiRequest(`/api/easycars/sync-history?page=1&pageSize=10&dealershipId=${dealershipId}`);
      
      if (response.ok) {
        const data = await response.json();
        setSyncHistory(data.logs || []);
      } else {
        console.error('Failed to fetch sync history');
      }
    } catch (err) {
      console.error('Error fetching sync history:', err);
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
        fetchSyncHistory()
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
    // Clear any existing polling
    stopPolling();

    // Poll every 5 seconds
    pollingIntervalRef.current = setInterval(async () => {
      const status = await fetchSyncStatus();
      
      if (status && status.status && status.status !== 'InProgress') {
        // Sync completed
        stopPolling();
        setIsSyncing(false);
        
        // Refresh history
        await fetchSyncHistory();
        
        // Show completion notification
        if (status.status === 'Success') {
          showToast('success', `Sync completed successfully! ${status.itemsSucceeded} vehicles synced.`);
        } else if (status.status === 'Failed') {
          showToast('error', 'Sync failed. Check details for more information.');
        } else if (status.status === 'PartialSuccess') {
          showToast('warning', `Sync partially successful: ${status.itemsSucceeded} succeeded, ${status.itemsFailed} failed.`);
        }
      }
    }, 5000); // 5 seconds

    // Timeout after 5 minutes
    pollingTimeoutRef.current = setTimeout(() => {
      stopPolling();
      setIsSyncing(false);
      showToast('warning', 'Sync is taking longer than expected. Check history for results.');
    }, 300000); // 5 minutes
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
      showToast('info', 'Sync started...');

      const response = await apiRequest(`/api/easycars/sync/trigger?dealershipId=${selectedDealership?.id}`, {
        method: 'POST'
      });

      if (response.ok) {
        const data = await response.json();
        console.log('Sync triggered:', data);
        
        // Start polling for status updates
        startPolling();
      } else {
        let errorMsg = 'Unknown error';
        try {
          const errorData = await response.json();
          errorMsg = errorData.error || errorMsg;
        } catch (_) { /* empty body */ }
        setIsSyncing(false);
        showToast('error', `Failed to start sync: ${errorMsg}`);
      }
    } catch (err) {
      console.error('Error triggering sync:', err);
      setIsSyncing(false);
      showToast('error', 'Failed to start sync. Please try again.');
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
    // Simple alert for now - can be enhanced with toast library
    const icons = {
      success: '✅',
      error: '❌',
      warning: '⚠️',
      info: 'ℹ️'
    };
    
    console.log(`${icons[type] || ''} ${message}`);
    
    // Create toast element
    const toast = document.createElement('div');
    toast.className = `fixed top-20 right-4 z-50 px-6 py-4 rounded-lg shadow-lg text-white font-medium max-w-md ${
      type === 'success' ? 'bg-green-600' :
      type === 'error' ? 'bg-red-600' :
      type === 'warning' ? 'bg-yellow-600' :
      'bg-blue-600'
    }`;
    toast.textContent = message;
    document.body.appendChild(toast);

    // Auto-remove after duration
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
          <h1 className="text-3xl font-bold text-gray-900">EasyCars Stock Sync</h1>
          <p className="text-gray-600 mt-2">
            Monitor and manage your vehicle inventory synchronization with EasyCars
          </p>
        </div>

        <div className="space-y-6">
          {/* Dashboard Section */}
          <StockSyncDashboard
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
        </div>

        {/* Details Modal */}
        {selectedLogId && (
          <SyncDetailsModal
            syncLogId={selectedLogId}
            dealershipId={selectedDealership?.id}
            onClose={handleCloseModal}
          />
        )}
      </div>
    </>
  );
}
