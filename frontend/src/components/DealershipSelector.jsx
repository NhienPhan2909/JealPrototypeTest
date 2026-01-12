/**
 * @fileoverview Dealership selector dropdown component.
 * Allows users to switch between different dealership websites.
 */

import { useState, useEffect } from 'react';
import { useDealershipContext } from '../context/DealershipContext';

function DealershipSelector() {
  const { currentDealershipId, setCurrentDealershipId } = useDealershipContext();
  const [dealerships, setDealerships] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetchDealerships() {
      try {
        const response = await fetch('/api/dealers');
        if (!response.ok) {
          throw new Error('Failed to fetch dealerships');
        }
        const data = await response.json();
        setDealerships(data);
        setLoading(false);
      } catch (err) {
        console.error('Failed to load dealerships:', err);
        setLoading(false);
      }
    }

    fetchDealerships();
  }, []);

  if (loading || dealerships.length <= 1) {
    return null;
  }

  return (
    <div className="flex items-center gap-2">
      <label htmlFor="dealership-select" className="text-sm text-gray-600 hidden md:block">
        View:
      </label>
      <select
        id="dealership-select"
        value={currentDealershipId}
        onChange={(e) => setCurrentDealershipId(parseInt(e.target.value, 10))}
        className="text-sm border border-gray-300 rounded px-2 py-1 focus:outline-none focus:ring-2 focus:ring-blue-500"
      >
        {dealerships.map(dealer => (
          <option key={dealer.id} value={dealer.id}>
            {dealer.name}
          </option>
        ))}
      </select>
    </div>
  );
}

export default DealershipSelector;
