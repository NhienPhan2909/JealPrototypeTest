/**
 * @fileoverview SyncHistoryTable - Table component showing sync history
 * Displays last sync operations with status, timestamps, and details
 */

import PropTypes from 'prop-types';
import { formatDateTime, formatDuration } from '../utils/syncFormatters';

/**
 * SyncHistoryTable - Displays sync history in table format (desktop) and card format (mobile)
 */
export default function SyncHistoryTable({ syncHistory, onViewDetails }) {

  const getStatusBadge = (status) => {
    switch (status) {
      case 'Success':
        return (
          <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
            ✅ Success
          </span>
        );
      case 'Failed':
        return (
          <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800">
            ❌ Failed
          </span>
        );
      case 'PartialSuccess':
        return (
          <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
            ⚠️ Partial
          </span>
        );
      default:
        return <span className="text-gray-500">Unknown</span>;
    }
  };

  if (!syncHistory || syncHistory.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow p-6">
        <h2 className="text-xl font-bold mb-4">Sync History</h2>
        <p className="text-gray-500 text-center py-8">
          No sync history available
        </p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h2 className="text-xl font-bold mb-4">Sync History</h2>
      
      {/* Desktop Table View */}
      <div className="hidden md:block overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Timestamp
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Processed
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Succeeded
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Failed
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Duration
              </th>
              <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {syncHistory.map((log) => (
              <tr key={log.id} className="hover:bg-gray-50">
                <td className="px-4 py-3 whitespace-nowrap">
                  {getStatusBadge(log.status)}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-900">
                  {formatDateTime(log.syncedAt)}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-900">
                  {log.itemsProcessed}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-sm text-green-600 font-medium">
                  {log.itemsSucceeded}
                </td>
                <td className={`px-4 py-3 whitespace-nowrap text-sm font-medium ${
                  log.itemsFailed > 0 ? 'text-red-600' : 'text-gray-500'
                }`}>
                  {log.itemsFailed}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-sm text-gray-500">
                  {formatDuration(log.durationMs, true)}
                </td>
                <td className="px-4 py-3 whitespace-nowrap text-sm">
                  <button
                    onClick={() => onViewDetails(log.id)}
                    className="text-blue-600 hover:text-blue-900 font-medium"
                  >
                    View Details
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Mobile Card View */}
      <div className="md:hidden space-y-4">
        {syncHistory.map((log) => (
          <div key={log.id} className="border rounded-lg p-4 bg-gray-50">
            <div className="flex justify-between items-center mb-3">
              {getStatusBadge(log.status)}
              <span className="text-xs text-gray-500">
                {formatDateTime(log.syncedAt)}
              </span>
            </div>
            
            <div className="space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-gray-600">Processed:</span>
                <span className="font-medium">{log.itemsProcessed}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Succeeded:</span>
                <span className="font-medium text-green-600">{log.itemsSucceeded}</span>
              </div>
              {log.itemsFailed > 0 && (
                <div className="flex justify-between">
                  <span className="text-gray-600">Failed:</span>
                  <span className="font-medium text-red-600">{log.itemsFailed}</span>
                </div>
              )}
              <div className="flex justify-between">
                <span className="text-gray-600">Duration:</span>
                <span className="font-medium">{formatDuration(log.durationMs, true)}</span>
              </div>
            </div>

            <button
              onClick={() => onViewDetails(log.id)}
              className="mt-3 w-full text-center text-blue-600 hover:text-blue-900 font-medium text-sm py-2 border border-blue-600 rounded hover:bg-blue-50 transition"
            >
              View Details →
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}

SyncHistoryTable.propTypes = {
  syncHistory: PropTypes.arrayOf(
    PropTypes.shape({
      id: PropTypes.number.isRequired,
      syncedAt: PropTypes.string.isRequired,
      status: PropTypes.oneOf(['Success', 'Failed', 'PartialSuccess']).isRequired,
      itemsProcessed: PropTypes.number.isRequired,
      itemsSucceeded: PropTypes.number.isRequired,
      itemsFailed: PropTypes.number.isRequired,
      durationMs: PropTypes.number.isRequired
    })
  ).isRequired,
  onViewDetails: PropTypes.func.isRequired
};
