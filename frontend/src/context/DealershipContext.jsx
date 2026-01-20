/**
 * @fileoverview Context provider for managing current dealership selection.
 * Provides global state for which dealership website is being viewed.
 * 
 * Dealership selection priority:
 * 1. URL parameter (?dealership=X) - Explicitly set by URL
 * 2. Default to dealership ID 1 (Acme Auto Sales) - Clean start
 * 
 * Note: localStorage is used to persist the selection AFTER a URL parameter
 * is used, but when visiting without a parameter, we default to ID 1.
 */

import { createContext, useContext, useState, useEffect } from 'react';

const DealershipContext = createContext();

export function DealershipProvider({ children }) {
  const [currentDealershipId, setCurrentDealershipId] = useState(() => {
    // Priority 1: Check URL parameter
    const urlParams = new URLSearchParams(window.location.search);
    const urlDealershipId = urlParams.get('dealership');
    if (urlDealershipId) {
      const parsed = parseInt(urlDealershipId, 10);
      if (!isNaN(parsed) && parsed > 0) {
        return parsed;
      }
    }
    
    // Priority 2: Default to dealership ID 1 (Acme Auto Sales)
    // This ensures a clean start always shows the first dealership
    return 1;
  });

  // Save to localStorage whenever dealership changes
  useEffect(() => {
    localStorage.setItem('selectedDealershipId', currentDealershipId.toString());
  }, [currentDealershipId]);

  // Watch for URL parameter changes
  useEffect(() => {
    const handleUrlChange = () => {
      const urlParams = new URLSearchParams(window.location.search);
      const urlDealershipId = urlParams.get('dealership');
      if (urlDealershipId) {
        const parsed = parseInt(urlDealershipId, 10);
        if (!isNaN(parsed) && parsed > 0 && parsed !== currentDealershipId) {
          setCurrentDealershipId(parsed);
        }
      }
    };

    // Listen for browser navigation events
    window.addEventListener('popstate', handleUrlChange);
    
    return () => {
      window.removeEventListener('popstate', handleUrlChange);
    };
  }, [currentDealershipId]);

  return (
    <DealershipContext.Provider value={{ currentDealershipId, setCurrentDealershipId }}>
      {children}
    </DealershipContext.Provider>
  );
}

export function useDealershipContext() {
  const context = useContext(DealershipContext);
  if (!context) {
    throw new Error('useDealershipContext must be used within DealershipProvider');
  }
  return context;
}
