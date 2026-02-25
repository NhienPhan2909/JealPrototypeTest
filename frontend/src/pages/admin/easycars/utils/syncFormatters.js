/**
 * @fileoverview Shared formatting utilities for stock sync components (Story 2.5)
 * Centralises date/duration formatting used by StockSyncDashboard, SyncHistoryTable, SyncDetailsModal
 */

/**
 * Formats a UTC date string into a localised display string.
 * Returns 'N/A' for null/undefined/invalid values.
 * @param {string|null|undefined} dateString - ISO date string
 * @param {boolean} [includeSeconds=false] - Whether to include seconds in output
 * @returns {string}
 */
export function formatDateTime(dateString, includeSeconds = false) {
  if (!dateString) return 'N/A';
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return 'N/A';

  const options = {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  };

  if (includeSeconds) {
    options.second = '2-digit';
    options.month = 'long';
  }

  return date.toLocaleString('en-US', options);
}

/**
 * Formats a duration in milliseconds into a human-readable string.
 * Returns 'N/A' for null/undefined/zero values.
 * @param {number|null|undefined} durationMs
 * @param {boolean} [compact=false] - If true, uses abbreviated form ("2m 5s"); else verbose ("2 minutes 5 seconds")
 * @returns {string}
 */
export function formatDuration(durationMs, compact = false) {
  if (!durationMs && durationMs !== 0) return 'N/A';
  if (durationMs === 0) return compact ? '0s' : '0 seconds';

  const seconds = Math.floor(durationMs / 1000);
  const minutes = Math.floor(seconds / 60);
  const remainingSeconds = seconds % 60;

  if (compact) {
    return minutes > 0
      ? `${minutes}m ${remainingSeconds}s`
      : `${seconds}s`;
  }

  if (minutes > 0) {
    return `${minutes} minute${minutes > 1 ? 's' : ''} ${remainingSeconds} second${remainingSeconds !== 1 ? 's' : ''}`;
  }
  return `${seconds} second${seconds !== 1 ? 's' : ''}`;
}
