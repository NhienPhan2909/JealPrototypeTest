/**
 * @fileoverview Context provider for managing current dealership selection.
 * Provides global state for which dealership website is being viewed.
 */

import { createContext, useContext, useState, useEffect } from 'react';

const DealershipContext = createContext();

export function DealershipProvider({ children }) {
  const [currentDealershipId, setCurrentDealershipId] = useState(() => {
    const saved = localStorage.getItem('selectedDealershipId');
    return saved ? parseInt(saved, 10) : 1;
  });

  useEffect(() => {
    localStorage.setItem('selectedDealershipId', currentDealershipId.toString());
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
