/**
 * @fileoverview Custom hook for fetching dealership data.
 * Provides centralized dealership data management to avoid duplicate API calls.
 */

import { useState, useEffect } from 'react';
import apiRequest from '../utils/api';

/**
 * useDealership - Custom hook to fetch and cache dealership data.
 *
 * @param {number} dealershipId - The dealership ID to fetch
 * @returns {Object} Object containing dealership data, loading state, and error
 * @returns {Object|null} returns.dealership - Dealership object from API
 * @returns {boolean} returns.loading - Loading state
 * @returns {string|null} returns.error - Error message if fetch fails
 *
 * @example
 * const { dealership, loading, error } = useDealership(1);
 */
function useDealership(dealershipId) {
  const [dealership, setDealership] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    /**
     * Fetches dealership data from the API.
     *
     * @returns {Promise<void>}
     */
    async function fetchDealership() {
      try {
        const response = await apiRequest(`/api/dealers/${dealershipId}`);

        if (!response.ok) {
          throw new Error('Failed to fetch dealership data');
        }

        const result = await response.json();
        setDealership(result.data || result.Data || result);
        setLoading(false);
      } catch (err) {
        console.error('Failed to load dealership:', err);
        setError(err.message);
        setLoading(false);
      }
    }

    fetchDealership();
  }, [dealershipId]);

  return { dealership, loading, error };
}

export default useDealership;
