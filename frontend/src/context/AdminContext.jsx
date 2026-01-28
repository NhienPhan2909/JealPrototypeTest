/**
 * @fileoverview AdminContext - Global authentication and user state management.
 * Provides authentication status, user data, and selected dealership to all components via Context API.
 *
 * ESLint Fast Refresh Exception: This file exports both context and provider component,
 * which is the standard React context pattern. Fast Refresh works correctly despite the warning.
 */

/* eslint-disable react-refresh/only-export-components */
import React, { createContext, useState, useEffect } from 'react';
import apiRequest from '../utils/api';

/**
 * AdminContext - React context for authentication and user management.
 *
 * @type {React.Context}
 */
export const AdminContext = createContext();

/**
 * AdminProvider - Context provider component for user state management.
 * Manages authentication state, user data, and selected dealership across the application.
 * Checks authentication status on mount by calling /api/auth/me.
 *
 * @component
 * @param {Object} props
 * @param {React.ReactNode} props.children - Child components to wrap with context
 *
 * @example
 * <AdminProvider>
 *   <App />
 * </AdminProvider>
 */
export function AdminProvider({ children }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [user, setUser] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  
  // Initialize selectedDealership from localStorage if available
  const [selectedDealership, setSelectedDealership] = useState(() => {
    try {
      const saved = localStorage.getItem('selectedDealership');
      return saved ? JSON.parse(saved) : null;
    } catch (error) {
      console.error('Failed to load selectedDealership from localStorage:', error);
      return null;
    }
  });

  /**
   * Persist selectedDealership to localStorage whenever it changes
   */
  useEffect(() => {
    if (selectedDealership) {
      localStorage.setItem('selectedDealership', JSON.stringify(selectedDealership));
    } else {
      localStorage.removeItem('selectedDealership');
    }
  }, [selectedDealership]);

  /**
   * Set CSS custom properties for theme color when selectedDealership changes.
   * This enables dynamic theming in the admin panel.
   */
  useEffect(() => {
    if (selectedDealership?.themeColor) {
      document.documentElement.style.setProperty('--theme-color', selectedDealership.themeColor);

      // Calculate lighter and darker shades for hover states
      const hex = selectedDealership.themeColor.replace('#', '');
      const r = parseInt(hex.substr(0, 2), 16);
      const g = parseInt(hex.substr(2, 2), 16);
      const b = parseInt(hex.substr(4, 2), 16);

      // Darker shade for hover (reduce brightness by 15%)
      const darkerR = Math.max(0, Math.floor(r * 0.85));
      const darkerG = Math.max(0, Math.floor(g * 0.85));
      const darkerB = Math.max(0, Math.floor(b * 0.85));
      const darkerColor = `#${darkerR.toString(16).padStart(2, '0')}${darkerG.toString(16).padStart(2, '0')}${darkerB.toString(16).padStart(2, '0')}`;

      document.documentElement.style.setProperty('--theme-color-dark', darkerColor);

      // Lighter shade for backgrounds (increase brightness by 90%)
      const lighterR = Math.min(255, Math.floor(r + (255 - r) * 0.9));
      const lighterG = Math.min(255, Math.floor(g + (255 - g) * 0.9));
      const lighterB = Math.min(255, Math.floor(b + (255 - b) * 0.9));
      const lighterColor = `#${lighterR.toString(16).padStart(2, '0')}${lighterG.toString(16).padStart(2, '0')}${lighterB.toString(16).padStart(2, '0')}`;

      document.documentElement.style.setProperty('--theme-color-light', lighterColor);
    } else {
      // Reset to default blue if no theme color
      document.documentElement.style.setProperty('--theme-color', '#3B82F6');
      document.documentElement.style.setProperty('--theme-color-dark', '#2563EB');
      document.documentElement.style.setProperty('--theme-color-light', '#EFF6FF');
    }

    // Set secondary theme color and its variations
    if (selectedDealership?.secondaryThemeColor) {
      document.documentElement.style.setProperty('--secondary-theme-color', selectedDealership.secondaryThemeColor);

      // Calculate lighter and darker shades for secondary color
      const hex = selectedDealership.secondaryThemeColor.replace('#', '');
      const r = parseInt(hex.substr(0, 2), 16);
      const g = parseInt(hex.substr(2, 2), 16);
      const b = parseInt(hex.substr(4, 2), 16);

      // Darker shade for hover (reduce brightness by 15%)
      const darkerR = Math.max(0, Math.floor(r * 0.85));
      const darkerG = Math.max(0, Math.floor(g * 0.85));
      const darkerB = Math.max(0, Math.floor(b * 0.85));
      const darkerColor = `#${darkerR.toString(16).padStart(2, '0')}${darkerG.toString(16).padStart(2, '0')}${darkerB.toString(16).padStart(2, '0')}`;

      document.documentElement.style.setProperty('--secondary-theme-color-dark', darkerColor);

      // Lighter shade for backgrounds (increase brightness by 90%)
      const lighterR = Math.min(255, Math.floor(r + (255 - r) * 0.9));
      const lighterG = Math.min(255, Math.floor(g + (255 - g) * 0.9));
      const lighterB = Math.min(255, Math.floor(b + (255 - b) * 0.9));
      const lighterColor = `#${lighterR.toString(16).padStart(2, '0')}${lighterG.toString(16).padStart(2, '0')}${lighterB.toString(16).padStart(2, '0')}`;

      document.documentElement.style.setProperty('--secondary-theme-color-light', lighterColor);
    } else {
      // Set default secondary color if not specified
      document.documentElement.style.setProperty('--secondary-theme-color', '#FFFFFF');
      document.documentElement.style.setProperty('--secondary-theme-color-dark', '#E5E5E5');
      document.documentElement.style.setProperty('--secondary-theme-color-light', '#FFFFFF');
    }
  }, [selectedDealership?.themeColor, selectedDealership?.secondaryThemeColor]);

  /**
   * Check authentication status on component mount.
   * Calls /api/auth/me to verify if user has valid session.
   */
  useEffect(() => {
    const checkAuthStatus = async () => {
      try {
        const response = await apiRequest('/api/auth/me');
        
        // Handle 401 Unauthorized (not logged in) - this is expected for public users
        if (response.status === 401) {
          setIsAuthenticated(false);
          setUser(null);
          return;
        }
        
        const data = await response.json();
        setIsAuthenticated(data.authenticated);
        setUser(data.user || null);
        
        // For non-admin users, auto-select their dealership
        if (data.authenticated && data.user && data.user.userType !== 'admin' && data.user.dealershipId) {
          // Fetch dealership info if not already selected
          if (!selectedDealership || selectedDealership.id !== data.user.dealershipId) {
            try {
              const dealershipRes = await apiRequest(`/api/dealers/${data.user.dealershipId}`);
              if (dealershipRes.ok) {
                const dealershipData = await dealershipRes.json();
                setSelectedDealership(dealershipData);
              }
            } catch (err) {
              console.error('Failed to fetch dealership:', err);
            }
          }
        }
      } catch (error) {
        console.error('Failed to check auth status:', error);
        setIsAuthenticated(false);
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    };

    checkAuthStatus();
  }, []);

  const value = {
    isAuthenticated,
    setIsAuthenticated,
    user,
    setUser,
    selectedDealership,
    setSelectedDealership,
    isLoading
  };

  return (
    <AdminContext.Provider value={value}>
      {children}
    </AdminContext.Provider>
  );
}
