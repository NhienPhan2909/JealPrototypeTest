/**
 * @fileoverview AdminHeader - Navigation header for admin panel.
 * Displays dealership selector dropdown, navigation menu, view website link, and logout button.
 * Manages dealership selection and updates AdminContext.
 */

import { useContext, useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { AdminContext } from '../context/AdminContext';
import { useDealershipContext } from '../context/DealershipContext';
import { hasPermission, canManageUsers, isAdmin } from '../utils/permissions';
import apiRequest from '../utils/api';

/**
 * AdminHeader - Admin panel header component.
 * Displays dealership selector, navigation menu, view website link, and logout button.
 * Fetches dealerships from API on mount and auto-selects first dealership.
 *
 * @component
 *
 * @example
 * <AdminHeader />
 */
export default function AdminHeader() {
  const { setIsAuthenticated, user, selectedDealership, setSelectedDealership } = useContext(AdminContext);
  const { setCurrentDealershipId } = useDealershipContext();
  const navigate = useNavigate();
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const [dealerships, setDealerships] = useState([]);

  /**
   * Fetch dealerships from API on component mount.
   * Auto-select first dealership if none is selected.
   */
  useEffect(() => {
    const fetchDealerships = async () => {
      try {
        // Admin users need to fetch all dealerships
        if (user?.userType === 'admin') {
          const response = await apiRequest('/api/dealers', {
            // Include session cookie
          });

          if (response.ok) {
            const result = await response.json();
            const data = result.data || result.Data || result;
            setDealerships(data);
          } else {
            console.error('Failed to fetch dealerships');
          }
        }
        // Owner/staff already have selectedDealership set by AdminContext
      } catch (error) {
        console.error('Error fetching dealerships:', error);
      }
    };

    fetchDealerships();
  }, [user]);

  /**
   * Auto-select first dealership if none is selected, or validate persisted dealership.
   * Triggers when dealerships array is populated.
   */
  useEffect(() => {
    if (dealerships.length > 0 && user?.userType === 'admin') {
      if (selectedDealership) {
        // Validate that the persisted dealership still exists
        const dealershipExists = dealerships.some(d => d.id === selectedDealership.id);
        if (!dealershipExists) {
          // Persisted dealership no longer exists, select first one
          setSelectedDealership(dealerships[0]);
        } else {
          // Update selectedDealership with fresh data from server
          const freshDealership = dealerships.find(d => d.id === selectedDealership.id);
          setSelectedDealership(freshDealership);
        }
      } else {
        // No dealership selected, auto-select first one
        setSelectedDealership(dealerships[0]);
      }
    }
  }, [dealerships, setSelectedDealership, user, selectedDealership]);

  /**
   * Handle dealership selection change from dropdown.
   * Finds selected dealership object and updates context.
   *
   * @param {Event} e - Change event from select element
   */
  const handleDealershipChange = (e) => {
    const dealershipId = parseInt(e.target.value, 10);
    const dealership = dealerships.find(d => d.id === dealershipId);
    if (dealership) {
      setSelectedDealership(dealership);
    }
  };

  /**
   * Handle logout - POST to /api/auth/logout and redirect to login.
   */
  const handleLogout = async () => {
    setIsLoggingOut(true);

    try {
      const response = await apiRequest('/api/auth/logout', {
        method: 'POST',
        // Include session cookie
      });

      if (response.ok) {
        // Clear JWT token and authentication state, then redirect to login
        localStorage.removeItem('jwt_token');
        localStorage.removeItem('selectedDealership');
        setIsAuthenticated(false);
        navigate('/admin/login');
      } else {
        console.error('Logout failed');
        alert('Failed to log out. Please try again.');
      }
    } catch (error) {
      console.error('Logout error:', error);
      alert('Failed to connect to server. Please try again.');
    } finally {
      setIsLoggingOut(false);
    }
  };

  /**
   * Handle view website - Sets public site dealership context and navigates to home page.
   * Updates DealershipContext so public site displays the correct dealership.
   */
  const handleViewWebsite = () => {
    if (selectedDealership) {
      // Update public site dealership context to match admin selection
      setCurrentDealershipId(selectedDealership.id);
      // Navigate to public home page
      navigate('/');
    }
  };

  // Check permissions for nav links
  const canManageUsersLink = canManageUsers(user);
  const isAdminUser = isAdmin(user);
  
  // All authenticated users can VIEW all sections
  // Staff will see read-only banners if they don't have edit permission
  const isAuthenticated = user !== null;

  return (
    <header className="bg-gray-800 text-white">
      <div className="container mx-auto px-4 py-4">
        {/* Top Row: Title and User Info */}
        <div className="flex items-center justify-between mb-4">
          <h1 className="text-2xl font-bold whitespace-nowrap">
            Admin Panel
            {user && (
              <span className="text-sm font-normal text-gray-300 ml-2">
                ({user.fullName})
              </span>
            )}
          </h1>

          {/* Dealership Selector - Only show for admin users */}
          {user?.userType === 'admin' && (
            <div className="flex-shrink-0">
              <label htmlFor="dealership-select" className="block text-sm mb-1">
                Select Dealership:
              </label>
              <select
                id="dealership-select"
                value={selectedDealership?.id || ''}
                onChange={handleDealershipChange}
                className="input-field bg-gray-700 text-white border-gray-600 min-w-[200px]"
              >
                {dealerships.map(dealership => (
                  <option key={dealership.id} value={dealership.id}>
                    {dealership.name}
                  </option>
                ))}
              </select>
              {selectedDealership && (
                <p className="text-sm mt-1 text-gray-300">
                  Managing: <span className="font-semibold">{selectedDealership.name}</span>
                </p>
              )}
            </div>
          )}

          {/* Dealership Display - For owner/staff (non-selectable) */}
          {user?.userType !== 'admin' && selectedDealership && (
            <div className="flex-shrink-0">
              <p className="text-sm text-gray-300">
                Dealership: <span className="font-semibold text-lg">{selectedDealership.name}</span>
              </p>
            </div>
          )}
        </div>

        {/* Bottom Row: Navigation Menu - All authenticated users can view all sections */}
        <nav className="flex flex-wrap items-center gap-3 md:gap-4 justify-between">
          <div className="flex flex-wrap items-center gap-3 md:gap-4">
            <Link
              to="/admin"
              className="text-white hover:text-blue-300 transition font-semibold flex items-center gap-1 whitespace-nowrap"
              title="Back to Dashboard"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
              </svg>
              Dashboard
            </Link>
            
            <Link
              to="/admin/vehicles"
              className="text-white hover:text-blue-300 transition whitespace-nowrap"
            >
              Vehicle Manager
            </Link>
            
            <Link
              to="/admin/blogs"
              className="text-white hover:text-blue-300 transition whitespace-nowrap"
            >
              Blog Manager
            </Link>
            
            <Link
              to="/admin/settings"
              className="text-white hover:text-blue-300 transition whitespace-nowrap"
            >
              Dealership Settings
            </Link>
            
            <Link
              to="/admin/leads"
              className="text-white hover:text-blue-300 transition whitespace-nowrap"
            >
              Lead Inbox
            </Link>
            
            <Link
              to="/admin/sales-requests"
              className="text-white hover:text-blue-300 transition whitespace-nowrap"
            >
              Sales Requests
            </Link>
            
            <Link
              to="/admin/easycars/stock-sync"
              className="text-white hover:text-blue-300 transition whitespace-nowrap flex items-center gap-1"
              title="EasyCars Stock Sync"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
              </svg>
              Stock Sync
            </Link>

            <Link
              to="/admin/easycars/lead-sync"
              className="text-white hover:text-blue-300 transition whitespace-nowrap flex items-center gap-1"
              title="EasyCars Lead Sync"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
              </svg>
              Lead Sync
            </Link>
            
            {canManageUsersLink && (
              <Link
                to="/admin/users"
                className="text-white hover:text-blue-300 transition whitespace-nowrap"
              >
                User Management
              </Link>
            )}
            
            {isAdminUser && (
              <Link
                to="/admin/dealerships"
                className="text-white hover:text-blue-300 transition whitespace-nowrap"
              >
                Dealership Management
              </Link>
            )}
          </div>

          <div className="flex items-center gap-3">
            <button
              onClick={handleViewWebsite}
              className="text-blue-400 hover:text-blue-300 transition font-medium flex items-center gap-1 whitespace-nowrap"
              disabled={!selectedDealership}
              title="View public website for this dealership"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 6H6a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-4M14 4h6m0 0v6m0-6L10 14" />
              </svg>
              View Website
            </button>
            <span className="text-gray-500">|</span>
            <button
              onClick={handleLogout}
              className="text-red-400 hover:text-red-300 transition font-medium whitespace-nowrap"
              disabled={isLoggingOut}
            >
              {isLoggingOut ? 'Logging out...' : 'Log Out'}
            </button>
          </div>
        </nav>
      </div>
    </header>
  );
}
