/**
 * @fileoverview LeadSyncDashboard - Dashboard component showing last lead sync status (Story 3.5)
 * Displays lead sync summary, direction indicator, sync now button, and credential warnings
 */

import PropTypes from 'prop-types';
import { formatDateTime, formatDuration } from '../utils/syncFormatters';

/**
 * LeadSyncDashboard - Displays last lead sync summary and sync now button
 */
export default function LeadSyncDashboard({ 
  syncStatus, 
  hasCredentials, 
  isSyncing, 
  onSyncNow 
}) {

  const getStatusBadge = (status) => {
    if (isSyncing) {
      return (
        <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-800">
          <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-blue-800" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          In Progress
        </span>
      );
    }

    switch (status) {
      case 'Success':
        return (
          <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-800">
            <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
            </svg>
            Success
          </span>
        );
      case 'Failed':
        return (
          <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-red-100 text-red-800">
            <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
            </svg>
            Failed
          </span>
        );
      case 'PartialSuccess':
        return (
          <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-yellow-100 text-yellow-800">
            <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
              <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
            </svg>
            Partial Success
          </span>
        );
      default:
        return <span className="text-gray-500">No sync yet</span>;
    }
  };

  return (
    <div className="bg-white rounded-lg shadow p-6 mb-6">
      <div className="flex items-center mb-4">
        <h2 className="text-xl font-bold">Last Sync Status</h2>
        <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800 ml-2">
          ↓ Inbound — EasyCars → Local
        </span>
      </div>
      
      {!hasCredentials && (
        <div className="bg-yellow-50 border-l-4 border-yellow-400 p-4 mb-4">
          <div className="flex">
            <div className="flex-shrink-0">
              <svg className="h-5 w-5 text-yellow-400" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
              </svg>
            </div>
            <div className="ml-3">
              <h3 className="text-sm font-medium text-yellow-800">
                EasyCars credentials not configured
              </h3>
              <div className="mt-2 text-sm text-yellow-700">
                <p>Lead synchronization requires valid EasyCars credentials.</p>
                <a 
                  href="/admin/settings" 
                  className="font-medium underline hover:text-yellow-600 mt-1 inline-block"
                >
                  Configure Credentials →
                </a>
              </div>
            </div>
          </div>
        </div>
      )}

      {syncStatus ? (
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <div>{getStatusBadge(syncStatus.status)}</div>
            <div className="text-sm text-gray-600">
              {formatDateTime(syncStatus.lastSyncedAt)}
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
            <div className="bg-gray-50 p-3 rounded">
              <p className="text-sm text-gray-600">Leads Processed</p>
              <p className="text-2xl font-bold text-gray-900">{syncStatus.itemsProcessed}</p>
            </div>
            <div className="bg-gray-50 p-3 rounded">
              <p className="text-sm text-gray-600">Successfully Synced</p>
              <p className="text-2xl font-bold text-green-600">{syncStatus.itemsSucceeded}</p>
            </div>
            {syncStatus.itemsFailed > 0 && (
              <div className="bg-gray-50 p-3 rounded">
                <p className="text-sm text-gray-600">Failed</p>
                <p className="text-2xl font-bold text-red-600">{syncStatus.itemsFailed}</p>
              </div>
            )}
            <div className="bg-gray-50 p-3 rounded">
              <p className="text-sm text-gray-600">Duration</p>
              <p className="text-lg font-semibold text-gray-900">
                {formatDuration(syncStatus.durationMs)}
              </p>
            </div>
          </div>
        </div>
      ) : (
        <p className="text-gray-500 text-center py-4">
          No synchronization has run yet
        </p>
      )}

      <div className="mt-6">
        <button
          onClick={onSyncNow}
          disabled={!hasCredentials || isSyncing}
          className={`w-full md:w-auto px-6 py-3 rounded-lg font-medium transition flex items-center justify-center gap-2 ${
            !hasCredentials || isSyncing
              ? 'bg-gray-400 text-gray-700 cursor-not-allowed'
              : 'bg-blue-600 text-white hover:bg-blue-700'
          }`}
          title={!hasCredentials ? 'Configure credentials to enable sync' : 'Trigger immediate lead synchronization'}
        >
          {isSyncing ? (
            <>
              <svg className="animate-spin h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Syncing...
            </>
          ) : (
            <>
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
              </svg>
              Sync Now
            </>
          )}
        </button>
      </div>
    </div>
  );
}

LeadSyncDashboard.propTypes = {
  syncStatus: PropTypes.shape({
    lastSyncedAt: PropTypes.string,
    status: PropTypes.oneOf(['Success', 'Failed', 'PartialSuccess']),
    itemsProcessed: PropTypes.number,
    itemsSucceeded: PropTypes.number,
    itemsFailed: PropTypes.number,
    durationMs: PropTypes.number
  }),
  hasCredentials: PropTypes.bool.isRequired,
  isSyncing: PropTypes.bool.isRequired,
  onSyncNow: PropTypes.func.isRequired
};
