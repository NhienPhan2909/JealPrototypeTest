/**
 * @fileoverview SyncDetailsModal - Modal component showing detailed sync log information
 * Displays full error messages and sync metadata
 */

import { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import apiRequest from '../../../../utils/api';
import { formatDateTime, formatDuration } from '../utils/syncFormatters';

/**
 * SyncDetailsModal - Modal displaying detailed sync log information
 */
export default function SyncDetailsModal({ syncLogId, dealershipId, onClose }) {
  const [details, setDetails] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchDetails = async () => {
      setLoading(true);
      setError(null);

      try {
        const response = await apiRequest(`/api/easycars/sync-logs/${syncLogId}?dealershipId=${dealershipId}`);
        
        if (response.ok) {
          const data = await response.json();
          setDetails(data);
        } else {
          const errorData = await response.json();
          setError(errorData.error || 'Failed to fetch sync details');
        }
      } catch (err) {
        console.error('Error fetching sync details:', err);
        setError('Failed to fetch sync details');
      } finally {
        setLoading(false);
      }
    };

    if (syncLogId) {
      fetchDetails();
    }
  }, [syncLogId]);

  const getStatusBadge = (status) => {
    switch (status) {
      case 'Success':
        return <span className="text-green-600 font-semibold">✅ Success</span>;
      case 'Failed':
        return <span className="text-red-600 font-semibold">❌ Failed</span>;
      case 'PartialSuccess':
        return <span className="text-yellow-600 font-semibold">⚠️ Partial Success</span>;
      default:
        return <span className="text-gray-500">Unknown</span>;
    }
  };

  const handleCopy = () => {
    if (!details) return;
    
    const text = `Sync Details - ${formatDateTime(details.syncedAt, true)}
Status: ${details.status}
Duration: ${formatDuration(details.durationMs)}
Vehicles Processed: ${details.itemsProcessed}
Successfully Synced: ${details.itemsSucceeded}
Failed: ${details.itemsFailed}

${details.errors.length > 0 ? 'Errors:\n' + details.errors.map((e, i) => `${i + 1}. ${e}`).join('\n') : 'No errors'}`;
    
    navigator.clipboard.writeText(text).then(() => {
      alert('Details copied to clipboard!');
    }).catch(err => {
      console.error('Failed to copy:', err);
      alert('Failed to copy to clipboard');
    });
  };

  // Handle ESC key press
  useEffect(() => {
    const handleEsc = (e) => {
      if (e.key === 'Escape') {
        onClose();
      }
    };
    
    window.addEventListener('keydown', handleEsc);
    return () => window.removeEventListener('keydown', handleEsc);
  }, [onClose]);

  return (
    <div 
      className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4"
      onClick={onClose}
    >
      <div 
        className="bg-white rounded-lg shadow-xl max-w-3xl w-full max-h-[90vh] overflow-hidden flex flex-col"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Modal Header */}
        <div className="border-b px-6 py-4 flex items-center justify-between">
          <h2 className="text-xl font-bold">
            {details ? `Sync Details - ${formatDateTime(details.syncedAt, true)}` : 'Sync Details'}
          </h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition"
            aria-label="Close modal"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Modal Body */}
        <div className="flex-1 overflow-y-auto px-6 py-4">
          {loading && (
            <div className="flex items-center justify-center py-12">
              <svg className="animate-spin h-8 w-8 text-blue-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
            </div>
          )}

          {error && (
            <div className="bg-red-50 border border-red-200 rounded p-4 text-red-800">
              <p className="font-medium">Error loading details</p>
              <p className="text-sm mt-1">{error}</p>
            </div>
          )}

          {details && (
            <div className="space-y-6">
              {/* Status Section */}
              <div>
                <h3 className="text-lg font-semibold mb-2">Status</h3>
                <div className="bg-gray-50 rounded p-4">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <p className="text-sm text-gray-600">Status</p>
                      <p className="text-lg mt-1">{getStatusBadge(details.status)}</p>
                    </div>
                    <div>
                      <p className="text-sm text-gray-600">Duration</p>
                      <p className="text-lg font-medium mt-1">{formatDuration(details.durationMs)}</p>
                    </div>
                  </div>
                </div>
              </div>

              {/* Summary Section */}
              <div>
                <h3 className="text-lg font-semibold mb-2">Summary</h3>
                <div className="bg-gray-50 rounded p-4">
                  <ul className="space-y-2">
                    <li className="flex justify-between">
                      <span className="text-gray-600">Vehicles Processed:</span>
                      <span className="font-medium">{details.itemsProcessed}</span>
                    </li>
                    <li className="flex justify-between">
                      <span className="text-gray-600">Successfully Synced:</span>
                      <span className="font-medium text-green-600">{details.itemsSucceeded}</span>
                    </li>
                    <li className="flex justify-between">
                      <span className="text-gray-600">Failed:</span>
                      <span className={`font-medium ${details.itemsFailed > 0 ? 'text-red-600' : 'text-gray-500'}`}>
                        {details.itemsFailed}
                      </span>
                    </li>
                  </ul>
                </div>
              </div>

              {/* Error Section */}
              {details.errors && details.errors.length > 0 && (
                <div>
                  <h3 className="text-lg font-semibold mb-2">Errors</h3>
                  <div className="bg-red-50 rounded p-4 max-h-64 overflow-y-auto">
                    <ul className="space-y-2 list-disc list-inside">
                      {details.errors.map((error, index) => (
                        <li key={index} className="text-sm text-red-800">
                          {error}
                        </li>
                      ))}
                    </ul>
                  </div>
                </div>
              )}

              {details.errors && details.errors.length === 0 && (
                <div>
                  <h3 className="text-lg font-semibold mb-2">Errors</h3>
                  <div className="bg-gray-50 rounded p-4">
                    <p className="text-gray-500 text-sm">No errors occurred during this sync</p>
                  </div>
                </div>
              )}
            </div>
          )}
        </div>

        {/* Modal Footer */}
        <div className="border-t px-6 py-4 flex justify-end gap-3">
          <button
            onClick={handleCopy}
            disabled={!details}
            className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50 transition disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Copy to Clipboard
          </button>
          <button
            onClick={onClose}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  );
}

SyncDetailsModal.propTypes = {
  syncLogId: PropTypes.number.isRequired,
  dealershipId: PropTypes.number,
  onClose: PropTypes.func.isRequired
};
